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
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.DateTime;
using VDS.RDF.Query.Operators.Numeric;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Configuration
{

    public class ConfigSerializationTests
    {
        [Fact]
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
                Assert.NotNull(resultOp);
                Assert.Equal(ops[i].GetType(), resultOp.GetType());
            }
        }
    }
}
