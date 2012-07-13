using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Management.Provisioning.Sesame
{
    /// <summary>
    /// Sesame Native index modes
    /// </summary>
    public enum SesameNativeIndexMode
    {
        SPOC,
        POSC
    }

    /// <summary>
    /// Template for creating Sesame Native stores
    /// </summary>
    /// <remarks>
    /// <pre>
    /// @prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
    /// @prefix rep: <http://www.openrdf.org/config/repository#>.
    /// @prefix sr: <http://www.openrdf.org/config/repository/sail#>.
    /// @prefix sail: <http://www.openrdf.org/config/sail#>.
    /// @prefix ns: <http://www.openrdf.org/config/sail/native#>.
    /// 
    /// [] a rep:Repository ;
    ///    rep:repositoryID "{this.ID}" ;
    ///    rdfs:label "{this.Label}" ;
    ///    rep:repositoryImpl [
    ///       rep:repositoryType "openrdf:SailRepository" ;
    ///       sr:sailImpl [
    ///          sail:sailType "openrdf:NativeStore" ;
    ///          ns:tripleIndexes "{this.IndexMode}"
    ///       ]
    ///    ].
    /// </pre>
    /// </remarks>
    class SesameNativeTemplate
        : BaseSesameTemplate
    {
        public SesameNativeTemplate(String id)
            : base(id, "Sesame Native", "A Sesame native store resides on disk")
        {
            this.IndexMode = SesameNativeIndexMode.SPOC;
        }

        public override IGraph GetTemplateGraph()
        {
            IGraph g = this.GetBaseTemplateGraph();
            INode impl = g.CreateBlankNode();
            g.Assert(this.ContextNode, g.CreateUriNode("rep:repositoryImpl"), impl);
            g.Assert(impl, g.CreateUriNode("rep:repositoryType"), g.CreateLiteralNode("openrdf:SailRepository"));
            INode sailImpl = g.CreateBlankNode();
            g.Assert(impl, g.CreateUriNode("sr:sailImpl"), sailImpl);
            g.Assert(sailImpl, g.CreateUriNode("sail:sailType"), g.CreateLiteralNode("openrdf:NativeStore"));

            String mode = this.IndexMode.ToString().ToLower();
            if (mode.Contains(".")) mode = mode.Substring(mode.LastIndexOf('.') + 1);
            g.Assert(sailImpl, g.CreateUriNode("ns:tripleIndexes"), g.CreateLiteralNode(mode));
            return g;
        }

        /// <summary>
        /// Gets/Sets the Indexing Mode
        /// </summary>
        [Category("Sesame Configuration"), Description("Sets the indexing mode for the store"), DefaultValue(SesameNativeIndexMode.SPOC)]
        public SesameNativeIndexMode IndexMode
        {
            get;
            set;
        }
    }
}
