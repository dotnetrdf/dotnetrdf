using System;
using System.Collections.Generic;
#if !NO_DATA
using System.Data;
#endif
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF
{
    /// <summary>
    /// The Graph Persistence is a Wrapper around another Graph that can be used to batch persistence actions with the ability to Flush/Discard changes as desired.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When disposed any outstanding changes are always flushed so if you make changes which you don't want to persist be sure to call the <see cref="GraphPersistenceWrapper.Discard">Discard()</see> method before disposing of the Graph
    /// </para>
    /// <para>
    /// Implementors who wish to make persistent graphs should extend this class and override the <see cref="GraphPersistenceWrapper.SupportsTriplePersistence">SupportsTriplePersistence</see> property and the <see cref="GraphPersistenceWrapper.PersistInsertedTriples">PersistInsertedTriples()</see>, <see cref="GraphPersistenceWrapper.PersistDeletedTriples">PersistDeletedTriples()</see> and <see cref="GraphPersistenceWrapper.PersistGraph">PersistGraph()</see> methods.  If you return true for the property then the <see cref="GraphPersistenceWrapper.PersistInsertedTriples">PersistInsertedTriples()</see> and <see cref="GraphPersistenceWrapper.PersistDeletedTriples">PersistDeletedTriples()</see> methods will be invoked to do persistence on batches of Triples.  If your persistence mechanism requires persisting the entire graph at once return false for the property and override the <see cref="GraphPersistenceWrapper.PersistGraph">PersistGraph()</see> method appropriately.
    /// </para>
    /// <h3>Warning</h3>
    /// <para>
    /// Note that the wrapper does not automatically dispose of the wrapped graph when the wrapper is Dispose, this is by design since disposing of the wrapped Graph can have unintended consequences
    /// </para>
    /// </remarks>
    public class GraphPersistenceWrapper : IGraph, ITransactionalGraph
    {
        protected readonly IGraph _g;
        private List<TriplePersistenceAction> _actions = new List<TriplePersistenceAction>();
        private bool _alwaysQueueActions = false;

        public GraphPersistenceWrapper()
            : this(new Graph()) { }

        public GraphPersistenceWrapper(bool alwaysQueueActions)
            : this(new Graph(), alwaysQueueActions) { }

        public GraphPersistenceWrapper(IGraph g)
        {
            if (g == null) throw new ArgumentNullException("graph", "Wrapped Graph cannot be null");
            this._g = g;
        }

        public GraphPersistenceWrapper(IGraph g, bool alwaysQueueActions)
            : this(g)
        {
            this._alwaysQueueActions = alwaysQueueActions;
        }

        ~GraphPersistenceWrapper()
        {
            this.Dispose(false);
        }

        #region Wrappers around all the standard IGraph stuff

        public Uri BaseUri
        {
            get
            {
                return this._g.BaseUri;
            }
            set
            {
                this._g.BaseUri = value;
            }
        }

        public bool IsEmpty
        {
            get 
            { 
                return this._g.IsEmpty; 
            }
        }

        public NamespaceMapper NamespaceMap
        {
            get
            { 
                return this._g.NamespaceMap; 
            }
        }

        public BaseNodeCollection Nodes
        {
            get 
            { 
                return this._g.Nodes; 
            }
        }

        public BaseTripleCollection Triples
        {
            get 
            {
                return this._g.Triples; 
            }
        }

        public void Assert(List<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                this.Assert(t);
            }
        }

        public void Assert(Triple[] ts)
        {
            foreach (Triple t in ts)
            {
                this.Assert(t);
            }
        }

        public void Assert(Triple t)
        {
            if (this._alwaysQueueActions || !this._g.Triples.Contains(t))
            {
                this._g.Assert(t);
                this._actions.Add(new TriplePersistenceAction(t));
            }
        }

        public void Assert(IEnumerable<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                this.Assert(t);
            }
        }

        public void Retract(Triple[] ts)
        {
            foreach (Triple t in ts)
            {
                this.Retract(t);
            }
        }

        public void Retract(Triple t)
        {
            if (this._alwaysQueueActions || this._g.Triples.Contains(t))
            {
                this._g.Retract(t);
                this._actions.Add(new TriplePersistenceAction(t, true));
            }
        }

        public void Retract(List<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                this.Retract(t);
            }
        }

        public void Retract(IEnumerable<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                this.Retract(t);
            }
        }

        public void Clear()
        {
            foreach (Triple t in this._g.Triples)
            {
                this._actions.Add(new TriplePersistenceAction(t, true));
            }
            this._g.Clear();
        }

        public IBlankNode CreateBlankNode(string nodeId)
        {
            IBlankNode b = this._g.CreateBlankNode(nodeId);
            return (IBlankNode)b.CopyNode(this);
        }

        public IBlankNode CreateBlankNode()
        {
            IBlankNode b = this._g.CreateBlankNode();
            return (IBlankNode)b.CopyNode(this);
        }

        public string GetNextBlankNodeID()
        {
            return this._g.GetNextBlankNodeID();
        }

        public IGraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._g.CreateGraphLiteralNode(subgraph);
        }

        public IGraphLiteralNode CreateGraphLiteralNode()
        {
            return this._g.CreateGraphLiteralNode();
        }

        public ILiteralNode CreateLiteralNode(string literal)
        {
            return this._g.CreateLiteralNode(literal);
        }

        public ILiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._g.CreateLiteralNode(literal, datatype);
        }

        public ILiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return this._g.CreateLiteralNode(literal, langspec);
        }

        public IUriNode CreateUriNode()
        {
            return this._g.CreateUriNode();
        }

        public IUriNode CreateUriNode(string qname)
        {
            return this._g.CreateUriNode(qname);
        }

        public IUriNode CreateUriNode(Uri uri)
        {
            return this._g.CreateUriNode(uri);
        }

        public IVariableNode CreateVariableNode(String varname)
        {
            return this._g.CreateVariableNode(varname);
        }

        public IBlankNode GetBlankNode(string nodeId)
        {
            return this._g.GetBlankNode(nodeId);
        }

        public ILiteralNode GetLiteralNode(string literal, string langspec)
        {
            return this._g.GetLiteralNode(literal, langspec);
        }

        public ILiteralNode GetLiteralNode(string literal)
        {
            return this._g.GetLiteralNode(literal);
        }

        public ILiteralNode GetLiteralNode(string literal, Uri datatype)
        {
            return this._g.GetLiteralNode(literal, datatype);
        }

        public IEnumerable<INode> GetNodes(ISelector<INode> selector)
        {
            return this._g.GetNodes(selector);
        }

        public IEnumerable<Triple> GetTriples(ISelector<Triple> firstSelector, List<IDependentSelector<Triple>> selectorChain)
        {
            return this._g.GetTriples(firstSelector, selectorChain);
        }

        public IEnumerable<Triple> GetTriples(List<ISelector<Triple>> selectorChain)
        {
            return this._g.GetTriples(selectorChain);
        }

        public IEnumerable<Triple> GetTriples(Uri uri)
        {
            return this._g.GetTriples(uri);
        }

        public IEnumerable<Triple> GetTriples(ISelector<Triple> selector)
        {
            return this._g.GetTriples(selector);
        }

        public IEnumerable<Triple> GetTriples(INode n)
        {
            return this._g.GetTriples(n);
        }

        public IEnumerable<Triple> GetTriplesWithObject(Uri u)
        {
            return this._g.GetTriplesWithObject(u);
        }

        public IEnumerable<Triple> GetTriplesWithObject(ISelector<INode> selector)
        {
            return this._g.GetTriplesWithObject(selector);
        }

        public IEnumerable<Triple> GetTriplesWithObject(INode n)
        {
            return this._g.GetTriplesWithObject(n);
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(INode n)
        {
            return this._g.GetTriplesWithPredicate(n);
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
        {
            return this._g.GetTriplesWithPredicate(u);
        }

        public IEnumerable<Triple> GetTriplesWithPredicate(ISelector<INode> selector)
        {
            return this._g.GetTriplesWithPredicate(selector);
        }

        public IEnumerable<Triple> GetTriplesWithSubject(INode n)
        {
            return this._g.GetTriplesWithSubject(n);
        }

        public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
        {
            return this._g.GetTriplesWithSubject(u);
        }

        public IEnumerable<Triple> GetTriplesWithSubject(ISelector<INode> selector)
        {
            return this._g.GetTriplesWithSubject(selector);
        }

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return this._g.GetTriplesWithSubjectPredicate(subj, pred);
        }

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return this._g.GetTriplesWithSubjectObject(subj, obj);
        }

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return this._g.GetTriplesWithPredicateObject(pred, obj);
        }

        public IUriNode GetUriNode(string qname)
        {
            return this._g.GetUriNode(qname);
        }

        public IUriNode GetUriNode(Uri uri)
        {
            return this._g.GetUriNode(uri);
        }

        public bool TriplesExist(ISelector<Triple> selector)
        {
            return this._g.TriplesExist(selector);
        }

        public bool ContainsTriple(Triple t)
        {
            return this._g.ContainsTriple(t);
        }

        /// <summary>
        /// Merges another Graph into the current Graph
        /// </summary>
        /// <param name="g">Graph to Merge into this Graph</param>
        /// <remarks>The Graph on which you invoke this method will preserve its Blank Node IDs while the Blank Nodes from the Graph being merged in will be given new IDs as required in the scope of this Graph.</remarks>
        public virtual void Merge(IGraph g)
        {
            this.Merge(g, false);
        }

        /// <summary>
        /// Merges another Graph into the current Graph
        /// </summary>
        /// <param name="g">Graph to Merge into this Graph</param>
        /// <param name="keepOriginalGraphUri">Indicates that the Merge should preserve the Graph URIs of Nodes so they refer to the Graph they originated in</param>
        /// <remarks>
        /// <para>
        /// The Graph on which you invoke this method will preserve its Blank Node IDs while the Blank Nodes from the Graph being merged in will be given new IDs as required in the scope of this Graph.
        /// </para>
        /// <para>
        /// The Graph will raise the <see cref="MergeRequested">MergeRequested</see> event before the Merge operation which gives any event handlers the oppurtunity to cancel this event.  When the Merge operation is completed the <see cref="Merged">Merged</see> event is raised
        /// </para>
        /// </remarks>
        public virtual void Merge(IGraph g, bool keepOriginalGraphUri)
        {
            //First copy and Prefixes across which aren't defined in this Graph
            this._g.NamespaceMap.Import(g.NamespaceMap);

            if (this.IsEmpty)
            {
                //Empty Graph so do a quick copy
                foreach (Triple t in g.Triples)
                {
                    this.Assert(new Triple(Tools.CopyNode(t.Subject, this._g, keepOriginalGraphUri), Tools.CopyNode(t.Predicate, this._g, keepOriginalGraphUri), Tools.CopyNode(t.Object, this._g, keepOriginalGraphUri)));
                }
            }
            else
            {   //Prepare a mapping of Blank Nodes to Blank Nodes
                Dictionary<INode, IBlankNode> mapping = new Dictionary<INode, IBlankNode>();

                foreach (Triple t in g.Triples)
                {
                    INode s, p, o;
                    if (t.Subject.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Subject))
                        {
                            IBlankNode temp = this.CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Subject.GraphUri;
                            mapping.Add(t.Subject, temp);
                        }
                        s = mapping[t.Subject];
                    }
                    else
                    {
                        s = Tools.CopyNode(t.Subject, this._g, keepOriginalGraphUri);
                    }

                    if (t.Predicate.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Predicate))
                        {
                            IBlankNode temp = this.CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Predicate.GraphUri;
                            mapping.Add(t.Predicate, temp);
                        }
                        p = mapping[t.Predicate];
                    }
                    else
                    {
                        p = Tools.CopyNode(t.Predicate, this._g, keepOriginalGraphUri);
                    }

                    if (t.Object.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Object))
                        {
                            IBlankNode temp = this.CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Object.GraphUri;
                            mapping.Add(t.Object, temp);
                        }
                        o = mapping[t.Object];
                    }
                    else
                    {
                        o = Tools.CopyNode(t.Object, this._g, keepOriginalGraphUri);
                    }

                    this.Assert(new Triple(s, p, o));
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is IGraph)
            {
                Dictionary<INode, INode> temp;
                return this.Equals((IGraph)obj, out temp);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return this._g.Equals(g, out mapping);
        }

        public bool IsSubGraphOf(IGraph g)
        {
            return this._g.IsSubGraphOf(g);
        }

        public bool IsSubGraphOf(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return this._g.IsSubGraphOf(g, out mapping);
        }

        public bool HasSubGraph(IGraph g)
        {
            return this._g.HasSubGraph(g);
        }

        public bool HasSubGraph(IGraph g, out Dictionary<INode, INode> mapping)
        {
            return this._g.HasSubGraph(g, out mapping);
        }

        public GraphDiffReport Difference(IGraph g)
        {
            return this._g.Difference(g);
        }

        public Uri ResolveQName(string qname)
        {
            return this._g.ResolveQName(qname);
        }

        public event TripleEventHandler TripleAsserted;

        public event TripleEventHandler TripleRetracted;

        public event GraphEventHandler Changed;

        public event CancellableGraphEventHandler ClearRequested;

        public event GraphEventHandler Cleared;

        public event CancellableGraphEventHandler MergeRequested;

        public event GraphEventHandler Merged;

