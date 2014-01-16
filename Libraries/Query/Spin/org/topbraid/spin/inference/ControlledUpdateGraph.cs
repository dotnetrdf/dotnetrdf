
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace  org.topbraid.spin.inference 
{

/**
 * A Graph implementation that is used by SPIN inferencing to
 * support UPDATE rules.
 * The Graph wraps another delegate Graph, and delegates most of
 * its operations to that.
 * However, it records which of the triples have actually been
 * added or deleted - the usual Graph policy is to perform those
 * operations regardless of whether a triple was already there.
 * This makes it possible to determine whether further iterations
 * are needed, and which new rdf:type triples have been added. 
 * 
 * @author Holger Knublauch
 */
class ControlledUpdateGraph 
    : GraphWithPerform 
{
	
	private BulkUpdateHandler buh;

	private IGraph _delegate;
	
	private HashSet<Triple> addedTriples = new HashSet<Triple>();
	
	private HashSet<Triple> deletedTriples = new HashSet<Triple>();
	
	
	ControlledUpdateGraph(IGraph _delegate) {
		this._delegate = _delegate;
		this.buh = new SimpleBulkUpdateHandler(this);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="t"></param>
    /// <exception cref="AddDeniedException">AddDeniedException</exception>
	override public void add(Triple t) {
		performAdd(t);
	}

	
	override public void clear() {
		foreach(Triple triple in find(INode.ANY, INode.ANY, INode.ANY).ToList()) {
			delete(triple);
		}
	}


	override public bool dependsOn(IGraph other) {
		return _delegate.dependsOn(other);
	}

	override public TransactionHandler getTransactionHandler() {
		return _delegate.getTransactionHandler();
	}

	override public BulkUpdateHandler getBulkUpdateHandler() {
		return buh;
	}

	override public Capabilities getCapabilities() {
		return _delegate.getCapabilities();
	}

	override public GraphEventManager getEventManager() {
		return _delegate.getEventManager();
	}

	override public GraphStatisticsHandler getStatisticsHandler() {
		return _delegate.getStatisticsHandler();
	}

	override public PrefixMapping getPrefixMapping() {
		return _delegate.getPrefixMapping();
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    /// <exception cref="DeleteDeniedException">DeleteDeniedException</exception>
	override public void delete(Triple t) {
		performDelete(t);
	}

	override public IEnumerable<Triple> find(TripleMatch m) {
		return _delegate.find(m);
	}

	override public IEnumerable<Triple> find(INode s, INode p, INode o) {
		return _delegate.find(s, p, o);
	}

	override public bool isIsomorphicWith(IGraph g) {
		return _delegate.isIsomorphicWith(g);
	}

	override public bool contains(INode s, INode p, INode o) {
		return _delegate.ContainsTriple(new Triple(s, p, o));
	}

	override public bool contains(Triple t) {
		return _delegate.ContainsTriple(t);
	}

	override public void close() {
		//_delegate.close();
	}

	override public bool isEmpty() {
		return _delegate.IsEmpty;
	}

	override public int size() {
        return _delegate.Triples.Count; //size();
	}

	override public bool isClosed() {
        return false; // _delegate.isClosed();
	}


	override public void performAdd(Triple t) {
		if(!_delegate.ContainsTriple(t)) {
			addedTriples.Add(t);
		}
		_delegate.Assert(t);
	}


	override public void performDelete(Triple t) {
		if(_delegate.ContainsTriple(t)) {
			deletedTriples.Add(t);
		}
		_delegate.Retract(t);
	}
	
	
	override public void remove(INode s, INode p, INode o) {
		foreach(Triple triple in find(s, p, o).ToList()) {
			delete(triple);
		}
	}


	public IEnumerable<Triple> getAddedTriples() {
		return addedTriples;
	}
	
	
	public bool isChanged() {
		return addedTriples.Count!=0 || deletedTriples.Count!=0; 
	}
}

}
