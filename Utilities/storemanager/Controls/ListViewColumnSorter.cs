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
using System.Collections;
using System.Windows.Forms;

namespace VDS.RDF.Utilities.StoreManager.Controls
{
    /// <summary>
    /// A column sorter for list views
    /// </summary>
    public class ListViewColumnSorter : IComparer
    {
        private readonly int _column;
        private readonly int _modifier = 1;

        public ListViewColumnSorter(int column, SortOrder order)
        {
            this._column = column;
            if (order == SortOrder.Descending)
            {
                this._modifier = -1;
            }
        }


        public int Compare(object x, object y)
        {
            if (!(x is ListViewItem) || !(y is ListViewItem)) return 0;
            ListViewItem a = (ListViewItem) x;
            ListViewItem b = (ListViewItem) y;

            int numA, numB;
            if (Int32.TryParse(a.SubItems[this._column].Text, out numA) && Int32.TryParse(b.SubItems[this._column].Text, out numB)) return this._modifier*numA.CompareTo(numB);
            return this._modifier*String.Compare(a.SubItems[this._column].Text, b.SubItems[this._column].Text, StringComparison.CurrentCulture);
        }
    }
}
