using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace SparqlAnalyzer
{
    class Options
    {
        [Option('q', "query")]
        public string Query { get; set; }

        [Option('f', "file")]
        public string File { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args).WithParsed(opts => RunSparqlAnalyzer(opts));
        }

        private static void RunSparqlAnalyzer(Options opts)
        {
            VDS.RDF.Options.AlgebraOptimisation = false;
            var parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
            var query = opts.Query != null ? 
                parser.ParseFromString(opts.Query) : 
                parser.ParseFromFile(opts.File);
            var algebra = query.ToAlgebra();
            Console.WriteLine(query.ToString());
            Console.WriteLine(algebra.ToString());
        }

    }

}
