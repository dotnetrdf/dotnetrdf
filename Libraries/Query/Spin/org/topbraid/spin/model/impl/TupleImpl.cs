/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.util;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{


    public abstract class TupleImpl : AbstractSPINResource
    {

        public TupleImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        public IResource getObject()
        {
            return getRDFNodeOrVariable(SP.PropertyObject);
        }


        public IResource getObjectResource()
        {
            IResource node = getRDFNodeOrVariable(SP.PropertyObject);
            if (!(node is IVariable))
            {
                return node;
            }
            else
            {
                return null;
            }
        }


        public IResource getSubject()
        {
            return getRDFNodeOrVariable(SP.PropertySubject);
        }


        protected IResource getRDFNodeOrVariable(INode predicate)
        {
            IResource node = getResource(predicate);
            if (node != null)
            {
                IVariable var = SPINFactory.asVariable(node);
                if (var != null)
                {
                    return var;
                }
                else
                {
                    return node;
                }
            }
            else
            {
                return null;
            }
        }


        protected void print(IResource node, IContextualSparqlPrinter p)
        {
            TupleImpl.print(getModel(), node, p);
        }


        protected void print(IResource node, IContextualSparqlPrinter p, bool abbrevRDFType)
        {
            TupleImpl.print(getModel(), node, p, abbrevRDFType);
        }


        public static void print(SpinProcessor model, IResource node, IContextualSparqlPrinter p)
        {
            print(model, node, p, false);
        }


        public static void print(SpinProcessor model, IResource node, IContextualSparqlPrinter p, bool abbrevRDFType)
        {
            // TODO find the good tests ?????
            if (!node.isLiteral())
            {
                if (abbrevRDFType && RDFUtil.sameTerm(RDF.PropertyType, node))
                {
                    p.print("a");
                }
                else
                {
                    printVarOrResource(p, node);
                }
            }
            else
            {
                //TODO INamespaceMapper pm = p.getUsePrefixes() ? model.getGraph().getPrefixMapping() : SPINExpressions.emptyPrefixMapping;
                String str = node.getSource().ToString();// TODO is this correct ? // FmtUtils.stringForNode(node, null/*TODO pm*/);
                p.print(str);
            }
        }
    }
}