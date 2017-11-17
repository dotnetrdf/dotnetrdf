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
using System.Collections.Generic;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Model
{
    internal class FunctionCallImpl : ModuleCallImpl, IFunctionCall
    {

        private static String SP_ARG = SP.PropertyArg.ToString();


        public FunctionCallImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public List<IResource> getArguments()
        {
            Dictionary<IResource, IResource> values = getArgumentsMap();
            IResource[] ps = getArgumentProperties(values);
            List<IResource> results = new List<IResource>(ps.Length);
            foreach (IResource key in ps)
            {
                IResource node = values[key];
                results.Add(SPINFactory.asExpression(node));
            }
            return results;
        }


        private IResource[] getArgumentProperties(Dictionary<IResource, IResource> values)
        {
            IResource[] ps = new IResource[values.Count];
            List<IResource> others = new List<IResource>();
            foreach (IResource p in values.Keys)
            {
                if (p.Uri.ToString().StartsWith(SP_ARG) && !RDFUtil.sameTerm(p, SP.PropertyArg))
                {
                    int index = int.Parse(p.Uri.ToString().Substring(SP_ARG.Length));
                    ps[index - 1] = p;
                }
                else
                {
                    others.Add(p);
                }
            }
            if (others.Count > 0)
            {
                others.Sort(delegate(IResource arg0, IResource arg1)
                {
                    //TODO is that OK ?
                    return RDFUtil.uriComparer.Compare(arg0.Uri, arg1.Uri);
                }
                );
                IEnumerator<IResource> it = others.GetEnumerator();
                for (int i = 0; i < ps.Length; i++)
                {
                    if (ps[i] == null)
                    {
                        ps[i] = it.Current;
                    }
                }
            }
            return ps;
        }


        public Dictionary<IResource, IResource> getArgumentsMap()
        {
            /*sealed*/
            Dictionary<IResource, IResource> values = new Dictionary<IResource, IResource>();
            IEnumerator<Triple> it = listProperties().GetEnumerator();
            while (it.MoveNext())
            {
                Triple s = it.Current;
                if (!RDFUtil.sameTerm(RDF.PropertyType, s.Predicate))
                {
                    values[Resource.Get(s.Predicate, getModel())] = Resource.Get(s.Object, getModel());
                }
            }
            return values;
        }


        public IResource getFunction()
        {

            // Need to iterate over rdf:types - some may have been inferred
            // Return the most specific type, i.e. the one that does not have
            // any subclasses
            IResource type = null;
            IEnumerator<Triple> it = listProperties(RDF.PropertyType).GetEnumerator();
            while (it.MoveNext())
            {
                Triple s = it.Current;
                if (s.Object is IUriNode)
                {
                    IResource candidate = Resource.Get(s.Object, getModel());
                    if (type == null)
                    {
                        type = candidate;
                    }
                    else if (!candidate.getModel().ContainsTriple(null, RDFS.PropertySubClassOf, candidate))
                    {
                        type = candidate;
                    }
                }
            }

            if (type != null)
            {
                if (type.hasProperty(RDFS.PropertySubClassOf, SPIN.ClassFunction))
                {
                    return type;
                }
                else
                {
                    IResource global = SPINModuleRegistry.getFunction(type.Uri, null);
                    if (global != null)
                    {
                        return global;
                    }
                    else
                    {
                        return type;
                    }
                }
            }
            else
            {
                return null;
            }
        }


        override public IModule getModule()
        {
            IResource function = getFunction();
            if (function != null)
            {
                return (IModule)function.As(typeof(FunctionImpl));
            }
            else
            {
                return null;
            }
        }


        private String getSymbol(IResource function)
        {
            if (function != null)
            {
                return function.getString(SPIN.PropertySymbol);
            }
            return null;
        }


        public static bool isSetOperator(String symbol)
        {
            return "IN".Equals(symbol) || "NOT IN".Equals(symbol);
        }


        override public void Print(ISparqlPrinter p)
        {
            IResource function = getFunction();
            List<IResource> args = getArguments();

            String symbol = getSymbol(function);
            //TODO implement isLetter
            if (symbol != null && (/*TODO !symbol[0].isLetter() ||*/ isSetOperator(symbol)))
            {
                printOperator(p, symbol, args);
            }
            else if (symbol != null && (RDFUtil.sameTerm(SP.PropertyExists, function) || RDFUtil.sameTerm(SP.PropertyNotExists, function)))
            {
                printExistsOrNotExists(p, symbol);
            }
            else
            {
                printFunction(p, function, args);
            }
        }


        void printOperator(ISparqlPrinter p, String op, List<IResource> args)
        {
            if (p.isNested())
            {
                p.print("(");
            }
            bool set = isSetOperator(op);
            if (args.Count == 1 && !set)
            {
                p.print(op);
                printNestedExpressionString(p, args[0]);
            }
            else
            { // assuming parameters.size() == 2
                printNestedExpressionString(p, args[0]);
                p.print(" ");
                p.print(op);
                p.print(" ");
                if (set)
                {
                    p.print("(");
                    for (int i = 1; i < args.Count; i++)
                    {
                        if (i > 1)
                        {
                            p.print(", ");
                        }
                        printNestedExpressionString(p, args[i]);
                    }
                    p.print(")");
                }
                else
                {
                    printNestedExpressionString(p, args[1]);
                }
            }
            if (p.isNested())
            {
                p.print(")");
            }
        }


        void printExistsOrNotExists(ISparqlPrinter p, String symbol)
        {
            p.print(symbol);
            printNestedElementList(p, SP.PropertyElements);
        }


        void printFunction(ISparqlPrinter p, IResource function, List<IResource> args)
        {
            printFunctionQName(p, function);
            p.print("(");
            IEnumerator<IResource> it = args.GetEnumerator();
            while (it.MoveNext())
            {
                IResource param = it.Current;
                printNestedExpressionString(p, param);
                if (it.MoveNext())
                {
                    p.print(", ");
                }
            }
            p.print(")");
        }


        private void printFunctionQName(ISparqlPrinter p, IResource function)
        {
            String symbol = getSymbol(function);
            if (symbol != null)
            {
                p.print(symbol);
            }
            else
            {
                //TODO ??????????
                p.printURIResource(function);
            }
        }
    }
}