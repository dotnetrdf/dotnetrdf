/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{


    public class FunctionImpl : ModuleImpl, IFunction
    {

        public FunctionImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public INode getReturnType()
        {
            return getObject(SPIN.PropertyReturnType);
        }


        public bool isMagicProperty()
        {
            return hasProperty(RDF.type, SPIN.ClassMagicProperty);
        }


        public bool isPrivate()
        {
            return hasProperty(SPIN.PropertyPrivate, RDFUtil.TRUE);
        }
    }
}