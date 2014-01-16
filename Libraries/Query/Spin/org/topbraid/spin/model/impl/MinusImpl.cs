/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.model.visitor;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{
    public class MinusImpl : ElementImpl, IMinus
    {

        public MinusImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        override public void visit(IElementVisitor visitor)
        {
            visitor.visit(this);
        }


        // TODO PRINT CONTEXT SHOULD DEPEND ON THE MODEL TO AVOID ADDING RESOURCE-CONSUMMING ATTERNS WHERE NOT NEEDED
        override public void print(IContextualSparqlPrinter p)
        {
            p.printKeyword("MINUS");
            printNestedElementList(p);
        }
    }
}