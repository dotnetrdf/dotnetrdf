/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using org.topbraid.spin.statistics;
using System.Text;
using VDS.RDF;
using org.topbraid.spin.model;
namespace  org.topbraid.spin.arq {

/**
 * Base implementation of Function comparable to Jena's FunctionBase.
 * 
 * @author Holger Knublauch
 */
public abstract class AbstractFunction : Function {

	public void build(String uri, ExprList args) {
	}

	
	public NodeValue exec(Binding binding, ExprList args, String uri, FunctionEnv env) {
		INode[] nodes = new INode[args.size()];
		for(int i = 0; i < args.size(); i++) {
            Expr e = args.get(i);
            try {
            	if(e != null && (!e.isVariable() || (e.isVariable() && binding.get(e.asVar()) != null)) ) {
	            	NodeValue x = e.eval(binding, env);
	            	if (x != null) {
						nodes[i] = x;
					} 
            	}
            }
            catch(ExprEvalException ex) {
            	throw ex;
            }
            catch(Exception ex) {
            	throw new ArgumentException("Exception during function evaluation", ex);
            }
        }
		if(SPINStatisticsManager.get().isRecording() && SPINStatisticsManager.get().isRecordingNativeFunctions()) {
			StringBuilder sb = new StringBuilder();
			sb.Append("SPARQL Function ");
			sb.Append(SSE.format(UriFactory.Create(uri), env.getActiveGraph().getPrefixMapping()));
			sb.Append("(");
			for(int i = 0; i < nodes.length; i++) {
				if(i > 0) {
					sb.Append(", ");
				}
				if(nodes[i] == null) {
					sb.Append("?arg" + (i + 1));
				}
				else {
					sb.Append(SSE.format(nodes[i], env.getActiveGraph().getPrefixMapping()));
				}
			}
			sb.Append(")");
			DateTime startTime = DateTime.Now;
			NodeValue result;
			try {
				result = exec(nodes, env);
				sb.Append(" = ");
				sb.Append(FmtUtils.stringForNode(result, env.getActiveGraph().getPrefixMapping()));
			}
			catch(ExprEvalException ex) {
				sb.Append(" : ");
				sb.Append(ex.getLocalizedMessage());
				throw ex;
			}
			finally {
				DateTime endTime = DateTime.Now;
				SPINStatistics stats = new SPINStatistics(sb.ToString(), 
						"(Native built-in function)", endTime - startTime, startTime, UriFactory.Create(uri));
				SPINStatisticsManager.get().addSilently(Collections.singleton(stats));
			}
			return result;
		}
		else {
			return exec(nodes, env);
		}
	}
	
	
	protected abstract NodeValue exec(INode[] nodes, FunctionEnv env);
}
}