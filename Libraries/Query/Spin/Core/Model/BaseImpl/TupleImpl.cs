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
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Model
{
    public abstract class TupleImpl : AbstractSPINResource
    {
        public TupleImpl(INode node, SpinModel spinModel)
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
            if (!(node is IVariableResource))
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
            IResource node = GetResource(predicate);
            if (node != null)
            {
                IVariableResource var = ResourceFactory.asVariable(node);
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
            TupleImpl.print(GetModel(), node, p);
        }

        internal void print(IResource node, ISparqlPrinter p, bool abbrevRDFType)
        {
            TupleImpl.print(GetModel(), node, p, abbrevRDFType);
        }

        public static void print(SpinModel model, IResource node, ISparqlPrinter p)
        {
            print(model, node, p, false);
        }

        public static void print(SpinModel model, IResource node, ISparqlPrinter p, bool abbrevRDFType)
        {
            // TODO find the good tests ?????
            if (!node.IsLiteral())
            {
                if (abbrevRDFType && RDFHelper.SameTerm(RDF.PropertyType, node))
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
                String str = node.AsNode().ToString();// TODO is this correct ? // FmtUtils.stringForNode(node, null/*TODO pm*/);
                p.print(str);
            }
        }
    }
}