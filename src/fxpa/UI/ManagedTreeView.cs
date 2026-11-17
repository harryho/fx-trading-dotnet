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
using System.Windows.Forms;
using System.Drawing;

namespace fxpa
{
	/// <summary>
	/// Summary description for ManagedTree.
	/// </summary>
	public class ManagedTreeView : TreeView
	{
		public ManagedTreeView()
		{
		}

		IManagedTreeNode _root;
		public IManagedTreeNode Root
		{
			get
			{
				return _root;
			}
			set
			{
				_root = value;
				CreateTree();
			}
		}

		protected void CreateTree()
		{
			this.Nodes.Clear();
			if ( _root != null )
			{
				_root.TreeModifiedEvent += new ModifiedDelegate(NodeModified);
				TreeNode rootNode = new TreeNode();
				UpdateNode(rootNode, _root);
				this.Nodes.Add(rootNode);
			}
			this.ExpandAll();
		}

		public TreeNode GetNodeByTag(TreeNodeCollection nodes, object tag)
		{
			foreach(TreeNode node in nodes)
			{
				if ( node.Tag == tag )
				{
					return node;
				}
				if ( GetNodeByTag(node.Nodes, tag) != null )
				{
					return GetNodeByTag(node.Nodes, tag);
				}
			}
			return null;
		}

		public void NodeModified(IManagedTreeNode node)
		{
			this.SuspendLayout();

			TreeNode treeNode = GetNodeByTag(Nodes, node);
			if ( treeNode != null )
			{
				UpdateNode(treeNode, node);
			}

			this.ExpandAll();
			this.ResumeLayout();
		}

		protected void UpdateNode(TreeNode treeNode, IManagedTreeNode node)
		{
			treeNode.Tag = node;
			treeNode.Text = node.TreeText;
			treeNode.ImageIndex = node.TreeImageIndex;
			treeNode.SelectedImageIndex = node.TreeImageIndex;

			for(int i=0; i<node.TreeChildren.Length; i++)
			{
				if ( treeNode.Nodes.Count > i )
				{// Update child.
					UpdateNode(treeNode.Nodes[i], node.TreeChildren[i]);
					continue;
				}
				
				// New child.
				TreeNode newChild = new TreeNode();
				node.TreeChildren[i].TreeModifiedEvent += new ModifiedDelegate(NodeModified);
				UpdateNode(newChild, node.TreeChildren[i]);
				treeNode.Nodes.Add(newChild);
			}

			for(int i=treeNode.Nodes.Count-1; i>=node.TreeChildren.Length; i--)
			{
				treeNode.Nodes[i].Remove();
			}
		}
    }

    public class DoubleBufferListView : ListView
    {
        public DoubleBufferListView()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawSubItem(e);
            double d;

            if (e.Item.BackColor != Color.MediumPurple)
            {
                if (e.Item.Index % 2 == 0) //theSubItemIndex是你指定要重绘的列
                {
                    //e.Item.BackColor = Color.FromArgb(247, 247, 247);

                    e.Item.BackColor = Color.Azure;
                    int count = e.Item.SubItems.Count - 1;
                    while (count >= 1)
                    {
                        e.Item.SubItems[count].BackColor = Color.Azure;
                        count--;
                    }
                    //e.Item.SubItems[2].BackColor = Color.FromArgb(247, 247, 247);
                }
                else
                {
                    //e.Item.ForeColor = Color.;
                }
            }
        }
    }
}
