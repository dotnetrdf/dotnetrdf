/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Model
{
    internal abstract class TupleImpl : AbstractSPINResource
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