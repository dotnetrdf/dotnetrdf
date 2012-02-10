using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.Web.Configuration
{
    public class VirtualDirectoryMapping
    {
        private String _path, _dir;

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

        public String Path
        {
            get
            {
                return this._path;
            }
        }

        public String Directory
        {
            get
            {
                return this._dir;
            }
        }
    }


}
