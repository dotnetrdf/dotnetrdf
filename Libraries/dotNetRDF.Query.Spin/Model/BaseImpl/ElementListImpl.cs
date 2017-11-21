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
/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System.Collections.Generic;
using System.Text;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Model
{
    internal class ElementListImpl : ElementImpl, IElementList
    {

        public ElementListImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        private int addListMembers(List<IElement> elements, int i, List<IResource> members)
        {
            bool first = true;
            while (i < elements.Count - 1 &&
                    elements[i] is ITriplePattern &&
                    elements[i + 1] is ITriplePattern)
            {
                ITriplePattern firstPattern = (ITriplePattern)elements[i];
                ITriplePattern secondPattern = (ITriplePattern)elements[i + 1];
                if (RDFUtil.sameTerm(RDF.PropertyFirst,firstPattern.getPredicate()) && RDFUtil.sameTerm(RDF.PropertyRest,secondPattern.getPredicate()))
                {
                    IResource firstSubject = firstPattern.getSubject();
                    IResource secondSubject = secondPattern.getSubject();
                    if (firstSubject is IVariable && secondSubject is IVariable)
                    {
                        IVariable firstVar = (IVariable)firstSubject;
                        IVariable secondVar = (IVariable)secondSubject;
                        if (firstVar.isBlankNodeVar() && firstVar.getName().Equals(secondVar.getName()))
                        {
                            members.Add(firstPattern.getObject());
                            IResource secondObject = secondPattern.getObject();
                            i++;
                            if (RDFUtil.sameTerm(RDF.Nil, secondObject))
                            {
                                return i + 1;
                            }
                        }
                    }
                }

                // We are not in a valid list
                if (first && members.Count == 0)
                {
                    break;
                }
                first = false;
                i++;
            }
            return i;
        }


        public new List<IElement> getElements()
        {
            List<IElement> results = new List<IElement>();
            IEnumerator<IResource> it = AsList().GetEnumerator();
            while (it.MoveNext())
            {
                IResource node = it.Current;
                if (!(node.isLiteral()))
                {
                    IElement element = SPINFactory.asElement(node);
                    if (element != null)
                    {
                        results.Add(element);
                    }
                }
            }
            return results;
        }


        private bool nextIsMatchingVarPattern(ITriplePattern main, List<IElement> elements, int i)
        {
            if (main.getObject() is IVariable &&
                    i < elements.Count - 2 &&
                    elements[i + 1] is ITriplePattern &&
                    elements[i + 2] is ITriplePattern)
            {
                IVariable mainVar = (IVariable)main.getObject();
                if (mainVar.isBlankNodeVar())
                {
                    ITriplePattern nextPattern = (ITriplePattern)elements[i + 1];
                    ITriplePattern lastPattern = (ITriplePattern)elements[i + 2];
                    IResource nextSubject = nextPattern.getSubject();
                    IResource lastSubject = lastPattern.getSubject();
                    if (nextSubject is IVariable &&
                       lastSubject is IVariable &&
                            RDFUtil.sameTerm(RDF.PropertyFirst, nextPattern.getPredicate()) &&
                            RDFUtil.sameTerm(RDF.PropertyRest, lastPattern.getPredicate()))
                    {
                        IVariable nextVar = (IVariable)nextSubject;
                        if (mainVar.getName().Equals(nextVar.getName()))
                        {
                            IVariable lastVar = (IVariable)lastSubject;
                            return mainVar.getName().Equals(lastVar.getName());
                        }
                    }
                }
            }
            return false;
        }


        override public void Print(ISparqlPrinter p)
        {
            List<IElement> elements = getElements();

            int oldI = -1;
            for (int i = 0; i < elements.Count; i++)
            {
                if (i == oldI)
                {
                    break; // Prevent unknown endless loop conditions
                }
                oldI = i;
                IElement element = elements[i];
                p.printIndentation(p.getIndentation());
                if (element is IElementList && ((IElementList)element).getElements().Count > 1)
                {
                    p.print("{");
                    p.println();
                    p.setIndentation(p.getIndentation() + 1);
                    element.Print(p);
                    p.setIndentation(p.getIndentation() - 1);
                    p.printIndentation(p.getIndentation());
                    p.print("} ");
                }
                else
                {
                    if (element is ITriplePattern)
                    {
                        i = printTriplePattern(elements, i, p);
                    }
                    else
                    {
                        element.Print(p);
                    }
                }
                if (!(element is IElementList) || ((IElementList)element).getElements().Count > 1)
                {
                    p.print(" .");
                }
                p.println();
            }
        }


        // Special treatment of nested rdf:Lists
        private int printTriplePattern(List<IElement> elements, int i, ISparqlPrinter p)
        {
            ITriplePattern main = (ITriplePattern)elements[i];

            // Print subject
            List<IResource> leftList = new List<IResource>();
            i = addListMembers(elements, i, leftList);
            if (leftList.Count == 0)
            {
                TupleImpl.print(getModel(), main.getSubject(), p);
            }
            else
            {
                printRDFList(p, leftList);
                main = (ITriplePattern)elements[i];
            }
            p.print(" ");

            // Print predicate
            if (RDFUtil.sameTerm(RDF.PropertyType, main.getPredicate()))
            {
                p.print("a");
            }
            else
            {
                TupleImpl.print(getModel(), main.getPredicate(), p);
            }
            p.print(" ");

            // Print object
            if (nextIsMatchingVarPattern(main, elements, i))
            {
                List<IResource> rightList = new List<IResource>();
                i = addListMembers(elements, i + 1, rightList);
                if (rightList.Count == 0)
                {
                    TupleImpl.print(getModel(), main.getObject(), p);
                    if (leftList.Count != 0)
                    {
                        i--;
                    }
                }
                else
                {
                    printRDFList(p, rightList);
                    i--;
                }
            }
            else
            {
                TupleImpl.print(getModel(), main.getObject(), p);
            }
            return i;
        }


        private void printRDFList(ISparqlPrinter p, List<IResource> members)
        {
            p.print("(");
            foreach (IResource node in members)
            {
                p.print(" ");
                TupleImpl.print(getModel(), node, p);
            }
            p.print(" )");
        }


        public new String toString()
        {
            StringBuilder sb = new StringBuilder();
            //ISparqlPrinter context = new StringSparqlPrinter(sb);
            //print(context);
            return sb.ToString();
        }

        //override public void visit(IElementVisitor visitor)
        //{
        //    visitor.visit(this);
        //}
    }
}