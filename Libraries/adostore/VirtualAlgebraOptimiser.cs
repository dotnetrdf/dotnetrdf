using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Storage.Virtualisation;

namespace VDS.RDF.Query.Optimisation
{
    public abstract class VirtualAlgebraOptimiser<TNodeID, TGraphID> : IAlgebraOptimiser, IExpressionTransformer
    {
        protected IVirtualRdfProvider<TNodeID, TGraphID> _provider;
        private Type _exprType = typeof(NodeExpressionTerm);

        public VirtualAlgebraOptimiser(IVirtualRdfProvider<TNodeID, TGraphID> provider)
        {
            this._provider = provider;
        }

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

        protected abstract INode CreateVirtualNode(TNodeID id, INode value);

        public bool IsApplicable(SparqlQuery q)
        {
            return true;
        }
    }

    public class SimpleVirtualAlgebraOptimiser : VirtualAlgebraOptimiser<int, int>
    {
        public SimpleVirtualAlgebraOptimiser(IVirtualRdfProvider<int, int> provider)
            : base(provider) { }

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
