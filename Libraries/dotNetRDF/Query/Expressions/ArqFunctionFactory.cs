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
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Functions.Arq;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Expression Factory which generates ARQ Function expressions
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed to help provide feature parity with the ARQ query engine contained in Jena
    /// </para>
    /// </remarks>
    public class ArqFunctionFactory : ISparqlCustomExpressionFactory
    {
        /// <summary>
        /// ARQ Function Namespace
        /// </summary>
        public const String ArqFunctionsNamespace = "http://jena.hpl.hp.com/ARQ/function#";

        /// <summary>
        /// Constants for ARQ Numeric functions
        /// </summary>
        public const String Max = "max",
                            Min = "min",
                            Pi = "pi",
                            E = "e";

        /// <summary>
        /// Constants for ARQ Graph functions
        /// </summary>
        public const String BNode = "bnode",
                            LocalName = "localname",
                            Namespace = "namespace";

        /// <summary>
        /// Constants for ARQ String functions
        /// </summary>
        public const String Substr = "substr",
                            Substring = "substring",
                            StrJoin = "strjoin";

        /// <summary>
        /// Constants for ARQ Miscellaneous functions
        /// </summary>
        public const String Sha1Sum = "sha1sum",
                            Now = "now";

        /// <summary>
        /// Array of Extension Function URIs
        /// </summary>
        private String[] FunctionUris = {
                                            Max,
                                            Min,
                                            Pi,
                                            E,
                                            BNode,
                                            LocalName,
                                            Namespace,
                                            Substr,
                                            Substring,
                                            StrJoin,
                                            Sha1Sum,
                                            Now,
                                        };

        /// <summary>
        /// Tries to create an ARQ Function expression if the function Uri correseponds to a supported ARQ Function
        /// </summary>
        /// <param name="u">Function Uri</param>
        /// <param name="args">Function Arguments</param>
        /// <param name="scalarArgs">Scalar Arguments</param>
        /// <param name="expr">Generated Expression</param>
        /// <returns>Whether an expression was successfully generated</returns>
        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<String,ISparqlExpression> scalarArgs, out ISparqlExpression expr)
        {
            // If any Scalar Arguments are present then can't possibly be an ARQ Function
            if (scalarArgs.Count > 0)
            {
                expr = null;
                return false;
            }

            String func = u.AbsoluteUri;
            if (func.StartsWith(ArqFunctionsNamespace))
            {
                func = func.Substring(ArqFunctionsNamespace.Length);
                ISparqlExpression arqFunc = null;

                switch (func)
                {
                    case BNode:
                        if (args.Count == 1)
                        {
                            arqFunc = new BNodeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ bnode() function");
                        }
                        break;
                    case E:
                        if (args.Count == 0)
                        {
                            arqFunc = new EFunction();
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ e() function");
                        }
                        break;
                    case LocalName:
                        if (args.Count == 1)
                        {
                            arqFunc = new LocalNameFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ localname() function");
                        }
                        break;
                    case Max:
                        if (args.Count == 2)
                        {
                            arqFunc = new MaxFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ max() function");
                        }
                        break;
                    case Min:
                        if (args.Count == 2)
                        {
                            arqFunc = new MinFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ min() function");
                        }
                        break;
                    case Namespace:
                        if (args.Count == 1)
                        {
                            arqFunc = new NamespaceFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ namespace() function");
                        }
                        break;
                    case Now:
                        if (args.Count == 0)
                        {
                            arqFunc = new NowFunction();
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ now() function");
                        }
                        break;
                    case Pi:
                        if (args.Count == 0)
                        {
                            arqFunc = new PiFunction();
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ pi() function");
                        }
                        break;
                    case Sha1Sum:
                        if (args.Count == 1)
                        {
                            arqFunc = new Sha1Function(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ sha1sum() function");
                        }
                        break;
                    case StrJoin:
                        if (args.Count >= 2)
                        {
                            arqFunc = new StringJoinFunction(args.First(), args.Skip(1));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ strjoing() function");
                        }
                        break;
                    case Substr:
                    case Substring:
                        if (args.Count == 2)
                        {
                            arqFunc = new SubstringFunction(args.First(), args.Last());
                        }
                        else if (args.Count == 3)
                        {
                            arqFunc = new SubstringFunction(args.First(), args[1], args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the ARQ " + func + "() function");
                        }
                        break;
                }

                if (arqFunc != null)
                {
                    expr = arqFunc;
                    return true;
                }
            }
            expr = null;
            return false;  
        }

        /// <summary>
        /// Gets the Extension Function URIs supported by this Factory
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionFunctions
        {
            get
            {
                return (from u in FunctionUris
                        select UriFactory.Create(ArqFunctionsNamespace + u));
            }
        }

        /// <summary>
        /// Gets the Extension Aggregate URIs supported by this Factory
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionAggregates
        {
            get
            {
                return Enumerable.Empty<Uri>();
            }
        }
    }
}
