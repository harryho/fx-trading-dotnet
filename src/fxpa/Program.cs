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
using System.Reflection;
using System.Windows.Forms;

using fxpa.Properties;
using System.Threading;
using System.Collections.Generic;

namespace fxpa
{
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        [MTAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            // .NET FX release runtime only enforced cross-thread UI checks under a debugger; .NET (Core) enforces always. The 2008 app updates charts from provider threads - restore original behaviour.
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.ThreadExit += new EventHandler(Application_ThreadExit);
            //AppContext.AppConnect();

            Console.WriteLine(Application.CommonAppDataPath);
            Console.WriteLine(Application.LocalUserAppDataPath);
            Console.WriteLine(Application.ExecutablePath);
            Console.WriteLine(Application.StartupPath);
            Console.WriteLine(Application.UserAppDataPath);
            Console.WriteLine(Application.UserAppDataRegistry);

            LogUtil.init(true, true, true, true);
            LogUtil.Info("\n\n ####################################################### \n\n ");

            Console.WriteLine(" LoadingForm loadingForm = new LoadingForm(); ");
            AppConst.InitAppSettings();
            LoadingForm loadingForm = new LoadingForm();

            if (loadingForm.ShowDialog() == DialogResult.OK)
            {
                if (!AppContext.NoSubscription)
                {
                    ProviderService.Init();
                    Form mainForm = new FxpaForm();
                    Application.Run(mainForm);
                    AppContext.FxpaMain = mainForm;
                }
            }

        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            //SystemMonitor.Error(e.Exception.Message);
            Console.WriteLine(" e " + e.Exception);
            LogUtil.Error(e.Exception.ToString());
            LogUtil.Close();
        }

        static void Application_ThreadExit(object sender, EventArgs e)
        {
            LogUtil.Info("Application Exit");
            LogUtil.Close();
        }
    }
}
