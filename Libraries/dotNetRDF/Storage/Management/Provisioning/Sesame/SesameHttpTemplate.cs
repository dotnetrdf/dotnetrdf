/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.ComponentModel;

namespace VDS.RDF.Storage.Management.Provisioning.Sesame
{
    /// <summary>
    /// Templates for creating remote Sesame stores
    /// </summary>
    /// <remarks>
    /// <para>
    /// This template generates a Sesame repository config graph like the following, depending on exact options the graph may differ:
    /// </para>
    /// <code>
    /// @prefix rdfs: &lt;http://www.w3.org/2000/01/rdf-schema#&gt;.
    /// @prefix rep: &lt;http://www.openrdf.org/config/repository#&gt;.
    /// @prefix hr: &lt;http://www.openrdf.org/config/repository/http#&gt;.
    /// 
    /// [] a rep:Repository ;
    /// rep:repositoryImpl [
    ///       rep:repositoryType "openrdf:HTTPRepository" ;
    ///       hr:repositoryURL &lt;{%Sesame server location|http://localhost:8080/openrdf-sesame%}/repositories/{%Remote repository ID|SYSTEM%}&gt;
    ///    ];
    ///    rep:repositoryID "{this.ID}" ;
    ///    rdfs:label "{this.Label}" .
    /// </code>
    /// <para>
    /// The placeholders of the form <strong>{this.Property}</strong> represent properties of this class whose values will be inserted into the repository config graph and used to create a new store in Sesame.
    /// </para>
    /// </remarks>
    public class SesameHttpTemplate
        : BaseSesameTemplate
    {
        /// <summary>
        /// Creates a new Template
        /// </summary>
        /// <param name="id">Store ID</param>
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

        /// <summary>
        /// Gets the template graph
        /// </summary>
        /// <returns></returns>
        public override IGraph GetTemplateGraph()
        {
            IGraph g = GetBaseTemplateGraph();
            INode impl = g.CreateBlankNode();
            g.Assert(ContextNode, g.CreateUriNode("rep:repositoryImpl"), impl);
            g.Assert(impl, g.CreateUriNode("rep:repositoryType"), g.CreateLiteralNode("openrdf:HTTPRepository"));

            String url = RemoteServer;
            if (!url.EndsWith("/")) url += "/";
            url += "repositories/";
            url += RemoteRepositoryID;

            g.Assert(impl, g.CreateUriNode("hr:repositoryURL"), g.CreateUriNode(UriFactory.Create(url)));
            return g;
        }
    }
}
