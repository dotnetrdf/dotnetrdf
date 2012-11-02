using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace VDS.RDF.Utilities.Build.SyncProjects
{
    /// <summary>
    /// Implementation of copying the contents of one project file and combining it with a template file to produce a new project file
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                ShowUsage();
            }
            else
            {
                switch (args[0].Trim().ToLower())
                {
                    case "copy":
                        CopyToTemplate.Main(args);
                        break;
                    case "sync":
                        CopyToTemplate.Main(args);
                        break;
                    case "help":
                        ShowHelp(args);
                        break;
                    default:
                        Console.Error.WriteLine("SyncProjects: Error: Unknown command " + args[0]);
                        ShowUsage();
                        break;
                }
            }

        }

        private static void ShowUsage()
        {
            Console.WriteLine("dotNetRDF Build Tools - Sync Projects");
            Console.WriteLine();
            Console.WriteLine("This tool is used to sync the compilable contents of the a Source Project to a Target Project or to generate a Project File from a Template");
            Console.WriteLine("Usage is SyncProjects command");

            Console.WriteLine("Available commands are:");
            Console.WriteLine("  copy     Copy a Source Projects contents into a Template to generate a new Project");
            Console.WriteLine("  help     Prints this usage summary or the usage summary for a given command");
            Console.WriteLine("  sync     Syncs the contents of a Source Project with a Target Project");
            return;
        }

        private static void ShowHelp(String[] args)
        {
            if (args.Length <= 1)
            {
                ShowUsage();
            }
            else
            {
                switch (args[1].Trim().ToLower())
                {
                    case "copy":
                        CopyToTemplate.ShowUsage();
                        break;
                    case "sync":
                        SyncProjects.ShowUsage();
                        break;
                    case "help":
                        ShowUsage();
                        break;
                    default:
                        Console.Error.WriteLine("SyncProjects: Error: Unknown command " + args[0]);
                        ShowUsage();
                        break;
                }
            }
        }
    }
}
