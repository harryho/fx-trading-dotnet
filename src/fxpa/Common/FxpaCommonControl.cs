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

using System.Reflection;


namespace fxpa
{
    public partial class FxpaCommonControl : UserControl
    {
        string description = "";
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        string _title = "";
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        protected string _imageName = "";
        public virtual string ImageName
        {
            get { return _imageName; }
            set { _imageName = value; }
        }

        public object Component
        {
            get { return Tag; }
        }

        /// <summary>
        /// 
        /// </summary>
        public FxpaCommonControl()
        {
            InitializeComponent();
            this.Name = this.GetType().Name;

        }

        private void CommonBaseControl_Load(object sender, EventArgs e)
        {
            this.HandleDestroyed += new EventHandler(CommonBaseControl_HandleDestroyed);
        }

        void CommonBaseControl_HandleDestroyed(object sender, EventArgs e)
        {
            UnInitializeControl();
        }

        /// <summary>
        /// Perform unloading here.
        /// </summary>
        public virtual void UnInitializeControl()
        {
            Tag = null;
        }


        

        static public FxpaCommonControl CreateCorrespondingControl(object component)
        {
            List<Type> types = ReflectionHelper.GatherTypeChildrenTypesFromAssembliesWithMatchingConstructor(typeof(FxpaCommonControl), true, ReflectionHelper.GetReferencedAndInitialAssembly(Assembly.GetEntryAssembly()), new Type[] { component.GetType() });
            if (types.Count == 0)
            {
                //// Try also with a more relaxed type match.
                //types = ReflectionHelper.GatherTypeChildrenTypesFromAssembliesWithMatchingConstructor(typeof(CommonBaseControl), false, ReflectionHelper.GetReferencedAndInitialAssembly(Assembly.GetEntryAssembly()), new Type[] { component.GetType() });
                //if (types.Count == 0)
                //{
                    return null;
                //}
            }

           // SystemMonitor.CheckWarning(types.Count == 1, "More than 1 control found for this type of component, creating the first one.");

            // Return the first proper object.
            FxpaCommonControl control = (FxpaCommonControl)types[0].GetConstructor(new Type[] { component.GetType() }).Invoke(new object[] { component });
            control.Tag = component;
            return control;
        }



    }
}
