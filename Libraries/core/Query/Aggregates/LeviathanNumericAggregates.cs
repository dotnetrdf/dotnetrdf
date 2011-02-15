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
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregates
{
    /// <summary>
    /// Class representing MEDIAN Aggregate Functions
    /// </summary>
    public class MedianAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new MEDIAN Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public MedianAggregate(VariableExpressionTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new MEDIAN Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public MedianAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new MEDIAN Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public MedianAggregate(VariableExpressionTerm expr, bool distinct)
            : base(expr, distinct)
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new MEDIAN Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifer applies</param>
        public MedianAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Median Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            if (this._varname != null)
            {
                //Ensured the MEDIANed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a MEDIAN Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            List<INode> values = new List<INode>();
            HashSet<INode> distinctValues = new HashSet<INode>();
            bool nullSeen = false;
            foreach (int id in bindingIDs)
            {
                try
                {
                    INode temp = this._expr.Value(context, id);
                    if (this._distinct)
                    {
                        if (temp != null)
                        {
                            if (distinctValues.Contains(temp))
                            {
                                continue;
                            }
                            else
                            {
                                distinctValues.Add(temp);
                            }
                        }
                        else if (!nullSeen)
                        {
                            nullSeen = false;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    values.Add(temp);
                }
                catch
                {
                    //Ignore errors
                }
            }

            if (values.Count == 0) return null;

            //Find the middle value and return
            values.Sort();
            int skip = values.Count / 2;
            return values.Skip(skip).First();
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.Median);
            output.Append(">(");
            if (this._distinct) output.Append("DISTINCT ");
            output.Append(this._expr.ToString());
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get 
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Median;
            }
        }
    }

    /// <summary>
    /// Class representing MODE Aggregate Functions
    /// </summary>
    public class ModeAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new MODE Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public ModeAggregate(VariableExpressionTerm expr)
            : base(expr, false) { }

        /// <summary>
        /// Creates a new MODE Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public ModeAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new MODE Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public ModeAggregate(VariableExpressionTerm expr, bool distinct)
            : base(expr, distinct)
        {
            this._varname = expr.ToString().Substring(1);
        }
        /// <summary>
        /// Creates a new MODE Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public ModeAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Mode Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            if (this._varname != null)
            {
                //Ensured the MODEd variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a MODE Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            Dictionary<INode, int> values = new Dictionary<INode, int>();
            int nullCount = 0;
            foreach (int id in bindingIDs)
            {
                try
                {
                    INode temp = this._expr.Value(context, id);
                    if (temp == null)
                    {
                        nullCount++;
                    }
                    else
                    {
                        if (values.ContainsKey(temp))
                        {
                            values[temp]++;
                        }
                        else
                        {
                            values.Add(temp, 1);
                        }
                    }
                }
                catch
                {
                    //Errors count as nulls
                    nullCount++;
                }
            }

            int mostPopular = values.Values.Max();
            if (mostPopular > nullCount)
            {
                return values.FirstOrDefault(p => p.Value == mostPopular).Key;
            }
            else
            {
                //Null is the most popular item
                return null;
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.Mode);
            output.Append(">(");
            if (this._distinct) output.Append("DISTINCT ");
            output.Append(this._expr.ToString());
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Mode;
            }
        }
    }

    /// <summary>
    /// Class representing NMAX Aggregate Functions
    /// </summary>
    /// <remarks>
    /// Only operates over numeric data which is typed to one of the supported SPARQL Numeric types (integers, decimals and doubles)
    /// </remarks>
    public class NumericMaxAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new NMAX Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public NumericMaxAggregate(VariableExpressionTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new NMAX Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public NumericMaxAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new NMAX Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public NumericMaxAggregate(VariableExpressionTerm expr, bool distinct)
            : base(expr, distinct)
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new NMAX Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public NumericMaxAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Numeric Max Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            if (this._varname != null)
            {
                //Ensured the MAXed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a NMAX Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            //Prep Variables
            long lngmax = 0;
            decimal decmax = 0.0m;
            float fltmax = 0.0f;
            double dblmax = 0.0d;
            SparqlNumericType maxtype = SparqlNumericType.NaN;
            if (!(this._expr is ISparqlNumericExpression)) throw new RdfQueryException("Cannot evaluate a NMAX aggregate over a non-numeric expression");
            ISparqlNumericExpression numExpr = (ISparqlNumericExpression)this._expr;
            SparqlNumericType numtype;

            foreach (int id in bindingIDs)
            {
                try
                {
                    numtype = numExpr.NumericType(context, id);
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
                    if (maxtype == SparqlNumericType.NaN)
                    {
                        //Initialise Maximums
                        switch (numtype)
                        {
                            case SparqlNumericType.Integer:
                                lngmax = numExpr.IntegerValue(context, id);
                                decmax = numExpr.DecimalValue(context, id);
                                fltmax = numExpr.FloatValue(context, id);
                                dblmax = numExpr.DoubleValue(context, id);
                                break;
                            case SparqlNumericType.Decimal:
                                decmax = numExpr.DecimalValue(context, id);
                                fltmax = numExpr.FloatValue(context, id);
                                dblmax = numExpr.DoubleValue(context, id);
                                break;
                            case SparqlNumericType.Float:
                                fltmax = numExpr.FloatValue(context, id);
                                dblmax = numExpr.FloatValue(context, id);
                                break;
                            case SparqlNumericType.Double:
                                dblmax = numExpr.DoubleValue(context, id);
                                break;
                        }
                        maxtype = numtype;
                        continue;
                    }
                    else
                    {
                        maxtype = numtype;
                    }
                }

                long lngval;
                decimal decval;
                float fltval;
                double dblval;
                switch (maxtype)
                {
                    case SparqlNumericType.Integer:
                        lngval = numExpr.IntegerValue(context, id);

                        if (lngval > lngmax)
                        {
                            lngmax = lngval;
                            decmax = numExpr.DecimalValue(context, id);
                            fltmax = numExpr.FloatValue(context, id);
                            dblmax = numExpr.DoubleValue(context, id);
                        }
                        break;
                    case SparqlNumericType.Decimal:
                        decval = numExpr.DecimalValue(context, id);

                        if (decval > decmax)
                        {
                            decmax = decval;
                            fltmax = numExpr.FloatValue(context, id);
                            dblmax = numExpr.DoubleValue(context, id);
                        }
                        break;
                    case SparqlNumericType.Float:
                        fltval = numExpr.FloatValue(context, id);

                        if (fltval > fltmax)
                        {
                            fltmax = fltval;
                            dblmax = numExpr.DoubleValue(context, id);
                        }
                        break;
                    case SparqlNumericType.Double:
                        dblval = numExpr.DoubleValue(context, id);

                        if (dblval > dblmax)
                        {
                            dblmax = dblval;
                        }
                        break;
                }
            }

            //Return the Max
            switch (maxtype)
            {
                case SparqlNumericType.NaN:
                    //No Numeric Values
                    return null;

                case SparqlNumericType.Integer:
                    //Integer Values
                    return new LiteralNode(null, lngmax.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

                case SparqlNumericType.Decimal:
                    //Decimal Values
                    return new LiteralNode(null, decmax.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));

                case SparqlNumericType.Float:
                    //Float values
                    return new LiteralNode(null, fltmax.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat));

                case SparqlNumericType.Double:
                    //Double Values
                    return new LiteralNode(null, dblmax.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));

                default:
                    throw new RdfQueryException("Failed to calculate a valid Maximum");
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.NumericMax);
            output.Append(">(");
            if (this._distinct) output.Append("DISTINCT ");
            output.Append(this._expr.ToString());
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.NumericMax;
            }
        }
    }

    /// <summary>
    /// Class representing NMIN Aggregate Functions
    /// </summary>
    /// <remarks>
    /// Only operates over numeric data which is typed to one of the supported SPARQL Numeric types (integers, decimals and doubles)
    /// </remarks>
    public class NumericMinAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new NMIN Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public NumericMinAggregate(VariableExpressionTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new NMIN Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public NumericMinAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new NMIN Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public NumericMinAggregate(VariableExpressionTerm expr, bool distinct)
            : base(expr, distinct)
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new NMIN Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public NumericMinAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Numeric Min Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            if (this._varname != null)
            {
                //Ensured the MINed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a NMIN Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            //Prep Variables
            long lngmin = 0;
            decimal decmin = 0.0m;
            float fltmin = 0.0f;
            double dblmin = 0.0d;
            SparqlNumericType mintype = SparqlNumericType.NaN;
            if (!(this._expr is ISparqlNumericExpression)) throw new RdfQueryException("Cannot evaluate a NMIN aggregate over a non-numeric expression");
            ISparqlNumericExpression numExpr = (ISparqlNumericExpression)this._expr;
            SparqlNumericType numtype;

            foreach (int id in bindingIDs)
            {
                try
                {
                    numtype = numExpr.NumericType(context, id);
                }
                catch
                {
                    continue;
                }

                //Skip if Not a Number
                if (numtype == SparqlNumericType.NaN) continue;

                //Track the Numeric Type
                if ((int)numtype > (int)mintype)
                {
                    if (mintype == SparqlNumericType.NaN)
                    {
                        //Initialise Minimums
                        switch (numtype)
                        {
                            case SparqlNumericType.Integer:
                                lngmin = numExpr.IntegerValue(context, id);
                                decmin = numExpr.DecimalValue(context, id);
                                fltmin = numExpr.FloatValue(context, id);
                                dblmin = numExpr.DoubleValue(context, id);
                                break;
                            case SparqlNumericType.Decimal:
                                decmin = numExpr.DecimalValue(context, id);
                                fltmin = numExpr.FloatValue(context, id);
                                dblmin = numExpr.DoubleValue(context, id);
                                break;
                            case SparqlNumericType.Float:
                                fltmin = numExpr.FloatValue(context, id);
                                dblmin = numExpr.DoubleValue(context, id);
                                break;
                            case SparqlNumericType.Double:
                                dblmin = numExpr.DoubleValue(context, id);
                                break;
                        }
                        mintype = numtype;
                        continue;
                    }
                    else
                    {
                        mintype = numtype;
                    }
                }

                long lngval;
                decimal decval;
                float fltval;
                double dblval;
                switch (mintype)
                {
                    case SparqlNumericType.Integer:
                        lngval = numExpr.IntegerValue(context, id);

                        if (lngval < lngmin)
                        {
                            lngmin = lngval;
                            decmin = numExpr.DecimalValue(context, id);
                            fltmin = numExpr.FloatValue(context, id);
                            dblmin = numExpr.DoubleValue(context, id);
                        }
                        break;
                    case SparqlNumericType.Decimal:
                        decval = numExpr.DecimalValue(context, id);

                        if (decval < decmin)
                        {
                            decmin = decval;
                            fltmin = numExpr.FloatValue(context, id);
                            dblmin = numExpr.DoubleValue(context, id);
                        }
                        break;
                    case SparqlNumericType.Float:
                        fltval = numExpr.FloatValue(context, id);

                        if (fltval < fltmin)
                        {
                            fltmin = fltval;
                            dblmin = numExpr.DoubleValue(context, id);
                        }
                        break;
                    case SparqlNumericType.Double:
                        dblval = numExpr.DoubleValue(context, id);

                        if (dblval < dblmin)
                        {
                            dblmin = dblval;
                        }
                        break;
                }
            }

            //Return the Min
            switch (mintype)
            {
                case SparqlNumericType.NaN:
                    //No Numeric Values
                    return null;

                case SparqlNumericType.Integer:
                    //Integer Values
                    return new LiteralNode(null, lngmin.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

                case SparqlNumericType.Decimal:
                    //Decimal Values
                    return new LiteralNode(null, decmin.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));

                case SparqlNumericType.Double:
                    //Double Values
                    return new LiteralNode(null, dblmin.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));

                default:
                    throw new RdfQueryException("Failed to calculate a valid Minimum");
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.NumericMin);
            output.Append(">(");
            if (this._distinct) output.Append("DISTINCT ");
            output.Append(this._expr.ToString());
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.NumericMin;
            }
        }
    }
}
