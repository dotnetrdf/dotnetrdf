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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Utilities.Web.Deploy
{
    public class List
    {
        private const String ListHandlers = "SELECT * WHERE { ?handler a dnr:HttpHandler ; dnr:type ?type . OPTIONAL { ?handler rdfs:label ?label } }";

        public void RunList(String[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: 2 Arguments are required in order to use the -list mode");
                return;
            }

            if (!File.Exists(args[1]))
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot list handlers from " + args[1] + " since the file does not exist");
                return;
            }

            Graph g = new Graph();
            try
            {
                FileLoader.Load(g, args[1]);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot parse the configuration file");
                Console.Error.WriteLine("rdfWebDeploy: Error: " + ex.Message);
                return;
            }

            Object results = g.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + ListHandlers);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                Console.WriteLine("rdfWebDeploy: Configuration file contains " + rset.Count + " handler registrations");

                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine();
                    Console.WriteLine("rdfWebDeploy: Handler URI: " + r["handler"].ToString());
                    Console.WriteLine("rdfWebDeploy: Handler Type: " + r["type"].ToString());
                    if (r.HasValue("label"))
                    {
                        Console.WriteLine("rdfWebDeploy: Label: " + r["label"].ToString());
                    }
                }
            }
        }
    }
}
