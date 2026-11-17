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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


using fxpa.Properties;

using System.Reflection;
using System.Threading;


namespace fxpa
{
    public partial class FxpaForm : Form
    {

        FxpaBase fxpaBaseForm;
        FxpaBase FxpaBase
        {
            get { return fxpaBaseForm; }
        }

        bool _hideTabTitles = false;
        public bool HideTabTitles
        {
            get { return _hideTabTitles; }
            set { _hideTabTitles = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public FxpaForm()
        {
          InitializeComponent();

        }

        // .NET Core delivers some menu commands (UIA clients, IME) on non-UI RPC threads; showing a dropdown
        // from such a thread creates its native handle there, and since RPC threads never pump messages, any
        // later UI-thread access to that dropdown (submenu hover measurement, GetWindowText) blocks the UI
        // thread forever. Create all dropdown handles on the UI thread up front so those calls reuse UI-owned
        // handles, matching .NET Framework behaviour.
        private void PreCreateMenuDropDownHandles(System.Windows.Forms.ToolStripItemCollection items)
        {
            foreach (System.Windows.Forms.ToolStripItem itemEntry in items)
            {
                System.Windows.Forms.ToolStripMenuItem item = itemEntry as System.Windows.Forms.ToolStripMenuItem;
                if (item == null)
                {
                    continue;
                }
                System.Windows.Forms.ToolStripDropDown dropDown = item.DropDown;
                if (dropDown.IsHandleCreated == false)
                {
                    IntPtr forceCreatedHandle = dropDown.Handle;   // reading Handle forces creation on the calling (UI) thread; CreateControl() is a no-op for invisible controls
                }
                PreCreateMenuDropDownHandles(dropDown.Items);
            }
        }

        private void FxpaMainForm_Load(object sender, EventArgs e)
        {
            PreCreateMenuDropDownHandles(menuStripMain.Items);
            PreCreateMenuDropDownHandles(contextMenuStrip1.Items);
            this.IsMdiContainer = true;

            tabControl.TabPages.Clear();
            LoadPlatform(new FxpaBase());

        }

        private void FxpaMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            DialogResult dialogResult = MessageBox.Show("Are you sure to exit? ", "Exit", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.OK)
            {
                //logOutHandler.Execute();
                LogUtil.Info("Client Close");
                fxpaBaseForm.UnInitialize();
                fxpaBaseForm.ActiveComponentUpdateEvent -= new FxpaBase.ActiveComponentUpdateDelegate(platform_ActiveComponentUpdateEvent);
                fxpaBaseForm = null;
                Application.ExitThread();
            }
            else
            {
                e.Cancel = true;
            }
        }

        void SetComponentMenuItemDropDownItems(ToolStripMenuItem item, Type parentingType,
            bool includeParentingType, string creationTitle, Image creationImage, Type[] constructorTypes)
        {
            if (fxpaBaseForm == null)
            {
                return;
            }

            if (constructorTypes != null)
            {// Add types available for creation

                List<Type> resultingTypes = ReflectionHelper.GatherTypeChildrenTypesFromAssembliesWithMatchingConstructor(parentingType,
                    false, ReflectionHelper.GetApplicationEntryAssemblyAndReferencedAssemblies(), constructorTypes);
                if (includeParentingType && parentingType.IsAbstract == false)
                {
                    resultingTypes.Add(parentingType);
                }

                foreach (Type componentType in resultingTypes)
                {
                    string name = parentingType.Name;
                    CustomNameAttribute.GetClassAttributeValue(componentType, ref name);
                    ToolStripMenuItem newItem = new ToolStripMenuItem(creationTitle + name, creationImage);
                    newItem.Tag = componentType;
                    newItem.Click += new EventHandler(createItem_Click);
                    item.DropDownItems.Add(newItem);
                }
            }

            // Existing platform components.
            foreach (IFxpaBaseCompoent component in fxpaBaseForm.Components)
            {
                if (parentingType.IsInstanceOfType(component))
                {
                    string name = component.Name;
                    ToolStripMenuItem newItem = new ToolStripMenuItem("Remove " + name, Properties.Resources.DELETE2);
                    newItem.Tag = component;
                    newItem.Click += new EventHandler(removeItem_Click);
                    item.DropDownItems.Add(newItem);
                }
            }

            // Also check the UI components that are stand alone (have no platform component).
            foreach (TabPage page in tabControl.TabPages)
            {
                if (((FxpaCommonControl)page.Tag).Component == null ||
                    ((FxpaCommonControl)page.Tag).Component is IFxpaBaseCompoent == false)
                {
                    if (parentingType.IsInstanceOfType((FxpaCommonControl)page.Tag))
                    {// Found existing standalone component.
                        string name = ((FxpaCommonControl)page.Tag).Name;
                        ToolStripMenuItem newItem = new ToolStripMenuItem("Remove " + name, Properties.Resources.DELETE2);
                        newItem.Tag = ((FxpaCommonControl)page.Tag);
                        newItem.Click += new EventHandler(removeItem_Click);
                        item.DropDownItems.Add(newItem);
                    }
                }
            }
        }

        void UpdateComponentsMenu()
        {
            tabControl.Visible = (tabControl.TabPages.Count != 0);
        }

        void createItem_Click(object sender, EventArgs e)
        {

            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Type type = (Type)item.Tag;

            IFxpaBaseCompoent component = null;

            if (type.IsSubclassOf(typeof(Analyzer)))
            {
                component = new LocalAnalyzerHost(CustomNameAttribute.GetClassAttributeName(type), type);
            }
            else
            {
                ConstructorInfo info = type.GetConstructor(new Type[] { });
                if (info != null)
                {
                    component = (IFxpaBaseCompoent)info.Invoke(new object[] { });
                }
            }

            // ...
            if (component == null)
            {
                MessageBox.Show("Failed to create component.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Set settings for component.
            if (component.SetInitialState(fxpaBaseForm.Settings) == false)
            {
                MessageBox.Show("Component failed to initialize from initial state.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Register to platform.
            fxpaBaseForm.RegisterComponent(component);
        }

        void removeItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            IFxpaBaseCompoent component = (IFxpaBaseCompoent)item.Tag;

            if (component == null)
            {// This a UI only component.
                RemoveComponentTab((FxpaCommonControl)item.Tag);
            }
            else
                if (MessageBox.Show("Remove " + component.Name + "?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    fxpaBaseForm.UnRegisterComponent(component);
                    UpdateComponentsMenu();
                }
        }

        void UpdateTabTitles()
        {
            foreach (TabPage page in tabControl.TabPages)
            {
                if (HideTabTitles)
                {
                    page.Text = "";
                }
                else
                {
                    page.Text = page.Name;
                }
            }

            if (HideTabTitles && tabControl.SelectedIndex > -1)
            {
                tabControl.TabPages[tabControl.SelectedIndex].Text = tabControl.TabPages[tabControl.SelectedIndex].Name;
            }
        }

        void SetupDefaultPlatform()
        {
            this.tabControl.SuspendLayout();
            this.tabControl.Visible = false;

            // Harry-RS
            FxpaSource fxpaSource = new FxpaSource();
            fxpaSource.SetInitialState(fxpaBaseForm.Settings);
            fxpaBaseForm.RegisterComponent(fxpaSource);

            LocalAnalyzerHost host = new LocalAnalyzerHost("Manual Trading", typeof(ProfessionalAnalyzer));
            fxpaBaseForm.RegisterComponent(host);

            //Thread.Sleep(250);

            ProfessionalAnalyzer expert = (ProfessionalAnalyzer)host.Analyzer;

            this.tabControl.ResumeLayout();
            this.tabControl.Visible = true;
        }

        void LoadPlatform(FxpaBase platform)
        {
            fxpaBaseForm = platform;
            if (fxpaBaseForm != null)
            {
                fxpaBaseForm.ActiveComponentUpdateEvent += new FxpaBase.ActiveComponentUpdateDelegate(platform_ActiveComponentUpdateEvent);
                fxpaBaseForm.Initialize(Settings.Default);
            }

            if (fxpaBaseForm != null)
            {
                this.Text = "FXPA";
                //this.skinEngine1.SkinFile = "skin/default.ssk";

                UpdateToolsMenu();
                UpdateViewMenu();
                UpdateComponentsMenu();

                //signalHandler.SignalListView = signalListView;
                //signalHandler.StatListView = statListView;

                //signalListHandler.SignalListView = signalListView;
                //signalListHandler.StatListView = statListView;

                //AppContext.SignalHandler = signalHandler;
                //AppContext.SignalListHandler = signalListHandler;
                AppContext.SignalListView = signalListView;
                AppContext.StatListView = statListView;
                AppContext.FxpaMain = this;

                //InfoPublishForm infoPublishForm = new InfoPublishForm();
                //AppContext.InfoPublishHandler.InfoListView = this.infoListView;

                DataService.Initialze();

                SetupDefaultPlatform();

                //timeCheckHandler.FXPA = this;
                //timeCheckHandler.SignalListView = signalListView;
                //timeCheckHandler.InfoListView = infoListView;

                //AppContext.TimeCheckHandler = timeCheckHandler;

                //AppClient.RegisterHandler(Protocol.S0002_1, timeCheckHandler);
                //AppClient.RegisterHandler(Protocol.C0001_4, logOutHandler);

            }
        }

        void UpdateViewMenu()
        {
        }

        void UpdateToolsMenu()
        {
        }

        private void registerAssembliesToGACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (RegisterReferencedAssembliesToGAC())
            //{
            //    MessageBox.Show("Assemblies registered to GAC.", "Operation");
            //}
            //else
            //{
            //    MessageBox.Show("Failed to register assemblies to GAC.", "Operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void unregisterAssembliesFromGACToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }


        void CreateComponentTab(FxpaCommonControl componentControl)
        {
            if (componentControl != null && componentControl.Title.Length > 0)
            {
                Console.WriteLine(" componentControl.Name   >>>>  " + componentControl.Name);
                TabPage newPage = new TabPage();
                tabControl.TabPages.Add(newPage);
                if (componentControl.Title == "Manual Trading")
                    newPage.Name = "Analyzer Analytic";
                else
                    newPage.Name = componentControl.Title;

                if (string.IsNullOrEmpty(componentControl.ImageName) == false)
                {
                    newPage.ImageKey = componentControl.ImageName;
                }
                else
                {
                    newPage.ImageKey = "dot.png";
                }

                componentControl.Dock = DockStyle.Fill;
                componentControl.Parent = newPage;
                newPage.Tag = componentControl;
                tabControl.SelectedIndex = tabControl.TabPages.Count - 1;
            }
        }

        void RemoveComponentTab(FxpaCommonControl componentControl)
        {
            TabPage page = (TabPage)componentControl.Parent;
            tabControl.TabPages.Remove(page);
        }

        void uiThread_ActiveComponentUpdateEvent(IFxpaBaseCompoent component, bool added)
        {
            if (added)
            {
                FxpaCommonControl control = FxpaCommonControl.CreateCorrespondingControl(component);
                CreateComponentTab(control);
            }
            else
            {
                foreach (TabPage page in tabControl.TabPages)
                {
                    IFxpaBaseCompoent currentComponent = ((FxpaCommonControl)page.Tag).Component as IFxpaBaseCompoent;
                    if (currentComponent == component)
                    {
                        RemoveComponentTab((FxpaCommonControl)page.Tag);
                        break;
                    }
                }
            }

            UpdateTabTitles();
            UpdateComponentsMenu();
        }

        void platform_ActiveComponentUpdateEvent(IFxpaBaseCompoent component, bool added)
        {
            this.BeginInvoke(new GeneralHelper.GenericDelegate<IFxpaBaseCompoent, bool>(
                uiThread_ActiveComponentUpdateEvent), component, added);
        }

        private void labelCloseTab_Click(object sender, EventArgs e)
        {
            tabControl.TabPages.RemoveAt(tabControl.SelectedIndex);
        }

        private void toolStripLabelAdds_MouseEnter(object sender, EventArgs e)
        {
            //toolStripLabelRemove.Image = Properties.Resources.button_cancel_12_b;
        }

        private void toolStripLabelAdds_MouseLeave(object sender, EventArgs e)
        {
            //toolStripLabelRemove.Image = Properties.Resources.button_cancel_12;
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTabTitles();
        }

        private void autoHideTabTitlesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateTabTitles();
        }

        private void toolStripLabelRemove_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                if (((FxpaCommonControl)(tabControl.SelectedTab.Tag)).Component == null)
                {// This a UI only component - directly remove.
                    RemoveComponentTab((FxpaCommonControl)(tabControl.SelectedTab.Tag));
                }
                else
                {// Platform component - remote trough platform.
                    fxpaBaseForm.UnRegisterComponent(((FxpaCommonControl)(tabControl.SelectedTab.Tag)).Component as IFxpaBaseCompoent);
                }
            }
        }

        private void tabControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabControl.TabPages.Count; i++)
                {
                    if (tabControl.GetTabRect(i).Contains(e.Location))
                    {
                        tabControl.SelectedIndex = i;

                        contextMenuStrip1.Items.Clear();
                        ToolStripItem item = contextMenuStrip1.Items.Add(Properties.Resources.DELETE2);
                        item.Text = "Remove";
                        item.Click += new EventHandler(toolStripLabelRemove_Click);

                        contextMenuStrip1.Show(this, new Point(e.X, e.Y + contextMenuStrip1.Height));
                    }
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Forex Professional Analyzer (FXPA)\r\nversion." + GeneralHelper.ApplicationVersion + "\r\nhttps://github.com/harryho/fx-trading-dotnet", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private void menuItemResetPwd_Click(object sender, EventArgs e)
        {
            //ResetPwdForm resetPwdForm = new ResetPwdForm();
            //ResetPwdHandler resetPwdHandler = new ResetPwdHandler(AppClient.GetInstance);
            //resetPwdHandler.ResetPwdForm = resetPwdForm;
            //resetPwdForm.ShowDialog();
        }



        private void menuItemExit_Click(object sender, EventArgs e)
        {
            //if (_platform != null)
            //{
            //    _platform.UnInitialize();
            //    _platform.ActiveComponentUpdateEvent -= new Platform.ActiveComponentUpdateDelegate(platform_ActiveComponentUpdateEvent);
            //    _platform = null;
            //    tabControl.TabPages.Clear();
            //    this.Close();
            //}

            DialogResult dialogResult = MessageBox.Show("Are you sure to exit? ", "Exit", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.OK)
            {
                //logOutHandler.Execute();
                LogUtil.Info("Client Close");
                fxpaBaseForm.UnInitialize();
                fxpaBaseForm.ActiveComponentUpdateEvent -= new FxpaBase.ActiveComponentUpdateDelegate(platform_ActiveComponentUpdateEvent);
                fxpaBaseForm = null;
                Application.ExitThread();
            }



        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        public void PopupWarningMsg(string msg)
        {
            MessageBox.Show(AppContext.FxpaMain, "Since Network issue，与Server的 connection has been disconnected，please try Re-Login", "Exit", MessageBoxButtons.OK, MessageBoxIcon.Question);
            this.warningStatusLabel.Text = msg;
            this.warningStatusLabel.Invalidate();
        }


        public void UpdateWarningStatusLabel(string msg)
        {
            this.warningStatusLabel.Text = msg;
            this.warningStatusLabel.Invalidate();
        }

        public void UpdateWelcomeStatusLabel(string msg)
        {
            this.welcomeStatusLabel.Text = msg;
            this.welcomeStatusLabel.Invalidate();

        }

        private void menuItemRelogin_Click(object sender, EventArgs e)
        {
            //ReLoginForm reLoginForm = new ReLoginForm();
            //reLoginForm.ShowDialog();
        }

        private void infoListView_SizeChanged(Object sender, EventArgs e)
        {
            //Adjust Listview column width
            infoListView_ColumnWidthChanged();
        }

        private void infoListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            //Adjust Listview column width
            infoListView_ColumnWidthChanged();
        }

        private void infoListView_ColumnWidthChanged()
        {
            infoListView.ColumnWidthChanged -= new ColumnWidthChangedEventHandler(infoListView_ColumnWidthChanged);
            columnHeader15.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            infoListView.ColumnWidthChanged += new ColumnWidthChangedEventHandler(infoListView_ColumnWidthChanged);
        }


        private void signalListView_SizeChanged(Object sender, EventArgs e)
        {
            //Adjust Listview column width
            signalListView_ColumnWidthChanged();

        }

        private void signalListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            //Adjust Listview column width
            signalListView_ColumnWidthChanged();
        }

        private void signalListView_ColumnWidthChanged()
        {
            signalListView.ColumnWidthChanged -= new ColumnWidthChangedEventHandler(signalListView_ColumnWidthChanged);
            columnHeader23.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            signalListView.ColumnWidthChanged += new ColumnWidthChangedEventHandler(signalListView_ColumnWidthChanged);
        }

        private void statListView_SizeChanged(Object sender, EventArgs e)
        {
            //Adjust Listview column width
            statListView_ColumnWidthChanged();
        }

        private void statListView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            //Adjust Listview column width
            statListView_ColumnWidthChanged();
        }

