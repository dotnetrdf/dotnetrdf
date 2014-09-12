/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Factories
{
    /// <summary>
    /// Factory Class for generating Expressions for Sparql Extension Functions
    /// </summary>
    /// <remarks>
    /// <para>
    /// Allows for users of the Library to implement and register Custom Expression Factories which can generate Expressions for their own Extension functions which they wish to use in their SPARQL queries.   Custom factories may be globally scoped by registering them with the <see cref="ExpressionFactory.AddCustomFactory">AddCustomFactory()</see> method or locally by passing them to the three argument constructor of the <see cref="ExpressionFactory.CreateExpression(Uri, List{IExpression}, IEnumerable{IExpressionFactory})">CreateExpression()</see> method.
    /// </para>
    /// </remarks>
    public static class ExpressionFactory
    {
        /// <summary>
        /// List of Custom Expression factories
        /// </summary>
        /// <remarks>
        /// All the standard function libraries (XPath, Leviathan and ARQ) included in dotNetRDF are automatically registered
        /// </remarks>
        private static readonly List<IExpressionFactory> _customFactories = new List<IExpressionFactory>() 
        {
            new SparqlBuiltInFunctionFactory(),
            new XPathFunctionFactory(),
            new LeviathanFunctionFactory(),
            new ArqFunctionFactory()
        };

        /// <summary>
        /// Tries to create an Expression from the given function Uri and list of argument expressions
        /// </summary>
        /// <param name="u">Function Uri</param>
        /// <param name="args">List of Argument Expressions</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Uses only the globally scoped custom expression factories
        /// </para>
        /// </remarks>
        public static IExpression CreateExpression(Uri u, List<IExpression> args)
        {
            return CreateExpression(u, args, Enumerable.Empty<IExpressionFactory>());
        }

        /// <summary>
        /// Tries to create an Expression from the given function Uri and list of argument expressions
        /// </summary>
        /// <param name="u">Function Uri</param>
        /// <param name="args">List of Argument Expressions</param>
        /// <param name="factories">Enumeration of locally scoped expression factories to use</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Globally scoped custom expression factories are tried first and then any locally scoped expression factories are used
        /// </para>
        /// </remarks>
        public static IExpression CreateExpression(Uri u, List<IExpression> args, IEnumerable<IExpressionFactory> factories)
        {
            return CreateExpression(u, args, new Dictionary<String, IExpression>(), factories);
        }

        /// <summary>
        /// Tries to create an Expression from the given function Uri and list of argument expressions
        /// </summary>
        /// <param name="u">Function Uri</param>
        /// <param name="args">List of Argument Expressions</param>
        /// <param name="scalarArgs">Scalar Arguments</param>
        /// <param name="factories">Enumeration of locally scoped expression factories to use</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Globally scoped custom expression factories are tried first and then any locally scoped expression factories are used
        /// </para>
        /// </remarks>
        public static IExpression CreateExpression(Uri u, List<IExpression> args, Dictionary<String,IExpression> scalarArgs, IEnumerable<IExpressionFactory> factories)
        {
            if (SparqlSpecsHelper.SupportedCastFunctions.Contains(u.AbsoluteUri))
            {
                //Should only have 1 argument
                if (args.Count != 1)
                {
                    throw new RdfParseException("Too few/many arguments for a XPath Cast function, expected a single Expression as an argument");
                }

                //One of the Supported XPath Cast functions
                IExpression arg = args[0];
                String cast = u.AbsoluteUri;
                if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                {
                    return new BooleanCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                {
                    return new DateTimeCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeDecimal))
                {
                    return new DecimalCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble))
                {
                    return new DoubleCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
                {
                    return new FloatCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeInteger) || cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeInt))
                {
                    return new IntegerCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                {
                    return new StringCast(arg);
                }
                else
                {
                    throw new RdfParseException("Unable to parse a supported XPath Cast Function with IRI <" + u.AbsoluteUri + ">, it appears to be a valid Cast function URI but it couldn't be parsed");
                }
            }
            else
            {
                //Try to use the Global Custom Factories to generate the Expression
                IExpression expr = null;
                foreach (IExpressionFactory customFactory in _customFactories)
                {
                    if (customFactory.TryCreateExpression(u, args, scalarArgs, out expr))
                    {
                        //If the Factory succesfully creates an expression we'll return it
                        return expr;
                    }
                }

                //If we have any locally scoped factories then we can now use these to try and generate the Expression
                foreach (IExpressionFactory customFactory in factories)
                {
                    if (customFactory.TryCreateExpression(u, args, scalarArgs, out expr)) 
                    {
                        //If the Factory creates an expression we'll return it
                        return expr;
                    }
                }

                //If we're allowing Unknown functions return an UnknownFunction
                if (SparqlOptions.QueryAllowUnknownFunctions)
                {
                    return args.Count == 0 ? new UnknownFunction(u) : new UnknownFunction(u, args);
                }

                //If we get here we haven't been able to create an expression so we error
                throw new RdfParseException("Unable to parse a SPARQL Extension Function with IRI <" + u.AbsoluteUri + ">, it is not a supported Casting function and no Custom Expression Factories are able to generate an Expression from this IRI");
            }
        }

        /// <summary>
        /// Registers a Custom Expression Factory
        /// </summary>
        /// <param name="factory">A Custom Expression Factory</param>
        public static void AddCustomFactory(IExpressionFactory factory)
        {
            //Only register the factory if it is not already registered
            if (_customFactories.All(f => f.GetType() != factory.GetType()))
            {
                _customFactories.Add(factory);
            }
        }

        /// <summary>
        /// Gets the Global Custom Expression Factories that are in use
        /// </summary>
        public static IEnumerable<IExpressionFactory> Factories
        {
            get
            {
                return _customFactories;
            }
        }
    }
}
