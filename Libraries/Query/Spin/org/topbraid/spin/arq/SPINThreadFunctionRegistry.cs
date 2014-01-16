using VDS.RDF.Storage;
using System.Threading;
using VDS.RDF.Query.Spin;
using System.Collections.Generic;
using System;
namespace  org.topbraid.spin.arq {

/**
 * An ARQ FunctionRegistry that can be used to associate functions
 * with Threads, so that additional functions from a given IUpdateableStorage can
 * be made visible depending on the SPARQL query thread.
 * 
 * <p>Note that this concept only works if ARQ has been set to single
 * threading, which is done by the static block below.</p>
 * 
 * <p>The contract of this class is very strict to prevent memory leaks:
 * Users always need to make sure that unregister is called as soon
 * as a query (block) ends, and to restore any old SPINThreadFunctions
 * object that was registered before.  So a typical block would be:</p>
 * 
 * <code>
 * 	IUpdateableStorage model = ... a IUpdateableStorage with extra SPIN functions
 * 	SPINThreadFunctions old = SPINThreadFunctionRegistry.register(model);
 * 	try {
 * 		// run SPARQL queries here
 * 	}
 * 	finally {
 * 		SPINThreadFunctionRegistry.unregister(old);
 * 	}</code>
 * 
 * <p>In preparation of the above, the application should start up with code
 * such as</p>
 * 
 * <code>
 * 	FunctionRegistry oldFR = FunctionRegistry.get();
 *  SPINThreadFunctionRegistry threadFR = new SPINThreadFunctionRegistry(oldFR);
 *	FunctionRegistry.set(ARQ.getContext(), threadFR);
 * </code>
 *
 * <p>and do the same for the SPINThreadPropertyFunctionRegistry.</p>
 * 
 * @author Holger Knublauch
 */
public class SPINThreadFunctionRegistry : FunctionRegistry {

	static SPINThreadFunctionRegistry()
    {
		// Suppress multi-threading (PatternStage-stuff)
		StageBuilder.setGenerator(ARQ.getContext(), new StageGenerator());//{
        //    public QueryIterator execute(BasicPattern pattern, QueryIterator input,
        //            ExecutionContext execCxt) {
        //        return QueryIterBlockTriples.create(input, pattern, execCxt);
        //    }
        //});
	}
	
	private static ThreadLocal<SPINThreadFunctions> localFunctions = new ThreadLocal<SPINThreadFunctions>();
	
	/**
	 * Registers a set of extra SPIN functions from a given IUpdateableStorage for the current
	 * Thread.
	 * @param model  the IUpdateableStorage containing the SPIN functions
	 * @return any old object that was registered for the current Thread, so that
	 *         the old value can be restored when done.
	 */
	public static SPINThreadFunctions register(IUpdateableStorage model) {
		SPINThreadFunctions old = localFunctions.get();
		SPINThreadFunctions neo = new SPINThreadFunctions(model);
		localFunctions.set(neo);
		return old;
	}
	
	
	/**
	 * Unregisters the current IUpdateableStorage for the current Thread.
	 * @param old  the old functions that shall be restored or null
	 */
	public static void unregister(SPINThreadFunctions old) {
		if(old != null) {
			localFunctions.set(old);
		}
		else {
			localFunctions.Remove();
		}
	}
	
	public static SPINThreadFunctions getFunctions() {
		return localFunctions.get();
	}
	
	private FunctionRegistry registry;
	
	public SPINThreadFunctionRegistry(FunctionRegistry registry) {
		this.registry = registry;
	}


	override public FunctionFactory get(String uri) {
		FunctionFactory b = registry.get(uri);
		if(b != null) {
			return b;
		}
		SPINThreadFunctions functions = localFunctions.get();
		if(functions != null) {
			FunctionFactory ff = functions.getFunctionFactory(uri);
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
		else {
			return get(uri) != null;
		}
	}


	override public IEnumerator<String> keys() {
		// Note: only returns registry keys
		return registry.keys();
	}


	override public void put(String uri, Type funcClass) {
		registry[uri] = funcClass;
	}


	override public void put(String uri, FunctionFactory f) {
		registry[uri] = f;
	}


	override public FunctionFactory remove(String uri) {
		return registry.Remove(uri);
	}
}
}