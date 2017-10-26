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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
 

    public class PropertyPathTransformationTests
    {
        private readonly NodeFactory _factory = new NodeFactory();
        private readonly SparqlFormatter _formatter = new SparqlFormatter();

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
                if (result.Contains(op)) continue;
                Console.WriteLine("Expected Operator '" + op + "' missing");
                Assert.True(false, "Expected Operator '" + op + "' missing");
            }
        }

        [Fact]
        public void SparqlPropertyPathTransformationProperty()
        {
            this.RunTest(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationInverse()
        {
            this.RunTest(new InversePath(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)))), new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationSequence()
        {
            SequencePath path = new SequencePath(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            this.RunTest(path, new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationAlternative()
        {
            AlternativePath path = new AlternativePath(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"))), new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "comment"))));
            this.RunTest(path, new String[] { "BGP", "Union" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationOptional()
        {
            ZeroOrOne path = new ZeroOrOne(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))));
            this.RunTest(path, new String[] { "BGP", "ZeroLengthPath" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationZeroOrMore()
        {
            ZeroOrMore path = new ZeroOrMore(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))));
            this.RunTest(path, new String[] { "ZeroOrMorePath" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationOneOrMore()
        {
            OneOrMore path = new OneOrMore(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))));
            this.RunTest(path, new String[] { "OneOrMorePath" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationFixed1()
        {
            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1);
            this.RunTest(path, new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationFixed2()
        {
            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 2);
            this.RunTest(path, new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationFixed10()
        {
            FixedCardinality path = new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 10);
            this.RunTest(path, new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationVariable1To2()
        {
            NToM path = new NToM(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1, 2);
            this.RunTest(path, new String[] { "BGP", "Union" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationVariable1To10()
        {
            NToM path = new NToM(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1, 10);
            this.RunTest(path, new String[] { "BGP", "Union" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationVariable1To1()
        {
            NToM path = new NToM(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1, 1);
            this.RunTest(path, new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationVariable3To7()
        {
            NToM path = new NToM(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 3, 7);
            this.RunTest(path, new String[] { "BGP", "Union" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationNOrMore1()
        {
            NOrMore path = new NOrMore(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1);
            this.RunTest(path, new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationNOrMore0()
        {
            NOrMore path = new NOrMore(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            this.RunTest(path, new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationZeroToN1()
        {
            ZeroToN path = new ZeroToN(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 1);
            this.RunTest(path, new String[] { "BGP" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationNegatedPropertySet()
        {
            NegatedSet path = new NegatedSet(new Property[] { new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))) }, Enumerable.Empty<Property>());
            this.RunTest(path, new String[] { "NegatedPropertySet" });
        }

        [Fact]
        public void SparqlPropertyPathTransformationNegatedPropertyInverseSet()
        {
            NegatedSet path = new NegatedSet(Enumerable.Empty<Property>(), new Property[] { new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))) });
            this.RunTest(path, new String[] { "NegatedPropertySet" });
        }

        [Fact]
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
