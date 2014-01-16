/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{


    public abstract class TripleImpl : TupleImpl
    {

        public TripleImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public IResource getPredicate()
        {
            return getResource(SP.PropertyPredicate);
        }


        override public void print(IContextualSparqlPrinter p)
        {
            print(getSubject(), p);
            p.print(" ");
            print(getPredicate(), p, true);
            p.print(" ");
            print(getObject(), p);
        }
    }
}