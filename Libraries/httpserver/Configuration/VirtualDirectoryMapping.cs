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

namespace VDS.Web.Configuration
{
    /// <summary>
    /// Represents a Virtual Directory mapping
    /// </summary>
    public class VirtualDirectoryMapping
    {
        private String _path, _dir;

        /// <summary>
        /// Creates a Virtual Directory mapping
        /// </summary>
        /// <param name="path">Virtual Path</param>
        /// <param name="directory">Physical Path</param>
        /// <remarks>
        /// Enforces a number of constraints on the virtual and physical paths:
        /// <ul>
        ///     <li>Virtual Path cannot be null</li>
        ///     <li>Virtual Paths must start and end with the / character</li>
        ///     <li>You cannot make the root path (the single / character) a virtual directory</li>
        ///     <li>Physical Path cannot be null</li>
        ///     <li>Physical Path must be a directory that exists</li>
        /// </ul>
        /// </remarks>
        public VirtualDirectoryMapping(String path, String directory)
        {
            if (path == null) throw new ArgumentNullException("path", "Virtual Directory Path cannot be null");
            if (!path.StartsWith("/")) throw new ArgumentException("Virtual Directory Paths must start with a /", "path");
            if (!path.EndsWith("/")) throw new ArgumentException("Virtual Directory Paths must end with a /", "path");
            if (path.Equals("/")) throw new ArgumentException("Cannot use the path / as a Virtual Directory", "path");

            if (directory == null) throw new ArgumentNullException("directory", "Directory cannot be null");
            if (!System.IO.Directory.Exists(directory)) throw new ArgumentException("Directory must be an existing directory", "directory");
            if (!directory.EndsWith(new String(new char[] { System.IO.Path.DirectorySeparatorChar }))) directory += System.IO.Path.DirectorySeparatorChar;

            this._path = path;
            this._dir = directory;
        }

        /// <summary>
        /// Gets the Virtual Path
        /// </summary>
        public String Path
        {
            get
            {
                return this._path;
            }
        }

        /// <summary>
        /// Gets the Physical Path
        /// </summary>
        public String Directory
        {
            get
            {
                return this._dir;
            }
        }
    }


}
