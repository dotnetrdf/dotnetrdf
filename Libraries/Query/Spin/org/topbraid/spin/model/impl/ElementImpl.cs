/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using org.topbraid.spin.model.visitor;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{

    public abstract class ElementImpl : AbstractSPINResource, IElement
    {

        public ElementImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }

        public abstract void visit(IElementVisitor visitor);
        
    }
}