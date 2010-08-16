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
using System.IO;
using System.Reflection;
using VDS.RDF;
using VDS.RDF.Configuration;

namespace rdfWebDeploy
{
    public enum WebDeployMode
    {
        Deploy,
        Extract,
        Help
    }

    public static class RdfWebDeployHelper
    {
        public const String NamespacePrefixes = "PREFIX rdf: <" + NamespaceMapper.RDF + "> PREFIX rdfs: <" + NamespaceMapper.RDFS + "> PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> PREFIX fn: <" + VDS.RDF.Query.Expressions.XPathFunctionFactory.XPathFunctionsNamespace + "> PREFIX dnr: <" + ConfigurationLoader.ConfigurationNamespace + ">";

        private static List<String> _requiredDLLs = new List<string>()
        {
            "dotNetRDF.dll",
            "virtado3.dll",
            "HtmlAgilityPack.dll",
            "Newtonsoft.Json.dll",
            "MySql.Data.dll"
        };

        public static IEnumerable<String> RequiredDLLs
        {
            get
            {
                return _requiredDLLs;
            }
        }

        public static String ExecutablePath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
    }

    public class RdfWebDeploy
    {
        public void RunDeployment(String[] args)
        {
            if (args.Length == 0)
            {
                this.ShowUsage();
            }
            else
            {
                switch (args[0])
                {
                    case "-deploy":
                        Deploy d = new Deploy();
                        d.RunDeploy(args);
                        break;
                    case "-extract":
                        Extract e = new Extract();
                        e.RunExtract(args);
                        break;
                    case "-dllupdate":
                        DllUpdate du = new DllUpdate();
                        du.RunDllUpdate(args);
                        break;
                    case "-dllverify":
                        DllVerify dv = new DllVerify();
                        dv.Verify(args);
                        break;
                    case "-test":
                        Test t = new Test();
                        t.RunTest(args);
                        break;
                    case "-list":
                        List l = new List();
                        l.RunList(args);
                        break;
                    case "-vocab":
                        Vocab v = new Vocab();
                        v.RunVocab(args);
                        break;
                    case "-help":
                        this.ShowUsage();
                        break;
                    case "-xmldeploy":
                        XmlDeploy x = new XmlDeploy();
                        x.RunXmlDeploy(args);
                        break;
                    default:
                        this.ShowUsage();
                        break;
                }
            }
        }


        private void ShowUsage()
        {
            Console.WriteLine("rdfWebDeploy Utility for dotNetRDF");
            Console.WriteLine("--------------------------------");
            Console.WriteLine();
            Console.WriteLine("Command usage is as follows:");
            Console.WriteLine("rdfWebDeploy mode [options]");
            Console.WriteLine();
            Console.WriteLine("e.g. rdfWebDeploy -deploy /demos config.ttl");
            Console.WriteLine("e.g. rdfWebDeploy -extract /demos config.ttl");
            Console.WriteLine("e.g. rdfWebDeploy -dllverify /demos");
            Console.WriteLine();
            Console.WriteLine("Notes");
            Console.WriteLine("-----");
            Console.WriteLine();
            Console.WriteLine("All modes which support the webapp parameter specify it as the virtual path for the parameter on your local IIS instance, if you don't have a local IIS instance specify a path to the root directory of your web application and specify the -noiis option as an additional command line argument");
            Console.WriteLine();
            Console.WriteLine("Supported Modes");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine("-deploy webapp config.ttl [options]");
            Console.WriteLine(" Automatically deploys the given configuration file to the given web applications by setting up it's Web.Config file appropriately and deploying necessary DLLs.");
            Console.WriteLine();
            Console.WriteLine("-dllupdate webapp [options]");
            Console.WriteLine(" Updates all the required DLLs in the applications bin directory to the versions in the toolkits directory.");
            Console.WriteLine();
            Console.WriteLine("-dllverify webapp [options]");
            Console.WriteLine(" Verifies whether the required DLLs are present in the applications bin directory");
            Console.WriteLine();
            //Console.WriteLine("-extract webapp config.ttl [options]");
            //Console.WriteLine(" Generates an outline configuration file based on the given web applications Web.Config file.  Extraction will not overwrite any existing files");
            //Console.WriteLine();
            Console.WriteLine("-help");
            Console.WriteLine(" Shows this usage guide");
            Console.WriteLine();
            Console.WriteLine("-list config.ttl");
            Console.WriteLine(" Lists the Handlers in the given configuration file");
            Console.WriteLine();
            Console.WriteLine("-test config.ttl");
            Console.WriteLine(" Tests whether a configuration file parses and makes various tests for validity");
            Console.WriteLine();
            Console.WriteLine("-vocab file.ttl");
            Console.WriteLine(" Outputs the Configuration Vocabulary to the given file for use as a reference");
            Console.WriteLine();
            Console.WriteLine("-xmldeploy web.config config.ttl [options]");
            Console.WriteLine(" Automatically deploys the given configuration file to the given web applications by setting up it's Web.Config file appropriately and deploying necessary DLLs");
            Console.WriteLine();
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine("-nointreg");
            Console.WriteLine(" If specified then Handlers will not be registered for IIS Integrated Mode.  Used by -deploy and -xmldeploy");
            Console.WriteLine();
            Console.WriteLine("-noclassicreg");
            Console.WriteLine(" If specified then Handlers will not be registered for IIS Classic Mode.  Used by -deploy and -xmldeploy");
            Console.WriteLine();
            Console.WriteLine("-noiis");
            Console.WriteLine(" If specified indicates that there is not a local IIS instance available or you wish to deploy to a web application which is not associated with your local IIS instance.  Essentially forces -deploy mode to switch to -xmldeploy mode.  Supported by all modes that take the webapp parameter");
            Console.WriteLine();
            Console.WriteLine("-site \"Site Name\"");
            Console.WriteLine(" Specifies the IIS site in which the web application resides.  Supported by all modes that take the webapp parameter");
        }
    }
}
