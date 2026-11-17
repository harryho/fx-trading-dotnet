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

namespace fxpa
{
    public abstract class Analyzer
    {
        string name;
        public string Name
        {
            get { lock (this) { return name; } }
        }

        IAnalyzerSessionManager _sessionManager;
        public IAnalyzerSessionManager SessionManager
        {
            get { lock (this) { return _sessionManager; } }
        }


        /// <summary>
        /// 
        /// </summary>
        public Analyzer(IAnalyzerSessionManager sessionManager, string name)
        {
            _sessionManager = sessionManager;
            this.name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Initialize()
        {
            return OnInitialize();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UnInitialize()
        {
            return OnUnInitialize();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual bool OnInitialize()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual bool OnUnInitialize()
        {
            return true;
        }

    }
}
