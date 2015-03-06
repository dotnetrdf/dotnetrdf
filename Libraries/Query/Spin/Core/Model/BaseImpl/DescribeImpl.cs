using System.Collections.Generic;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Model.IO;

namespace VDS.RDF.Query.Spin.Model
{
    public class DescribeImpl : QueryImpl, IDescribeResource
    {

        public DescribeImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }

        public List<IResource> getResultNodes()
        {
            List<IResource> results = new List<IResource>();
            foreach (IResource node in getList(SP.PropertyResultNodes))
            {
                IVariableResource variable = ResourceFactory.asVariable(node);
                if (variable != null)
                {
                    results.Add(variable);
                }
                else if (node.IsUri())
                {
                    results.Add(node);
                }
            }
            return results;
        }


        override public void printSPINRDF(ISparqlPrinter context)
        {
            printComment(context);
            printPrefixes(context);
            context.printKeyword("DESCRIBE");
            context.print(" ");
            List<IResource> nodes = getResultNodes();
            if (nodes.Count == 0)
            {
                context.print("*");
            }
            else
            {
                for (IEnumerator<IResource> nit = nodes.GetEnumerator(); nit.MoveNext(); )
                {
                    IResource node = nit.Current;
                    if (node is IVariableResource)
                    {
                        context.print(node.ToString());
                    }
                    else
                    {
                        printVarOrResource(context, node);
                    }
                    if (nit.MoveNext())
                    {
                        context.print(" ");
                    }
                }
            }
            printStringFrom(context);
            if (getWhereElements().Count != 0)
            {
                context.println();
                printWhere(context);
            }
            printSolutionModifiers(context);
            printValues(context);
        }

        override public void Print(ISparqlPrinter p)
        {
            // TODO Auto-generated method stub
        }

    }
}