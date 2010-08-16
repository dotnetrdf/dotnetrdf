using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    public static class BackgroundPersistedGraphTest
    {
        public static void Main(string[] args)
        {
            for (int i = 0; i < 100; i++)
            {
                WriteOnlyStoreGraph g = new WriteOnlyStoreGraph(new Uri("http://example.org"), new MockGenericManager());
                FileLoader.Load(g, "InferenceTest.ttl");
                g.Flush();
                g.Dispose();
                g.Dispose();
                Debug.WriteLine("Run " + i + "/100 Done");
            }
        }
    }
}
