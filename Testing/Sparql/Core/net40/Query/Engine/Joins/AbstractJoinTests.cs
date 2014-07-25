using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Collections;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine.Joins
{
    public abstract class AbstractJoinTests
    {
        /// <summary>
        /// Prepares a join enumerable for the kind of join being tested
        /// </summary>
        /// <param name="lhs">LHS</param>
        /// <param name="lhsVars"></param>
        /// <param name="rhs">RHS</param>
        /// <param name="rhsVars"></param>
        /// <returns>Join enumerable</returns>
        protected abstract IEnumerable<ISolution> MakeJoinEnumerable(IEnumerable<ISolution> lhs, IEnumerable<string> lhsVars, IEnumerable<ISolution> rhs, IEnumerable<string> rhsVars);

        /// <summary>
        /// Prepares expected results for the kind of join being tested
        /// </summary>
        /// <param name="lhs">LHS</param>
        /// <param name="lhsVars"></param>
        /// <param name="rhs">RHS</param>
        /// <param name="rhsVars"></param>
        /// <returns>Expected Results</returns>
        protected abstract IEnumerable<ISolution> MakeExpectedResults(IEnumerable<ISolution> lhs, IEnumerable<string> lhsVars, IEnumerable<ISolution> rhs, IEnumerable<string> rhsVars);

        public static IEnumerable<String> NoVariables { get { return Enumerable.Empty<String>(); }}
        
        [Test]
        public void EmptyLhs()
        {
            IEnumerable<ISolution> lhs = Enumerable.Empty<ISolution>();
            IEnumerable<ISolution> rhs = new Solution().AsEnumerable();

            EnumerableTests.Check(MakeExpectedResults(lhs, NoVariables, rhs, NoVariables), MakeJoinEnumerable(lhs, NoVariables, rhs, NoVariables));
        }

        [Test]
        public void EmptyRhs()
        {
            IEnumerable<ISolution> lhs = new Solution().AsEnumerable();
            IEnumerable<ISolution> rhs = Enumerable.Empty<ISolution>();

            EnumerableTests.Check(MakeExpectedResults(lhs, NoVariables, rhs, NoVariables), MakeJoinEnumerable(lhs, NoVariables, rhs, NoVariables));
        }

        [Test]
        public void NoVariables1()
        {
            IEnumerable<ISolution> lhs = new Solution().AsEnumerable();
            IEnumerable<ISolution> rhs = new Solution().AsEnumerable();

            EnumerableTests.Check(MakeExpectedResults(lhs, NoVariables, rhs, NoVariables), MakeJoinEnumerable(lhs, NoVariables, rhs, NoVariables));
        }

        [Test]
        public void SingleVariableNoCommon()
        {
            Solution x = new Solution();
            x.Add("x", new LiteralNode("x"));
            Solution y = new Solution();
            y.Add("y", new LiteralNode("y"));
            IEnumerable<ISolution> lhs = x.AsEnumerable();
            IEnumerable<ISolution> rhs = y.AsEnumerable();

            String[] lhsVars = {"x"};
            String[] rhsVars = {"y"};

            EnumerableTests.Check(MakeExpectedResults(lhs, lhsVars, rhs, rhsVars), MakeJoinEnumerable(lhs, lhsVars, rhs, rhsVars));
        }

        [Test]
        public void SingleVariableCommon1()
        {
            Solution x = new Solution();
            x.Add("x", new LiteralNode("x"));
            Solution y = new Solution();
            y.Add("x", new LiteralNode("x"));
            IEnumerable<ISolution> lhs = x.AsEnumerable();
            IEnumerable<ISolution> rhs = y.AsEnumerable();

            String[] vars = { "x" };

            EnumerableTests.Check(MakeExpectedResults(lhs, vars, rhs, vars), MakeJoinEnumerable(lhs, vars, rhs, vars));
        }

        [Test]
        public void SingleVariableCommon2()
        {
            Solution x = new Solution();
            x.Add("x", new LiteralNode("a"));
            Solution y = new Solution();
            y.Add("x", new LiteralNode("b"));
            IEnumerable<ISolution> lhs = x.AsEnumerable();
            IEnumerable<ISolution> rhs = y.AsEnumerable();

            String[] vars = { "x" };

            EnumerableTests.Check(MakeExpectedResults(lhs, vars, rhs, vars), MakeJoinEnumerable(lhs, vars, rhs, vars));
        }

        [Test]
        public void MultiVariableCommon1()
        {
            Solution x = new Solution();
            x.Add("x", new LiteralNode("a"));
            x.Add("y", new LiteralNode("1"));
            Solution y = new Solution();
            y.Add("x", new LiteralNode("b"));
            y.Add("y", new LiteralNode("1"));
            IEnumerable<ISolution> lhs = x.AsEnumerable();
            IEnumerable<ISolution> rhs = y.AsEnumerable();

            String[] vars = { "x", "y" };

            EnumerableTests.Check(MakeExpectedResults(lhs, vars, rhs, vars), MakeJoinEnumerable(lhs, vars, rhs, vars));
        }

        [Test]
        public void MultiVariableCommon2()
        {
            Solution x = new Solution();
            x.Add("x", new LiteralNode("a"));
            x.Add("y", new LiteralNode("1"));
            Solution y = new Solution();
            y.Add("x", new LiteralNode("b"));
            y.Add("y", new LiteralNode("2"));
            IEnumerable<ISolution> lhs = x.AsEnumerable();
            IEnumerable<ISolution> rhs = y.AsEnumerable();

            String[] vars = { "x", "y" };

            EnumerableTests.Check(MakeExpectedResults(lhs, vars, rhs, vars), MakeJoinEnumerable(lhs, vars, rhs, vars));
        }
    }
}