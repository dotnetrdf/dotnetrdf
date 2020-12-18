/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Inference;
using VDS.RDF.Update;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing Triple Stores which are collections of RDF Graphs.
    /// </summary>
    public class TripleStore
        : BaseTripleStore, IInMemoryQueryableStore, IInferencingTripleStore, IUpdateableTripleStore
    {
        /// <summary>
        /// List of Reasoners that are applied to Graphs as they are added to the Triple Store.
        /// </summary>
        protected List<IInferenceEngine> _reasoners = new List<IInferenceEngine>();
        /// <summary>
        /// Controls whether inferred information is stored in a special Graph or in the original Graph.
        /// </summary>
        protected bool _storeInferencesExternally = false;
        /// <summary>
        /// Graph Uri for the special Graph used to store inferred information.
        /// </summary>
        protected UriNode _inferenceGraphUri = new UriNode(UriFactory.Create("dotNetRDF:inference-graph"));

        /// <summary>
        /// Creates a new Triple Store using a new empty Graph collection.
        /// </summary>
        public TripleStore()
            : this(new GraphCollection()) { }

        /// <summary>
        /// Creates a new Triple Store using the given Graph collection which may be non-empty.
        /// </summary>
        /// <param name="graphCollection">Graph Collection.</param>
        public TripleStore(BaseGraphCollection graphCollection)
            : base(graphCollection) { }

        #region Selection

        /// <summary>
        /// Returns whether the Store contains the given Triple within the Query Triples.
        /// </summary>
        /// <param name="t">Triple to search for.</param>
        /// <returns></returns>
        public bool Contains(Triple t)
        {
            return _graphs.Any(g => g.ContainsTriple(t));
        }

        #region Selection over Entire Triple Store

        /// <summary>
        /// Selects all Triples which have a Uri Node with the given Uri from all the Query Triples.
        /// </summary>
        /// <param name="uri">Uri.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(Uri uri)
        {
            return GetTriples(new UriNode(uri));
        }

        /// <summary>
        /// Selects all Triples which contain the given Node from all Graphs in the Triple Store.
        /// </summary>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(INode n)
        {
            return from g in _graphs
                from t in g.GetTriples(n)
                select t;
        }

        /// <summary>
        /// Selects all Triples where the Object is a Uri Node with the given Uri from all Graphs in the Triple Store.
        /// </summary>
        /// <param name="u">Uri.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            return GetTriplesWithObject(new UriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Object is a given Node from all Graphs in the Triple Store.
        /// </summary>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            return from g in _graphs
                from t in g.GetTriplesWithObject(n)
                select t;
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a given Node from all Graphs in the Triple Store.
        /// </summary>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            return from g in _graphs
                from t in g.GetTriplesWithPredicate(n)
                select t;
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri from all Graphs in the Triple Store.
        /// </summary>
        /// <param name="u">Uri.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            return GetTriplesWithPredicate(new UriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Subject is a given Node from all Graphs in the Triple Store.
        /// </summary>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            return from g in _graphs
                from t in g.GetTriplesWithSubject(n)
                select t;
        }

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri from all Graphs in the Triple Store.
        /// </summary>
        /// <param name="u">Uri.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            return GetTriplesWithSubject(new UriNode(u));
        }

        /// <summary>
        /// Selects all the Triples with the given Subject-Predicate pair from all the Query Triples.
        /// </summary>
        /// <param name="subj">Subject.</param>
        /// <param name="predicate">Predicate.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode predicate)
        {
            return from g in _graphs
                from t in g.GetTriplesWithSubjectPredicate(subj, predicate)
                select t;
        }

        /// <summary>
        /// Selects all the Triples with the given Predicate-Object pair from all the Query Triples.
        /// </summary>
        /// <param name="predicate">Predicate.</param>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode predicate, INode obj)
        {
            return from g in _graphs
                from t in g.GetTriplesWithPredicateObject(predicate, obj)
                select t;
        }

        /// <summary>
        /// Selects all the Triples with the given Subject-Object pair from all the Query Triples.
        /// </summary>
        /// <param name="subj">Subject.</param>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return from g in _graphs
                from t in g.GetTriplesWithSubjectObject(subj, obj)
                select t;
        }

        #endregion

        #region Selection over Subset of Triple Store

        /// <summary>
        /// Selects all Triples which have a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
        /// <param name="uri">Uri.</param>
        /// <returns></returns>
        [Obsolete("Replaced by GetTriples(List<IRefNode>, Uri)")]
        public IEnumerable<Triple> GetTriples(List<Uri> graphUris, Uri uri)
        {
            return GetTriples(graphUris, new UriNode(uri));
        }

        /// <summary>
        /// Selects all Triples which contain the given Node from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        [Obsolete("Replaced by GetTriples(List<IRefNode>, INode)")]
        public IEnumerable<Triple> GetTriples(List<Uri> graphUris, INode n)
        {
            var graphNames = graphUris.Select(u => u == null ? null : new UriNode(u) as IRefNode).ToList();
            return GetTriples(graphNames, n);
        }

        /// <summary>
        /// Selects all Triples which have a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
        /// <param name="uri">Uri.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(List<IRefNode> graphNames, Uri uri)
        {
            return GetTriples(graphNames, new UriNode(uri));
        }

        /// <summary>
        /// Selects all Triples which contain the given Node from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(List<IRefNode> graphNames, INode n)
        {
            return from g in _graphs
                where graphNames.Contains(g.Name)
                from t in g.GetTriples(n)
                select t;
        }

        /// <summary>
        /// Selects all Triples where the Object is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
        /// <param name="u">Uri.</param>
        /// <returns></returns>
        [Obsolete("Replaced by GetTriplesWithObject(List<IRefNode>, Uri)")]
        public IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, Uri u)
        {
            return GetTriplesWithObject(graphUris, new UriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Object is a given Node from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        [Obsolete("Replaced by GetTriplesWithObject(List<IRefNode>, INode)")]
        public IEnumerable<Triple> GetTriplesWithObject(List<Uri> graphUris, INode n)
        {
            var graphNames = graphUris.Select(x => x == null ? null : new UriNode(x) as IRefNode).ToList();
            return GetTriplesWithObject(graphNames, n);
        }

        /// <summary>
        /// Selects all Triples where the Object is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
        /// <param name="u">Uri.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(List<IRefNode> graphNames, Uri u)
        {
            return GetTriplesWithObject(graphNames, new UriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Object is a given Node from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(List<IRefNode> graphNames, INode n)
        {
            return from g in _graphs
                where graphNames.Contains(g.Name)
                from t in g.GetTriplesWithObject(n)
                select t;
        }
        /// <summary>
        /// Selects all Triples where the Predicate is a given Node from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        [Obsolete("Replaced by GetTriplesWithPredicate(List<IRefNode>, INode)")]
        public IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, INode n)
        {
            var graphNames = graphUris.Select(x => x == null ? null : new UriNode(x) as IRefNode).ToList();
            return GetTriplesWithPredicate(graphNames, n);
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
        /// <param name="u">Uri.</param>
        /// <returns></returns>
        [Obsolete("Replaced by GetTriplesWithPredicate(List<IRefNode>, Uri)")]
        public IEnumerable<Triple> GetTriplesWithPredicate(List<Uri> graphUris, Uri u)
        {
            return GetTriplesWithPredicate(graphUris, new UriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
        /// <param name="u">Uri.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(List<IRefNode> graphNames, Uri u)
        {
            return GetTriplesWithPredicate(graphNames, new UriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Predicate is a given Node from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(List<IRefNode> graphNames, INode n)
        {
            return from g in _graphs
                   where graphNames.Contains(g.Name)
                   from t in g.GetTriplesWithPredicate(n)
                   select t;
        }

        /// <summary>
        /// Selects all Triples where the Subject is a given Node from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        [Obsolete("Replaced by GetTriplesWithSubject(List<IRefNode>, INode)")]
        public IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, INode n)
        {
            IEnumerable<Triple> ts = from g in _graphs
                                     where graphUris.Contains(g.BaseUri)
                                     from t in g.GetTriplesWithSubject(n)
                                     select t;

            return ts;
        }

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphUris">List of the Graph URIs of Graphs you want to select over.</param>
        /// <param name="u">Uri.</param>
        /// <returns></returns>
        [Obsolete("Replaced by GetTriplesWithSubject(List<IRefNode>, Uri)")]
        public IEnumerable<Triple> GetTriplesWithSubject(List<Uri> graphUris, Uri u)
        {
            return GetTriplesWithSubject(graphUris, new UriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Subject is a Uri Node with the given Uri from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
        /// <param name="u">Uri.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(List<IRefNode> graphNames, Uri u)
        {
            return GetTriplesWithSubject(graphNames, new UriNode(u));
        }

        /// <summary>
        /// Selects all Triples where the Subject is a given Node from a Subset of Graphs in the Triple Store.
        /// </summary>
        /// <param name="graphNames">List of the names of Graphs you want to select over.</param>
        /// <param name="n">Node.</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(List<IRefNode> graphNames, INode n)
        {
            return from g in _graphs
                where graphNames.Contains(g.Name)
                from t in g.GetTriplesWithSubject(n)
                select t;
        }
        #endregion


        #endregion

        #region Loading with Inference

        /// <summary>
        /// Applies Inference to the given Graph.
        /// </summary>
        /// <param name="g">Graph to apply inference to.</param>
        public void ApplyInference(IGraph g)
        {
            // Apply Inference if we have any Inference Engines defined
            if (_reasoners.Count > 0)
            {
                // Set up Inference Graph if needed
                if (_storeInferencesExternally)
                {
                    if (!_graphs.Contains(_inferenceGraphUri))
                    {
                        IGraph i = new ThreadSafeGraph(_inferenceGraphUri) {BaseUri = _inferenceGraphUri.Uri};
                        _graphs.Add(i, true);
                    }
                }

                // Apply inference
                foreach (IInferenceEngine reasoner in _reasoners)
                {
                    if (_storeInferencesExternally)
                    {
                        reasoner.Apply(g, _graphs[_inferenceGraphUri]);
                    }
                    else
                    {
                        reasoner.Apply(g);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an Inference Engine to the Triple Store.
        /// </summary>
        /// <param name="reasoner">Reasoner to add.</param>
        public void AddInferenceEngine(IInferenceEngine reasoner)
        {
            _reasoners.Add(reasoner);

            // Apply Inference to all existing Graphs
            if (_graphs.Count > 0)
            {
                lock (_graphs)
                {
                    // Have to do a ToList() in case someone else inserts a Graph
                    // Which ApplyInference may do if the Inference information is stored in a special Graph
                    foreach (IGraph g in _graphs.ToList())
                    {
                        ApplyInference(g);
                    }
                }
            }
        }

        /// <summary>
        /// Removes an Inference Engine from the Triple Store.
        /// </summary>
        /// <param name="reasoner">Reasoner to remove.</param>
        public void RemoveInferenceEngine(IInferenceEngine reasoner)
        {
            _reasoners.Remove(reasoner);
        }

        /// <summary>
        /// Clears all Inference Engines from the Triple Store.
        /// </summary>
        public void ClearInferenceEngines()
        {
            _reasoners.Clear();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of a Triple Store.
        /// </summary>
        public override void Dispose()
        {
            _graphs.Dispose();
        }

        #endregion

        #region IUpdateableTripleStore Members

        /// <summary>
        /// Executes an Update against the Triple Store.
        /// </summary>
        /// <param name="update">SPARQL Update Command(s).</param>
        /// <remarks>
        /// As per the SPARQL 1.1 Update specification the command string may be a sequence of commands.
        /// </remarks>
        public void ExecuteUpdate(string update)
        {
            var parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commandSet = parser.ParseFromString(update);
            ExecuteUpdate(commandSet);
        }

        /// <summary>
        /// Executes a single Update Command against the Triple Store.
        /// </summary>
        /// <param name="update">SPARQL Update Command.</param>
        public void ExecuteUpdate(SparqlUpdateCommand update)
        {
            var context = new SparqlUpdateEvaluationContext(new InMemoryDataset(this), new LeviathanUpdateOptions());
            update.Evaluate(context);
        }

        /// <summary>
        /// Executes a set of Update Commands against the Triple Store.
        /// </summary>
        /// <param name="updates">SPARQL Update Command Set.</param>
        public void ExecuteUpdate(SparqlUpdateCommandSet updates)
        {
            var context = new SparqlUpdateEvaluationContext(new InMemoryDataset(this), new LeviathanUpdateOptions());

            for (var i = 0; i < updates.CommandCount; i++)
            {
                updates[i].Evaluate(context);
            }
        }

        #endregion

        /// <summary>
        /// Event Handler for the <see cref="BaseGraphCollection.GraphAdded">Graph Added</see> event of the underlying Graph Collection which calls the normal event processing of the parent class <see cref="BaseTripleStore">BaseTripleStore</see> and then applies Inference to the newly added Graph.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Graph Event Arguments.</param>
        protected override void OnGraphAdded(object sender, GraphEventArgs args)
        {
            base.OnGraphAdded(sender, args);
            ApplyInference(args.Graph);
        }
    }
}
