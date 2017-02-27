using VDS.RDF.Query.Spin.LibraryOntology;
/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using VDS.RDF.Query.Spin.SparqlUtil;

namespace VDS.RDF.Query.Spin.Model
{

    public class NamedGraphImpl : ElementImpl, INamedGraph
    {

        public NamedGraphImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public IResource getNameNode()
        {
            IResource r = getObject(SP.PropertyGraphNameNode);
            if (r != null)
            {
                IVariable variable = SPINFactory.asVariable(r);
                if (variable != null)
                {
                    return variable;
                }
                else
                {
                    return r;
                }
            }
            else
            {
                return null;
            }
        }


        override public void Print(ISparqlPrinter p)
        {
            p.printKeyword("GRAPH");
            p.print(" ");
            printVarOrResource(p, getNameNode());
            printNestedElementList(p);
        }

        override public void PrintEnhancedSPARQL(ISparqlPrinter p)
        {
            p.PrintEnhancedSPARQL(this);
        }

        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}
