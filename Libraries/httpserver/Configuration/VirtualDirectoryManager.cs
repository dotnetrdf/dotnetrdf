using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Web.Configuration
{
    public class VirtualDirectoryManager
    {
        public SortedList<String, VirtualDirectoryMapping> _mappings = new SortedList<string,VirtualDirectoryMapping>(new VirtualDirectoryComparer());

        public VirtualDirectoryManager()
        {

        }

        public void AddVirtualDirectory(String path, String directory)
        {
            this.AddVirtualDirectory(new VirtualDirectoryMapping(path, directory));
        }

        public void AddVirtualDirectory(VirtualDirectoryMapping mapping)
        {
            this._mappings.Add(mapping.Path, mapping);
        }

        public void RemoveVirtualDirectory(String path)
        {
            this._mappings.Remove(path);
        }

        public void RemoveVirtualDirectory(VirtualDirectoryMapping mapping)
        {
            this._mappings.Remove(mapping.Path);
        }

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

    class VirtualDirectoryComparer : IComparer<String>
    {
        public int Compare(string x, string y)
        {
            return x.Length.CompareTo(y.Length);
        }
    }
}
