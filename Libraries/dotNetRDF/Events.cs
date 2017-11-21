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

namespace VDS.RDF
{
    #region Reader and Writer Warning Events

    /// <summary>
    /// Delegate Type for Warning Messages raised by RDF Readers
    /// </summary>
    /// <param name="warning">Warning Message</param>
    public delegate void RdfReaderWarning(String warning);

    /// <summary>
    /// Delegate Type for Warning Messages raised by RDF Writers
    /// </summary>
    /// <param name="message">Warning Message</param>
    public delegate void RdfWriterWarning(String message);

    /// <summary>
    /// Delegate Type for Warning Events raised by RDF Dataset Writers
    /// </summary>
    /// <param name="message">Warning Message</param>
    public delegate void StoreWriterWarning(String message);

    /// <summary>
    /// Delegate Type for Warning Events raised by RDF Dataset Readers
    /// </summary>
    /// <param name="message">Warning Message</param>
    public delegate void StoreReaderWarning(String message);

    /// <summary>
    /// Delegate Type for Warning Events raised by SPARQL Readers and Writers for Queries, Updates and Results
    /// </summary>
    /// <param name="message">Warning Message</param>
    public delegate void SparqlWarning(String message);

    #endregion

    #region Triple, Graph and Triple Store Events

    /// <summary>
    /// Delegate Type for Triple events raised by Graphs
    /// </summary>
    /// <param name="sender">Originator of the Event</param>
    /// <param name="args">Triple Event Arguments</param>
    public delegate void TripleEventHandler(Object sender, TripleEventArgs args);

    /// <summary>
    /// Delegate Type for Graph events raised by Graphs
    /// </summary>
    /// <param name="sender">Originator of the Event</param>
    /// <param name="args">Graph Event Arguments</param>
    public delegate void GraphEventHandler(Object sender, GraphEventArgs args);

    /// <summary>
    /// Delegate Type for Graph events raised by Graphs where event handlers may set a Cancel flag to cancel the subsequent operation
    /// </summary>
    /// <param name="sender">Originator of the Event</param>
    /// <param name="args">Graph Event Arguments</param>
    public delegate void CancellableGraphEventHandler(Object sender, CancellableGraphEventArgs args);

    /// <summary>
    /// Delegate Type for Triple Store events raised by Triple Stores
    /// </summary>
    /// <param name="sender">Originator of the event</param>
    /// <param name="args">Triple Store Event Arguments</param>
    public delegate void TripleStoreEventHandler(Object sender, TripleStoreEventArgs args);

    #endregion

    #region Event Argument Classes

    /// <summary>
    /// Event Arguments for Events regarding the assertion and retraction of Triples
    /// </summary>
    public class TripleEventArgs : EventArgs
    {
        private Triple _t;
        private IGraph _g;
        private bool _added = true;

        /// <summary>
        /// Creates a new set of Triple Event Arguments for the given Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="g">Graph the Triple Event occurred in</param>
        public TripleEventArgs(Triple t, IGraph g)
            : base()
        {
            _t = t;
            _g = g;
        }

        /// <summary>
        /// Creates a new set of Triple Event Arguments for the given Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="g">Graph the Triple Event occurred in</param>
        /// <param name="asserted">Was the Triple Asserted (if not then it was Retracted)</param>
        public TripleEventArgs(Triple t, IGraph g, bool asserted)
            : this(t, g)
        {
            _added = asserted;
        }

        /// <summary>
        /// Gets the Triple
        /// </summary>
        public Triple Triple
        {
            get
            {
                return _t;
            }
        }

        /// <summary>
        /// Gets the Graph the Triple belongs to (may be null)
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return _g;
            }
            internal set
            {
                _g = value;
            }
        }

        /// <summary>
        /// Gets the URI of the Graph the Triple belongs to (may be null)
        /// </summary>
        public Uri GraphUri
        {
            get
            {
                return (_g == null) ? null : _g.BaseUri;
            }
        }

        /// <summary>
        /// Gets whether the Triple was asserted
        /// </summary>
        public bool WasAsserted
        {
            get
            {
                return _added;
            }
        }

        /// <summary>
        /// Gets whether the Triple was retracted
        /// </summary>
        public bool WasRetracted
        {
            get
            {
                return !_added;
            }
        }
    }

    /// <summary>
    /// Event Arguments for Events regarding Graphs
    /// </summary>
    public class GraphEventArgs : EventArgs
    {
        private IGraph _g;
        private TripleEventArgs _args;

        /// <summary>
        /// Creates a new set of Graph Event Arguments
        /// </summary>
        /// <param name="g">Graph</param>
        public GraphEventArgs(IGraph g)
            : base()
        {
            _g = g;
        }

        /// <summary>
        /// Creates a new set of Graph Event Arguments
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="args">Triple Event Arguments</param>
        public GraphEventArgs(IGraph g, TripleEventArgs args)
            : this(g)
        {
            _args = args;
        }

        /// <summary>
        /// Gets the Graph
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return _g;
            }
        }

        /// <summary>
        /// Gets the Triple Event Arguments (if any)
        /// </summary>
        public TripleEventArgs TripleEvent
        {
            get
            {
                return _args;
            }
        }
    }

    /// <summary>
    /// Event Arguments for Events regarding Graphs which may be cancelled
    /// </summary>
    public class CancellableGraphEventArgs : GraphEventArgs
    {
        private bool _cancel;

        /// <summary>
        /// Creates a new set of Cancellable Graph Event Arguments
        /// </summary>
        /// <param name="g">Graph</param>
        public CancellableGraphEventArgs(IGraph g)
            : base(g) { }

        /// <summary>
        /// Creates a new set of Cancellable Graph Event Arguments
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="args">Triple Event Arguments</param>
        public CancellableGraphEventArgs(IGraph g, TripleEventArgs args)
            : base(g, args) { }

        /// <summary>
        /// Gets/Sets whether the Event should be cancelled
        /// </summary>
        public bool Cancel
        {
            get
            {
                return _cancel;
            }
            set
            {
                _cancel = value;
            }
        }
    }
    
    /// <summary>
    /// Event Arguments for Events regarding Graphs
    /// </summary>
    public class TripleStoreEventArgs : EventArgs
    {
        private ITripleStore _store;
        private GraphEventArgs _args;

        /// <summary>
        /// Creates a new set of Triple Store Event Arguments
        /// </summary>
        /// <param name="store">Triple Store</param>
        public TripleStoreEventArgs(ITripleStore store)
            : base()
        {
            _store = store;
        }

        /// <summary>
        /// Creates a new set of Triple Store Event Arguments
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="args">Graph Event Arguments</param>
        public TripleStoreEventArgs(ITripleStore store, GraphEventArgs args)
            : this(store)
        {
            _args = args;
        }

        /// <summary>
        /// Creates a new set of Triple Store Event Arguments
        /// </summary>
        /// <param name="store">Triple Store</param>
        /// <param name="g">Graph</param>
        public TripleStoreEventArgs(ITripleStore store, IGraph g)
            : this(store, new GraphEventArgs(g)) { }

        /// <summary>
        /// Gets the Triple Store
        /// </summary>
        public ITripleStore TripleStore
        {
            get
            {
                return _store;
            }
        }

        /// <summary>
        /// Gets the Graph Event Arguments (if any)
        /// </summary>
        public GraphEventArgs GraphEvent
        {
            get
            {
                return _args;
            }
        }
    }

    #endregion
}
