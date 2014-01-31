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

    public abstract class ElementImpl : AbstractSPINResource, IElement
    {

        public ElementImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }

        public abstract void visit(IElementVisitor visitor);
        
    }
}