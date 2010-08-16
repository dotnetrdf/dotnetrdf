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
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace rdfWebDeploy
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
