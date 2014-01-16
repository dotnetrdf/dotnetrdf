/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
using System.Collections.Generic;
using System;
namespace  org.topbraid.spin.util {

/**
 * A simple implementation of the ParameterProvider interface, based
 * on a HashMap.
 * 
 * @author Holger Knublauch
 */
public class SimpleParameterProvider : ParameterProvider {

	private /*sealed*/ Dictionary<String,String> map;


	public SimpleParameterProvider()
        : this(new Dictionary<String, String>())
    {
	}


	public SimpleParameterProvider(Dictionary<String,String> map) {
		this.map = map;
	}
	
	
	/**
	 * Adds a new entry to the internal Map.
	 * This is typically used in conjunction with the constructor
	 * without arguments.
	 * @param key  the parameter key
	 * @param value  the value
	 */
	public void add(String key, String value) {
		map[key] = value;
	}


	public String getParameter(String key) {
        if (map.ContainsKey(key))
        {
            return map[key];
        }
        return null;
	}


	public IEnumerator<String> listParameterNames() {
		return map.Keys.GetEnumerator();
	}
}
}