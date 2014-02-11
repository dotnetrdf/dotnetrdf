/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{

    public class AggregationImpl : AbstractSPINResource, IAggregation
    {

        public AggregationImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public IVariable getAs()
        {
            IResource node = this.getObject(SP.PropertyAs);
            if (node != null)
            {
                return SPINFactory.asVariable(node);
            }
            else
            {
                return null;
            }
        }


        public INode getExpression()
        {
            return getObject(SP.PropertyExpression);
        }


        public bool isDistinct()
        {
            return hasProperty(SP.PropertyDistinct, RDFUtil.TRUE);
        }

        override public void Print(ISparqlPrinter p)
        {

            IVariable asVar = getAs();
            if (asVar != null)
            {
                p.print("(");
            }

            INode aggType = this.getObject(RDF.PropertyType);
            String aggName = Aggregations.getName(aggType);
            p.printKeyword(aggName);
            p.print("(");

            if (isDistinct())
            {
                p.print("DISTINCT ");
            }

            Triple exprS = this.getProperty(SP.PropertyExpression);
            if (exprS != null && !(exprS.Object is ILiteralNode))
            {
                IResource r = Resource.Get(exprS.Object, getModel());
                IResource expr = SPINFactory.asExpression(r);
                if (expr is IPrintable)
                {
                    ((IPrintable)expr).Print(p);
                }
                else
                {
                    p.printURIResource(r);
                }
            }
            else
            {
                p.print("*");
            }
            String separator = getString(SP.PropertySeparator);
            if (separator != null)
            {
                p.print("; ");
                p.printKeyword("SEPARATOR");
                p.print("=''"); //"='" + DatasetUtil.escapeString(separator) + "'");
            }
            if (asVar != null)
            {
                p.print(") ");
                p.printKeyword("AS");
                p.print(" ");
                p.print(asVar.ToString());
            }
            p.print(")");
        }
    }
}