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
using VDS.RDF.Query.Expressions.Functions;

namespace VDS.RDF.Query.Aggregates
{
    /// <summary>
    /// Class representing AVG Aggregate Functions
    /// </summary>
    public class AverageAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new AVG Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public AverageAggregate(VariableExpressionTerm expr, bool distinct)
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
        public AverageAggregate(VariableExpressionTerm expr)
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
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
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
            HashSet<INode> values = new HashSet<INode>();
            int count = 0;
            //long lngtotal = 0;
            decimal dectotal = 0.0m;
            float flttotal = 0.0f;
            double dbltotal = 0.0d;
            SparqlNumericType maxtype = SparqlNumericType.NaN;
            ISparqlNumericExpression numExpr;
            if (!(this._expr is ISparqlNumericExpression))
            {
                numExpr = new NumericWrapperExpression(this._expr);
                //throw new RdfQueryException("Cannot calculate an average aggregate over a non-numeric expression");
            }
            else
            {
                numExpr = (ISparqlNumericExpression)this._expr;
            }
            SparqlNumericType numtype;

            foreach (int id in bindingIDs)
            {
                try
                {
                    //Apply DISTINCT modifier if required
                    if (this._distinct)
                    {
                        INode temp = this._expr.Value(context, id);
                        if (temp == null) return null;
                        if (values.Contains(temp))
                        {
                            continue;
                        }
                        else
                        {
                            values.Add(temp);
                        }
                    }
                    numtype = numExpr.NumericType(context, id);
                }
                catch
                {
                    //SPARQL Working Group changed spec so this should now return no binding
                    return null;
                }

                //Skip if Not a Number
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
                        dectotal += numExpr.DecimalValue(context, id);
                        flttotal += numExpr.FloatValue(context, id);
                        dbltotal += numExpr.DoubleValue(context, id);
                        break;
                    case SparqlNumericType.Decimal:
                        dectotal += numExpr.DecimalValue(context, id);
                        flttotal += numExpr.FloatValue(context, id);
                        dbltotal += numExpr.DoubleValue(context, id);
                        break;
                    case SparqlNumericType.Float:
                        flttotal += numExpr.FloatValue(context, id);
                        dbltotal += numExpr.DoubleValue(context, id);
                        break;
                    case SparqlNumericType.Double:
                        dbltotal += numExpr.DoubleValue(context, id);
                        break;
                }

