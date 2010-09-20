using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*
 * -i -n http://purl.org/ontology/mo/ -o ..\..\out.cs -m ..\..\meta.xml
 * -e:http://DBpedia.org/sparql -i -n http://purl.org/ontology/mo/ -o ..\..\out.cs -m ..\..\meta.xml
 * 
 * -i -n http://purl.org/ontology/mo/ -o "C:\shared.datastore\repository\personal\dev\projects\semantic-web\LinqToRdf.Prototypes\TestHarness\mo.cs" -m "C:\shared.datastore\repository\personal\dev\projects\semantic-web\LinqToRdf.Prototypes\TestHarness\meta.xml"
 * */
namespace rdfMetal
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Options opts = ProcessOptions(args);
            IEnumerable<OntologyClass> classes = null;

            if (!string.IsNullOrEmpty(opts.endpoint) || !string.IsNullOrEmpty(opts.sourceFile))
            {
                var mr = new MetadataRetriever(opts);
                classes = new List<OntologyClass>(mr.GetClasses());
            }

            if (!string.IsNullOrEmpty(opts.metadataLocation) && classes != null)
            {
                var mw = new ModelWriter();
                mw.Write(opts.metadataLocation, classes);
            }

            if (classes == null && !string.IsNullOrEmpty(opts.metadataLocation))
            {
                var mw = new ModelWriter();
                classes = mw.Read(opts.metadataLocation);
            }

            if (!string.IsNullOrEmpty(opts.sourceLocation) && classes != null)
            {
                var cg = new CodeGenerator();
                string code = cg.Generate(classes, opts);
                WriteSource(opts.sourceLocation, code);
            }
	         Console.WriteLine("done.");
             Console.Read();
        }


        private static void WriteSource(string path, string code)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(path, false);
                sw.Write(code);
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }

        private static Options ProcessOptions(string[] args)
        {
            var opts = new Options();
            opts.ProcessArgs(args);
            return opts;
        }

    }
}
