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
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Expressions.Functions;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Expression Factory which generates Leviathan Function expressions
    /// </summary>
    public class LeviathanFunctionFactory : ISparqlCustomExpressionFactory
    {
        /// <summary>
        /// Leviathan Function Namespace
        /// </summary>
        public const String LeviathanFunctionsNamespace = "http://www.dotnetrdf.org/leviathan#";

        /// <summary>
        /// Constants for Leviathan String Functions
        /// </summary>
        public const String MD5Hash = "md5hash",
                            Sha256Hash = "sha256hash";

        /// <summary>
        /// Constants for Leviathan Numeric Functions
        /// </summary>
        public const String Random = "rnd",
                            TrigSin = "sin",
                            TrigSinInv = "sin-1",
                            TrigCos = "cos",
                            TrigCosInv = "cos-1",
                            TrigTan = "tan",
                            TrigTanInv = "tan-1",
                            TrigSec = "sec",
                            TrigSecInv = "sec-1",
                            TrigCosec = "cosec",
                            TrigCosecInv = "cosec-1",
                            TrigCotan = "cotan",
                            TrigCotanInv = "cotan-1",
                            DegreesToRadians = "degrees-to-radians",
                            RadiansToDegrees = "radians-to-degrees",
                            Log = "log",
                            Ln = "ln",
                            E = "e",
                            Ten = "ten",
                            Power = "pow",
                            Square = "sq",
                            Cube = "cube",
                            SquareRoot = "sqrt",
                            Root = "root",
                            Pythagoras = "pythagoras",
                            Cartesian = "cartesian",
                            Factorial = "factorial",
                            Reciprocal = "reciprocal";

        /// <summary>
        /// Constants for Leviathan Boolean Aggregates
        /// </summary>
        public const String All = "all",
                            Any = "any",
                            None = "none";

        /// <summary>
        /// Constants for Leviathan Numeric Aggregates
        /// </summary>
        public const String NumericMin = "nmin",
                            NumericMax = "nmax";

        /// <summary>
        /// Constants for other Leviathan Aggregate
        /// </summary>
        public const String Mode = "mode",
                            Median = "median";

        /// <summary>
        /// Array of Extension Function URIs
        /// </summary>
        private String[] FunctionUris = {
                                            MD5Hash,
                                            Sha256Hash,
                                            Random,
                                            TrigCos,
                                            TrigCosec,
                                            TrigCosecInv,
                                            TrigCosInv,
                                            TrigCotan,
                                            TrigCotanInv,
                                            TrigSec,
                                            TrigSecInv,
                                            TrigSin,
                                            TrigSinInv,
                                            TrigTan,
                                            TrigTanInv,
                                            RadiansToDegrees,
                                            DegreesToRadians,
                                            Log,
                                            Ln,
                                            E,
                                            Ten,
                                            Power,
                                            Square,
                                            Cube,
                                            SquareRoot,
                                            Root,
                                            Pythagoras,
                                            Cartesian,
                                            Factorial,
                                        };

        /// <summary>
        /// Array of Extension Aggregate URIs
        /// </summary>
        private String[] AggregateUris = {
                                             All,
                                             Any,
                                             None,
                                             NumericMax,
                                             NumericMin,
                                             Mode,
                                             Median
                                         };


        /// <summary>
        /// Tries to create an Leviathan Function expression if the function Uri correseponds to a supported Leviathan Function
        /// </summary>
        /// <param name="u">Function Uri</param>
        /// <param name="args">Function Arguments</param>
        /// <param name="scalarArgs">Scalar Arguments</param>
        /// <param name="expr">Generated Expression</param>
        /// <returns>Whether an expression was successfully generated</returns>
        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, Dictionary<String,ISparqlExpression> scalarArgs, out ISparqlExpression expr)
        {
            //If any Scalar Arguments are present then can't possibly be a Leviathan Function
            if (scalarArgs.Count > 0)
            {
                expr = null;
                return false;
            }

            String func = u.ToString();
            if (func.StartsWith(LeviathanFunctionFactory.LeviathanFunctionsNamespace))
            {
                func = func.Substring(LeviathanFunctionFactory.LeviathanFunctionsNamespace.Length);
                ISparqlExpression lvnFunc = null;

                switch (func)
                {
                    case LeviathanFunctionFactory.All:
                        if (args.Count == 1)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new AllAggregate(args.First()));
                        }
                        else if (args.Count == 2 && args.First() is DistinctModifierExpression)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new AllAggregate(args.Last(), true));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for Leviathan all() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.Any:
                        if (args.Count == 1)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new AnyAggregate(args.First()));
                        }
                        else if (args.Count == 2 && args.First() is DistinctModifierExpression)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new AnyAggregate(args.Last(), true));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for Leviathan any() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.Cartesian:
                        if (args.Count == 4)
                        {
                            lvnFunc = new LeviathanCartesianFunction(args[0], args[1], args[2], args[3]);
                        }
                        else if (args.Count == 6)
                        {
                            lvnFunc = new LeviathanCartesianFunction(args[0], args[1], args[2], args[3], args[4], args[5]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for Leviathan cartesian() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Cube:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanCubeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan cube() function");
                        }
                        break;
                    case LeviathanFunctionFactory.DegreesToRadians:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanDegreesToRadiansFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan degrees-to-radians() function");
                        }
                        break;
                    case LeviathanFunctionFactory.E:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanEFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan e() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Factorial:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanFactorialFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan factorial() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Ln:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanNaturalLogFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan ln() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Log:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanLogFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            lvnFunc = new LeviathanLogFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan log() function");
                        }
                        break;
                    case LeviathanFunctionFactory.MD5Hash:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanMD5HashFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan md5hash() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Median:
                        if (args.Count == 1)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new MedianAggregate(args.First()));
                        }
                        else if (args.Count == 2 && args.First() is DistinctModifierExpression)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new MedianAggregate(args.Last(), true));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan median() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.Mode:
                        if (args.Count == 1)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new ModeAggregate(args.First()));
                        }
                        else if (args.Count == 2 && args.First() is DistinctModifierExpression)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new ModeAggregate(args.Last(), true));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan mode() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.None:
                        if (args.Count == 1)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new NoneAggregate(args.First()));
                        }
                        else if (args.Count == 2 && args.First() is DistinctModifierExpression)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new NoneAggregate(args.Last(), true));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan none() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.NumericMax:
                        if (args.Count == 1)
                        {
                            lvnFunc = new AggregateExpressionTerm(new NumericMaxAggregate(args.First()));
                        }
                        else if (args.Count == 2 && args.First() is DistinctModifierExpression)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new NumericMaxAggregate(args.Last(), true));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan nmax() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.NumericMin:
                        if (args.Count == 1)
                        {
                            lvnFunc = new AggregateExpressionTerm(new NumericMinAggregate(args.First()));
                        }
                        else if (args.Count == 2 && args.First() is DistinctModifierExpression)
                        {
                            lvnFunc = new NonNumericAggregateExpressionTerm(new NumericMinAggregate(args.Last(), true));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan nmin() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.Power:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanSquareFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            lvnFunc = new LeviathanPowerFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan pow() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Pythagoras:
                        if (args.Count == 2)
                        {
                            lvnFunc = new LeviathanPyathagoreanDistanceFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan pythagoras() function");
                        }
                        break;
                    case LeviathanFunctionFactory.RadiansToDegrees:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanRadiansToDegreesFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan radians-to-degrees() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Random:
                        if (args.Count == 0)
                        {
                            lvnFunc = new LeviathanRandomFunction();
                        }
                        else if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanRandomFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            lvnFunc = new LeviathanRandomFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan rnd() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Reciprocal:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanReciprocalFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan reciprocal() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Root:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanSquareRootFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            lvnFunc = new LeviathanRootFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan root() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Sha256Hash:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanSha256HashFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan sha256hash() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Square:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanSquareFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan sq() function");
                        }
                        break;
                    case LeviathanFunctionFactory.SquareRoot:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanSquareRootFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan sqrt() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Ten:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanTenFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan ten() function");
                        }
                        break;
                    case LeviathanFunctionFactory.TrigCos:
                    case LeviathanFunctionFactory.TrigCosInv:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanCosineFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigCosInv));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan " + func + "() function");
                        }
                        break;
                    case LeviathanFunctionFactory.TrigCosec:
                    case LeviathanFunctionFactory.TrigCosecInv:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanCosecantFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigCosecInv));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan " + func + "() function");
                        }
                        break;
                    case LeviathanFunctionFactory.TrigCotan:
                    case LeviathanFunctionFactory.TrigCotanInv:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanCotangentFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigCotanInv));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan " + func + "() function");
                        }
                        break;
                    case LeviathanFunctionFactory.TrigSec:
                    case LeviathanFunctionFactory.TrigSecInv:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanSecantFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigSecInv));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan " + func + "() function");
                        }
                        break;
                    case LeviathanFunctionFactory.TrigSin:
                    case LeviathanFunctionFactory.TrigSinInv:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanSineFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigSinInv));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan " + func + "() function");
                        }
                        break;
                    case LeviathanFunctionFactory.TrigTan:
                    case LeviathanFunctionFactory.TrigTanInv:
                        if (args.Count == 1)
                        {
                            lvnFunc = new LeviathanTangentFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigTanInv));
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan " + func + "() function");
                        }
                        break;
                }

                if (lvnFunc != null)
                {
                    expr = lvnFunc;
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
                        select new Uri(LeviathanFunctionsNamespace + u));
            }
        }

        /// <summary>
        /// Gets the Extension Aggregate URIs supported by this Factory
        /// </summary>
        public IEnumerable<Uri> AvailableExtensionAggregates
        {
            get
            {
                return (from u in AggregateUris
                        select new Uri(LeviathanFunctionsNamespace + u));
            }
        }
    }
}
