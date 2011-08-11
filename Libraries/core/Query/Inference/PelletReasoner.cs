/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Ontology;
using VDS.RDF.Query.Inference.Pellet;
using VDS.RDF.Query.Inference.Pellet.Services;

namespace VDS.RDF.Query.Inference
{
    /// <summary>
    /// A Pellet Reasoner which provides OWL 2 capable reasoning using an external knowledge base from a Pellet Server instance
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> Currently this reasoner operates only on a external knowledge base and there is currently no way to introduce new knowledge bases/data through the dotNetRDF API
    /// </para>
    /// </remarks>
    public class PelletReasoner : IQueryableOwlReasoner
    {
        private PelletServer _server;
        private KnowledgeBase _kb;

        /// <summary>
        /// Creates a new Pellet Reasoner
        /// </summary>
        /// <param name="server">Pellet Server</param>
        /// <param name="kbName">Knowledge Base name</param>
        public PelletReasoner(PelletServer server, String kbName)
        {
            this._server = server;
            if (this._server.HasKnowledgeBase(kbName))
            {
                this._kb = this._server.GetKnowledgeBase(kbName);
            }
            else
            {
                throw new RdfReasoningException("Cannot create a Pellet Reasoner for the Knowledge Base named '" + kbName + "' as this Server does not have the named Knowledge Base");
            }
        }

        /// <summary>
        /// Creates a new Pellet Reasoner
        /// </summary>
        /// <param name="serverUri">Pellet Server URI</param>
        /// <param name="kbName">Knowledge Base name</param>
        public PelletReasoner(Uri serverUri, String kbName)
            : this(new PelletServer(serverUri), kbName) { }

        /// <summary>
        /// Gets the Knowledge Base this Reasoner operates over
        /// </summary>
        public KnowledgeBase KnowledgeBase
        {
            get
            {
                return this._kb;
            }
        }

        /// <summary>
        /// Gets the Pellet Server this Reasoner operates on
        /// </summary>
        public PelletServer Server
        {
            get
            {
                return this._server;
            }
        }

        /// <summary>
        /// Executes a SPARQL Query against the underlying Knowledge Base
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public object ExecuteQuery(string sparqlQuery)
        {
            Type svcType = typeof(QueryService);
            if (this._kb.SupportsService(svcType))
            {
                QueryService svc = (QueryService)this._kb.GetService(svcType);
                return svc.Query(sparqlQuery);
            }
            else
            {
                throw new NotSupportedException("The Knowledge Base does not support the SPARQL Query service");
            }
        }

        /// <summary>
        /// Adds a Graph to the Knowledge Base
        /// </summary>
        /// <param name="g">Graph</param>
        /// <remarks>
        /// Currently not supported by Pellet Server
        /// </remarks>
        public void Add(IGraph g)
        {
            throw new NotSupportedException("Pellet Server does not currently support the addition of data to a Knowledge Base");
        }

        /// <summary>
        /// Extract a reasoning enhanced sub-graph from the given Graph rooted at the given Node
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="n">Root Node</param>
        /// <returns></returns>
        /// <remarks>
        /// Currently not supported by Pellet Server
        /// </remarks>
        public IGraph Extract(IGraph g, INode n)
        {
            throw new NotSupportedException("Pellet Server does not currently support the extraction of data from a Knowledge Base");
        }

        /// <summary>
        /// Extracts all possible triples using the given extraction mode
        /// </summary>
        /// <param name="mode">Extraction Mode</param>
        /// <returns></returns>
        /// <remarks>
        /// Currently not supported by Pellet Server
        /// </remarks>
        public IEnumerable<Triple> Extract(string mode)
        {
            throw new NotSupportedException("Pellet Server does not currently support the extraction of data from a Knowledge Base");
        }

        /// <summary>
        /// Extracts all possible triples using the given extraction modes
        /// </summary>
        /// <param name="modes">Extraction Modes</param>
        /// <returns></returns>
        /// <remarks>
        /// Currently not supported by Pellet Server
        /// </remarks>
        public IEnumerable<Triple> Extract(IEnumerable<string> modes)
        {
            throw new NotSupportedException("Pellet Server does not currently support the extraction of data from a Knowledge Base");
        }

        /// <summary>
        /// Extracts the triples which comprise the class hierarchy
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Triple> Classify()
        {
            Type svcType = typeof(ClassifyService);
            if (this._kb.SupportsService(svcType))
            {
                ClassifyService svc = (ClassifyService)this._kb.GetService(svcType);
                return svc.Classify().Triples;
            }
            else
            {
                throw new NotSupportedException("The Knowledge Base does not support the Classify service");
            }
        }

        /// <summary>
        /// Extracts the triples which comprise the class hierarchy and individuals of those classes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Triple> Realize()
        {
            Type svcType = typeof(RealizeService);
            if (this._kb.SupportsService(svcType))
            {
                RealizeService svc = (RealizeService)this._kb.GetService(svcType);
                return svc.Realize().Triples;
            }
            else
            {
                throw new NotSupportedException("The Knowledge Base does not support the Realize service");
            }
        }

        /// <summary>
        /// Returns whether the underlying knowledge base is consistent
        /// </summary>
        /// <returns></returns>
        public bool IsConsistent()
        {
            Type svcType = typeof(ConsistencyService);
            if (this._kb.SupportsService(svcType))
            {
                ConsistencyService svc = (ConsistencyService)this._kb.GetService(svcType);
                return svc.IsConsistent();
            }
            else
            {
                throw new NotSupportedException("The Knowledge Base does not support the Consistency service");
            }
        }

        /// <summary>
        /// Returns whether the given Graph is consistent with the underlying knowledge base
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        /// <remarks>
        /// Currently not supported by Pellet Server
        /// </remarks>
        public bool IsConsistent(IGraph g)
        {
            throw new NotSupportedException("Pellet Server does not currently support checking whether additional data is consistent - Use IsConsistent() to determine if the Knowledge Base itself is consistent");
        }

        /// <summary>
        /// Returns the enumeration of unsatisfiable classes
        /// </summary>
        /// <remarks>
        /// Currently not supported by Pellet Server
        /// </remarks>
        public IEnumerable<OntologyResource> Unsatisfiable
        {
            get 
            {
                throw new NotSupportedException("Pellet Server does not currently support the enumeration of Unsatisfiable Classes from a Knowledge Base");
            }
        }

    }
}

#endif