        private void statListView_ColumnWidthChanged()
        {
            statListView.ColumnWidthChanged -= new ColumnWidthChangedEventHandler(statListView_ColumnWidthChanged);
            columnHeader24.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            statListView.ColumnWidthChanged += new ColumnWidthChangedEventHandler(statListView_ColumnWidthChanged);
        }

        private void mainTabControl_DoubleClick(object sender, EventArgs e)
        {
            if (AppContext.SignalDatas.Count > 0)
            {
                Signal[] signals = null;
                lock (AppConst.SignalDatasLocker)
                {
                    signals = new Signal[AppContext.SignalDatas.Count];
                    AppContext.SignalDatas.CopyTo(signals, 0);
                }
                Array.Sort(signals);
                if (mainTabControl.SelectedTab == signalTabPage)
                {
                    lock (signalListView) //  AppContext.SignalListView)
                    {
                        signalListView.Items.Clear();
                        foreach (Signal signal in signals)
                        {
                            string strSignal = AppUtil.GetSignalChinese(signal.Arrow);
                            int arrow = signal.Arrow == -1 ? 0 : 1;
                            ListViewItem item = new ListViewItem(new string[] { signal.Symbol.ToString(), signal.ActPrice.ToString(), strSignal, signal.ActTime.ToString(), signal.StopLossPrice.ToString(), signal.StopGainPrice.ToString() }, arrow);
                            signalListView.Items.Add(item);
                        }
                        signalListView.Invalidate();
                    }
                }
                else if (mainTabControl.SelectedTab == statTabPage && AppContext.SignalDatas.Count > 0)
                {
                    lock (statListView) //  AppContext.SignalListView)
                    {
                        statListView.Items.Clear();
                        foreach (Signal signal in signals)
                        {
                            string strSignal = AppUtil.GetSignalChinese(signal.Arrow);
                            ListViewItem item = null; int arrow = signal.Arrow == -1 ? 0 : 1;
                            if (signal.ProfitPrice > 0)
                                item = new ListViewItem(new string[] { signal.Symbol.ToString(), signal.ActPrice.ToString(), strSignal, signal.ActTime.ToString(), signal.ProfitTime.ToString(), signal.Profit.ToString() }, arrow);
                            else
                                item = new ListViewItem(new string[] { signal.Symbol.ToString(), signal.ActPrice.ToString(), strSignal, signal.ActTime.ToString(), "", "" }, arrow);
                            statListView.Items.Add(item);
                        }
                        statListView.Invalidate();
                    }
                }
            }
        }

