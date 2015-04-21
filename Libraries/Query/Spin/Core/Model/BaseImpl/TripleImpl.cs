using VDS.RDF.Query.Spin.Model.IO;

/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public abstract class TripleImpl : TupleImpl
    {
        public TripleImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public IResource getPredicate()
        {
            return GetResource(SP.PropertyPredicate);
        }

        override public void Print(ISparqlPrinter p)
        {
            print(getSubject(), p);
            p.print(" ");
            print(getPredicate(), p, true);
            p.print(" ");
            print(getObject(), p);
        }
    }
}