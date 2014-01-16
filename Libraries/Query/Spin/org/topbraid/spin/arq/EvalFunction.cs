/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Storage;
using System;
using VDS.RDF;
using System.Text;
using org.topbraid.spin.statistics;

using org.topbraid.spin.util;
using org.topbraid.spin.model;
using VDS.RDF.Query;
using org.topbraid.spin.vocabulary;
namespace org.topbraid.spin.arq
{

    /**
     * The SPARQL function spin:eval.
     * 
     * The first argument is a SPIN expression, e.g. a function call or variable.
     * All other arguments must come in pairs, alternating between an argument property
     * and its value, e.g.
     * 
     *  	spin:eval(ex:myInstance, sp:arg3, "value")
     *  
     * The expression will be evaluated with all bindings from the property-value pairs above.
     * 
     * @author Holger Knublauch
     */
    public class EvalFunction : AbstractFunction, FunctionFactory
    {


        private void addStatistics(INode[] nodes, FunctionEnv env, DateTime startTime, String expr, INode result)
        {
            DateTime endTime = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append("spin:eval(");
            sb.Append(expr);
            for (int i = 1; i < nodes.Length; i++)
            {
                sb.Append(", ");
                if (nodes[i] == null)
                {
                    sb.Append("?arg" + (i + 1));
                }
                else
                {
                    sb.Append(SSE.format(nodes[i], env.getActiveGraph().getPrefixMapping()));
                }
            }
            sb.Append(")");
            if (result != null)
            {
                sb.Append(" = ");
                sb.Append(FmtUtils.stringForNode(result, env.getActiveGraph().getPrefixMapping()));
            }
            SPINStatistics stats = new SPINStatistics(sb.ToString(),
                    "(spin:eval)", endTime - startTime, startTime, SPIN.eval);
            SPINStatisticsManager.get().addSilently(stats);
        }


        override public Function create(String uri)
        {
            return this;
        }


        override public NodeValue exec(INode[] nodes, FunctionEnv env)
        {

            IUpdateableStorage baseModel = ModelFactory.createModelForGraph(env.getActiveGraph());
            INode exprNode = nodes[0];
            if (exprNode == null)
            {
                throw new ExprEvalException("No expression specified");
            }
            else if (exprNode is ILiteralNode)
            {
                return NodeValue.makeNode(exprNode);
            }
            else
            {
                IUpdateableStorage model = baseModel;
                if (!model.Contains(SPIN._arg1, RDF.type, SP.Variable))
                {
                    MultiUnion multiUnion = JenaUtil.createMultiUnion(new Graph[] {
						env.getActiveGraph(),
						SPIN.getModel().getGraph()
				});
                    model = ModelFactory.createModelForGraph(multiUnion);
                }
                INode exprRDFNode = (INode)model.asRDFNode(exprNode);
                SparqlResultSet bindings = getBindings(nodes, model);
                org.topbraid.spin.model.Query spinQuery = SPINFactory.asQuery(exprRDFNode);
                Dataset newDataset = new DatasetWithDifferentDefaultModel(model, DatasetImpl.wrap(env.getDataset()));
                DateTime startTime = DateTime.Now;
                if (spinQuery is Select || spinQuery is Ask)
                {
                    Query query = ARQFactory.get().createQuery(spinQuery);
                    QueryExecution qexec = ARQFactory.get().createQueryExecution(query, newDataset, bindings);
                    if (query.isAskType())
                    {
                        bool result = qexec.execAsk();
                        if (SPINStatisticsManager.get().isRecording() && SPINStatisticsManager.get().isRecordingSPINFunctions())
                        {
                            addStatistics(nodes, env, startTime, "ASK...", result ? JenaDatatypes.TRUE : JenaDatatypes.FALSE);
                        }
                        return NodeValue.makeBoolean(result);
                    }
                    else
                    {
                        SparqlResultSet rs = qexec.execSelect();
                        try
                        {
                            String var = rs.getResultVars().get(0);
                            if (rs.MoveNext())
                            {
                                INode result = rs.Current.get(var);
                                if (SPINStatisticsManager.get().isRecording() && SPINStatisticsManager.get().isRecordingSPINFunctions())
                                {
                                    addStatistics(nodes, env, startTime, "SELECT...", result);
                                }
                                if (result != null)
                                {
                                    return NodeValue.makeNode(result);
                                }
                            }
                        }
                        finally
                        {
                            qexec.close();
                        }
                    }
                }
                else
                {
                    INode expr = SPINFactory.asExpression(exprRDFNode);
                    INode result = SPINExpressions.evaluate((INode)expr, newDataset, bindings);
                    if (SPINStatisticsManager.get().isRecording() && SPINStatisticsManager.get().isRecordingSPINFunctions())
                    {
                        addStatistics(nodes, env, startTime, SPINExpressions.getExpressionString(expr), result);
                    }
                    if (result != null)
                    {
                        return NodeValue.makeNode(result);
                    }
                }
                throw new ExprEvalException("Expression has no result");
            }
        }


        private SparqlResultSet getBindings(INode[] nodes, IUpdateableStorage model)
        {
            SparqlResultSet bindings = new SparqlResultSet();
            for (int i = 1; i < nodes.length - 1; i += 2)
            {
                INode property = nodes[i];
                INode value = nodes[i + 1];
                if (value != null)
                {
                    bindings.Add(property.getLocalName(), model.asRDFNode(value));
                }
            }
            return bindings;
        }
    }
}