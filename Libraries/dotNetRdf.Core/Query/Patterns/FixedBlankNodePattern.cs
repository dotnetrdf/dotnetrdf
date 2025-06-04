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

namespace VDS.RDF.Query.Patterns;

/// <summary>
/// Pattern which matches the Blank Node with the given Internal ID regardless of the Graph the nodes come from.
/// </summary>
public class FixedBlankNodePattern : PatternItem
{
    /// <summary>
    /// Creates a new Fixed Blank Node Pattern.
    /// </summary>
    /// <param name="id">ID.</param>
    public FixedBlankNodePattern(string id)
    {
        if (id.StartsWith("_:"))
        {
            InternalID = id.Substring(2);
        }
        else
        {
            InternalID = id;
        }
    }

    /// <summary>
    /// Gets the Blank Node ID.
    /// </summary>
    public string InternalID { get; }

    /// <inheritdoc />
    public override bool IsFixed => true;

    /// <summary>
    /// Checks whether the pattern accepts the given Node.
    /// </summary>
    /// <param name="context">SPARQL Evaluation Context.</param>
    /// <param name="obj">Node to test.</param>
    /// <param name="s"></param>
    /// <returns></returns>
    public override bool Accepts(IPatternEvaluationContext context, INode obj, ISet s)
    {
        return obj.NodeType == NodeType.Blank && ((IBlankNode)obj).InternalID.Equals(InternalID);
    }

    /// <summary>
    /// Returns a Blank Node with a fixed ID scoped to whichever graph is provided.
    /// </summary>
    /// <param name="context">Construct Context.</param>
    public override INode Construct(ConstructContext context)
    {
        if (context.Graph != null)
        {
            IBlankNode b = context.Graph.GetBlankNode(InternalID);
            if (b != null)
            {
                return b;
            }
            else
            {
                return context.Graph.CreateBlankNode(InternalID);
            }
        }
        else
        {
            return new BlankNode(InternalID);
        }
    }

    /// <inheritdoc />
    public override INode Bind(ISet variableBindings)
    {
        return new BlankNode(InternalID);
    }

    /// <inheritdoc />
    public override void AddBindings(INode forNode, ISet toSet)
    {
        // No-op for fixed patterns
    }

    /// <summary>
    /// Gets the String representation of the Pattern Item.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "<_:" + InternalID + ">";
    }
}
