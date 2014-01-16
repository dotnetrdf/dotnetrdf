using VDS.RDF;
using org.topbraid.spin.util;
using System.Collections.Generic;
using org.topbraid.spin.model;

namespace  org.topbraid.spin.arq {

/**
 * The singleton that creates ARQ FunctionFactories from SPIN functions.
 * Can be used by applications to install a different singleton with support
 * for different kinds of functions, such as SPINx.
 * 
 * @author Holger Knublauch
 */
public class SPINFunctionDrivers : SPINFunctionDriver {

	private static SPINFunctionDrivers singleton = new SPINFunctionDrivers();
	
	public static SPINFunctionDrivers get() {
		return singleton;
	}
	
	public static void set(SPINFunctionDrivers value) {
		singleton = value;
	}
	
	
	private Dictionary<INode,SPINFunctionDriver> drivers = new Dictionary<INode,SPINFunctionDriver>();
	
	SPINFunctionDrivers() {
		drivers[SPIN.body] = new SPINBodyFunctionDriver();
	}


	override public SPINFunctionFactory create(Function function) {
		SPINFunctionDriver driver = getDriver(function);
		if(driver != null) {
			return driver.create(function);
		}
		else {
			return null;
		}
	}
	

	/**
	 * Registers a new SPINFunctionDriver for a given key predicate.
	 * For example, SPARQLMotion functions are recognized via sm:body.
	 * Any previous entry will be overwritten.
	 * @param predicate  the key predicate
	 * @param driver  the driver to register
	 */
	public void register(INode predicate, SPINFunctionDriver driver) {
		drivers[predicate] = driver;
	}
	
	
	private SPINFunctionDriver getDriver(Function spinFunction) {
		JenaUtil.setGraphReadOptimization(true);
		try {
			SPINFunctionDriver direct = getDirectDriver(spinFunction);
			if(direct != null) {
				return direct;
			}
			else {
				return getDriver(spinFunction, new HashSet<INode>());
			}
		}
		finally {
			JenaUtil.setGraphReadOptimization(false);
		}
	}
	
	
	private SPINFunctionDriver getDriver(INode spinFunction, HashSet<INode> reached) {
		reached.Add(spinFunction);
		foreach(INode superClass in JenaUtil.getSuperClasses(spinFunction)) {
			if(!reached.Contains(spinFunction)) {
				SPINFunctionDriver superFunction = getDirectDriver(superClass);
				if(superFunction != null) {
					return superFunction;
				}
			}
		}
		return null;
	}
	
	
	private SPINFunctionDriver getDirectDriver(INode spinFunction) {
		if(!spinFunction.hasProperty(SPIN.abstract_, JenaDatatypes.TRUE)) {
			IEnumerator<Triple> it = spinFunction.listProperties();
			while(it.MoveNext()) {
				Triple s = it.Current;
                if (drivers.ContainsKey(s.Predicate))
                {
                    /*sealed*/
                    SPINFunctionDriver driver = drivers[s.Predicate];
                    if (driver != null)
                    {
                        it.Dispose();
                        return driver;
                    }
                }
			}
		}
		return null;
	}
}
}