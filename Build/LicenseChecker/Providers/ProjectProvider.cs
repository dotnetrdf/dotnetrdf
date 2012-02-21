using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LicenseChecker.Providers
{
    class ProjectProvider
        : ISourceProvider
    {
        private String _projectFile;

        public ProjectProvider(String projectFile)
        {
            this._projectFile = projectFile;
        }

        public IEnumerable<string> GetSourceFiles()
        {
            //Load Project
            XmlDocument project = new XmlDocument();
            project.Load(this._projectFile);
            String basePath = Path.GetFullPath(this._projectFile);

            //Find Project Items
            foreach (XmlNode itemGroup in project.DocumentElement.GetElementsByTagName("ItemGroup"))
            {
                foreach (XmlNode compile in itemGroup.ChildNodes)
                {
                    if (compile.Name.Equals("Compile") || compile.Name.Equals("EmbeddedResource"))
                    {
                        if (compile.Attributes.GetNamedItem("Include") != null)
                        {
                            //If linked from another project then ensure we resolve the linked file
                            String file = String.Empty;
                            if (compile.HasChildNodes)
                            {
                                foreach (XmlNode n in compile.ChildNodes)
                                {
                                    if (n.Name.Equals("Link"))
                                    {
                                        file = n.InnerText;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                               file = compile.Attributes["Include"].Value;
                            }

                            if (String.IsNullOrEmpty(file)) continue;

                            //Resolve the path
                            file = Path.Combine(basePath, file);
                            yield return file;
                        }
                    }
                }
            }
        }
    }
}
