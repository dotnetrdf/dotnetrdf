/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Functions;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Factory Class for generating Expressions for Sparql Extension Functions
    /// </summary>
    /// <remarks>
    /// <para>
    /// Allows for users of the Library to implement and register Custom Expression Factories which can generate Expressions for their own Extension functions which they wish to use in their SPARQL queries.   Custom factories may be globally scoped by registering them with the <see cref="SparqlExpressionFactory.AddCustomFactory">AddCustomFactory()</see> method or locally by passing them to the three argument constructor of the <see cref="SparqlExpressionFactory.CreateExpression">CreateExpression()</see> method.
    /// </para>
    /// </remarks>
    public static class SparqlExpressionFactory
    {
        /// <summary>
        /// List of Custom Expression factories
        /// </summary>
        /// <remarks>
        /// All the standard function libraries (XPath, Leviathan and ARQ) included in dotNetRDF are automatically registered
        /// </remarks>
        private static List<ISparqlCustomExpressionFactory> _customFactories = new List<ISparqlCustomExpressionFactory>() 
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
        public static ISparqlExpression CreateExpression(Uri u, List<ISparqlExpression> args)
        {
            return CreateExpression(u, args, Enumerable.Empty<ISparqlCustomExpressionFactory>());
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
        public static ISparqlExpression CreateExpression(Uri u, List<ISparqlExpression> args, IEnumerable<ISparqlCustomExpressionFactory> factories)
        {
            return CreateExpression(u, args, new Dictionary<String, ISparqlExpression>(), factories);
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
        public static ISparqlExpression CreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<String,ISparqlExpression> scalarArgs, IEnumerable<ISparqlCustomExpressionFactory> factories)
        {
            if (SparqlSpecsHelper.SupportedCastFunctions.Contains(u.ToString()))
            {
                //Should only have 1 argument
                if (args.Count != 1)
                {
                    throw new RdfParseException("Too few/many arguments for a XPath Cast function, expected a single Expression as an argument");
                }

                //One of the Supported XPath Cast functions
                ISparqlExpression arg = args[0];
                String cast = u.ToString();
                if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                {
                    return new XPathBooleanCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                {
                    return new XPathDateTimeCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeDecimal))
                {
                    return new XPathDecimalCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble))
                {
                    return new XPathDoubleCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
                {
                    return new XPathFloatCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeInteger) || cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeInt))
                {
                    return new XPathIntegerCast(arg);
                }
                else if (cast.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                {
                    return new XPathStringCast(arg);
                }
                else
                {
                    throw new RdfParseException("Unable to parse a supported XPath Cast Function with IRI <" + u.ToString() + ">, it appears to be a valid Cast function URI but it couldn't be parsed");
                }
            }
            else
            {
                //Try to use the Global Custom Factories to generate the Expression
                ISparqlExpression expr = null;
                foreach (ISparqlCustomExpressionFactory customFactory in _customFactories)
                {
                    if (customFactory.TryCreateExpression(u, args, scalarArgs, out expr))
                    {
                        //If the Factory succesfully creates an expression we'll return it
                        return expr;
                    }
                }

                //If we have any locally scoped factories then we can now use these to try and generate the Expression
                foreach (ISparqlCustomExpressionFactory customFactory in factories)
                {
                    if (customFactory.TryCreateExpression(u, args, scalarArgs, out expr)) 
                    {
                        //If the Factory creates an expression we'll return it
                        return expr;
                    }
                }

                //If we're allowing Unknown functions return an UnknownFunction
                if (Options.QueryAllowUnknownFunctions)
                {
                    if (args.Count == 0)
                    {
                        return new UnknownFunction(u);
                    }
                    else
                    {
                        return new UnknownFunction(u, args);
                    }
                }

                //If we get here we haven't been able to create an expression so we error
                throw new RdfParseException("Unable to parse a SPARQL Extension Function with IRI <" + u.ToString() + ">, it is not a supported Casting function and no Custom Expression Factories are able to generate an Expression from this IRI");
            }
        }

        /// <summary>
        /// Registers a Custom Expression Factory
        /// </summary>
        /// <param name="factory">A Custom Expression Factory</param>
        public static void AddCustomFactory(ISparqlCustomExpressionFactory factory)
        {
            //Only register the factory if it is not already registered
            if (!_customFactories.Any(f => f.GetType().Equals(factory.GetType())))
            {
                _customFactories.Add(factory);
            }
        }

        /// <summary>
        /// Gets the Global Custom Expression Factories that are in use
        /// </summary>
        public static IEnumerable<ISparqlCustomExpressionFactory> Factories
        {
            get
            {
                return _customFactories;
            }
        }
    }
}
