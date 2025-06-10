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

using System.Collections.Generic;
using Xunit;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.DateTime;
using VDS.RDF.Query.Operators.Numeric;

namespace VDS.RDF.Configuration;


public class ConfigSerializationTests
{
    [Fact]
    public void ConfigurationSerializationOperators()
    {
        var ops = new List<ISparqlOperator>
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

        var g = new Graph();
        var context = new ConfigurationSerializationContext(g);
        var nodes = new List<INode>();

        foreach (ISparqlOperator op in ops)
        {
            INode opNode = g.CreateBlankNode();
            context.NextSubject = opNode;
            nodes.Add(opNode);

            ((IConfigurationSerializable)op).SerializeConfiguration(context);
        }

        for (var i = 0; i < ops.Count; i++)
        {
            INode opNode = nodes[i];
            var resultOp = ConfigurationLoader.LoadObject(g, opNode) as ISparqlOperator;
            Assert.NotNull(resultOp);
            Assert.Equal(ops[i].GetType(), resultOp.GetType());
        }
    }
}
