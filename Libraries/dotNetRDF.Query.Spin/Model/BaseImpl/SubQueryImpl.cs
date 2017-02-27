using VDS.RDF.Query.Spin.LibraryOntology;
/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model
{
    public class SubQueryImpl : ElementImpl, ISubQuery
    {

        public SubQueryImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        public IQuery getQuery()
        {
            IResource r = getResource(SP.PropertyQuery);
            if (r != null)
            {
                return SPINFactory.asQuery(r);
            }
            else
            {
                return null;
            }
        }


        override public void Print(ISparqlPrinter p)
        {
            p.print("{");
            p.println();
            IQuery query = getQuery();
            if (query != null)
            {
                p.setIndentation(p.getIndentation() + 1);
                query.Print(p);
                p.setIndentation(p.getIndentation() - 1);
            }
            else
            {
                p.print("<Exception: Missing sub-query>");
            }
            p.println();
            p.printIndentation(p.getIndentation());
            p.print("}");
        }


        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}