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
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Model
{
    public class VariableImpl : AbstractSPINResource, IVariableResource
    {
        public VariableImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        private void addTriplePatterns(INode predicate, HashSet<ITriplePatternResource> results)
        {
            IEnumerator<Triple> it = GetModel().GetTriplesWithPredicateObject(predicate, this).GetEnumerator();
            while (it.MoveNext())
            {
                IResource subject = SpinResource.Get(it.Current.Subject, GetModel());
                results.Add((TriplePatternImpl)subject.As(typeof(TriplePatternImpl)));
            }
        }

        public String getName()
        {
            return GetString(SP.PropertyVarName);
        }

        public HashSet<ITriplePatternResource> getTriplePatterns()
        {
            HashSet<ITriplePatternResource> results = new HashSet<ITriplePatternResource>();
            addTriplePatterns(SP.PropertySubject, results);
            addTriplePatterns(SP.PropertyPredicate, results);
            addTriplePatterns(SP.PropertyObject, results);
            return results;
        }

        public bool isBlankNodeVar()
        {
            String name = getName();
            if (name != null)
            {
                return name.StartsWith("?");
            }
            else
            {
                return false;
            }
        }

        override public void Print(ISparqlPrinter p)
        {
            String name = getName();
            if (name.StartsWith("?"))
            {
                p.print("_:");
                p.print(name.Substring(1));
            }
            else
            {
                p.printVariable(name);
            }
        }
    }
}