/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.Model.visitor;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{

    public class TriplePatternImpl : TripleImpl, ITriplePattern
    {

        public TriplePatternImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        public void visit(IElementVisitor visitor)
        {
            visitor.visit(this);
        }
    }
}