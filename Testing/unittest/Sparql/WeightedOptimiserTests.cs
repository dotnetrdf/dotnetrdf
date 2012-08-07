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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
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
                weightings.LoadFromEmbeddedResource("VDS.RDF.Test.Sparql.SampleWeightings.n3, dotNetRDF.Test");
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
                weightings.LoadFromEmbeddedResource("VDS.RDF.Test.Sparql.SampleWeightings.n3, dotNetRDF.Test");
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
                weightings.LoadFromEmbeddedResource("VDS.RDF.Test.Sparql.SampleWeightings.n3, dotNetRDF.Test");
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
                weightings.LoadFromEmbeddedResource("VDS.RDF.Test.Sparql.SampleWeightings.n3, dotNetRDF.Test");
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
                weightings.LoadFromEmbeddedResource("VDS.RDF.Test.Sparql.SampleWeightings.n3, dotNetRDF.Test");
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
                weightings.LoadFromEmbeddedResource("VDS.RDF.Test.Sparql.SampleWeightings.n3, dotNetRDF.Test");
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
                weightings.LoadFromEmbeddedResource("VDS.RDF.Test.Sparql.SampleWeightings.n3, dotNetRDF.Test");
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
                weightings.LoadFromEmbeddedResource("VDS.RDF.Test.Sparql.SampleWeightings.n3, dotNetRDF.Test");
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
