/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public class AggregationImpl : AbstractSPINResource, IAggregationResource
    {

        public AggregationImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }


        public IVariableResource getAs()
        {
            IResource node = this.GetObject(SP.PropertyAs);
            if (node != null)
            {
                return ResourceFactory.asVariable(node);
            }
            else
            {
                return null;
            }
        }


        public INode getExpression()
        {
            return GetObject(SP.PropertyExpression);
        }


        public bool isDistinct()
        {
            return HasProperty(SP.PropertyDistinct, RDFHelper.TRUE);
        }

        override public void Print(ISparqlPrinter p)
        {

            IVariableResource asVar = getAs();
            if (asVar != null)
            {
                p.print("(");
            }

            INode aggType = this.GetObject(RDF.PropertyType);
            String aggName = Aggregations.getName(aggType);
            p.printKeyword(aggName);
            p.print("(");

            if (isDistinct())
            {
                p.print("DISTINCT ");
            }

            Triple exprS = this.GetProperty(SP.PropertyExpression);
            if (exprS != null && !(exprS.Object is ILiteralNode))
            {
                IResource r = SpinResource.Get(exprS.Object, GetModel());
                IResource expr = ResourceFactory.asExpression(r);
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
            String separator = GetString(SP.PropertySeparator);
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