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
    class SyncProjects
    {
        public const String MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        public const String DefaultRelativePath = @"..\net40\";

        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                ShowUsage();
                return;
            }

            //Get Settings
            String sourceProject = args[1];
            String targetProject = args[2];
            String relativePath = (args.Length >= 4) ? args[3] : DefaultRelativePath;

            try
            {
                Console.WriteLine("dotNetRDF Build Tools: Attempting to sync Source Project " + sourceProject + " with Target Project " + targetProject);

                //Load Source Project
                XmlDocument source = new XmlDocument();
                source.Load(sourceProject);

                //Firstly scan the Source Project for sync eligible items
                HashSet<String> sourceFiles = new HashSet<string>();
                HashSet<String> resourceFiles = new HashSet<string>();

                //Build the sets of included files (Compilable Source and Embedded Resources)
                foreach (XmlNode itemGroup in source.DocumentElement.GetElementsByTagName("ItemGroup"))
                {
                    foreach (XmlNode item in itemGroup.ChildNodes)
                    {
                        if ((item.Name.Equals("Compile") || item.Name.Equals("EmbeddedResource")) && item.Attributes.GetNamedItem("Include") != null)
                        {
                            //Never sync files which are themselves linked from another project
                            if (item.HasChildNodes)
                            {
                                bool ok = true;
                                foreach (XmlNode n in item.ChildNodes)
                                {
                                    if (n.Name.Equals("Link")) ok = false;
                                }
                                if (!ok) continue;
                            }

                            //Get the Include attribute which denotes a file in the Project
                            String includeFile = item.Attributes["Include"].Value;

                            //Never sync AssemblyInfo
                            if (includeFile.EndsWith("AssemblyInfo.cs")) continue;

                            switch (item.Name)
                            {
                                case "Compile":
                                    sourceFiles.Add(includeFile);
                                    break;
                                case "EmbeddedResource":
                                    resourceFiles.Add(includeFile);
                                    break;
                            }
                        }
                    }
                }
                source = null;

                //Open Target Project
                XmlDocument target = new XmlDocument();
                target.Load(targetProject);

                //Now scan over compilable items which are linked from this project to check the items are present and correct
                HashSet<String> linkedSources = new HashSet<string>();
                HashSet<String> linkedResources = new HashSet<string>();

                int updates = 0;
                foreach (XmlNode itemGroup in target.DocumentElement.GetElementsByTagName("ItemGroup"))
                {
                    List<XmlNode> toRemove = new List<XmlNode>();
                    foreach (XmlNode item in itemGroup.ChildNodes)
                    {
                        if ((item.Name.Equals("Compile") || item.Name.Equals("EmbeddedResource")) && item.Attributes.GetNamedItem("Include") != null)
                        {
                            //Only sync files which are themselves linked from another project
                            bool ok = false;
                            if (item.HasChildNodes)
                            {
                                foreach (XmlNode n in item.ChildNodes)
                                {
                                    if (n.Name.Equals("Link")) ok = true;
                                }
                            }
                            if (!ok) continue;

                            //Get the Include attribute which denotes a file in the Project
                            XmlAttribute includeAttr = item.Attributes["Include"];
                            String includeFile = includeAttr.Value;

                            //Get the current Link value
                            XmlElement linkEl = item["Link"];
                            String linkValue = linkEl.InnerText;

                            //Never sync AssemblyInfo
                            if (includeFile.EndsWith("AssemblyInfo.cs")) continue;

                            //Now we must see whether the linked item still exists in the source project
                            //Remember that linkValue holds the actual path that should appear in the source project
                            //The includeFile holds the relative path to this file

                            //The actualPath is the path we want to ensure is set as the value of the Include attribute
                            String actualPath = relativePath + includeFile;

                            switch (item.Name)
                            {
                                case "Compile":
                                    //Check file exists in source project
                                    if (sourceFiles.Contains(linkValue))
                                    {
                                        //Check path is correct in target
                                        if (!includeFile.Equals(actualPath))
                                        {
                                            includeAttr.Value = actualPath;
                                            updates++;
                                        }

                                        //This linked item is correct
                                        linkedSources.Add(includeFile);
                                    }
                                    else
                                    {
                                        //This linked item refers to a file no longer in the source
                                        //and so should be removed
                                        toRemove.Add(item);
                                    }
                                    break;
                                case "EmbeddedResource":
                                    //Check file exists in source project
                                    if (resourceFiles.Contains(linkValue))
                                    {
                                        //Check path is correct in target
                                        if (!includeFile.Equals(actualPath))
                                        {
                                            includeAttr.Value = actualPath;
                                            updates++;
                                        }

                                        //This linked item is correct
                                        linkedResources.Add(includeFile);
                                    }
                                    else
                                    {
                                        //This linked item refers to a file no longer in the source and so
                                        //should be removed
                                        toRemove.Add(item);
                                    }
                                    break;
                            }
                        }
                    }

                    //Remove any defunct entries
                    if (toRemove.Count > 0)
                    {
                        updates += toRemove.Count;
                        foreach (XmlNode item in toRemove)
                        {
                            itemGroup.RemoveChild(item);
                        }
                    }
                }

                //Ensure any missing items are now added
                if (linkedSources.Count < sourceFiles.Count)
                {
                    XmlElement newItemGroup = target.CreateElement("ItemGroup", MSBuildNamespace);
                    foreach (String file in sourceFiles)
                    {
                        if (!linkedSources.Contains(file))
                        {
                            XmlElement newItem = target.CreateElement("Compile", MSBuildNamespace);
                            XmlAttribute include = target.CreateAttribute("Include");
                            include.Value = relativePath + file;
                            newItem.Attributes.Append(include);

                            XmlElement newLink = target.CreateElement("Link", MSBuildNamespace);
                            newLink.InnerText = file;
                            newItem.AppendChild(newLink);

                            newItemGroup.AppendChild(newItem);
                            updates++;
                        }
                    }
                    target.DocumentElement.AppendChild(newItemGroup);
                }
                if (linkedResources.Count < resourceFiles.Count)
                {
                    XmlElement newItemGroup = target.CreateElement("ItemGroup", MSBuildNamespace);
                    foreach (String file in resourceFiles)
                    {
                        if (!linkedResources.Contains(file))
                        {
                            XmlElement newItem = target.CreateElement("EmbeddedResource", MSBuildNamespace);
                            XmlAttribute include = target.CreateAttribute("Include");
                            include.Value = relativePath + file;
                            newItem.Attributes.Append(include);

                            XmlElement newLink = target.CreateElement("Link", MSBuildNamespace);
                            newLink.InnerText = file;
                            newItem.AppendChild(newLink);

                            newItemGroup.AppendChild(newItem);
                            updates++;
                        }
                    }
                    target.DocumentElement.AppendChild(newItemGroup);
                }

                if (updates > 0)
                {
                    target.Save(targetProject);
                    Console.WriteLine("dotNetRDF Build Tools: Successfully synced projects (" + updates + " Changes)");
                }
                else
                {
                    Console.WriteLine("dotNetRDF Build Tools: Successfully synced projects (No Changes)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("dotNetRDF Build Tools: An error occurred attempting to sync Source Project " + sourceProject + " with Target Project " + targetProject);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void ShowUsage()
        {
            Console.WriteLine("dotNetRDF Build Tools - Sync Projects - Sync Project Files");
            Console.WriteLine();
            Console.WriteLine("This tool is used to sync the compilable contents of the a Source Project to a Target Project");
            Console.WriteLine("Usage is SyncProjects sync Source.csproj Target.csproj [relPath]");
            Console.WriteLine("Relative Path is " + DefaultRelativePath + " by default");
        }
    }
}
