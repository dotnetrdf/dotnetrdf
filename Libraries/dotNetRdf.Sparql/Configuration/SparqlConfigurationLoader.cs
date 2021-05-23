using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Operators;

namespace VDS.RDF.Configuration
{
    public static class SparqlConfigurationLoader
    {
        public const string PropertyEnabled = ConfigurationLoader.ConfigurationNamespace + "enabled";
        public const string ClassSparqlOperator = ConfigurationLoader.ConfigurationNamespace + "SparqlOperator";

        /// <summary>
        /// Given a Configuration Graph will detect and configure SPARQL Operators.
        /// </summary>
        /// <param name="g">Configuration Graph.</param>
        public static void AutoConfigureSparqlOperators(IGraph g)
        {
            INode rdfType = g.CreateUriNode(g.UriFactory.Create(RdfSpecsHelper.RdfType)),
                operatorClass = g.CreateUriNode(g.UriFactory.Create(ClassSparqlOperator)),
                enabled = g.CreateUriNode(g.UriFactory.Create(PropertyEnabled));

            foreach (Triple t in g.GetTriplesWithPredicateObject(rdfType, operatorClass))
            {
                var temp = ConfigurationLoader.LoadObject(g, t.Subject);
                if (temp is ISparqlOperator)
                {
                    var enable = ConfigurationLoader.GetConfigurationBoolean(g, t.Subject, enabled, true);
                    if (enable)
                    {
                        SparqlOperators.AddOperator((ISparqlOperator)temp);
                    }
                    else
                    {
                        SparqlOperators.RemoveOperatorByType((ISparqlOperator)temp);
                    }
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Auto-configuration of SPARQL Operators failed as the Operator specified by the Node '" + t.Subject.ToString() + "' does not implement the required ISparqlOperator interface");
                }
            }
        }

        /// <summary>
        /// Given a Configuration Graph applies all available SPARQL auto-configuration based on the contents of the graph.
        /// </summary>
        /// <param name="g">Configuration Graph.</param>
        public static void AutoConfigure(IGraph g)
        {
            AutoConfigureSparqlOperators(g);
        }
    }
}
