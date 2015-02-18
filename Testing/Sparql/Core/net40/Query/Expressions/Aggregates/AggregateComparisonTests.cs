using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Query.Expressions.Aggregates.Sparql;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Aggregates
{
    [TestFixture]
    public class AggregateComparisonTests
    {
        private void TestEquals(params IAggregateExpression[] aggs)
        {
            // Should be equal to itself
            foreach (IAggregateExpression agg in aggs)
            {
                Assert.AreEqual(agg, agg, "Should be equal to self");
                Assert.AreEqual(agg.GetHashCode(), agg.GetHashCode(), "Should have same hash code as self");
            }

            // Should be equal to all others
            for (int i = 0; i < aggs.Length; i++)
            {
                IAggregateExpression agg = aggs[i];
                for (int j = 0; j < aggs.Length; j++)
                {
                    if (i == j) continue;

                    Assert.AreEqual(agg, aggs[j], "Should be equal to other aggregates");
                    Assert.AreEqual(agg.GetHashCode(), aggs[j].GetHashCode(), "Should have same hash code as other aggregates");
                }
            }
        }

        private void TestNotEquals(params IAggregateExpression[] aggs)
        {
            // Should be equal to itself
            foreach (IAggregateExpression agg in aggs)
            {
                Assert.AreEqual(agg, agg, "Should be equal to self");
                Assert.AreEqual(agg.GetHashCode(), agg.GetHashCode(), "Should have same hash code as self");
            }

            // Should not be equal to all others
            for (int i = 0; i < aggs.Length; i++)
            {
                IAggregateExpression agg = aggs[i];
                for (int j = 0; j < aggs.Length; j++)
                {
                    if (i == j) continue;

                    Assert.AreNotEqual(agg, aggs[j], "Should not be equal to other aggregates");
                }
            }
        }

        [Test]
        public void AggregateEqualityCount()
        {
            IAggregateExpression[] aggs = new IAggregateExpression[]
                                          {
                                              new CountAggregate(new VariableTerm("x")), 
                                              new CountAggregate(new VariableTerm("x"))
                                          };
            TestEquals(aggs);
            aggs[1] = new CountAggregate(new VariableTerm("y"));
            TestNotEquals(aggs);
        }

        [Test]
        public void AggregateEqualityCountAll()
        {
            IAggregateExpression[] aggs = new IAggregateExpression[]
                                          {
                                              new CountAllAggregate(), 
                                              new CountAllAggregate()
                                          };
            TestEquals(aggs);
            aggs[1] = new CountAggregate(new VariableTerm("x"));
            TestNotEquals(aggs);
        }

        [Test]
        public void AggregateEqualityCountDistinct()
        {
            IAggregateExpression[] aggs = new IAggregateExpression[]
                                          {
                                              new CountDistinctAggregate(new VariableTerm("x")), 
                                              new CountDistinctAggregate(new VariableTerm("x"))
                                          };
            TestEquals(aggs);
            aggs[1] = new CountAggregate(new VariableTerm("x"));
            TestNotEquals(aggs);
        }

        [Test]
        public void AggregateEqualityCountAllDistinct()
        {
            IAggregateExpression[] aggs = new IAggregateExpression[]
                                          {
                                              new CountAllDistinctAggregate(), 
                                              new CountAllDistinctAggregate()
                                          };
            TestEquals(aggs);
            aggs[1] = new CountAggregate(new VariableTerm("x"));
            TestNotEquals(aggs);
        }
    }
}
