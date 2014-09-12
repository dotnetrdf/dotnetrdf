/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Constructor
{
    /// <summary>
    /// Class representing the SPARQL BNODE() function
    /// </summary>
    public class BNodeFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new BNode Function
        /// </summary>
        public BNodeFunction()
            : base(null) { }

        /// <summary>
        /// Creates a new BNode Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public BNodeFunction(IExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the value of the expression as evaluated in a given Context for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            this._funcContext = context[SparqlSpecsHelper.SparqlKeywordBNode] as BNodeFunctionContext;

            if (this._funcContext == null)
            {
                this._funcContext = new BNodeFunctionContext(context.InputMultiset.GetHashCode());
                context[SparqlSpecsHelper.SparqlKeywordBNode] = this._funcContext;
            }
            else if (this._funcContext.CurrentInput != context.InputMultiset.GetHashCode())
            {
                //Clear the Context
                this._funcContext.BlankNodes.Clear();
                context[SparqlSpecsHelper.SparqlKeywordBNode] = this._funcContext;
            }

            if (this._expr == null)
            {
                //If no argument then always a fresh BNode
                return this._funcContext.Graph.CreateBlankNode().AsValuedNode();
            }
            else
            {
                INode temp = this._expr.Evaluate(solution, context);
                if (temp != null)
                {
                    if (temp.NodeType == NodeType.Literal)
                    {
                        INode lit = temp;

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
                                    this._funcContext.BlankNodes[bindingID].Add(lit.Value, this._funcContext.Graph.CreateBlankNode());
                                }
                                return this._funcContext.BlankNodes[bindingID][lit.Value].AsValuedNode();
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
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordBNode;
            }
        }
    }
}
