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
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Model
{
    internal abstract class UpdateImpl : AbstractSPINResource, IUpdate
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


        override public void Print(ISparqlPrinter p)
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


        public abstract void printSPINRDF(ISparqlPrinter p);


        protected void printGraphDefaultNamedOrAll(ISparqlPrinter p)
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


        protected void printGraphIRIs(ISparqlPrinter p, String keyword)
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


        protected void printSilent(ISparqlPrinter p)
        {
            if (isSilent())
            {
                p.printKeyword("SILENT");
                p.print(" ");
            }
        }


        protected bool printTemplates(ISparqlPrinter p, INode predicate, String keyword, bool force, IResource graphIRI)
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
                        namedGraph.Print(p);
                        p.setNamedBNodeMode(false);
                        p.setIndentation(p.getIndentation() - 1);
                    }
                    else
                    {
                        ITripleTemplate template = (ITripleTemplate)node.As(typeof(TripleTemplateImpl));
                        template.Print(p);
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


        protected void printWhere(ISparqlPrinter p)
        {
            p.printIndentation(p.getIndentation());
            p.printKeyword("WHERE");
            printNestedElementList(p, SP.PropertyWhere);
        }
    }
}