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
using System.Reflection;
using System.Threading;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// An abstract dataset wrapper that can be used to wrap another dataset and just modify some functionality i.e. provides a decorator over an existing dataset
    /// </summary>
    public abstract class WrapperDataset
        : ISparqlDataset, IConfigurationSerializable, IThreadSafeDataset
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Underlying Dataset
        /// </summary>
        protected ISparqlDataset _dataset;

        /// <summary>
        /// Creates a new wrapped dataset
        /// </summary>
        /// <param name="dataset">Dataset</param>
        public WrapperDataset(ISparqlDataset dataset)
        {
            if (dataset == null) throw new ArgumentNullException("dataset");
            _dataset = dataset;
        }

        /// <summary>
        /// Gets the Lock used to ensure MRSW concurrency on the dataset when available
        /// </summary>
        public ReaderWriterLockSlim Lock
        {
            get
            {
                if (_dataset is IThreadSafeDataset)
                {
                    return ((IThreadSafeDataset)_dataset).Lock;
                }
                else
                {
                    return _lock;
                }
            }
        }

        /// <summary>
        /// Gets the underlying dataset
        /// </summary>
        public ISparqlDataset UnderlyingDataset
        {
            get
            {
                return _dataset;
            }
        }

        #region ISparqlDataset Members

        /// <summary>
        /// Sets the Active Graph for the dataset
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        public virtual void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            _dataset.SetActiveGraph(graphUris);
        }

        /// <summary>
        /// Sets the Active Graph for the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public virtual void SetActiveGraph(Uri graphUri)
        {
            _dataset.SetActiveGraph(graphUri);
        }

        /// <summary>
        /// Sets the Default Graph for the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public virtual void SetDefaultGraph(Uri graphUri)
        {
            _dataset.SetDefaultGraph(graphUri);
        }

        /// <summary>
        /// Sets the Default Graph for the dataset
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        public virtual void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            _dataset.SetDefaultGraph(graphUris);
        }

        /// <summary>
        /// Resets the Active Graph
        /// </summary>
        public virtual void ResetActiveGraph()
        {
            _dataset.ResetActiveGraph();
        }

        /// <summary>
        /// Resets the Default Graph
        /// </summary>
        public virtual void ResetDefaultGraph()
        {
            _dataset.ResetDefaultGraph();
        }

        /// <summary>
        /// Gets the Default Graph URIs
        /// </summary>
        public virtual IEnumerable<Uri> DefaultGraphUris
        {
            get
            {
                return _dataset.DefaultGraphUris;
            }
        }

        /// <summary>
        /// Gets the Active Graph URIs
        /// </summary>
        public virtual IEnumerable<Uri> ActiveGraphUris
        {
            get
            {
                return _dataset.ActiveGraphUris;
            }
        }

        /// <summary>
        /// Gets whether the default graph is the union of all graphs
        /// </summary>
        public virtual bool UsesUnionDefaultGraph
        {
            get
            {
                return _dataset.UsesUnionDefaultGraph;
            }
        }

        /// <summary>
        /// Adds a Graph to the dataset
        /// </summary>
        /// <param name="g">Graph</param>
        public virtual bool AddGraph(IGraph g)
        {
            return _dataset.AddGraph(g);
        }

        /// <summary>
        /// Removes a Graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public virtual bool RemoveGraph(Uri graphUri)
        {
            return _dataset.RemoveGraph(graphUri);
        }

        /// <summary>
        /// Gets whether the dataset contains a given Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public virtual bool HasGraph(Uri graphUri)
        {
            return _dataset.HasGraph(graphUri);
        }

        /// <summary>
        /// Gets the Graphs in the dataset
        /// </summary>
        public virtual IEnumerable<IGraph> Graphs
        {
            get 
            {
                return _dataset.Graphs;
            }
        }

        /// <summary>
        /// Gets the URIs of Graphs in the dataset
        /// </summary>
        public virtual IEnumerable<Uri> GraphUris
        {
            get 
            {
                return _dataset.GraphUris;
            }
        }

        /// <summary>
        /// Gets a Graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public virtual IGraph this[Uri graphUri]
        {
            get
            {
                return _dataset[graphUri];
            }
        }

        /// <summary>
        /// Gets a modifiable graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public virtual IGraph GetModifiableGraph(Uri graphUri)
        {
            return _dataset.GetModifiableGraph(graphUri);
        }

        /// <summary>
        /// Gets whether the dataset has any triples
        /// </summary>
        public virtual bool HasTriples
        {
            get 
            {
                return _dataset.HasTriples; 
            }
        }

        /// <summary>
        /// Gets whether the dataset contains a given triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public virtual bool ContainsTriple(Triple t)
        {
            return _dataset.ContainsTriple(t);
        }

        /// <summary>
        /// Gets all triples from the dataset
        /// </summary>
        public virtual IEnumerable<Triple> Triples
        {
            get
            {
                return _dataset.Triples;
            }
        }

        /// <summary>
        /// Gets triples with a given subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return _dataset.GetTriplesWithSubject(subj);
        }

        /// <summary>
        /// Gets triples with a given predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return _dataset.GetTriplesWithPredicate(pred);
        }

        /// <summary>
        /// Gets triples with a given object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return _dataset.GetTriplesWithObject(obj);
        }

        /// <summary>
        /// Gets triples with a given subject and predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return _dataset.GetTriplesWithSubjectPredicate(subj, pred);
        }

        /// <summary>
        /// Gets triples with a given subject and object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return _dataset.GetTriplesWithSubjectObject(subj, obj);
        }

        /// <summary>
        /// Gets triples with a given predicate and object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public virtual IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return _dataset.GetTriplesWithPredicateObject(pred, obj);
        }

        /// <summary>
        /// Flushes any changes to the dataset
        /// </summary>
        public virtual void Flush()
        {
            _dataset.Flush();
        }

        /// <summary>
        /// Discards any changes to the dataset
        /// </summary>
        public virtual void Discard()
        {
            _dataset.Discard();
        }

        #endregion

        /// <summary>
        /// Serializes the Configuration of the Dataset
        /// </summary>
        /// <param name="context">Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            if (_dataset is IConfigurationSerializable)
            {
                INode dataset = context.NextSubject;
                INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
                INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
                INode datasetClass = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassSparqlDataset));
                INode usingDataset = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingDataset));
                INode innerDataset = context.Graph.CreateBlankNode();

#if NETCORE
                String assm = typeof(WrapperDataset).GetTypeInfo().Assembly.FullName;
#else
                String assm = Assembly.GetAssembly(GetType()).FullName;
#endif
                if (assm.Contains(",")) assm = assm.Substring(0, assm.IndexOf(','));
                String effectiveType = GetType().FullName + (assm.Equals("dotNetRDF") ? String.Empty : ", " + assm);

                context.Graph.Assert(dataset, rdfType, datasetClass);
                context.Graph.Assert(dataset, dnrType, context.Graph.CreateLiteralNode(effectiveType));
                context.Graph.Assert(dataset, usingDataset, innerDataset);
                context.NextSubject = innerDataset;

                ((IConfigurationSerializable)_dataset).SerializeConfiguration(context);
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to serialize configuration as the inner dataset is now serializable");
            }
        }
    }
}
