/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Operators.Numeric;
using VDS.RDF.Query.Operators.DateTime;

namespace VDS.RDF.Query.Operators
{
    /// <summary>
    /// Registry of SPARQL Operands
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

                //Set up empty registry for each operator type
                foreach (SparqlOperatorType type in Enum.GetValues(typeof(SparqlOperatorType)).OfType<SparqlOperatorType>())
                {
                    _operators.Add(type, new List<ISparqlOperator>());
                }

                //Register default operators
                //Numerics
                _operators[SparqlOperatorType.Add].Add(new AdditionOperator());
                _operators[SparqlOperatorType.Subtract].Add(new SubtractionOperator());
                _operators[SparqlOperatorType.Divide].Add(new DivisionOperator());
                _operators[SparqlOperatorType.Multiply].Add(new MultiplicationOperator());
                //Date Time
                _operators[SparqlOperatorType.Add].Add(new DateTimeAddition());
                _operators[SparqlOperatorType.Subtract].Add(new DateTimeSubtraction());
                //Time Span
                _operators[SparqlOperatorType.Add].Add(new TimeSpanAddition());
                _operators[SparqlOperatorType.Subtract].Add(new TimeSpanSubtraction());

                _init = true;
            }
        }

        /// <summary>
        /// Registers a new operator
        /// </summary>
        /// <param name="operator">Operator</param>
        public static void AddOperator(ISparqlOperator op)
        {
            if (!_init) Init();
            lock (_operators)
            {
                _operators[op.Operator].Add(op);
            }
        }

        /// <summary>
        /// Removes the registration of an operator
        /// </summary>
        /// <param name="operator">Operator</param>
        public static void RemoveOperand( ISparqlOperator op)
        {
            if (!_init) Init();
            lock (_operators)
            {
                _operators[op.Operator].Remove(op);
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
