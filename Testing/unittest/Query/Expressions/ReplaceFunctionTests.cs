using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Functions.XPath.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions
{
    [TestClass]
    public class ReplaceFunctionTests
    {
        [TestMethod]
        public void CanParalleliseShouldNotThrowWhenFindExpressionsIsConstant()
        {
            // when
            var find = new ConstantTerm(new StringNode(null, "find"));
            var replace = new VariableTerm("replacement");
            ReplaceFunction func = new ReplaceFunction(new VariableTerm("term"), find, replace);

            // then
            var canParallelise = func.CanParallelise;
        }

        [TestMethod]
        public void CanParalleliseShouldNotThrowWhenReplaceExpressionsIsConstant()
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