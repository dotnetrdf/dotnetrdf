using org.topbraid.spin.model.update;
using System.Collections.Generic;
using System;
using VDS.RDF;
namespace org.topbraid.spin.util
{


    /**
     * Utility on SPARQL Update operations.
     * 
     * @author Holger Knublauch
     */
    public class UpdateUtil
    {

        /**
         * Gets all Graphs that are potentially updated in a given Update request.
         * @param update  the Update (UpdateModify and UpdateDeleteWhere are supported)
         * @param dataset  the Dataset to get the Graphs from
         * @return the Graphs
         */
        public static IEnumerable<Graph> getUpdatedGraphs(Update update, Dataset dataset, Dictionary<String, INode> templateBindings)
        {
            HashSet<Graph> results = new HashSet<Graph>();
            if (update is UpdateModify)
            {
                addUpdatedGraphs(results, (UpdateModify)update, dataset, templateBindings);
            }
            else if (update is UpdateDeleteWhere)
            {
                addUpdatedGraphs(results, (UpdateDeleteWhere)update, dataset, templateBindings);
            }
            return results;
        }


        private static void addUpdatedGraphs(IEnumerable<Graph> results, UpdateDeleteWhere update, Dataset dataset, Dictionary<String, INode> templateBindings)
        {
            addUpdatedGraphs(results, update.getQuads(), dataset, templateBindings);
        }


        private static void addUpdatedGraphs(HashSet<Graph> results, UpdateModify update, Dataset dataset, Dictionary<String, INode> templateBindings)
        {
            INode withIRI = update.getWithIRI();
            if (withIRI != null)
            {
                results.Add(dataset.getNamedModel(withIRI.Uri).getGraph());
            }
            addUpdatedGraphs(results, update.getDeleteQuads(), dataset, templateBindings);
            addUpdatedGraphs(results, update.getInsertQuads(), dataset, templateBindings);
        }


        private static void addUpdatedGraphs(HashSet<Graph> results, HashSet<Quad> quads, Dataset graphStore, Dictionary<String, INode> templateBindings)
        {
		foreach(Quad quad in quads) {
			if(quad.isDefaultGraph()) {
				results.Add(graphStore.getDefaultModel().getGraph());
			}
			else if(quad.getGraph().isVariable()) {
				if(templateBindings != null) {
					String varName = quad.getGraph().getName();
					INode binding = templateBindings.get(varName);
					if(binding != null && binding is IUriNode) {
						results.Add(graphStore.getNamedModel(binding.Uri).getGraph());
					}
				}
			}
			else {
				results.Add(graphStore.getNamedModel(quad.getGraph().Uri).getGraph());
			}
		}
	}
    }
}