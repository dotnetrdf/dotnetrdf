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

using System.Collections.Generic;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;

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