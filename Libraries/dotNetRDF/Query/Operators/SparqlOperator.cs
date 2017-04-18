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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Operators.DateTime;
using VDS.RDF.Query.Operators.Numeric;

namespace VDS.RDF.Query.Operators
{
    /// <summary>
    /// Registry of SPARQL Operators
    /// </summary>
    public static class SparqlOperators
    {
        private static Dictionary<SparqlOperatorType, List<ISparqlOperator>> _operators = new Dictionary<SparqlOperatorType, List<ISparqlOperator>>();
        private static bool _init = false;

        /// <summary>
        /// Initializes the Operators registry
        /// </summary>
        private static void Init()
        {
            if (_init) return;
            lock (_operators)
            {
                if (_init) return;

                // Set up empty registry for each operator type
                foreach (SparqlOperatorType type in Enum.GetValues(typeof(SparqlOperatorType)).OfType<SparqlOperatorType>())
                {
                    _operators.Add(type, new List<ISparqlOperator>());
                }
     
                // Register default operators
                // Numerics
                _operators[SparqlOperatorType.Add].Add(new AdditionOperator());
                _operators[SparqlOperatorType.Subtract].Add(new SubtractionOperator());
                _operators[SparqlOperatorType.Divide].Add(new DivisionOperator());
                _operators[SparqlOperatorType.Multiply].Add(new MultiplicationOperator());
                // Date Time
                _operators[SparqlOperatorType.Add].Add(new DateTimeAddition());
                _operators[SparqlOperatorType.Subtract].Add(new DateTimeSubtraction());
                // Time Span
                _operators[SparqlOperatorType.Add].Add(new TimeSpanAddition());
                _operators[SparqlOperatorType.Subtract].Add(new TimeSpanSubtraction());

                _init = true;
            }
        }

        /// <summary>
        /// Registers a new operator
        /// </summary>
        /// <param name="op">Operator</param>
        public static void AddOperator(ISparqlOperator op)
        {
            if (!_init) Init();
            lock (_operators)
            {
                _operators[op.Operator].Add(op);
            }
        }

        /// <summary>
        /// Removes the registration of an operator by instance reference
        /// </summary>
        /// <param name="op">Operator Reference</param>
        public static void RemoveOperator(ISparqlOperator op)
        {
            if (!_init) Init();
            lock (_operators)
            {
                _operators[op.Operator].Remove(op);
            }
        }

        /// <summary>
        /// Removes the registration of an operator by instance type of the operator
        /// </summary>
        /// <param name="op">Operator</param>
        public static void RemoveOperatorByType(ISparqlOperator op)
        {
            if (!_init) Init();
            lock (_operators)
            {
                _operators[op.Operator].RemoveAll(o => op.GetType().Equals(o.GetType()));
            }
        }

        /// <summary>
        /// Resets Operator registry to default state
        /// </summary>
        public static void Reset()
        {
            if (_init)
            {
                lock (_operators)
                {
                    _init = false;
                    _operators = new Dictionary<SparqlOperatorType, List<ISparqlOperator>>();
                    Init();
                }
            }
        }

        /// <summary>
        /// Returns whether the given operator is registered
        /// </summary>
        /// <param name="op">Operator</param>
        /// <returns></returns>
        /// <remarks>
        /// Checking is done both by reference and instance type so you can check if an operator is registered even if you don't have the actual reference to the instance that registered
        /// </remarks>
        public static bool IsRegistered(ISparqlOperator op)
        {
            if (!_init) Init();
            lock (_operators)
            {
                return _operators[op.Operator].Contains(op) || _operators[op.Operator].Any(o => op.GetType().Equals(o.GetType()));
            }
        }

        /// <summary>
        /// Gets all registered Operators
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ISparqlOperator> GetOperators()
        {
            if (!_init) Init();
            lock (_operators)
            {
                return (from type in _operators.Keys
                        from op in _operators[type]
                        select op).ToList();
            }
        }

        /// <summary>
        /// Gets all registered operators for the given Operator Type
        /// </summary>
        /// <param name="type">Operator Type</param>
        /// <returns></returns>
        public static IEnumerable<ISparqlOperator> GetOperators(SparqlOperatorType type)
        {
            if (!_init) Init();
            lock (_operators)
            {
                return _operators[type].ToList();
            }
        }

        /// <summary>
        /// Tries to return the operator which applies for the given inputs
        /// </summary>
        /// <param name="type">Operator Type</param>
        /// <param name="op">Operator</param>
        /// <param name="ns">Inputs</param>
        /// <returns></returns>
        public static bool TryGetOperator(SparqlOperatorType type, out ISparqlOperator op, params IValuedNode[] ns)
        {
            if (!_init) Init();

            op = null;
            List<ISparqlOperator> ops;
            lock (_operators)
            {
                if (_operators.TryGetValue(type, out ops))
                {
                    foreach (ISparqlOperator possOp in ops)
                    {
                        if (possOp.IsApplicable(ns))
                        {
                            op = possOp;
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
