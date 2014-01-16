using VDS.RDF;
using VDS.RDF.Query.Spin;
using System.Collections.Generic;
using System;

namespace org.topbraid.spin.util
{

    /**
     * An internal helper object for the algorithm that determines whether a property
     * is considered multi-valued, as exposed by JenaUtil.isMulti().
     * 
     * @author Holger Knublauch
     */
    class IsMultiFunctionHelper
    {

        private static INode integerOne = new NodeFactory().CreateLiteralNode("1", XSD.integer);


        public static bool isMulti(INode property, INode type, IGraph graph)
        {

            // FILTER NOT EXISTS { ?property a owl:FunctionalProperty }
            if (graph.ContainsTriple(property, graph.CreateUriNode(RDF.type), graph.CreateUriNode(OWL.FunctionalProperty)))
            {
                return false;
            }

            if (type != null)
            {
                // Walk up classes, doing restrictions and constraints at once
                HashSet<INode> reached = new HashSet<INode>();
                return walk(property, type, graph, reached);
            }
            else
            {
                return true;
            }
        }


        private static bool hasMaxCardinality(INode restriction, IGraph graph, INode predicate)
        {
		IEnumerator<Triple> it = graph.find(restriction, predicate, INode.ANY);
		if(it.MoveNext()) {
            INode obj = it.Current.Object;
			it.Dispose();
			if(obj is ILiteralNode) {
				String lex = obj.getLiteralLexicalForm();
				if("0".Equals(lex) || "1".Equals(lex)) {
					return true;
				}
			}
		}
		return false;
	}


        private static bool walk(INode property, INode type, Graph graph, HashSet<INode> classes)
        {
		
		classes.Add(type);
		
		{
			IEnumerator<Triple> it = graph.find(type, SPIN.constraint, INode.ANY);
			while(it.MoveNext()) {
                INode constraint = it.Current.Object;
				if(graph.ContainsTriple(constraint, SPL.predicate, property) &&
                        graph.ContainsTriple(constraint, RDF.type, SPL.Argument))
                {
					it.Dispose();
					return false;
				}
                else if (graph.ContainsTriple(constraint, ARG.property, property) &&
                        graph.ContainsTriple(constraint, RDF.type, SPL.ObjectCountPropertyConstraint) &&
                        graph.ContainsTriple(constraint, ARG.maxCount, integerOne))
                {
					it.Dispose();
					return false;
				}
			}
		}

        List<INode> superClasses = new List<INode>();
		{
			IEnumerator<Triple> it = graph.find(type, RDFS.subClassOf, INode.ANY);
			while(it.MoveNext()) {
                INode restriction = it.Current.Object;
				if(restriction is IBlankNode &&
                        graph.ContainsTriple(restriction, OWL.onProperty, property))
                {
					if(hasMaxCardinality(restriction, graph, OWL.maxCardinality) ||
							hasMaxCardinality(restriction, graph, OWL.cardinality)) {
						it.Dispose();
						return false;
					}
				}
				else if(restriction is IUriNode) {
					superClasses.Add(restriction);
				}
			}
		}		
		
		foreach(INode superClass in superClasses) {
			if(!classes.Contains(superClass)) {
				if(!walk(property, superClass, graph, classes)) {
					return false;
				}
			}
		}
		
		return true;
	}
    }
}