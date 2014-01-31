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

    public class DeleteImpl : UpdateImpl, IDelete
    {

        public DeleteImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {
            
        }


        public override void printSPINRDF(ISparqlFactory p)
        {
            printComment(p);
            printPrefixes(p);
            p.printIndentation(p.getIndentation());
            p.printKeyword("DELETE");
            printGraphIRIs(p, "FROM");
            printTemplates(p, SP.PropertyDeletePattern, null, true, null);
            printWhere(p);
        }
    }
}