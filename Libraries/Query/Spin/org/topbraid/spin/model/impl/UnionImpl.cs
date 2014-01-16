/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using org.topbraid.spin.model.visitor;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace org.topbraid.spin.model.impl
{


    public class UnionImpl : ElementImpl, IUnion
    {

        public UnionImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        override public void print(IContextualSparqlPrinter p)
        {
            List<IElement> elements = getElements();
            for (IEnumerator<IElement> it = elements.GetEnumerator(); it.MoveNext(); )
            {
                IElement element = it.Current;
                p.print("{");
                p.println();
                p.setIndentation(p.getIndentation() + 1);
                element.print(p);
                p.setIndentation(p.getIndentation() - 1);
                p.printIndentation(p.getIndentation());
                p.print("}");
                if (it.MoveNext())
                {
                    p.println();
                    p.printIndentation(p.getIndentation());
                    p.printKeyword("UNION");
                    p.println();
                    p.printIndentation(p.getIndentation());
                }
            }
        }


        override public void visit(IElementVisitor visitor)
        {
            visitor.visit(this);
        }
    }
}