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
using VDS.RDF.Writing.Formatting;

namespace soh
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                ShowUsage();
            }
            else
            {
                Dictionary<String, String> arguments = ParseArguments(args.Skip(1).ToArray());

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
        }

        static void DoQuery(Dictionary<String, String> arguments)
        {
            SparqlRemoteEndpoint endpoint;
            bool verbose = arguments.ContainsKey("verbose") || arguments.ContainsKey("v");

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
            Console.Error.WriteLine("soh: Error: Not yet implemented");
        }

        static void DoProtocol(Dictionary<String, String> arguments)
        {
            Console.Error.WriteLine("soh: Error: Not yet implemented");
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
            Console.WriteLine("-v, --verbose");
            Console.WriteLine("  Verbose Mode");
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
            Console.WriteLine("--version");
            Console.WriteLine("  Print version and exit");
            Console.WriteLine("--h, --help");
            Console.WriteLine("  Display this screen and exit");
            Console.WriteLine();
            Console.WriteLine("Options for SPARQL Uniform HTTP Protocol");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine();
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
            Console.WriteLine("All supported options should be equivalent to using the soh scripts provided by Fuseki");
            Console.WriteLine("Where a Fuseki soh script option is not present it is not supported");
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
                            if (!args[i+1].StartsWith("-") && !IsSimpleArgument(args[i+1]))
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
            switch (arg.ToLower())
            {
                case "-v":
                case "--verbose":
                case "-h":
                case "--help":
                case "--version":
                    return true;
                default:
                    return false;
            }
        }
    }
}
