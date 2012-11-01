/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Net;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.Query
{
    public class RoqetQuery
    {
        private bool _count = false;
        private bool _dump = false;
        private String _dumpFormat = String.Empty;
        private bool _dryrun = false;
        private bool _quiet = false;
        private bool _walk = false;
        private ISparqlResultsWriter _resultsWriter = new SparqlXmlWriter();
        private IRdfWriter _graphWriter = new NTriplesWriter();
        private String _inputUri = String.Empty;
        private String _queryString = String.Empty;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private WebDemandTripleStore _store = new WebDemandTripleStore();
        private List<String> _namedGraphs = new List<string>();
        
        public void RunQuery(String[] args)
        {
            if (!this.SetOptions(args))
            {
                //Abort if we can't set options properly
                return;
            }

            //If no input URI/Query specified exit
            if (this._inputUri.Equals(String.Empty) && this._queryString.Equals(String.Empty))
            {
                Console.Error.WriteLine("rdfQuery: No Query Input URI of -e QUERY option was specified so nothing to do");
                return;
            }
            else if (!this._inputUri.Equals(String.Empty))
            {
                //Try and load the query from the File/URI
                try
                {
                    Uri u = new Uri(this._inputUri);
                    if (u.IsAbsoluteUri)
                    {
                        using (StreamReader reader = new StreamReader(HttpWebRequest.Create(this._inputUri).GetResponse().GetResponseStream()))
                        {
                            this._queryString = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        using (StreamReader reader = new StreamReader(this._inputUri))
                        {
                            this._queryString = reader.ReadToEnd();
                        }
                    }
                }
                catch (UriFormatException)
                {
                    using (StreamReader reader = new StreamReader(this._inputUri))
                    {
                        this._queryString = reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("rdfQuery: Error: Unable to read the query from the URI '" + this._inputUri + "' due to the following error:");
                    Console.Error.WriteLine("rdfQuery: Error: " + ex.Message);
                    return;
                }
            }

            //Try to parse the query
            SparqlQuery q;
            try
            {
                q = this._parser.ParseFromString(this._queryString);
            }
            catch (RdfParseException parseEx)
            {
                Console.Error.WriteLine("rdfQuery: Parser Error: Unable to parse the query due to the following error:");
                Console.Error.WriteLine("rdfQuery: Parser Error: " + parseEx.Message);
                return;
            }
            catch (RdfQueryException queryEx)
            {
                Console.Error.WriteLine("rdfQuery: Query Error: Unable to read the query due to the following error:");
                Console.Error.WriteLine("rdfQuery: Query Error: " + queryEx.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfQuery: Error: Unable to read the query due to the following error:");
                Console.Error.WriteLine("rdfQuery: Error: " + ex.Message);
                return;
            }

            //If dumping/dry-running/walking then just print the appropriate debug stuff and exit
            if (this._dryrun || this._walk || this._dump)
            {
                if (this._dump || this._walk)
                {
                    switch (this._dumpFormat)
                    {
                        case "debug":
                            Console.WriteLine("rdfQuery: Parsed and Optimised Query with explicit nesting where appropriate:");
                            Console.WriteLine();
                            Console.WriteLine(q.ToString());
                            Console.WriteLine();
                            Console.WriteLine("rdfQuery: Algebra Form of Query");
                            Console.WriteLine();
                            Console.WriteLine(q.ToAlgebra().ToString());
                            break;
                        case "sparql":
                            Console.WriteLine(q.ToString());
                            break;
                        case "structure":
                            Console.WriteLine(q.ToAlgebra().ToString());
                            break;
                        default:
                            Console.Error.WriteLine("rdfQuery: Unknown dump format");
                            break;
                    }
                }
                else
                {
                    Console.Error.WriteLine("rdfQuery: Dry run complete - Query OK");
                }
                return;
            }

            //Show number of Graphs and Triples we're querying against
            if (!this._quiet) Console.Error.WriteLine("rdfQuery: Making query against " + this._store.Graphs.Count + " Graphs with " + this._store.Triples.Count() + " Triples (plus " + this._namedGraphs.Count + " named graphs which will be loaded as required)");

            //Now execute the actual query against the store
            //Add additional names graphs to the query
            foreach (String uri in this._namedGraphs)
            {
                try
                {
                    q.AddNamedGraph(new Uri(uri));
                }
                catch (UriFormatException)
                {
                    Console.Error.WriteLine("rdfQuery: Ignoring Named Graph URI '" + uri + "' since it does not appear to be a valid URI");
                }
            }
            try
            {
                Object results = this._store.ExecuteQuery(q);
                if (results is SparqlResultSet)
                {
                    this._resultsWriter.Save((SparqlResultSet)results, Console.Out);
                }
                else if (results is Graph)
                {
                    this._graphWriter.Save((Graph)results, Console.Out);
                }
                else
                {
                    Console.Error.WriteLine("rdfQuery: Unexpected result from query - unable to output result");
                }
            }
            catch (RdfQueryException queryEx)
            {
                Console.Error.WriteLine("rdfQuery: Query Error: Unable to execute query due to the following error:");
                Console.Error.WriteLine("rdfQuery: Query Error: " + queryEx.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfQuery: Error: Unable to execute query due to the following error:");
                Console.Error.WriteLine("rdfQuery: Error: " + ex.Message);
            }
        }

        private bool SetOptions(String[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && (args[0].Equals("-h") || args[0].Equals("--help"))))
            {
                this.ShowUsage();
                return false;
            }

            int i = 0;
            String arg;
            while (i < args.Length)
            {
                arg = args[i];

                if (arg.Equals("-e") || arg.Equals("--exec"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.Error.WriteLine("rdfQuery: Unexpected end of arguments - expected a query string to be executed");
                        return false;
                    }
                    arg = args[i];
                    this._queryString = arg;
                }
                else if (arg.Equals("-i") || arg.Equals("--input"))
                {
                    i++;
                    if (i > args.Length)
                    {
                        Console.Error.WriteLine("rdfQuery: Unexpected end of arguments - expected a query language");
                        return false;
                    }
                    arg = args[i];
                    if (!this.SetLanguage(arg)) return false;
                }
                else if (arg.Equals("-r") || arg.Equals("--results"))
                {
                    i++;
                    if (i > args.Length)
                    {
                        Console.Error.WriteLine("rdfQuery: Unexpected end of arguments - expected a query results format");
                        return false;
                    }
                    arg = args[i];
                    if (!this.SetResultsFormat(arg)) return false;
                }
                else if (arg.Equals("-c") || arg.Equals("--count"))
                {
                    this._count = true;
                }
                else if (arg.Equals("-D") || arg.Equals("--data"))
                {
                    i++;
                    if (i > args.Length)
                    {
                        Console.Error.WriteLine("rdfQuery: Unexpected end of arguments - expected a data file URI");
                        return false;
                    }
                    arg = args[i];
                    if (!this.SetDataUri(arg)) return false;
                }
                else if (arg.Equals("-d") || arg.Equals("--dump-query"))
                {
                    i++;
                    if (i > args.Length)
                    {
                        Console.Error.WriteLine("rdfQuery: Unexpected end of arguments - expected a dump format");
                        return false;
                    }
                    arg = args[i];
                    if (!this.SetDumpFormat(arg)) return false;
                }
                else if (arg.Equals("-f") || arg.Equals("--feature"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.Error.WriteLine("rdfQuery: Unexpected end of arguments - expected a feature setting");
                        return false;
                    }
                    Console.Error.WriteLine("rdfQuery: rdfQuery does not support features - this option will be ignored");
                }
                else if (arg.Equals("-G") || arg.Equals("--named") || arg.Equals("-s") || arg.Equals("--source"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.Error.WriteLine("rdfQuery: Unexpected end of arguments - expected a named graph URI");
                        return false;
                    }
                    arg = args[i];
                    if (!this.SetNamedUri(arg)) return false;
                }
                else if (arg.Equals("-help") || arg.Equals("--help"))
                {
                    //Ignore when other arguments are specified
                }
                else if (arg.Equals("-n") || arg.Equals("--dryrun"))
                {
                    this._dryrun = true;
                }
                else if (arg.Equals("-q") || arg.Equals("--quiet"))
                {
                    this._quiet = true;
                }
                else if (arg.Equals("-v") || arg.Equals("--version"))
                {
                    Console.WriteLine("dotNetRDF Version " + Assembly.GetAssembly(typeof(Triple)).GetName().Version.ToString());
                    Console.WriteLine("Copyright Rob Vesse 2009-10");
                    Console.WriteLine("http://www.dotnetrdf.org");
                    return false;
                }
                else if (arg.Equals("-w") || arg.Equals("--walk-query"))
                {
                    this._walk = true;
                    if (this._dumpFormat.Equals(String.Empty))
                    {
                        this._dumpFormat = "debug";
                    }
                }
                else if (arg.StartsWith("-") && !arg.Equals("-"))
                {
                    Console.Error.WriteLine("rdfQuery: Unexpected argument '" + arg + "' encountered - this does not appear to be a valid roqet mode option");
                    return false;
                }
                else
                {
                    //Assume this is the Input URI
                    this._inputUri = arg;

                    i++;
                    if (i < args.Length)
                    {
                        //Assume the next thing is the Input Base URI if we haven't had a -I or --input-uri option
                        arg = args[i];
                        if (!this.SetBaseUri(arg)) return false;

                        if (i < args.Length - 1)
                        {
                            Console.Error.WriteLine("rdfConvert: Additional arguments were specified after the Input URI (and Input Base URI if specified) which is not permitted - these arguments have been ignored");
                        }
                    }

                    return true;
                }

                i++;
            }

            return true;
        }

        private bool SetResultsFormat(String format)
        {
            switch (format)
            {
                case "xml":
                    this._resultsWriter = new SparqlXmlWriter();
                    this._graphWriter = new RdfXmlWriter();
                    break;
                case "json":
                    this._resultsWriter = new SparqlJsonWriter();
                    this._graphWriter = new RdfJsonWriter();
                    break;
                case "ntriples":
                    this._graphWriter = new NTriplesWriter();
                    break;
                case "rdfxml":
                    this._graphWriter = new RdfXmlWriter();
                    break;
                case "turtle":
                    this._graphWriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
                    break;
                case "n3":
                    this._graphWriter = new Notation3Writer(WriterCompressionLevel.High);
                    break;
                case "html":
                case "rdfa":
                    this._resultsWriter = new SparqlHtmlWriter();
                    this._graphWriter = new HtmlWriter();
                    break;
                case "csv":
                    this._resultsWriter = new SparqlCsvWriter();
                    this._graphWriter = new CsvWriter();
                    break;
                case "tsv":
                    this._resultsWriter = new SparqlTsvWriter();
                    this._graphWriter = new TsvWriter();
                    break;
                default:
                    Console.Error.WriteLine("rdfQuery: The value '" + format + "' is not a valid Results Format");
                    return false;
            }

            return true;
        }

        private bool SetLanguage(String lang)
        {
            switch (lang)
            {
                case "sparql":
                    //OK
                    return true;
                case "rdql":
                    Console.Error.WriteLine("rdfQuery: dotNetRDF does not support RDQL");
                    return false;
                default:
                    Console.Error.WriteLine("rdfQuery: The value '" + lang + "' is not a valid query language");
                    return false;
            }
        }

        private bool SetDataUri(String uri)
        {
            //No need to load the data if we aren't bothering to execute the query
            if (this._dryrun || this._walk || this._dump) return true;

            IGraph g = this.LoadGraph(uri, false);
            if (g == null)
            {
                return false;
            }
            else
            {
                this._store.Add(g, true);
                return true;
            }
        }

        private bool SetNamedUri(String uri)
        {
            this._namedGraphs.Add(uri);
            return true;
        }

        private bool SetBaseUri(String uri)
        {
            try 
            {
                this._parser.DefaultBaseUri = new Uri(uri);
                return true;
            } 
            catch (UriFormatException) 
            {
                Console.Error.WriteLine("Unable to use the Base URI '" + uri + "' as it is not a valid URI");
                return false;
            }
        }

        private bool SetDumpFormat(String format)
        {
            switch (format)
            {
                case "debug":
                case "structure":
                case "sparql":
                    this._dump = true;
                    this._dumpFormat = format;
                    return true;
                default:
                    Console.Error.WriteLine("rdfQuery: The value '" + format + "' is not a valid dump format");
                    return false;
            }
        }

        private IGraph LoadGraph(String uri, bool fromFile)
        {
            Graph g = new Graph();
            try
            {
                if (fromFile)
                {
                    FileLoader.Load(g, uri);
                }
                else
                {
                    Uri u = new Uri(uri);
                    if (u.IsAbsoluteUri)
                    {
                        UriLoader.Load(g, u);
                    }
                    else
                    {
                        FileLoader.Load(g, uri);
                    }
                }
                return g;
            }
            catch (UriFormatException)
            {
                //Try loading as a file as it's not a valid URI
                return this.LoadGraph(uri, true);
            }
            catch (RdfParseException parseEx)
            {
                Console.Error.WriteLine("rdfQuery: Parser Error: Unable to parse data from URI '" + uri + "' due to the following error:");
                Console.Error.WriteLine("rdfQuery: Parser Error: " + parseEx.Message);
            }
            catch (RdfException rdfEx)
            {
                Console.Error.WriteLine("rdfQuery: RDF Error: Unable to read data from URI '" + uri + "' due to the following error:");
                Console.Error.WriteLine("rdfQuery: RDF Error: " + rdfEx.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfQuery: Error: Unable to read data from URI '" + uri + "' due to the following error:");
                Console.Error.WriteLine("rdfQuery: Error: " + ex.Message);
            }
            return null;
        }

        private void ShowUsage()
        {
            Console.WriteLine("rdfQuery Utility for dotNetRDF");
            Console.WriteLine("--------------------------------");
            Console.WriteLine();
            Console.WriteLine("Running in roqet compatibility mode (first argument was -roqet)");
            Console.WriteLine("Usage is rdfQuery -roqet ROQET_ARGS");
            Console.WriteLine("In roqet compatibility mode ROQET_ARGS must be replaced with arguments in the roqet command line format as described at http://librdf.org/rasqal/roqet.html");
            Console.WriteLine();
            Console.WriteLine("Supported Output Formats");
            Console.WriteLine("------------------------");
            Console.WriteLine();
            Console.WriteLine("The first set of formats provide output for either Result Sets/Graphs and the second set only provide Graph output");
            Console.WriteLine();
            Console.WriteLine("xml          SPARQL XML Results or RDF/XML");
            Console.WriteLine("json         SPARQL JSON Results or RDF/JSON Resource-Centric");
            Console.WriteLine("html         HTML Table of Results or HTML containing RDFa");
            Console.WriteLine("rdfa         HTML Table of Results or HTML containing RDFa");
            Console.WriteLine("csv          Comma Separated Values");
            Console.WriteLine("tsv          Tab Separated Values");
            Console.WriteLine();
            Console.WriteLine("ntriples     NTriples");
            Console.WriteLine("turtle       Turtle");
            Console.WriteLine("n3           Notation 3");
            Console.WriteLine("rdfxml       RDF/XML");
            Console.WriteLine();
            Console.WriteLine("Notes");
            Console.WriteLine("-----");
            Console.WriteLine();
            Console.WriteLine("1. The -f roqet option is currently ignored");
            Console.WriteLine("2. roqet compatibility mode copies the syntax (not the logic) of the Redland roqet tool so it does not make any guarantee that it behaves in the same way as roqet would given a particular input.");

        }
    }
}
