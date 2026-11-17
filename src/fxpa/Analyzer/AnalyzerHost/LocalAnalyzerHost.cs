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

using System.Windows.Forms;

using System.Diagnostics;
using System.Reflection;

namespace fxpa
{
    /// <summary>
    /// Analyzer host has a tripple role:
    /// (local mode) - It can host an expert in the local platform execution process
    /// </summary>
    public class LocalAnalyzerHost : AnalyzerHost, IFxpaBaseCompoent
    {
        FxpaBase _platform;

        public bool IsInitialized
        {
            get { lock (this) { return _platform != null; } }
        }

        /// <summary>
        /// Local execution. Analyzer to be executed locally, within the platform process space.
        /// </summary>
        /// <param name="platformAddress"></param>
        public LocalAnalyzerHost(string name, Type expertType)
            : base(name, expertType)
        {
            // TracerHelper.Trace(this.Name);
        }

        public bool Initialize(FxpaBase platform)
        {
            lock (this)
            {
                _platform = platform;
                //HostInitialize(new List<ArbiterClientId?>(new ArbiterClientId? [] { _platform.SubscriptionClientID } ));
                HostInitialize();
            }

            return true;
        }

        public bool UnInitialize()
        {
            lock (this)
            {
                HostUnInitialize();
                _platform = null;
            }
            return true;
        }

        public bool SetInitialState(System.Configuration.SettingsBase data)
        {
            return true;
        }

        //public override AnalyzerSession CreateLiveTradingSession(SessionInfo sessionInfo)
        //{

        //    //DataProviderSource[] sources = _platform.GetComponents<DataProviderSource>();
        //    //List<ArbiterClientId?> stack = new List<ArbiterClientId?>();
        //    //stack.Add(sources[0].SubscriptionClientID);
        //    //DataProvider dataProvider = new DataProvider("DP1", stack);
        //    //dataProvider.Initialize();

        //    //OrderExecutionProvider executionProvider = new OrderExecutionProvider("EP1");

        //    //AnalyzerSession session = new AnalyzerSession(dataProvider, executionProvider);
        //    //return session;

        //}


    }
}
