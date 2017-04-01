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
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.Model.visitor;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Core
{


    /**
     * A utility that can be used to find all properties that occur as object
     * in a triple pattern with ?this as subject.  The system also walks into
     * calls to SPIN Functions such as spl:cardinality and SPIN Templates.
     * 
     * @author Holger Knublauch
     */
    public class ObjectPropertiesGetter : AbstractTriplesVisitor
    {

        private HashSet<IResource> properties = new HashSet<IResource>();

        private SpinProcessor _targetModel;


        public ObjectPropertiesGetter(SpinProcessor targetModel, IElement element, Dictionary<IResource, IResource> initialBindings)
            : base(element, initialBindings)
        {

            this._targetModel = targetModel;
        }


        public HashSet<IResource> getResults()
        {
            return properties;
        }


        override protected void handleTriplePattern(ITriplePattern triplePattern, Dictionary<IResource, IResource> bindings)
        {
            bool valid = false;
            IResource subject = triplePattern.getSubject();
            if (RDFUtil.sameTerm(SPIN.Property_this, subject))
            {
                valid = true;
            }
            else if (bindings != null)
            {
                IVariable var = SPINFactory.asVariable(subject);
                if (var != null)
                {
                    String varName = var.getName();
                    foreach (IResource argPredicate in bindings.Keys)
                    {
                        if (varName.Equals(argPredicate.Uri().ToString().Replace(SP.BASE_URI, "").Replace(SPIN.BASE_URI, "")))
                        {
                            IResource b = bindings[argPredicate];
                            if (RDFUtil.sameTerm(SPIN.Property_this, b))
                            {
                                valid = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (valid)
            {
                IResource predicate = triplePattern.getPredicate();
                if (predicate != null)
                {
                    IVariable variable = SPINFactory.asVariable(predicate);
                    if (variable == null)
                    {
                        Uri uri = predicate.Uri();
                        if (uri != null)
                        {
                            properties.Add(Resource.Get(predicate, _targetModel));
                        }
                    }
                    else if (bindings != null)
                    {
                        String varName = variable.getName();
                        foreach (IResource argPredicate in bindings.Keys)
                        {
                            if (varName.Equals(argPredicate.Uri().ToString().Replace(SP.BASE_URI, "").Replace(SPIN.BASE_URI, "")))
                            {
                                IResource b = bindings[argPredicate];
                                if (b != null && b.isUri())
                                {
                                    properties.Add(Resource.Get(b, _targetModel));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}