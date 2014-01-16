/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
namespace  org.topbraid.spin.util {

/**
 * A set that uses ConcurrentHashMap as its implementation.
 * 
 * @author Jeremy
 *
 * @param <E>
 */
public class ConcurrentHashSet<E> : AbstractSet<E> {

    private /*sealed*/ ConcurrentDictionary<E, Boolean> _delegate = new ConcurrentHashMap<E, Boolean>();

	public ConcurrentHashSet() {
	}

	override public bool add(E o) {
		if (o == null) {
			return false;
		}
		return _delegate.putIfAbsent(o, Boolean.True) == null;
	}


	override public void clear() {
		_delegate.Clear();
	}

	override public bool contains(Object o) {
		return _delegate.ContainsKey(o);
	}


	override public IEnumerator<E> iterator() {
		return _delegate.Keys.GetEnumerator();
	}


	override public bool remove(Object o) {
		if (o == null) {
			return false;
		}
		return _delegate.Remove(o) != null;
	}


	override public int size() {
		return _delegate.Keys.size();
	}

}
}