        private void mainTabControl_Click(object sender, EventArgs e)
        {
            mainTabControl.SelectedTab.ForeColor = Color.Orange;
            //mainTabControl.SelectedTab.BackColor = Color.Orange;
            mainTabControl.Invalidate();
        }

        private void TabPageControl_Click(object sender, EventArgs e)
        {
            ((TabPage)sender).ForeColor = Color.Orange;
            //mainTabControl.SelectedTab.BackColor = Color.Orange;
            ((TabPage)sender).Invalidate();
        }

        private void manualTradeAnalyzerControl1_Load(object sender, EventArgs e)
        {

        }
       

        private void OpenSessionIndicatorsDialog()
        {
            if (AppContext.TradeAnalyzerControl != null && AppContext.TradeAnalyzerControl.ASession != null)
            {
                AnalyzerSessionIndicatorsControl control = new AnalyzerSessionIndicatorsControl(AppContext.TradeAnalyzerControl.ASession, AppContext.TradeAnalyzerControl.OpChartControl.Panes);
                control.AddIndicatorEvent += new AnalyzerSessionIndicatorsControl.AddIndicatorDelegate(((AnalyzerSessionControl)(AppContext.TradeAnalyzerControl.ControlButton.Tag)).control_AddIndicatorEvent);
                HostingForm form = new HostingForm("Session " + AppContext.TradeAnalyzerControl.ASession.SessionInfo.Id + " Indicators", control);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.ShowDialog();
            }
        }

        private void rSIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSessionIndicatorsDialog();
        }

        private void indicatorMenuItem_Click(object sender, EventArgs e)
        {
            OpenSessionIndicatorsDialog();
        }

    }
}
