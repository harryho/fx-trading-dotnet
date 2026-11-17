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

namespace fxpa
{
    public class ListEx<TClass> : List<TClass>
    {
        bool _singleEntryMode = true;
        /// <summary>
        /// An item is allowed to enter only once.
        /// </summary>
        public bool SingleEntryMode
        {
            get { return _singleEntryMode; }
            set { _singleEntryMode = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ListEx()
        {
        }

        public void UpdateItem(TClass item, bool isAdded)
        {
            if (isAdded)
            {
                Add(item);
            }
            else
            {
                Remove(item);
            }
        }

        public new bool Add(TClass item)
        {
            if (SingleEntryMode && this.Contains(item))
            {
                return false;
            }
            base.Add(item);
            return true;
        }

        public new void AddRange(IEnumerable<TClass> collection)
        {
            if (SingleEntryMode)
            {
                List<TClass> items = new List<TClass>();
                foreach (TClass item in collection)
                {
                    if (this.Contains(item) == false)
                    {
                        items.Add(item);
                    }
                }
                base.AddRange(items);
            }
            else
            {
                base.AddRange(collection);
            }
        }

        public new void Insert(int index, TClass item)
        {
            if (SingleEntryMode && this.Contains(item))
            {
                return;
            }

            base.Insert(index, item);
        }

        public new void InsertRange(int index, IEnumerable<TClass> collection)
        {
            if (SingleEntryMode)
            {
                List<TClass> items = new List<TClass>();
                foreach (TClass item in collection)
                {
                    if (this.Contains(item) == false)
                    {
                        items.Add(item);
                    }
                }
                base.InsertRange(index, items);
            }
            else
            {
                base.InsertRange(index, collection);
            }
            
        }

    }
}
