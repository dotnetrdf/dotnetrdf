/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{

    public class AskImpl : QueryImpl, IAsk
    {

        public AskImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        override public void printSPINRDF(IContextualSparqlPrinter context)
        {
            printComment(context);
            printPrefixes(context);
            context.printIndentation(context.getIndentation());
            context.printKeyword("ASK");
            printStringFrom(context);
            context.print(" ");
            if (context.getIndentation() > 0)
            {
                // Avoid unnecessary whitespace after ASK -> put on extra row
                context.println();
            }
            printWhere(context);
            printValues(context);
        }
    }
}