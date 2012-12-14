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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    [TestClass]
    public class WeightedOptimiserTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();

        [TestMethod]
        public void SparqlOptimiserQueryWeightedSimple()
        {
            try
            {
                String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s ?p ?o .
  ?s rdfs:label ?label .
}";

                Graph weightings = new Graph();
                weightings.LoadFromEmbeddedResource("VDS.RDF.Sparql.SampleWeightings.n3, dotNetRDF.Test");
                SparqlOptimiser.QueryOptimiser = new WeightedOptimiser(weightings);

                SparqlQuery q = this._parser.ParseFromString(query);

                Console.WriteLine(this._formatter.Format(q));

                Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0].IsAcceptAll, "First Triple Pattern should not be the ?s ?p ?o Pattern");
                Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].IsAcceptAll, "Second Triple Pattern should be the ?s ?p ?o pattern");
            }
            finally
            {
                SparqlOptimiser.ResetOptimisers();
            }
        }

        [TestMethod]
        public void SparqlOptimiserQueryWeightedSimple2()
        {
            try
            {
                String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s rdfs:label ?label .
  ?s rdfs:comment ?comment .
}";

                Graph weightings = new Graph();
                weightings.LoadFromEmbeddedResource("VDS.RDF.Sparql.SampleWeightings.n3, dotNetRDF.Test");
                SparqlOptimiser.QueryOptimiser = new WeightedOptimiser(weightings);

                SparqlQuery q = this._parser.ParseFromString(query);

                Console.WriteLine(this._formatter.Format(q));

                Assert.IsTrue(q.RootGraphPattern.TriplePatterns[0].Variables.Contains("comment"), "First Triple Pattern should contain ?comment");
                Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].Variables.Contains("label"), "Second Triple Pattern should contain ?label");
            }
            finally
            {
                SparqlOptimiser.ResetOptimisers();
            }
        }

        [TestMethod]
        public void SparqlOptimiserQueryWeightedSimple3()
        {
            try
            {
                String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s a ?type .
  ?s rdfs:label ?label .
}";

                Graph weightings = new Graph();
                weightings.LoadFromEmbeddedResource("VDS.RDF.Sparql.SampleWeightings.n3, dotNetRDF.Test");
                SparqlOptimiser.QueryOptimiser = new WeightedOptimiser(weightings);

                SparqlQuery q = this._parser.ParseFromString(query);

                Console.WriteLine(this._formatter.Format(q));

                Assert.IsTrue(q.RootGraphPattern.TriplePatterns[0].Variables.Contains("label"), "First Triple Pattern should contain ?label");
                Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].Variables.Contains("type"), "Second Triple Pattern should contain ?type");
            }
            finally
            {
                SparqlOptimiser.ResetOptimisers();
            }
        }

        [TestMethod]
        public void SparqlOptimiserQueryWeightedSimple4()
        {
            try
            {
                String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s a rdfs:Class .
  ?s rdfs:label 'example' .
}";

                Graph weightings = new Graph();
                weightings.LoadFromEmbeddedResource("VDS.RDF.Sparql.SampleWeightings.n3, dotNetRDF.Test");
                SparqlOptimiser.QueryOptimiser = new WeightedOptimiser(weightings);

                SparqlQuery q = this._parser.ParseFromString(query);

                Console.WriteLine(this._formatter.Format(q));

                Assert.IsTrue(((NodeMatchPattern)((IMatchTriplePattern)q.RootGraphPattern.TriplePatterns[0]).Object).Node.NodeType == NodeType.Literal, "First Triple Pattern should have object 'example'");
                Assert.IsTrue(((NodeMatchPattern)((IMatchTriplePattern)q.RootGraphPattern.TriplePatterns[1]).Object).Node.NodeType == NodeType.Uri, "Second Triple Pattern should have object rdfs:Class");

            }
            finally
            {
                SparqlOptimiser.ResetOptimisers();
            }
        }

        [TestMethod]
        public void SparqlOptimiserQueryWeightedSimple5()
        {
            try
            {
                String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s rdfs:label 'example' .
  ?s a rdfs:Class .
}";

                Graph weightings = new Graph();
                weightings.LoadFromEmbeddedResource("VDS.RDF.Sparql.SampleWeightings.n3, dotNetRDF.Test");
                SparqlOptimiser.QueryOptimiser = new WeightedOptimiser(weightings);

                SparqlQuery q = this._parser.ParseFromString(query);

                Console.WriteLine(this._formatter.Format(q));

                Assert.IsTrue(((NodeMatchPattern)((IMatchTriplePattern)q.RootGraphPattern.TriplePatterns[0]).Object).Node.NodeType == NodeType.Uri, "First Triple Pattern should have object rdfs:Class");
                Assert.IsTrue(((NodeMatchPattern)((IMatchTriplePattern)q.RootGraphPattern.TriplePatterns[1]).Object).Node.NodeType == NodeType.Literal, "Second Triple Pattern should have object 'example'");
            }
            finally
            {
                SparqlOptimiser.ResetOptimisers();
            }
        }

        [TestMethod]
        public void SparqlOptimiserQueryWeightedSimple6()
        {
            try
            {
                String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s rdfs:label 'example' .
  ?s a rdfs:Class .
  ?s rdfs:subClassOf rdfs:Class .
}";

                Graph weightings = new Graph();
                weightings.LoadFromEmbeddedResource("VDS.RDF.Sparql.SampleWeightings.n3, dotNetRDF.Test");
                SparqlOptimiser.QueryOptimiser = new WeightedOptimiser(weightings);

                SparqlQuery q = this._parser.ParseFromString(query);

                Console.WriteLine(this._formatter.Format(q));

                Assert.IsTrue(((NodeMatchPattern)((IMatchTriplePattern)q.RootGraphPattern.TriplePatterns[2]).Object).Node.NodeType == NodeType.Literal, "Third Triple Pattern should have object 'example'");
                Assert.IsTrue(((NodeMatchPattern)((IMatchTriplePattern)q.RootGraphPattern.TriplePatterns[1]).Object).Node.NodeType == NodeType.Uri, "Second Triple Pattern should have object rdfs:Class");
                Assert.IsTrue(((NodeMatchPattern)((IMatchTriplePattern)q.RootGraphPattern.TriplePatterns[0]).Object).Node.NodeType == NodeType.Uri, "First Triple Pattern should have object rdfs:Class");

            }
            finally
            {
                SparqlOptimiser.ResetOptimisers();
            }
        }

        [TestMethod]
        public void SparqlOptimiserQueryWeightedUnknowns()
        {
            try
            {
                String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s rdfs:comment 'Predicates are weighted less than subjects' .
  rdfs:comment rdfs:comment 'Subjects are weighted higher than predicates' .
}";

                Graph weightings = new Graph();
                weightings.LoadFromEmbeddedResource("VDS.RDF.Sparql.SampleWeightings.n3, dotNetRDF.Test");
                SparqlOptimiser.QueryOptimiser = new WeightedOptimiser(weightings);

                SparqlQuery q = this._parser.ParseFromString(query);

                Console.WriteLine(this._formatter.Format(q));

                Assert.IsFalse(q.RootGraphPattern.TriplePatterns[0].Variables.Contains("s"), "First Triple Pattern should not contain ?s");
                Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].Variables.Contains("s"), "Second Triple Pattern should contain ?s");
            }
            finally
            {
                SparqlOptimiser.ResetOptimisers();
            }
        }

        [TestMethod]
        public void SparqlOptimiserQueryWeightedUnknowns2()
        {
            try
            {
                String query = @"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
SELECT * WHERE
{
  ?s <http://weightings/PredicatesLast> ?o .
  <http://weightings/SubjectsFirst> ?p ?o .
  ?s ?p 'Objects In Middle' .
}";

                Graph weightings = new Graph();
                weightings.LoadFromEmbeddedResource("VDS.RDF.Sparql.SampleWeightings.n3, dotNetRDF.Test");
                SparqlOptimiser.QueryOptimiser = new WeightedOptimiser(weightings);

                SparqlQuery q = this._parser.ParseFromString(query);

                Console.WriteLine(this._formatter.Format(q));

                Assert.IsTrue(q.RootGraphPattern.TriplePatterns[0].Variables.Intersect(new String[] { "p", "o" }).Count() == 2, "First Triple Pattern should contain ?p and ?o");
                Assert.IsTrue(q.RootGraphPattern.TriplePatterns[1].Variables.Intersect(new String[] { "s", "p" }).Count() == 2, "Second Triple Pattern should contain ?s and ?p");
                Assert.IsTrue(q.RootGraphPattern.TriplePatterns[2].Variables.Intersect(new String[] { "s", "o" }).Count() == 2, "Second Triple Pattern should contain ?s and ?o");
            }
            finally
            {
                SparqlOptimiser.ResetOptimisers();
            }
        }
    }
}
