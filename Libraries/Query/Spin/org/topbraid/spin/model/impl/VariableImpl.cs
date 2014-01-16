/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{


    public class VariableImpl : AbstractSPINResource, IVariable
    {

        public VariableImpl(INode node, SpinProcessor spinModel)
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


        override public void print(IContextualSparqlPrinter p)
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