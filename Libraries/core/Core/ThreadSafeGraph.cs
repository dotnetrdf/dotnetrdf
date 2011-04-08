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

#if !NO_RWLOCK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.RDF
{
    /// <summary>
    /// A Thread Safe version of the <see cref="Graph">Graph</see> class
    /// </summary>
    /// <threadsafety instance="true">Should be safe for almost any concurrent read and write access scenario, internally managed using a <see cref="ReaderWriterLockSlim">ReaderWriterLockSlim</see>.  If you encounter any sort of Threading/Concurrency issue please report to the <a href="mailto:dotnetrdf-bugs@lists.sourceforge.net">dotNetRDF Bugs Mailing List</a></threadsafety>
    /// <remarks>Performance will be marginally worse than a normal <see cref="Graph">Graph</see> but in multi-threaded scenarios this will likely be offset by the benefits of multi-threading.</remarks>
    public class ThreadSafeGraph : Graph
    {
        /// <summary>
        /// Locking Manager for the Graph
        /// </summary>
        protected ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Creates a new Thread Safe Graph
        /// </summary>
        public ThreadSafeGraph() 
            : base(new IndexedThreadSafeTripleCollection()) 
        {
            this._nodes = new ThreadSafeNodeCollection();
        }

        #region Triple Assertion and Retraction

        /// <summary>
        /// Asserts a Triple in the Graph
        /// </summary>
        /// <param name="t">The Triple to add to the Graph</param>
        public override void Assert(Triple t)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Assert(t);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples to add to the Graph</param>
        public override void Assert(List<Triple> ts)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Assert(ts);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Asserts multiple Triples in the Graph
        /// </summary>
        /// <param name="ts">Array of Triples to add</param>
        public override void Assert(Triple[] ts)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Assert(ts);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Asserts a List of Triples in the graph
        /// </summary>
        /// <param name="ts">List of Triples in the form of an IEnumerable</param>
        public override void Assert(IEnumerable<Triple> ts)
        {
            this.Assert(ts.ToList());
        }

        /// <summary>
        /// Retracts a Triple from the Graph
        /// </summary>
        /// <param name="t">Triple to Retract</param>
        /// <remarks>Current implementation may have some defunct Nodes left in the Graph as only the Triple is retracted</remarks>
        public override void Retract(Triple t)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Retract(t);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retracts a List of Triples from the graph
        /// </summary>
        /// <param name="ts">List of Triples to retract from the Graph</param>
        public override void Retract(List<Triple> ts)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Retract(ts);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retracts multiple Triples from the Graph
        /// </summary>
        /// <param name="ts">Array of Triples to retract</param>
        public override void Retract(Triple[] ts)
        {
            try
            {
                this._lockManager.EnterWriteLock();
                base.Retract(ts);
            }
            finally
            {
                this._lockManager.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retracts a enumeration of Triples from the graph
        /// </summary>
        /// <param name="ts">Enumeration of Triples to retract</param>
        public override void Retract(IEnumerable<Triple> ts)
        {
            this.Retract(ts.ToList());
        }

        #endregion

        /// <summary>
        /// Creates a new Blank Node ID and returns it
        /// </summary>
        /// <returns></returns>
        public override string GetNextBlankNodeID()
        {
            String id = String.Empty;
            try 
            {
                this._lockManager.EnterWriteLock();
                id = base.GetNextBlankNodeID();
            }
            finally 
            {
                this._lockManager.ExitWriteLock();
                if (id.Equals(String.Empty))
                {
                    throw new RdfException("Unable to generate a new Blank Node ID due to a Threading issue");
                }
            }
            return id;
        }

        /// <summary>
        /// Disposes of a Graph
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this._lockManager.Dispose();
        }

        #region Node Selection

        /// <summary>
        /// Returns the Blank Node with the given Identifier
        /// </summary>
        /// <param name="nodeId">The Identifier of the Blank Node to select</param>
        /// <returns>Either the Blank Node or null if no Node with the given Identifier exists</returns>
        public override IBlankNode GetBlankNode(string nodeId)
        {
            IBlankNode b = null;
            try
            {
                this._lockManager.EnterReadLock();
                b = base.GetBlankNode(nodeId);
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return b;
        }

        /// <summary>
        /// Returns the LiteralNode with the given Value if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value exists</returns>
        /// <remarks>The LiteralNode in the Graph must have no Language or DataType set</remarks>
        public override ILiteralNode GetLiteralNode(string literal)
        {
            ILiteralNode l = null;
            try
            {
                this._lockManager.EnterReadLock();
                l = base.GetLiteralNode(literal);
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return l;
        }

        /// <summary>
        /// Returns the LiteralNode with the given Value in the given Language if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <param name="langspec">The Language Specifier for the Node to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value and Language Specifier exists</returns>
        public override ILiteralNode GetLiteralNode(string literal, string langspec)
        {
            ILiteralNode l = null;
            try
            {
                this._lockManager.EnterReadLock();
                l = base.GetLiteralNode(literal, langspec);
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return l;
        }

        /// <summary>
        /// Returns the LiteralNode with the given Value and given Data Type if it exists
        /// </summary>
        /// <param name="literal">The literal value of the Node to select</param>
        /// <param name="datatype">The Uri for the Data Type of the Literal to select</param>
        /// <returns>Either the LiteralNode Or null if no Node with the given Value and Data Type exists</returns>
        public override ILiteralNode GetLiteralNode(string literal, Uri datatype)
        {
            ILiteralNode l = null;
            try
            {
                this._lockManager.EnterReadLock();
                l = base.GetLiteralNode(literal, datatype);
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return l;
        }

        /// <summary>
        /// Returns the UriNode with the given QName if it exists
        /// </summary>
        /// <param name="qname">The QName of the Node to select</param>
        /// <returns></returns>
        public override IUriNode GetUriNode(string qname)
        {
            IUriNode u = null;
            try
            {
                this._lockManager.EnterReadLock();
                u = base.GetUriNode(qname);
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return u;
        }

        /// <summary>
        /// Returns the UriNode with the given Uri if it exists
        /// </summary>
        /// <param name="uri">The Uri of the Node to select</param>
        /// <returns>Either the UriNode Or null if no Node with the given Uri exists</returns>
        public override IUriNode GetUriNode(Uri uri)
        {
            IUriNode u = null;
            try
            {
                this._lockManager.EnterReadLock();
                u = base.GetUriNode(uri);
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return u;
        }

        /// <summary>
        /// Gets all the Nodes according to some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Nodes</returns>
        public override IEnumerable<INode> GetNodes(ISelector<INode> selector)
        {
            List<INode> nodes = new List<INode>();
            try
            {
                this._lockManager.EnterReadLock();
                nodes = base.GetNodes(selector).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return nodes;
        }

        #endregion

        #region Triple Selection

        /// <summary>
        /// Gets all the Triples involving the given Node
        /// </summary>
        /// <param name="n">The Node to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriples(INode n)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriples(n).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all Triples which are selected by the final Selector in the Chain (where the results of each Selector are used to initialise the next Selector in the chain and selection applied to the whole Graph each time)
        /// </summary>
        /// <param name="firstSelector">Selector Class which does the initial Selection</param>
        /// <param name="selectorChain">Chain of Dependent Selectors to perform the Selection</param>
        /// <returns>Zero/More Triples</returns>
        /// <remarks>This method is used to apply a series of Selectors where each filter is applied to the entire Graph but is initialised with the results of the previous Selector in the chain.  This means that something eliminated in a given step can potentially be selected by a later Selector in the Chain.</remarks>
        public override IEnumerable<Triple> GetTriples(ISelector<Triple> firstSelector, List<IDependentSelector<Triple>> selectorChain)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriples(firstSelector, selectorChain).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples which meet some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triple</returns>
        public override IEnumerable<Triple> GetTriples(ISelector<Triple> selector)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriples(selector).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all Triples which are selected by all the Selectors in the Chain (with the Selectors applied in order to the result set of the previous Selector)
        /// </summary>
        /// <param name="selectorChain">Chain of Selector Classes to perform the Selection</param>
        /// <returns>Zero/More Triples</returns>
        /// <remarks>This method is used to apply a series of Selectors where each filters the results of the previous.  Each application of a Selector potentially reduces the results set, anything eliminated in a given step cannot possibly be selected by a later Selector in the Chain.</remarks>
        public override IEnumerable<Triple> GetTriples(List<ISelector<Triple>> selectorChain)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriples(selectorChain).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples involving the given Uri
        /// </summary>
        /// <param name="uri">The Uri to find Triples involving</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriples(Uri uri)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriples(uri).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Object
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Object</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriplesWithObject(n).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with an Object matching some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithObject(ISelector<INode> selector)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriplesWithObject(selector).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Object
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Object</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriplesWithObject(u).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Predicate
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Predicate</param>
        /// <returns></returns>
        public override IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriplesWithPredicate(n).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with a Predicate matching some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithPredicate(ISelector<INode> selector)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriplesWithPredicate(selector).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Predicate
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Predicate</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriplesWithPredicate(u).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with the given Node as the Subject
        /// </summary>
        /// <param name="n">The Node to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriplesWithSubject(n).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with a Subject matching some arbitrary criteria as embodied in a Selector
        /// </summary>
        /// <param name="selector">Selector class which performs the Selection</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithSubject(ISelector<INode> selector)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriplesWithSubject(selector).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Gets all the Triples with the given Uri as the Subject
        /// </summary>
        /// <param name="u">The Uri to find Triples with it as the Subject</param>
        /// <returns>Zero/More Triples</returns>
        public override IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            List<Triple> triples = new List<Triple>();
            try
            {
                this._lockManager.EnterReadLock();
                triples = base.GetTriplesWithSubject(u).ToList();
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return triples;
        }

        /// <summary>
        /// Checks whether any Triples Exist which match a given Selector
        /// </summary>
        /// <param name="selector">Selector Class which performs the Selection</param>
        /// <returns></returns>
        public override bool TriplesExist(ISelector<Triple> selector)
        {
            bool exist = false;
            try
            {
                this._lockManager.EnterReadLock();
                exist = base.TriplesExist(selector);
            }
            finally
            {
                this._lockManager.ExitReadLock();
            }
            return exist;
        }

        #endregion
    }

    /// <summary>
    /// A Thread Safe version of the <see cref="Graph">Graph</see> class
    /// </summary>
    /// <threadsafety instance="true">Should be safe for almost any concurrent read and write access scenario, internally managed using a <see cref="ReaderWriterLockSlim">ReaderWriterLockSlim</see>.  If you encounter any sort of Threading/Concurrency issue please report to the <a href="mailto:dotnetrdf-bugs@lists.sourceforge.net">dotNetRDF Bugs Mailing List</a></threadsafety>
    /// <remarks>
    /// <para>
    /// Performance will be marginally worse than a normal <see cref="Graph">Graph</see> but in multi-threaded scenarios this will likely be offset by the benefits of multi-threading.
    /// </para>
    /// <para>
    /// Since this is a non-indexed version load performance will be better but query performance better
    /// </para>
    /// </remarks>
    public class NonIndexedThreadSafeGraph : ThreadSafeGraph
    {
        /// <summary>
        /// Creates a new non-indexed Thread Safe Graph
        /// </summary>
        public NonIndexedThreadSafeGraph()
            : base()
        {
            this._triples = new ThreadSafeTripleCollection();
        }
    }
}

#endif