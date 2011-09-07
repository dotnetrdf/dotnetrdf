/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
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
        private Type _exprType = typeof(NodeExpressionTerm);

        /// <summary>
        /// Creates a new Virtual Algebra Optimiser
        /// </summary>
        /// <param name="provider">Virtual RDF Provider</param>
        public VirtualAlgebraOptimiser(IVirtualRdfProvider<TNodeID, TGraphID> provider)
        {
            this._provider = provider;
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
                    TNodeID nullID = this._provider.NullID;

                    for (int i = 0; i < current.PatternCount; i++)
                    {
                        if (ps[i] is FilterPattern || ps[i] is BindPattern)
                        {
                            //First ensure that if we've found any other Triple Patterns up to this point
                            //we dump this into a BGP and join with the result so far
                            if (patterns.Count > 0)
                            {
                                result = Join.CreateJoin(result, new Bgp(patterns));
                                patterns.Clear();
                            }
                            if (ps[i] is FilterPattern)
                            {
                                result = new Filter(result, ((FilterPattern)ps[i]).Filter);
                            }
                            else
                            {
                                BindPattern bind = (BindPattern)ps[i];
                                result = new Extend(result, bind.AssignExpression, bind.VariableName);
                            }
                        }
                        else
                        {
                            //Convert Terms in the Pattern into Virtual Nodes
                            TriplePattern tp = (TriplePattern)ps[i];
                            PatternItem subj, pred, obj;
                            if (tp.Subject is NodeMatchPattern)
                            {
                                TNodeID id = this._provider.GetID(((NodeMatchPattern)tp.Subject).Node);
                                if (id == null || id.Equals(nullID))
                                {
                                    result = new NullOperator(current.Variables);
                                    break;
                                }
                                else
                                {
                                    subj = new NodeMatchPattern(this.CreateVirtualNode(id, ((NodeMatchPattern)tp.Subject).Node));
                                }
                            }
                            else
                            {
                                subj = tp.Subject;
                            }
                            if (tp.Predicate is NodeMatchPattern)
                            {
                                TNodeID id = this._provider.GetID(((NodeMatchPattern)tp.Predicate).Node);
                                if (id == null || id.Equals(nullID))
                                {
                                    result = new NullOperator(current.Variables);
                                    break;
                                }
                                else
                                {
                                    pred = new NodeMatchPattern(this.CreateVirtualNode(id, ((NodeMatchPattern)tp.Predicate).Node));
                                }
                            }
                            else
                            {
                                pred = tp.Predicate;
                            }
                            if (tp.Object is NodeMatchPattern)
                            {
                                TNodeID id = this._provider.GetID(((NodeMatchPattern)tp.Object).Node);
                                if (id == null || id.Equals(nullID))
                                {
                                    result = new NullOperator(current.Variables);
                                    break;
                                }
                                else
                                {
                                    obj = new NodeMatchPattern(this.CreateVirtualNode(id, ((NodeMatchPattern)tp.Object).Node));
                                }
                            }
                            else
                            {
                                obj = tp.Object;
                            }
                            patterns.Add(new TriplePattern(subj, pred, obj));
                        }
                    }

                    if (result is NullOperator)
                    {
                        return result;
                    }
                    else if (patterns.Count == current.PatternCount)
                    {
                        //If count of remaining patterns same as original pattern count there was no optimisation
                        //to do so return as is
                        return current;
                    }
                    else if (patterns.Count > 0)
                    {
                        //If any patterns left at end join as a BGP with result so far
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
                    return this.SubstitutePrimaryExpression(expr);
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
            if (expr.GetType().Equals(this._exprType))
            {
                NodeExpressionTerm term = (NodeExpressionTerm)expr;
                INode curr = term.Value(null, 0);
                TNodeID id = this._provider.GetID(curr);
                if (id == null || id.Equals(this._provider.NullID)) throw new RdfQueryException("Cannot transform the Expression to use Virtual Nodes");
                INode virt = this.CreateVirtualNode(id, curr);
                return new NodeExpressionTerm(virt);
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
                    return new SimpleVirtualBlankNode(null, id, this._provider, (IBlankNode)value);
                case NodeType.GraphLiteral:
                    return new SimpleVirtualGraphLiteralNode(null, id, this._provider, (IGraphLiteralNode)value);
                case NodeType.Literal:
                    return new SimpleVirtualLiteralNode(null, id, this._provider, (ILiteralNode)value);
                case NodeType.Uri:
                    return new SimpleVirtualUriNode(null, id, this._provider, (IUriNode)value);
                case NodeType.Variable:
                    return new SimpleVirtualVariableNode(null, id, this._provider, (IVariableNode)value);
                default:
                    throw new RdfException("Cannot create Virtual Nodes from unknown Node Types");
            }
        }
    }
}
