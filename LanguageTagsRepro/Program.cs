using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

var graph = new Graph();

var sourceGraph = @"
    @prefix ex: <http://example.org/> .

    ex:entity ex:has_name ""name without language tag"" .
    ex:entity ex:has_name ""name with language tag""@en .
";

using TextReader reader = new StringReader(sourceGraph);
var parser = new TurtleParser();
parser.Load(graph, reader);

var result = graph.ExecuteQuery(@"
    PREFIX ex: <http://example.org/>

    CONSTRUCT
    {
        ?entity ex:has_name ?name .
        ?entity ex:has_name ?StringWithLanguageTag .
        ?entity ex:has_name ?StringWithoutLanguageTag .
    }
    WHERE
    {
        ?entity ex:has_name ?name .

        BIND(STRLANG(""string with language tag"", ""en"") as ?StringWithLanguageTag) .
        BIND(""string without language tag"" as ?StringWithoutLanguageTag) .
    }
") as IGraph;

var writer = new CompressingTurtleWriter();
var serialized = VDS.RDF.Writing.StringWriter.Write(result, writer);

Console.WriteLine(serialized);
