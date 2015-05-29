/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using VDS.RDF.Query.Expressions.Aggregates.Leviathan;
using VDS.RDF.Query.Expressions.Functions.Leviathan.Hash;
using VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric;
using VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry;

namespace VDS.RDF.Query.Expressions.Factories
{
    /// <summary>
    /// Expression Factory which generates Leviathan Function expressions
    /// </summary>
    public class LeviathanFunctionFactory
        : IExpressionFactory
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
        public bool TryCreateExpression(Uri u, List<IExpression> args, Dictionary<String,IExpression> scalarArgs, out IExpression expr)
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
                IExpression lvnFunc = null;

                switch (func)
                {
                    case LeviathanFunctionFactory.All:
                        if (args.Count == 1)
                        {
                            // TODO Produce DISTINCT form where appropriate
                            lvnFunc = new AllAggregate(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for Leviathan all() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.Any:
                        if (args.Count == 1)
                        {
                            // TODO Produce DISTINCT form where appropriate
                            lvnFunc = new AnyAggregate(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for Leviathan any() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.Cartesian:
                        if (args.Count == 4)
                        {
                            lvnFunc = new CartesianFunction(args[0], args[1], args[2], args[3]);
                        }
                        else if (args.Count == 6)
                        {
                            lvnFunc = new CartesianFunction(args[0], args[1], args[2], args[3], args[4], args[5]);
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for Leviathan cartesian() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Cube:
                        if (args.Count == 1)
                        {
                            lvnFunc = new CubeFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan cube() function");
                        }
                        break;
                    case LeviathanFunctionFactory.DegreesToRadians:
                        if (args.Count == 1)
                        {
                            lvnFunc = new DegreesToRadiansFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan degrees-to-radians() function");
                        }
                        break;
                    case LeviathanFunctionFactory.E:
                        if (args.Count == 1)
                        {
                            lvnFunc = new EFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan e() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Factorial:
                        if (args.Count == 1)
                        {
                            lvnFunc = new FactorialFunction(args.First());
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
                            lvnFunc = new LogFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            lvnFunc = new LogFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan log() function");
                        }
                        break;
                    case LeviathanFunctionFactory.MD5Hash:
#if !SILVERLIGHT
                        if (args.Count == 1)
                        {
                            lvnFunc = new MD5HashFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan md5hash() function");
                        }
                        break;
#else
                        throw new RdfParseException("MD5 function not supported under Silverlight/Windows Phone 7");
#endif
                    case LeviathanFunctionFactory.Median:
                        if (args.Count == 1)
                        {
                            // TODO Produce DISTINCT form where appropriate
                            lvnFunc = new MedianAggregate(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan median() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.Mode:
                        if (args.Count == 1)
                        {
                            // TODO Produce DISTINCT form where appropriate
                            lvnFunc = new ModeAggregate(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan mode() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.None:
                        if (args.Count == 1)
                        {
                            // TODO Produce DISTINCT form where appropriate
                            lvnFunc = new NoneAggregate(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan none() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.NumericMax:
                        if (args.Count == 1)
                        {
                            // TODO Produce DISTINCT form where appropriate
                            lvnFunc = new NumericMaxAggregate(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan nmax() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.NumericMin:
                        if (args.Count == 1)
                        {
                            // TODO Produce DISTINCT form where appropriate
                            lvnFunc = new NumericMinAggregate(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan nmin() aggregate");
                        }
                        break;
                    case LeviathanFunctionFactory.Power:
                        if (args.Count == 1)
                        {
                            lvnFunc = new SquareFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            lvnFunc = new PowerFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan pow() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Pythagoras:
                        if (args.Count == 2)
                        {
                            lvnFunc = new PythagoreanDistanceFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan pythagoras() function");
                        }
                        break;
                    case LeviathanFunctionFactory.RadiansToDegrees:
                        if (args.Count == 1)
                        {
                            lvnFunc = new RadiansToDegreesFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan radians-to-degrees() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Random:
                        if (args.Count == 0)
                        {
                            lvnFunc = new RandomFunction();
                        }
                        else if (args.Count == 1)
                        {
                            lvnFunc = new RandomFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            lvnFunc = new RandomFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan rnd() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Reciprocal:
                        if (args.Count == 1)
                        {
                            lvnFunc = new ReciprocalFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan reciprocal() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Root:
                        if (args.Count == 1)
                        {
                            lvnFunc = new SquareRootFunction(args.First());
                        }
                        else if (args.Count == 2)
                        {
                            lvnFunc = new RootFunction(args.First(), args.Last());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan root() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Sha256Hash:
                        if (args.Count == 1)
                        {
                            lvnFunc = new Sha256HashFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan sha256hash() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Square:
                        if (args.Count == 1)
                        {
                            lvnFunc = new SquareFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan sq() function");
                        }
                        break;
                    case LeviathanFunctionFactory.SquareRoot:
                        if (args.Count == 1)
                        {
                            lvnFunc = new SquareRootFunction(args.First());
                        }
                        else
                        {
                            throw new RdfParseException("Incorrect number of arguments for the Leviathan sqrt() function");
                        }
                        break;
                    case LeviathanFunctionFactory.Ten:
                        if (args.Count == 1)
                        {
                            lvnFunc = new TenFunction(args.First());
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
                            lvnFunc = new CosineFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigCosInv));
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
                            lvnFunc = new CosecantFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigCosecInv));
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
                            lvnFunc = new CotangentFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigCotanInv));
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
                            lvnFunc = new SecantFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigSecInv));
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
                            lvnFunc = new SineFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigSinInv));
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
                            lvnFunc = new TangentFunction(args.First(), func.Equals(LeviathanFunctionFactory.TrigTanInv));
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
                        select UriFactory.Create(LeviathanFunctionsNamespace + u));
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
                        select UriFactory.Create(LeviathanFunctionsNamespace + u));
            }
        }
    }
}
