/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
namespace VDS.RDF.Query.Spin.Model.visitor
{

    /**
     * A visitor to visit the various types of expression elements.
     * 
     * @author Holger Knublauch
     */
    public interface IExpressionVisitor
    {

        void visit(IAggregation aggregation);


        void visit(IFunctionCall functionCall);


        void visit(INode node);


        void visit(IVariable variable);
    }
}