#if !NO_DATA

        public DataTable ToDataTable()
        {
            return this._g.ToDataTable();
        }

#endif

        #endregion

        #region Persistence Implementation

        /// <summary>
        /// Flushes all changes which have yet to be persisted to the underlying storage
        /// </summary>
        public void Flush()
        {
            if (this._actions.Count > 0)
            {
                if (this.SupportsTriplePersistence)
                {
                    TriplePersistenceAction action = this._actions[0];
                    bool isDelete = action.IsDelete;
                    List<Triple> ts = new List<Triple>();
                    ts.Add(action.Triple);

                    int i = 1;
                    while (i < this._actions.Count)
                    {
                        action = this._actions[i];
                        if (action.IsDelete != isDelete)
                        {
                            //Action switches to/from delete so process the current batch then continue
                            if (isDelete)
                            {
                                this.PersistDeletedTriples(ts);
                            }
                            else
                            {
                                this.PersistInsertedTriples(ts);
                            }
                            isDelete = action.IsDelete;
                            ts.Clear();
                        }
                        ts.Add(action.Triple);
                        i++;
                    }

                    //Most likely will be left with a batch to process at the end
                    if (ts.Count > 0)
                    {
                        if (isDelete)
                        {
                            this.PersistDeletedTriples(ts);
                        }
                        else
                        {
                            this.PersistInsertedTriples(ts);
                        }
                    }
                }
                else
                {
                    this.PersistGraph();
                }
                this._actions.Clear();
            }
        }

        /// <summary>
        /// Discards all changes which have yet to be persisted so that they are not persisted to the underlying storage
        /// </summary>
        public void Discard()
        {
            int total = this._actions.Count;
            int i = this._actions.Count - 1;
            while (i >= 0)
            {
                TriplePersistenceAction action = this._actions[i];
                if (action.IsDelete)
                {
                    this._g.Assert(action.Triple);
                }
                else
                {
                    this._g.Retract(action.Triple);
                }
                i--;
            }

            if (total == this._actions.Count)
            {
                this._actions.Clear();
            }
            else
            {
                this._actions.RemoveRange(0, total);
            }
        }

        /// <summary>
        /// Used to indicate whether the persistence mechansim can persist batches of Triples
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <strong>true</strong> then the <see cref="GraphPersistenceWrapper.PersistInsertedTriples">PersistInsertedTriples()</see> and <see cref="GraphPersistenceWrapper.PersistDeletedTriples">PersistDeletedTriples()</see> methods are used to persist changes when the <see cref="GraphPersistenceWrapper.Flush">Flush()</see> method is called.  If <strong>false</strong> then the <see cref="GraphPersistenceWrapper.PersistGraph">PersistGraph()</see> method will be invoked instead.
        /// </para>
        /// </remarks>
        protected virtual bool SupportsTriplePersistence
        {
            get
            {
                return true;
            }
        }

        protected virtual void PersistInsertedTriples(IEnumerable<Triple> ts)
        {
            //Does Nothing
        }

        protected virtual void PersistDeletedTriples(IEnumerable<Triple> ts)
        {
            //Does Nothing
        }

        protected virtual void PersistGraph()
        {
            //Does Nothing
        }

        #endregion

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            this.Flush();
        }
    }

