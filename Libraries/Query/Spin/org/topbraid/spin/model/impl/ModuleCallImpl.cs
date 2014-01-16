/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{
    public abstract class ModuleCallImpl : AbstractSPINResource, IModuleCall
    {

        public ModuleCallImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }

        public abstract IModule getModule();
    }
}