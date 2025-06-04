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
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Query;
using VDS.RDF.Shacl.Validation;

namespace VDS.RDF.Shacl;

/// <summary>
/// Represents a SHACL shapes graph that acts as a fully compliant SHACL Core and SHACL-SPARQL processor.
/// </summary>
/// <remarks>The Datatype constraint component is not supported under .NET Standard 1.4.</remarks>
public class ShapesGraph : WrapperGraph
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShapesGraph"/> class.
    /// </summary>
    /// <param name="shapesGraph">The original graph containing SHACL shapes.</param>
    [DebuggerStepThrough]
    public ShapesGraph(IGraph shapesGraph)
        : base(shapesGraph)
    {
    }

    internal IEnumerable<ConstraintComponent> ConstraintComponents
    {
        get
        {
            return
                from constraintComponent in this.ShaclInstancesOf(Vocabulary.ConstraintComponent)
                select new ConstraintComponent(constraintComponent, this._g);
        }
    }

    private IEnumerable<Shape> TargetedShapes
    {
        get
        {
            var query = new SparqlParameterizedString(@"
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX sh: @shacl

SELECT DISTINCT ?shape {
    {
        SELECT * {
            VALUES ?target {
                sh:targetNode
                sh:targetClass
                sh:targetSubjectsOf
                sh:targetObjectsOf
            }

            ?shape ?target ?any .
        }
    }

    UNION {
        SELECT * {
            VALUES ?class {
                sh:NodeShape
                sh:PropertyShape
            }

            ?shape a rdfs:Class, ?class .
        }
    }
}
");
            query.SetUri("shacl", UriFactory.Create(Vocabulary.BaseUri));
            
            return
                from result in (SparqlResultSet)this.ExecuteQuery(query)
                let shape = result["shape"]
                select Shape.Parse(shape, this);
        }
    }

    /// <summary>
    /// Checks the given data graph against this shapes graph for SHACL conformance and reports validation results.
    /// </summary>
    /// <param name="dataGraph">The data graph to check for SHACL conformance.</param>
    /// <returns>A SHACL validation report containing possible validation results.</returns>
    public Report Validate(IGraph dataGraph)
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("sh", UriFactory.Create(Vocabulary.BaseUri));
        var report = Report.Create(g);

        Validate(dataGraph, report);

        return report;
    }

    /// <summary>
    /// Checks the given data graph against this shapes graph for SHACL conformance.
    /// </summary>
    /// <param name="dataGraph">The data graph to check for SHACL conformance.</param>
    /// <returns>Whether the data graph SHACL conforms to this shapes graph.</returns>
    public bool Conforms(IGraph dataGraph)
    {
        return Validate(dataGraph, null);
    }

    private bool Validate(IGraph dataGraph, Report report)
    {
        return (
            from shape in TargetedShapes
            select shape.Validate(dataGraph, report))
            .Aggregate(true, (a, b) => a && b);
    }
}
