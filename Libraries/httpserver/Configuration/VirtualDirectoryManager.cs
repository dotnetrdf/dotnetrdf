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
