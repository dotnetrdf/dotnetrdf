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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.DateTime;
using VDS.RDF.Query.Operators.Numeric;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Test.Configuration
{
    [TestClass]
    public class ConfigSerializationTests
    {
        [TestMethod]
        public void ConfigurationSerializationOperators()
        {
            List<ISparqlOperator> ops = new List<ISparqlOperator>()
            {
                new AdditionOperator(),
                new DateTimeAddition(),
                new TimeSpanAddition(),
                new DivisionOperator(),
                new MultiplicationOperator(),
                new DivisionOperator(),
                new SubtractionOperator(),
                new DateTimeSubtraction(),
                new TimeSpanSubtraction()
            };

            Graph g = new Graph();
            ConfigurationSerializationContext context = new ConfigurationSerializationContext(g);
            List<INode> nodes = new List<INode>();

            foreach (ISparqlOperator op in ops)
            {
                INode opNode = g.CreateBlankNode();
                context.NextSubject = opNode;
                nodes.Add(opNode);

                ((IConfigurationSerializable)op).SerializeConfiguration(context);
            }

            for (int i = 0; i < ops.Count; i++)
            {
                INode opNode = nodes[i];
                ISparqlOperator resultOp = ConfigurationLoader.LoadObject(g, opNode) as ISparqlOperator;
                Assert.IsNotNull(resultOp, "Failed to load serialized operator " + ops[i].GetType().Name);
                Assert.AreEqual(ops[i].GetType(), resultOp.GetType());
            }
        }
    }
}
