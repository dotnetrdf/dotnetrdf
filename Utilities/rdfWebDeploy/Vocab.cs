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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.Web.Deploy
{
    public class Vocab
    {
        public void RunVocab(String[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: 2 Arguments are required in order to use the -vocab mode");
                return;
            }

            if (File.Exists(args[1]))
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot output the configuration vocabulary to " + args[1] + " as a file already exists at that location");
                return;
            }

            TurtleParser ttlparser = new TurtleParser();
            StreamReader reader = new StreamReader(Assembly.GetAssembly(typeof(IGraph)).GetManifestResourceStream("VDS.RDF.Configuration.configuration.ttl"));
            Graph g = new Graph();
            ttlparser.Load(g, reader);

            IRdfWriter writer;
            try
            {
                writer = MimeTypesHelper.GetWriter(MimeTypesHelper.GetMimeType(MimeTypesHelper.GetTrueFileExtension(args[1])));
            }
            catch (RdfWriterSelectionException)
            {
                writer = new CompressingTurtleWriter(WriterCompressionLevel.High);
            }
            writer.Save(g, args[1]);
            Console.WriteLine("rdfWebDeploy: Configuration Vocabulary output to " + args[1]);
        }
    }
}
