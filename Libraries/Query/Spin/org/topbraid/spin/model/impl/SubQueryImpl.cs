/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.model.visitor;
using org.topbraid.spin.vocabulary;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
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


        override public void print(IContextualSparqlPrinter p)
        {
            p.print("{");
            p.println();
            IQuery query = getQuery();
            if (query != null)
            {
                p.setIndentation(p.getIndentation() + 1);
                query.print(p);
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


        override public void visit(IElementVisitor visitor)
        {
            visitor.visit(this);
        }
    }
}