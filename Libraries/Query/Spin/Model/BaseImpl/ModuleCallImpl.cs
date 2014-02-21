/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
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