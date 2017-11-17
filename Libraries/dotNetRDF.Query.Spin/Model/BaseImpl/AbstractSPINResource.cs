/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Model
{
    internal abstract class AbstractSPINResource : Resource, IPrintable
    {

        /**
         * One level of indentation (four spaces), used by toString methods
         */
        public const String INDENTATION = " ";


        public AbstractSPINResource(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }

        public abstract void Print(ISparqlPrinter p);

        public virtual void PrintEnhancedSPARQL(ISparqlPrinter p) {
            Print(p);
        }

        public String getComment()
        {
            return getString(RDFS.PropertyComment);
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

        private String getPrefix(Uri ns, ISparqlPrinter context)
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
            IResource elementsRaw = getObject(predicate);
            if (elementsRaw != null)
            {
                IElementList elements = (IElementList)elementsRaw.As(typeof(ElementListImpl));
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
            IVariable variable = SPINFactory.asVariable(resource);
            if (variable != null)
            {
                variable.Print(p);
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