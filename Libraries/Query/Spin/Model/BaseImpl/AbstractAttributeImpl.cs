/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{

    public abstract class AbstractAttributeImpl : AbstractSPINResource, IAbstractAttribute
    {

        public AbstractAttributeImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public IResource getPredicate()
        {
            IResource r = getResource(SPL.PropertyPredicate);
            if (r!=null && r.isUri())
            {
                return r;
            }
            else
            {
                return null;
            }
        }


        public IResource getValueType()
        {
            return getObject(SPL.PropertyValueType);
        }


        public String getComment()
        {
            return getString(RDFS.PropertyComment);
        }

        public bool IsOptional()
        {
            return (bool)getBoolean(SPL.PropertyOptional);
        }

        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }
    }
}