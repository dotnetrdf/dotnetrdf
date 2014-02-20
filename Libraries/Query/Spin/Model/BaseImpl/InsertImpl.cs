/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/


using System;
using VDS.RDF;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Datasets;
namespace VDS.RDF.Query.Spin.Model
{
    [Obsolete()]
    public class InsertImpl : UpdateImpl, IInsert
    {

        public InsertImpl(INode node, SpinProcessor graph)
            :base(node, graph)
        {
            
        }


        override public void printSPINRDF(ISparqlPrinter p)
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