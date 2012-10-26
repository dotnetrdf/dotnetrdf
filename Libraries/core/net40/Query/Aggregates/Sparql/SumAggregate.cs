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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Aggregates.Sparql
{
    /// <summary>
    /// Class representing SUM Aggregate Functions
    /// </summary>
    public class SumAggregate
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new SUM Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public SumAggregate(VariableTerm expr, bool distinct)
            : base(expr, distinct)
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new SUM Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public SumAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Creates a new SUM Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public SumAggregate(VariableTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new SUM Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public SumAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Applies the Sum Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            if (this._varname != null)
            {
                //Ensured the SUMmed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a SUM Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            //Prep Variables
            long lngtotal = 0;
            decimal dectotal = 0.0m;
            float flttotal = 0.0f;
            double dbltotal = 0.0d;
            SparqlNumericType maxtype = SparqlNumericType.NaN;
            SparqlNumericType numtype;
            HashSet<IValuedNode> values = new HashSet<IValuedNode>();

            foreach (int id in bindingIDs)
            {
                IValuedNode temp;
                try
                {
                    temp = this._expr.Evaluate(context, id);
                    if (this._distinct)
                    {
                        
                        if (temp == null) continue;
                        if (values.Contains(temp))
                        {
                            continue;
                        }
                        else
                        {
                            values.Add(temp);
                        }
                    }
                    numtype = temp.NumericType;
                }
                catch
                {
                    continue;
                }

                //Skip if Not a Number
                if (numtype == SparqlNumericType.NaN) continue;

                //Track the Numeric Type
                if ((int)numtype > (int)maxtype)
                {
                    maxtype = numtype;
                }

                //Increment the Totals based on the current Numeric Type
                switch (maxtype)
                {
                    case SparqlNumericType.Integer:
                        lngtotal += temp.AsInteger();
                        dectotal += temp.AsDecimal();
                        flttotal += temp.AsFloat();
                        dbltotal += temp.AsDouble();
                        break;
                    case SparqlNumericType.Decimal:
                        dectotal += temp.AsDecimal();
                        flttotal += temp.AsFloat();
                        dbltotal += temp.AsDouble();
                        break;
                    case SparqlNumericType.Float:
                        flttotal += temp.AsFloat();
                        dbltotal += temp.AsDouble();
                        break;
                    case SparqlNumericType.Double:
                        dbltotal += temp.AsDouble();
                        break;
                }
            }

            //Return the Sum
            switch (maxtype)
            {
                case SparqlNumericType.NaN:
                    //No Numeric Values
                    return new LongNode(0);

                case SparqlNumericType.Integer:
                    //Integer Values
                    return new LongNode(lngtotal);

                case SparqlNumericType.Decimal:
                    //Decimal Values
                    return new DecimalNode(dectotal);

                case SparqlNumericType.Float:
                    //Float Values
                    return new FloatNode(flttotal);

                case SparqlNumericType.Double:
                    //Double Values
                    return new DoubleNode(dbltotal);

                default:
                    throw new RdfQueryException("Failed to calculate a valid Sum");
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("SUM(");
            if (this._distinct) output.Append("DISTINCT ");
            output.Append(this._expr.ToString() + ")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSum;
            }
        }
    }
}
