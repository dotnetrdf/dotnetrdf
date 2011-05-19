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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Utilities.Web.Deploy
{
    class XmlDeploy
    {
        private bool _noClassicRegistration = false;
        private bool _noIntegratedRegistration = false;
        private bool _noLocalIIS = false;
        private bool _negotiate = false;

        public void RunXmlDeploy(String[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: 3 Arguments are required in order to use the -xmldeploy mode, type rdfWebDeploy -help to see usage summary");
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

            try
            {
                //Determine the path to the Web.config file if possible
                if (args[1].Equals("."))
                {
                    args[1] = Path.Combine(".", "Web.config");
                }
                else if (Path.GetFileName(args[1]).Equals(String.Empty))
                {
                    //If we were given a Folder Path then add Web.config to the end
                    args[1] = Path.Combine(args[1], "Web.config");
                } 
                else  if (!Path.GetFileName(args[1]).Equals("Web.config", StringComparison.OrdinalIgnoreCase))
                {
                    //If out path was to a file and it wasn't a Web.config file we error
                    Console.Error.WriteLine("rdfWebDeploy: Error: You must specify a Web.config file for the web application you wish to deploy to");
                    return;
                }

                //Open the Configuration File
                XmlDocument config = new XmlDocument();
                if (File.Exists(args[1]))
                {
                    //If the File exists open it
                    config.Load(args[1]);

                    //Verify this does appear to be a Web.config file
                    if (!config.DocumentElement.Name.Equals("configuration"))
                    {
                        Console.Error.WriteLine("rdfWebDeploy: Error: The Web.Config file for the Web Application does not appear to be a valid Web.Config file");
                        return;
                    }
                }
                else
                {
                    //If the Web.Config file doesn't exist then create one
                    XmlDeclaration decl = config.CreateXmlDeclaration("1.0", "utf-8", null);
                    config.AppendChild(decl);
                    XmlElement docEl = config.CreateElement("configuration");
                    config.AppendChild(docEl);
                    config.Save(args[1]);
                }
                Console.Out.WriteLine("rdfWebDeploy: Opened the Web.Config file for the specified Web Application");
                
                XmlNode reg = null;

                //Detect Folders
                String appFolder = Path.GetDirectoryName(args[1]);
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
                XmlNodeList appSettingsNodes = config.DocumentElement.GetElementsByTagName("appSettings");
                XmlNode appSettings;
                if (appSettingsNodes.Count == 0)
                {
                    appSettings = config.CreateElement("appSettings");
                    config.DocumentElement.AppendChild(appSettings);
                }
                else if (appSettingsNodes.Count > 1)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: The Configuration File for the Web Application appears to be invalid as more than one <appSettings> node exists");
                    return;
                }
                else
                {
                    appSettings = appSettingsNodes[0];
                }
                XmlNode configFileSetting = null;
                foreach (XmlNode setting in appSettings.ChildNodes)
                {
                    if (setting.Attributes.GetNamedItem("key") != null)
                    {
                        if (setting.Attributes["key"].Value.Equals("dotNetRDFConfig"))
                        {
                            configFileSetting = setting;
                            break;
                        }
                    }
                }
                if (configFileSetting == null)
                {
                    configFileSetting = config.CreateElement("add");
                    XmlAttribute attr = config.CreateAttribute("key");
                    attr.Value = "dotNetRDFConfig";
                    configFileSetting.Attributes.Append(attr);
                    attr = config.CreateAttribute("value");
                    attr.Value = "~/App_Data/" + Path.GetFileName(args[2]);
                    configFileSetting.Attributes.Append(attr);
                    appSettings.AppendChild(configFileSetting);
                }
                else
                {
                    configFileSetting.Attributes["value"].Value = "~/App_Data/" + Path.GetFileName(args[2]);
                }
                Console.WriteLine("rdfWebDeploy: Set the \"dotNetRDFConfig\" appSetting to \"~/App_Data/" + Path.GetFileName(args[2]) + "\"");

                //Now load the Configuration Graph from the App_Data folder
                Graph g = new Graph();
                FileLoader.Load(g, Path.Combine(appDataFolder, args[2]));

                Console.WriteLine("rdfWebDeploy: Successfully deployed required DLLs and appSettings");
                config.Save(args[1]);
                Console.WriteLine();

                //Detect Handlers from the Configution Graph and deploy
                IUriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                IUriNode dnrType = g.CreateUriNode(new Uri(ConfigurationLoader.ConfigurationNamespace + "type"));
                IUriNode httpHandlerType = g.CreateUriNode(new Uri(ConfigurationLoader.ConfigurationNamespace + "HttpHandler"));

                //Deploy for IIS Classic Mode
                if (!this._noClassicRegistration)
                {
                    Console.WriteLine("rdfWebDeploy: Attempting deployment for IIS Classic Mode");

                    //Get the appropriate section of the Config File
                    XmlNodeList systemWebNodes = config.DocumentElement.GetElementsByTagName("system.web");
                    XmlElement systemWeb;
                    if (systemWebNodes.Count == 0)
                    {
                        systemWeb = config.CreateElement("system.web");
                        config.DocumentElement.AppendChild(systemWeb);
                    }
                    else if (systemWebNodes.Count > 1)
                    {
                        Console.Error.WriteLine("rdfWebDeploy: Error: The Configuration File for the Web Application appears to be invalid as more than one <system.web> node exists");
                        return;
                    }
                    else
                    {
                        systemWeb = (XmlElement)systemWebNodes[0];
                    }

                    XmlNodeList httpHandlersNodes = systemWeb.GetElementsByTagName("httpHandlers");
                    XmlElement httpHandlers;
                    if (httpHandlersNodes.Count == 0)
                    {
                        httpHandlers = config.CreateElement("httpHandlers");
                        systemWeb.AppendChild(httpHandlers);
                    }
                    else if (httpHandlersNodes.Count > 1)
                    {
                        Console.Error.WriteLine("rdfWebDeploy: Error: The Configuration File for the Web Application appears to be invalid as more than one <httpHandlers> node exists");
                        return;
                    }
                    else
                    {
                        httpHandlers = (XmlElement)httpHandlersNodes[0];
                    }

                    foreach (INode n in g.GetTriplesWithPredicateObject(rdfType, httpHandlerType).Select(t => t.Subject))
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

                                //Add XML to register the Handler
                                reg = null;
                                foreach (XmlNode existingReg in httpHandlers.ChildNodes)
                                {
                                    if (existingReg.Attributes.GetNamedItem("path") != null)
                                    {
                                        if (existingReg.Attributes["path"].Value.Equals(handlerPath))
                                        {
                                            reg = existingReg;
                                            break;
                                        }
                                    }
                                }
                                if (reg == null)
                                {
                                    reg = config.CreateElement("add");
                                    XmlAttribute attr = config.CreateAttribute("path");
                                    attr.Value = handlerPath;
                                    reg.Attributes.Append(attr);
                                    attr = config.CreateAttribute("verb");
                                    attr.Value = "*";
                                    reg.Attributes.Append(attr);
                                    attr = config.CreateAttribute("type");
                                    attr.Value = handlerType;
                                    reg.Attributes.Append(attr);
                                    httpHandlers.AppendChild(reg);
                                }
                                else
                                {
                                    reg.Attributes["type"].Value = handlerType;
                                }

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

                    //Deploy Negotiate by File Extension
                    if (this._negotiate)
                    {
                        XmlNodeList httpModulesNodes = systemWeb.GetElementsByTagName("httpModules");
                        XmlElement httpModules;
                        if (httpModulesNodes.Count == 0)
                        {
                            httpModules = config.CreateElement("httpModules");
                            systemWeb.AppendChild(httpModules);
                        }
                        else if (httpModulesNodes.Count > 1)
                        {
                            Console.Error.WriteLine("rdfWebDeploy: Error: The Configuration File for the Web Application appears to be invalid as more than one <httpModules> node exists");
                            return;
                        }
                        else
                        {
                            httpModules = (XmlElement)httpModulesNodes[0];
                        }
                        reg = null;
                        foreach (XmlNode existingReg in httpModules.ChildNodes)
                        {
                            if (existingReg.Attributes.GetNamedItem("name") != null)
                            {
                                if (existingReg.Attributes["name"].Value.Equals("NegotiateByExtension"))
                                {
                                    reg = existingReg;
                                    break;
                                }
                            }
                        }
                        if (reg == null)
                        {
                            reg = config.CreateElement("add");
                            XmlAttribute name = config.CreateAttribute("name");
                            name.Value = "NegotiateByExtension";
                            reg.Attributes.Append(name);
                            XmlAttribute type = config.CreateAttribute("type");
                            type.Value = "VDS.RDF.Web.NegotiateByFileExtension";
                            reg.Attributes.Append(type);
                            httpModules.AppendChild(reg);
                        }
                        Console.WriteLine("rdfWebDeploy: Deployed the Negotiate by File Extension Module");
                    }

                    config.Save(args[1]);
                    Console.WriteLine("rdfWebDeploy: Successfully deployed for IIS Classic Mode");
                }

                Console.WriteLine();

                //Deploy for IIS Integrated Mode
                if (!this._noIntegratedRegistration)
                {
                    Console.WriteLine("rdfWebDeploy: Attempting deployment for IIS Integrated Mode");

                    //Get the appropriate section of the Config File
                    XmlNodeList systemWebServerNodes = config.DocumentElement.GetElementsByTagName("system.webServer");
                    XmlElement systemWebServer;
                    if (systemWebServerNodes.Count == 0)
                    {
                        systemWebServer = config.CreateElement("system.webServer");
                        config.DocumentElement.AppendChild(systemWebServer);
                    }
                    else if (systemWebServerNodes.Count > 1)
                    {
                        Console.Error.WriteLine("rdfWebDeploy: Error: The Configuration File for the Web Application appears to be invalid as more than one <system.webServer> node exists");
                        return;
                    }
                    else
                    {
                        systemWebServer = (XmlElement)systemWebServerNodes[0];
                    }

                    //Set the <validation validateIntegratedModeConfiguration="false" /> element
                    XmlNodeList validateNodes = systemWebServer.GetElementsByTagName("validation");
                    if (validateNodes.Count > 0)
                    {
                        foreach (XmlNode validate in validateNodes)
                        {
                            if (validate.Attributes.GetNamedItem("validateIntegratedModeConfiguration") != null)
                            {
                                validate.Attributes["validateIntegratedModeConfiguration"].Value = "false";
                            }
                            else
                            {
                                XmlAttribute valAttr = config.CreateAttribute("validateIntegratedModeConfiguration");
                                valAttr.Value = "false";
                                validate.Attributes.Append(valAttr);
                            }
                        }
                    }
                    else
                    {
                        XmlElement valEl = config.CreateElement("validation");
                        XmlAttribute valAttr = config.CreateAttribute("validateIntegratedModeConfiguration");
                        valAttr.Value = "false";
                        valEl.Attributes.Append(valAttr);
                        systemWebServer.AppendChild(valEl);
                    }

                    //Find <handlers> element
                    XmlNodeList httpHandlersNodes = systemWebServer.GetElementsByTagName("handlers");
                    XmlElement httpHandlers;
                    if (httpHandlersNodes.Count == 0)
                    {
                        httpHandlers = config.CreateElement("handlers");
                        systemWebServer.AppendChild(httpHandlers);
                    }
                    else if (httpHandlersNodes.Count > 1)
                    {
                        Console.Error.WriteLine("rdfWebDeploy: Error: The Configuration File for the Web Application appears to be invalid as more than one <handlers> node exists");
                        return;
                    }
                    else
                    {
                        httpHandlers = (XmlElement)httpHandlersNodes[0];
                    }

                    foreach (INode n in g.GetTriplesWithPredicateObject(rdfType, httpHandlerType).Select(t => t.Subject))
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

                                //Add XML to register the Handler
                                reg = null;
                                foreach (XmlNode existingReg in httpHandlers.ChildNodes)
                                {
                                    if (existingReg.Attributes.GetNamedItem("path") != null)
                                    {
                                        if (existingReg.Attributes["path"].Value.Equals(handlerPath))
                                        {
                                            reg = existingReg;
                                            break;
                                        }
                                    }
                                }
                                if (reg == null)
                                {
                                    reg = config.CreateElement("add");
                                    XmlAttribute attr = config.CreateAttribute("name");
                                    attr.Value = handlerPath;
                                    reg.Attributes.Append(attr);                                        
                                    attr = config.CreateAttribute("path");
                                    attr.Value = handlerPath;
                                    reg.Attributes.Append(attr);
                                    attr = config.CreateAttribute("verb");
                                    attr.Value = "*";
                                    reg.Attributes.Append(attr);
                                    attr = config.CreateAttribute("type");
                                    attr.Value = handlerType;
                                    reg.Attributes.Append(attr);
                                    httpHandlers.AppendChild(reg);
                                }
                                else
                                {
                                    reg.Attributes["type"].Value = handlerType;
                                }

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

                    //Deploy Negotiate by File Extension
                    if (this._negotiate)
                    {
                        XmlNodeList httpModulesNodes = systemWebServer.GetElementsByTagName("modules");
                        XmlElement httpModules;
                        if (httpModulesNodes.Count == 0)
                        {
                            httpModules = config.CreateElement("modules");
                            systemWebServer.AppendChild(httpModules);
                        }
                        else if (httpModulesNodes.Count > 1)
                        {
                            Console.Error.WriteLine("rdfWebDeploy: Error: The Configuration File for the Web Application appears to be invalid as more than one <modules> node exists");
                            return;
                        }
                        else
                        {
                            httpModules = (XmlElement)httpModulesNodes[0];
                        }
                        reg = null;
                        foreach (XmlNode existingReg in httpModules.ChildNodes)
                        {
                            if (existingReg.Attributes.GetNamedItem("name") != null)
                            {
                                if (existingReg.Attributes["name"].Value.Equals("NegotiateByExtension"))
                                {
                                    reg = existingReg;
                                    break;
                                }
                            }
                        }
                        if (reg == null)
                        {
                            reg = config.CreateElement("add");
                            XmlAttribute name = config.CreateAttribute("name");
                            name.Value = "NegotiateByExtension";
                            reg.Attributes.Append(name);
                            XmlAttribute type = config.CreateAttribute("type");
                            type.Value = "VDS.RDF.Web.NegotiateByFileExtension";
                            reg.Attributes.Append(type);
                            httpModules.AppendChild(reg);
                        }
                        Console.WriteLine("rdfWebDeploy: Deployed the Negotiate by File Extension Module");
                    }

                    config.Save(args[1]);
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
                        Console.WriteLine("rdfWebDeploy: IIS Integrated Mode Handler registration will not be performed");
                        break;
                    case "-noclassicreg":
                        this._noClassicRegistration = true;
                        Console.WriteLine("rdfWebDeploy: IIS Classic Handler registration will not be performed");
                        break;
                    case "-noiis":
                        break;
                    default:
                        Console.Error.WriteLine("rdfWebDeploy: Error: " + args[i] + " is not a known option for the -xmldeploy mode of this tool");
                        return false;
                }
                i++;
            }

            return true;
        }
    }
}
