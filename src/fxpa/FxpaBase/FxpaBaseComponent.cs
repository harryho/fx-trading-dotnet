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


using System.Configuration;

namespace fxpa
{
    public abstract class FxpaBaseComponent :  IFxpaBaseCompoent
    {
        FxpaBase fxpaBase;
        protected FxpaBase FxpaBase
        {
            get { return fxpaBase; }
        }
        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        public virtual bool IsInitialized
        {
            get { return fxpaBase != null; }
        }
        private bool IsSingleThread = false;
        /// <summary>
        /// Component will try to use UserFriendlyNameAttribute to establish a name for the current component, or if not available use class type name.
        /// </summary>
        public FxpaBaseComponent(bool singleThreadMode)
        {
            name = "FxpaBaseComponent";
            IsSingleThread = singleThreadMode;
        }

        public bool SetInitialState(SettingsBase data)
        {
            return OnSetInitialState(data);
        }

        public bool Initialize(FxpaBase platform)
        {
            //SystemMonitor.CheckThrow(_platform == null);

            if (OnInitialize(platform) == false)
            {
                return false;
            }
            fxpaBase = platform;
            
            return true;
        }

        public bool UnInitialize()
        {
            if (IsInitialized == false)
            {
                return true;
            }

            bool result = OnUnInitialize();
            fxpaBase = null;

            return result;
        }

        protected virtual bool OnSetInitialState(SettingsBase data)
        {
            return true;
        }

        protected virtual bool OnInitialize(FxpaBase platform)
        {
            return true;
        }

        protected virtual bool OnUnInitialize()
        {
            return true;
        }

    }
}
