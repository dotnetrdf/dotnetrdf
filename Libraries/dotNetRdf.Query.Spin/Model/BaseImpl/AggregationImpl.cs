/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Model;

internal class AggregationImpl : AbstractSPINResource, IAggregation
{

    public AggregationImpl(INode node, IGraph graph, SpinProcessor spinModel)
        : base(node, graph, spinModel)
    {
    }


    public IVariable getAs()
    {
        IResource node = getObject(SP.PropertyAs);
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

        INode aggType = getObject(RDF.PropertyType);
        var aggName = Aggregations.getName(aggType);
        p.printKeyword(aggName);
        p.print("(");

        if (isDistinct())
        {
            p.print("DISTINCT ");
        }

        Triple exprS = getProperty(SP.PropertyExpression);
        if (exprS != null && !(exprS.Object is ILiteralNode))
        {
            IResource r = Resource.Get(exprS.Object, Graph, getModel());
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
        var separator = getString(SP.PropertySeparator);
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