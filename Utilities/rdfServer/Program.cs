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
