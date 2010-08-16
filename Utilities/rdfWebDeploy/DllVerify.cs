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
using System.Linq;
using System.Configuration;
using System.Web.Configuration;
using System.IO;

namespace rdfWebDeploy
{
    class DllVerify
    {
        private bool _noLocalIIS = false;
        private String _site = "Default Web Site";

        public void Verify(String[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: 2 Arguments are required in order to use the -dllverify mode");
                return;
            }
            if (args.Length > 2)
            {
                if (!this.SetOptions(args.Skip(2).ToArray()))
                {
                    Console.Error.WriteLine("rdfWebDeploy: DLL Verify aborted since one/more options were not valid");
                    return;
                }
            }

            String appFolder;
            if (!this._noLocalIIS)
            {
                //Open the Configuration File
                Configuration config = WebConfigurationManager.OpenWebConfiguration(args[1], this._site);
                Console.Out.WriteLine("rdfWebDeploy: Opened the Web.Config file for the specified Web Application");

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

            //Verify all required DLLs are in the bin directory of the application
            foreach (String dll in RdfWebDeployHelper.RequiredDLLs)
            {
                if (!File.Exists(Path.Combine(binFolder, dll)))
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: Required DLL " + dll + " which needs deploying to the web applications bin directory is not present");
                    return;
                }
            }

            Console.WriteLine("rdfWebDeploy: OK - All required DLLs are present");
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
                    default:
                        Console.Error.WriteLine("rdfWebDeploy: Error: " + args[i] + " is not a known option for the -dllverify mode of this tool");
                        return false;
                }
                i++;
            }
            return true;
        }
    }
}
