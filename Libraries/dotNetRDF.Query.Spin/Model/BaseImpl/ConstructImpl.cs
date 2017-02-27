/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{
    public class ConstructImpl : QueryImpl, IConstruct
    {

        public ConstructImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public List<ITripleTemplate> getTemplates()
        {
            List<ITripleTemplate> results = new List<ITripleTemplate>();
            foreach (IResource next in getList(SP.PropertyTemplates))
            {
                if (next != null && !(next.isLiteral()))
                {
                    results.Add((ITripleTemplate)next.As(typeof(TripleTemplateImpl)));
                }
            }
            return results;
        }

        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }

        override public void printSPINRDF(ISparqlPrinter context)
        {
            printComment(context);
            printPrefixes(context);
            context.printIndentation(context.getIndentation());
            context.printKeyword("CONSTRUCT");
            context.print(" {");
            context.println();
            foreach (ITripleTemplate template in getTemplates())
            {
                context.printIndentation(context.getIndentation() + 1);
                template.Print(context);
                context.print(" .");
                context.println();
            }
            context.printIndentation(context.getIndentation());
            context.print("}");
            printStringFrom(context);
            context.println();
            printWhere(context);
            printSolutionModifiers(context);
            printValues(context);
        }
    }
}