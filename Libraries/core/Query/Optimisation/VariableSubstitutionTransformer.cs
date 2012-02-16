/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

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

        /// <summary>
        /// Create a transform that replaces one variable with another
        /// </summary>
        /// <param name="findVar">Find Variable</param>
        /// <param name="replaceVar">Replace Variable</param>
        public VariableSubstitutionTransformer(String findVar, String replaceVar)
        {
            this._findVar = findVar;
            this._replaceItem = new VariablePattern("?" + replaceVar);
            this._replaceExpr = new VariableTerm(replaceVar);
            this._replaceToken = new VariableToken("?" + replaceVar, 0, 0, 0);
        }

        /// <summary>
        /// Create a transform that replaces a variable with a constant
        /// </summary>
        /// <param name="findVar">Find Variable</param>
        /// <param name="replaceTerm">Replace Constant</param>
        public VariableSubstitutionTransformer(String findVar, INode replaceTerm)
        {
            this._findVar = findVar;
            this._replaceItem = new NodeMatchPattern(replaceTerm);
            this._replaceExpr = new ConstantTerm(replaceTerm);
            if (replaceTerm is IUriNode)
            {
                this._replaceToken = new UriToken("<" + ((IUriNode)replaceTerm).Uri.ToString() + ">", 0, 0, 0);
            }
        }

        /// <summary>
        /// Attempts to do variable substitution within the given algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            if (algebra is IBgp)
            {
                IBgp bgp = (IBgp)algebra;
                if (bgp.PatternCount == 0) return bgp;

                //Do variable substitution on the patterns
                List<ITriplePattern> ps = new List<ITriplePattern>();
                foreach (ITriplePattern p in bgp.TriplePatterns)
                {
                    if (p is TriplePattern)
                    {
                        TriplePattern tp = (TriplePattern)p;
                        PatternItem subj = tp.Subject.VariableName != null && tp.Subject.VariableName.Equals(this._findVar) ? this._replaceItem : tp.Subject;
                        PatternItem pred = tp.Predicate.VariableName != null && tp.Predicate.VariableName.Equals(this._findVar) ? this._replaceItem : tp.Predicate;
                        PatternItem obj = tp.Object.VariableName != null && tp.Object.VariableName.Equals(this._findVar) ? this._replaceItem : tp.Object;
                        ps.Add(new TriplePattern(subj, pred, obj));
                    }
                    else if (p is FilterPattern)
                    {
                        FilterPattern fp = (FilterPattern)p;
                        ps.Add(new FilterPattern(new UnaryExpressionFilter(this.Transform(fp.Filter.Expression))));
                    }
                    else if (p is BindPattern)
                    {
                        BindPattern bp = (BindPattern)p;
                        ps.Add(new BindPattern(bp.VariableName, this.Transform(bp.AssignExpression)));
                    }
                    else if (p is LetPattern)
                    {
                        LetPattern lp = (LetPattern)p;
                        ps.Add(new LetPattern(lp.VariableName, this.Transform(lp.AssignExpression)));
                    }
                    else if (p is SubQueryPattern)
                    {
                        throw new RdfQueryException("Cannot do variable substitution when a sub-query is present");
                    }
                    else if (p is PropertyPathPattern)
                    {
                        throw new RdfQueryException("Cannot do variable substitution when a property path is present");
                    }
                    else
                    {
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
                if (g.GraphSpecifier is VariableToken && g.GraphSpecifier.Value.Equals("?" + this._findVar))
                {
                    if (this._replaceToken != null)
                    {
                        return new Algebra.Graph(g.InnerAlgebra, this._replaceToken);
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
                if (expr.Variables.First().Equals(this._findVar))
                {
                    return this._replaceExpr;
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
                alg = this.Optimise(alg);
                return new GraphPatternTerm(alg.ToGraphPattern());
            }
            else
            {
                return expr;
            }
        }
    }
}
