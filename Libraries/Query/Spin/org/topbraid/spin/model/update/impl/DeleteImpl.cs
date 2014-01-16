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

    public class DeleteImpl : UpdateImpl, IDelete
    {

        public DeleteImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {
            
        }


        public override void printSPINRDF(IContextualSparqlPrinter p)
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