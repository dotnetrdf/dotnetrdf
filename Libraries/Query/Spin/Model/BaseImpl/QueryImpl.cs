/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Linq;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{


    public abstract class QueryImpl : AbstractSPINResource, ISolutionModifierQuery
    {


        public QueryImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        public List<IResource> getFrom()
        {
            return getList(SP.PropertyFrom);
        }


        public List<IResource> getFromNamed()
        {
            return getList(SP.PropertyFromNamed);
        }


        public long? getLimit()
        {
            return getLong(SP.PropertyLimit);
        }


        public long? getOffset()
        {
            return getLong(SP.PropertyOffset);
        }


        private List<String> getStringList(INode predicate)
        {
            List<String> results = new List<String>();
            IEnumerator<Triple> it = this.listProperties(predicate).GetEnumerator();
            while (it.MoveNext())
            {
                INode node = it.Current.Object;
                if (node is IValuedNode)
                {
                    results.Add(((IValuedNode)node).AsString());
                }
                else if (node is IUriNode)
                {
                    results.Add(((IUriNode)node).Uri.ToString());
                }
            }
            return results;
        }


        public IValues getValues()
        {
            IResource values = getResource(SP.PropertyValues);
            if (values != null)
            {
                return (IValues)values.As(typeof(ValuesImpl));
            }
            else
            {
                return null;
            }
        }


        public IElementList getWhere()
        {
            IResource whereS = getResource(SP.PropertyWhere);
            if (whereS != null)
            {
                IElement element = SPINFactory.asElement(whereS);
                return (IElementList)element;
            }
            else
            {
                return null;
            }
        }


        public List<IElement> getWhereElements()
        {
            return getElements(SP.PropertyWhere);
        }


        override public void Print(ISparqlPrinter p)
        {
            String text = getString(SP.PropertyText);
            if (text != null)
            {
                if (p.hasInitialBindings())
                {
                    throw new ArgumentException("Queries that only have an sp:text cannot be converted to a query string if initial bindings are present.");
                }
                else
                {
                    p.print(text);
                }
            }
            else
            {
                printSPINRDF(p);
            }
        }


        public abstract void printSPINRDF(ISparqlPrinter p);


        protected void printStringFrom(ISparqlPrinter context)
        {
            IEnumerable<INode> froms = getFrom();
            if (froms.Count() == 0)
            {
                froms = context.Dataset.DefaultGraphs;
            }
            foreach (IResource from in froms)
            {
                context.println();
                context.printKeyword("FROM");
                context.printURIResource(from);
            }
            froms = getFromNamed();
            if (froms.Count() == 0)
            {
                froms = context.Dataset.DefaultGraphs;
            }
            foreach (IResource fromNamed in froms)
            {
                context.println();
                context.printKeyword("FROM NAMED");
                context.printURIResource(fromNamed);
            }
        }


        protected void printSolutionModifiers(ISparqlPrinter context)
        {
            List<IResource> orderBy = getList(SP.PropertyOrderBy);
            if (orderBy.Count > 0)
            {
                context.println();
                context.printIndentation(context.getIndentation());
                context.printKeyword("ORDER BY");
                foreach (IResource node in orderBy)
                {
                    if (!node.isLiteral())
                    {
                        if (node.hasProperty(RDF.PropertyType, SP.ClassAsc))
                        {
                            context.print(" ");
                            context.printKeyword("ASC");
                            context.print(" ");
                            IResource expression = node.getResource(SP.PropertyExpression);
                            printOrderByExpression(context, expression);
                        }
                        else if (node.hasProperty(RDF.PropertyType, SP.ClassDesc))
                        {
                            context.print(" ");
                            context.printKeyword("DESC");
                            context.print(" ");
                            IResource expression = node.getResource(SP.PropertyExpression);
                            printOrderByExpression(context, expression);
                        }
                        else
                        {
                            context.print(" ");
                            printOrderByExpression(context, node);
                        }
                    }
                }
            }
            long? limit = getLimit();
            if (limit != null)
            {
                context.println();
                context.printIndentation(context.getIndentation());
                context.printKeyword("LIMIT");
                context.print(" " + limit);
            }
            long? offset = getOffset();
            if (offset != null)
            {
                context.println();
                context.printIndentation(context.getIndentation());
                context.print("OFFSET");
                context.print(" " + offset);
            }
        }


        private void printOrderByExpression(ISparqlPrinter sb, IResource node)
        {
            // TODO check for real test
            if (node is INode)
            {
                IResource resource = (IResource)node;
                IFunctionCall call = SPINFactory.asFunctionCall(resource);
                if (call != null)
                {
                    sb.print("(");
                    ISparqlPrinter pc = sb.clone();
                    pc.setNested(true);
                    call.Print(pc);
                    sb.print(")");
                    return;
                }
            }

            printNestedExpressionString(sb, node, true);
        }


        protected void printValues(ISparqlPrinter p)
        {
            IValues values = getValues();
            if (values != null)
            {
                p.println();
                values.Print(p);
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