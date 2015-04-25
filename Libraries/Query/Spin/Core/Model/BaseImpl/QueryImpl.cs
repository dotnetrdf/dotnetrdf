/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/
/*
 
A C# port of the SPIN API (http://topbraid.org/spin/api/)
an open source Java API distributed by TopQuadrant to encourage the adoption of SPIN in the community. The SPIN API is built on the Apache Jena API and provides the following features: 
 
-----------------------------------------------------------------------------

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public abstract class QueryImpl : AbstractSPINResource, ISolutionModifierQueryResource
    {
        public QueryImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public IEnumerable<Uri> getFrom()
        {
            return getList(SP.PropertyFrom).Select(r => r.Uri);
        }

        public IEnumerable<Uri> getFromNamed()
        {
            return getList(SP.PropertyFromNamed).Select(r => r.Uri);
        }

        public long? getLimit()
        {
            return GetLong(SP.PropertyLimit);
        }

        public long? getOffset()
        {
            return GetLong(SP.PropertyOffset);
        }

        private List<String> getStringList(INode predicate)
        {
            List<String> results = new List<String>();
            IEnumerator<Triple> it = this.ListProperties(predicate).GetEnumerator();
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

        public IValuesResource getValues()
        {
            IResource values = GetResource(SP.PropertyValues);
            if (values != null)
            {
                return (IValuesResource)values.As(typeof(ValuesImpl));
            }
            else
            {
                return null;
            }
        }

        public IElementListResource getWhere()
        {
            IResource whereS = GetResource(SP.PropertyWhere);
            if (whereS != null)
            {
                IElementResource element = ResourceFactory.asElement(whereS);
                return (IElementListResource)element;
            }
            else
            {
                return null;
            }
        }

        public List<IElementResource> getWhereElements()
        {
            return getElements(SP.PropertyWhere);
        }

        override public void Print(ISparqlPrinter p)
        {
            String text = GetString(SP.PropertyText);
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
            foreach (Uri from in getFrom())
            {
                context.println();
                context.printKeyword("FROM");
                context.print(" <");
                context.print(from.ToString());
                context.print(">");
            }
            foreach (Uri fromNamed in getFromNamed())
            {
                context.println();
                context.printKeyword("FROM NAMED");
                context.print(" <");
                context.print(fromNamed.ToString());
                context.print(">");
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
                    if (!node.IsLiteral())
                    {
                        if (node.HasProperty(RDF.PropertyType, SP.ClassAsc))
                        {
                            context.print(" ");
                            context.printKeyword("ASC");
                            context.print(" ");
                            IResource expression = node.GetResource(SP.PropertyExpression);
                            printOrderByExpression(context, expression);
                        }
                        else if (node.HasProperty(RDF.PropertyType, SP.ClassDesc))
                        {
                            context.print(" ");
                            context.printKeyword("DESC");
                            context.print(" ");
                            IResource expression = node.GetResource(SP.PropertyExpression);
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
                IFunctionCallResource call = ResourceFactory.asFunctionCall(resource);
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
            IValuesResource values = getValues();
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