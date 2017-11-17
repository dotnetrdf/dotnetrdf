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

using System.Collections.Generic;
using System.Linq;
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
            _funcContext = context[SparqlSpecsHelper.SparqlKeywordBNode] as BNodeFunctionContext;

            if (_funcContext == null)
            {
                _funcContext = new BNodeFunctionContext(context.InputMultiset.GetHashCode());
                context[SparqlSpecsHelper.SparqlKeywordBNode] = _funcContext;
            }
            else if (_funcContext.CurrentInput != context.InputMultiset.GetHashCode())
            {
                // Clear the Context
                _funcContext.BlankNodes.Clear();
                context[SparqlSpecsHelper.SparqlKeywordBNode] = _funcContext;
            }

            if (_expr == null)
            {
                // If no argument then always a fresh BNode
                return _funcContext.Graph.CreateBlankNode().AsValuedNode();
            }
            else
            {
                INode temp = _expr.Evaluate(context, bindingID);
                if (temp != null)
                {
                    if (temp.NodeType == NodeType.Literal)
                    {
                        ILiteralNode lit = (ILiteralNode)temp;

                        if (lit.DataType == null)
                        {
                            if (lit.Language.Equals(string.Empty))
                            {
                                if (!_funcContext.BlankNodes.ContainsKey(bindingID))
                                {
                                    _funcContext.BlankNodes.Add(bindingID, new Dictionary<string, INode>());
                                }

                                if (!_funcContext.BlankNodes[bindingID].ContainsKey(lit.Value))
                                {
                                    _funcContext.BlankNodes[bindingID].Add(lit.Value, _funcContext.Graph.CreateBlankNode());
                                }
                                return _funcContext.BlankNodes[bindingID][lit.Value].AsValuedNode();
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a lanuage specified literal");
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
                if (_expr == null) return Enumerable.Empty<string>();
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
                if (_expr == null) return Enumerable.Empty<ISparqlExpression>();
                return base.Arguments;
            }
        }

        /// <summary>
        /// Gets whether the expression can be parallelised
        /// </summary>
        public override bool CanParallelise
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordBNode + "(" + _expr.ToSafeString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new BNodeFunction(transformer.Transform(_expr));
        }
    }

    class BNodeFunctionContext
    {
        private Dictionary<int, Dictionary<string, INode>> _bnodes = new Dictionary<int, Dictionary<string, INode>>();
        private Graph _g = new Graph();
        private int _currInput;

        public BNodeFunctionContext(int currInput)
        {
            _currInput = currInput;
        }

        public int CurrentInput
        {
            get
            {
                return _currInput;
            }
        }

        public IGraph Graph
        {
            get
            {
                return _g;
            }
        }

        public Dictionary<int, Dictionary<string, INode>> BlankNodes
        {
            get
            {
                return _bnodes;
            }
            set
            {
                _bnodes = value;
            }
        }
    }
}
