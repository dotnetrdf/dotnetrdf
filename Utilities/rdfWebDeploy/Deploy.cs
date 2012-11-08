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
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using Admin = Microsoft.Web.Administration;

namespace VDS.RDF.Utilities.Web.Deploy
{
    class Deploy
    {
        private bool _noClassicRegistration = false;
        private bool _noIntegratedRegistration = false;
        private bool _noLocalIIS = false;
        private bool _negotiate = false;
        private String _site = "Default Web Site";
        private bool _sql = false, _virtuoso = false, _fulltext = false;

        public void RunDeploy(String[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: 3 Arguments are required in order to use the -deploy mode, type rdfWebDeploy -help to see usage summary");
                return;
            }
            if (args.Length > 3)
            {
                if (!this.SetOptions(args.Skip(3).ToArray()))
                {
                    Console.Error.WriteLine("rdfWebDeploy: Deployment aborted since one/more options were not valid");
                    return;
                }
            }

            if (this._noLocalIIS)
            {
                Console.WriteLine("rdfWebDeploy: No Local IIS Server available so switching to -xmldeploy mode");
                XmlDeploy xdeploy = new XmlDeploy();
                xdeploy.RunXmlDeploy(args);
                return;
            }

            //Define the Server Manager object
            Admin.ServerManager manager = null;

            try
            {
                //Connect to the Server Manager
                if (!this._noIntegratedRegistration) manager = new Admin.ServerManager();

                //Open the Configuration File
                System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(args[1], this._site);
                Console.Out.WriteLine("rdfWebDeploy: Opened the Web.config file for the specified Web Application");

                //Detect Folders
                String appFolder = Path.GetDirectoryName(config.FilePath);
                String binFolder = Path.Combine(appFolder, "bin\\");
                String appDataFolder = Path.Combine(appFolder, "App_Data\\");
                if (!Directory.Exists(binFolder))
                {
                    Directory.CreateDirectory(binFolder);
                    Console.WriteLine("rdfWebDeploy: Created a bin\\ directory for the web application");
                }
                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                    Console.WriteLine("rdfWebDeploy: Created an App_Data\\ directory for the web application");
                }

                //Deploy dotNetRDF and required DLLs to the bin directory of the application
                String sourceFolder = RdfWebDeployHelper.ExecutablePath;
                IEnumerable<String> dlls = RdfWebDeployHelper.RequiredDLLs;
                if (this._sql) dlls = dlls.Concat(RdfWebDeployHelper.RequiredSqlDLLs);
                if (this._virtuoso) dlls = dlls.Concat(RdfWebDeployHelper.RequiredVirtuosoDLLs);
                if (this._fulltext) dlls = dlls.Concat(RdfWebDeployHelper.RequiredFullTextDLLs);
                foreach (String dll in dlls)
                {
                    if (File.Exists(Path.Combine(sourceFolder, dll)))
                    {
                        File.Copy(Path.Combine(sourceFolder, dll), Path.Combine(binFolder, dll), true);
                        Console.WriteLine("rdfWebDeploy: Deployed " + dll + " to the web applications bin directory");
                    }
                    else
                    {
                        Console.Error.WriteLine("rdfWebDeploy: Error: Required DLL " + dll + " which needs deploying to the web applications bin directory could not be found");
                        return;
                    }
                }

                //Deploy the configuration file to the App_Data directory
                if (File.Exists(args[2]))
                {
                    File.Copy(args[2], Path.Combine(appDataFolder, args[2]), true);
                    Console.WriteLine("rdfWebDeploy: Deployed the configuration file to the web applications App_Data directory");
                }
                else if (!File.Exists(Path.Combine(appDataFolder, args[2])))
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: Unable to continue deployment as the configuration file " + args[2] + " could not be found either locally for deployment to the App_Data folder or already present in the App_Data folder");
                    return;
                }

                //Set the AppSetting for the configuration file
                config.AppSettings.Settings.Remove("dotNetRDFConfig");
                config.AppSettings.Settings.Add("dotNetRDFConfig", "~/App_Data/" + Path.GetFileName(args[2]));
                Console.WriteLine("rdfWebDeploy: Set the \"dotNetRDFConfig\" appSetting to \"~/App_Data/" + Path.GetFileName(args[2]) + "\"");

                //Now load the Configuration Graph from the App_Data folder
                Graph g = new Graph();
                FileLoader.Load(g, Path.Combine(appDataFolder, args[2]));