                count++;
            }

            //Calculate the Average
            if (count == 0)
            {
                return new LiteralNode(null, "0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            }
            else
            {
                //long lngavg;
                decimal decavg;
                float fltavg;
                double dblavg;

                switch (maxtype)
                {
                    case SparqlNumericType.NaN:
                        //No Numeric Values
                        return new LiteralNode(null, "0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

                    case SparqlNumericType.Integer:
                        ////Integer Values
                        //lngavg = lngtotal / (long)count;
                        //return new LiteralNode(null, lngavg.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

                    case SparqlNumericType.Decimal:
                        //Decimal Values
                        decavg = dectotal / (decimal)count;
                        return new LiteralNode(null, decavg.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));

                    case SparqlNumericType.Float:
                        //Float values
                        fltavg = flttotal / (float)count;
                        return new LiteralNode(null, fltavg.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat));

                    case SparqlNumericType.Double:
                        //Double Values
                        dblavg = dbltotal / (double)count;
                        return new LiteralNode(null, dblavg.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));

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

    /// <summary>
    /// Class representing COUNT Aggregate Function
    /// </summary>
    public class CountAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new COUNT Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public CountAggregate(VariableExpressionTerm expr)
            : base(expr) 
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new Count Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public CountAggregate(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Counts the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            int c = 0;
            if (this._varname != null)
            {
                //Ensure the COUNTed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a COUNT Aggregate since the Variable does not occur in a Graph Pattern");
                }


                //Just Count the number of results where the variable is bound
                VariableExpressionTerm varExpr = (VariableExpressionTerm)this._expr;
                foreach (int id in bindingIDs)
                {
                    if (varExpr.Value(context, id) != null) c++;
                }
            }
            else
            {
                //Count the number of results where the result in not null/error
                foreach (int id in bindingIDs)
                {
                    try
                    {
                        if (this._expr.Value(context, id) != null) c++;
                    }
                    catch
                    {
                        //Ignore errors
                    }
                }
            }


            LiteralNode count = new LiteralNode(null, c.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            return count;
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("COUNT(" + this._expr.ToString() + ")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordCount;
            }
        }
    }

    /// <summary>
    /// Class representing COUNT(DISTINCT ?x) Aggregate Function
    /// </summary>
    public class CountDistinctAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new COUNT(DISTINCT ?x) Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public CountDistinctAggregate(VariableExpressionTerm expr)
            : base(expr)
        {
            this._varname = expr.ToString().Substring(1);
        }


        /// <summary>
        /// Creates a new COUNT DISTINCT Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public CountDistinctAggregate(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Counts the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            int c;
            List <INode> values = new List<INode>();

            if (this._varname != null)
            {
                //Ensure the COUNTed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a COUNT Aggregate since the Variable does not occur in a Graph Pattern");
                }

                //Just Count the number of results where the variable is bound
                VariableExpressionTerm varExpr = (VariableExpressionTerm)this._expr;

                foreach (int id in bindingIDs)
                {
                    INode temp = varExpr.Value(context, id);
                    if (temp != null)
                    {
                        values.Add(temp);
                    }
                }
                c = values.Distinct().Count();
            }
            else
            {
                //Count the distinct non-null results
                foreach (int id in bindingIDs)
                {
                    try
                    {
                        INode temp = this._expr.Value(context, id);
                        if (temp != null)
                        {
                            values.Add(temp);
                        }
                    }
                    catch
                    {
                        //Ignore errors
                    }
                }
                c = values.Distinct().Count();
            }

            LiteralNode count = new LiteralNode(null, c.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            return count;
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("COUNT(" + this._expr.ToString() + ")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCount;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Aggregate
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return new ISparqlExpression[] { new DistinctModifierExpression() }.Concat(base.Arguments);
            }
        }
    }

    /// <summary>
    /// Class representing COUNT(*) Aggregate Function
    /// </summary>
    /// <remarks>
    /// Differs from a COUNT in that it justs counts rows in the results
    /// </remarks>
    public class CountAllAggregate : BaseAggregate
    {
        /// <summary>
        /// Creates a new COUNT(*) Aggregate
        /// </summary>
        public CountAllAggregate()
            : base(null)
        {
        }

        /// <summary>
        /// Counts the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            //Just Count the number of results
            int c = bindingIDs.Count();
            LiteralNode count = new LiteralNode(null, c.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            return count;
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "COUNT(*)";
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCount;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Aggregate
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return new ISparqlExpression[] { new AllModifierExpression() };
            }
        }
    }

    /// <summary>
    /// Class representing COUNT(DISTINCT *) Aggregate Function
    /// </summary>
    public class CountAllDistinctAggregate : BaseAggregate
    {
        /// <summary>
        /// Creates a new COUNT(DISTINCT*) Aggregate
        /// </summary>
        public CountAllDistinctAggregate()
            : base(null)
        {
        }

        /// <summary>
        /// Counts the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            int c = context.InputMultiset.Sets.Distinct().Count();

            LiteralNode count = new LiteralNode(null, c.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            return count;
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "COUNT(DISTINCT *)";
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCount;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Aggregate
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return new ISparqlExpression[] { new DistinctModifierExpression(), new AllModifierExpression() };
            }
        }
    }

    /// <summary>
    /// Class representing GROUP_CONCAT Aggregate
    /// </summary>
    public class GroupConcatAggregate : XPathStringJoinFunction
    {
        private bool _customSeparator = false;

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Should a distinct modifer be applied</param>
        public GroupConcatAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, new NodeExpressionTerm(new LiteralNode(null, " "))) 
        {
            this._distinct = distinct;
        }

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public GroupConcatAggregate(ISparqlExpression expr)
            : base(expr, new NodeExpressionTerm(new LiteralNode(null, " "))) { }

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="sepExpr">Separator Expression</param>
        /// <param name="distinct">Should a distinct modifer be applied</param>
        public GroupConcatAggregate(ISparqlExpression expr, ISparqlExpression sepExpr, bool distinct)
            : base(expr, sepExpr)
        {
            this._distinct = distinct;
            this._customSeparator = true;
        }

        /// <summary>
        /// Creates a new GROUP_CONCAT aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="sepExpr">Separator Expression</param>
        public GroupConcatAggregate(ISparqlExpression expr, ISparqlExpression sepExpr)
            : base(expr, sepExpr) 
        {
            this._customSeparator = true;
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("GROUP_CONCAT(");
            if (this._distinct) output.Append("DISTINCT ");
            if (this._expr is XPathConcatFunction)
            {
                XPathConcatFunction concatFunc = (XPathConcatFunction)this._expr;
                for (int i = 0; i < concatFunc.Arguments.Count(); i++)
                {
                    output.Append(concatFunc.Arguments.Skip(i).First().ToString());
                    if (i < concatFunc.Arguments.Count() - 1)
                    {
                        output.Append(", ");
                    }
                }
            }
            else
            {
                output.Append(this._expr.ToString());
            }
            if (this._customSeparator)
            {
                output.Append(" ; SEPARATOR = ");
                output.Append(this._sep.ToString());
            }
            output.Append(")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordGroupConcat;
            }
        }
    }

    /// <summary>
    /// Class representing MAX Aggregate Functions
    /// </summary>
    public class MaxAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new MAX Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public MaxAggregate(VariableExpressionTerm expr, bool distinct)
            : base(expr, distinct)
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new MAX Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public MaxAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Creates a new MAX Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public MaxAggregate(VariableExpressionTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new MAX Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public MaxAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new MAX Aggregate
        /// </summary>
        /// <param name="distinct">Distinct Modifier</param>
        /// <param name="expr">Expression</param>
        public MaxAggregate(ISparqlExpression distinct, ISparqlExpression expr)
            : base(expr)
        {
            if (distinct is DistinctModifierExpression)
            {
                this._distinct = true;
            }
            else
            {
                throw new RdfQueryException("The first argument to the MaxAggregate constructor must be of type DistinctModifierExpression");
            }
        }

        /// <summary>
        /// Applies the Max Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            List<INode> values = new List<INode>();

            if (this._varname != null)
            {
                //Ensured the MAXed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(this._varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a MAX Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            foreach (int id in bindingIDs)
            {
                try
                {
                    values.Add(this._expr.Value(context, id));
                }
                catch
                {
                    //Ignore errors
                }
            }

            values.Sort(new SparqlOrderingComparer());
            values.Reverse();
            return values.FirstOrDefault();
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("MAX(");
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
                return SparqlSpecsHelper.SparqlKeywordMax;
            }
        }
    }

    /// <summary>
    /// Class representing MIN Aggregate Functions
    /// </summary>
    public class MinAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new MIN Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public MinAggregate(VariableExpressionTerm expr, bool distinct)
            : base(expr, distinct)
        {
            this._varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new MIN Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public MinAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Creates a new MIN Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public MinAggregate(VariableExpressionTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new MIN Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public MinAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Applies the Min Aggregate function to the results
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
                    throw new RdfQueryException("Cannot use the Variable " + this._expr.ToString() + " in a MIN Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            List<INode> values = new List<INode>();
            foreach (int id in bindingIDs)
            {
                try
                {
                    values.Add(this._expr.Value(context, id));
                }
                catch
                {
                    //Ignore errors
                }
            }

            values.Sort(new SparqlOrderingComparer());
            return values.FirstOrDefault();
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("MIN(");
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
                return SparqlSpecsHelper.SparqlKeywordMin;
            }
        }
    }

    /// <summary>
    /// Class representing the SAMPLE aggregate
    /// </summary>
    public class SampleAggregate : BaseAggregate
    {
        /// <summary>
        /// Creates a new SAMPLE Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public SampleAggregate(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Applies the SAMPLE Aggregate
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            //Try the expression with each member of the Group until we find a non-null
            foreach (int id in bindingIDs)
            {
                try
                {

                    //First non-null result we find is returned
                    INode temp = this._expr.Value(context, id);
                    if (temp != null) return temp;
                }
                catch (RdfQueryException)
                {
                    //Ignore errors - we'll loop round and try the next
                }
            }

            //If the Group is Empty of the Expression fails to evaluate for the entire Group then the result is null
            return null;
        }

        /// <summary>
        /// Gets the String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._distinct)
            {
                return "SAMPLE(DISTINCT " + this._expr.ToString() + ")";
            }
            else
            {
                return "SAMPLE(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSample;
            }
        }
    }

    /// <summary>
    /// Class representing SUM Aggregate Functions
    /// </summary>
    public class SumAggregate : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new SUM Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public SumAggregate(VariableExpressionTerm expr, bool distinct)
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
        public SumAggregate(VariableExpressionTerm expr)
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
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
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
            if (!(this._expr is ISparqlNumericExpression)) throw new RdfQueryException("Cannot calculate an sum aggregate over a non-numeric expression");
            ISparqlNumericExpression numExpr = (ISparqlNumericExpression)this._expr;
            SparqlNumericType numtype;
            HashSet<INode> values = new HashSet<INode>();

            foreach (int id in bindingIDs)
            {
                try
                {
                    if (this._distinct)
                    {
                        INode temp = this._expr.Value(context, id);
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
                    maxtype = numtype;
                }

                //Increment the Totals based on the current Numeric Type
                switch (maxtype)
                {
                    case SparqlNumericType.Integer:
                        lngtotal += numExpr.IntegerValue(context, id);
                        dectotal += numExpr.DecimalValue(context, id);
                        flttotal += numExpr.FloatValue(context, id);
                        dbltotal += numExpr.DoubleValue(context, id);
                        break;
                    case SparqlNumericType.Decimal:
                        dectotal += numExpr.DecimalValue(context, id);
                        flttotal += numExpr.FloatValue(context, id);
                        dbltotal += numExpr.DoubleValue(context, id);
                        break;
                    case SparqlNumericType.Float:
                        flttotal += numExpr.FloatValue(context, id);
                        dbltotal += numExpr.DoubleValue(context, id);
                        break;
                    case SparqlNumericType.Double:
                        dbltotal += numExpr.DoubleValue(context, id);
                        break;
                }
            }

            //Return the Sum
            switch (maxtype)
            {
                case SparqlNumericType.NaN:
                    //No Numeric Values
                    return new LiteralNode(null, "0", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

                case SparqlNumericType.Integer:
                    //Integer Values
                    return new LiteralNode(null, lngtotal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

                case SparqlNumericType.Decimal:
                    //Decimal Values
                    return new LiteralNode(null, dectotal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));

                case SparqlNumericType.Float:
                    //Float Values
                    return new LiteralNode(null, flttotal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat));

                case SparqlNumericType.Double:
                    //Double Values
                    return new LiteralNode(null, dbltotal.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));

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