/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

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

namespace VDS.Web.Configuration
{
    /// <summary>
    /// Manager for Virtual Directoreis
    /// </summary>
    /// <remarks>
    /// Supports nested virtual paths, will always try to match the longest path first.  For example if you have /path/ and /path/other/ a request to /path/other/index.html would map to the latter rather than the former even if the former had a physical directory called other in it
    /// </remarks>
    public class VirtualDirectoryManager
    {
        private SortedList<String, VirtualDirectoryMapping> _mappings = new SortedList<string,VirtualDirectoryMapping>(new VirtualDirectoryComparer());

        /// <summary>
        /// Adds a virtual directory
        /// </summary>
        /// <param name="path">Virtual Path</param>
        /// <param name="directory">Physical Path</param>
        public void AddVirtualDirectory(String path, String directory)
        {
            this.AddVirtualDirectory(new VirtualDirectoryMapping(path, directory));
        }

        /// <summary>
        /// Adds a virtual directory
        /// </summary>
        /// <param name="mapping">Mapping</param>
        public void AddVirtualDirectory(VirtualDirectoryMapping mapping)
        {
            this._mappings.Add(mapping.Path, mapping);
        }

        /// <summary>
        /// Removes a virtual directory
        /// </summary>
        /// <param name="path">Virtual Path</param>
        public void RemoveVirtualDirectory(String path)
        {
            this._mappings.Remove(path);
        }

        /// <summary>
        /// Removes a virtual directory
        /// </summary>
        /// <param name="mapping">Mapping</param>
        public void RemoveVirtualDirectory(VirtualDirectoryMapping mapping)
        {
            this._mappings.Remove(mapping.Path);
        }

        /// <summary>
        /// Tries to get a directory for the specified path returning false if no directory exists
        /// </summary>
        /// <param name="path">Virtual Path</param>
        /// <param name="modPath">The path relative to the virtual directory</param>
        /// <returns></returns>
        public String GetDirectory(String path, out String modPath)
        {
            modPath = null;
            foreach (VirtualDirectoryMapping mapping in this._mappings.Values)
            {
                if (path.StartsWith(mapping.Path))
                {
                    modPath = path.Substring(mapping.Path.Length);
                    return mapping.Directory;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Comparer for virtual directories which ensure the longest paths appear first in the sort order
    /// </summary>
    class VirtualDirectoryComparer
        : IComparer<String>
    {
        /// <summary>
        /// Compares two paths based on length
        /// </summary>
        /// <param name="x">Path 1</param>
        /// <param name="y">Path 2</param>
        /// <returns></returns>
        public int Compare(string x, string y)
        {
            return x.Length.CompareTo(y.Length);
        }
    }
}
