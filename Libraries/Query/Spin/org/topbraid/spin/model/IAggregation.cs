/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
namespace org.topbraid.spin.model
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