using System.Collections.Generic;
using System;
using VDS.RDF.Query;
using VDS.RDF;
namespace  org.topbraid.spin.arq {

/**
 * A QueryIterator produced by a SPIN Magic INode.
 * 
 * This basically walks through the resultset of the body SELECT query.
 * 
 * @author Holger Knublauch
 */
class PFunctionQueryIterator : QueryIteratorBase {
	
	private Binding parentBinding;
	
	private QueryExecution qexec;

	private SparqlResultSet rs;
	
	private List<String> rvs;
	
	private Dictionary<String,Var> vars;
	
	
	PFunctionQueryIterator(SparqlResultSet rs, QueryExecution qexec, Dictionary<String,Var> vars, Binding parentBinding) {
		this.parentBinding = parentBinding;
		this.qexec = qexec;
		this.rs = rs;
		this.rvs = rs.getResultVars();
		this.vars = vars;
	}


	override protected void closeIterator() {
		qexec.close();
	}


	override protected bool hasNextBinding() {
		return rs.MoveNext();
	}


	override protected Binding moveToNextBinding() {
		QuerySolution s = rs.nextSolution();
		BindingMap result = new BindingHashMap(parentBinding);
		foreach(String varName in rvs) {
			INode resultNode = s.get(varName);
			if(resultNode != null) {
				Var var = vars.get(varName);
				if(var != null) {
					result.Add(var, resultNode);
				}
			}
		}
		return result;
	}


	public void output(IndentedWriter outputStream, SerializationContext sCxt) {
	}


	override protected void requestCancel() {
		// TODO: what needs to happen here?
	}
}
}