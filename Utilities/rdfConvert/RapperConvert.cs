/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.Reflection;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.Convert
{
    class RapperConvert
    {
        private String _input;
        private IRdfReader _parser;
        private IRdfWriter _writer;
        private Graph _g = new Graph(true);
        private Uri _outputBase = null, _inputBase = null;
        private bool _quiet = false;
        private bool _useNullOutputBase = false;
        private bool _useInputBase = false;
        private bool _count = false;
        private bool _guess = false;
        private bool _showGraphs = false;
        private bool _showNamespaces = false;
        private bool _ignoreWarnings = false;

        public void RunConvert(String[] args)
        {
            //Single Help Argument means Show Help
            if (args.Length == 1 && (args[0].Equals("-h") || args[0].Equals("--help")))
            {
                this.ShowUsage();
                return;
            }

            //Set the Options
            if (!this.SetOptions(args))
            {
                //If SetOptions returns false then some options were invalid and errors have been output to the error stream
                return;
            }

            if (this._input == null)
            {
                //If no Input then abort
                Console.Error.WriteLine("The required argument Input URI was not specified");
                return;
            }
            if (this._writer == null && !this._count)
            {
                //If no Output Format was specified and the Count option was not specified we abort
                if (!this._quiet) Console.Error.WriteLine("rdfConvert: No Ouput Format specified - using default serializer NTriples");
                this._writer = new NTriplesWriter();                
            }
            if (this._parser == null && !this._guess)
            {
                //Use guess mode if no specific input format or guess mode was specified
                if (!this._quiet) Console.Error.WriteLine("rdfConvert: No Input Format was specified and the guess option was not specified - using default parser RDF/XML");
                this._parser = new RdfXmlParser();
            }
            //Set Parser to Null if using guess mode
            if (this._guess) this._parser = null;
            if (this._parser != null && !this._ignoreWarnings)
            {
                //Output warnings to stderror if not ignoring warnings or using a mode where can't add a handler to the warning
                this._parser.Warning += this.ShowWarnings;
                if (this._writer != null) this._writer.Warning += this.ShowWarnings;
            }
            else if (!this._ignoreWarnings)
            {
                UriLoader.Warning += this.ShowWarnings;
                UriLoader.StoreWarning += this.ShowWarnings;
                FileLoader.Warning += this.ShowWarnings;
                FileLoader.StoreWarning += this.ShowWarnings;
            }

            //Try to parse in the Input
            try
            {
                if (!this._quiet) 
                {
                    if (this._parser != null)
                    {
                        Console.Error.WriteLine("rdfConvert: Parsing URI " + this._input + " with Parser " + this._parser.GetType().Name);
                    }
                    else
                    {
                        Console.Error.WriteLine("rdfConvert: Parsing URI " + this._input + " with guessing of Content Type");
                    }
                }
                if (this._input == "-")
                {
                    //Read from Standard In
                    if (this._guess) 
                    {
                        StringParser.Parse(this._g, Console.In.ReadToEnd());
                    } 
                    else 
                    {
                        this._parser.Load(this._g, new StreamReader(Console.OpenStandardInput()));
                    }
                }
                else
                {
                    try
                    {
                        Uri u = new Uri(this._input);
                        if (u.IsAbsoluteUri)
                        {
                            //Valid Absolute URI
                            UriLoader.Load(this._g, u, this._parser);
                        }
                        else
                        {
                            //If not an absolute URI then probably a filename
                            FileLoader.Load(this._g, this._input, this._parser);
                        }
                    }
                    catch (UriFormatException)
                    {
                        //If not a valid URI then probably a filename
                        FileLoader.Load(this._g, this._input, this._parser);
                    }
                }
            }
            catch (RdfParseException parseEx)
            {
                this.ShowErrors(parseEx, "Parse Error");
                return;
            }
            catch (RdfException rdfEx)
            {
                this.ShowErrors(rdfEx, "RDF Error");
                return;
            }
            catch (Exception ex)
            {
                this.ShowErrors(ex, "Error");
                return;
            }

            if (!this._quiet) Console.Error.WriteLine("rdfConvert: Parsing returned " + this._g.Triples.Count + " Triples");

            //Show only count if that was asked for
            if (this._count)
            {
                Console.WriteLine(this._g.Triples.Count);
                return;
            }

            //Show Namespaces if asked for
            if (this._showNamespaces)
            {
                foreach (String prefix in this._g.NamespaceMap.Prefixes)
                {
                    Console.WriteLine(prefix + ": <" + this._g.NamespaceMap.GetNamespaceUri(prefix).AbsoluteUri + ">");
                }
            }

            //Now do the serialization
            if (this._outputBase != null || this._useNullOutputBase)
            {
                //Set the Output Base URI if specified
                this._g.BaseUri = this._outputBase;
            }
            else if (this._useInputBase)
            {
                //Set the Output Base URI to the Input Base URI if specified
                //Have to reset this since parsing the Input may have changed the Base URI
                this._g.BaseUri = this._inputBase;
            }
            try
            {
                if (!this._quiet) Console.Error.WriteLine("rdfConvert: Serializing with serializer " + this._writer.GetType().Name);

                //Save the Graph to Standard Out
                this._writer.Save(this._g, Console.Out);
            }
            catch (RdfOutputException outEx)
            {
                this.ShowErrors(outEx, "Output Error");
                return;
            }
            catch (RdfException rdfEx)
            {
                this.ShowErrors(rdfEx, "RDF Error");
                return;
            }
            catch (Exception ex)
            {
                this.ShowErrors(ex, "Error");
                return;
            }
        }

        private bool SetOptions(String[] args)
        {
            //No arguments means fail
            if (args.Length == 0)
            {
                Console.WriteLine("rdfConvert: No arguments specified - type rdfConvert -rapper -h for usage information");
                return false;
            }

            int i = 0;
            while (i < args.Length)
            {
                String arg = args[i];

                if (arg.Equals("-i") || arg.Equals("--input"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.Error.WriteLine("rdfConvert: Unexpected end of arguments - expected a format for the input");
                        return false;
                    }
                    arg = args[i];
                    if (!this.SetParser(arg)) return false;
                }
                else if (arg.Equals("-I") || arg.Equals("--input-uri"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.Error.WriteLine("rdfConvert: Unexpected end of arguments - expected a Base URI for the Input");
                        return false;
                    }
                    arg = args[i];
                    if (!this.SetBaseUri(arg)) return false;
                }
                else if (arg.Equals("-o") || arg.Equals("--output"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.Error.WriteLine("rdfConvert: Unexpected end of arguments - expected a Format for the Output");
                        return false;
                    }
                    arg = args[i];
                    if (!this.SetWriter(arg)) return false;
                }
                else if (arg.Equals("-O") || arg.Equals("--output-uri"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.Error.WriteLine("rdfConvert: Unexpected end of arguments - expected a Base URI for the Output");
                        return false;
                    }
                    arg = args[i];
                    if (!this.SetOutputBaseUri(arg)) return false;
                }
                else if (arg.Equals("-c") || arg.Equals("--count"))
                {
                    this._count = true;
                }
                else if (arg.Equals("-e") || arg.Equals("--ignore-errors"))
                {
                    Console.Error.WriteLine("rdfConvert: rdfConvert does not support ignoring errors - this option will be ignored");
                }
                else if (arg.Equals("-f") || arg.Equals("--feature"))
                {
                    i++;
                    if (i >= args.Length)
                    {
                        Console.Error.WriteLine("rdfConvert: Unexpected end of arguments - expected a feature setting");
                        return false;
                    }
                    Console.Error.WriteLine("rdfConvert: rdfConvert does not support parser features - this option will be ignored");
                }
                else if (arg.Equals("-g") || arg.Equals("--guess"))
                {
                    this._guess = true;
                }
                else if (arg.Equals("-h") || arg.Equals("--help"))
                {
                    Console.Error.WriteLine("rdfConvert: Help can only be shown when no other arguments are specified");
                }
                else if (arg.Equals("-q") || arg.Equals("--quiet"))
                {
                    this._quiet = true;
                }
                else if (arg.Equals("-r") || arg.Equals("--replace-newlines"))
                {
                    Console.Error.WriteLine("rdfConvert: rdfConvert does not support replacing newlines - this option will be ignored");
                }
                else if (arg.Equals("-s") || arg.Equals("--scan"))
                {
                    Console.Error.WriteLine("rdfConvert: rdfConvert does not support the scan option - this option will be ignored");
                }
                else if (arg.Equals("--show-graphs"))
                {
                    this._showGraphs = true;
                }
                else if (arg.Equals("--show-namespaces"))
                {
                    this._showNamespaces = true;
                }
                else if (arg.Equals("-t") || arg.Equals("--trace"))
                {
                    Console.Error.WriteLine("rdfConvert: rdfConvert does not support the trace option - this option will be ignored");
                }
                else if (arg.Equals("-w") || arg.Equals("--ignore-warnings"))
                {
                    this._ignoreWarnings = true;
                }
                else if (arg.Equals("-v"))
                {
                    Console.WriteLine("dotNetRDF Version " + Assembly.GetAssembly(typeof(Triple)).GetName().Version.ToString());
                    Console.WriteLine("Copyright Rob Vesse 2009-10");
                    Console.WriteLine("http://www.dotnetrdf.org");
                    return false;
                }
                else if (arg.StartsWith("-") && !arg.Equals("-"))
                {
                    Console.Error.WriteLine("rdfConvert: Unexpected argument '" + arg + "' encountered - this does not appear to be a valid rapper mode option");
                    return false;
                }
                else
                {
                    //Assume this is the Input URI
                    this._input = arg;

                    i++;
                    if (i < args.Length)
                    {
                        //Assume the next thing is the Input Base URI if we haven't had a -I or --input-uri option
                        if (!this._useInputBase)
                        {
                            arg = args[i];
                            if (!this.SetBaseUri(arg)) return false;
                        }

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

        private bool SetParser(String format)
        {
            switch (format)
            {
                case "rdfxml":
                    this._parser = new RdfXmlParser();
                    break;
                case "ntriples":
                    this._parser = new NTriplesParser();
                    break;
                case "turtle":
                    this._parser = new TurtleParser();
                    break;
                case "n3":
                    this._parser = new Notation3Parser();
                    break;
                case "json":
                    this._parser = new RdfJsonParser();
                    break;
                case "html":
                case "rdfa":
                    this._parser = new RdfAParser();
                    break;
                case "guess":
                    this._guess = true;
                    break;
                default:
                    Console.Error.WriteLine("rdfConvert: The value '" + format + "' is not a valid format parameter");
                    return false;
            }
            return true;
        }

        private bool SetWriter(String format)
        {
            switch (format)
            {
                case "rdfxml":
                    this._writer = new PrettyRdfXmlWriter();
                    break;
                case "ntriples":
                    this._writer = new NTriplesWriter();
                    break;
                case "turtle":
                    this._writer = new CompressingTurtleWriter(WriterCompressionLevel.High);
                    break;
                case "n3":
                    this._writer = new Notation3Writer(WriterCompressionLevel.High);
                    break;
                case "html":
                case "rdfa":
                    this._writer = new HtmlWriter();
                    break;
                case "json":
                    this._writer = new RdfJsonWriter();
                    break;
                case "dot":
                    this._writer = new GraphVizWriter();
                    break;
                default:
                    Console.Error.WriteLine("rdfConvert: The value '" + format + "' is not a valid format parameter");
                    return false;
            }
            return true;
        }

        private bool SetBaseUri(String uri)
        {
            if (uri.Equals("-"))
            {
                this._useInputBase = true;
                return true;
            }
            else
            {
                try
                {
                    Uri u = new Uri(uri);
                    this._g.BaseUri = u;
                    this._inputBase = u;
                    this._useInputBase = true;
                    return true;
                }
                catch (UriFormatException ex)
                {
                    Console.Error.WriteLine("rdf:Convert The value '" + uri + "' was not a valid URI for use as the Base URI");
                    this.ShowErrors(ex, "URI Format Error");
                    return false;
                }
            }
        }

        private bool SetOutputBaseUri(String uri)
        {
            if (uri.Equals("-"))
            {
                this._useNullOutputBase = true;
                return true;
            }
            else
            {
                try
                {
                    Uri u = new Uri(uri);
                    this._outputBase = u;
                    return true;
                }
                catch (UriFormatException ex)
                {
                    Console.Error.WriteLine("rdfConvert: The value '" + uri + "' was not a valid URI for use as the Output Base URI");
                    this.ShowErrors(ex, "URI Format Error");
                    return false;
                }
            }
        }

        private void ShowWarnings(String message)
        {
            Console.Error.WriteLine("rdfConvert: Warning: " + message);
        }

        private void ShowErrors(Exception ex, String prefix)
        {
            Console.Error.WriteLine("rdfConvert: " + prefix + ": " + ex.Message);
            if (ex.InnerException != null) this.ShowErrors(ex.InnerException, prefix);
        }

        private void ShowUsage()
        {
            Console.WriteLine("rdfConvert Utility for dotNetRDF");
            Console.WriteLine("--------------------------------");
            Console.WriteLine();
            Console.WriteLine("Running in rapper compatibility mode (first argument was -rapper)");
            Console.WriteLine("Usage is rdfConvert -rapper RAPPER_ARGS");
            Console.WriteLine("In rapper compatibility mode RAPPER_ARGS must be replaced with arguments in the rapper command line format as described at http://librdf.org/raptor/rapper.html");
            Console.WriteLine();
            Console.WriteLine("Supported Input Formats");
            Console.WriteLine("-----------------------");
            Console.WriteLine();
            Console.WriteLine("rdfxml       RDF/XML (default)");
            Console.WriteLine("ntriples     NTriples");
            Console.WriteLine("turtle       Turtle Terse RDF Triple Language");
            Console.WriteLine("n3           Notation 3");
            Console.WriteLine("json         RDF/JSON Resource-Centric");
            Console.WriteLine("html         HTML containing RDFa");
            Console.WriteLine("rdfa         HTML containing RDFa");
            Console.WriteLine("guess        Pick the parser to use using content type/file extension/heuristic guessing");
            Console.WriteLine();
            Console.WriteLine("Supported Output Formats");
            Console.WriteLine("------------------------");
            Console.WriteLine();
            Console.WriteLine("ntriples     NTriples (default)");
            Console.WriteLine("turtle       Turtle");
            Console.WriteLine("n3           Notation 3");
            Console.WriteLine("rdfxml       RDF/XML");
            Console.WriteLine("json         RDF/JSON Resource-Centric");
            Console.WriteLine("html         HTML containing RDFa");
            Console.WriteLine("rdfa         HTML containing RDFa");
            Console.WriteLine("dot          GraphViz DOT format");
            Console.WriteLine();
            Console.WriteLine("Notes");
            Console.WriteLine("-----");
            Console.WriteLine();
            Console.WriteLine("1. The -e, -f, -r, -s and -t rapper options are currently ignored");
            Console.WriteLine("2. rapper compatibility mode copies the syntax (not the logic) of the Redland rapper tool so it does not make any guarantee that it behaves in the same way as rapper would given a particular input");
            Console.WriteLine("3. rapper compatibility mode does not support RDF dataset formats - NQuads, TriG and TriX - use the default mode of rdfConvert to process these");
        }
    }
}
