/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/

namespace VDS.RDF.Query.Spin.Model
{

    public class TriplePatternImpl : TripleImpl, ITriplePattern
    {

        public TriplePatternImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }

        //public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}