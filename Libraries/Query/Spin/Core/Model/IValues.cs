using System.Collections.Generic;
using System;
using VDS.RDF;
namespace VDS.RDF.Query.Spin.Model
{
    /**
     * A VALUES element (inside of a WHERE clause).
     * 
     * @author Holger Knublauch
     */
    public interface IValuesResource : IElementResource
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