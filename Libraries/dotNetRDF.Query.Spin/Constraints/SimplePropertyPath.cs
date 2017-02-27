/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using System;

namespace VDS.RDF.Query.Spin.Constraints
{
    /**
     * A property path that describes a mechanism to get values starting
     * from a given RDF node (root) by following a given predicate.
     * There are two subclasses for SP->O and OP->S paths.
     * 
     * @author Holger Knublauch
     */
    public abstract class SimplePropertyPath
    {

        private INode predicate;

        private INode root;


        public SimplePropertyPath(INode root, INode predicate)
        {
            this.predicate = predicate;
            this.root = root;
        }


        public INode getPredicate()
        {
            return predicate;
        }


        public INode getRoot()
        {
            return root;
        }
    }
}