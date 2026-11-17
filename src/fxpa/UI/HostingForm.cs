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

namespace fxpa
{
    public partial class HostingForm : Form
    {
        Control control;
        public Control Control
        {
            get { return control; }
        }

        public bool ShowCloseButton
        {
            get { return panelButtonClose.Visible; }
            set { panelButtonClose.Visible = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public HostingForm(string formText)
        {
            InitializeComponent();
            this.Text = formText;
        }

        /// <summary>
        /// 
        /// </summary>
        public HostingForm(string formText, Control control)
        {
            InitializeComponent();
            this.Text = formText;

            this.control = control;
            this.control.Visible = true;

            this.SuspendLayout();

            Controls.Add(control);
            control.Dock = System.Windows.Forms.DockStyle.Fill;

            Size = control.Size;
            Width += 6;
            Height += 55;

            this.ResumeLayout();
        }

        void controlVisibleChanged(object sender, EventArgs e)
        {
            if (control.Visible == false && this.Visible == true)
            {
                this.Close();
            }
        }

        public static HostingForm ShowHostingForm(Control control, string controlName)
        {
            HostingForm form = CreateHostingFormControl(control, controlName);
            form.Show();
            return form;
        }

        /// <summary>
        /// 
        /// </summary>
        public static HostingForm CreateHostingFormControl(Control control, string controlName)
        {
            Size requiredSize = control.Size;
            HostingForm hostingForm = new HostingForm(controlName);
            hostingForm.Controls.Add(control);
            control.Dock = System.Windows.Forms.DockStyle.Fill;
            hostingForm.Size = requiredSize;
            return hostingForm;
        }

        private void HostingForm_Load(object sender, EventArgs e)
        {
            // Not before that.
            control.VisibleChanged += new EventHandler(controlVisibleChanged);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            // If shown with Show() dialog result is not considered.
            this.Close();
        }

    }
}
