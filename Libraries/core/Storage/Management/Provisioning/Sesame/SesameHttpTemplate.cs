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
    /// Templates for creating remote Sesame stores
    /// </summary>
    /// <remarks>
    /// <para>
    /// This template generates a Sesame repository config graph like the following, depending on exact options the graph may differ:
    /// </para>
    /// <pre>
    /// @prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
    /// @prefix rep: <http://www.openrdf.org/config/repository#>.
    /// @prefix hr: <http://www.openrdf.org/config/repository/http#>.
    /// 
    /// [] a rep:Repository ;
    /// rep:repositoryImpl [
    ///       rep:repositoryType "openrdf:HTTPRepository" ;
    ///       hr:repositoryURL <{%Sesame server location|http://localhost:8080/openrdf-sesame%}/repositories/{%Remote repository ID|SYSTEM%}>
    ///    ];
    ///    rep:repositoryID "{this.ID}" ;
    ///    rdfs:label "{this.Label}" .
    /// </pre>
    /// <para>
    /// The placeholders of the form <strong>{this.Property}</strong> represent properties of this class whose values will be inserted into the repository config graph and used to create a new store in Sesame.
    /// </para>
    /// </remarks>
    public class SesameHttpTemplate
        : BaseSesameTemplate
    {
        public SesameHttpTemplate(String id)
            : base(id, "Remote Sesame Store", "A remote Sesame store is any Sesame store not residing on this Sesame server accessible via the Sesame HTTP protocol") { }

        /// <summary>
        /// Gets/Sets the remote Sesame server to connect to
        /// </summary>
        [Category("Sesame Configuration"), Description("The Base URL of the remote Sesame server the remote store resides upon")]
        public String RemoteServer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the ID of the remote repository to connect to
        /// </summary>
        [Category("Sesame Configuration"), Description("The ID of the remote repository")]
        public String RemoteRepositoryID
        {
            get;
            set;
        }

        public override IGraph GetTemplateGraph()
        {
            IGraph g = this.GetBaseTemplateGraph();
            INode impl = g.CreateBlankNode();
            g.Assert(this.ContextNode, g.CreateUriNode("rep:repositoryImpl"), impl);
            g.Assert(impl, g.CreateUriNode("rep:repositoryType"), g.CreateLiteralNode("openrdf:HTTPRepository"));

            String url = this.RemoteServer;
            if (!url.EndsWith("/")) url += "/";
            url += "repositories/";
            url += this.RemoteRepositoryID;

            g.Assert(impl, g.CreateUriNode("hr:repositoryURL"), g.CreateUriNode(UriFactory.Create(url)));
            return g;
        }
    }
}
