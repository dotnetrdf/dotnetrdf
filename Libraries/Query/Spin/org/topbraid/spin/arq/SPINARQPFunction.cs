/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Storage;
using org.topbraid.spin.model;
using System.Collections.Generic;
using VDS.RDF;
using System;
using System.Text;
namespace  org.topbraid.spin.arq {
/**
 * An ARQ PropertyFunction based on a spin:MagicProperty.
 * For convenience, also implements PropertyFunctionFactory.
 *
 * @author Holger Knublauch
 */
public class SPINARQPFunction : PropertyFunctionBase , PropertyFunctionFactory {
	
	private com.hp.hpl.jena.query.Query arqQuery;

	private String queryString;
	
	private List<String> objectVarNames = new List<String>();

	
	public SPINARQPFunction(Function functionCls) {
		try {
			Select spinQuery = (Select) functionCls.getBody();
			List<INode> resultVariables = spinQuery.getResultVariables();
			if(resultVariables == null) {
				throw new ArgumentException("SELECT * not supported in magic properties");
			}
			foreach(INode var in resultVariables) {
				if(var is Variable) {
					objectVarNames.Add(((Variable)var).getName());
				}
				else {
					throw new ArgumentException("SELECT with expressions not supported in magic properties");
				}
			}
			queryString = ARQFactory.get().createCommandString(spinQuery);
			int selectStart = queryString.IndexOf("SELECT ");
			int eol = queryString.IndexOf('\n', selectStart);
			
			StringBuilder sb = new StringBuilder(queryString.substring(0, eol));
			foreach(Argument arg in functionCls.getArguments(true)) {
				sb.Append(" ?");
				sb.Append(arg.getVarName());
			}
			sb.Append(queryString.substring(eol));
			
			arqQuery = ARQFactory.get().createQuery(sb.ToString());
		}
		catch(Exception t) {
			//t.printStackTrace();
			throw new ArgumentException("Function definition does not contain a valid body", t);
		}
	}

	
	public PropertyFunction create(String arg0) {
		return this;
	}

	
	override public QueryIterator exec(Binding binding, PropFuncArg argSubject, INode predicate,
			PropFuncArg argObject, ExecutionContext context) {

		argObject = Substitute.substitute(argObject, binding);
		argSubject = Substitute.substitute(argSubject, binding);
		
		ExprList subjectExprList = argSubject.asExprList(argSubject);
		ExprList objectExprList = argObject.asExprList(argObject);
		
		QueryIterConcat existingValues = null;
		MagicPropertyPolicy.Policy policy = MagicPropertyPolicy.Policy.QUERY_RESULTS_ONLY;
		// Handle cases with one argument on both sides (S, P, O)
		if(objectExprList.size() == 1 && subjectExprList.size() == 1) {
			Expr subject = subjectExprList.get(0);
			Expr obj = objectExprList.get(0);
			if(subject.isVariable() || obj.isVariable()) {
				
				INode matchSubject = null;
				if(subject.isConstant()) {
					INode n = subject.getConstant();
					if(n.isURI() || n.isBlank()) {
						matchSubject = n;
					}
				}
				
				INode matchObject = null;
				if(obj.isConstant()) {
					matchObject = obj.getConstant();
				}
				
				Graph queryGraph = context.getActiveGraph();
				policy = MagicPropertyPolicy.get().getPolicy(predicate.Uri, queryGraph, matchSubject, matchObject);

				if(policy != MagicPropertyPolicy.Policy.QUERY_RESULTS_ONLY) {
					IEnumerator<Triple> it = queryGraph.find(matchSubject, predicate, matchObject);
					while(it.MoveNext()) {
						Triple triple = it.Current;
						BindingMap map = new BindingHashMap(binding);
						if(subject.isVariable()) {
							map.Add(subject.asVar(), triple.Subject);
						}
						if(obj.isVariable()) {
							map.Add(obj.asVar(), triple.Object);
						}
						if(existingValues == null) {
							existingValues = new QueryIterConcat(context);
						}
						QueryIterator nested = IterLib.result(map, context);
						existingValues.Add(nested);
					}
				}
			}
		}
		
		if(policy != MagicPropertyPolicy.Policy.TRIPLES_ONLY) {
			
			IUpdateableStorage model = ModelFactory.createModelForGraph(context.getActiveGraph());
			INode t = binding.get(Var.alloc(SPIN.THIS_VAR_NAME));
			SparqlResultSet bindings = new SparqlResultSet();
			if(t != null) {
				bindings.Add(SPIN.THIS_VAR_NAME, model.asRDFNode(t));
			}
	
			// Map object expressions to original objectVarNames
			Dictionary<String,Var> vars = new Dictionary<String,Var>();
			for(int i = 0; i < objectVarNames.size() && i < objectExprList.size(); i++) {
				Expr expr = objectExprList.get(i);
				String objectVarName = objectVarNames.get(i);
				if(expr.isVariable() && !binding.Contains(expr.asVar())) {
					Var var = expr.asVar();
					vars[objectVarName] = var;
				}
				else {
		        	NodeValue x = expr.eval(binding, context);
		        	if(x != null) {
		        		bindings.Add(objectVarName, model.asRDFNode(x));
		        	}
				}
			}
			
			// Map subject expressions to arg1 etc
			for(int i = 0; i < subjectExprList.size(); i++) {
				String subjectVarName = "arg" + (i + 1);
				Expr expr = subjectExprList.get(i);
				if(expr.isVariable() && !binding.Contains(expr.asVar())) {
					Var var = expr.asVar();
					vars[subjectVarName]= var;
				}
				else {
		        	NodeValue x = expr.eval(binding, context);
		        	if(x != null) {
		        		bindings.Add(subjectVarName, model.asRDFNode(x));
		        	}
				}
			}
			
			// Execute SELECT query and wrap it with a custom iterator
			Dataset newDataset = new DatasetWithDifferentDefaultModel(model, DatasetImpl.wrap(context.getDataset()));
			QueryExecution qexec = ARQFactory.get().createQueryExecution(arqQuery, newDataset, bindings);
			SparqlResultSet rs = qexec.execSelect();
			QueryIterator it = new PFunctionQueryIterator(rs, qexec, vars, binding);
			if(existingValues != null) {
				existingValues.Add(it);
				return existingValues;
			}
			else {
				return it;
			}
		}
		else if(existingValues != null) {
			return existingValues;
		}
		else {
			return IterLib.result(binding, context);
		}
	}
}
}