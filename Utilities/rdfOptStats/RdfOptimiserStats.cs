using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

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
                //TODO: Add in cases for other combinations

                if (this._nodes)
                {
                    //TODO: Add in node count handler
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

                if (ok)
                {
                    //Output the Stats
                    Graph g = new Graph();
                    try
                    {
                        foreach (BaseStatsHandler h in handlers)
                        {
                            h.GetStats(g);
                        }
                        g.SaveToFile(this._file);

                        Console.WriteLine("rdfOptStats: Statistics output to " + this._file);
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
                    this._inputs.AddRange(Directory.GetFiles(dir).Where(f => ext.Equals(Path.GetExtension(f))));
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
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine(" -all");
            Console.WriteLine("  Specifies that counts of Subjects, Predicate and Objects should be generated");
            Console.WriteLine();
            Console.WriteLine(" -literals");
            Console.WriteLine("  Specifies that counts for Literals should be generated");
            Console.WriteLine();
            Console.WriteLine(" -output file");
            Console.WriteLine("  Specifies the file to output the statistics to");
            Console.WriteLine();

        }
    }
}
