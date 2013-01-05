using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Functions.XPath.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions
{
    [TestClass]
    public class ReplaceFunctionTests
    {
        [TestMethod]
        public void SparqlParsingReplaceExpression()
        {
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT (REPLACE(?term, 'find', 'replace') AS ?test) { }");

            ISparqlExpression expr = q.Variables.First().Projection;
            Assert.IsInstanceOfType(expr, typeof(VDS.RDF.Query.Expressions.Functions.Sparql.String.ReplaceFunction));
        }

        [TestMethod]
        public void SparqlExpressionsXPathReplaceNullInCanParallelise1()
        {
            // when
            var find = new ConstantTerm(new StringNode(null, "find"));
            var replace = new VariableTerm("replacement");
            ReplaceFunction func = new ReplaceFunction(new VariableTerm("term"), find, replace);

            // then
            var canParallelise = func.CanParallelise;
        }

        [TestMethod]
        public void SparqlExpressionsXPathReplaceNullInCanParallelise2()
        {
            // when
            var find = new VariableTerm("find");
            var replace = new ConstantTerm(new StringNode(null, "replacement"));
            ReplaceFunction func = new ReplaceFunction(new VariableTerm("term"), find, replace);

            // then
            var canParallelise = func.CanParallelise;
        }
    }
}