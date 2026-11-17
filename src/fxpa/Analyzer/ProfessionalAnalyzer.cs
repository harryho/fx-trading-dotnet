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


namespace fxpa
{
    [CustomName("Professional Analyzer")]
    public class ProfessionalAnalyzer : Analyzer
    {
        IAnalyzerSessionManager _sessionManager;

        /// <summary>
        /// 
        /// </summary>
        public ProfessionalAnalyzer(IAnalyzerSessionManager sessionManager, string name)
            : base(sessionManager, name)
        {
            _sessionManager = sessionManager;
            //_sessionManager.SourcesUpdateEvent += new GeneralHelper.GenericDelegate<IAnalyzerSessionManager>(_sessionManager_SourcesUpdateEvent);
        }

        protected override bool OnInitialize()
        {
            return true;
        }

        void _sessionManager_SessionsUpdateEvent(IAnalyzerSessionManager parameter1)
        {
        }

        void _sessionManager_SourcesUpdateEvent(IAnalyzerSessionManager parameter1)
        {
            //lock (this)
            //{
            //    if (_sessionManager.OrderExecutionSources.Length > 0
            //        && _sessionManager.DataProviderSources.Length > 0)
            //    {
            //        SessionInfo[] sessionInfos = new SessionInfo[] { };
            //        while (sessionInfos.Length == 0)
            //        {
            //            Thread.Sleep(1000);
            //            sessionInfos = _sessionManager.GetSourceSessions(_sessionManager.DataProviderSources[0], false);
            //        }

            //        _sessionManager.CreateSession(AnalyzerSessionTypeEnum.LiveTrading,
            //            _sessionManager.DataProviderSources[0], _sessionManager.OrderExecutionSources[0], sessionInfos[0]);
            //    }
            //}
        }

    }
}
