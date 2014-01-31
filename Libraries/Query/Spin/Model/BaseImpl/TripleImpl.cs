/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
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


        override public void print(ISparqlFactory p)
        {
            print(getSubject(), p);
            p.print(" ");
            print(getPredicate(), p, true);
            p.print(" ");
            print(getObject(), p);
        }
    }
}