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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
 
    [TestClass]
    public class PropertyPathTransformationTests
    {
        private NodeFactory _factory = new NodeFactory();
        private SparqlFormatter _formatter = new SparqlFormatter();

        private void RunTest(ISparqlPath path, IEnumerable<String> expectedOperators)
        {
            VariablePattern x = new VariablePattern("?x");
            VariablePattern y = new VariablePattern("?y");
            PathTransformContext context = new PathTransformContext(x, y);

            Console.WriteLine("Path: " + path.ToString());

            ISparqlAlgebra algebra = path.ToAlgebra(context);
            String result = algebra.ToString();
            Console.WriteLine("Algebra: " + result);

            try
            {
                GraphPattern gp = algebra.ToGraphPattern();
                Console.WriteLine("GraphPattern:");
                Console.WriteLine(this._formatter.Format(gp));
                Console.WriteLine();
            }
            catch
            {
                Console.WriteLine("Algebra not translatable to a GraphPattern");
            }

            foreach (String op in expectedOperators)
            {
                if (!result.Contains(op))
                {
                    Console.WriteLine("Expected Operator '" + op + "' missing");
                    Assert.Fail("Expected Operator '" + op + "' missing");
                }
            }
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationProperty()
        {
            this.RunTest(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationInverse()
        {
            this.RunTest(new InversePath(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)))), new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationSequence()
        {
            SequencePath path = new SequencePath(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            this.RunTest(path, new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationAlternative()
        {
            AlternativePath path = new AlternativePath(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"))), new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "comment"))));
            this.RunTest(path, new String[] { "BGP", "Union" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationOptional()
        {
            ZeroOrOne path = new ZeroOrOne(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))));
            this.RunTest(path, new String[] { "BGP", "ZeroLengthPath" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationZeroOrMore()
        {
            ZeroOrMore path = new ZeroOrMore(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))));
            this.RunTest(path, new String[] { "ZeroOrMorePath" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationOneOrMore()
        {
            OneOrMore path = new OneOrMore(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))));
            this.RunTest(path, new String[] { "OneOrMorePath" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationFixed1()
        {
            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1);
            this.RunTest(path, new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationFixed2()
        {
            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 2);
            this.RunTest(path, new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationFixed10()
        {
            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 10);
            this.RunTest(path, new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationVariable1To2()
        {
            NToM path = new NToM(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1, 2);
            this.RunTest(path, new String[] { "BGP", "Union" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationVariable1To10()
        {
            NToM path = new NToM(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1, 10);
            this.RunTest(path, new String[] { "BGP", "Union" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationVariable1To1()
        {
            NToM path = new NToM(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1, 1);
            this.RunTest(path, new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationVariable3To7()
        {
            NToM path = new NToM(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 3, 7);
            this.RunTest(path, new String[] { "BGP", "Union" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationNOrMore1()
        {
            NOrMore path = new NOrMore(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1);
            this.RunTest(path, new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationNOrMore0()
        {
            NOrMore path = new NOrMore(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            this.RunTest(path, new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationZeroToN1()
        {
            ZeroToN path = new ZeroToN(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1);
            this.RunTest(path, new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationNegatedPropertySet()
        {
            VDS.RDF.Query.Paths.NegatedSet path = new VDS.RDF.Query.Paths.NegatedSet(new Property[] { new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))) }, Enumerable.Empty<Property>());
            this.RunTest(path, new String[] { "NegatedPropertySet" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationNegatedPropertyInverseSet()
        {
            NegatedSet path = new NegatedSet(Enumerable.Empty<Property>(), new Property[] { new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))) });
            this.RunTest(path, new String[] { "NegatedPropertySet" });
        }

        [TestMethod]
        public void SparqlPropertyPathTransformationSequencedAlternatives()
        {
            INode a = this._factory.CreateUriNode(new Uri("ex:a"));
            INode b = this._factory.CreateUriNode(new Uri("ex:b"));
            INode c = this._factory.CreateUriNode(new Uri("ex:c"));
            INode d = this._factory.CreateUriNode(new Uri("ex:d"));
            SequencePath path = new SequencePath(new AlternativePath(new Property(a), new Property(c)), new AlternativePath(new Property(b), new Property(d)));
            this.RunTest(path, new String[] { "BGP" });
        }
    }
}
