/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Management.Provisioning.Sesame
{
    /// <summary>
    /// Abstract base class for templates for creating new stores in Sesame
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sesame templates generate a configuration graph like the one <a href="http://www.openrdf.org/doc/sesame2/users/ch07.html#section-repository-config">mentioned</a> in the Sesame documentation, this graph is POSTed to the SYSTEM repository causing a new store to be created.
    /// </para>
    /// </remarks>
    public abstract class BaseSesameTemplate
        : StoreTemplate
    {
        /// <summary>
        /// Constants for Sesame repository configuration namespaces
        /// </summary>
        public const String RepositoryNamespace = "http://www.openrdf.org/config/repository#",
                            RepositorySailNamespace = "http://www.openrdf.org/config/repository/sail#",
                            RepositoryHttpNamespace = "http://www.openrdf.org/config/repository/http#",
                            SailNamespace = "http://www.openrdf.org/config/sail#",
                            SailMemoryNamespace = "http://www.openrdf.org/config/sail/memory#",
                            SailNativeNamespace = "http://www.openrdf.org/config/sail/native#";

        /// <summary>
        /// Creates a new Sesame template
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="name">Template name</param>
        /// <param name="description">Template description</param>
        public BaseSesameTemplate(String id, String name, String description)
            : base(id, name, description) { }

        /// <summary>
        /// Gets/Sets the descriptive label for a Sesame store
        /// </summary>
        [Category("Sesame Configuration"), Description("A descriptive label for the store that Sesame will store and present when browsing the server through the Sesame workbench UI")]
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
        protected virtual IGraph GetBaseTemplateGraph()
        {
            IGraph g = new Graph();

            //Add relevant namespaces
            g.NamespaceMap.AddNamespace("rep", UriFactory.Create(RepositoryNamespace));
            g.NamespaceMap.AddNamespace("sr", UriFactory.Create(RepositorySailNamespace));
            g.NamespaceMap.AddNamespace("hr", UriFactory.Create(RepositoryHttpNamespace));
            g.NamespaceMap.AddNamespace("sail", UriFactory.Create(SailNamespace));
            g.NamespaceMap.AddNamespace("ms", UriFactory.Create(SailMemoryNamespace));
            g.NamespaceMap.AddNamespace("ns", UriFactory.Create(SailNativeNamespace));

            //Assert basic triples
            this.ContextNode = g.CreateBlankNode();
            g.Assert(this.ContextNode, g.CreateUriNode("rdf:type"), g.CreateUriNode("rep:Repository"));
            g.Assert(this.ContextNode, g.CreateUriNode("rep:repositoryID"), g.CreateLiteralNode(this.ID));
            g.Assert(this.ContextNode, g.CreateUriNode("rdfs:label"), g.CreateLiteralNode(this.Label.ToSafeString()));
            return g;
        }

        /// <summary>
        /// Gets the Node used to refer to the store configuration context
        /// </summary>
        [Browsable(false)]
        public INode ContextNode
        {
            get;
            private set;
        }
    }
}
