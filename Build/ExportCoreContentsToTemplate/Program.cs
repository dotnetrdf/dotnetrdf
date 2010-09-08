using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ExportCoreContentsToTemplate
{
    class Program
    {
        public const String MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("dotNetRDF Build Tools - Export Core Library Contents to Template");
                Console.WriteLine();
                Console.WriteLine("This tool is used to export the compilable contents of the dotNetRDF library to a Template Project file to create an up to date Project file for building dotNetRDF targetted at a specific platform");
                Console.WriteLine("Usage is ExportCoreContentsToTemplate CoreProject.csproj Target.csproj.template Output.csproj [relPath]");
                Console.WriteLine("Relative Path is ..\\core\\ by default");
                return;
            }

            try
            {
                Console.WriteLine("dotNetRDF Build Tools: Attempting to generate Project " + args[2] + " using Template " + args[1] + " from Project " + args[1]);

                //Load Core Project
                XmlDocument coreProject = new XmlDocument();
                coreProject.Load(args[0]);

                //Set Relative Path
                String relativePath = (args.Length >= 4) ? args[3] : "..\\core\\";

                //Open Target Project Template
                XmlDocument template = new XmlDocument();
                template.Load(args[1]);

                //Copy Compile Item Groups from Core to Template
                foreach (XmlNode itemGroup in coreProject.DocumentElement.GetElementsByTagName("ItemGroup"))
                {
                    XmlElement newItemGroup = template.CreateElement("ItemGroup", MSBuildNamespace);

                    foreach (XmlNode compile in itemGroup.ChildNodes)
                    {
                        if (compile.Name.Equals("Compile") && compile.Attributes.GetNamedItem("Include") != null)
                        {
                            //Never include Compiled files which are themselves linked from another project
                            if (compile.HasChildNodes)
                            {
                                bool ok = true;
                                foreach (XmlNode n in compile.ChildNodes)
                                {
                                    if (n.Name.Equals("Link")) ok = false;
                                }
                                if (!ok) continue;
                            }

                            XmlElement newCompile = template.CreateElement("Compile", MSBuildNamespace);
                            XmlAttribute include = template.CreateAttribute("Include");//, MSBuildNamespace);
                            String includeFile = compile.Attributes["Include"].Value;

                            //Never include AssemblyInfo from source Project
                            if (includeFile.EndsWith("AssemblyInfo.cs")) continue;

                            include.Value = relativePath + includeFile;
                            newCompile.Attributes.Append(include);

                            XmlElement link = template.CreateElement("Link", MSBuildNamespace);
                            link.InnerText = compile.Attributes["Include"].Value;
                            newCompile.AppendChild(link);

                            newItemGroup.AppendChild(newCompile);
                        }
                    }

                    if (newItemGroup.HasChildNodes)
                    {
                        template.DocumentElement.AppendChild(newItemGroup);
                    }
                }

                template.Save(args[2]);

                Console.WriteLine("dotNetRDF Build Tools: Successfully generated Project " + args[2]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("dotNetRDF Build Tools: An error occurred attempting to export Core Project " + args[0] + " to Project " + args[2] + " using Template " + args[1]);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
