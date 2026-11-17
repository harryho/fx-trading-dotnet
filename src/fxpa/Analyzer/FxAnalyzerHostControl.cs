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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;





namespace fxpa
{
    public partial class FxAnalyzerHostControl : FxpaCommonControl
    {
        AnalyzerHost _host;
        FxpaCommonControl _expertControl;

        /// <summary>
        /// 
        /// </summary>
        public FxAnalyzerHostControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FxAnalyzerHostControl(LocalAnalyzerHost expertHost)
        {
            InitializeComponent();

            Initialize(expertHost);
        }

        /// <summary>
        /// 
        /// </summary>
        //public ActualAnalyzerHostControl(RemoteAnalyzerHost expertHost)
        //{
        //    InitializeComponent();

        //    Initialize(expertHost);
        //}

        public FxAnalyzerHostControl(AnalyzerHost expertHost)
        {
            InitializeComponent();

            Initialize(expertHost);
        }

        void Initialize(AnalyzerHost expertHost)
        {
            _host = expertHost;
            this.Title = expertHost.Name;
            //_host.SessionsUpdateEvent += new GeneralHelper.GenericDelegate<IAnalyzerSessionManager>(_expertHost_SessionsUpdateEvent);
            //_host.SourcesUpdateEvent += new GeneralHelper.GenericDelegate<IAnalyzerSessionManager>(_expertHost_SourcesUpdateEvent);
            //UpdateUI();
        }

        private void AnalyzerHostControl_Load(object sender, EventArgs e)
        {
            _expertControl = FxpaCommonControl.CreateCorrespondingControl(_host.Analyzer);
            if (_expertControl == null)
            {
                labelMain.Text = _host.AnalyzerName + ", " + _host.Analyzer.GetType().Name + " has no user interface component.";
                return;
            }

            _expertControl.Dock = DockStyle.Fill;
            _expertControl.Parent = this;
            _expertControl.BringToFront();
        }

        public override void UnInitializeControl()
        {
            base.UnInitializeControl();

            if (_host != null)
            {
                //_host.SessionsUpdateEvent -= new GeneralHelper.GenericDelegate<IAnalyzerSessionManager>(_expertHost_SessionsUpdateEvent);
                //_host.SourcesUpdateEvent -= new GeneralHelper.GenericDelegate<IAnalyzerSessionManager>(_expertHost_SourcesUpdateEvent);
                _host = null;
            }
        }

   

    }
}
