/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.util;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using org.topbraid.spin.system;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{


    public abstract class AbstractSPINResource : Resource, IPrintable
    {

        /**
         * One level of indentation (four spaces), used by toString methods
         */
        public const String INDENTATION = " ";


        public AbstractSPINResource(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }

        public abstract void print(IContextualSparqlPrinter p);

        public String getComment()
        {
            return getString(RDFS.comment);
        }


        public List<IElement> getElements()
        {
            return getElements(SP.PropertyElements);
        }


        public List<IElement> getElements(INode predicate)
        {
            List<IElement> results = new List<IElement>();
            foreach (IResource node in getList(predicate))
            {
                if (node != null && !node.isLiteral())
                {
                    results.Add(SPINFactory.asElement(node));
                }
            }
            return results;
        }

        public List<IResource> getList(INode predicate)
        {
            IResource rawList = getObject(predicate);
            if (rawList != null)
            {
                return rawList.AsList();
            }
            return new List<IResource>();
        }

        private String getPrefix(Uri ns, IContextualSparqlPrinter context)
        {
            String prefix = getSource().Graph.NamespaceMap.GetPrefix(ns);
            if (prefix == null && context.getUseExtraPrefixes())
            {
                INamespaceMapper extras = ExtraPrefixes.getExtraPrefixes();
                foreach (String extraPrefix in extras.Prefixes)
                {
                    Uri extraNs = extras.GetNamespaceUri(extraPrefix);
                    if (RDFUtil.sameTerm(ns, extraNs))
                    {
                        return extraPrefix;
                    }
                }
            }
            return prefix;
        }


        public static bool hasRDFType(INode node, IGraph graph, INode type)
        {
            return graph.ContainsTriple(new Triple(node, RDF.type, type));
        }

        protected void printComment(IContextualSparqlPrinter context)
        {
            String str = getComment();
            if (str != null)
            {
                String[] rows = str.Split('\n');
                for (int i = 0; i < rows.Length; i++)
                {
                    context.print("# ");
                    context.print(rows[i]);
                    context.println();
                }
            }
        }


        protected void printNestedElementList(IContextualSparqlPrinter p)
        {
            printNestedElementList(p, SP.PropertyElements);
        }


        protected void printNestedElementList(IContextualSparqlPrinter p, INode predicate)
        {
            p.print(" {");
            p.println();
            IResource elementsRaw = getObject(predicate);
            if (elementsRaw != null)
            {
                IElementList elements = (IElementList)elementsRaw.As(typeof(ElementListImpl));
                p.setIndentation(p.getIndentation() + 1);
                elements.print(p);
                p.setIndentation(p.getIndentation() - 1);
            }
            p.printIndentation(p.getIndentation());
            p.print("}");
        }


        protected void printNestedExpressionString(IContextualSparqlPrinter context, IResource node)
        {
            printNestedExpressionString(context, node, false);
        }


        protected void printNestedExpressionString(IContextualSparqlPrinter p, IResource node, bool force)
        {
            // TODO handle namespace prefixes
            SPINExpressions.printExpressionString(p, node, true, force, new NamespaceMapper());
        }


        protected void printPrefixes(IContextualSparqlPrinter context)
        {
            if (context.getPrintPrefixes())
            {
                HashSet<IResource> uriResources = SPINUtil.getURIResources(this);
                HashSet<Uri> namespaces = new HashSet<Uri>();
                foreach (IResource uriResource in uriResources)
                {
                    // TODO gestion des prefixes
                    //String ns = uriResource.getNameSpace();
                    //namespaces.Add(ns);
                }
                INamespaceMapper prefix2Namespace = new NamespaceMapper();
                foreach (Uri ns in namespaces)
                {
                    String prefix = getPrefix(ns, context);
                    if (prefix != null)
                    {
                        prefix2Namespace.AddNamespace(prefix, ns);
                    }
                }
                List<String> prefixes = new List<String>(prefix2Namespace.Prefixes);
                prefixes.Sort();
                foreach (String prefix in prefixes)
                {
                    context.printKeyword("PREFIX");
                    context.print(" ");
                    context.print(prefix);
                    context.print(": <");
                    String ns = prefix2Namespace.GetNamespaceUri(prefix).ToString();
                    context.print(ns);
                    context.print(">");
                    context.println();
                }
            }
        }


        public String toString()
        {
            //StringSparqlPrinter p = new StringSparqlPrinter();
            //print(p);
            //return p.getString();
            return String.Empty;
        }


        public static void printVarOrResource(IContextualSparqlPrinter p, IResource resource)
        {
            IVariable variable = SPINFactory.asVariable(resource);
            if (variable != null)
            {
                variable.print(p);
            }
            else if (resource.isUri())
            {
                p.printURIResource(resource);
            }
            else if (p.isNamedBNodeMode())
            {
                // TODO is this correct ?
                p.print(resource.ToString());
            }
            else
            {
                p.print("[]");
            }
        }
    }
}