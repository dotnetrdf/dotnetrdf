using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.OptimiserStats
{
    public class RdfOptimiserStats
    {
        private String[] _args;
        private bool _subjects = false, _predicates = false, _objects = false, _nodes = false;
        private String _file = "stats.ttl";
        private List<String> _inputs = new List<string>();
        private bool _literals = false;

        public RdfOptimiserStats(String[] args)
        {
            this._args = args;
        }

        public void Run()
        {
            if (this._args.Length == 0)
            {
                this.ShowUsage();
            }
            else
            {
                if (!this.ParseOptions())
                {
                    Console.Error.WriteLine("rdfOptStats: Error: One/More options were invalid");
                    return;
                }

                if (this._inputs.Count == 0)
                {
                    Console.Error.WriteLine("rdfOptStats: Error: No Inputs Specified");
                    return;
                }

                List<BaseStatsHandler> handlers = new List<BaseStatsHandler>();
                if (this._subjects && this._predicates && this._objects)
                {
                    handlers.Add(new SPOStatsHandler(this._literals));
                }
                else if (this._subjects && this._predicates)
                {
                    handlers.Add(new SPStatsHandler(this._literals));
                }
                else
                {
                    if (this._subjects) handlers.Add(new SubjectStatsHandler(this._literals));
                    if (this._predicates) handlers.Add(new PredicateStatsHandler(this._literals));
                    if (this._objects) handlers.Add(new ObjectStatsHandler(this._literals));
                }
                if (this._nodes)
                {
                    handlers.Add(new NodeStatsHandler());
                }

                bool ok = true;
                IRdfHandler handler;
                if (handlers.Count == 1)
                {
                    handler = handlers[0];
                }
                else
                {
                    handler = new MultiHandler(handlers.OfType<IRdfHandler>());
                }

                Stopwatch timer = new Stopwatch();
                timer.Start();
                for (int i = 0; i < this._inputs.Count; i++)
                {
                    Console.WriteLine("rdfOptStats: Processing Input " + (i + 1) + " of " + this._inputs.Count + " - '" + this._inputs[i] + "'");

                    try
                    {
                        FileLoader.Load(handler, this._inputs[i]);
                    }
                    catch (RdfParserSelectionException selEx)
                    {
                        ok = false;
                        Console.Error.WriteLine("rdfOptStats: Error: Unable to select a Parser to read input");
                        break;
                    }
                    catch (RdfParseException parseEx)
                    {
                        ok = false;
                        Console.Error.WriteLine("rdfOptStats: Error: Parsing Error while reading input");
                        break;
                    }
                    catch (RdfException parseEx)
                    {
                        ok = false;
                        Console.Error.WriteLine("rdfOptStats: Error: RDF Error while reading input");
                        break;
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        Console.Error.WriteLine("rdfOptStats: Error: Unexpected Error while reading input");
                        break;
                    }
                }
                Console.WriteLine("rdfOptStats: Finished Processing Inputs");
                timer.Stop();
                Console.WriteLine("rdfOptStats: Took " + timer.Elapsed + " to process inputs");
                timer.Reset();

                if (ok)
                {
                    //Output the Stats
                    timer.Start();
                    Graph g = new Graph();
                    g.NamespaceMap.Import(handlers.First().Namespaces);
                    try
                    {
                        foreach (BaseStatsHandler h in handlers)
                        {
                            h.GetStats(g);
                        }
                        IRdfWriter writer = MimeTypesHelper.GetWriter(MimeTypesHelper.GetMimeTypes(MimeTypesHelper.GetTrueFileExtension(this._file)));
                        if (writer is ICompressingWriter)
                        {
                            ((ICompressingWriter)writer).CompressionLevel = WriterCompressionLevel.High;
                        }
                        if (writer is IHighSpeedWriter)
                        {
                            ((IHighSpeedWriter)writer).HighSpeedModePermitted = false;
                        }
                        writer.Save(g, this._file);

                        Console.WriteLine("rdfOptStats: Statistics output to " + this._file);
                        timer.Stop();
                        Console.WriteLine("rdfOptStats: Took " + timer.Elapsed + " to output statistics");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("rdfOptStats: Error: Unexpected error outputting statistics to " + this._file);
                        Console.Error.WriteLine(ex.Message);
                        Console.Error.WriteLine(ex.StackTrace);
                    }
                }
                else
                {
                    Console.Error.WriteLine("rdfOptStats: Error: Unable to output statistics due to errors during input processing");
                }
            }
        }

        private bool ParseOptions()
        {
            bool ok = true;

            for (int i = 0; i < this._args.Length; i++)
            {
                String arg = this._args[i];
                switch (arg)
                {
                    case "-all":
                        this._subjects = true;
                        this._predicates = true;
                        this._objects = true;
                        break;
                    case "-s":
                        this._subjects = true;
                        break;
                    case "-p":
                        this._predicates = true;
                        break;
                    case "-o":
                        this._objects = true;
                        break;
                    case "-nodes":
                        this._nodes = true;
                        break;

                    case "-literals":
                        this._literals = true;
                        break;

                    case "-output":
                        if (i < this._args.Length - 1)
                        {
                            this._file = this._args[i + 1];
                            i++;
                        }
                        else
                        {
                            Console.Error.WriteLine("rdfOptStats: Error: -output option should be followed by a filename");
                            ok = false;
                        }
                        break;

                    default:
                        ok = this.ParseInputs(arg);
                        break;
                }

                if (!ok) break;
            }

            return ok;
        }

        private bool ParseInputs(String arg)
        {
            if (arg.Contains("*"))
            {
                int count = arg.ToCharArray().Count(c => c == '*');
                if (count > 2 || (count == 2 && !arg.EndsWith("*.*")))
                {
                    Console.Error.WriteLine("rdfOptStats: Error: Input Wildcard '" + arg + "' does not appear to be a valid wildcard");
                    return false;
                }

                String sep = new String(new char[] { Path.DirectorySeparatorChar });
                if (arg.Contains(sep))
                {
                    //Wildcard has a Directory in it
                    String dir = arg.Substring(0, arg.LastIndexOf(sep) + 1);
                    if (!Directory.Exists(dir))
                    {
                        Console.Error.WriteLine("rdfOptStats: Error: Input Wildcard '" + arg + "' uses a Directory '" + dir + "' which does not exist");
                        return false;
                    }

                    String wildcard = arg.Substring(dir.Length);
                    return this.ParseInputs(wildcard, dir);
                }
                else
                {
                    return this.ParseInputs(arg, Environment.CurrentDirectory);
                }
            }
            else
            {
                if (!File.Exists(arg))
                {
                    Console.Error.WriteLine("rdfOptStats: Error: Input File '" + arg + "' does not exist");
                    return false;
                }
                else
                {
                    this._inputs.Add(arg);
                    return true;
                }
            }

            return true;
        }

        private bool ParseInputs(String arg, String dir)
        {
            if (arg.Contains("*"))
            {
                //Wildcard is a File wildcard only
                if (arg.Equals("*") || arg.Equals("*.*"))
                {
                    //All Files
                    this._inputs.AddRange(Directory.GetFiles(dir));
                    return true;
                }
                else if (arg.Contains("*."))
                {
                    //All Files with a given Extension
                    String ext = arg.Substring(arg.LastIndexOf("*.") + 1);
                    this._inputs.AddRange(Directory.GetFiles(dir).Where(f => ext.Equals(MimeTypesHelper.GetTrueFileExtension(f))));
                    return true;
                }
                else
                {
                    //Invalid File Wildcard
                    Console.Error.WriteLine("rdfOptStats: Error: Input Wildcard '" + arg + "' does not appear to be a valid wildcard - only simple wildcards like *.* or *.rdf are currently supported");
                    return false;
                }
            } 
            else
            {
                if (!File.Exists(arg))
                {
                    Console.Error.WriteLine("rdfOptStats: Error: Input File '" + arg + "' does not exist");
                    return false;
                }
                else
                {
                    this._inputs.Add(arg);
                    return true;
                }
            }
            return true;
        }

        private void ShowUsage()
        {
            Console.WriteLine("rdfOptStats");
            Console.WriteLine("-----------");
            Console.WriteLine();
            Console.WriteLine("Command usage is as follows:");
            Console.WriteLine("rdfOptStats [options] input1 [input2 [input3 ...]]");
            Console.WriteLine();
            Console.WriteLine("e.g. rdfOptStats -all -output stats.ttl data1.rdf data2.rdf");
            Console.WriteLine("e.g. rdfOptStats -all -output stats.nt data\\*");
            Console.WriteLine();
            Console.WriteLine("Notes");
            Console.WriteLine("-----");
            Console.WriteLine("Only simple wildcard patterns are supported as inputs e.g.");
            Console.WriteLine("data\\*");
            Console.WriteLine("some\\path\\*.rdf");
            Console.WriteLine("*.*");
            Console.WriteLine("*.ttl");
            Console.WriteLine();
            Console.WriteLine("Any other wildcard pattern will be rejected");
            Console.WriteLine();
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine(" -all");
            Console.WriteLine("  Specifies that counts of Subjects, Predicates and Objects should be generated");
            Console.WriteLine();
            Console.WriteLine(" -literals");
            Console.WriteLine("  Specifies that counts should include Literals (default is URIs only) - this requires an output format that supports Literal Subjects e.g. N3");
            Console.WriteLine();
            Console.WriteLine(" -nodes");
            Console.WriteLine("  Specifies that aggregated for Nodes should be generated i.e. counts that don't specify which position the URI/Literal occurs in");
            Console.WriteLine();
            Console.WriteLine(" -p");
            Console.WriteLine("  Specifies that counts for Predicates should be generated");
            Console.WriteLine();
            Console.WriteLine(" -o");
            Console.WriteLine("  Specifies that counts for Objects should be generated");
            Console.WriteLine();
            Console.WriteLine(" -output file");
            Console.WriteLine("  Specifies the file to output the statistics to");
            Console.WriteLine();
            Console.WriteLine(" -s");
            Console.WriteLine("  Specifies that counts for Subjects should be generated");
            Console.WriteLine();
        }
    }
}
