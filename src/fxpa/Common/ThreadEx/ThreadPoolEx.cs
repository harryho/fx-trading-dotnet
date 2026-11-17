// -----
// MIT License
//
// Copyright (c) 2005-2026 Harry Ho
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace fxpa
{
    public class ThreadPoolEx
    {
        /// <summary>
        /// Running threads.
        /// </summary>
        public volatile int MaximumSimultaniouslyRunningThreadsAllowed = 50;
        
        /// <summary>
        /// Total threads (running, sleeping, suspended, etc.)
        /// </summary>
        public volatile int MaximumTotalThreadsAllowed = 500;

        class TargetInfo
        {
            internal TargetInfo(Delegate d, object[] args)
            {
                Target = d;
                Args = args;
            }

            internal readonly Delegate Target;
            internal readonly object[] Args;
        }

        List<Thread> _threads = new List<Thread>();

        List<TargetInfo> _infos = new List<TargetInfo>();

        public int QueuedItemsCount
        {
            get { lock (_infos) { return _infos.Count; } }
        }

        delegate void InvokeDelegate(TargetInfo info);

        /// <summary>
        /// 
        /// </summary>
        public ThreadPoolEx()
        {
        }

        public void Queue(Delegate d, params object[] args)
        {
            if (d == null)
            {
                return;
            }

            lock (_infos)
            {
                TargetInfo info = new TargetInfo(d, args);
                _infos.Add(info);
            }

            ProcessQueue();
        }

        /// <summary>
        /// LOCK _threads before calling.
        /// We need to count only those actively working.
        /// </summary>
        /// <returns></returns>
        int GetRunningThreadsCount()
        {
            int result = 0;
            foreach(Thread t in _threads)
            {
                if (t.ThreadState == System.Threading.ThreadState.Running)
                {
                    result++;
                }
            }

            return result;
        }

        void ProcessQueue()
        {
            lock (_infos)
            {
                if (_infos.Count == 0)
                {
                    return;
                }
            }

            lock (_threads)
            {
                // Keep these inside the lock section.
                if (GetRunningThreadsCount() >= MaximumSimultaniouslyRunningThreadsAllowed)
                {
                    //TracerHelper.Trace("Too many running threads, suspeding.");
                    Console.WriteLine("Too many running threads, suspeding.");
                    return;
                }
                //Console.WriteLine(" _info " + ((TargetInfo)_infos[0]).Target.Target + ((TargetInfo)_infos[0]).Target.Method );
                //_threads.Clear();
                //foreach (Thread thd in _threads)
                //{
                //    Console.WriteLine(thd.ToString());
                //}
                Thread thread = new Thread(new ThreadStart(Execute));
                _threads.Add(thread);

                if (Debugger.IsAttached)
                {// Since we have debugger attached, also show the caller.
                    thread.Name = "FireAndForget slow trace maybe from "; // +TracerHelper.GetFullCallingMethodName(3);
                }
                else
                {
                    thread.Name = "FireAndForget";
                }

                thread.Start();
            }
        }

        void Execute()
        {
            while (true)
            {
                TargetInfo targetInfo;
                lock (_infos)
                {
                    if (_infos.Count == 0)
                    {
                        break;
                    }

                    targetInfo = _infos[0];
                    _infos.RemoveAt(0);
                }

                targetInfo.Target.DynamicInvoke(targetInfo.Args);
            }

            lock (_threads)
            {
                _threads.Remove(Thread.CurrentThread);
            }
        }
    }
}
