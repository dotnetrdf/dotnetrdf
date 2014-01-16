/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Constraints;
using org.topbraid.spin.model;
using org.topbraid.spin.model.visitor;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin.Util;

namespace org.topbraid.spin.util
{


    /**
     * A utility that can be used to find all SimplePropertyPaths encoded in a
     * SPIN element where either subject or object is ?this.
     * 
     * @author Holger Knublauch
     */
    public class PropertyPathsGetter : AbstractTriplesVisitor
    {

        private INode localThis;

        private HashSet<SimplePropertyPath> results = new HashSet<SimplePropertyPath>();

        private IGraph targetModel;


        public PropertyPathsGetter(IElement element, Dictionary<IResource, IResource> initialBindings)
            : base(element, initialBindings)
        {
            this.targetModel = element.Graph;
            this.localThis = SPIN._this;
        }


        public HashSet<SimplePropertyPath> getResults()
        {
            return results;
        }


        override protected void handleTriplePattern(ITriplePattern triplePattern, Dictionary<IResource, IResource> bindings)
        {
            if (RDFUtil.sameTerm(SPIN._this, triplePattern.getSubject()))
            {
                IResource predicate = triplePattern.getPredicate();
                if (predicate != null && predicate.isUri())
                {
                    IVariable variable = SPINFactory.asVariable(predicate);
                    if (variable == null)
                    {
                        results.Add(new ObjectPropertyPath(localThis, predicate));
                    }
                    else if (bindings != null)
                    {
                        String varName = variable.getName();
                        IResource argProperty = Resource.Get(RDFUtil.CreateUriNode(UriFactory.Create(SP.NS_URI + varName)), triplePattern.getModel());
                        if (bindings.ContainsKey(argProperty))
                        {
                            INode b = bindings[argProperty];
                            if (b != null && b is IUriNode)
                            {
                                results.Add(new ObjectPropertyPath(localThis, b));
                            }
                        }
                    }
                }
            }
            if (RDFUtil.sameTerm(SPIN._this, triplePattern.getObject()))
            {
                IResource predicate = triplePattern.getPredicate();
                if (predicate != null && predicate.isUri())
                {
                    IVariable variable = SPINFactory.asVariable(predicate);
                    if (variable == null)
                    {
                        results.Add(new SubjectPropertyPath(localThis, predicate));
                    }
                    else if (bindings != null)
                    {
                        String varName = variable.getName();
                        IResource argProperty = Resource.Get(RDFUtil.CreateUriNode(UriFactory.Create(SP.NS_URI + varName)), triplePattern.getModel());
                        if (bindings.ContainsKey(argProperty))
                        {
                            INode b = bindings[argProperty];
                            if (b != null && b is IUriNode)
                            {
                                results.Add(new SubjectPropertyPath(localThis, b));
                            }
                        }
                    }
                }
            }
        }
    }
}