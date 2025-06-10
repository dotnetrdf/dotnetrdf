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
/// Pattern which matches Variables.
/// </summary>
public class VariablePattern 
    : PatternItem
{
    /// <summary>
    /// Creates a new Variable Pattern.
    /// </summary>
    /// <param name="name">Variable name.</param>
    public VariablePattern(string name)
    {
        VariableName = name;

        // Strip leading ?/$ if present
        if (VariableName.StartsWith("?") || VariableName.StartsWith("$"))
        {
            VariableName = VariableName.Substring(1);
        }
    }

    /// <summary>
    /// Creates a new Variable Pattern.
    /// </summary>
    /// <param name="name">Variable name.</param>
    /// <param name="rigorousEvaluation">Whether to force rigorous evaluation.</param>
    public VariablePattern(string name, bool rigorousEvaluation)
        : this(name)
    {
        RigorousEvaluation = rigorousEvaluation;
    }

    /// <summary>
    /// Checks whether the given Node is a valid value for the Variable in the current Binding Context.
    /// </summary>
    /// <param name="context">Evaluation Context.</param>
    /// <param name="obj">Node to test.</param>
    /// <param name="s"></param>
    /// <returns></returns>
    public override bool Accepts(IPatternEvaluationContext context, INode obj, ISet s)
    {
        if (s.ContainsVariable(VariableName)) return s[VariableName].Equals(obj);
        if ((context.RigorousEvaluation || RigorousEvaluation) && context.ContainsVariable(VariableName) && !context.ContainsValue(VariableName, obj))
        {
            return false;
        }
        s.Add(VariableName, obj);
        return true;
    }

    /// <summary>
    /// Constructs a Node based on the given Set.
    /// </summary>
    /// <param name="context">Construct Context.</param>
    /// <returns>The Node which is bound to this Variable in this Solution.</returns>
    public override INode Construct(ConstructContext context)
    {
        INode value = context.Set[VariableName];

        if (value == null) throw new RdfQueryException("Unable to construct a Value for this Variable for this solution as it is bound to a null");
        return value.NodeType switch
        {
            NodeType.Blank => new BlankNode(((IBlankNode)value).InternalID),
            _ => context.GetNode(value),
        };
    }

    /// <inheritdoc />
    public override INode Bind(ISet variableBindings)
    {
        return variableBindings[VariableName];
    }

    /// <inheritdoc />
    public override void AddBindings(INode forNode, ISet toSet)
    {
        toSet.Add(VariableName, forNode);
    }

    /// <summary>
    /// Gets the String representation of this pattern.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "?" + VariableName;
    }

    /// <summary>
    /// Gets the name of the variable that this pattern matches.
    /// </summary>
    public string VariableName { get; }

    /// <summary>
    /// Gets the Name of the Variable this Pattern matches.
    /// </summary>
    public override IEnumerable<string> Variables => VariableName.AsEnumerable();

    /// <inheritdoc />
    public override bool IsFixed => false;
}
