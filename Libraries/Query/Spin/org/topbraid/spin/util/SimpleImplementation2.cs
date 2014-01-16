/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using System.Reflection;
namespace org.topbraid.spin.util
{
    /**
     * An extension of the Jena polymorphism mechanism.
     * In contrast to SimpleImplementation, this maps to two different RDF classes.
     * 
     * @author Holger Knublauch
     */
    public class SimpleImplementation2 : Implementation
    {

        //@SuppressWarnings("rawtypes")
        private ConstructorInfo constructor;

        private /*sealed*/ INode type1;

        private /*sealed*/ INode type2;


        //@SuppressWarnings({ "unchecked", "rawtypes" })
        public SimpleImplementation2(INode type1, INode type2, Type implClass)
        {
            this.type1 = type1;
            this.type2 = type2;
            try
            {
                constructor = implClass.GetConstructor(new Type[] { typeof(INode), typeof(Model) });
            }
            catch (Exception t)
            {
                throw t;
            }
        }


        override public bool canWrap(INode node, IGraph eg)
        {
            return eg.ContainsTriple(new Triple(node, RDF.type, type1)) ||
                    eg.ContainsTriple(new Triple(node, RDF.type, type2));
        }


        override public INode wrap(INode node, IGraph eg)
        {
            try
            {
                return (INode)constructor.Invoke(new object[] { node, eg });
            }
            catch (Exception t)
            {
                throw t;
                return null;
            }
        }
    }
}
