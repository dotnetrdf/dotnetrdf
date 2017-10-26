using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    internal static class VariableClassificationExtensions
    {
        internal static PatternItem ToPatternItem(this INode n)
        {
            if (n.NodeType == NodeType.Blank) return new BlankNodePattern(((IBlankNode) n).InternalID);
            if (n.NodeType == NodeType.Variable) return new VariablePattern(((IVariableNode)n).VariableName);
            return new NodeMatchPattern(n);
        }
    }

    /// <summary>
    /// Tests that the <see cref="ISparqlAlgebra.FixedVariables"/> and <see cref="ISparqlAlgebra.FloatingVariables"/> are implemented correctly
    /// </summary>

    public class VariableClassificationTests
    {
        private readonly INodeFactory _factory = new NodeFactory();
        private readonly List<String> _emptyList = new List<String>();
        private readonly INode _rdfType, _s, _p, _o;

        public VariableClassificationTests()
        {
            this._rdfType = this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            this._s = this._factory.CreateUriNode(new Uri("http://s"));
            this._p = this._factory.CreateUriNode(new Uri("http://p"));
            this._o = this._factory.CreateUriNode(new Uri("http://o"));
        }

        private TriplePattern MakeTriplePattern(INode s, INode p, INode o)
        {
            return new TriplePattern(s.ToPatternItem(), p.ToPatternItem(), o.ToPatternItem());
        }

        private void TestClassification(ISparqlAlgebra algebra, IEnumerable<String> expectedFixed, IEnumerable<String> expectedFloating)
        {
            this.TestClassification(algebra, expectedFixed.ToList(), expectedFloating.ToList());
        }

        private void TestClassification(ISparqlAlgebra algebra, List<String> expectedFixed, List<String> expectedFloating)
        {
            expectedFixed.Sort();
            expectedFloating.Sort();
            List<String> actualFixed = algebra.FixedVariables.ToList();
            actualFixed.Sort();
            List<String> actualFloating = algebra.FloatingVariables.ToList();
            actualFloating.Sort();

            Console.WriteLine("Expected Fixed: " + String.Join(",", expectedFixed));
            Console.WriteLine("Actual Fixed: " + String.Join(",", actualFixed));
            Console.WriteLine("Expected Floating: " + String.Join(",", expectedFloating));
            Console.WriteLine("Actual Floating: " + String.Join(",", actualFloating));
            Console.WriteLine();

            // Check fixed and floating are correct
            Assert.Equal(expectedFixed, actualFixed);
            Assert.Equal(expectedFloating, actualFloating);

            // A variable can't be both fixed and floating
            Assert.True(actualFixed.All(v => !actualFloating.Contains(v)));
            Assert.True(actualFloating.All(v => !actualFixed.Contains(v)));
        }

        [Fact]
        public void SparqlAlgebraVariableClassification1()
        {
            TriplePattern tp = MakeTriplePattern(this._s, this._p, this._o);
            IBgp bgp = new Bgp(tp);
            this.TestClassification(bgp, this._emptyList, this._emptyList);
        }

        [Fact]
        public void SparqlAlgebraVariableClassification2()
        {
            TriplePattern tp = MakeTriplePattern(this._factory.CreateVariableNode("s"), this._p, this._o);
            IBgp bgp = new Bgp(tp);
            this.TestClassification(bgp, new String[] { "s" }, this._emptyList);
        }

        [Fact]
        public void SparqlAlgebraVariableClassification3()
        {
            TriplePattern tp = MakeTriplePattern(this._factory.CreateVariableNode("s"), this._rdfType, this._factory.CreateVariableNode("type"));
            IBgp lhs = new Bgp(tp);
            this.TestClassification(lhs, new String[] { "s", "type" }, this._emptyList);

            tp = MakeTriplePattern(this._factory.CreateVariableNode("s"), this._factory.CreateVariableNode("p"), this._factory.CreateVariableNode("o"));
            IBgp rhs = new Bgp(tp);
            this.TestClassification(rhs, new String[] { "s", "p", "o"}, this._emptyList);

            // In a join everything should end up fixed since everything started as fixed
            IJoin join = new Join(lhs, rhs);
            this.TestClassification(join, new String[] { "s", "type", "p", "o"}, this._emptyList);

            // In the left join only the LHS variables should be fixed, others should be floating
            ILeftJoin leftJoin = new LeftJoin(lhs, rhs);
            this.TestClassification(leftJoin, new String[] { "s", "type"}, new String[] { "p", "o"});
            leftJoin = new LeftJoin(rhs, lhs);
            this.TestClassification(leftJoin, new String[] { "s", "p", "o" }, new String[] { "type" });

            // In the union only fixed variables on both sides are fixed, others should be floating
            IUnion union = new Union(lhs, rhs);
            this.TestClassification(union, new String[] { "s"}, new String[] { "p", "o", "type"});
        }

        [Fact]
        public void SparqlAlgebraVariableClassification4()
        {
            TriplePattern tp = MakeTriplePattern(this._factory.CreateVariableNode("s"), this._rdfType, this._factory.CreateVariableNode("type"));
            IBgp lhs = new Bgp(tp);
            tp = MakeTriplePattern(this._factory.CreateVariableNode("s"), this._factory.CreateVariableNode("p"), this._factory.CreateVariableNode("o"));
            IBgp rhs = new Bgp(tp);

            // In the left join only the LHS variables should be fixed, others should be floating
            ILeftJoin leftJoin = new LeftJoin(lhs, rhs);
            this.TestClassification(leftJoin, new String[] { "s", "type" }, new String[] { "p", "o" });

            tp = MakeTriplePattern(this._factory.CreateVariableNode("s"), this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label")), this._factory.CreateVariableNode("label"));
            Bgp top = new Bgp(tp);

            // Everything in the RHS not fixed on the LHS is floating
            ILeftJoin parentJoin = new LeftJoin(top, leftJoin);
            this.TestClassification(parentJoin, new String[] { "s", "label"}, new String[] { "p", "o", "type"});

            parentJoin = new LeftJoin(leftJoin, top);
            this.TestClassification(parentJoin, new String[] { "s", "type" }, new String[] { "p", "o", "label" });
        }
    }

    /// <summary>
    /// Helper class for testing out floating and fixed variable classifications
    /// </summary>
    internal class FakeAlgebra : ISparqlAlgebra
    {
        public FakeAlgebra(IEnumerable<String> fixedVars, IEnumerable<String> floatingVars)
        {
            this.FixedVariables = fixedVars;
            this.FloatingVariables = floatingVars;
            this.Variables = this.FixedVariables.Concat(this.FloatingVariables).Distinct();
        }

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Variables { get; private set; }
        public IEnumerable<string> FloatingVariables { get; private set; }
        public IEnumerable<string> FixedVariables { get; private set; }

        public SparqlQuery ToQuery()
        {
            throw new NotImplementedException();
        }

        public GraphPattern ToGraphPattern()
        {
            throw new NotImplementedException();
        }
    }
}