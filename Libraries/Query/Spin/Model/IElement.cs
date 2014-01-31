/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.Model.visitor;
using VDS.RDF.Query.Spin.SparqlUtil;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * The abstract base interface for the various Element types.
     * 
     * @author Holger Knublauch
     */
    public interface IElement : IPrintable, IResource
    {

        /**
         * Visits this with a given visitor.
         * @param visitor  the visitor to visit this with
         */
        void visit(IElementVisitor visitor);
    }
}