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
using System.Linq;
using System.Text;
using VDS.RDF.Storage;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Spin.Util;


namespace VDS.RDF.Query.Spin
{
    /// <summary>
    /// A simple IGraph implementation that only tracks triple removal and additions
    /// </summary>
    public class SpinWrappedGraph : IGraph
    {
        internal HashSet<Triple> Additions = new HashSet<Triple>(RDFUtil.tripleEqualityComparer);

        internal HashSet<Triple> Removals = new HashSet<Triple>(RDFUtil.tripleEqualityComparer);

        internal bool Readonly = true;

        internal bool IsChanged => Additions.Count > 0 || Removals.Count > 0;

        #region Events helpers

        /// <summary>
        /// Helper method for raising the triple events manually
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="asserted">True to raise TripleAsserted, false to raise TripleRetracted</param>
        protected void RaiseTripleEvent(Triple t, bool asserted)
        {
            var d = asserted ? TripleAsserted : TripleRetracted;
            var e = Changed;
            if (d != null || e != null)
            {
                var args = new TripleEventArgs(t, this, asserted);
                d?.Invoke(this, args);
                e?.Invoke(this, new GraphEventArgs(this, args));
            }
        }

        internal void RaiseClearedEvent()
        {
            var d = Cleared;
            var e = Changed;
            var args = new GraphEventArgs(this);
            d?.Invoke(this, args);
            e?.Invoke(this, args);
        }

        #endregion

        internal void Reset() {
            Additions.Clear();
            Removals.Clear();
        }

        /// <inheritdoc />
        public bool Assert(Triple t) {
            if (Readonly) {
                throw new Exception("This graph is marked as read only");
            }
            Removals.Remove(t);
            Additions.Add(t);
            RaiseTripleEvent(t, true);
            return true;
        }

        /// <inheritdoc />
        public bool Retract(Triple t)
        {
            if (Readonly)
            {
                throw new Exception("This graph is marked as read only");
            }
            Additions.Remove(t);
            Removals.Add(t);
            RaiseTripleEvent(t, false);
            return true;
        }

        /// <inheritdoc />
        public bool Assert(IEnumerable<Triple> ts)
        {
            var result = false;
            foreach (var t in ts)
            {
                result |= Assert(t);
            }
            return result;
        }

        /// <inheritdoc />
        public bool Retract(IEnumerable<Triple> ts)
        {
            var result = false;
            foreach (var t in ts)
            {
                result |= Retract(t);
            }
            return result;
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (Readonly)
            {
                throw new Exception("This graph is marked as read only");
            }
            RaiseClearedEvent();
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Uri BaseUri { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Reset();
        }

        #region IGraph remaining implementation

        // TODO provide the triple selection methods

        /// <inheritdoc />
        public bool IsEmpty
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public INamespaceMapper NamespaceMap
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public IEnumerable<INode> Nodes
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        public BaseTripleCollection Triples
        {
            get { throw new NotImplementedException(); }
        }


        /// <inheritdoc />
        public IUriNode CreateUriNode()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IUriNode CreateUriNode(string qname)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IBlankNode GetBlankNode(string nodeId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ILiteralNode GetLiteralNode(string literal, string langspec)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ILiteralNode GetLiteralNode(string literal)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ILiteralNode GetLiteralNode(string literal, Uri datatype)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriples(Uri uri)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriples(INode n)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IUriNode GetUriNode(string qname)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IUriNode GetUriNode(Uri uri)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool ContainsTriple(Triple t)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Merge(IGraph g)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Merge(IGraph g, bool keepOriginalGraphUri)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsSubGraphOf(IGraph g)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool HasSubGraph(IGraph g)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public GraphDiffReport Difference(IGraph g)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Uri ResolveQName(string qname)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public event TripleEventHandler TripleAsserted;

        /// <inheritdoc />
        public event TripleEventHandler TripleRetracted;

        /// <inheritdoc />
        public event GraphEventHandler Changed;

        /// <inheritdoc />
        public event CancellableGraphEventHandler ClearRequested;

        /// <inheritdoc />
        public event GraphEventHandler Cleared;

        /// <inheritdoc />
        public event CancellableGraphEventHandler MergeRequested;

        /// <inheritdoc />
        public event GraphEventHandler Merged;

        /// <inheritdoc />
        public IBlankNode CreateBlankNode()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IBlankNode CreateBlankNode(string nodeId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ILiteralNode CreateLiteralNode(string literal)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IUriNode CreateUriNode(Uri uri)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IVariableNode CreateVariableNode(string varname)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string GetNextBlankNodeID()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ReadXml(System.Xml.XmlReader reader)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
