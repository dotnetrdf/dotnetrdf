/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{

    public class TemplateImpl : ModuleImpl, ITemplate
    {

        public TemplateImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        public String getLabelTemplate()
        {
            return getString(SPIN.PropertyLabelTemplate);
        }
    }
}