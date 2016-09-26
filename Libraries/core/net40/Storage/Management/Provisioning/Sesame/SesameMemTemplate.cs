/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
#if !WINDOWS_PHONE && !NETCORE && !NETCORE
using System.ComponentModel;
#if SILVERLIGHT
using System.ComponentModel.DataAnnotations;
#endif
#endif
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Management.Provisioning.Sesame
{
    /// <summary>
    /// Template for creating Sesame memory stores
    /// </summary>
    /// <remarks>
    /// <para>
    /// This template generates a Sesame repository config graph like the following, depending on exact options the graph may differ:
    /// </para>
    /// <code>
    /// @prefix rdfs: &lt;http://www.w3.org/2000/01/rdf-schema#&gt;.
    /// @prefix rep: &lt;http://www.openrdf.org/config/repository#&gt;.
    /// @prefix sr: &lt;http://www.openrdf.org/config/repository/sail#&gt;.
    /// @prefix sail: &lt;http://www.openrdf.org/config/sail#&gt;.
    /// @prefix ms: &lt;http://www.openrdf.org/config/sail/memory#&gt;.
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
    /// </code>
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
            : base(id, "Sesame Memory", "A Sesame memory store is stored fully in-memory and may be persisted to/from disk") 
        {
            this.Persist = true;
            this.SyncDelay = 0;
        }

        /// <summary>
        /// Gets the template graph used to create the store
        /// </summary>
        /// <returns></returns>
        public override IGraph GetTemplateGraph()
        {
            IGraph g = this.GetBaseTemplateGraph();
            INode impl = g.CreateBlankNode();
            g.Assert(this.ContextNode, g.CreateUriNode("rep:repositoryImpl"), impl);
            g.Assert(impl, g.CreateUriNode("rep:repositoryType"), g.CreateLiteralNode("openrdf:SailRepository"));
            INode sailImpl = g.CreateBlankNode();
            g.Assert(impl, g.CreateUriNode("sr:sailImpl"), sailImpl);

            if (this.DirectTypeHierarchyInferencing)
            {
                INode sailDelegate = g.CreateBlankNode();
                g.Assert(sailImpl, g.CreateUriNode("sail:sailType"), g.CreateLiteralNode("openrdf:DirectTypeHierarchyInferencer"));
                g.Assert(sailImpl, g.CreateUriNode("sail:delegate"), sailDelegate);
                sailImpl = sailDelegate;
            }
            if (this.RdfSchemaInferencing)
            {
                INode sailDelegate = g.CreateBlankNode();
                g.Assert(sailImpl, g.CreateUriNode("sail:sailType"), g.CreateLiteralNode("openrdf:ForwardChainingRDFSInferencer"));
                g.Assert(sailImpl, g.CreateUriNode("sail:delegate"), sailDelegate);
                sailImpl = sailDelegate;
            }
            g.Assert(sailImpl, g.CreateUriNode("sail:sailType"), g.CreateLiteralNode("openrdf:MemoryStore"));
            g.Assert(sailImpl, g.CreateUriNode("ms:persist"), this.Persist.ToLiteral(g));
            g.Assert(sailImpl, g.CreateUriNode("ms:syncDelay"), this.SyncDelay.ToLiteral(g));

            return g;
        }

        /// <summary>
        /// Gets/Sets whether to persist the store
        /// </summary>
#if !WINDOWS_PHONE && !NETCORE
        [Category("Sesame Configuration"), Description("Whether the store is persisted"), DefaultValue(true)]
#endif
        public bool Persist
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the sync delay
        /// </summary>
#if !WINDOWS_PHONE && !NETCORE
#if !SILVERLIGHT
        [Category("Sesame Configuration"), DisplayName("Sync Delay"), Description("Sets the sync delay for the store"), DefaultValue(0)]
#else
        [Category("Sesame Configuration"), Display(Name = "Sync Delay"), Description("Sets the sync delay for the store"), DefaultValue(0)]
#endif
#endif
        public int SyncDelay
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to enable direct type hierarchy inferencing
        /// </summary>
#if !WINDOWS_PHONE && !NETCORE
#if !SILVERLIGHT
        [Category("Sesame Reasoning"), DisplayName("Direct Type Hierarchy Inference"), Description("Enables/Disables Direct Type Hierarchy Inference"), DefaultValue(false)]
#else
        [Category("Sesame Reasoning"), Display(Name = "Direct Type Hierarchy Inference"), Description("Enables/Disables Direct Type Hierarchy Inference"), DefaultValue(false)]
#endif
#endif
        public bool DirectTypeHierarchyInferencing
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to enable RDF Schema Inferencing
        /// </summary>
#if !WINDOWS_PHONE && !NETCORE
#if !SILVERLIGHT
        [Category("Sesame Reasoning"), DisplayName("RDF Schema Inference"), Description("Enables/Disables RDF Schema inferencing"), DefaultValue(false)]
#else
        [Category("Sesame Reasoning"), Display(Name = "RDF Schema Inference"), Description("Enables/Disables RDF Schema inferencing"), DefaultValue(false)]
#endif
#endif
        public bool RdfSchemaInferencing
        {
            get;
            set;
        }
    }
}
