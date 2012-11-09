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
    class CopyToTemplate
    {
        public const String MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        public const String DefaultRelativePath = @"..\net40\";

        public static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                ShowUsage();
                return;
            }

            //Get Settings
            String sourceProject = args[1];
            String templateProject = args[2];
            String targetProject = args[3];
            String relativePath = (args.Length >= 5) ? args[4] : DefaultRelativePath;

            try
            {
                Console.WriteLine("dotNetRDF Build Tools: Attempting to generate Project " + targetProject +" using Template " + templateProject + " from Project " + sourceProject);

                //Load Core Project
                XmlDocument coreProject = new XmlDocument();
                coreProject.Load(sourceProject);

                //Open Target Project Template
                XmlDocument template = new XmlDocument();
                template.Load(templateProject);

                //Copy Compile Item Groups from Core to Template
                foreach (XmlNode itemGroup in coreProject.DocumentElement.GetElementsByTagName("ItemGroup"))
                {
                    XmlElement newItemGroup = template.CreateElement("ItemGroup", MSBuildNamespace);

                    foreach (XmlNode compile in itemGroup.ChildNodes)
                    {
                        if ((compile.Name.Equals("Compile") || compile.Name.Equals("EmbeddedResource")) && compile.Attributes.GetNamedItem("Include") != null)
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

                            XmlElement newCompile = template.CreateElement(compile.Name, MSBuildNamespace);
                            XmlAttribute include = template.CreateAttribute("Include");
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

                template.Save(targetProject);

                Console.WriteLine("dotNetRDF Build Tools: Successfully generated Project " + targetProject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("dotNetRDF Build Tools: An error occurred attempting to export Project " + sourceProject + " to Project " + targetProject + " using Template " + templateProject);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void ShowUsage()
        {
            Console.WriteLine("dotNetRDF Build Tools - Sync Projects - Copy Contents to Template");
            Console.WriteLine();
            Console.WriteLine("This tool is used to copy the compilable contents of the one Project to a Template Project file to create an up to date Project file for building the templated project targetted at a specific platform");
            Console.WriteLine("Usage is SyncProjects copy Source.csproj Target.csproj.template Output.csproj [relPath]");
            Console.WriteLine("Relative Path is " + DefaultRelativePath + " by default");
        }
    }
}
