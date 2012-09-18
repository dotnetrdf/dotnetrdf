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
