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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    public class PropertyPathEvaluationTests
    {
        private NodeFactory _factory = new NodeFactory();
        private ISparqlDataset _data;

        private ISparqlAlgebra GetAlgebra(ISparqlPath path)
        {
            return GetAlgebra(path, null, null);
        }

        private ISparqlAlgebra GetAlgebra(ISparqlPath path, INode start, INode end)
        {
            PatternItem x, y;
            if (start == null)
            {
                x = new VariablePattern("?x");
            }
            else
            {
                x = new NodeMatchPattern(start);
            }
            if (end == null)
            {
                y = new VariablePattern("?y");
            }
            else
            {
                y = new NodeMatchPattern(end);
            }
            PathTransformContext context = new PathTransformContext(x, y);
            return path.ToAlgebra(context);
        }

        private ISparqlAlgebra GetAlgebraUntransformed(ISparqlPath path)
        {
            return this.GetAlgebraUntransformed(path, null, null);
        }

        private ISparqlAlgebra GetAlgebraUntransformed(ISparqlPath path, INode start, INode end)
        {
            PatternItem x, y;
            if (start == null)
            {
                x = new VariablePattern("?x");
            }
            else
            {
                x = new NodeMatchPattern(start);
            }
            if (end == null)
            {
                y = new VariablePattern("?y");
            }
            else
            {
                y = new NodeMatchPattern(end);
            }
            return new Bgp(new PropertyPathPattern(x, path, y));
        }

        private void EnsureTestData()
        {
            if (this._data == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                store.Add(g);
                this._data = new InMemoryDataset(store, g.BaseUri);
            }
        }

        [Fact]
        public void SparqlPropertyPathEvaluationZeroLength()
        {
            EnsureTestData();

            FixedCardinality path =
                new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationZeroLengthWithTermEnd()
        {
            EnsureTestData();

            FixedCardinality path =
                new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            INode rdfsClass = this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, null, rdfsClass);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
            Assert.Equal(1, results.Count);
            Assert.Equal(rdfsClass, results[1]["x"]);
        }

        [Fact]
        public void SparqlPropertyPathEvaluationZeroLengthWithTermStart()
        {
            EnsureTestData();

            FixedCardinality path =
                new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path,
                                                     new Graph().CreateUriNode(
                                                         UriFactory.Create(ConfigurationLoader.ClassHttpHandler)), null);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationZeroLengthWithBothTerms()
        {
            EnsureTestData();

            FixedCardinality path =
                new FixedCardinality(new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType))), 0);
            ISparqlAlgebra algebra = this.GetAlgebra(path,
                                                     new Graph().CreateUriNode(
                                                         UriFactory.Create(ConfigurationLoader.ClassHttpHandler)),
                                                     this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")));
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.True(results.IsEmpty, "Results should  be empty");
            Assert.True(results is NullMultiset, "Results should be Null");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationNegatedPropertySet()
        {
            EnsureTestData();

            NegatedSet path =
                new NegatedSet(
                    new Property[] {new Property(this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)))},
                    Enumerable.Empty<Property>());
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationInverseNegatedPropertySet()
        {
            EnsureTestData();

            NegatedSet path = new NegatedSet(Enumerable.Empty<Property>(),
                                             new Property[]
                                                 {
                                                     new Property(
                                                 this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)))
                                                 });
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationSequencedAlternatives()
        {
            EnsureTestData();

            INode a = this._factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode b = this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "range"));
            SequencePath path = new SequencePath(new AlternativePath(new Property(a), new Property(b)),
                                                 new AlternativePath(new Property(a), new Property(a)));
            ISparqlAlgebra algebra = this.GetAlgebraUntransformed(path);
            SparqlEvaluationContext context = new SparqlEvaluationContext(null, this._data);
            BaseMultiset results = algebra.Evaluate(context);

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationOneOrMorePath()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store, g.BaseUri);

            OneOrMore path =
                new OneOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationOneOrMorePathForward()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store, g.BaseUri);

            OneOrMore path =
                new OneOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            INode sportsCar = this._factory.CreateUriNode(new Uri("http://example.org/vehicles/SportsCar"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, sportsCar, null);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationOneOrMorePathReverse()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store, g.BaseUri);

            OneOrMore path =
                new OneOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            INode airVehicle = this._factory.CreateUriNode(new Uri("http://example.org/vehicles/AirVehicle"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, null, airVehicle);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationZeroOrMorePath()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store, g.BaseUri);

            ZeroOrMore path =
                new ZeroOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            ISparqlAlgebra algebra = this.GetAlgebra(path);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationZeroOrMorePathForward()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            ZeroOrMore path =
                new ZeroOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            INode sportsCar = this._factory.CreateUriNode(new Uri("http://example.org/vehicles/SportsCar"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, sportsCar, null);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationZeroOrMorePathReverse()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            ZeroOrMore path =
                new ZeroOrMore(new Property(this._factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "subClassOf"))));
            INode airVehicle = this._factory.CreateUriNode(new Uri("http://example.org/vehicles/AirVehicle"));
            ISparqlAlgebra algebra = this.GetAlgebra(path, null, airVehicle);
            BaseMultiset results = algebra.Evaluate(new SparqlEvaluationContext(null, dataset));

            TestTools.ShowMultiset(results);

            Assert.False(results.IsEmpty, "Results should not be empty");
        }

        [Fact]
        public void SparqlPropertyPathEvaluationGraphInteraction()
        {
            String query = @"PREFIX ex: <http://www.example.org/schema#>
PREFIX in: <http://www.example.org/instance#>

SELECT ?x
FROM NAMED <http://example/1>
FROM NAMED <http://example/2>
WHERE
{
  GRAPH ?g { in:a ex:p1 / ex:p2 ?x . }
}";

            String data =
                @"<http://www.example.org/instance#a> <http://www.example.org/schema#p1> <http://www.example.org/instance#b> <http://example/1> .
<http://www.example.org/instance#b> <http://www.example.org/schema#p2> <http://www.example.org/instance#c> <http://example/2> .";

            TripleStore store = new TripleStore();
            store.LoadFromString(data, new NQuadsParser());

            SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
            Assert.Equal(0, results.Results.Count);
        }

        [Fact]
        public void SparqlPropertyPathEvaluationDuplicates()
        {
            IGraph g = new Graph();
            g.LoadFromFile("resources\\schema-org.ttl");

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromFile("resources\\schema-org.rq");
            SparqlQuery qDistinct = parser.ParseFromFile("resources\\schema-org.rq");
            qDistinct.QueryType = SparqlQueryType.SelectDistinct;

            InMemoryDataset dataset = new InMemoryDataset(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.False(results.IsEmpty);
            SparqlResultSet resultsDistinct = processor.ProcessQuery(qDistinct) as SparqlResultSet;
            Assert.NotNull(resultsDistinct);
            Assert.False(resultsDistinct.IsEmpty);

            Assert.Equal(resultsDistinct.Count, results.Count);
        }

        [Fact]
        public void SparqlPropertyPathEvaluationCore349RigorousEvaluation()
        {
            try
            {
                Options.RigorousEvaluation = true;

                //Test case from CORE-349
                Graph g = new Graph();
                g.LoadFromFile(@"..\\resources\core-349.ttl");
                InMemoryDataset dataset = new InMemoryDataset(g);

                String query = @"SELECT * WHERE 
{ 
  ?subject <http://www.w3.org/2000/01/rdf-schema#label> ?name .
  ?subject <http://www.w3.org/1999/02/22-rdf-syntax-ns#type>/<http://www.w3.org/2000/01/rdf-schema#subClassOf>* <http://example.org/unnamed#Level1_1> . 
?subject a ?type . } ";

                SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
                Console.WriteLine(new SparqlFormatter().Format(q));
                Console.WriteLine(q.ToAlgebra().ToString());
                LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);

                Console.WriteLine();
                TestTools.ShowResults(results);

                Assert.Equal(2, results.Count);
            }
            finally
            {
                Options.RigorousEvaluation = false;
            }
        }

        [Fact]
        public void SparqlPropertyPathEvaluationCore349NonRigorousEvaluation()
        {
            try
            {
                Options.RigorousEvaluation = false;

                //Test case from CORE-349
                Graph g = new Graph();
                g.LoadFromFile(@"..\\resources\core-349.ttl");
                InMemoryDataset dataset = new InMemoryDataset(g);

                String query = @"SELECT * WHERE 
{ 
  ?subject <http://www.w3.org/2000/01/rdf-schema#label> ?name .
  ?subject <http://www.w3.org/1999/02/22-rdf-syntax-ns#type>/<http://www.w3.org/2000/01/rdf-schema#subClassOf>* <http://example.org/unnamed#Level1_1> . 
?subject a ?type . } ";

                SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
                Console.WriteLine(new SparqlFormatter().Format(q));
                Console.WriteLine(q.ToAlgebra().ToString());
                LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);

                Console.WriteLine();
                TestTools.ShowResults(results);

                Assert.Equal(2, results.Count);
            }
            finally
            {
                Options.RigorousEvaluation = false;
            }
        }

        [Fact]
        public void SparqlPropertyPathEvaluationNonRigorous()
        {
            try
            {
                Graph g = new Graph();
                g.LoadFromFile(@"..\\resources\InferenceTest.ttl");
                InMemoryDataset dataset = new InMemoryDataset(g);

                String query = "SELECT * WHERE { ?subClass <http://www.w3.org/2000/01/rdf-schema#subClassOf>* ?class }";

                SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
                Console.WriteLine(new SparqlFormatter().Format(q));
                Console.WriteLine(q.ToAlgebra().ToString());
                LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);

                Console.WriteLine();
                TestTools.ShowResults(results);

                Assert.Equal(73, results.Count);
            }
            finally
            {
                Options.RigorousEvaluation = false;
            }
        }

