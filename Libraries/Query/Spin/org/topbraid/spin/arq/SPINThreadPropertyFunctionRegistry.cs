using VDS.RDF.Storage;
using System;
using System.Collections.Generic;
using VDS.RDF.Query.PropertyFunctions;


namespace  org.topbraid.spin.arq {
/**
 * An ARQ PropertyFunctionRegistry that can be used to associate functions
 * with Threads, so that additional functions from a given IUpdateableStorage can
 * be made visible depending on the SPARQL query thread.
 * 
 * Note that this concept only works if ARQ has been set to single
 * threading, which is done by the static block below.
 * 
 * The contract of this class is very strict to prevent memory leaks:
 * Users always need to make sure that unregister is called as soon
 * as a query (block) ends.
 * 
 * @author Holger Knublauch
 */
public class SPINThreadPropertyFunctionRegistry : PropertyFunctionRegistry {
	
	private PropertyFunctionRegistry registry;
	
	
	public SPINThreadPropertyFunctionRegistry(PropertyFunctionRegistry registry) {
		this.registry = registry;
	}


	override public PropertyFunctionFactory get(String uri) {
		PropertyFunctionFactory b = registry.get(uri);
		if(b != null) {
			return b;
		}
		SPINThreadFunctions functions = SPINThreadFunctionRegistry.getFunctions();
		if(functions != null) {
			PropertyFunctionFactory ff = functions.getPFunctionFactory(uri);
			if(ff != null) {
				return ff;
			}
		}
		return null;
	}


	override public bool isRegistered(String uri) {
		if(registry.isRegistered(uri)) {
			return true;
		}
		return get(uri) != null;
	}


	override public IEnumerator<String> keys() {
		// Note: only includes registry keys
		return registry.keys();
	}


	override public bool manages(String uri) {
		if(registry.manages(uri)) {
			return true;
		}
		else {
			return get(uri) != null;
		}
	}


	override public void put(String uri, Type extClass) {
		registry.put(uri, extClass);
	}


	override public void put(String uri, PropertyFunctionFactory factory) {
		registry.put(uri, factory);
	}


	override public PropertyFunctionFactory remove(String uri) {
		return registry.Remove(uri);
	}
}
}