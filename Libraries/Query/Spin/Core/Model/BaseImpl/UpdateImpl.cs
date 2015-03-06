using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{

    public abstract class UpdateImpl : AbstractSPINResource, IUpdateResource
    {

        public UpdateImpl(INode node, SpinModel graph)
            : base(node, graph)
        {

        }

        public IElementListResource getWhere()
        {
            IResource whereS = this.GetResource(SP.PropertyWhere);
            if (whereS != null)
            {
                return (IElementListResource)ResourceFactory.asElement(whereS);
            }
            else
            {
                return null;
            }
        }

        public bool isSilent()
        {
            return HasProperty(SP.PropertySilent, RDFHelper.TRUE);
        }


        override public void Print(ISparqlPrinter p)
        {
            String text = GetString(SP.PropertyText);
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
            IResource graph = this.GetResource(SP.PropertyGraphIRI);
            if (graph != null)
            {
                p.printKeyword("GRAPH");
                p.print(" ");
                p.printURIResource(graph);
            }
            else if (HasProperty(SP.PropertyDefault, RDFHelper.TRUE))
            {
                p.printKeyword("DEFAULT");
            }
            else if (HasProperty(SP.PropertyNamed, RDFHelper.TRUE))
            {
                p.printKeyword("NAMED");
            }
            else if (HasProperty(SP.PropertyAll, RDFHelper.TRUE))
            {
                p.printKeyword("ALL");
            }
        }


        protected void printGraphIRIs(ISparqlPrinter p, String keyword)
        {
            List<String> graphIRIs = new List<String>();
            {
                IEnumerator<Triple> it = ListProperties(SP.PropertyGraphIRI).GetEnumerator();
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
                p.printURIResource(SpinResource.Get(RDFHelper.CreateUriNode(UriFactory.Create(graphIRI)), GetModel()));
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
            IResource resource = GetResource(predicate);
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
                    if (node.CanAs(SP.ClassNamedGraph))
                    {
                        INamedGraphResource namedGraph = (INamedGraphResource)node.As(typeof(NamedGraphImpl));
                        p.setIndentation(p.getIndentation() + 1);
                        p.setNamedBNodeMode(true);
                        namedGraph.Print(p);
                        p.setNamedBNodeMode(false);
                        p.setIndentation(p.getIndentation() - 1);
                    }
                    else
                    {
                        ITripleTemplateResource template = (ITripleTemplateResource)node.As(typeof(TripleTemplateImpl));
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