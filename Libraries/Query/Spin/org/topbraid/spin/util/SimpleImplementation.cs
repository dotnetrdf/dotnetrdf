/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using System;
namespace  org.topbraid.spin.util {

/**
 * An extension of the Jena polymorphism mechanism.
 * 
 * @author Holger Knublauch
 */
public class SimpleImplementation : ImplementationByType {

	//@SuppressWarnings("rawtypes")
	private Constructor constructor;


	//@SuppressWarnings({ "unchecked", "rawtypes" })
	public SimpleImplementation(INode type, Type implClass) 
        : base (type)
    {
		
		try {
			constructor = implClass.getConstructor(INode, IGraph);
		}
		catch (Exception t) {
			//t.printStackTrace();
		}
	}


    override public INode wrap(INode node, IGraph eg)
    {
		try {
            return (INode)constructor.newInstance(node, eg);
		}
		catch (Exception t) {
			//t.printStackTrace();
			return null;
		}
	}
}
}