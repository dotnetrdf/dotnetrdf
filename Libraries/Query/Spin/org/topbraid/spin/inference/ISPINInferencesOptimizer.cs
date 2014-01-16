using System.Collections.Generic;
using VDS.RDF;
using org.topbraid.spin.util;
using org.topbraid.spin.model;
namespace org.topbraid.spin.inference
{
    /**
     * An interface for objects that can pre-process a set of rules, usually to optimize
     * the performance of rule execution.
     * 
     * @author Holger Knublauch
     */
    public interface ISPINInferencesOptimizer
    {

        /**
         * Takes a rule set and either returns the same rule set unchanged or a new
         * one with refactored rules.
         * @param class2Query  the rules to execute
         * @return a new rule set or class2Query unchanged
         */
        Dictionary<IResource, List<CommandWrapper>> optimize(Dictionary<IResource, List<CommandWrapper>> class2Query);
    }
}