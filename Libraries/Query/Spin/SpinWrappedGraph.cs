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

        private Uri _baseUri;

        internal HashSet<Triple> additions = new HashSet<Triple>(RDFUtil.tripleEqualityComparer);

        internal HashSet<Triple> removals = new HashSet<Triple>(RDFUtil.tripleEqualityComparer);

        internal bool _readonly = true;

        internal bool IsChanged {
            get {
                return additions.Count > 0 || removals.Count > 0; 
            }
            private set { }
        }

        #region Events helpers

        /// <summary>
        /// Helper method for raising the triple events manually
        /// </summary>
        /// <param name="t">Triple</param>
        protected void RaiseTripleEvent(Triple t, bool asserted)
        {
            TripleEventHandler d = asserted ? this.TripleAsserted : this.TripleRetracted;
            GraphEventHandler e = this.Changed;
            if (d != null || e != null)
            {
                TripleEventArgs args = new TripleEventArgs(t, this, asserted);
                if (d != null) d(this, args);
                if (e != null) e(this, new GraphEventArgs(this, args));
            }
        }

        protected void RaiseClearedEvent()
        {
            GraphEventHandler d = this.Cleared;
            GraphEventHandler e = this.Changed;
            GraphEventArgs args = new GraphEventArgs(this);
            if (d != null)
            {
                d(this, args);
            }
            if (e != null)
            {
                e(this, args);
            }
        }

        #endregion

        internal void Reset() {
            additions.Clear();
            removals.Clear();
        }

        public bool Assert(Triple t) {
            if (_readonly) {
                throw new Exception("This graph is marked as read only");
            }
            removals.Remove(t);
            additions.Add(t);
            RaiseTripleEvent(t, true);
            return true;
        }

        public bool Retract(Triple t)
        {
            if (_readonly)
            {
                throw new Exception("This graph is marked as read only");
            }
            additions.Remove(t);
            removals.Add(t);
            RaiseTripleEvent(t, false);
            return true;
        }

        public bool Assert(IEnumerable<Triple> ts)
        {
            bool result = false;
            foreach (Triple t in ts)
            {
                result |= Assert(t);
            }
            return result;
        }

        public bool Retract(IEnumerable<Triple> ts)
        {
            bool result = false;
            foreach (Triple t in ts)
            {
                result |= Retract(t);
            }
            return result;
        }

        public void Clear()
        {
            if (_readonly)
            {
                throw new Exception("This graph is marked as read only");
            }
            RaiseClearedEvent();
            throw new NotImplementedException();
        }

        public Uri BaseUri
        {
            get
            {
                return _baseUri;
            }
            set
            {
                _baseUri = value;
            }
        }

        public void Dispose()
        {
            Reset();
        }

        #region IGraph remaining implementation

        // TODO provide the triple selection methods

        public bool IsEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public INamespaceMapper NamespaceMap
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<INode> Nodes
        {
            get { throw new NotImplementedException(); }
        }

        public BaseTripleCollection Triples
        {
            get { throw new NotImplementedException(); }
        }


        public IUriNode CreateUriNode()
        {
            throw new NotImplementedException();
        }

        public IUriNode CreateUriNode(string qname)
        {
            throw new NotImplementedException();
        }

        public IBlankNode GetBlankNode(string nodeId)
        {
            throw new NotImplementedException();
        }

        public ILiteralNode GetLiteralNode(string literal, string langspec)
        {
            throw new NotImplementedException();
        }

        public ILiteralNode GetLiteralNode(string literal)
        {
            throw new NotImplementedException();
        }

        public ILiteralNode GetLiteralNode(string literal, Uri datatype)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriples(Uri uri)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriples(INode n)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            throw new NotImplementedException();
        }

        public IUriNode GetUriNode(string qname)
        {
            throw new NotImplementedException();
        }

        public IUriNode GetUriNode(Uri uri)
        {
            throw new NotImplementedException();
        }

        public bool ContainsTriple(Triple t)
        {
            throw new NotImplementedException();
        }

        public void Merge(IGraph g)
        {
            throw new NotImplementedException();
        }

        public void Merge(IGraph g, bool keepOriginalGraphUri)
        {
            throw new NotImplementedException();
        }

        public bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
        {
            throw new NotImplementedException();
        }

        public bool IsSubGraphOf(IGraph g)
        {
            throw new NotImplementedException();
        }

        public bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
        {
            throw new NotImplementedException();
        }

        public bool HasSubGraph(IGraph g)
        {
            throw new NotImplementedException();
        }

        public bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping)
        {
            throw new NotImplementedException();
        }

        public GraphDiffReport Difference(IGraph g)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable ToDataTable()
        {
            throw new NotImplementedException();
        }

        public Uri ResolveQName(string qname)
        {
            throw new NotImplementedException();
        }

        public event TripleEventHandler TripleAsserted;

        public event TripleEventHandler TripleRetracted;

        public event GraphEventHandler Changed;

        public event CancellableGraphEventHandler ClearRequested;

        public event GraphEventHandler Cleared;

        public event CancellableGraphEventHandler MergeRequested;

        public event GraphEventHandler Merged;

        public IBlankNode CreateBlankNode()
        {
            throw new NotImplementedException();
        }

        public IBlankNode CreateBlankNode(string nodeId)
        {
            throw new NotImplementedException();
        }

        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            throw new NotImplementedException();
        }

        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            throw new NotImplementedException();
        }

        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            throw new NotImplementedException();
        }

        public ILiteralNode CreateLiteralNode(string literal)
        {
            throw new NotImplementedException();
        }

        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            throw new NotImplementedException();
        }

        public IUriNode CreateUriNode(Uri uri)
        {
            throw new NotImplementedException();
        }

        public IVariableNode CreateVariableNode(string varname)
        {
            throw new NotImplementedException();
        }

        public string GetNextBlankNodeID()
        {
            throw new NotImplementedException();
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
