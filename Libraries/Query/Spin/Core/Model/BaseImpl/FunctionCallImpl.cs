/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved.
 *******************************************************************************/

using System;
using System.Collections.Generic;
using VDS.RDF.Query.Spin.Model.IO;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.Model
{
    public class FunctionCallImpl : ModuleCallImpl, IFunctionCallResource
    {
        private static String SP_ARG = SP.PropertyArg.ToString();

        public FunctionCallImpl(INode node, SpinModel spinModel)
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
                results.Add(ResourceFactory.asExpression(node));
            }
            return results;
        }

        private IResource[] getArgumentProperties(Dictionary<IResource, IResource> values)
        {
            IResource[] ps = new IResource[values.Count];
            List<IResource> others = new List<IResource>();
            foreach (IResource p in values.Keys)
            {
                if (p.Uri.ToString().StartsWith(SP_ARG) && !RDFHelper.SameTerm(p, SP.PropertyArg))
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
                    return RDFHelper.uriComparer.Compare(arg0.Uri, arg1.Uri);
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
            IEnumerator<Triple> it = ListProperties().GetEnumerator();
            while (it.MoveNext())
            {
                Triple s = it.Current;
                if (!RDFHelper.SameTerm(RDF.PropertyType, s.Predicate))
                {
                    values[SpinResource.Get(s.Predicate, GetModel())] = SpinResource.Get(s.Object, GetModel());
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
            IEnumerator<Triple> it = ListProperties(RDF.PropertyType).GetEnumerator();
            while (it.MoveNext())
            {
                Triple s = it.Current;
                if (s.Object is IUriNode)
                {
                    IResource candidate = SpinResource.Get(s.Object, GetModel());
                    if (type == null)
                    {
                        type = candidate;
                    }
                    else if (!candidate.GetModel().ContainsTriple(null, RDFS.PropertySubClassOf, candidate))
                    {
                        type = candidate;
                    }
                }
            }

            if (type != null)
            {
                if (type.HasProperty(RDFS.PropertySubClassOf, SPIN.ClassFunction))
                {
                    return type;
                }
                //else
                //{
                //    IResource global = SPINModuleRegistry.getFunction(type.Uri, null);
                //    if (global != null)
                //    {
                //        return global;
                //    }
                //    else
                //    {
                //        return type;
                //    }
                //}
                return type;
            }
            else
            {
                return null;
            }
        }

        override public IModuleResource getModule()
        {
            IResource function = getFunction();
            if (function != null)
            {
                return (IModuleResource)function.As(typeof(FunctionImpl));
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
                return function.GetString(SPIN.PropertySymbol);
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
            else if (symbol != null && (RDFHelper.SameTerm(SP.PropertyExists, function) || RDFHelper.SameTerm(SP.PropertyNotExists, function)))
            {
                printExistsOrNotExists(p, symbol);
            }
            else
            {
                printFunction(p, function, args);
            }
        }

        private void printOperator(ISparqlPrinter p, String op, List<IResource> args)
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

        private void printExistsOrNotExists(ISparqlPrinter p, String symbol)
        {
            p.print(symbol);
            printNestedElementList(p, SP.PropertyElements);
        }

        private void printFunction(ISparqlPrinter p, IResource function, List<IResource> args)
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