/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
using VDS.RDF;
using System.Collections.Generic;
using System;
namespace  org.topbraid.spin.util {
/**
 * An abstract GraphListeners that forwards each call
 * into {@link #notifyAddTriple} and
 * {@link #notifyDeleteTriple} to
 * reduce the implementation burden of subclasses.
 * All of the bulk operations are forwarded to
 * {@link #notifyAddIterator} and
 * {@link #notifyDeleteIterator}.
 * So subclasses can override those two methods to
 * modify all the bulk operations, except the removeAll
 * ones.
 * For the removeAll operations, subclasses should implement
 * {@link #notifyRemoveAll(Graph, Triple)},
 * this is only called by the default implementation
 * of {@link #notifyEvent(Graph, Object)}.
 *
 *
 *
 * @author Holger Knublauch, Jeremy Carroll
 */
public abstract class AbstractGraphListener : GraphListener {



	public void notifyAddArray(Graph g, Triple[] triples) {
		notifyAddIterator(g,Arrays.asList(triples).GetEnumerator());
	}

	public void notifyAddGraph(Graph g, Graph added) {
		notifyAddIterator(g,added.find(Triple.ANY));
	}


	public void notifyAddIterator(Graph g, IEnumerator<Triple> it) {
		if (it is ClosableIterator) {
			// copy in case the find result is holding locks ...
			notifyAddList(g,IteratorCollection.iteratorToList(it));
		} else {
			while (it.MoveNext()) {
				Triple t = it.Current;
				notifyAddTriple(g, t);
			}
		}
	}


	public void notifyAddList(Graph g, List<Triple> triples) {
		notifyAddIterator(g, triples.GetEnumerator());
	}

	public void notifyDeleteArray(Graph g, Triple[] triples) {
		notifyDeleteIterator(g,Arrays.asList(triples).GetEnumerator());
	}
	public void notifyDeleteGraph(Graph g, Graph removed) {
		notifyDeleteIterator(g,removed.find(Triple.ANY));
	}



	public void notifyDeleteIterator(Graph g, IEnumerator<Triple> it) {
		if (it is ClosableIterator) {
			// copy in case the find result is holding locks ...
			notifyDeleteList(g,IteratorCollection.iteratorToList(it));
		} else {
			while (it.MoveNext()) {
				Triple triple = it.Current;
				notifyDeleteTriple(g, triple);
			}
		}
	}



	public void notifyDeleteList(Graph g, List<Triple> list) {
		notifyDeleteIterator(g, list.GetEnumerator());
	}



	/**
    <code>value</code> is usually a {@link GraphEvents}.
    Special attention is drawn to {@link GraphEvents#removeAll}
    and events whose {@link GraphEvents#getTitle()} is <code>"remove"</code>
    (see {@link GraphEvents#remove(INode, INode, INode)}. These correspond
    to the bulk operations {@link BulkUpdateHandler#removeAll()},
    and {@link BulkUpdateHandler#remove(INode, INode, INode)}, respectively.
    Unlike other notifications, the listener cannot tell which triples
    have been modified, since they have already been deleted by the time
    this event is sent, and the event does not include a record of them.
    This default implementation maps these two events to
    {@link #notifyRemoveAll(Graph, Triple)} calls.
    */
	public void notifyEvent(Graph source, Object value) {
		if (value is GraphEvents) {
			if (GraphEvents.removeAll.Equals(value)) {
				notifyRemoveAll(source,Triple.ANY);
			} else {
				GraphEvents evt = (GraphEvents)value;
				if ("remove".Equals(evt.getTitle())) {
                    notifyRemoveAll(source, (Triple)evt.getContent());
				}
			}
		}
	}

	/**
	 * Called after a removeAll modification. The
	 * actual triples deleted cannot be identified easily.
	 * See {@link #notifyEvent(Graph, Object)} for explanation
	 * of this method.
	 * @param source
	 * @param pattern The pattern of triples being removed, often {@link Triple#ANY}.
	 */
	protected abstract void notifyRemoveAll(Graph source, Triple pattern);
}
}