/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
// TODO is this really needed ?
using VDS.RDF.Query.Spin.Model.IO;
namespace VDS.RDF.Query.Spin.SparqlUtil
{

    /**
     * An interface for objects that can be printed into a PrintContext.
     * This is implemented by SPIN Queries and Elements.
     * 
     * @author Holger Knublauch
     */
    public interface IPrintable
    {

        /**
         * Instructs this to print itself into a given PrintContext.
         * Implementations need to use the provided functions of p.
         * @param p  the context
         */
        void Print(ISparqlPrinter p);

    }

}