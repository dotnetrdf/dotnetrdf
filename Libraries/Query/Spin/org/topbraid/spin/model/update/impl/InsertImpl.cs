/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/


using System;
using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Datasets;
namespace org.topbraid.spin.model.update.impl
{
    [Obsolete()]
    public class InsertImpl : UpdateImpl, IInsert
    {

        public InsertImpl(INode node, SpinProcessor graph)
            :base(node, graph)
        {
            
        }


        override public void printSPINRDF(IContextualSparqlPrinter p)
        {
            printComment(p);
            printPrefixes(p);
            p.printIndentation(p.getIndentation());
            p.printKeyword("INSERT");
            printGraphIRIs(p, "INTO");
            printTemplates(p, SP.PropertyInsertPattern, null, true, null);
            printWhere(p);
        }
    }
}