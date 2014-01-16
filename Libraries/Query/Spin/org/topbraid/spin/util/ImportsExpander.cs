using System;
using VDS.RDF;
namespace org.topbraid.spin.util
{
    /**
     * An interface that can be implemented by a Dataset to support the
     * SWP function ui:graphWithImports.
     * 
     * @author Holger Knublauch
     */
    public interface ImportsExpander
    {

        /**
         * Starting with a given base Graph (and its URI), this method creates a new
         * Graph that also includes the owl:imports of the base Graph.  Typically
         * this will return a Jena MultiUnion.
         * @param baseURI  the base URI of the base Graph
         * @param baseGraph  the base Graph
         * @return the Graph with imports
         */
        Graph expandImports(String baseURI, Graph baseGraph);
    }
}