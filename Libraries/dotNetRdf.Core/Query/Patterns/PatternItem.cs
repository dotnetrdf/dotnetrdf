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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns;

/// <summary>
/// Class for representing Node Patterns in Sparql Queries.
/// </summary>
public abstract class PatternItem
{
    /// <summary>
    /// Binding Context for Pattern Item.
    /// </summary>
    protected SparqlResultBinder _context = null;

    /// <summary>
    /// Checks whether the Pattern Item accepts the given Node in the given Context.
    /// </summary>
    /// <param name="context">Evaluation Context.</param>
    /// <param name="obj">Node to test.</param>
    /// <param name="set"></param>
    /// <returns></returns>
    public abstract bool Accepts(IPatternEvaluationContext context, INode obj, ISet set);

    /// <summary>
    /// Constructs a Node based on this Pattern for the given Set.
    /// </summary>
    /// <param name="context">Construct Context.</param>
    /// <returns></returns>
    public abstract INode Construct(ConstructContext context);

    /// <summary>
    /// Returns a node created by applying the variable bindings in the input set to this pattern item.
    /// </summary>
    /// <param name="variableBindings"></param>
    /// <returns>A node binding for the pattern item or null if no binding is possible with the provide set.</returns>
    public abstract INode Bind(ISet variableBindings);

    /// <summary>
    /// Returns the variable bindings created when this pattern item accepts the given node to the specified set.
    /// </summary>
    /// <param name="forNode"></param>
    /// <param name="toSet"></param>
    public abstract void AddBindings(INode forNode, ISet toSet);

    /// <summary>
    /// Sets the Binding Context for the Pattern Item.
    /// </summary>
    public SparqlResultBinder BindingContext
    {
        set => _context = value;
    }

    /// <summary>
    /// Gets/Sets whether rigorous evaluation is used, note that this setting may be overridden by the <see cref="LeviathanQueryOptions.RigorousEvaluation" /> option
    /// passed to the query processor when it is initialized.
    /// </summary>
    public bool RigorousEvaluation { get; set; } = false;

    /// <summary>
    /// Gets the String representation of the Pattern.
    /// </summary>
    /// <returns></returns>
    public abstract override string ToString();

    /// <summary>
    /// Gets an enumeration of the names of the variables of this pattern.
    /// </summary>
    /// <remarks>
    /// If this item is a Variable Pattern, the enumeration will contain a single item.
    /// If this item is a QuotedTriplePattern, the enumeration may contain zero or more items.
    /// For other pattern items the enumeration will be empty.
    /// </remarks>
    public virtual IEnumerable<string> Variables => Enumerable.Empty<string>();

    /// <summary>
    /// Return true if this pattern item contains no variables, false otherwise.
    /// </summary>
    public abstract bool IsFixed { get; }

    /// <summary>
    /// Gets/Sets whether the Variable is repeated in the Pattern.
    /// </summary>
    public virtual bool Repeated { get; set; } = false;
}