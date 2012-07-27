using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Utilities.SparqlOverHttp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length < 1)
            {
                ShowUsage();
            }
            else
            {
                Dictionary<String, String> arguments = ParseArguments(args.Skip(1).ToArray());
                if (arguments.ContainsKey("nobom")) Options.UseBomForUtf8 = false;

                try
                {
                    switch (args[0].ToLower())
                    {
                        case "query":
                            DoQuery(arguments);
                            break;
                        case "update":
                            DoUpdate(arguments);
                            break;
                        case "protocol":
                            DoProtocol(arguments);
                            break;

                        default:
                            if (arguments.ContainsKey("help") || arguments.ContainsKey("h"))
                            {
                                ShowUsage();
                            }
                            else if (arguments.ContainsKey("version"))
                            {
                                ShowVersion();
                            }
                            else
                            {
                                Console.Error.WriteLine("soh: Error: First argument must be one of query, update or protocol - Type soh --help for details");
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("soh: Error: Unexpected Error occurred");
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine(ex.StackTrace);
                    Environment.Exit(-1);
                    return;
                }
            }
        }

        static void DoQuery(Dictionary<String, String> arguments)
        {
            SparqlRemoteEndpoint endpoint;
            bool verbose = arguments.ContainsKey("verbose") || arguments.ContainsKey("v");
            if (verbose) Options.HttpDebugging = true;

            //First get the Server to which we are going to connect
            try
            {
                if (arguments.ContainsKey("server") && !arguments["server"].Equals(String.Empty))
                {
                    endpoint = new SparqlRemoteEndpoint(new Uri(arguments["server"]));
                }
                else if (arguments.ContainsKey("service") && !arguments["service"].Equals(String.Empty))
                {
                    endpoint = new SparqlRemoteEndpoint(new Uri(arguments["service"]));
                }
                else
                {
                    Console.Error.WriteLine("soh: Error: Required --server/--service argument not present");
                    Environment.Exit(-1);
                    return;
                }
            }
            catch (UriFormatException uriEx)
            {
                Console.Error.WriteLine("soh: Error: Malformed SPARQL Endpoint URI");
                Console.Error.WriteLine(uriEx.Message);
                Environment.Exit(-1);
                return;
            }
            if (verbose) Console.Error.WriteLine("soh: SPARQL Endpoint for URI " + endpoint.Uri + " created OK");

            //Then decide where to get the query to execute from
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery query;
            try
            {
                if (arguments.ContainsKey("query") && !arguments["query"].Equals(String.Empty))
                {
                    query = parser.ParseFromFile(arguments["query"]);
                }
                else if (arguments.ContainsKey("file") && !arguments["file"].Equals(String.Empty))
                {
                    query = parser.ParseFromFile(arguments["file"]);
                }
                else if (arguments.ContainsKey("$1") && !arguments["$1"].Equals(String.Empty))
                {
                    query = parser.ParseFromString(arguments["$1"]);
                }
                else
                {
                    Console.Error.WriteLine("soh: Error: Required SPARQL Query not found - may be specified as --file/--query FILE or as final argument");
                    Environment.Exit(-1);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("soh: Error: Error Parsing SPARQL Query");
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-1);
                return;
            }

            if (verbose)
            {
                Console.Error.WriteLine("soh: Parsed Query OK");
                Console.Error.WriteLine("soh: dotNetRDF's interpretation of the Query:");
                SparqlFormatter formatter = new SparqlFormatter();
                Console.Error.WriteLine(formatter.Format(query));
                Console.Error.WriteLine("soh: Submitting Query");
            }

            try
            {
                using (HttpWebResponse response = endpoint.QueryRaw(query.ToString()))
                {
                    MimeTypeDefinition definition = MimeTypesHelper.GetDefinitions(response.ContentType).FirstOrDefault();
                    Encoding enc;
                    if (definition != null)
                    {
                        enc = definition.Encoding;
                    }
                    else if (!response.ContentEncoding.Equals(String.Empty))
                    {
                        enc = Encoding.GetEncoding(response.ContentEncoding);
                    }
                    else if (response.ContentType.Contains("charset="))
                    {
                        enc = Encoding.GetEncoding(response.ContentType.Substring(response.ContentType.IndexOf('=') + 1));
                    }
                    else
                    {
                        enc = Console.OutputEncoding;
                    }

                    if (verbose)
                    {
                        Console.Error.WriteLine("soh: Got Response from SPARQL Endpoint OK");
                        Console.Error.WriteLine("soh: Content-Type: " + response.ContentType);
                        Console.Error.WriteLine("soh: Content-Encoding: " + enc.WebName);
                    }

                    String requestedType = arguments.ContainsKey("accept") ? arguments["accept"] : null;

                    if (requestedType == null || response.ContentType.StartsWith(requestedType, StringComparison.OrdinalIgnoreCase))
                    {
                        //If no --accept (OR matches servers content type) then just return whatever the server has given us
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            using (StreamWriter writer = new StreamWriter(Console.OpenStandardOutput(), enc))
                            {
                                while (!reader.EndOfStream)
                                {
                                    writer.WriteLine(reader.ReadLine());
                                }
                                writer.Close();
                            }
                            reader.Close();
                        }
                    }
                    else
                    {
                        if (verbose) Console.Error.WriteLine("soh: Warning: Retrieved Content Type '" + response.ContentType + "' does not match your desired Content Type '" + requestedType + "' so dotNetRDF will not attempt to transcode the response into your desired format");

                        //Requested Type Doesn't match servers returned type so parse then serialize
                        MimeTypeDefinition outputDefinition;
                        try
                        {
                            ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                            SparqlResultSet results = new SparqlResultSet();
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                sparqlParser.Load(results, reader);
                                reader.Close();
                            }

                            outputDefinition = MimeTypesHelper.GetDefinitions(requestedType).FirstOrDefault(d => d.CanWriteSparqlResults || d.CanWriteRdf);
                            if (outputDefinition == null) throw new RdfWriterSelectionException("No MIME Type Definition for the MIME Type '" + requestedType + "' was found that can write SPARQL Results/RDF");
                            ISparqlResultsWriter writer = outputDefinition.GetSparqlResultsWriter();
                            Console.OutputEncoding = outputDefinition.Encoding;
                            writer.Save(results, new StreamWriter(Console.OpenStandardOutput(), outputDefinition.Encoding));
                        }
                        catch (RdfParserSelectionException)
                        {
                            try 
                            {
                                IRdfReader rdfParser = MimeTypesHelper.GetParser(response.ContentType);
                                Graph g = new Graph();
                                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                {
                                    rdfParser.Load(g, reader);
                                    reader.Close();
                                }

                                outputDefinition = MimeTypesHelper.GetDefinitions(requestedType).FirstOrDefault(d => d.CanWriteRdf);
                                if (outputDefinition == null) throw new RdfWriterSelectionException("No MIME Type Definition for the MIME Type '" + requestedType + "' was found that can write RDF");
                                IRdfWriter writer = outputDefinition.GetRdfWriter();
                                Console.OutputEncoding = outputDefinition.Encoding;
                                writer.Save(g, new StreamWriter(Console.OpenStandardOutput(), outputDefinition.Encoding));

                            }
                            catch (Exception ex)
                            {
                                //For any other exception show a warning
                                Console.Error.WriteLine("soh: Warning: You wanted results in the format '" + requestedType + "' but the server returned '" + response.ContentType + "' and dotNetRDF was unable to translate the response into your desired format.");
                                Console.Error.WriteLine(ex.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            //For any other exception show a warning
                            Console.Error.WriteLine("soh: Warning: You wanted results in the format '" + requestedType + "' but the server returned '" + response.ContentType + "' and dotNetRDF was unable to translate the response into your desired format.");
                            Console.Error.WriteLine(ex.Message);
                        }
                    }
                    

                    response.Close();
                }

                if (verbose) Console.Error.WriteLine("soh: Query Completed OK");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("soh: Error: Error while making the SPARQL Query");
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-1);
                return;
            }
        }

        static void DoUpdate(Dictionary<String, String> arguments)
        {
            SparqlRemoteUpdateEndpoint endpoint;
            bool verbose = arguments.ContainsKey("verbose") || arguments.ContainsKey("v");
            if (verbose) Options.HttpDebugging = true;

            //First get the Server to which we are going to connect
            try
            {
                if (arguments.ContainsKey("server") && !arguments["server"].Equals(String.Empty))
                {
                    endpoint = new SparqlRemoteUpdateEndpoint(new Uri(arguments["server"]));
                }
                else if (arguments.ContainsKey("service") && !arguments["service"].Equals(String.Empty))
                {
                    endpoint = new SparqlRemoteUpdateEndpoint(new Uri(arguments["service"]));
                }
                else
                {
                    Console.Error.WriteLine("soh: Error: Required --server/--service argument not present");
                    Environment.Exit(-1);
                    return;
                }
            }
            catch (UriFormatException uriEx)
            {
                Console.Error.WriteLine("soh: Error: Malformed SPARQL Update Endpoint URI");
                Console.Error.WriteLine(uriEx.Message);
                Environment.Exit(-1);
                return;
            }
            if (verbose) Console.Error.WriteLine("soh: SPARQL Update Endpoint for URI " + endpoint.Uri + " created OK");

            //Then decide where to get the update to execute from
            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds;
            try
            {
                if (arguments.ContainsKey("update") && !arguments["update"].Equals(String.Empty))
                {
                    cmds = parser.ParseFromFile(arguments["update"]);
                }
                else if (arguments.ContainsKey("file") && !arguments["file"].Equals(String.Empty))
                {
                    cmds = parser.ParseFromFile(arguments["file"]);
                }
                else if (arguments.ContainsKey("$1") && !arguments["$1"].Equals(String.Empty))
                {
                    cmds = parser.ParseFromString(arguments["$1"]);
                }
                else
                {
                    Console.Error.WriteLine("soh: Error: Required SPARQL Update not found - may be specified as --file/--update FILE or as final argument");
                    Environment.Exit(-1);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("soh: Error: Error Parsing SPARQL Update");
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-1);
                return;
            }

            if (verbose)
            {
                Console.Error.WriteLine("soh: Parsed Update OK");
                Console.Error.WriteLine("soh: dotNetRDF's interpretation of the Update:");
                Console.Error.WriteLine(cmds.ToString());
                Console.Error.WriteLine("soh: Submitting Update");
            }

            try
            {
                endpoint.Update(cmds.ToString());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("soh: Error: Error while making the SPARQL Update");
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-1);
                return;
            }
        }

        static void DoProtocol(Dictionary<String, String> arguments)
        {
            String method;
            SparqlHttpProtocolConnector endpoint;
            bool verbose = arguments.ContainsKey("verbose") || arguments.ContainsKey("v");
            if (verbose) Options.HttpDebugging = true;
            Options.UriLoaderCaching = !arguments.ContainsKey("nocache");
            String dataFile;

            //First Argument must be HTTP Method
            if (arguments.ContainsKey("$1") && !arguments["$1"].Equals(String.Empty))
            {
                method = arguments["$1"].ToUpper();
            }
            else
            {
                Console.Error.WriteLine("soh: Error: First argument must be one of head, get, put or post - type soh --help for details");
                Environment.Exit(-1);
                return;
            }

            try
            {
                if (arguments.ContainsKey("$2") && !arguments["$2"].Equals(String.Empty))
                {
                    endpoint = new SparqlHttpProtocolConnector(new Uri(arguments["$2"]));
                }
                else
                {
                    Console.Error.WriteLine("soh: Error: Second argument is required and must be the Dataset URI");
                    Environment.Exit(-1);
                    return;
                }
            }
            catch (UriFormatException uriEx)
            {
                Console.Error.WriteLine("soh: Error: Malformed SPARQL Endpoint URI");
                Console.Error.WriteLine(uriEx.Message);
                Environment.Exit(-1);
                return;
            }
            if (verbose) Console.Error.WriteLine("soh: Connection to SPARQL Uniform HTTP Protocol endpoint created OK");

            Uri graphUri;
            try
            {
                if (arguments.ContainsKey("$3") && !arguments["$3"].Equals(String.Empty))
                {
                    if (arguments["$3"].Equals("default"))
                    {
                        graphUri = null;
                        if (verbose) Console.Error.WriteLine("soh: Graph URI for Protocol request is 'default' (indicates the default unnamed graph)");
                    }
                    else
                    {
                        graphUri = new Uri(arguments["$3"]);
                        if (verbose) Console.Error.WriteLine("soh: Graph URI for Protocol request is '" + graphUri.ToString() + "'");
                    }
                }
                else
                {
                    Console.Error.WriteLine("soh: Error: Third argument is required and must be the Graph URI or default to indicate the default unnamed Graph");
                    Environment.Exit(-1);
                    return;
                }
            }
            catch (UriFormatException uriEx)
            {
                Console.Error.WriteLine("soh: Error: Malformed Graph URI");
                Console.Error.WriteLine(uriEx.Message);
                Environment.Exit(-1);
                return;
            }

            try
            {
                switch (method)
                {
                    case "GET":
                        if (arguments.ContainsKey("$4"))
                        {
                            Console.Error.WriteLine("soh: Error: Optional file argument for protocol mode is not permitted with the get method");
                            Environment.Exit(-1);
                            return;
                        }

                        if (verbose) Console.Error.WriteLine("soh: Making a GET request to the Protocol endpoint");
                        Graph g = new Graph();
                        endpoint.LoadGraph(g, graphUri);
                        if (verbose) Console.Error.WriteLine("soh: Received a Graph with " + g.Triples.Count + " Triples");

                        //Use users choice of output format otherwise RDF/XML
                        MimeTypeDefinition definition = null;
                        if (arguments.ContainsKey("accept"))
                        {
                            definition = MimeTypesHelper.GetDefinitions(arguments["accept"]).FirstOrDefault(d => d.CanWriteRdf);
                        }

                        if (definition != null)
                        {
                            Console.OutputEncoding = definition.Encoding;
                            IRdfWriter writer = definition.GetRdfWriter();
                            writer.Save(g, new StreamWriter(Console.OpenStandardOutput(), definition.Encoding));
                        }
                        else
                        {
                            if (arguments.ContainsKey("accept") && verbose) Console.Error.WriteLine("soh: Warning: You wanted output in format '" + arguments["accept"] + "' but dotNetRDF does not support this format so RDF/XML will be returned instead");

                            RdfXmlWriter rdfXmlWriter = new RdfXmlWriter();
                            rdfXmlWriter.Save(g, new StreamWriter(Console.OpenStandardOutput(), Encoding.UTF8));
                        }

                        break;

                    case "HEAD":
                        if (arguments.ContainsKey("$4"))
                        {
                            Console.Error.WriteLine("soh: Error: Optional file argument for protocol mode is not permitted with the head method");
                            Environment.Exit(-1);
                            return;
                        }

                        if (verbose) Console.Error.WriteLine("soh: Making a HEAD request to the Protocol endpoint");

                        bool exists = endpoint.HasGraph(graphUri);
                        Console.WriteLine(exists.ToString().ToLower());
                        break;

                    case "PUT":
                        //Parse in the Graph to be PUT first
                        if (arguments.ContainsKey("$4") && !arguments["$4"].Equals(String.Empty))
                        {
                            dataFile = arguments["$4"];
                        }
                        else
                        {
                            Console.Error.WriteLine("soh: Error: The file argument for protocol mode is required when using the put method");
                            Environment.Exit(-1);
                            return;
                        }
                        Graph toPut = new Graph();
                        FileLoader.Load(toPut, dataFile);
                        toPut.BaseUri = graphUri;

                        if (verbose)
                        {
                            Console.Error.WriteLine("soh: Graph to be uploaded has " + toPut.Triples.Count + " Triples");
                            Console.Error.WriteLine("soh: Making a PUT request to the Protocol endpoint");
                        }

                        endpoint.SaveGraph(toPut);
                        Console.WriteLine("soh: Graph saved to Protocol endpoint OK");
                        break;

                    case "POST":
                        //Parse in the Graph to be PUT first
                        if (arguments.ContainsKey("$4") && !arguments["$4"].Equals(String.Empty))
                        {
                            dataFile = arguments["$4"];
                        }
                        else
                        {
                            Console.Error.WriteLine("soh: Error: The file argument for protocol mode is required when using the post method");
                            Environment.Exit(-1);
                            return;
                        }
                        Graph toPost = new Graph();
                        FileLoader.Load(toPost, dataFile);

                        if (verbose)
                        {
                            Console.Error.WriteLine("soh: Graph to be uploaded has " + toPost.Triples.Count + " Triples");
                            Console.Error.WriteLine("soh: Making a POST request to the Protocol endpoint");
                        }

                        endpoint.UpdateGraph(graphUri, toPost.Triples, null);
                        Console.WriteLine("soh: Graph updated to Protocol endpoint OK");
                        break;

                    case "DELETE":
                        if (arguments.ContainsKey("$4"))
                        {
                            Console.Error.WriteLine("soh: Error: Optional file argument for protocol mode is not permitted with the head method");
                            Environment.Exit(-1);
                            return;
                        }
                        endpoint.DeleteGraph(graphUri);
                        Console.WriteLine("soh: Graph deleted from Protocol endpoint OK");
                        break;

                    default:
                        Console.Error.WriteLine("soh: Error: " + method + " is not a HTTP Method supported by this tool");
                        Environment.Exit(-1);
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("soh: Error: Error processing HTTP Protocol request");
                Console.Error.WriteLine(ex.Message);
                while (ex.InnerException != null)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine(ex.InnerException.Message);
                    Console.Error.WriteLine(ex.InnerException.StackTrace);
                    ex = ex.InnerException;
                }
                Environment.Exit(-1);
                return;
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("SPARQL over HTTP (dotNetRDF Implementation)");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("Usage for SPARQL Query is:");
            Console.WriteLine("soh query [--query QUERY] [--service URI] 'query' > @file");
            Console.WriteLine();
            Console.WriteLine("Usage for SPARQL Update is:");
            Console.WriteLine("soh update [--file=REQUEST] [--service URI] 'request' > @file");
            Console.WriteLine();
            Console.WriteLine("Usage for SPARQL Uniform HTTP Protocol");
            Console.WriteLine("soh protocol [get|post|put|delete] datasetURI graph [file]");
            Console.WriteLine();
            Console.WriteLine("Options for SPARQL Query");
            Console.WriteLine("------------------------");
            Console.WriteLine();
            Console.WriteLine("--service URI, --server=URI");
            Console.WriteLine("  SPARQL Endpoint");
            Console.WriteLine("--query QUERY, --file=FILE");
            Console.WriteLine("  Takes query from a File");
            Console.WriteLine("--accept MIMETYPE");
            Console.WriteLine("  Sets the MIME Type that you want to get the Results as");
            Console.WriteLine("-v, --verbose");
            Console.WriteLine("  Verbose Mode");
            Console.WriteLine("--nobom");
            Console.WriteLine("  Disables the use of the BOM for UTF-8 Output");
            Console.WriteLine("--version");
            Console.WriteLine("  Print version and exit");
            Console.WriteLine("--h, --help");
            Console.WriteLine("  Display this screen and exit");
            Console.WriteLine();
            Console.WriteLine("Options for SPARQL Update");
            Console.WriteLine("------------------------_");
            Console.WriteLine();
            Console.WriteLine("--service URI, --server=URI");
            Console.WriteLine("  SPARQL Endpoint");
            Console.WriteLine("--update UPDATE, --file=FILE");
            Console.WriteLine("  Takes update from a File");
            Console.WriteLine("-v, --verbose");
            Console.WriteLine("  Verbose Mode");
            Console.WriteLine("--nobom");
            Console.WriteLine("  Disables the use of the BOM for UTF-8 Output");
            Console.WriteLine("--version");
            Console.WriteLine("  Print version and exit");
            Console.WriteLine("--h, --help");
            Console.WriteLine("  Display this screen and exit");
            Console.WriteLine();
            Console.WriteLine("Options for SPARQL Uniform HTTP Protocol");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine();
            Console.WriteLine("--accept MIMETYPE");
            Console.WriteLine("  Sets the MIME Type that you want to receive Graphs as for GET requests");
            Console.WriteLine("--nobom");
            Console.WriteLine("  Disables the use of the BOM for UTF-8 Output");
            Console.WriteLine("--nocache");
            Console.WriteLine("  Disables caching of GET requests so that you always see the fresh data from the server");
            Console.WriteLine("-v, --verbose");
            Console.WriteLine("  Verbose Mode");
            Console.WriteLine("--version");
            Console.WriteLine("  Print Version and exit");
            Console.WriteLine("-h, --help");
            Console.WriteLine("  Display this screen and exit");
            Console.WriteLine();
            Console.WriteLine("Note");
            Console.WriteLine("----");
            Console.WriteLine();
            Console.WriteLine("Where a Fuseki soh script option is not present it is not supported");
            Console.WriteLine("All supported options which are the same as Fuseki soh options should be equivalent to using the soh scripts provided by Fuseki");

        }

        static void ShowVersion()
        {
            Console.WriteLine("SPARQL over HTTP (dotNetRDF Implementation)");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("SPARQL over HTTP Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("dotNetRDF Version: " + Assembly.GetAssembly(typeof(IGraph)).GetName().Version.ToString());
        }

        static Dictionary<String, String> ParseArguments(String[] args)
        {
            Dictionary<String, String> arguments = new Dictionary<string, string>();
            int anonArg = 1;
            String arg, name, value;

            for (int i = 0; i < args.Length; i++)
            {
                arg = args[i];

                if (arg.StartsWith("-"))
                {
                    name = arg.Substring(1);
                    if (name[0] == '-') name = name.Substring(1);
                    if (name.Contains("="))
                    {
                        name = name.Substring(0, name.IndexOf('='));
                        value = arg.Substring(arg.IndexOf('=') + 1);
                    }
                    else
                    {
                        if (i < args.Length - 1)
                        {
                            if (IsSimpleArgument(name))
                            {
                                value = String.Empty;
                            }
                            else if (!args[i+1].StartsWith("-"))
                            {
                                value = args[i+1];
                                i++;
                            } 
                            else
                            {
                                value = String.Empty;
                            }
                        }
                        else
                        {
                            value = String.Empty;
                        }
                    }
                    if (!arguments.ContainsKey(name))
                    {
                        arguments.Add(name, value);
                    }
                    else
                    {
                        Console.Error.WriteLine("soh: Error: Duplicate " + name + " argument");
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    //Anonymous argument get named $1, $2 etc
                    arguments.Add("$" + anonArg, arg);
                    anonArg++;
                }
            }

            return arguments;
        }

        static bool IsSimpleArgument(String arg)
        {
            if (arg.StartsWith("--")) arg = arg.Substring(2);
            if (arg.StartsWith("-")) arg = arg.Substring(1);
            switch (arg.ToLower())
            {
                case "v":
                case "verbose":
                case "h":
                case "help":
                case "version":
                case "nocache":
                    return true;
                default:
                    return false;
            }
        }
    }
}
