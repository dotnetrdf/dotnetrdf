/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin;
using VDS.RDF;
namespace org.topbraid.spin.util
{
    /**
     * Extended polymorphism support for Jena, checking whether the INode
     * has a given rdf:type. 
     * 
     * @author Holger Knublauch
     */
    public abstract class ImplementationByType : Implementation
    {

        private /*sealed*/ INode type;


        public ImplementationByType(INode type)
        {
            this.type = type;
        }


        override public bool canWrap(INode node, IGraph eg)
        {
            return eg.asGraph().Contains(node, RDF.type, type);
        }
    }
}