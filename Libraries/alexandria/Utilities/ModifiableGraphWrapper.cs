using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;

namespace VDS.Alexandria.Utilities
{
    public class ModifiableGraphWrapper : IGraph
    {
        private IGenericIOManager _manager;
        private IGraph _g;
        private Queue<PersistenceAction> _actions = new Queue<PersistenceAction>();

        public ModifiableGraphWrapper(IGraph g, IGenericIOManager manager)
        {
            this._g = g;
            this._manager = manager;
        }

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
            if (!this._g.Triples.Contains(t))
            {
                this._g.Assert(t);
                this._actions.Enqueue(new PersistenceAction(t));
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
            if (this._g.Triples.Contains(t))
            {
                this._g.Retract(t);
                this._actions.Enqueue(new PersistenceAction(t, true));
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
                this._actions.Enqueue(new PersistenceAction(t, true));
            }
            this._g.Clear();
        }

        public BlankNode CreateBlankNode(string nodeId)
        {
            return this._g.CreateBlankNode(nodeId);
        }

        public BlankNode CreateBlankNode()
        {
            return this._g.CreateBlankNode();
        }

        public string GetNextBlankNodeID()
        {
            return this._g.GetNextBlankNodeID();
        }

        public GraphLiteralNode CreateGraphLiteralNode(IGraph subgraph)
        {
            return this._g.CreateGraphLiteralNode(subgraph);
        }

        public GraphLiteralNode CreateGraphLiteralNode()
        {
            return this._g.CreateGraphLiteralNode();
        }

        public LiteralNode CreateLiteralNode(string literal)
        {
            return this._g.CreateLiteralNode(literal);
        }

        public LiteralNode CreateLiteralNode(string literal, Uri datatype)
        {
            return this._g.CreateLiteralNode(literal, datatype);
        }

        public LiteralNode CreateLiteralNode(string literal, string langspec)
        {
            return this._g.CreateLiteralNode(literal, langspec);
        }

        public UriNode CreateUriNode()
        {
            return this._g.CreateUriNode();
        }

        public UriNode CreateUriNode(string qname)
        {
            return this._g.CreateUriNode(qname);
        }

        public UriNode CreateUriNode(Uri uri)
        {
            return this._g.CreateUriNode(uri);
        }

        public VariableNode CreateVariableNode(String varname)
        {
            return this._g.CreateVariableNode(varname);
        }

        public BlankNode GetBlankNode(string nodeId)
        {
            return this._g.GetBlankNode(nodeId);
        }

        public LiteralNode GetLiteralNode(string literal, string langspec)
        {
            return this._g.GetLiteralNode(literal, langspec);
        }

        public LiteralNode GetLiteralNode(string literal)
        {
            return this._g.GetLiteralNode(literal);
        }

        public LiteralNode GetLiteralNode(string literal, Uri datatype)
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

        public UriNode GetUriNode(string qname)
        {
            return this._g.GetUriNode(qname);
        }

        public UriNode GetUriNode(Uri uri)
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
                    this.Assert(new Triple(Tools.CopyNode(t.Subject, this, keepOriginalGraphUri), Tools.CopyNode(t.Predicate, this, keepOriginalGraphUri), Tools.CopyNode(t.Object, this, keepOriginalGraphUri)));
                }
            }
            else
            {   //Prepare a mapping of Blank Nodes to Blank Nodes
                Dictionary<INode, BlankNode> mapping = new Dictionary<INode, BlankNode>();

                foreach (Triple t in g.Triples)
                {
                    INode s, p, o;
                    if (t.Subject.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Subject))
                        {
                            BlankNode temp = this.CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Subject.GraphUri;
                            mapping.Add(t.Subject, temp);
                        }
                        s = mapping[t.Subject];
                    }
                    else
                    {
                        s = Tools.CopyNode(t.Subject, this, keepOriginalGraphUri);
                    }

                    if (t.Predicate.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Predicate))
                        {
                            BlankNode temp = this.CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Predicate.GraphUri;
                            mapping.Add(t.Predicate, temp);
                        }
                        p = mapping[t.Predicate];
                    }
                    else
                    {
                        p = Tools.CopyNode(t.Predicate, this, keepOriginalGraphUri);
                    }

                    if (t.Object.NodeType == NodeType.Blank)
                    {
                        if (!mapping.ContainsKey(t.Object))
                        {
                            BlankNode temp = this.CreateBlankNode();
                            if (keepOriginalGraphUri) temp.GraphUri = t.Object.GraphUri;
                            mapping.Add(t.Object, temp);
                        }
                        o = mapping[t.Object];
                    }
                    else
                    {
                        o = Tools.CopyNode(t.Object, this, keepOriginalGraphUri);
                    }

                    this.Assert(new Triple(s, p, o));
                }
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

        public DataTable ToDataTable()
        {
            return this._g.ToDataTable();
        }

        public void Dispose()
        {
            if (this._actions.Count > 0)
            {
                PersistenceAction action = this._actions.Dequeue();
                bool isDelete = action.IsDelete;
                List<Triple> ts = new List<Triple>();
                ts.Add(action.Triple);

                do
                {
                    action = this._actions.Dequeue();
                    if (action.IsDelete != isDelete)
                    {
                        //Action switches to/from delete so process the current batch then continue
                        if (isDelete)
                        {
                            this._manager.UpdateGraph(this._g.BaseUri, null, ts);
                        }
                        else
                        {
                            this._manager.UpdateGraph(this._g.BaseUri, ts, null);
                        }
                        isDelete = action.IsDelete;
                        ts.Clear();
                    }
                    ts.Add(action.Triple);
                } while (this._actions.Count > 0);

                //Most likely will be left with a batch to process at the end
                if (ts.Count > 0)
                {
                    if (isDelete)
                    {
                        this._manager.UpdateGraph(this._g.BaseUri, null, ts);
                    }
                    else
                    {
                        this._manager.UpdateGraph(this._g.BaseUri, ts, null);
                    }
                }
            }
            this._g.Dispose();
        }
    }
}
