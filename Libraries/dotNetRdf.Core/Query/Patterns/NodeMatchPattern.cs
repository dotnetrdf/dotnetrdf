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

using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Patterns;

/// <summary>
/// Pattern which matches specific Nodes.
/// </summary>
public class NodeMatchPattern
    : PatternItem
{
    /// <summary>
    /// Creates a new Node Match Pattern.
    /// </summary>
    /// <param name="n">Exact Node to match.</param>
    public NodeMatchPattern(INode n)
    {
        Node = n;
    }

    /// <summary>
    /// Creates a new Node Match Pattern.
    /// </summary>
    /// <param name="n">Exact Node to match.</param>
    /// <param name="rigorousEvaluation">Whether to force rigorous evaluation regardless of the global setting.</param>
    public NodeMatchPattern(INode n, bool rigorousEvaluation)
        : this(n)
    {
        RigorousEvaluation = rigorousEvaluation;
    }

    /// <summary>
    /// Checks whether the given Node matches the Node this pattern was instantiated with.
    /// </summary>
    /// <param name="context">Evaluation Context.</param>
    /// <param name="obj">Node to test.</param>
    /// <param name="s"></param>
    /// <returns></returns>
    public override bool Accepts(IPatternEvaluationContext context, INode obj, ISet s)
    {
        if (context.RigorousEvaluation || RigorousEvaluation)
        {
            return Node.Equals(obj);
        }

        return true;
    }

    /// <summary>
    /// Constructs a Node based on the given Set.
    /// </summary>
    /// <param name="context">Construct Context.</param>
    public override INode Construct(ConstructContext context)
    {
        return context.GetNode(Node);
    }

    /// <inheritdoc />
    public override INode Bind(ISet variableBindings)
    {
        return Node;
    }

    /// <inheritdoc />
    public override void AddBindings(INode forNode, ISet toSet)
    {
        // No-op for fixed item patterns
    }

    /// <summary>
    /// Gets a String representation of the Node.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return new SparqlFormatter().Format(Node);
    }

    /// <inheritdoc />
    public override bool IsFixed => true;

    /// <summary>
    /// Gets the Node that this Pattern matches.
    /// </summary>
    public INode Node { get; }
}
