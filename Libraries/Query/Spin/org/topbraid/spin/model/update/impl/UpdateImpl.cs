/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using org.topbraid.spin.model.impl;
using VDS.RDF;

using org.topbraid.spin.util;
using System;

using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.vocabulary;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;
namespace org.topbraid.spin.model.update.impl
{

    public abstract class UpdateImpl : AbstractSPINResource, IUpdate
    {

        public UpdateImpl(INode node, SpinProcessor graph)
            : base(node, graph)
        {

        }

        public IElementList getWhere()
        {
            IResource whereS = this.getResource(SP.PropertyWhere);
            if (whereS != null)
            {
                return (IElementList)SPINFactory.asElement(whereS);
            }
            else
            {
                return null;
            }
        }

        public bool isSilent()
        {
            return hasProperty(SP.PropertySilent, RDFUtil.TRUE);
        }


        override public void print(IContextualSparqlPrinter p)
        {
            String text = getString(SP.PropertyText);
            if (text != null)
            {
                p.print(text);
            }
            else
            {
                printSPINRDF(p);
            }
        }


        public abstract void printSPINRDF(IContextualSparqlPrinter p);


        protected void printGraphDefaultNamedOrAll(IContextualSparqlPrinter p)
        {
            IResource graph = this.getResource(SP.PropertyGraphIRI);
            if (graph != null)
            {
                p.printKeyword("GRAPH");
                p.print(" ");
                p.printURIResource(graph);
            }
            else if (hasProperty(SP.PropertyDefault, RDFUtil.TRUE))
            {
                p.printKeyword("DEFAULT");
            }
            else if (hasProperty(SP.PropertyNamed, RDFUtil.TRUE))
            {
                p.printKeyword("NAMED");
            }
            else if (hasProperty(SP.PropertyAll, RDFUtil.TRUE))
            {
                p.printKeyword("ALL");
            }
        }


        protected void printGraphIRIs(IContextualSparqlPrinter p, String keyword)
        {
            List<String> graphIRIs = new List<String>();
            {
                IEnumerator<Triple> it = listProperties(SP.PropertyGraphIRI).GetEnumerator();
                while (it.MoveNext())
                {
                    Triple s = it.Current;
                    if (s.Object is IUriNode)
                    {
                        graphIRIs.Add(((IUriNode)s.Object).Uri.ToString());
                    }
                }
                graphIRIs.Sort();
            }
            foreach (String graphIRI in graphIRIs)
            {
                p.print(" ");
                if (keyword != null)
                {
                    p.printKeyword(keyword);
                    p.print(" ");
                }
                p.printURIResource(Resource.Get(RDFUtil.CreateUriNode(UriFactory.Create(graphIRI)), getModel()));
            }
        }


        protected void printSilent(IContextualSparqlPrinter p)
        {
            if (isSilent())
            {
                p.printKeyword("SILENT");
                p.print(" ");
            }
        }


        protected bool printTemplates(IContextualSparqlPrinter p, INode predicate, String keyword, bool force, IResource graphIRI)
        {
            IResource resource = getResource(predicate);
            if (resource == null) return false;
            List<IResource> nodes = resource.AsList();
            if (nodes.Count > 0 || force)
            {
                if (keyword != null)
                {
                    p.printIndentation(p.getIndentation());
                    p.printKeyword(keyword);
                }
                p.print(" {");
                p.println();
                if (graphIRI != null)
                { // Legacy triple
                    p.setIndentation(p.getIndentation() + 1);
                    p.printIndentation(p.getIndentation());
                    p.printKeyword("GRAPH");
                    p.print(" ");
                    printVarOrResource(p, graphIRI);
                    p.print(" {");
                    p.println();
                }
                foreach (IResource node in nodes)
                {
                    p.printIndentation(p.getIndentation() + 1);
                    if (node.canAs(SP.ClassNamedGraph))
                    {
                        INamedGraph namedGraph = (INamedGraph)node.As(typeof(NamedGraphImpl));
                        p.setIndentation(p.getIndentation() + 1);
                        p.setNamedBNodeMode(true);
                        namedGraph.print(p);
                        p.setNamedBNodeMode(false);
                        p.setIndentation(p.getIndentation() - 1);
                    }
                    else
                    {
                        ITripleTemplate template = (ITripleTemplate)node.As(typeof(TripleTemplateImpl));
                        template.print(p);
                    }
                    p.print(" .");
                    p.println();
                }
                if (graphIRI != null)
                {
                    p.printIndentation(p.getIndentation());
                    p.setIndentation(p.getIndentation() - 1);
                    p.print("}");
                    p.println();
                }
                p.printIndentation(p.getIndentation());
                p.print("}");
                return true;
            }
            else
            {
                return false;
            }
        }


        protected void printWhere(IContextualSparqlPrinter p)
        {
            p.printIndentation(p.getIndentation());
            p.printKeyword("WHERE");
            printNestedElementList(p, SP.PropertyWhere);
        }
    }
}