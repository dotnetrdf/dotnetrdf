/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/

namespace VDS.RDF.Query.Spin.Model
{

    public abstract class ElementImpl : AbstractSPINResource, IElement
    {

        public ElementImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }

        //public abstract void visit(IElementVisitor visitor);
        
    }
}