/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * The base interface of TriplePattern and TripleTemplate.
     * 
     * @author Holger Knublauch
     */
    public interface ITriple : IPrintable, IResource
    {

        /**
         * Gets the subject of this Triple, downcasting it into Variable if appropriate.
         * @return the subject
         */
        IResource getSubject();


        /**
         * Gets the predicate of this Triple, downcasting it into Variable if appropriate.
         * @return the predicate
         */
        IResource getPredicate();


        /**
         * Gets the object of this Triple, downcasting it into Variable if appropriate.
         * @return the object
         */
        IResource getObject();


        /**
         * Gets the object as a INode.
         * @return the object or null if it's not a resource (i.e., a literal)
         */
        IResource getObjectResource();
    }
}