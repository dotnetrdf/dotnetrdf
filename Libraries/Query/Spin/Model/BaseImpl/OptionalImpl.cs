/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model
{

    public class OptionalImpl : ElementImpl, IOptional
    {

        public OptionalImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        override public void print(ISparqlFactory p)
        {
            p.printKeyword("OPTIONAL");
            printNestedElementList(p);
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}