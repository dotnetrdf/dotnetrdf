/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Model
{
    public abstract class AbstractSPINResource : SpinResource, IPrintable
    {
        /**
         * One level of indentation (four spaces), used by toString methods
         */
        public const String INDENTATION = " ";

        public AbstractSPINResource(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public abstract void Print(ISparqlPrinter p);

        public String getComment()
        {
            return GetString(RDFS.PropertyComment);
        }

        public List<IElementResource> getElements()
        {
            return getElements(SP.PropertyElements);
        }

        public List<IElementResource> getElements(INode predicate)
        {
            List<IElementResource> results = new List<IElementResource>();
            foreach (IResource node in getList(predicate))
            {
                if (node != null && !node.IsLiteral())
                {
                    results.Add(ResourceFactory.asElement(node));
                }
            }
            return results;
        }

        public List<IResource> getList(INode predicate)
        {
            IResource rawList = GetObject(predicate);
            if (rawList != null)
            {
                return rawList.AsList();
            }
            return new List<IResource>();
        }

        private String getPrefix(Uri ns, ISparqlPrinter context)
        {
            String prefix = AsNode().Graph.NamespaceMap.GetPrefix(ns);
            if (prefix == null && context.getUseExtraPrefixes())
            {
                INamespaceMapper extras = ExtraPrefixes.getExtraPrefixes();
                foreach (String extraPrefix in extras.Prefixes)
                {
                    Uri extraNs = extras.GetNamespaceUri(extraPrefix);
                    if (RDFHelper.SameTerm(ns, extraNs))
                    {
                        return extraPrefix;
                    }
                }
            }
            return prefix;
        }

        public static bool hasRDFType(INode node, IGraph graph, INode type)
        {
            return graph.ContainsTriple(new Triple(node, RDF.PropertyType, type));
        }

        protected void printComment(ISparqlPrinter context)
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

        protected void printNestedElementList(ISparqlPrinter p)
        {
            printNestedElementList(p, SP.PropertyElements);
        }

        protected void printNestedElementList(ISparqlPrinter p, INode predicate)
        {
            p.print(" {");
            p.println();
            IResource elementsRaw = GetObject(predicate);
            if (elementsRaw != null)
            {
                IElementListResource elements = (IElementListResource)elementsRaw.As(typeof(ElementListImpl));
                p.setIndentation(p.getIndentation() + 1);
                elements.Print(p);
                p.setIndentation(p.getIndentation() - 1);
            }
            p.printIndentation(p.getIndentation());
            p.print("}");
        }

        protected void printNestedExpressionString(ISparqlPrinter context, IResource node)
        {
            printNestedExpressionString(context, node, false);
        }

        protected void printNestedExpressionString(ISparqlPrinter p, IResource node, bool force)
        {
            // TODO handle namespace prefixes
            //SPINExpressions.printExpressionString(p, node, true, force, new NamespaceMapper());
        }

        protected void printPrefixes(ISparqlPrinter context)
        {
            return;
            //if (context.getPrintPrefixes())
            //{
            //    HashSet<IResource> uriResources = SPINUtil.getURIResources(this);
            //    HashSet<Uri> namespaces = new HashSet<Uri>();
            //    foreach (IResource uriResource in uriResources)
            //    {
            //        // TODO gestion des prefixes
            //        //String ns = uriResource.getNameSpace();
            //        //namespaces.Add(ns);
            //    }
            //    INamespaceMapper prefix2Namespace = new NamespaceMapper();
            //    foreach (Uri ns in namespaces)
            //    {
            //        String prefix = getPrefix(ns, context);
            //        if (prefix != null)
            //        {
            //            prefix2Namespace.AddNamespace(prefix, ns);
            //        }
            //    }
            //    List<String> prefixes = new List<String>(prefix2Namespace.Prefixes);
            //    prefixes.Sort();
            //    foreach (String prefix in prefixes)
            //    {
            //        context.printKeyword("PREFIX");
            //        context.print(" ");
            //        context.print(prefix);
            //        context.print(": <");
            //        String ns = prefix2Namespace.GetNamespaceUri(prefix).ToString();
            //        context.print(ns);
            //        context.print(">");
            //        context.println();
            //    }
            //}
        }

        public String toString()
        {
            //StringSparqlPrinter p = new StringSparqlPrinter();
            //print(p);
            //return p.getString();
            return String.Empty;
        }

        public static void printVarOrResource(ISparqlPrinter p, IResource resource)
        {
            IVariableResource variable = ResourceFactory.asVariable(resource);
            if (variable != null)
            {
                variable.Print(p);
            }
            else if (resource.IsUri())
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