                Console.WriteLine("rdfWebDeploy: Successfully deployed required DLLs and appSettings");
                Console.WriteLine();

                //Get the sections of the Configuration File we want to edit
                HttpHandlersSection handlersSection = config.GetSection("system.web/httpHandlers") as HttpHandlersSection;
                if (handlersSection == null)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: Unable to access the Handlers section of the web applications Web.Config file");
                    return;
                }

                //Detect Handlers from the Configution Graph and deploy
                IUriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                IUriNode dnrType = g.CreateUriNode(new Uri(ConfigurationLoader.PropertyType));
                IUriNode httpHandler = g.CreateUriNode(new Uri(ConfigurationLoader.ClassHttpHandler));

                //Deploy for IIS Classic Mode
                if (!this._noClassicRegistration)
                {
                    Console.WriteLine("rdfWebDeploy: Attempting deployment for IIS Classic Mode");
                    foreach (INode n in g.GetTriplesWithPredicateObject(rdfType, httpHandler).Select(t => t.Subject))
                    {
                        if (n.NodeType == NodeType.Uri)
                        {
                            String handlerPath = ((IUriNode)n).Uri.AbsolutePath;
                            INode type = g.GetTriplesWithSubjectPredicate(n, dnrType).Select(t => t.Object).FirstOrDefault();
                            if (type == null)
                            {
                                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot deploy the Handler <" + n.ToString() + "> as there is no dnr:type property specified");
                                continue;
                            }
                            if (type.NodeType == NodeType.Literal)
                            {
                                String handlerType = ((ILiteralNode)type).Value;

                                //First remove any existing registration
                                handlersSection.Handlers.Remove("*", handlerPath);

                                //Then add the new registration
                                handlersSection.Handlers.Add(new HttpHandlerAction(handlerPath, handlerType, "*"));

                                Console.WriteLine("rdfWebDeploy: Deployed the Handler <" + n.ToString() + "> to the web applications Web.Config file");
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot deploy the Handler <" + n.ToString() + "> as the value given for the dnr:type property is not a Literal");
                                continue;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("rdfWebDeploy: Error: Cannot deploy a Handler which is not specified as a URI Node");
                        }
                    }

                    //Deploy Negotiate by File Extension if appropriate
                    if (this._negotiate)
                    {
                        HttpModulesSection modulesSection = config.GetSection("system.web/httpModules") as HttpModulesSection;
                        if (modulesSection == null)
                        {
                            Console.Error.WriteLine("rdfWebDeploy: Error: Unable to access the Modules section of the web applications Web.Config file");
                            return;
                        }
                        modulesSection.Modules.Remove("NegotiateByExtension");
                        modulesSection.Modules.Add(new HttpModuleAction("NegotiateByExtension", "VDS.RDF.Web.NegotiateByFileExtension"));
                        Console.WriteLine("rdfWebDeploy: Deployed the Negotiate by File Extension Module");
                    }

                    Console.WriteLine("rdfWebDeploy: Successfully deployed for IIS Classic Mode");
                }

                //Save the completed Configuration File
                config.Save(ConfigurationSaveMode.Minimal);
                Console.WriteLine();

                //Deploy for IIS Integrated Mode
                if (!this._noIntegratedRegistration)
                {
                    Console.WriteLine("rdfWebDeploy: Attempting deployment for IIS Integrated Mode");
                    Admin.Configuration adminConfig = manager.GetWebConfiguration(this._site, args[1]);
                    Admin.ConfigurationSection newHandlersSection = adminConfig.GetSection("system.webServer/handlers");
                    Admin.ConfigurationElementCollection newHandlers = newHandlersSection.GetCollection();

                    foreach (INode n in g.GetTriplesWithPredicateObject(rdfType, httpHandler).Select(t => t.Subject))
                    {
                        if (n.NodeType == NodeType.Uri)
                        {
                            String handlerPath = ((IUriNode)n).Uri.AbsolutePath;
                            INode type = g.GetTriplesWithSubjectPredicate(n, dnrType).Select(t => t.Object).FirstOrDefault();
                            if (type == null)
                            {
                                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot deploy the Handler <" + n.ToString() + "> as there is no dnr:type property specified");
                                continue;
                            }
                            if (type.NodeType == NodeType.Literal)
                            {
                                String handlerType = ((ILiteralNode)type).Value;

                                //First remove any existing registration
                                foreach (Admin.ConfigurationElement oldReg in newHandlers.Where(el => el.GetAttributeValue("name").Equals(handlerPath)).ToList())
                                {
                                    newHandlers.Remove(oldReg);
                                }

                                //Then add the new registration
                                Admin.ConfigurationElement reg = newHandlers.CreateElement("add");
                                reg["name"] = handlerPath;
                                reg["path"] = handlerPath;
                                reg["verb"] = "*";
                                reg["type"] = handlerType;
                                newHandlers.AddAt(0, reg);

                                Console.WriteLine("rdfWebDeploy: Deployed the Handler <" + n.ToString() + "> to the web applications Web.Config file");
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot deploy the Handler <" + n.ToString() + "> as the value given for the dnr:type property is not a Literal");
                                continue;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("rdfWebDeploy: Error: Cannot deploy a Handler which is not specified as a URI Node");
                        }

                        //Deploy Negotiate by File Extension if appropriate
                        if (this._negotiate)
                        {
                            Admin.ConfigurationSection newModulesSection = adminConfig.GetSection("system.webServer/modules");
                            if (newModulesSection == null)
                            {
                                Console.Error.WriteLine("rdfWebDeploy: Error: Unable to access the Modules section of the web applications Web.Config file");
                                return;
                            }

                            //First remove the Old Module
                            Admin.ConfigurationElementCollection newModules = newModulesSection.GetCollection();
                            foreach (Admin.ConfigurationElement oldReg in newModules.Where(el => el.GetAttribute("name").Equals("NegotiateByExtension")).ToList())
                            {
                                newModules.Remove(oldReg);
                            }

                            //Then add the new Module
                            Admin.ConfigurationElement reg = newModules.CreateElement("add");
                            reg["name"] = "NegotiateByExtension";
                            reg["type"] = "VDS.RDF.Web.NegotiateByFileExtension";
                            newModules.AddAt(0, reg);

                            Console.WriteLine("rdfWebDeploy: Deployed the Negotiate by File Extension Module");
                        }
                    }

                    manager.CommitChanges();
                    Console.WriteLine("rdfWebDeploy: Successfully deployed for IIS Integrated Mode");
                }

            }
            catch (ConfigurationException configEx)
            {
                Console.Error.WriteLine("rdfWebDeploy: Configuration Error: " + configEx.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        private bool SetOptions(String[] args)
        {
            int i = 0;
            while (i < args.Length)
            {
                switch (args[i])
                {
                    case "-negotiate":
                        this._negotiate = true;
                        Console.WriteLine("rdfWebDeploy: Negotiate by File Extension Module will be performed");
                        break;
                    case "-nointreg":
                        this._noIntegratedRegistration = true;
                        Console.WriteLine("rdfWebDeploy: IIS Integrated Mode Handler and Module registration will not be performed");
                        break;
                    case "-noclassicreg":
                        this._noClassicRegistration = true;
                        Console.WriteLine("rdfWebDeploy: IIS Classic Handler and Module registration will not be performed");
                        break;
                    case "-noiis":
                        this._noLocalIIS = true;
                        Console.WriteLine("rdfWebDeploy: No local IIS Server available");
                        break;
                    case "-site":
                        if (i < args.Length - 1)
                        {
                            i++;
                            this._site = args[i];
                            Console.WriteLine("rdfWebDeploy: Using IIS Site " + this._site);
                        }
                        else
                        {
                            Console.Error.Write("rdfWebDeploy: Error: Expected a site name to be specified after the -site option");
                            return false;
                        }
                        break;
                    case "-sql":
                        this._sql = true;
                        Console.WriteLine("rdfWebDeploy: Will include Data.Sql DLLs");
                        break;
                    case "-virtuoso":
                        this._virtuoso = true;
                        Console.WriteLine("rdfWebDeploy: Will include Data.Virtuoso DLLs");
                        break;
                    case "-fulltext":
                        this._fulltext = true;
                        Console.WriteLine("rdfWebDeploy: Will include Query.FullText DLLs");
                        break;
                    default:
                        Console.Error.WriteLine("rdfWebDeploy: Error: " + args[i] + " is not a known option for the -deploy mode of this tool");
                        return false;
                }
                i++;
            }

            return true;
        }
    }
}
