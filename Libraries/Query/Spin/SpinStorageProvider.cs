/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlStrategies;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin
{
    /// <summary>
    /// This class is responsible for
    ///     => configuring the chain of SPARQL strategy handlers that are necessary to interpret the client's commands
    ///     => creating the Connection objects to a specific store through the GetConnection method.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class SpinStorageProvider
        : ISparqlSDPlugin
    {
        private static readonly IUriNode PropertySparqlStrategy = RDFHelper.CreateUriNode(UriFactory.Create(ConfigurationLoader.ConfigurationNamespace + "sparqlStrategy"));

        private IGraph _configurationGraph;
        private INode _thisConfigurationNode;
        private INode _storageNode;
        private IGraph _sdFeaturesGraph = null;

        private List<ISparqlHandlingStrategy> _sparqlHandlers;

        internal SpinStorageProvider(IGraph g, INode thisNode, INode storageNode)
        {
            _configurationGraph = g;
            _thisConfigurationNode = thisNode;
            _storageNode = storageNode;

            INode strategyNode = thisNode;
            // Find and create the sparqlHandlers required for the underlying StorageProvider
            while (strategyNode != null)
            {
                if (strategyNode.IsListRoot(g))
                {
                    _sparqlHandlers = g.GetListItems(strategyNode)
                        .Select(n => (ISparqlHandlingStrategy)ConfigurationLoader.LoadObject(_configurationGraph, RDFHelper.CreateBlankNode(), Type.GetType(((ILiteralNode)n).Value))).ToList();
                    break;
                }
                strategyNode = g.GetTriplesWithSubjectPredicate(strategyNode, PropertySparqlStrategy).Select(t => t.Object).FirstOrDefault();
            }
            // Get the SPIN imports for this storage and create the model
            IEnumerable<Uri> spinImports = g.GetTriplesWithSubjectPredicate(_thisConfigurationNode, SPIN.PropertyImports).Select(t => ((IUriNode)t.Object).Uri);
            SpinModel.Register(this, (IQueryableStorage)ConfigurationLoader.LoadObject(_configurationGraph, _storageNode), spinImports);
        }


        internal void Handle(SparqlCommandUnit command)
        {
            foreach (ISparqlHandlingStrategy handler in _sparqlHandlers)
            {
                handler.Handle(command);
            }
        }

        /// <summary>
        /// Returns a new Connection instance to the Storage
        /// </summary>
        /// <returns></returns>
        public Connection GetConnection()
        {
            IStorageProvider storage = (IQueryableStorage)ConfigurationLoader.LoadObject(_configurationGraph, _storageNode);
            if (!(storage is IUpdateableStorage))
            {
                throw new DotNetRdfConfigurationException("A SpinStorageProvider underlying storage must support SPARQL 1.1 Updates.");
            }
            return new Connection(this, (IUpdateableStorage)storage);
        }

        #region ISparqlSDPlugin members

        public INode Resource
        {
            get
            {
                return _thisConfigurationNode;
            }
        }

        public IGraph SparqlSDContribution
        {
            get
            {
                if (_sdFeaturesGraph != null) return _sdFeaturesGraph;
                _sdFeaturesGraph = new Graph();
                foreach (ISparqlHandlingStrategy handler in _sparqlHandlers)
                {
                    _sdFeaturesGraph.Assert(_thisConfigurationNode, SD.PropertyHasFeature, handler.Resource);
                    _sdFeaturesGraph.Merge(handler.SparqlSDContribution);
                }
                return _sdFeaturesGraph;
            }
        }

        #endregion ISparqlSDPlugin members
    }
}