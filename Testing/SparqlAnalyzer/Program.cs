using System;
using CommandLine;
using VDS.RDF.Parsing;

namespace SparqlAnalyzer;

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
        var parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
        var query = opts.Query != null ? 
            parser.ParseFromString(opts.Query) : 
            parser.ParseFromFile(opts.File);
        var algebra = query.ToAlgebra(false);
        Console.WriteLine(query.ToString());
        Console.WriteLine(algebra.ToString());
    }

}
