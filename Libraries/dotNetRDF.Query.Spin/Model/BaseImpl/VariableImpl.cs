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
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{
    internal class VariableImpl : AbstractSPINResource, IVariable
    {
        internal VariableImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        private void addTriplePatterns(INode predicate, HashSet<ITriplePattern> results)
        {
            IEnumerator<Triple> it = getModel().GetTriplesWithPredicateObject(predicate, this).GetEnumerator();
            while (it.MoveNext())
            {
                IResource subject = Resource.Get(it.Current.Subject, getModel());
                results.Add((TriplePatternImpl)subject.As(typeof(TriplePatternImpl)));
            }
        }


        public String getName()
        {
            return getString(SP.PropertyVarName);
        }


        public HashSet<ITriplePattern> getTriplePatterns()
        {
            HashSet<ITriplePattern> results = new HashSet<ITriplePattern>();
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