using VDS.RDF.Storage;
using System.Collections.Generic;
using System;
using VDS.RDF.Query.PropertyFunctions;
using org.topbraid.spin.model;
using org.topbraid.spin.util;

using VDS.RDF.Query.Spin;
namespace  org.topbraid.spin.arq {

/**
 * A helper object that can be used to register SPARQL functions
 * (and property functions) per thread, e.g. per servlet request.
 * 
 * @author Holger Knublauch
 */
public class SPINThreadFunctions {
	
	private Dictionary<String,FunctionFactory> functionsCache = new Dictionary<String,FunctionFactory>();
	
	private Dictionary<String,PropertyFunctionFactory> pfunctionsCache = new Dictionary<String,PropertyFunctionFactory>();

	private IUpdateableStorage model;
	
	
	SPINThreadFunctions(IUpdateableStorage model) {
		this.model = model;
	}
	
	
	FunctionFactory getFunctionFactory(String uri) {
		FunctionFactory old = null;
        if(functionsCache.ContainsKey(uri)) {
            old = functionsCache[uri];
        }
		if(old != null) {
			return old;
		}
		else if(functionsCache.ContainsKey(uri)) {
			return null;
		}
		else {
			return getFunctionFactoryFromModel(uri);
		}
	}
	
	
	PropertyFunctionFactory getPFunctionFactory(String uri) {
		PropertyFunctionFactory old = null;
        if(pfunctionsCache.ContainsKey(uri)) {
            old = pfunctionsCache[uri];
		}
        if(old != null) {
			return old;
		}
		else if(pfunctionsCache.ContainsKey(uri)) {
			return null;
		}
		else {
			return getPropertyFunctionFactoryFromModel(uri);
		}
	}


	private FunctionFactory getFunctionFactoryFromModel(String uri) {
		Function spinFunction = model.getResource(uri).As(Function);
		if(JenaUtil.hasIndirectType(spinFunction, SPIN.Function.inModel(spinFunction.getModel()))) {
			FunctionFactory arqFunction = SPINFunctionDrivers.get().create(spinFunction);
			if(arqFunction != null) {
				functionsCache[uri] = arqFunction;
				return arqFunction;
			}
		}
		// Remember failed attempt for future
		functionsCache[uri] = null;
		return null;
	}


	private PropertyFunctionFactory getPropertyFunctionFactoryFromModel(String uri) {
		Function spinFunction = model.getResource(uri).As(Function);
		if(JenaUtil.hasIndirectType(spinFunction, SPIN.MagicProperty.inModel(spinFunction.getModel()))) {
			if(spinFunction.hasProperty(SPIN.body)) {
				/*sealed*/ SPINARQPFunction arqFunction = new SPINARQPFunction(spinFunction);
				pfunctionsCache[uri] = arqFunction;
				return arqFunction;
			}
		}
		// Remember failed attempt for future
		pfunctionsCache[uri] = null;
		return null;
	}
}
}