#if !NO_FILE
        [Fact]
        public void SparqlPropertyPathEvaluationCore395ExactQuery()
        {
            IGraph g = new Graph();
            g.LoadFromFile(@"..\\resources/pp.rdf");

            InMemoryDataset dataset = new InMemoryDataset(g);
            SparqlQuery query = new SparqlQueryParser().ParseFromFile(@"..\\resources/pp.rq");
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            Object results = processor.ProcessQuery(query);
            Assert.NotNull(results);
            TestTools.ShowResults(results);

            Assert.IsType(typeof(SparqlResultSet), results);
            SparqlResultSet rset = (SparqlResultSet) results;
            Assert.Equal(3, rset.Count);
        }

        [Fact]
        public void SparqlPropertyPathEvaluationCore395ListQuery()
        {
            IGraph g = new Graph();
            g.LoadFromFile(@"..\\resources/pp.rdf");

            InMemoryDataset dataset = new InMemoryDataset(g);
            SparqlQuery query = new SparqlQueryParser().ParseFromString(@"
prefix rdfs:  <http://www.w3.org/2000/01/rdf-schema#> 
prefix owl:   <http://www.w3.org/2002/07/owl#> 
prefix rdf:   <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 

select ?superclass where {
  ?s owl:intersectionOf/rdf:rest*/rdf:first ?superclass .
  filter(!isBlank(?superclass))
}
");
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            Object results = processor.ProcessQuery(query);
            Assert.NotNull(results);
            TestTools.ShowResults(results);

            Assert.IsType(typeof(SparqlResultSet), results);
            SparqlResultSet rset = (SparqlResultSet)results;
            Assert.Equal(2, rset.Count);
        }

        [Fact]
        public void SparqlPropertyPathEvaluationCore441ZeroOrMorePath()
        {
            IGraph g = new Graph();
            g.LoadFromFile(@"..\\resources\core-441\data.ttl");

            InMemoryDataset dataset = new InMemoryDataset(g);
            SparqlQuery query = new SparqlQueryParser().ParseFromFile(@"..\\resources\core-441\star-path.rq");
            Console.WriteLine(query.ToAlgebra().ToString());

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            Object results = processor.ProcessQuery(query);
            Assert.NotNull(results);
            TestTools.ShowResults(results);

            Assert.IsType(typeof(SparqlResultSet), results);
            SparqlResultSet rset = (SparqlResultSet)results;
            Assert.Equal(1, rset.Count);
            Assert.Equal(g.CreateUriNode("Frame:Sheep"), rset[0]["prey"]);
        }

        [Fact]
        public void SparqlPropertyPathEvaluationCore441OneOrMorePath()
        {
            IGraph g = new Graph();
            g.LoadFromFile(@"..\\resources\core-441\data.ttl");

            InMemoryDataset dataset = new InMemoryDataset(g);
            SparqlQuery query = new SparqlQueryParser().ParseFromFile(@"..\\resources\core-441\plus-path.rq");
            Console.WriteLine(query.ToAlgebra().ToString());

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            Object results = processor.ProcessQuery(query);
            Assert.NotNull(results);
            TestTools.ShowResults(results);

            Assert.IsType(typeof(SparqlResultSet), results);
            SparqlResultSet rset = (SparqlResultSet)results;
            Assert.Equal(0, rset.Count);
        }

        [Fact]
        public void SparqlPropertyPathEvaluationCore441NoPath()
        {
            IGraph g = new Graph();
            g.LoadFromFile(@"..\\resources\core-441\data.ttl");

            InMemoryDataset dataset = new InMemoryDataset(g);
            SparqlQuery query = new SparqlQueryParser().ParseFromFile(@"..\\resources\core-441\no-path.rq");

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            Object results = processor.ProcessQuery(query);
            Assert.NotNull(results);
            TestTools.ShowResults(results);

            Assert.IsType(typeof(SparqlResultSet), results);
            SparqlResultSet rset = (SparqlResultSet)results;
            Assert.Equal(1, rset.Count);
            Assert.Equal(g.CreateUriNode("Frame:Sheep"), rset[0]["prey"]);
        }
#endif
    }
}
