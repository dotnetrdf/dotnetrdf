/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{
    public class TripleTemplateImpl : TripleImpl, ITripleTemplate
    {

        public TripleTemplateImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        override public void Print(ISparqlPrinter p)
        {
            p.setNamedBNodeMode(true);
            base.Print(p);
            p.setNamedBNodeMode(false);
        }
    }
}