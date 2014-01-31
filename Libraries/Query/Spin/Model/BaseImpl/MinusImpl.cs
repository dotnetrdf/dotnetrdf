/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model
{
    public class MinusImpl : ElementImpl, IMinus
    {

        public MinusImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}


        // TODO PRINT CONTEXT SHOULD DEPEND ON THE MODEL TO AVOID ADDING RESOURCE-CONSUMMING ATTERNS WHERE NOT NEEDED
        override public void print(ISparqlFactory p)
        {
            p.printKeyword("MINUS");
            printNestedElementList(p);
        }
    }
}