#if !NO_STORAGE

    public class StoreGraphPersistenceWrapper : GraphPersistenceWrapper
    {
        private IGenericIOManager _manager;

        public StoreGraphPersistenceWrapper(IGenericIOManager manager, IGraph g, Uri graphUri, bool writeOnly)
            : base(g, writeOnly)
        {
            if (manager == null) throw new ArgumentNullException("manager","Cannot persist to a null Generic IO Manager");
            if (manager.IsReadOnly) throw new ArgumentException("Cannot persist to a read-only Generic IO Manager", "manager");
            if (writeOnly && !manager.UpdateSupported) throw new ArgumentException("If writeOnly is set to true then the Generic IO Manager must support triple level updates", "writeOnly");
            if (writeOnly && !g.IsEmpty) throw new ArgumentException("If writeOnly is set to true then the input graph must be empty", "writeOnly");

            this._manager = manager;
            this.BaseUri = graphUri;
       }

        public StoreGraphPersistenceWrapper(IGenericIOManager manager, IGraph g, bool writeOnly)
            : this(manager, g, g.BaseUri, writeOnly) { }

        public StoreGraphPersistenceWrapper(IGenericIOManager manager, IGraph g)
            : this(manager, g, g.BaseUri, false) { }

        public StoreGraphPersistenceWrapper(IGenericIOManager manager, Uri graphUri, bool writeOnly)
            : base(writeOnly)
        {
            if (manager == null) throw new ArgumentNullException("manager", "Cannot persist to a null Generic IO Manager");
            if (manager.IsReadOnly) throw new ArgumentException("Cannot persist to a read-only Generic IO Manager", "manager");
            if (writeOnly && !manager.UpdateSupported) throw new ArgumentException("If writeOnly is set to true then the Generic IO Manager must support triple level updates", "writeOnly");

            this._manager = manager;
            this.BaseUri = graphUri;

            if (!writeOnly)
            {
                //Load in the existing data
                this._manager.LoadGraph(this._g, graphUri);
            }
        }

        public StoreGraphPersistenceWrapper(IGenericIOManager manager, Uri graphUri)
            : this(manager, graphUri, false) { }


        protected override bool SupportsTriplePersistence
        {
            get
            {
                return this._manager.UpdateSupported;
            }
        }

        protected override void PersistDeletedTriples(IEnumerable<Triple> ts)
        {
            if (this._manager.UpdateSupported)
            {
                this._manager.UpdateGraph(this.BaseUri, null, ts);
            }
            else
            {
                throw new NotSupportedException("The underlying Generic IO Manager does not support Triple Level persistence");
            }
        }

        protected override void PersistInsertedTriples(IEnumerable<Triple> ts)
        {
            if (this._manager.UpdateSupported)
            {
                this._manager.UpdateGraph(this.BaseUri, ts, null);
            }
            else
            {
                throw new NotSupportedException("The underlying Generic IO Manager does not support Triple Level persistence");
            }
        }

        protected override void PersistGraph()
        {
            this._manager.SaveGraph(this);
        }
    }

#endif

    public class FileGraphPersistenceWrapper : GraphPersistenceWrapper
    {
        private String _filename;

        public FileGraphPersistenceWrapper(IGraph g, String filename)
            : base(g)
        {
            if (filename == null) throw new ArgumentException("Cannot persist to a null Filename", "filename");
            this._filename = filename;
        }

        public FileGraphPersistenceWrapper(String filename)
            : base(new Graph())
        {
            if (filename == null) throw new ArgumentException("Cannot persist to a null Filename", "filename");

            if (File.Exists(filename))
            {
                this._g.LoadFromFile(filename);
            }
        }

        protected override bool SupportsTriplePersistence
        {
            get
            {
                return false;
            }
        }

        protected override void PersistGraph()
        {
            this.SaveToFile(this._filename);
        }
    }
}
