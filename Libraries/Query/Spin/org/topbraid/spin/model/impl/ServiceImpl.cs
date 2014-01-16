/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.model.visitor;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{

    public class ServiceImpl : ElementImpl, IService
    {

        public ServiceImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        public Uri getServiceURI()
        {
            IResource s = getResource(SP.PropertyServiceURI);
            if (s != null && s.isUri())
            {
                IVariable variable = SPINFactory.asVariable(s);
                if (variable == null)
                {
                    return s.Uri();
                }
            }
            return null;
        }


        public IVariable getServiceVariable()
        {
            IResource s = getResource(SP.PropertyServiceURI);
            if (s != null)
            {
                IVariable variable = SPINFactory.asVariable(s);
                if (variable != null)
                {
                    return variable;
                }
            }
            return null;
        }


        override public void print(IContextualSparqlPrinter p)
        {
            p.printKeyword("SERVICE");
            IVariable var = getServiceVariable();
            if (var != null)
            {
                p.print(" ");
                p.printVariable(var.getName());
            }
            else
            {
                Uri uri = getServiceURI();
                if (uri != null)
                {
                    p.print(" ");
                    p.printURIResource(Resource.Get(RDFUtil.CreateUriNode(uri), getModel()));
                }
            }
            printNestedElementList(p);
        }


        override public void visit(IElementVisitor visitor)
        {
            visitor.visit(this);
        }
    }
}