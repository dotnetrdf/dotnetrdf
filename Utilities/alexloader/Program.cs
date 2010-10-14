using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VDS.Alexandria;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace alexloader
{
    class Program
    {
        static void Main(string[] args)
        {
            AlexandriaLoader loader = new AlexandriaLoader();
            loader.Run(args);
        }

    }
}
