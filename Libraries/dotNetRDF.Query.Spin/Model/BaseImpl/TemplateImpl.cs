/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
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