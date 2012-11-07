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
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using System.IO;

namespace VDS.RDF.Utilities.Web.Deploy
{
    class DllUpdate
    {
        private bool _noLocalIIS = false;
        private String _site = "Default Web Site";
        private bool _sql = false, _virtuoso = false, _fulltext = false;

        public void RunDllUpdate(String[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: 2 Arguments are required in order to use the -dllupdate mode");
                return;
            }
            if (args.Length > 2)
            {
                if (!this.SetOptions(args.Skip(2).ToArray()))
                {
                    Console.Error.WriteLine("rdfWebDeploy: DLL Update aborted since one/more options were not valid");
                    return;
                }
            }

            String appFolder;
            if (!this._noLocalIIS)
            {
                //Open the Configuration File
                System.Configuration.Configuration config = WebConfigurationManager.OpenWebConfiguration(args[1], this._site);
                Console.Out.WriteLine("rdfWebDeploy: Opened the Web.config file for the specified Web Application");

                appFolder = Path.GetDirectoryName(config.FilePath);
            }
            else
            {
                appFolder = Path.GetDirectoryName(args[1]);
            }

            //Detect Folders

            String binFolder = Path.Combine(appFolder, "bin\\");
            if (!Directory.Exists(binFolder))
            {
                Directory.CreateDirectory(binFolder);
                Console.WriteLine("rdfWebDeploy: Created a bin\\ directory for the web application");
            }

            //Copy all required DLLs are in the bin directory of the application
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
                    Console.WriteLine("rdfWebDeploy: Updated " + dll + " in the web applications bin directory");
                }
                else
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: Required DLL " + dll + " which needs deploying to the web applications bin directory could not be found");
                    return;
                }
            }

            Console.WriteLine("rdfWebDeploy: OK - All required DLLs are now up to date");
        }

        private bool SetOptions(String[] args)
        {
            int i = 0;
            while (i < args.Length)
            {
                switch (args[i])
                {
                    case "-noiis":
                        this._noLocalIIS = true;
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
                        Console.Error.WriteLine("rdfWebDeploy: Error: " + args[i] + " is not a known option for the -dllupdate mode of this tool");
                        return false;
                }
                i++;
            }
            return true;
        }
    }
}
