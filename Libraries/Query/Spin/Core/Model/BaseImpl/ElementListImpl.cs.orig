using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{


    public class ElementListImpl : ElementImpl, IElementListResource
    {

        public ElementListImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        private int addListMembers(List<IElementResource> elements, int i, List<IResource> members)
        {
            bool first = true;
            while (i < elements.Count - 1 &&
                    elements[i] is ITriplePatternResource &&
                    elements[i + 1] is ITriplePatternResource)
            {
                ITriplePatternResource firstPattern = (ITriplePatternResource)elements[i];
                ITriplePatternResource secondPattern = (ITriplePatternResource)elements[i + 1];
                if (RDFHelper.SameTerm(RDF.PropertyFirst, firstPattern.getPredicate()) && RDFHelper.SameTerm(RDF.PropertyRest, secondPattern.getPredicate()))
                {
                    IResource firstSubject = firstPattern.getSubject();
                    IResource secondSubject = secondPattern.getSubject();
                    if (firstSubject is IVariableResource && secondSubject is IVariableResource)
                    {
                        IVariableResource firstVar = (IVariableResource)firstSubject;
                        IVariableResource secondVar = (IVariableResource)secondSubject;
                        if (firstVar.isBlankNodeVar() && firstVar.getName().Equals(secondVar.getName()))
                        {
                            members.Add(firstPattern.getObject());
                            IResource secondObject = secondPattern.getObject();
                            i++;
                            if (RDFHelper.SameTerm(RDF.Nil, secondObject))
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


        public List<IElementResource> getElements()
        {
            List<IElementResource> results = new List<IElementResource>();
            IEnumerator<IResource> it = AsList().GetEnumerator();
            while (it.MoveNext())
            {
                IResource node = it.Current;
                if (!(node.IsLiteral()))
                {
                    IElementResource element = ResourceFactory.asElement(node);
                    if (element != null)
                    {
                        results.Add(element);
                    }
                }
            }
            return results;
        }


        private bool nextIsMatchingVarPattern(ITriplePatternResource main, List<IElementResource> elements, int i)
        {
            if (main.getObject() is IVariableResource &&
                    i < elements.Count - 2 &&
                    elements[i + 1] is ITriplePatternResource &&
                    elements[i + 2] is ITriplePatternResource)
            {
                IVariableResource mainVar = (IVariableResource)main.getObject();
                if (mainVar.isBlankNodeVar())
                {
                    ITriplePatternResource nextPattern = (ITriplePatternResource)elements[i + 1];
                    ITriplePatternResource lastPattern = (ITriplePatternResource)elements[i + 2];
                    IResource nextSubject = nextPattern.getSubject();
                    IResource lastSubject = lastPattern.getSubject();
                    if (nextSubject is IVariableResource &&
                       lastSubject is IVariableResource &&
                            RDFHelper.SameTerm(RDF.PropertyFirst, nextPattern.getPredicate()) &&
                            RDFHelper.SameTerm(RDF.PropertyRest, lastPattern.getPredicate()))
                    {
                        IVariableResource nextVar = (IVariableResource)nextSubject;
                        if (mainVar.getName().Equals(nextVar.getName()))
                        {
                            IVariableResource lastVar = (IVariableResource)lastSubject;
                            return mainVar.getName().Equals(lastVar.getName());
                        }
                    }
                }
            }
            return false;
        }


        override public void Print(ISparqlPrinter p)
        {
            List<IElementResource> elements = getElements();

            int oldI = -1;
            for (int i = 0; i < elements.Count; i++)
            {
                if (i == oldI)
                {
                    break; // Prevent unknown endless loop conditions
                }
                oldI = i;
                IElementResource element = elements[i];
                p.printIndentation(p.getIndentation());
                if (element is IElementListResource && ((IElementListResource)element).getElements().Count > 1)
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
                    if (element is ITriplePatternResource)
                    {
                        i = printTriplePattern(elements, i, p);
                    }
                    else
                    {
                        element.Print(p);
                    }
                }
                if (!(element is IElementListResource) || ((IElementListResource)element).getElements().Count > 1)
                {
                    p.print(" .");
                }
                p.println();
            }
        }


        // Special treatment of nested rdf:Lists
        private int printTriplePattern(List<IElementResource> elements, int i, ISparqlPrinter p)
        {
            ITriplePatternResource main = (ITriplePatternResource)elements[i];

            // Print subject
            List<IResource> leftList = new List<IResource>();
            i = addListMembers(elements, i, leftList);
            if (leftList.Count == 0)
            {
                TupleImpl.print(GetModel(), main.getSubject(), p);
            }
            else
            {
                printRDFList(p, leftList);
                main = (ITriplePatternResource)elements[i];
            }
            p.print(" ");

            // Print predicate
            if (RDFHelper.SameTerm(RDF.PropertyType, main.getPredicate()))
            {
                p.print("a");
            }
            else
            {
                TupleImpl.print(GetModel(), main.getPredicate(), p);
            }
            p.print(" ");

            // Print object
            if (nextIsMatchingVarPattern(main, elements, i))
            {
                List<IResource> rightList = new List<IResource>();
                i = addListMembers(elements, i + 1, rightList);
                if (rightList.Count == 0)
                {
                    TupleImpl.print(GetModel(), main.getObject(), p);
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
                TupleImpl.print(GetModel(), main.getObject(), p);
            }
            return i;
        }


        private void printRDFList(ISparqlPrinter p, List<IResource> members)
        {
            p.print("(");
            foreach (IResource node in members)
            {
                p.print(" ");
                TupleImpl.print(GetModel(), node, p);
            }
            p.print(" )");
        }


        public String toString()
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