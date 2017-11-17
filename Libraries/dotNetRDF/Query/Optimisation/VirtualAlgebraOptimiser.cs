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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Storage.Virtualisation;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// Abstract implementation of an algebra optimiser and expression transformer which optimises the algebra to replace any Node terms with Virtual Node terms for more efficient querying of virtualised RDF data
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    /// <typeparam name="TGraphID">Graph ID Type</typeparam>
    public abstract class VirtualAlgebraOptimiser<TNodeID, TGraphID> 
        : IAlgebraOptimiser, IExpressionTransformer
    {
        /// <summary>
        /// Virtual RDF Provider
        /// </summary>
        protected IVirtualRdfProvider<TNodeID, TGraphID> _provider;
        private Type _exprType = typeof(ConstantTerm);

        /// <summary>
        /// Creates a new Virtual Algebra Optimiser
        /// </summary>
        /// <param name="provider">Virtual RDF Provider</param>
        public VirtualAlgebraOptimiser(IVirtualRdfProvider<TNodeID, TGraphID> provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Optimises the algebra so that all Node terms are virtualised
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            if (algebra is IAbstractJoin)
            {
                return ((IAbstractJoin)algebra).Transform(this);
            }
            else if (algebra is IUnaryOperator)
            {
                return ((IUnaryOperator)algebra).Transform(this);
            }
            else if (algebra is IBgp)
            {
                IBgp current = (IBgp)algebra;
                if (current.PatternCount == 0)
                {
                    return current;
                }
                else
                {
                    ISparqlAlgebra result = new Bgp();
                    List<ITriplePattern> patterns = new List<ITriplePattern>();
                    List<ITriplePattern> ps = new List<ITriplePattern>(current.TriplePatterns.ToList());
                    TNodeID nullID = _provider.NullID;

                    for (int i = 0; i < current.PatternCount; i++)
                    {
                        if (ps[i].PatternType == TriplePatternType.Filter || ps[i].PatternType == TriplePatternType.BindAssignment || ps[i].PatternType == TriplePatternType.LetAssignment)
                        {
                            // First ensure that if we've found any other Triple Patterns up to this point
                            // we dump this into a BGP and join with the result so far
                            if (patterns.Count > 0)
                            {
                                result = Join.CreateJoin(result, new Bgp(patterns));
                                patterns.Clear();
                            }
                            if (ps[i].PatternType == TriplePatternType.Filter)
                            {
                                result = new Filter(result, new UnaryExpressionFilter(Transform(((IFilterPattern)ps[i]).Filter.Expression)));
                            }
                            else
                            {
                                IAssignmentPattern bind = (IAssignmentPattern)ps[i];
                                result = new Extend(result, Transform(bind.AssignExpression), bind.VariableName);
                            }
                        }
                        else if (ps[i].PatternType == TriplePatternType.Match)
                        {
                            // Convert Terms in the Pattern into Virtual Nodes
                            IMatchTriplePattern tp = (IMatchTriplePattern)ps[i];
                            PatternItem subj, pred, obj;
                            if (tp.Subject is NodeMatchPattern)
                            {
                                TNodeID id = _provider.GetID(((NodeMatchPattern)tp.Subject).Node);
                                if (id == null || id.Equals(nullID))
                                {
                                    result = new NullOperator(current.Variables);
                                    break;
                                }
                                else
                                {
                                    subj = new NodeMatchPattern(CreateVirtualNode(id, ((NodeMatchPattern)tp.Subject).Node));
                                }
                            }
                            else
                            {
                                subj = tp.Subject;
                            }
                            if (tp.Predicate is NodeMatchPattern)
                            {
                                TNodeID id = _provider.GetID(((NodeMatchPattern)tp.Predicate).Node);
                                if (id == null || id.Equals(nullID))
                                {
                                    result = new NullOperator(current.Variables);
                                    break;
                                }
                                else
                                {
                                    pred = new NodeMatchPattern(CreateVirtualNode(id, ((NodeMatchPattern)tp.Predicate).Node));
                                }
                            }
                            else
                            {
                                pred = tp.Predicate;
                            }
                            if (tp.Object is NodeMatchPattern)
                            {
                                TNodeID id = _provider.GetID(((NodeMatchPattern)tp.Object).Node);
                                if (id == null || id.Equals(nullID))
                                {
                                    result = new NullOperator(current.Variables);
                                    break;
                                }
                                else
                                {
                                    obj = new NodeMatchPattern(CreateVirtualNode(id, ((NodeMatchPattern)tp.Object).Node));
                                }
                            }
                            else
                            {
                                obj = tp.Object;
                            }
                            patterns.Add(new TriplePattern(subj, pred, obj));
                        }
                        else
                        {
                            // Can't optimize if other pattern types involved
                            return current;
                        }
                    }

                    if (result is NullOperator)
                    {
                        return result;
                    }
                    else if (patterns.Count > 0)
                    {
                        // If any patterns left at end join as a BGP with result so far
                        result = Join.CreateJoin(result, new Bgp(patterns));
                        return result;
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            else if (algebra is ITerminalOperator)
            {
                return algebra;
            }
            else
            {
                return algebra;
            }
        }

        /// <summary>
        /// Transforms an expression so Node terms are virtualised
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <returns></returns>
        public ISparqlExpression Transform(ISparqlExpression expr)
        {
            try
            {
                if (expr.Type == SparqlExpressionType.Primary)
                {
                    return SubstitutePrimaryExpression(expr);
                }
                else
                {
                    return expr.Transform(this);
                }
            }
            catch
            {
                return expr;
            }
        }

        /// <summary>
        /// Substitutes a primary expression which is a Node term for a virtual Node term
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <returns></returns>
        protected ISparqlExpression SubstitutePrimaryExpression(ISparqlExpression expr)
        {
            if (expr.GetType().Equals(_exprType))
            {
                ConstantTerm term = (ConstantTerm)expr;
                INode curr = term.Evaluate(null, 0);
                TNodeID id = _provider.GetID(curr);
                if (id == null || id.Equals(_provider.NullID)) throw new RdfQueryException("Cannot transform the Expression to use Virtual Nodes");
                INode virt = CreateVirtualNode(id, curr);
                return new ConstantTerm(virt);
            }
            else
            {
                return expr;
            }
        }

        /// <summary>
        /// Creates a virtual Node based on a given Value
        /// </summary>
        /// <param name="id">Node ID</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        protected abstract INode CreateVirtualNode(TNodeID id, INode value);

        /// <summary>
        /// Returns that the optimiser is applicable to all queries
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }

        /// <summary>
        /// Returns that the optimiser is applicable to all updates
        /// </summary>
        /// <param name="cmds">Updates</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return true;
        }
    }

    /// <summary>
    /// A concrete implementation of a Virtual Algebra Optimiser where the virtual IDs are simply integers
    /// </summary>
    public class SimpleVirtualAlgebraOptimiser
        : VirtualAlgebraOptimiser<int, int>
    {
        /// <summary>
        /// Creates a new Simple Virtual Algebra Optimiser
        /// </summary>
        /// <param name="provider">Virtual RDF provider</param>
        public SimpleVirtualAlgebraOptimiser(IVirtualRdfProvider<int, int> provider)
            : base(provider) { }

        /// <summary>
        /// Creates a new Virtual Node using the virtual RDF provider this optimiser was instantiated with
        /// </summary>
        /// <param name="id">Virtual Node ID</param>
        /// <param name="value">Node Value</param>
        /// <returns></returns>
        protected override INode CreateVirtualNode(int id, INode value)
        {
            switch (value.NodeType)
            {
                case NodeType.Blank:
                    return new SimpleVirtualBlankNode(null, id, _provider, (IBlankNode)value);
                case NodeType.GraphLiteral:
                    return new SimpleVirtualGraphLiteralNode(null, id, _provider, (IGraphLiteralNode)value);
                case NodeType.Literal:
                    return new SimpleVirtualLiteralNode(null, id, _provider, (ILiteralNode)value);
                case NodeType.Uri:
                    return new SimpleVirtualUriNode(null, id, _provider, (IUriNode)value);
                case NodeType.Variable:
                    return new SimpleVirtualVariableNode(null, id, _provider, (IVariableNode)value);
                default:
                    throw new RdfException("Cannot create Virtual Nodes from unknown Node Types");
            }
        }
    }
}
