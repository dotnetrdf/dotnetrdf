/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;

namespace VDS.RDF.Shacl.Validation;

/// <summary>
/// Represents a SHACL validation report.
/// </summary>
public class Report : GraphWrapperNode
{
    private Report(INode node, IGraph reportGraph)
        : base(node, reportGraph)
    {
        Graph.TripleAsserted += TripleAsserted;
        Graph.TripleRetracted += TripleRetracted;
    }

    /// <summary>
    /// Gets a value indicating whether conformance checking was successful.
    /// </summary>
    public bool Conforms
    {
        get
        {
            INode conforms = Vocabulary.Conforms.ObjectsOf(this).SingleOrDefault();

            if (conforms is null)
            {
                return true;
            }

            return conforms.AsValuedNode().AsBoolean();
        }

        internal set
        {
            foreach (INode conforms in Vocabulary.Conforms.ObjectsOf(this).ToList())
            {
                Graph.Retract(this, Vocabulary.Conforms, conforms);
            }

            Graph.Assert(this, Vocabulary.Conforms, value.ToLiteral(Graph));
        }
    }

    /// <summary>
    /// Gets a normalised graph containing validation report data as required for SHACL compliance testing.
    /// </summary>
    public IGraph Normalised
    {
        get
        {
            IEnumerable<INode> reportNodes = Graph.GetTriplesWithPredicateObject(Vocabulary.RdfType, Vocabulary.ValidationReport)
                .Select(t => t.Subject);
            var describer = new ReportDescribeAlgorithm();
            return describer.Describe(Graph, reportNodes);

            /*
            SparqlQuery q = new SparqlQueryParser().ParseFromString(@"
PREFIX sh: <http://www.w3.org/ns/shacl#> 

DESCRIBE ?s
WHERE {
?s a sh:ValidationReport .
}
");

            var processor = new LeviathanQueryProcessor(new InMemoryDataset(Graph), option =>
            {
                option.Describer = new SparqlDescriber(new ReportDescribeAlgorithm());
            });
            return (IGraph)processor.ProcessQuery(q);
            */
        }
    }

    /// <summary>
    /// Gets the collection of validation results for this report.
    /// </summary>
    public ICollection<Result> Results
    {
        get
        {
            return new ResultCollection(this);
        }
    }

    internal INode Type
    {
        get
        {
            return Vocabulary.RdfType.ObjectsOf(this).SingleOrDefault();
        }

        set
        {
            foreach (INode type in Vocabulary.RdfType.ObjectsOf(this).ToList())
            {
                Graph.Retract(this, Vocabulary.RdfType, type);
            }

            if (value is null)
            {
                return;
            }

            Graph.Assert(this, Vocabulary.RdfType, value);
        }
    }

    /// <summary>
    /// Wraps a graph with SHACL validation report data.
    /// </summary>
    /// <param name="g">The graph containing SHACL validation report statements.</param>
    /// <returns>A report representing the SHACL validation report in the erapped graph.</returns>
    public static Report Parse(IGraph g)
    {
        return new Report(g.InstancesOf(Vocabulary.ValidationReport).Single(), g);
    }

    internal static Report Create(IGraph g)
    {
        var report = new Report(g.CreateBlankNode(), g)
        {
            Type = Vocabulary.ValidationReport,
            Conforms = true,
        };

        return report;
    }

    private void TripleRetracted(object sender, TripleEventArgs args)
    {
        if (args.Triple.Predicate.Equals(Vocabulary.Result))
        {
            Conforms = !Results.Any();
        }
    }

    private void TripleAsserted(object sender, TripleEventArgs args)
    {
        if (args.Triple.Predicate.Equals(Vocabulary.Result))
        {
            Conforms = false;
        }
    }
}
