/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Storage;
using System.Collections.Generic;
using System;
using org.topbraid.spin.model;
using org.topbraid.spin.util;

using org.topbraid.spin.system;
using org.topbraid.spin.statistics;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query.Spin;
namespace  org.topbraid.spin.arq {

/**
 * An ARQ function that delegates its functionality into a user-defined
 * SPIN function. 
 * 
 * @author Holger Knublauch
 */
public class SPINARQFunction : Function, SPINFunctionFactory {
	
	private Query arqQuery;
	
	private List<String> argNames = new List<String>();

    private List<INode> argNodes = new List<INode>();
	
	private String queryString;
	
	private Function spinFunction;
	

	/**
	 * Constructs a new SPINARQFunction based on a given SPIN Function.
	 * The spinFunction model be associated with the IUpdateableStorage containing
	 * the triples of its definition.
	 * @param spinFunction  the SPIN function
	 */
	public SPINARQFunction(Function spinFunction) {
		
		this.spinFunction = spinFunction;
		
		try {
			Query spinQuery = (Query) spinFunction.getBody();
			queryString = ARQFactory.get().createCommandString(spinQuery);
			arqQuery = ARQFactory.get().createQuery(queryString);
			
			// TODO if above three lines never involve writes, then we can move the optimization up 
			// and the finally block onto the outer try, which would be easier to read.
			JenaUtil.setGraphReadOptimization(true);
			try {
				foreach(Argument arg in spinFunction.getArguments(true)) {
					String varName = arg.getVarName();
					if(varName == null) {
						throw new InvalidOperationException("Argument " + arg + " of " + spinFunction + " does not have a valid predicate");
					}
					argNames.Add(varName);
					argNodes.Add(arg.Predicate);
				}
			}
			finally {
				JenaUtil.setGraphReadOptimization(false);
			}
		}
		catch(Exception ex) {
			throw new ArgumentException("Function " + spinFunction.Uri + " does not define a valid body", ex);
		}
	}
	

	public void build(String uri, ExprList args) {
	}

	
	public com.hp.hpl.jena.sparql.function.Function create(String uri) {
		return this;
	}

	
	public NodeValue exec(Binding binding, ExprList args, String uri, FunctionEnv env) {
		IUpdateableStorage model = ModelFactory.createModelForGraph(env.getActiveGraph());
		SparqlResultSet bindings = new SparqlResultSet();
        INode t = binding.get(Var.alloc(SPIN.THIS_VAR_NAME));
		if(t != null) {
			bindings.Add(SPIN.THIS_VAR_NAME, model.asRDFNode(t));
		}
		for(int i = 0; i < args.size(); i++) {
			Expr expr = args.get(i);
			if(expr != null && (!expr.isVariable() || binding.Contains(expr.asVar()))) {
	        	NodeValue x = expr.eval(binding, env);
	        	if(x != null) {
	        		String argName;
	        		if(i < argNames.size()) {
	        			argName = argNames.get(i);
	        		}
	        		else {
	        			argName = "arg" + (i + 1);
	        		}
	        		bindings.Add(argName, model.asRDFNode(x));
	        	}
			}
		}
		
		if(SPINArgumentChecker.get() != null) {
			SPINArgumentChecker.get().check(spinFunction, bindings);
		}
		
		if(SPINStatisticsManager.get().isRecording() && SPINStatisticsManager.get().isRecordingSPINFunctions()) {
			StringBuilder sb = new StringBuilder();
			sb.Append("SPIN Function ");
			sb.Append(SSE.format(UriFactory.Create(uri), model));
			sb.Append("(");
			for(int i = 0; i < args.size(); i++) {
				if(i > 0) {
					sb.Append(", ");
				}
				Expr expr = args.get(i);
				expr = Substitute.substitute(expr, binding);
				if(expr == null) {
					sb.Append("?unbound");
				}
				else {
					ByteArrayOutputStream bos = new ByteArrayOutputStream();
					IndentedWriter iOut = new IndentedWriter(bos);
					ExprUtils.fmtSPARQL(iOut, expr, new SerializationContext(model));
					iOut.flush();
					sb.Append(bos.ToString());
				}
			}
			sb.Append(")");
			DateTime startTime = DateTime.Now;
			NodeValue result;
			try {
				result = executeBody(DatasetImpl.wrap(env.getDataset()), model, bindings);
				sb.Append(" = ");
				sb.Append(FmtUtils.stringForNode(result, model));
			}
			catch(ExprEvalException ex) {
				sb.Append(" : ");
				sb.Append(ex.getLocalizedMessage());
				throw ex;
			}
			finally {
				DateTime endTime = DateTime.Now;
				SPINStatistics stats = new SPINStatistics(sb.ToString(), queryString, endTime - startTime, startTime, UriFactory.Create(uri));
				SPINStatisticsManager.get().addSilently(Collections.singleton(stats));
			}
			return result;
		}
		else {
			return executeBody(DatasetImpl.wrap(env.getDataset()), model, bindings);
		}
	}


	public NodeValue executeBody(IUpdateableStorage model, QuerySolution bindings) {
		return executeBody(null, model, bindings);
	}
	
	
	public NodeValue executeBody(Dataset dataset, IUpdateableStorage defaultModel, QuerySolution bindings) {
		QueryExecution qexec;
		if(dataset != null) {
			Dataset newDataset = new DatasetWithDifferentDefaultModel(defaultModel, dataset);
			qexec = ARQFactory.get().createQueryExecution(arqQuery, newDataset);
		}
		else {
			qexec = ARQFactory.get().createQueryExecution(arqQuery, defaultModel);
		}
		qexec.setInitialBinding(bindings);
		if(arqQuery.isAskType()) {
			bool result = qexec.execAsk();
			qexec.close();
			return NodeValue.makeBoolean(result);
		}
		else if(arqQuery.isSelectType()) {
			SparqlResultSet rs = qexec.execSelect();
			try {
				if(rs.MoveNext()) {
					QuerySolution s = rs.nextSolution();
					List<String> resultVars = rs.getResultVars();
					String varName = resultVars.get(0);
					INode resultNode = s.get(varName);
					if(resultNode != null) {
						return NodeValue.makeNode(resultNode);
					}
				}
				throw new ExprEvalException("Empty result set for SPIN function " + queryString);
			}
			finally {
				qexec.close();
			}
		}
		else {
			throw new ExprEvalException("Body must be ASK or SELECT query");
		}
	}
	
	
	/**
	 * Gets the names of the declared arguments, in order from left to right.
	 * @return the arguments
	 */
	public String[] getArgNames() {
		return argNames.toArray(new String[0]);
	}


    public INode[] getArgPropertyNodes()
    {
        return argNodes.toArray(new INode[0]);
	}
	

	/**
	 * Gets the Jena Query object for execution.
	 * @return the Jena Query
	 */
	public com.hp.hpl.jena.query.Query getBodyQuery() {
		return arqQuery;
	}
}
}