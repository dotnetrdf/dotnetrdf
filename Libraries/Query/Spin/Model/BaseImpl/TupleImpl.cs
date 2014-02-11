/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Model
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


        internal void print(IResource node, ISparqlPrinter p)
        {
            TupleImpl.print(getModel(), node, p);
        }


        internal void print(IResource node, ISparqlPrinter p, bool abbrevRDFType)
        {
            TupleImpl.print(getModel(), node, p, abbrevRDFType);
        }


        public static void print(SpinProcessor model, IResource node, ISparqlPrinter p)
        {
            print(model, node, p, false);
        }


        public static void print(SpinProcessor model, IResource node, ISparqlPrinter p, bool abbrevRDFType)
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