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

    public class NotExistsImpl : ElementImpl, INotExists
    {

        public NotExistsImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        override public void visit(IElementVisitor visitor)
        {
            visitor.visit(this);
        }


        override public void print(IContextualSparqlPrinter p)
        {
            p.printKeyword("NOT EXISTS");
            printNestedElementList(p);
        }
    }
}