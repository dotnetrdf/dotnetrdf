/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model
{
    public class ExistsImpl : ElementImpl, IExists
    {

        public ExistsImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}


        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("EXISTS");
            printNestedElementList(p);
        }
    }
}