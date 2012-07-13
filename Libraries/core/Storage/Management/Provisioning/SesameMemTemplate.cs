using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Management.Provisioning
{
    /// <summary>
    /// Template for creating Sesame memory stores
    /// </summary>
    /// <remarks>
    /// <para>
    /// This template generates the following Sesame repository config graph:
    /// </para>
    /// <pre>
    /// @prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
    /// @prefix rep: <http://www.openrdf.org/config/repository#>.
    /// @prefix sr: <http://www.openrdf.org/config/repository/sail#>.
    /// @prefix sail: <http://www.openrdf.org/config/sail#>.
    /// @prefix ms: <http://www.openrdf.org/config/sail/memory#>.
    /// 
    /// [] a rep:Repository ;
    /// rep:repositoryID "{this.ID}" ;
    /// rdfs:label "{this.Label}" ;    
    ///    rep:repositoryImpl [
    ///       rep:repositoryType "openrdf:SailRepository" ;
    ///       sr:sailImpl [
    ///          sail:sailType "openrdf:MemoryStore" ;
    ///          ms:persist {this.Persist} ;
    ///          ms:syncDelay {this.SyncDelay}
    ///       ]
    ///   ].
    /// </pre>
    /// <para>
    /// The placeholders of the form <strong>{this.Property}</strong> represent properties of this class whose values will be inserted into the repository config graph and used to create a new store in Sesame.
    /// </para>
    /// </remarks>
    public class SesameMemTemplate
        : BaseSesameTemplate
    {
        /// <summary>
        /// Creates a new memory store template
        /// </summary>
        /// <param name="id">Store ID</param>
        public SesameMemTemplate(String id)
            : base(id) { }

        /// <summary>
        /// Gets the template graph used to create the store
        /// </summary>
        /// <returns></returns>
        public override IGraph GetTemplateGraph()
        {
            INode n;
            IGraph g = this.GetBaseTemplateGraph(out n);
            INode impl = g.CreateBlankNode();
            g.Assert(n, g.CreateUriNode("rep:repositoryImpl"), impl);
            g.Assert(impl, g.CreateUriNode("rep:repositoryType"), g.CreateLiteralNode("openrdf:SailRepository"));
            INode sailImpl = g.CreateBlankNode();
            g.Assert(impl, g.CreateUriNode("sr:sailImpl"), sailImpl);
            g.Assert(sailImpl, g.CreateUriNode("sail:sailType"), g.CreateLiteralNode("openrdf:MemoryStore"));
            g.Assert(sailImpl, g.CreateLiteralNode("ms:persist"), this.Persist.ToLiteral(g));
            g.Assert(sailImpl, g.CreateLiteralNode("ms:syncDelay"), this.SyncDelay.ToLiteral(g));

            return g;
        }

        /// <summary>
        /// Gets/Sets whether to persist the store
        /// </summary>
        [Description("Whether the store is persisted"), DefaultValue(true)]
        public bool Persist
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the sync delay
        /// </summary>
        [DefaultValue(0)]
        public int SyncDelay
        {
            get;
            set;
        }
    }
}
