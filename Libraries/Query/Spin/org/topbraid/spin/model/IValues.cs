using System.Collections.Generic;
using System;
using VDS.RDF;
namespace org.topbraid.spin.model
{
    /**
     * A VALUES element (inside of a WHERE clause).
     * 
     * @author Holger Knublauch
     */
    public interface IValues : IElement
    {

        /**
         * Gets the bindings (rows), from top to bottom as entered.
         * @return the Bindings
         */
        List<Dictionary<String, IResource>> getBindings();

        /**
         * Gets the names of the declared variables, ordered as entered.
         * @return the variable names
         */
        List<String> getVarNames();
    }
}