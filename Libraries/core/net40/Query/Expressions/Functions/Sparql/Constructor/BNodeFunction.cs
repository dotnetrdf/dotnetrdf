/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Constructor
{
    /// <summary>
    /// Class representing the SPARQL BNODE() function
    /// </summary>
    public class BNodeFunction 
        : BaseUnaryExpression
    {
        private BNodeFunctionContext _funcContext;

        /// <summary>
        /// Creates a new BNode Function
        /// </summary>
        public BNodeFunction()
            : base(null) { }

        /// <summary>
        /// Creates a new BNode Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public BNodeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the value of the expression as evaluated in a given Context for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            this._funcContext = context[SparqlSpecsHelper.SparqlKeywordBNode] as BNodeFunctionContext;

            if (this._funcContext == null)
            {
                this._funcContext = new BNodeFunctionContext(context.InputMultiset.GetHashCode());
                context[SparqlSpecsHelper.SparqlKeywordBNode] = this._funcContext;
            }
            else if (this._funcContext.CurrentInput != context.InputMultiset.GetHashCode())
            {
                this._funcContext = new BNodeFunctionContext(context.InputMultiset.GetHashCode());
                context[SparqlSpecsHelper.SparqlKeywordBNode] = this._funcContext;
            }

            if (this._expr == null)
            {
                //If no argument then always a fresh BNode
                return new BlankNode(this._funcContext.Graph, this._funcContext.Mapper.GetNextID());
            }
            else
            {
                INode temp = this._expr.Evaluate(context, bindingID);
                if (temp != null)
                {
                    if (temp.NodeType == NodeType.Literal)
                    {
                        ILiteralNode lit = (ILiteralNode)temp;

                        if (lit.DataType == null)
                        {
                            if (lit.Language.Equals(string.Empty))
                            {
                                if (!this._funcContext.BlankNodes.ContainsKey(bindingID))
                                {
                                    this._funcContext.BlankNodes.Add(bindingID, new Dictionary<string, INode>());
                                }

                                if (!this._funcContext.BlankNodes[bindingID].ContainsKey(lit.Value))
                                {
                                    this._funcContext.BlankNodes[bindingID].Add(lit.Value, new BlankNode(this._funcContext.Graph, this._funcContext.Mapper.GetNextID()));
                                }
                                return this._funcContext.BlankNodes[bindingID][lit.Value].AsValuedNode();
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create a Blank Node whne the argument Expression evaluates to a lanuage specified literal");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a typed literal node");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a non-literal node");
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to null");
                }
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordBNode;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Expression
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                if (this._expr == null) return Enumerable.Empty<string>();
                return base.Variables;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (this._expr == null) return Enumerable.Empty<ISparqlExpression>();
                return base.Arguments;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordBNode + "(" + this._expr.ToSafeString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new BNodeFunction(transformer.Transform(this._expr));
        }
    }

    class BNodeFunctionContext
    {
        private Dictionary<int, Dictionary<string, INode>> _bnodes = new Dictionary<int, Dictionary<string, INode>>();
        private BlankNodeMapper _mapper = new BlankNodeMapper("bnodeFunc");
        private Graph _g = new Graph();
        private int _currInput;

        public BNodeFunctionContext(int currInput)
        {
            this._currInput = currInput;
        }

        public int CurrentInput
        {
            get
            {
                return this._currInput;
            }
        }

        public BlankNodeMapper Mapper
        {
            get
            {
                return this._mapper;
            }
        }

        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        public Dictionary<int, Dictionary<string, INode>> BlankNodes
        {
            get
            {
                return this._bnodes;
            }
            set
            {
                this._bnodes = value;
            }
        }
    }
}
