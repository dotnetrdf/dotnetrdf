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
    /// Class representing AVG Aggregate Functions
    /// </summary>
    public class AverageAggregate
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new AVG Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public AverageAggregate(VariableTerm expr, bool distinct)
            : base(expr, distinct)
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new AVG Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public AverageAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Creates a new AVG Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public AverageAggregate(VariableTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new AVG Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public AverageAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Applies the Average Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            if (this._varname != null)
            {
                //Ensured the AVGed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a AVG Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            //Prep Variables
            HashSet<IValuedNode> values = new HashSet<IValuedNode>();
            int count = 0;
            //long lngtotal = 0;
            decimal dectotal = 0.0m;
            float flttotal = 0.0f;
            double dbltotal = 0.0d;
            SparqlNumericType maxtype = SparqlNumericType.NaN;
            SparqlNumericType numtype;

            foreach (int id in bindingIDs)
            {
                IValuedNode temp;
                try
                {
                    temp = this._expr.Evaluate(context, id);
                    if (temp == null) return null;
                    //Apply DISTINCT modifier if required
                    if (this._distinct)
                    {
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
                    //SPARQL Working Group changed spec so this should now return no binding
                    return null;
                }

                //No result if anything resolves to non-numeric
                if (numtype == SparqlNumericType.NaN) return null;

                //Track the Numeric Type
                if ((int)numtype > (int)maxtype)
                {
                    maxtype = numtype;
                }

                //Increment the Totals based on the current Numeric Type
                switch (maxtype)
                {
                    case SparqlNumericType.Integer:
                        //lngtotal += numExpr.IntegerValue(context, id);
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

                count++;
            }

            //Calculate the Average
            if (count == 0)
            {
                return new LongNode(null, 0);
            }
            else
            {
                //long lngavg;
                decimal decavg;
                float fltavg;
                double dblavg;

                switch (maxtype)
                {
                    case SparqlNumericType.Integer:
                    ////Integer Values
                    //lngavg = lngtotal / (long)count;
                    //return new LiteralNode(null, lngavg.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

                    case SparqlNumericType.Decimal:
                        //Decimal Values
                        decavg = dectotal / (decimal)count;
                        return new DecimalNode(null, decavg);

                    case SparqlNumericType.Float:
                        //Float values
                        fltavg = flttotal / (float)count;
                        return new FloatNode(null, fltavg);

                    case SparqlNumericType.Double:
                        //Double Values
                        dblavg = dbltotal / (double)count;
                        return new DoubleNode(null, dblavg);

                    default:
                        throw new RdfQueryException("Failed to calculate a valid Average");
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("AVG(");
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
                return SparqlSpecsHelper.SparqlKeywordAvg;
            }
        }
    }
}
