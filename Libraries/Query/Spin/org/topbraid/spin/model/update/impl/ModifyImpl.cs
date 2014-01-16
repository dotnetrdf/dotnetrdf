/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Linq;
using VDS.RDF;
using System.Collections.Generic;
using org.topbraid.spin.util;
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Spin.Util;

namespace org.topbraid.spin.model.update.impl
{

    public class ModifyImpl : UpdateImpl, IModify
    {

        public ModifyImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {
        }


        public IEnumerable<INode> getUsing()
        {
            return listProperties(SP.PropertyUsing).Select(t => t.Object);
        }


        public IEnumerable<INode> getUsingNamed()
        {
            return listProperties(SP.PropertyUsingNamed).Select(t => t.Object);
        }


        public override void printSPINRDF(IContextualSparqlPrinter p)
        {
            printComment(p);
            printPrefixes(p);

            IResource iri = getResource(SP.PropertyGraphIRI);

            IResource with = getResource(SP.PropertyWith);
            if (with != null)
            {
                p.printIndentation(p.getIndentation());
                p.printKeyword("WITH");
                p.print(" ");
                p.printURIResource(with);
                p.println();
            }

            p.CurrentSparqlContext = SparqlContext.TemplateContext;
            // TODO add a INSERT/CONSTRUCT pattern before the delete is effective
            if (printTemplates(p, SP.PropertyDeletePattern, "DELETE", hasProperty(SP.PropertyDeletePattern), iri))
            {
                p.print("\n");
            }
            if (printTemplates(p, SP.PropertyInsertPattern, "INSERT", hasProperty(SP.PropertyInsertPattern), iri))
            {
                p.print("\n");
            }

            IEnumerable<INode> usings = getUsing();
            if (usings.Count() == 0)
            {
                usings = p.Dataset().DefaultGraphs;
            }
            foreach (IResource _using in usings)
            {
                p.printKeyword("USING");
                p.print(" ");
                p.printURIResource(_using);
                p.println();
            }

            usings = getUsingNamed();
            if (usings.Count() == 0)
            {
                usings = p.Dataset().ActiveGraphs;
            }
            foreach (IResource usingNamed in usings)
            {
                p.printKeyword("USING");
                p.print(" ");
                p.printKeyword("NAMED");
                p.print(" ");
                p.printURIResource(usingNamed);
                p.println();
            }
            p.CurrentSparqlContext = SparqlContext.QueryContext;
            printWhere(p);
        }


    }
}