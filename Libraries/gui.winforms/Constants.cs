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
using System.Collections.Generic;
using System.Drawing;

namespace VDS.RDF.GUI
{
    /// <summary>
    /// Constants for use in GUI Applications
    /// </summary>
    public static class Constants
    {
        private static Icon _windowIcon = null;

        /// <summary>
        /// Gets/Sets the Window Icon to use
        /// </summary>
        public static Icon WindowIcon
        {
            get
            {
                return _windowIcon;
            }
            set
            {
                _windowIcon = value;
            }
        }
    }

    /// <summary>
    /// Comparer which compares objects based on the value of their ToString() method
    /// </summary>
    /// <typeparam name="T">Type to compare</typeparam>
    class ToStringComparer<T> 
        : IComparer<T>
    {
        /// <summary>
        /// Compares two objects by their ToString() values
        /// </summary>
        /// <param name="x">Object 1</param>
        /// <param name="y">Object 2</param>
        /// <returns></returns>
        public int Compare(T x, T y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null)
            {
                return -1;
            }
            else if (y == null)
            {
                return 1;
            }
            else
            {
                return x.ToString().CompareTo(y.ToString());
            }
        }
    }

}
