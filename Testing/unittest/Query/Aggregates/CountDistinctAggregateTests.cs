using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Query.Aggregates
{
    [TestClass]
    public class CountDistinctAggregateTests
    {
        [TestMethod]
        public void CorrectlyAppliesDistinctWhenToStringCalled()
        {
            // given
            var aggregate = new CountDistinctAggregate(new VariableTerm("var"));

            // when
            string aggregateString = aggregate.ToString();

            // then
            Assert.AreEqual("COUNT(DISTINCT ?var)", aggregateString);
        }

        [TestMethod]
        public void ContainsDistinctModifier()
        {
            // given
            var term = new VariableTerm("var");
            var aggregate = new CountDistinctAggregate(term);

            // when
            var arguments = aggregate.Arguments.ToArray();

            // then
            Assert.AreEqual(2, arguments.Length);
            Assert.IsTrue(arguments[0] is DistinctModifier);
            Assert.AreSame(term, arguments[1]);
        }
    }
}