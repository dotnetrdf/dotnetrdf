using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.OptimiserStats
{
    class Program
    {
        static void Main(string[] args)
        {
            RdfOptimiserStats stats = new RdfOptimiserStats(args);
            stats.Run();
        }

    }
}
