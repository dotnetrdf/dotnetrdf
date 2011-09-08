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

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Abstract base class for Unary Expressions
    /// </summary>
    public abstract class BaseUnaryExpression : ISparqlExpression
    {
        /// <summary>
        /// The sub-expression of this Expression
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Base Unary Expression
        /// </summary>
        /// <param name="expr">Expression</param>
        public BaseUnaryExpression(ISparqlExpression expr)
        {
            this._expr = expr;
        }

        /// <summary>
        /// Gets the Value of a Sparql Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <remarks>
        /// If this method is not overridden in derived classes the value returned is the Effective Boolean Value as a Literal Node
        /// </remarks>
        public virtual INode Value(SparqlEvaluationContext context, int bindingID)
        {
            bool result = this.EffectiveBooleanValue(context, bindingID);
            return new LiteralNode(null, result.ToString().ToLower(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Gets the Effective Boolean Value of a Sparql Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <remarks>Must be implemented by derived classes</remarks>
        public virtual bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new NotImplementedException("Derived class does not implement an EffectiveBooleanValue method");
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets an enumeration of all the Variables used in this expression
        /// </summary>
        public virtual IEnumerable<String> Variables
        {
            get
            {
                return this._expr.Variables;
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public abstract SparqlExpressionType Type
        {
            get;
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public virtual IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._expr.AsEnumerable();
            }
        }

        /// <summary>
        /// Transforms the arguments of the expression using the given transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }

    /// <summary>
    /// Abstract base class for Binary Expressions
    /// </summary>
    public abstract class BaseBinaryExpression : ISparqlExpression
    {
        /// <summary>
        /// The sub-expressions of this Expression
        /// </summary>
        protected ISparqlExpression _leftExpr, _rightExpr;

        /// <summary>
        /// Creates a new Base Binary Expression
        /// </summary>
        /// <param name="leftExpr">Left Expression</param>
        /// <param name="rightExpr">Right Expression</param>
        public BaseBinaryExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr)
        {
            this._leftExpr = leftExpr;
            this._rightExpr = rightExpr;
        }

        /// <summary>
        /// Gets the Value of a Sparql Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <remarks>If this method is not overridden in derived classes the value returned is the Effective Boolean Value as a Literal Node</remarks>
        public virtual INode Value(SparqlEvaluationContext context, int bindingID)
        {
            bool result = this.EffectiveBooleanValue(context, bindingID);
            return new LiteralNode(null, result.ToString().ToLower(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Gets the Effective Boolean Value of a Sparql Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <remarks>Must be implemented by derived classes</remarks>
        public virtual bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new NotImplementedException("Derived class does not implement an EffectiveBooleanValue method");
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets an enumeration of all the Variables used in this expression
        /// </summary>
        public virtual IEnumerable<String> Variables
        {
            get
            {
                return this._leftExpr.Variables.Concat(this._rightExpr.Variables);
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public abstract SparqlExpressionType Type
        {
            get;
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return new ISparqlExpression[] { this._leftExpr, this._rightExpr };
            }
        }

        /// <summary>
        /// Transforms the arguments of the expression using the given transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }

    /// <summary>
    /// Abstract Base class for arithmetic expressions
    /// </summary>
    public abstract class BaseArithmeticExpression : ISparqlNumericExpression
    {
        /// <summary>
        /// Gets the Numeric Value of a Sparql Expression as an Integer as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public abstract object NumericValue(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the value of the Sparql Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual INode Value(SparqlEvaluationContext context, int bindingID)
        {
            Object result = this.NumericValue(context, bindingID);
            if (result is Double)
            {
                return new LiteralNode(null, result.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
            }
            else if (result is Single)
            {
                return new LiteralNode(null, result.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat));
            }
            else if (result is Decimal)
            {
                return new LiteralNode(null, result.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
            }
            else if (result is Int32 || result is Int64)
            {
                return new LiteralNode(null, result.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
            }
            else
            {
                throw new RdfQueryException("Value is not numeric");
            }
        }

        /// <summary>
        /// Gets the numeric type of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual SparqlNumericType NumericType(SparqlEvaluationContext context, int bindingID)
        {
            Object value = this.NumericValue(context, bindingID);
            if (value is Double)
            {
                return SparqlNumericType.Double;
            }
            else if (value is Single)
            {
                return SparqlNumericType.Float;
            }
            else if (value is Decimal)
            {
                return SparqlNumericType.Decimal;
            }
            else if (value is Int64 || value is Int32)
            {
                return SparqlNumericType.Integer;
            }
            else
            {
                throw new RdfQueryException("Value is not Numeric");
            }
        }

        /// <summary>
        /// Gets the Integer value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual long IntegerValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToInt64(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Gets the Decimal value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual decimal DecimalValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToDecimal(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Gets the Float value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual float FloatValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToSingle(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Gets the Double value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual double DoubleValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToDouble(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Gets the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            try
            {
                switch (this.NumericType(context, bindingID))
                {
                    case SparqlNumericType.Double:
                        double dblvalue = this.DoubleValue(context, bindingID);
                        return !(dblvalue == 0.0d);

                    case SparqlNumericType.Float:
                        float fvalue = this.FloatValue(context, bindingID);
                        return !(fvalue == 0.0f);

                    case SparqlNumericType.Decimal:
                        decimal decvalue = this.DecimalValue(context, bindingID);
                        return !(decvalue == 0);

                    case SparqlNumericType.Integer:
                        long intvalue = this.IntegerValue(context, bindingID);
                        return !(intvalue == 0);

                    default:
                        return false;
                }
            }
            catch (RdfQueryException)
            {
                //return false;
                return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
            }
        }

        /// <summary>
        /// Gets the Variable used in the Expression
        /// </summary>
        public abstract IEnumerable<String> Variables
        {
            get;
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public abstract SparqlExpressionType Type
        {
            get;
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public abstract IEnumerable<ISparqlExpression> Arguments
        {
            get;
        }

        /// <summary>
        /// Transforms the arguments of the expression using the given transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }

    /// <summary>
    /// Abstract Base Class for Unary Numeric Expressions
    /// </summary>
    public abstract class BaseUnaryArithmeticExpression : BaseArithmeticExpression
    {
        /// <summary>
        /// The sub-expression of this Expression
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Base Unary Arithmetic Expression
        /// </summary>
        /// <param name="expr">Sub-expression</param>
        public BaseUnaryArithmeticExpression(ISparqlExpression expr)
        {
            this._expr = expr;
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the enumeration of Variables used in this expression
        /// </summary>
        public override IEnumerable<String> Variables
        {
            get
            {
                return this._expr.Variables;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get 
            {
                return this._expr.AsEnumerable();
            }
        }
    }

    /// <summary>
    /// Abstract base class for Binary Arithmetic Expressions
    /// </summary>
    public abstract class BaseBinaryArithmeticExpression : BaseArithmeticExpression
    {
        /// <summary>
        /// The sub-expressions of this Expression
        /// </summary>
        protected ISparqlExpression _leftExpr, _rightExpr;

        /// <summary>
        /// Creates a new Base Binary Arithmetic Expression
        /// </summary>
        /// <param name="leftExpr"></param>
        /// <param name="rightExpr"></param>
        public BaseBinaryArithmeticExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr)
        {
            this._leftExpr = leftExpr;
            this._rightExpr = rightExpr;
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets an enumeration of all the Variables used in this expression
        /// </summary>
        public override IEnumerable<String> Variables
        {
            get
            {
                return this._leftExpr.Variables.Concat(this._rightExpr.Variables);
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get 
            {
                return new ISparqlExpression[] { this._leftExpr, this._rightExpr }; 
            }
        }
    }
}