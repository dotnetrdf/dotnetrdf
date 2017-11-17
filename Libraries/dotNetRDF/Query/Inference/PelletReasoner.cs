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
using System.Collections.Generic;
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
            _server = server;
            if (_server.HasKnowledgeBase(kbName))
            {
                _kb = _server.GetKnowledgeBase(kbName);
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
                return _kb;
            }
        }

        /// <summary>
        /// Gets the Pellet Server this Reasoner operates on
        /// </summary>
        public PelletServer Server
        {
            get
            {
                return _server;
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
            if (_kb.SupportsService(svcType))
            {
                QueryService svc = (QueryService)_kb.GetService(svcType);
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
            if (_kb.SupportsService(svcType))
            {
                ClassifyService svc = (ClassifyService)_kb.GetService(svcType);
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
            if (_kb.SupportsService(svcType))
            {
                RealizeService svc = (RealizeService)_kb.GetService(svcType);
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
            if (_kb.SupportsService(svcType))
            {
                ConsistencyService svc = (ConsistencyService)_kb.GetService(svcType);
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
