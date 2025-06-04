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

namespace VDS.RDF.Query.Spin.Model;

internal abstract class AbstractAttributeImpl : AbstractSPINResource, IAbstractAttribute
{

    public AbstractAttributeImpl(INode node, IGraph graph, SpinProcessor spinModel)
        : base(node, graph, spinModel)
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

    public bool IsOptional()
    {
        return (bool)getBoolean(SPL.PropertyOptional);
    }

    public override void Print(ISparqlPrinter p)
    {
        // TODO Auto-generated method stub
    }
}