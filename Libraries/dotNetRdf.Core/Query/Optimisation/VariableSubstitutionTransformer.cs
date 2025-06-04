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

using System;
using System.Linq;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation;

/// <summary>
/// An optimiser for walking algebra and expression trees and replacing a Variable with another Variable or a Constant.
/// </summary>
public class VariableSubstitutionTransformer
    : PrimaryExpressionSubstituter, IAlgebraOptimiser
{
    private string _findVar;
    private PatternItem _replaceItem;
    private ISparqlExpression _replaceExpr;
    private IToken _replaceToken;
    private bool _canReplaceObjects;
    private bool _canReplaceCustom = false;

    /// <inheritdoc/>
    public bool UnsafeOptimisation { get; set; }

    /// <summary>
    /// Create a transform that replaces one variable with another.
    /// </summary>
    /// <param name="findVar">Find Variable.</param>
    /// <param name="replaceVar">Replace Variable.</param>
    public VariableSubstitutionTransformer(string findVar, string replaceVar)
    {
        _findVar = findVar;
        _replaceItem = new VariablePattern("?" + replaceVar);
        _replaceExpr = new VariableTerm(replaceVar);
        _replaceToken = new VariableToken("?" + replaceVar, 0, 0, 0);
        _canReplaceObjects = false;
    }

    /// <summary>
    /// Create a transform that replaces a variable with a constant.
    /// </summary>
    /// <param name="findVar">Find Variable.</param>
    /// <param name="replaceTerm">Replace Constant.</param>
    public VariableSubstitutionTransformer(string findVar, INode replaceTerm)
    {
        _findVar = findVar;
        _replaceItem = new NodeMatchPattern(replaceTerm);
        _replaceExpr = new ConstantTerm(replaceTerm);
        if (replaceTerm is IUriNode)
        {
            _replaceToken = new UriToken("<" + ((IUriNode)replaceTerm).Uri.AbsoluteUri + ">", 0, 0, 0);
        }
        _canReplaceObjects = true;
    }

    /// <summary>
    /// Gets/Sets whethe the Transformer is allowed to replace objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The transformer will intelligently select this depending on whether it is replacing with a constant (defaults to true) or a variable (defaults to false),  when replacing a variable the behaviour changes automatically.  If you set it explicitly the transformer will respect your setting regardless.
    /// </para>
    /// </remarks>
    public bool CanReplaceObjects
    {
        get
        {
            return _canReplaceObjects;
        }
        set
        {
            _canReplaceObjects = value;
            _canReplaceCustom = true;
        }
    }

    /// <summary>
    /// Attempts to do variable substitution within the given algebra.
    /// </summary>
    /// <param name="algebra">Algebra.</param>
    /// <returns></returns>
    public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
    {
        // By default we are only safe to replace objects in a scope if we are replacing with a constant
        // Note that if we also make a replace in a subject/predicate position for a variable replace then
        // that makes object replacement safe for that scope only
        var canReplaceObjects = (_canReplaceCustom ? _canReplaceObjects : _replaceItem is NodeMatchPattern);

        if (algebra is IBgp)
        {
            var bgp = (IBgp)algebra;
            if (bgp.PatternCount == 0) return bgp;

            // Do variable substitution on the patterns
            var ps = bgp.TriplePatterns.Select(p => ApplySubstitution(p, canReplaceObjects)).ToList();
            return new Bgp(ps);
        }
        else if (algebra is Service)
        {
            throw new RdfQueryException("Cannot do variable substitution when a SERVICE clause is present");
        }
        else if (algebra is SubQuery)
        {
            throw new RdfQueryException("Cannot do variable substitution when a sub-query is present");
        }
        else if (algebra is IPathOperator)
        {
            throw new RdfQueryException("Cannot do variable substitution when a property path is present");
        }
        else if (algebra is Algebra.Graph)
        {
            var g = (Algebra.Graph)((IUnaryOperator)algebra).Transform(this);
            if (g.GraphSpecifier is VariableToken && g.GraphSpecifier.Value.Equals("?" + _findVar))
            {
                if (_replaceToken != null)
                {
                    return new Algebra.Graph(g.InnerAlgebra, _replaceToken);
                }
                else
                {
                    throw new RdfQueryException("Cannot do a variable substitution when the variable is used for a GRAPH specifier and the replacement term is not a URI");
                }
            }
            else
            {
                return g;
            }
        }
        else if (algebra is IUnaryOperator)
        {
            return ((IUnaryOperator)algebra).Transform(this);
        }
        else if (algebra is IAbstractJoin)
        {
            return ((IAbstractJoin)algebra).Transform(this);
        }
        else if (algebra is ITerminalOperator)
        {
            return algebra;
        }
        else
        {
            throw new RdfQueryException("Cannot do variable substitution on unknown algebra");
        }               
    }

    private ITriplePattern ApplySubstitution(ITriplePattern p, bool canReplaceObjects)
    {
        switch (p.PatternType)
        {
            case TriplePatternType.Match:
                var tp = (IMatchTriplePattern)p;
                PatternItem subj = ApplySubstitution(tp.Subject);
                if (ReferenceEquals(subj, _replaceItem)) canReplaceObjects = (_canReplaceCustom ? _canReplaceObjects : true);
                PatternItem pred = ApplySubstitution(tp.Predicate);
                if (ReferenceEquals(pred, _replaceItem)) canReplaceObjects = (_canReplaceCustom ? _canReplaceObjects : true);
                PatternItem obj = ApplySubstitution(tp.Object);
                if (ReferenceEquals(obj, _replaceItem) && !canReplaceObjects) throw new Exception("Unable to substitute a variable into the object position in this scope");
                return new TriplePattern(subj, pred, obj);
                
            case TriplePatternType.Filter:
                var fp = (IFilterPattern)p;
                return new FilterPattern(new UnaryExpressionFilter(Transform(fp.Filter.Expression)));

            case TriplePatternType.BindAssignment:
                var bp = (IAssignmentPattern)p;
                return new BindPattern(bp.VariableName, Transform(bp.AssignExpression));

            case TriplePatternType.LetAssignment:
                var lp = (IAssignmentPattern)p;
                return(new LetPattern(lp.VariableName, Transform(lp.AssignExpression)));

            case TriplePatternType.SubQuery:
                throw new RdfQueryException("Cannot do variable substitution when a sub-query is present");
            case TriplePatternType.Path:
                throw new RdfQueryException("Cannot do variable substitution when a property path is present");
            case TriplePatternType.PropertyFunction:
                throw new RdfQueryException("Cannot do variable substitution when a property function is present");
            default:
                throw new RdfQueryException("Cannot do variable substitution on unknown triple patterns");
        }
    }

    private PatternItem ApplySubstitution(PatternItem patternItem)
    {
        if (!patternItem.Variables.Contains(_findVar))
        {
            return patternItem;
        }

        return patternItem switch
        {
            QuotedTriplePattern qtp =>
                new QuotedTriplePattern(ApplySubstitution(qtp.QuotedTriple, _canReplaceCustom ? _canReplaceObjects : _replaceItem is NodeMatchPattern) as TriplePattern),
            VariablePattern or BlankNodePattern => _replaceItem,
            _ => patternItem
        };
    }
    /// <summary>
    /// Returns false because this optimiser is never globally applicable.
    /// </summary>
    /// <param name="q">Query.</param>
    /// <returns></returns>
    public bool IsApplicable(SparqlQuery q)
    {
        return false;
    }

    /// <summary>
    /// Returns false because this optimiser is never globally applicable.
    /// </summary>
    /// <param name="cmds">Update Commands.</param>
    /// <returns></returns>
    public bool IsApplicable(SparqlUpdateCommandSet cmds)
    {
        return false;
    }

    /// <summary>
    /// Tries to substitute variables within primary expressions.
    /// </summary>
    /// <param name="expr">Expression.</param>
    /// <returns></returns>
    protected override ISparqlExpression SubstitutePrimaryExpression(ISparqlExpression expr)
    {
        if (expr is VariableTerm)
        {
            if (expr.Variables.First().Equals(_findVar))
            {
                return _replaceExpr;
            }
            else
            {
                return expr;
            }
        }
        else if (expr is GraphPatternTerm)
        {
            var gp = (GraphPatternTerm)expr;
            ISparqlAlgebra alg = gp.Pattern.ToAlgebra();
            alg = Optimise(alg);
            return new GraphPatternTerm(alg.ToGraphPattern());
        }
        else
        {
            return expr;
        }
    }
}
