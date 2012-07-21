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
