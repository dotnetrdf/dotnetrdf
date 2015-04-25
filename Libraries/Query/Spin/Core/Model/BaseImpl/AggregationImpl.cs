/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
/*
 
A C# port of the SPIN API (http://topbraid.org/spin/api/)
an open source Java API distributed by TopQuadrant to encourage the adoption of SPIN in the community. The SPIN API is built on the Apache Jena API and provides the following features: 
 
-----------------------------------------------------------------------------

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;

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