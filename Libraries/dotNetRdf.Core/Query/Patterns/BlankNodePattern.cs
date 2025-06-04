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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns;

/// <summary>
/// Pattern which matches temporary variables.
/// </summary>
public class BlankNodePattern 
    : PatternItem
{
    /// <summary>
    /// Creates a new Pattern representing a Blank Node.
    /// </summary>
    /// <param name="name">Blank Node ID.</param>
    public BlankNodePattern(string name)
    {
        ID = "_:" + name;
    }

    /// <summary>
    /// Creates a new Pattern representing a Blank Node.
    /// </summary>
    /// <param name="name">Blank Node ID.</param>
    /// <param name="rigorousEvaluation">Whether to force rigorous evaluation.</param>
    public BlankNodePattern(string name, bool rigorousEvaluation)
        : this(name)
    {
        RigorousEvaluation = rigorousEvaluation;
    }

    /// <summary>
    /// Gets the Blank Node ID.
    /// </summary>
    public string ID { get; }

    /// <inheritdoc />
    public override bool IsFixed => false;

    /// <summary>
    /// Checks whether the given Node is a valid value for the Temporary Variable.
    /// </summary>
    /// <param name="context">Evaluation Context.</param>
    /// <param name="obj">Node to test.</param>
    /// <param name="s"></param>
    /// <returns></returns>
    public override bool Accepts(IPatternEvaluationContext context, INode obj, ISet s)
    {
        if (s.ContainsVariable(ID)) return obj.Equals(s[ID]);
        if ((context.RigorousEvaluation || RigorousEvaluation) && context.ContainsVariable(ID) && !context.ContainsValue(ID, obj))
        {
            return false;
        }

        s.Add(ID, obj);
        return true;
    }

    /// <summary>
    /// Constructs a Node based on the given Set.
    /// </summary>
    /// <param name="context">Construct Context.</param>
    /// <returns></returns>
    public override INode Construct(ConstructContext context)
    {
        return context.GetBlankNode(ID);
    }

    /// <summary>
    /// Gets the String representation of this Pattern.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return ID;
    }

    /// <summary>
    /// Gets the Temporary Variable Name of this Pattern.
    /// </summary>
    public override IEnumerable<string> Variables => ID.AsEnumerable();

    /// <inheritdoc />
    public override void AddBindings(INode forNode, ISet toSet)
    {
        toSet.Add(ID, forNode);
    }

    /// <inheritdoc />
    public override INode Bind(ISet variableBindings)
    {
        //return new BlankNode(ID.Substring(2));
        return variableBindings[ID];
    }
}
