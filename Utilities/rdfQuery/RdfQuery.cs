/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Utilities.Query
{
    public enum RdfQueryMode
    {
        Unknown,
        Local,
        Remote
    }

    public class RdfQuery
    {
        private RdfQueryMode _mode = RdfQueryMode.Unknown;
        private IInMemoryQueryableStore _store = new WebDemandTripleStore();
        private SparqlRemoteEndpoint _endpoint;
        private String _output;
        private long _timeout = 0;
        private bool _partialResults = false;
        private ISparqlResultsWriter _resultsWriter = new SparqlXmlWriter();
        private IRdfWriter _graphWriter = new NTriplesWriter();
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private String _query;
        private bool _print = false;
        private bool _debug = false;
        private bool _explain = false;
        private ExplanationLevel _level = (ExplanationLevel.Default | ExplanationLevel.OutputToConsoleStdErr) ^ ExplanationLevel.OutputToConsoleStdOut;

        public void RunQuery(String[] args) 
        {
            //If we can't set options then we abort
            if (!this.SetOptions(args))
            {
                return;
            }

            if (this._query == null)
            {
                //Try to read query from Standard In instead
                this._query = Console.In.ReadToEnd();
                if (this._query.Equals(String.Empty))
                {
                    Console.Error.WriteLine("rdfQuery: No Query was specified");
                    return;
                }
            }

            //Parse the Query
            try
            {
                SparqlQuery q = this._parser.ParseFromString(this._query);

                //Set Timeout if necessary
                q.Timeout = this._timeout;
                q.PartialResultsOnTimeout = this._partialResults;

                //Execute the Query unless print was specified
                Object results = null;
                if (!this._print)
                {
                    switch (this._mode)
                    {
                        case RdfQueryMode.Local:
                            if (this._explain)
                            {
                                ExplainQueryProcessor processor = new ExplainQueryProcessor(this._store, this._level);
                                results = processor.ProcessQuery(q);
                            }
                            else
                            {
                                results = this._store.ExecuteQuery(q);
                            }
                            break;
                        case RdfQueryMode.Remote:
                            if (this._explain) Console.Error.WriteLine("rdfQuery: Warning: Cannot explain queries when the query is being sent to a remote endpoint");
                            this._endpoint.Timeout = Convert.ToInt32(this._timeout);
                            switch (q.QueryType)
                            {
                                case SparqlQueryType.Construct:
                                case SparqlQueryType.Describe:
                                case SparqlQueryType.DescribeAll:
                                    results = this._endpoint.QueryWithResultGraph(this._query);
                                    break;
                                default:
                                    results = this._endpoint.QueryWithResultSet(this._query);
                                    break;
                            }
                            break;
                        case RdfQueryMode.Unknown:
                        default:
                            if (this._explain)
                            {
                                ExplainQueryProcessor processor = new ExplainQueryProcessor(new TripleStore(), this._level);
                                processor.ProcessQuery(q);
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfQuery: There were no inputs or a remote endpoint specified in the arguments so no query can be executed");
                            }
                            return;
                    }
                }

                //Select the Output Stream
                StreamWriter output;
                if (this._output == null)
                {
                    output = new StreamWriter(Console.OpenStandardOutput());
                }
                else
                {
                    output = new StreamWriter(this._output);
                }

                if (!this._print)
                {
                    //Output the Results
                    if (results is SparqlResultSet)
                    {
                        this._resultsWriter.Save((SparqlResultSet)results, output);
                    }
                    else if (results is IGraph)
                    {
                        this._graphWriter.Save((IGraph)results, output);
                    }
                    else
                    {
                        Console.Error.WriteLine("rdfQuery: The Query resulted in an unknown result");
                    }
                }
                else
                {
                    //If Printing Print the Query then the Algebra
                    SparqlFormatter formatter = new SparqlFormatter(q.NamespaceMap);
                    output.WriteLine("Query");
                    output.WriteLine(formatter.Format(q));
                    output.WriteLine();
                    output.WriteLine("Algebra");
                    output.WriteLine(q.ToAlgebra().ToString());
                    output.Flush();
                    output.Close();
                }
            }
            catch (RdfQueryTimeoutException timeout)
            {
                Console.Error.WriteLine("rdfQuery: Query Timeout: " + timeout.Message);
                if (this._debug) this.DebugErrors(timeout);
                return;
            }
            catch (RdfQueryException queryEx)
            {
                Console.Error.WriteLine("rdfQuery: Query Error: " + queryEx.Message);
                if (this._debug) this.DebugErrors(queryEx);
                return;
            }
            catch (RdfParseException parseEx)
            {
                Console.Error.WriteLine("rdfQuery: Parser Error: " + parseEx.Message);
                if (this._debug) this.DebugErrors(parseEx);
                return;
            }
            catch (RdfException rdfEx)
            {
                Console.Error.WriteLine("rdfQuery: RDF Error: " + rdfEx.Message);
                if (this._debug) this.DebugErrors(rdfEx);
                return;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfQuery: Error: " + ex.Message);
                if (this._debug) this.DebugErrors(ex);
                return;
            }
        }

        private bool SetOptions(String[] args)
        {
            if (args.Length == 0 || args.Length == 1 && args[0].Equals("-help"))
            {
                this.ShowUsage();
                return false;
            }

            String arg;
            int i = 0;
            while (i < args.Length)
            {
                arg = args[i];

                if (arg.StartsWith("-uri:"))
                {
                    if (this._mode == RdfQueryMode.Remote)
                    {
                        Console.Error.WriteLine("rdfQuery: Cannot specify input URIs as well as specifying a remote endpoint to query");
                        return false;
                    }

                    String uri = arg.Substring(5);
                    try
                    {
                        this._mode = RdfQueryMode.Local;

                        //Try and parse RDF from the given URI
                        if (!this._print)
                        {
                            Uri u = new Uri(uri);
                            Graph g = new Graph();
                            UriLoader.Load(g, u);
                            this._store.Add(g);
                        }
                        else
                        {
                            Console.Error.WriteLine("rdfQuery: Ignoring the input URI '" + uri + "' since -print has been specified so the query will not be executed so no need to load the data");
                        }
                    }
                    catch (UriFormatException uriEx)
                    {
                        Console.Error.WriteLine("rdfQuery: Ignoring the input URI '" + uri + "' since this is not a valid URI");
                        if (this._debug) this.DebugErrors(uriEx);
                    }
                    catch (RdfParseException parseEx)
                    {
                        Console.Error.WriteLine("rdfQuery: Ignoring the input URI '" + uri + "' due to the following error:");
                        Console.Error.WriteLine("rdfQuery: Parser Error: " + parseEx.Message);
                        if (this._debug) this.DebugErrors(parseEx);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("rdfQuery: Ignoring the input URI '" + uri + "' due to the following error:");
                        Console.Error.WriteLine("rdfQuery: Error: " + ex.Message);
                        if (this._debug) this.DebugErrors(ex);
                    }
                }
                else if (arg.StartsWith("-endpoint:"))
                {
                    if (this._mode == RdfQueryMode.Local)
                    {
                        Console.Error.WriteLine("rdfQuery: Cannot specify a remote endpoint to query as well as specifying local files and/or input URIs");
                        return false;
                    }
                    else if (this._mode == RdfQueryMode.Remote)
                    {
                        if (!(this._endpoint is FederatedSparqlRemoteEndpoint))
                        {
                            this._endpoint = new FederatedSparqlRemoteEndpoint(this._endpoint);
                        }
                    }

                    try
                    {
                        this._mode = RdfQueryMode.Remote;
                        if (this._endpoint is FederatedSparqlRemoteEndpoint)
                        {
                            ((FederatedSparqlRemoteEndpoint)this._endpoint).AddEndpoint(new SparqlRemoteEndpoint(new Uri(arg.Substring(arg.IndexOf(':') + 1))));
                        }
                        else
                        {
                            this._endpoint = new SparqlRemoteEndpoint(new Uri(arg.Substring(arg.IndexOf(':') + 1)));
                        }
                    }
                    catch (UriFormatException uriEx)
                    {
                        Console.Error.WriteLine("rdfQuery: Unable to use remote endpoint with URI '" + arg.Substring(arg.IndexOf(':') + 1) + "' since this is not a valid URI");
                        if (this._debug) this.DebugErrors(uriEx);
                        return false;
                    }
                }
                else if (arg.StartsWith("-output:") || arg.StartsWith("-out:"))
                {
                    this._output = arg.Substring(arg.IndexOf(':') + 1);
                }
                else if (arg.StartsWith("-outformat:"))
                {
                    String format = arg.Substring(arg.IndexOf(':') + 1);
                    try
                    {
                        if (format.Contains("/"))
                        {
                            //MIME Type
                            this._graphWriter = MimeTypesHelper.GetWriter(format);
                            this._resultsWriter = MimeTypesHelper.GetSparqlWriter(format);
                        }
                        else
                        {
                            //File Extension
                            this._graphWriter = MimeTypesHelper.GetWriter(MimeTypesHelper.GetMimeType(format));
                            this._resultsWriter = MimeTypesHelper.GetSparqlWriter(MimeTypesHelper.GetMimeType(format));
                        }
                    }
                    catch (RdfException)
                    {
                        Console.Error.WriteLine("rdfQuery: The file extension '" + format + "' could not be used to determine a MIME Type and select a writer - default writers will be used");
                    }
                }
                else if (arg.StartsWith("-syntax"))
                {
                    if (arg.Contains(':'))
                    {
                        String syntax = arg.Substring(arg.IndexOf(':') + 1);
                        switch (syntax)
                        {
                            case "1":
                            case "1.0":
                                this._parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_0;
                                break;
                            case "1.1":
                                this._parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_1;
                                break;
                            case "E":
                            case "e":
                                this._parser.SyntaxMode = SparqlQuerySyntax.Extended;
                                break;
                            default:
                                Console.Error.WriteLine("rdfQuery: The value '" + syntax + "' is not a valid query syntax specifier - assuming SPARQL 1.1 with Extensions");
                                this._parser.SyntaxMode = SparqlQuerySyntax.Extended;
                                break;
                        }
                    }
                    else
                    {
                        this._parser.SyntaxMode = SparqlQuerySyntax.Extended;
                    }
                }
                else if (arg.StartsWith("-timeout:"))
                {
                    long timeout;
                    if (Int64.TryParse(arg.Substring(arg.IndexOf(':') + 1), out timeout))
                    {
                        this._timeout = timeout;
                    }
                    else
                    {
                        Console.Error.WriteLine("rdfQuery: The value '" + arg.Substring(arg.IndexOf(':') + 1) + "' is not a valid timeout in milliseconds - default timeouts will be used");
                    }
                }
                else if (arg.StartsWith("-r:"))
                {
                    arg = arg.Substring(arg.IndexOf(':') + 1);
                    switch (arg)
                    {
                        case "rdfs":
                            ((IInferencingTripleStore)this._store).AddInferenceEngine(new RdfsReasoner());
                            break;
                        case "skos":
                            ((IInferencingTripleStore)this._store).AddInferenceEngine(new SkosReasoner());
                            break;
                        default:
                            Console.Error.WriteLine("rdfQuery: The value '" + arg + "' is not a valid Reasoner - ignoring this option");
                            break;
                    }
                }
                else if (arg.StartsWith("-partialResults"))
                {
                    if (arg.Contains(':'))
                    {
                        bool partial;
                        if (Boolean.TryParse(arg.Substring(arg.IndexOf(':') + 1), out partial))
                        {
                            this._partialResults = partial;
                        }
                        else
                        {
                            Console.Error.WriteLine("rdfQuery: The value '" + arg.Substring(arg.IndexOf(':') + 1) + "' is not a valid boolean - partial results mode is disabled");
                        }
                    }
                    else
                    {
                        this._partialResults = true;
                    }
                }
                else if (arg.StartsWith("-noopt"))
                {
                    if (arg.Equals("-noopt"))
                    {
                        Options.QueryOptimisation = false;
                        Options.AlgebraOptimisation = false;
                    }
                    else if (arg.Length >= 7)
                    {
                        String opts = arg.Substring(7);
                        foreach (char c in opts.ToCharArray())
                        {
                            if (c == 'a' || c == 'A')
                            {
                                Options.AlgebraOptimisation = false;
                            }
                            else if (c == 'q' || c == 'Q')
                            {
                                Options.QueryOptimisation = false;
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfQuery: The value '" + c + "' as part of the -noopt argument is not supported - it has been ignored");
                            }
                        }
                    }
                }
                else if (arg.Equals("-nocache"))
                {
                    Options.UriLoaderCaching = false;
                }
                else if (arg.Equals("-nobom"))
                {
                    Options.UseBomForUtf8 = false;
                }
                else if (arg.Equals("-print"))
                {
                    this._print = true;
                }
                else if (arg.Equals("-debug"))
                {
                    this._debug = true;
                }
                else if (arg.StartsWith("-explain"))
                {
                    this._explain = true;
                    if (arg.Length > 9)
                    {
                        try
                        {
                            this._level = (ExplanationLevel)Enum.Parse(typeof(ExplanationLevel), arg.Substring(9));
                            this._level = (this._level | ExplanationLevel.OutputToConsoleStdErr | ExplanationLevel.Simulate) ^ ExplanationLevel.OutputToConsoleStdOut;
                        }
                        catch
                        {
                            Console.Error.WriteLine("rdfQuery: The argument '" + arg + "' does not specify a valid Explanation Level");
                            return false;
                        }
                    }
                }
                else if (arg.Equals("-help"))
                {
                    //Ignore Help Argument if other arguments present
                }
                else if (arg.StartsWith("-"))
                {
                    //Report Invalid Argument
                    Console.Error.WriteLine("rdfQuery: The argument '" + arg + "' is not a supported argument - it has been ignored");
                }
                else if (i == args.Length - 1)
                {
                    //Last Argument must be the Query
                    this._query = arg;
                }
                else
                {
                    //Treat as an input file

                    if (this._mode == RdfQueryMode.Remote)
                    {
                        Console.Error.WriteLine("rdfQuery: Cannot specify local files as well as specifying a remote endpoint to query");
                        return false;
                    }

                    try
                    {
                        this._mode = RdfQueryMode.Local;

                        //Try and parse RDF from the given file
                        if (!this._print)
                        {
                            Graph g = new Graph();
                            FileLoader.Load(g, arg);
                            this._store.Add(g);
                        }
                        else
                        {
                            Console.Error.WriteLine("rdfQuery: Ignoring the local file '" + arg + "' since -print has been specified so the query will not be executed so no need to load the data");
                        }
                    }
                    catch (RdfParseException parseEx)
                    {
                        Console.Error.WriteLine("rdfQuery: Ignoring the local file '" + arg + "' due to the following error:");
                        Console.Error.WriteLine("rdfQuery: Parser Error: " + parseEx.Message);
                        if (this._debug) this.DebugErrors(parseEx);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("rdfQuery: Ignoring the local file '" + arg + "' due to the following error:");
                        Console.Error.WriteLine("rdfQuery: Error: " + ex.Message);
                        if (this._debug) this.DebugErrors(ex);
                    }
                }


                i++;
            }

            return true;
        }

        private void ShowUsage()
        {
            Console.WriteLine("rdfQuery Utility for dotNetRDF");
            Console.WriteLine("--------------------------------");
            Console.WriteLine();
            Console.WriteLine("Command usage is as follows:");
            Console.WriteLine("rdfQuery input1 [input2 [input3 [...]]] [options] query");
            Console.WriteLine();
            Console.WriteLine("e.g. rdfQuery data.rdf \"SELECT * WHERE {?s a ?type}\"");
            Console.WriteLine("e.g. rdfQuery -uri:http://example.org/something -outformat:json \"SELECT * WHERE {?s a ?type}\" > results.json");
            Console.WriteLine("e.g. rdfQuery -endpoint:http://dbpedia.org/sparql -out:results.srx -outformat:srx \"SELECT * WHERE {?s a ?type} LIMIT 50\"");
            Console.WriteLine();
            Console.WriteLine("As in the above examples you can query a mixture of local files and URIs by prefixing URIs with -uri:");
            Console.WriteLine("To query a remote endpoint you can use the -endpoint: option and specify the endpoint URI.  If you use this option you cannot specify file/URI inputs but may specify multiple endpoints to federate your query");
            Console.WriteLine();
            Console.WriteLine("Notes");
            Console.WriteLine("-----");
            Console.WriteLine();
            Console.WriteLine("1. rdfQuery uses the SPARQL XML Results & NTriples Writers as its default writers if a specific format is not specified with the -outformat option");
            Console.WriteLine("2. rdfQuery writes to stdout if no output file is specified with the -out or -output option");
            Console.WriteLine("3. rdfQuery reads a query from stdin if the last argument is not a query (i.e. it is anything other than some -option)");
            Console.WriteLine();
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine("-debug");
            Console.WriteLine("Prints more detailed error messages if errors occur");
            Console.WriteLine();
            Console.WriteLine("-explain[:level]");
            Console.WriteLine("Prints query evaluation explanation to Standard Error");
            Console.WriteLine();
            //Console.WriteLine(" -nobom");
            //Console.WriteLine(" Specifies that no BOM should be used for UTF-8 Output");
            //Console.WriteLine();
            Console.WriteLine("-nocache");
            Console.WriteLine("Specifies that UriLoader caching is disabled");
            Console.WriteLine();
            Console.WriteLine("-noopt[:args]");
            Console.WriteLine("Disables optimisations, if used as -noopt: then args should be a series of characters indicating optimisations to disable, a/A to disable algebra optimisation and q/Q to disable query optimisation");
            Console.WriteLine();
            Console.WriteLine("-partialResults[:(true|false)]");
            Console.WriteLine("Specifies whether partial results should be returned in the event of a query timeout.  Only valid for queries over local files and URIs");
            Console.WriteLine();
            Console.WriteLine("-print");
            Console.WriteLine("Just prints the Query and it's SPARQL Algebra and doesn't execute the Query");
            Console.WriteLine();
            Console.WriteLine("-r:(skos|rdfs)");
            Console.WriteLine("Applies a SKOS/RDFS reasoner to the input data.  SKOS Concept Hierarchy/RDFS class & property hierarchies are dynamically determined based on the input data.  You may need to reorder the input files and URIs in order to get correct inference results e.g. if your schema was the last input it would result in little/no inferences being made");
            Console.WriteLine();
            Console.WriteLine("-roqet");
            Console.WriteLine("Runs rdfQuery in roqet compatibility mode, type rdfQuery -roqet -h for more information.  Must be the first argument or ignored.");
            Console.WriteLine();
            Console.WriteLine("-syntax[:(1|1.0|1.1|E)]");
            Console.WriteLine("Specifies what Query Syntax should be permitted");
            Console.WriteLine(" 1       SPARQL 1.0 Standard");
            Console.WriteLine(" 1.0     SPARQL 1.0 Standard");
            Console.WriteLine(" 1.1     SPARQL 1.1 Standard (Current Working Draft)");
            Console.WriteLine(" E       SPARQL 1.1 with Extensions (LET and additional aggregates)");
            Console.WriteLine();
            Console.WriteLine("-timeout:i");
            Console.WriteLine("Specifies the Query Timeout in milliseconds");
        }

        private void DebugErrors(Exception ex)
        {
            Console.Error.WriteLine("rdfQuery: Error Stack Trace:");
            Console.Error.WriteLine(ex.StackTrace);

            while (ex.InnerException != null)
            {
                Console.Error.WriteLine("rdfQuery: Error: " + ex.InnerException.Message);
                Console.Error.WriteLine("rdfQuery: Error Stack Trace:");
                Console.Error.WriteLine(ex.InnerException.StackTrace);
                ex = ex.InnerException;
            }
        }
    }
}
