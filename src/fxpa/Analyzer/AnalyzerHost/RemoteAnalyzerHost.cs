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
using System.Diagnostics;


using System.Reflection;

namespace fxpa
{
    /// <summary>
    /// AnalyzerHost (link mode) - It can provide a link to a remote separate process space host.
    /// </summary>
    /// 
    [CustomName("Remote Analyzer")]
    public class RemoteAnalyzerHostProxy : FxpaBaseComponent, /*IAnalyzerHost,*/ IDisposable
    {
        Process _remoteHostProcess;

        //string _expertName;
        //public string AnalyzerName
        //{
        //    get { return _expertName; }
        //}

        public IntPtr ProcessMainWindowHandle
        {
            get { lock (this) { return _remoteHostProcess.MainWindowHandle; } }
        }


        /// <summary>
        /// 
        /// </summary>
        public RemoteAnalyzerHostProxy(Type expertType, Uri uri)
            : base(false)
        {// Run expert host as new process

            //_expertName = expertName;

            _remoteHostProcess = new Process();
            _remoteHostProcess.StartInfo.Arguments = "\"" + uri.ToString() + "\"" + " " + expertType.ToString() + " " + "expertName";
            _remoteHostProcess.StartInfo.FileName = Assembly.GetEntryAssembly().Location;
            _remoteHostProcess.Exited += new EventHandler(process_Exited);

            if (_remoteHostProcess.Start() == false)
            {
                _remoteHostProcess = null;
                //SystemMonitor.Error("Failed to start host process.");
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_remoteHostProcess != null && _remoteHostProcess.HasExited == false)
                {
                    _remoteHostProcess.Kill();
                }
            }
        }

        void process_Exited(object sender, EventArgs e)
        {// A process has exited.
            //if (HostUnInitializedEvent != null)
            //{
            //    HostUnInitializedEvent(this);
            //}

            lock (this)
            {
                _remoteHostProcess = null;
            }
        }

        protected override bool OnUnInitialize()
        {
            bool result = base.OnUnInitialize();

            lock (this)
            {
                _remoteHostProcess.CloseMainWindow();
                _remoteHostProcess.Close();
            }

            return result;
        }

    }
}
