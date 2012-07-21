/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections;
using System.Windows.Forms;

namespace VDS.RDF.Utilities.StoreManager
{
    public class ListViewColumnSorter : IComparer
    {
        private int _column = 0;
        private int _modifier = 1;

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
            if (x is ListViewItem && y is ListViewItem)
            {
                ListViewItem a = (ListViewItem)x;
                ListViewItem b = (ListViewItem)y;

                int numA, numB;
                if (Int32.TryParse(a.SubItems[this._column].Text, out numA) && Int32.TryParse(b.SubItems[this._column].Text, out numB))
                {
                    return this._modifier * numA.CompareTo(numB);
                }
                else
                {
                    return this._modifier * a.SubItems[this._column].Text.CompareTo(b.SubItems[this._column].Text);
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
