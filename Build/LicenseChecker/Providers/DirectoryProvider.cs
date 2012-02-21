using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LicenseChecker.Providers
{
    class DirectoryProvider
        : ISourceProvider
    {
        private String _dir;
        private bool _recurse = true;

        public DirectoryProvider(String dir, bool recurse)
        {
            this._dir = dir;
            this._recurse = recurse;
        }

        public IEnumerable<string> GetSourceFiles()
        {
            return this.GetSourceFiles(this._dir, this._recurse);
        }

        private IEnumerable<String> GetSourceFiles(String dir, bool recurse)
        {
            if (Directory.Exists(this._dir))
            {
                IEnumerable<String> fs = Directory.GetFiles(dir).Select(f => Path.GetFullPath(f));
                if (recurse)
                {
                    fs = fs.Concat(from d in Directory.GetDirectories(dir)
                                   from f in this.GetSourceFiles(d, recurse)
                                   select Path.GetFullPath(f));
                }
                return fs;
            }
            else
            {
                return Enumerable.Empty<String>();
            }
        }
    }
}
