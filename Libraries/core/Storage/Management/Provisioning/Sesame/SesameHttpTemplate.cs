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
