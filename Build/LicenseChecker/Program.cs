using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LicenseChecker.Checkers;
using LicenseChecker.Providers;

namespace LicenseChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckerOptions options = new CheckerOptions();
            ParseOptions(options, args);

            CheckerFactory.Init(options);

            ISourceProvider provider = options.GetProvider();
            long good = 0, bad = 0, unknown = 0;

            foreach (String file in provider.GetSourceFiles())
            {
                ILicenseChecker checker = CheckerFactory.GetChecker(file);
                if (checker == null)
                {
                    Console.Error.WriteLine("LicenseChecker: Warn: Unable to select a license checker for " + file);
                    unknown++;
                }
                else
                {
                    if (checker.IsLicensed(file))
                    {
                        good++;
                    }
                    else if (options.Fix && checker.FixLicense(file))
                    {
                        Console.WriteLine("LicenseChecker: Added License to file " + file);
                        good++;
                    }
                    else
                    {
                        bad++;
                        Console.Error.WriteLine("LicenseChecker: Error: No license found for file " + file);
                    }
                }
            }

            Console.WriteLine("LicenseChecker: " + good + " Licensed Files, " + bad + " Unlicensed Files, " + unknown + " Unknown Files");
            if (bad > 0 || unknown > 0) Environment.Exit(1);
            Environment.Exit(0);
        }

        static void ParseOptions(CheckerOptions options, String[] args)
        {
            String file;
            for (int i = 0; i < args.Length; i++)
            {
                String arg = args[i];
                switch (arg)
                {
                    case "-p":
                    case "-project":
                        String projFile = GetArgValue(arg, args, ref i);
                        if (File.Exists(projFile))
                        {
                            options.ProjectFiles.Add(projFile);
                        }
                        else
                        {
                            Console.Error.WriteLine("LicenseChecker: Error: Provided Project File " + projFile + " does not exist");
                            Environment.Exit(1);
                        }
                        break;
                    case "-d":
                    case "-directory":
                        String dir = GetArgValue(arg, args, ref i);
                        if (Directory.Exists(dir))
                        {
                            options.Directories.Add(dir);
                        }
                        else
                        {
                            Console.Error.WriteLine("LicenseChecker: Error: Provided directory " + dir + " does not exist");
                            Environment.Exit(1);
                        }
                        break;
                    case "-exclude-exts":
                        options.ExcludedExtensions.AddRange(GetArgValue(arg, args, ref i).Split(','));
                        break;
                    case "-include-exts":
                        options.IncludedExtensions.AddRange(GetArgValue(arg, args, ref i).Split(','));
                        break;
                    case "-exclude-files":
                        options.ExcludedFiles.AddRange(GetArgValue(arg, args, ref i).Split(','));
                        break;
                    case "-include-files":
                        options.IncludedFiles.AddRange(GetArgValue(arg, args, ref i).Split(','));
                        break;
                    case "-exclude":
                        options.ExclusionPattern = GetArgValue(arg, args, ref i);
                        break;
                    case "-include":
                        options.InclusionPattern = GetArgValue(arg, args, ref i);
                        break;
                    case "-license-search":
                        options.SearchString = GetArgValue(arg, args, ref i);
                        break;
                    case "-license-search-file":
                        file = GetArgValue(arg, args, ref i);
                        if (File.Exists(file))
                        {
                            options.SearchString = File.ReadAllText(file);
                        }
                        else
                        {
                            Console.Error.WriteLine("LicenseChecker: Error: Value of option " + arg + " pointed to a non-existent file");
                            Environment.Exit(1);
                        }
                        break;
                    case "-license-file":
                        file = GetArgValue(arg, args, ref i);
                        if (File.Exists(file))
                        {
                            options.LicenseString = File.ReadAllText(file);
                        }
                        else
                        {
                            Console.Error.WriteLine("LicenseChecker: Error: Value of option " + arg + " pointed to a non-existent file");
                        }
                        break;
                    case "-fix":
                        options.Fix = true;
                        break;
                    case "-overwrite":
                        options.OverwriteExisting = true;
                        break;
                    default:
                        Console.Error.WriteLine("LicenseChecker: Error: Unknown option " + arg);
                        Environment.Exit(1);
                        break;
                }
            }
        }

        static String GetArgValue(String arg, String[] args, ref int i)
        {
            if (i < args.Length - 1)
            {
                i++;
                return args[i];
            }
            else
            {
                Console.Error.WriteLine("LicenseChecker: Error: Expected a value after the " + arg + " option");
                Environment.Exit(1);
                return null;
            }
        }
    }
}
