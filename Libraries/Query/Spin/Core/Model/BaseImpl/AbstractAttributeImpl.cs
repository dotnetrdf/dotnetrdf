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

namespace VDS.RDF.Query.Spin.Model
{
    public abstract class AbstractAttributeImpl : AbstractSPINResource, IAbstractAttributeResource
    {
        public AbstractAttributeImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public IResource getPredicate()
        {
            IResource r = GetResource(SPL.PropertyPredicate);
            if (r != null && r.IsUri())
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
            return GetObject(SPL.PropertyValueType);
        }

        public String getComment()
        {
            return GetString(RDFS.PropertyComment);
        }

        public bool IsOptional()
        {
            return (bool)GetBoolean(SPL.PropertyOptional);
        }

        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }
    }
}