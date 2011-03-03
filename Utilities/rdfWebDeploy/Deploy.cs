/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.IO;
using System.Linq;
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
        private String _site = "Default Web Site";

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
                foreach (String dll in RdfWebDeployHelper.RequiredDLLs)
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
                HttpHandlersSection handlersSection = (HttpHandlersSection)config.GetSection("system.web/httpHandlers");
                if (handlersSection == null)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: Unable to access the Handlers section of the web applications Web.Config file");
                    return;
                }

                //Detect Handlers from the Configution Graph and deploy
                UriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                UriNode dnrType = g.CreateUriNode(new Uri(ConfigurationLoader.ConfigurationNamespace + "type"));
                UriNode httpHandler = g.CreateUriNode(new Uri(ConfigurationLoader.ConfigurationNamespace + "HttpHandler"));

                //Deploy for IIS Classic Mode
                if (!this._noClassicRegistration)
                {
                    Console.WriteLine("rdfWebDeploy: Attempting deployment for IIS Classic Mode");
                    foreach (INode n in g.GetTriplesWithPredicateObject(rdfType, httpHandler).Select(t => t.Subject))
                    {
                        if (n.NodeType == NodeType.Uri)
                        {
                            String handlerPath = ((UriNode)n).Uri.AbsolutePath;
                            INode type = g.GetTriplesWithSubjectPredicate(n, dnrType).Select(t => t.Object).FirstOrDefault();
                            if (type == null)
                            {
                                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot deploy the Handler <" + n.ToString() + "> as there is no dnr:type property specified");
                                continue;
                            }
                            if (type.NodeType == NodeType.Literal)
                            {
                                String handlerType = ((LiteralNode)type).Value;

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
                            String handlerPath = ((UriNode)n).Uri.AbsolutePath;
                            INode type = g.GetTriplesWithSubjectPredicate(n, dnrType).Select(t => t.Object).FirstOrDefault();
                            if (type == null)
                            {
                                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot deploy the Handler <" + n.ToString() + "> as there is no dnr:type property specified");
                                continue;
                            }
                            if (type.NodeType == NodeType.Literal)
                            {
                                String handlerType = ((LiteralNode)type).Value;

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
                    }

                    manager.CommitChanges();
                    Console.WriteLine("rdfWebDeploy: Successfully deployed for IIS Integrated Mode");
                }

            }
            catch (ConfigurationException configEx)
            {
                Console.Error.Write("rdfWebDeploy: Configuration Error: " + configEx.Message);
            }
            catch (Exception ex)
            {
                Console.Error.Write("rdfWebDeploy: Error: " + ex.Message);
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
                    case "-nointreg":
                        this._noIntegratedRegistration = true;
                        Console.WriteLine("rdfWebDeploy: IIS Integrated Mode Handler registration will not be performed");
                        break;
                    case "-noclassicreg":
                        this._noClassicRegistration = true;
                        Console.WriteLine("rdfWebDeploy: IIS Classic Handler registration will not be performed");
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
