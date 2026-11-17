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
    public partial class PenControl : UserControl
    {
        Pen pen;
        public Pen Pen
        {
            get 
            { 
                return pen; 
            }

            set 
            { 
                pen = (Pen)value.Clone();
                UpdateUI();
            }
        }

        string penName;
        public string PenName
        {
            get 
            { 
                return penName;
            }

            set 
            { 
                penName = value;
                UpdateUI();
            }
        }

        bool _readOnly = false;

        public bool ReadOnly
        {
            get 
            { 
                return _readOnly; 
            }

            set 
            { 
                _readOnly = value;
                UpdateUI();
            }
        }

        public delegate void PenChangedDelegate(PenControl control);
        public event PenChangedDelegate PenChangedEvent;

        /// <summary>
        /// 
        /// </summary>
        public PenControl()
        {
            InitializeComponent();
            comboBoxDash.Items.AddRange(Enum.GetNames(typeof(System.Drawing.Drawing2D.DashStyle)));
        }

        private void PenControl_Load(object sender, EventArgs e)
        {
        }

        void UpdateUI()
        {
            if (pen == null)
            {
                labelName.Text = "";
                comboBoxDash.SelectedIndex = -1;
                labelColor.BackColor = this.BackColor;
                return;
            }

            labelColor.BackColor = pen.Color;
            labelName.Text = penName;
            comboBoxDash.Enabled = !ReadOnly;
            comboBoxDash.SelectedIndex = (int)pen.DashStyle;
        }

        private void labelColor_Click(object sender, EventArgs e)
        {
            if (ReadOnly)
            {
                return;
            }

            ColorDialog dialog = new ColorDialog();
            dialog.Color = pen.Color;
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                pen.Color = dialog.Color;
            }

            UpdateUI();

            if (PenChangedEvent != null)
            {
                PenChangedEvent(this);
            }
        }

        private void comboBoxDash_SelectedIndexChanged(object sender, EventArgs e)
        {
            pen.DashStyle = (System.Drawing.Drawing2D.DashStyle)Enum.Parse(typeof(System.Drawing.Drawing2D.DashStyle), comboBoxDash.SelectedItem.ToString());

            if (PenChangedEvent != null)
            {
                PenChangedEvent(this);
            }
        }
    }
}
