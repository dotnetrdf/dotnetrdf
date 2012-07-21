/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Collections;
using System.Threading;
using VDS.Web;

namespace VDS.RDF.Utilities.Server
{
    /// <summary>
    /// Main Class for launching the Server
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point for the program which launches the server
        /// </summary>
        /// <param name="args">Command Line Arguments</param>
        static void Main(string[] args)
        {
            try
            {
                RdfServerOptions options = new RdfServerOptions(args);

                switch (options.Mode)
                {
                    case RdfServerConsoleMode.Run:
                        using (HttpServer server = options.GetServerInstance())
                        {
                            if (!options.QuietMode) Console.WriteLine("rdfServer: Running");
                            server.Start();
                            while (true)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        break;

                    case RdfServerConsoleMode.Quit:
                        return;
                }

                if (!options.QuietMode)
                {
                    Console.WriteLine("rdfServer: Finished - press any key to exit");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfServer: Error: An unexpected error occurred while trying to start up the Server.  See subsequent error messages for details:");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                while (ex.InnerException != null)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Inner Exception:");
                    Console.Error.WriteLine(ex.InnerException.Message);
                    Console.Error.WriteLine(ex.InnerException.StackTrace);
                    ex = ex.InnerException;
                }
            }
        }
    }
}
