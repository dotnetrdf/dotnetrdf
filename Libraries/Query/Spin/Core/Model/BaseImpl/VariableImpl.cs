/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Spin.Model.IO;

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