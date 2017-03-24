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
using VDS.RDF.Configuration;
using VDS.RDF.Query.FullText.Indexing;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// A Full Text Indexed Dataset is a wrapper around another dataset and provides automatic full text indexing of data that is added and removed
    /// </summary>
    public class FullTextIndexedDataset
        : WrapperDataset
    {
        private IFullTextIndexer _indexer;
        private bool _indexNow = false;

        /// <summary>
        /// Creates a new Full Text Indexed Dataset
        /// </summary>
        /// <param name="dataset">Dataset to wrap</param>
        /// <param name="indexer">Indexer to use</param>
        /// <param name="indexNow">Whether the dataset provided should be indexed now, set to false if indexer is linked to an existing index for this data</param>
        /// <remarks>
        /// If <paramref name="indexNow"/> is true then the provided dataset will be fully indexed when this constructor is called
        /// </remarks>
        public FullTextIndexedDataset(ISparqlDataset dataset, IFullTextIndexer indexer, bool indexNow)
            : base(dataset)
        {
            this._indexer = indexer;

            //Index Now if requested
            this._indexNow = indexNow;
            if (indexNow)
            {
                foreach (IGraph g in this.Graphs)
                {
                    this._indexer.Index(g);
                }
            }
        }

        /// <summary>
        /// Creates a new Full Text Indexed Dataset
        /// </summary>
        /// <param name="dataset">Dataset to wrap</param>
        /// <param name="indexer">Indexer to use</param>
        /// <remarks>
        /// Does not do any indexing, assumes the provided dataset is already indexed.  When using this constructor only changes to the dataset affect the index
        /// </remarks>
        public FullTextIndexedDataset(ISparqlDataset dataset, IFullTextIndexer indexer)
            : this(dataset, indexer, false) { }

        /// <summary>
        /// Adds a Graph to the Dataset updating the Full Text Index appropriately
        /// </summary>
        /// <param name="g">Graph to add</param>
        public override bool AddGraph(IGraph g)
        {
            this._indexer.Index(g);
            return base.AddGraph(g);
        }

        /// <summary>
        /// Removes a Graph from the Dataset updating the Full Text Index appropriately
        /// </summary>
        /// <param name="graphUri">URI of the Graph to remove</param>
        public override bool RemoveGraph(Uri graphUri)
        {
            if (this.HasGraph(graphUri))
            {
                this._indexer.Unindex(this[graphUri]);
            }
            return base.RemoveGraph(graphUri);
        }

        /// <summary>
        /// Gets a modifiable graph from the store ensuring that modifications will update the Full Text Index
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override IGraph GetModifiableGraph(Uri graphUri)
        {
            //Use Events to pick up Triple Level changes in the Modifiable Graph
            //because writing a Graph wrapper for this seems like overkill when
            //there are just events we can hook into
            IGraph g = base.GetModifiableGraph(graphUri);
            g.TripleAsserted += new TripleEventHandler(this.HandleTripleAdded);
            g.TripleRetracted += new TripleEventHandler(this.HandleTripleRemoved);
            return g;
        }

        /// <summary>
        /// Handles the updating of the index when a triple is added
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Event Arguments</param>
        private void HandleTripleAdded(Object sender, TripleEventArgs args)
        {
            this._indexer.Index(args.Triple);
        }

        /// <summary>
        /// Handles the updating of the index when a triple is removed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Event Arguments</param>
        private void HandleTripleRemoved(Object sender, TripleEventArgs args)
        {
            this._indexer.Unindex(args.Triple);
        }

        /// <summary>
        /// Flushes changes to the Dataset and ensures the Index is up to date
        /// </summary>
        public override void Flush()
        {
            //Always flush the index in Flush because triple level index changes don't cause an automatic Flush()
            this._indexer.Flush();
            base.Flush();
        }

        /// <summary>
        /// Discards changes to the Dataset and ensures the Index is up to date
        /// </summary>
        public override void Discard()
        {
            //Always flush the index in Discard because triple level index changes don't cause an automatic Flush()
            this._indexer.Flush();
            base.Discard();
        }

        /// <summary>
        /// Serializes the Configuration of the Dataset
        /// </summary>
        /// <param name="context">Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            if (this._dataset is IConfigurationSerializable)
            {
                context.EnsureObjectFactory(typeof(FullTextObjectFactory));

                INode dataset = context.NextSubject;
                context.NextSubject = dataset;

                //First get the base class to serialize the main configuration
                base.SerializeConfiguration(context);

                //Then add additional configuration to the serialization
                if (this._indexer is IConfigurationSerializable)
                {
                    INode indexer = context.NextSubject;
                    context.NextSubject = indexer;
                    context.Graph.Assert(dataset, context.Graph.CreateUriNode(UriFactory.Create(FullTextHelper.PropertyIndexer)), indexer);
                    context.Graph.Assert(dataset, context.Graph.CreateUriNode(UriFactory.Create(FullTextHelper.PropertyIndexNow)), this._indexNow.ToLiteral(context.Graph));
                    ((IConfigurationSerializable)this._indexer).SerializeConfiguration(context);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to serialize configuration as the full text indexer is not serializable");
                }
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to serialize configuration as the inner dataset is not serializable");
            }
        }
    }
}
