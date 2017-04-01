using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing
{
    public static class FileLoader
    {
        public static void Load(IGraph g, string fileName) {
            g.LoadFromFile(fileName);
        }
    }
}
