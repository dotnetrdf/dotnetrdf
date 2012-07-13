using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Management.Provisioning
{
    /// <summary>
    /// Abstract base class for templates for creating new stores in Sesame
    /// </summary>
    public abstract class BaseSesameTemplate
        : StoreTemplate
    {
        public const String RepositoryNamespace = "http://www.openrdf.org/config/repository#",
                            RepositorySailNamespace = "http://www.openrdf.org/config/repository/sail#",
                            SailNamespace = "http://www.openrdf.org/config/sail#",
                            SailMemoryNamespace = "http://www.openrdf.org/config/sail/memory#";

        public BaseSesameTemplate(String id)
            : base(id) { }

        /// <summary>
        /// Gets/Sets the descriptive label for a Sesame store
        /// </summary>
        [Description("Descriptive label for the store")]
        public String Label
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a Graph representing the RDF that must be inserted into Sesame's SYSTEM repository in order to create the desired store
        /// </summary>
        /// <returns></returns>
        public abstract IGraph GetTemplateGraph();

        /// <summary>
        /// Gets the basic template graph which is a graph with all the required namespaces registered and the ID and label filled in
        /// </summary>
        /// <param name="n">Node that represents the repository for which additional properties may be stated by the calling code</param>
        /// <returns></returns>
        protected virtual IGraph GetBaseTemplateGraph(out INode n)
        {
            Graph g = new Graph();

            //Add relevant namespaces
            g.NamespaceMap.AddNamespace("rep", UriFactory.Create(RepositoryNamespace));
            g.NamespaceMap.AddNamespace("sr", UriFactory.Create(RepositorySailNamespace));
            g.NamespaceMap.AddNamespace("sail", UriFactory.Create(SailNamespace));
            g.NamespaceMap.AddNamespace("ms", UriFactory.Create(SailMemoryNamespace));

            //Assert basic triples
            n = g.CreateBlankNode();
            g.Assert(n, g.CreateUriNode("rdf:type"), g.CreateUriNode("rep:Repository"));
            g.Assert(n, g.CreateUriNode("rep:repositoryID"), g.CreateLiteralNode(this.ID));
            g.Assert(n, g.CreateLiteralNode("rdfs:label"), g.CreateLiteralNode(this.Label));

            return g;
        }
    }
}
