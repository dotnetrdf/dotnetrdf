/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Windows.Forms;
using VDS.RDF.GUI.WinForms;
using VDS.RDF.Query;

namespace VDS.RDF.Utilities.StoreManager.Forms
{
    /// <summary>
    /// Extension to form class which provides a bunch of useful methods for doing cross thread invokes
    /// </summary>
    public class CrossThreadForm 
        : Form
    {
        #region Cross Thread Messaging

        private delegate void CrossThreadMessageDelegate(String message, String title, MessageBoxIcon icon);

        protected void CrossThreadMessage(String message, String title, MessageBoxIcon icon)
        {
            if (this.InvokeRequired)
            {
                CrossThreadMessageDelegate d = this.CrossThreadMessage;
                this.Invoke(d, new Object[] { message, title, icon });
            }
            else
            {
                MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
            }
        }

#endregion

        #region Cross Thread Set Text properties

        private delegate void CrossThreadSetTextDelegate(Control c, String text);

        private delegate void CrossThreadSetToolStripTextDelegate(ToolStripItem item, String text);

        protected void CrossThreadSetText(Control c, String text)
        {
            if (this.InvokeRequired)
            {
                CrossThreadSetTextDelegate d = this.CrossThreadSetText;
                this.Invoke(d, new Object[] { c, text });
            }
            else
            {
                c.Text = text;
            }
        }

        protected void CrossThreadSetText(ToolStripItem item, String text)
        {
            if (this.InvokeRequired)
            {
                CrossThreadSetToolStripTextDelegate d = this.CrossThreadSetText;
                this.Invoke(d, new Object[] { item, text });
            }
            else
            {
                item.Text = text;
            }
        }

#endregion

        #region Cross Thread Set Visibility properties

        private delegate void CrossThreadSetVisibilityDelegate(Control c, bool visible);

        protected void CrossThreadSetVisibility(Control c, bool visible)
        {
            if (this.InvokeRequired)
            {
                CrossThreadSetVisibilityDelegate d = this.CrossThreadSetVisibility;
                this.Invoke(d, new Object[] { c, visible });
            }
            else
            {
                c.Visible = visible;
            }
        }

#endregion

        #region Cross Thread Refresh

        private delegate void CrossThreadRefreshDelegate(Control c);

        protected void CrossThreadRefresh(Control c)
        {
            if (this.InvokeRequired)
            {
                CrossThreadRefreshDelegate d = this.CrossThreadRefresh;
                this.Invoke(d, new Object[] { c });
            }
            else
            {
                c.Refresh();
            }
        }

#endregion

        #region Cross Thread ListView manipulation

        private delegate void CrossThreadBeginUpdateDelegate(ListView lview);

        protected void CrossThreadBeginUpdate(ListView lview)
        {
            if (this.InvokeRequired)
            {
                CrossThreadBeginUpdateDelegate d = this.CrossThreadBeginUpdate;
                this.Invoke(d, new Object[] { lview });
            }
            else
            {
                lview.BeginUpdate();
            }
        }

        private delegate void CrossThreadEndUpdateDelegate(ListView lview);

        protected void CrossThreadEndUpdate(ListView lview)
        {
            if (this.InvokeRequired)
            {
                CrossThreadEndUpdateDelegate d = this.CrossThreadEndUpdate;
                this.Invoke(d, new Object[] { lview });
            }
            else
            {
                lview.EndUpdate();
            }
        }

        private delegate void CrossThreadClearDelegate(ListView lview);

        protected void CrossThreadClear(ListView lview)
        {
            if (this.InvokeRequired)
            {
                CrossThreadClearDelegate d = this.CrossThreadClear;
                this.Invoke(d, new Object[] { lview });
            }
            else
            {
                lview.Items.Clear();
            }
        }

        private delegate void CrossThreadAddDelegate(ListView lview, String item);

        protected void CrossThreadAdd(ListView lview, String item)
        {
            if (this.InvokeRequired)
            {
                CrossThreadAddDelegate d = this.CrossThreadAdd;
                this.Invoke(d, new Object[] { lview, item });
            }
            else
            {
                lview.Items.Add(item);
            }
        }

        private delegate void CrossThreadAddItemDelegate(ListView lview, ListViewItem item);

        protected void CrossThreadAddItem(ListView lview, ListViewItem item)
        {
            if (this.InvokeRequired)
            {
                CrossThreadAddItemDelegate d = this.CrossThreadAddItem;
                this.Invoke(d, new Object[] { lview, item });
            }
            else
            {
                lview.Items.Add(item);
            }
        }

        private delegate void CrossThreadAlterSubItemDelegate(ListViewItem item, int index, String text);

        protected void CrossThreadAlterSubItem(ListViewItem item, int index, String text)
        {
            if (this.InvokeRequired)
            {
                CrossThreadAlterSubItemDelegate d = this.CrossThreadAlterSubItem;
                this.Invoke(d, new Object[] { item, index, text });
            }
            else
            {
                item.SubItems[index] = new ListViewItem.ListViewSubItem(item, text);
            }
        }

        #endregion

        #region Cross Thead ListBox manipulation

        private delegate Object CrossThreadGetSelectedItemDelegate(ListBox lbox);

        protected Object CrossThreadGetSelectedItem(ListBox lbox)
        {
            if (this.InvokeRequired)
            {
                CrossThreadGetSelectedItemDelegate d = this.CrossThreadGetSelectedItem;
                return this.Invoke(d, new Object[] { lbox });
            }
            return lbox.SelectedItem;
        }

        private delegate int CrossThreadGetSelectedIndexDelegate(ListControl c);

        protected int CrossThreadGetSelectedIndex(ListControl c)
        {
            if (this.InvokeRequired)
            {
                CrossThreadGetSelectedIndexDelegate d = this.CrossThreadGetSelectedIndex;
                return (int)this.Invoke(d, new Object[] { c });
            }
            return c.SelectedIndex;
        }

        #endregion

        #region Cross Thread Set Enabled

        private delegate void CrossThreadSetEnabledDelegate(Control c, bool enabled);

        protected void CrossThreadSetEnabled(Control c, bool enabled)
        {
            if (this.InvokeRequired)
            {
                CrossThreadSetEnabledDelegate d = this.CrossThreadSetEnabled;
                this.Invoke(d, new Object[] { c, enabled });
            }
            else
            {
                c.Enabled = enabled;
            }
        }

        #endregion

        #region Cross Thread Progress Bars

        private delegate void CrossThreadUpdateProgressDelegate(ProgressBar prg, int value);

        protected void CrossThreadUpdateProgress(ProgressBar prg, int value)
        {
            if (this.InvokeRequired)
            {
                CrossThreadUpdateProgressDelegate d = this.CrossThreadUpdateProgress;
                this.Invoke(d, new Object[] { prg, value });
            }
            else
            {
                prg.Value = value;
            }
        }

        private delegate void CrossThreadSetProgressMaximumDelegate(ProgressBar prg, int value);

        protected void CrossThreadSetProgressMaximum(ProgressBar prg, int value)
        {
            if (this.InvokeRequired)
            {
                CrossThreadSetProgressMaximumDelegate d = this.CrossThreadSetProgressMaximum;
                this.Invoke(d, new Object[] { prg, value });
            }
            else
            {
                prg.Maximum = value;
            }
        }

        #endregion

        #region Cross Thread Form Management


        private delegate void CrossThreadSetQueryDelegate(
           RichTextBox txtSparqlQuery,
           string newQuery);

        protected void CrossThreadSetQuery(
            RichTextBox txtSparqlQuery,
            string newQuery)
        {
            if (this.InvokeRequired)
            {
                CrossThreadSetQueryDelegate d = new CrossThreadSetQueryDelegate(this.CrossThreadSetQuery);
                this.Invoke(d, new Object[] { txtSparqlQuery, newQuery});
            }
            else
            {
                txtSparqlQuery.Text = newQuery;
            }
        }

        private delegate void CrossThreadLoadResultSetDelegate(
            ResultSetViewerControl resultSetViewerControl,
            SparqlResultSet results,
            INamespaceMapper nsmap,
            UserControl controlToHide);

        protected void CrossThreadSetResultSet(
            ResultSetViewerControl resultSetViewerControl,
            SparqlResultSet results,
            INamespaceMapper nsmap,
            UserControl controlToHide)
        {
            if (this.InvokeRequired)
            {
                CrossThreadLoadResultSetDelegate d = new CrossThreadLoadResultSetDelegate(this.CrossThreadSetResultSet);
                this.Invoke(d, new Object[] { resultSetViewerControl, results, nsmap, controlToHide });
            }
            else
            {
                controlToHide.Hide();
                resultSetViewerControl.Show();
                resultSetViewerControl.Dock = DockStyle.Fill;
                resultSetViewerControl.DisplayResultSet(results, nsmap);
            }
        }

        private delegate void CrossThreadLoadGraphDelegate(
            GraphViewerControl graphViewerControl, 
            IGraph graph,
            UserControl controlToHide);

        protected void CrossThreadSetResultGraph(GraphViewerControl graphViewerControl, IGraph results, UserControl controlToHide)
        {
            if (this.InvokeRequired)
            {
                CrossThreadLoadGraphDelegate d = new CrossThreadLoadGraphDelegate(this.CrossThreadSetResultGraph);
                this.Invoke(d, new Object[] { graphViewerControl, results, controlToHide });
            }
            else
            {
                controlToHide.Hide();
                graphViewerControl.Show();
                graphViewerControl.Dock = DockStyle.Fill;
                graphViewerControl.DisplayGraph(results);
            }
        }

        private delegate void CrossThreadSetMdiParentDelegate(Form f);

        /// <summary>
        /// Sets the MDI Parent of the given Form to be same as this forms MDI Parent
        /// </summary>
        /// <param name="f">Form</param>
        protected void CrossThreadSetMdiParent(Form f)
        {
            if (this.InvokeRequired)
            {
                CrossThreadSetMdiParentDelegate d = this.CrossThreadSetMdiParent;
                this.Invoke(d, new Object[] { f });
            }
            else
            {
                f.MdiParent = this.MdiParent;
            }
        }

        private delegate void CrossThreadSetMdiParentDelegate2(Form f, Form parent);

        /// <summary>
        /// Sets the MDI Parent of the given form to be the given parent
        /// </summary>
        /// <param name="f">Form</param>
        /// <param name="parent">Parent</param>
        protected void CrossThreadSetMdiParent(Form f, Form parent)
        {
            if (this.InvokeRequired)
            {
                CrossThreadSetMdiParentDelegate2 d = this.CrossThreadSetMdiParent;
                this.Invoke(d, new object[] {f, parent});
            }
            else
            {
                f.MdiParent = parent;
            }
        }

        private delegate void CrossThreadShowQueryPanelDelegate(SplitContainer splitContainer);

        protected void CrossThreadShowQueryPanel(SplitContainer splitContainer)
        {
            if (this.InvokeRequired)
            {
                CrossThreadShowQueryPanelDelegate d = new CrossThreadShowQueryPanelDelegate(this.CrossThreadShowQueryPanel);
                this.Invoke(d, new Object[] { splitContainer });
            }
            else
            {

                splitContainer.Panel2Collapsed = false;
            }
        }



        private delegate void CrossThreadShowDelegate(Form f);

        protected void CrossThreadShow(Form f)
        {
            if (this.InvokeRequired)
            {
                CrossThreadShowDelegate d = this.CrossThreadShow;
                this.Invoke(d, new Object[] { f });
            }
            else
            {

                f.Show();
            }
        }

        private delegate void CrossThreadCloseDelegate(Form f);

        protected void CrossThreadClose(Form f)
        {
            if (this.InvokeRequired)
            {
                CrossThreadCloseDelegate d = this.CrossThreadClose;
                this.Invoke(d, new Object[] { f });
            }
            else
            {
                f.Close();
            }
        }

        protected void CrossThreadClose()
        {
            this.CrossThreadClose(this);
        }

        #endregion
    }
}
