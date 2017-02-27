/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
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