/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * Part of a SPARQL expression that calls an Aggregation (such as SUM).
     * 
     * @author Holger Knublauch
     */
    public interface IAggregation : IPrintable, IResource
    {

        IVariable getAs();


        INode getExpression();


        bool isDistinct();
    }
}