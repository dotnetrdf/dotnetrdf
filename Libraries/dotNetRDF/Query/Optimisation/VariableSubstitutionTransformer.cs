/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An optimiser for walking algebra and expression trees and replacing a Variable with another Variable or a Constant
    /// </summary>
    public class VariableSubstitutionTransformer
        : PrimaryExpressionSubstituter, IAlgebraOptimiser
    {
        private String _findVar;
        private PatternItem _replaceItem;
        private ISparqlExpression _replaceExpr;
        private IToken _replaceToken;
        private bool _canReplaceObjects;
        private bool _canReplaceCustom = false;

        /// <summary>
        /// Create a transform that replaces one variable with another
        /// </summary>
        /// <param name="findVar">Find Variable</param>
        /// <param name="replaceVar">Replace Variable</param>
        public VariableSubstitutionTransformer(String findVar, String replaceVar)
        {
            _findVar = findVar;
            _replaceItem = new VariablePattern("?" + replaceVar);
            _replaceExpr = new VariableTerm(replaceVar);
            _replaceToken = new VariableToken("?" + replaceVar, 0, 0, 0);
            _canReplaceObjects = false;
        }

        /// <summary>
        /// Create a transform that replaces a variable with a constant
        /// </summary>
        /// <param name="findVar">Find Variable</param>
        /// <param name="replaceTerm">Replace Constant</param>
        public VariableSubstitutionTransformer(String findVar, INode replaceTerm)
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
        /// Gets/Sets whethe the Transformer is allowed to replace objects
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
        /// Attempts to do variable substitution within the given algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            // By default we are only safe to replace objects in a scope if we are replacing with a constant
            // Note that if we also make a replace in a subject/predicate position for a variable replace then
            // that makes object replacement safe for that scope only
            bool canReplaceObjects = (_canReplaceCustom ? _canReplaceObjects : _replaceItem is NodeMatchPattern);

            if (algebra is IBgp)
            {
                IBgp bgp = (IBgp)algebra;
                if (bgp.PatternCount == 0) return bgp;

                // Do variable substitution on the patterns
                List<ITriplePattern> ps = new List<ITriplePattern>();
                foreach (ITriplePattern p in bgp.TriplePatterns)
                {
                    switch (p.PatternType)
                    {
                        case TriplePatternType.Match:
                            IMatchTriplePattern tp = (IMatchTriplePattern)p;
                            PatternItem subj = tp.Subject.VariableName != null && tp.Subject.VariableName.Equals(_findVar) ? _replaceItem : tp.Subject;
                            if (ReferenceEquals(subj, _replaceItem)) canReplaceObjects = (_canReplaceCustom ? _canReplaceObjects : true);
                            PatternItem pred = tp.Predicate.VariableName != null && tp.Predicate.VariableName.Equals(_findVar) ? _replaceItem : tp.Predicate;
                            if (ReferenceEquals(pred, _replaceItem)) canReplaceObjects = (_canReplaceCustom ? _canReplaceObjects : true);
                            PatternItem obj = tp.Object.VariableName != null && tp.Object.VariableName.Equals(_findVar) ? _replaceItem : tp.Object;
                            if (ReferenceEquals(obj, _replaceItem) && !canReplaceObjects) throw new Exception("Unable to substitute a variable into the object position in this scope");
                            ps.Add(new TriplePattern(subj, pred, obj));
                            break;
                        case TriplePatternType.Filter:
                            IFilterPattern fp = (IFilterPattern)p;
                            ps.Add(new FilterPattern(new UnaryExpressionFilter(Transform(fp.Filter.Expression))));
                            break;
                        case TriplePatternType.BindAssignment:
                            IAssignmentPattern bp = (IAssignmentPattern)p;
                            ps.Add(new BindPattern(bp.VariableName, Transform(bp.AssignExpression)));
                            break;
                        case TriplePatternType.LetAssignment:
                            IAssignmentPattern lp = (IAssignmentPattern)p;
                            ps.Add(new LetPattern(lp.VariableName, Transform(lp.AssignExpression)));
                            break;
                        case TriplePatternType.SubQuery:
                            throw new RdfQueryException("Cannot do variable substitution when a sub-query is present");
                        case TriplePatternType.Path:
                            throw new RdfQueryException("Cannot do variable substitution when a property path is present");
                        case TriplePatternType.PropertyFunction:
                            throw new RdfQueryException("Cannot do variable substituion when a property function is present");
                        default:
                            throw new RdfQueryException("Cannot do variable substitution on unknown triple patterns");
                    }
                }
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
                Algebra.Graph g = (Algebra.Graph)((IUnaryOperator)algebra).Transform(this);
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

        /// <summary>
        /// Returns false because this optimiser is never globally applicable
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            return false;
        }

        /// <summary>
        /// Returns false because this optimiser is never globally applicable
        /// </summary>
        /// <param name="cmds">Update Commands</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return false;
        }

        /// <summary>
        /// Tries to substitute variables within primary expressions
        /// </summary>
        /// <param name="expr">Expression</param>
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
                GraphPatternTerm gp = (GraphPatternTerm)expr;
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
}
