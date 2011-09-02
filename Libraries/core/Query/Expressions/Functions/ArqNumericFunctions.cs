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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents the ARQ max() function
    /// </summary>
    public class ArqMaxFunction : BaseBinaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new ARQ max() function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public ArqMaxFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        /// <summary>
        /// Gets the numeric value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._leftExpr is ISparqlNumericExpression && this._rightExpr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a, b;
                a = (ISparqlNumericExpression)this._leftExpr;
                b = (ISparqlNumericExpression)this._rightExpr;

                SparqlNumericType type = (SparqlNumericType)Math.Max((int)a.NumericType(context, bindingID), (int)b.NumericType(context, bindingID));

                switch (type)
                {
                    case SparqlNumericType.Integer:
                        return Math.Max(a.IntegerValue(context, bindingID), b.IntegerValue(context, bindingID));
                    case SparqlNumericType.Decimal:
                        return Math.Max(a.DecimalValue(context, bindingID), b.DecimalValue(context, bindingID));
                    case SparqlNumericType.Double:
                        return Math.Max(a.DoubleValue(context, bindingID),b.DoubleValue(context, bindingID));
                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the two sub-expressions are not Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Max + ">(" + this._leftExpr.ToString() + ", " + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function; 
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Max; 
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new ArqMaxFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Represents the ARQ min() function
    /// </summary>
    public class ArqMinFunction : BaseBinaryArithmeticExpression
    {
        /// <summary>
        /// Creates a new ARQ min() function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public ArqMinFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        /// <summary>
        /// Gets the numeric value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._leftExpr is ISparqlNumericExpression && this._rightExpr is ISparqlNumericExpression)
            {
                ISparqlNumericExpression a, b;
                a = (ISparqlNumericExpression)this._leftExpr;
                b = (ISparqlNumericExpression)this._rightExpr;

                SparqlNumericType type = (SparqlNumericType)Math.Max((int)a.NumericType(context, bindingID), (int)b.NumericType(context, bindingID));

                switch (type)
                {
                    case SparqlNumericType.Integer:
                        return Math.Min(a.IntegerValue(context, bindingID), b.IntegerValue(context, bindingID));
                    case SparqlNumericType.Decimal:
                        return Math.Min(a.DecimalValue(context, bindingID), b.DecimalValue(context, bindingID));
                    case SparqlNumericType.Double:
                        return Math.Min(a.DoubleValue(context, bindingID), b.DoubleValue(context, bindingID));
                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evalute an Arithmetic Expression where the two sub-expressions are not Numeric Expressions");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Min + ">(" + this._leftExpr.ToString() + ", " + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Min;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new ArqMinFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Represents the ARQ pi() function
    /// </summary>
    public class ArqPiFunction : NumericExpressionTerm
    {
        /// <summary>
        /// Creates a new ARQ Pi function
        /// </summary>
        public ArqPiFunction()
            : base(Math.PI) { }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Pi + ">()";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Pi;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return transformer.Transform(this);
        }
    }

    /// <summary>
    /// Represents the ARQ e() function
    /// </summary>
    public class ArqEFunction : NumericExpressionTerm
    {
        /// <summary>
        /// Creates a new ARQ e function
        /// </summary>
        public ArqEFunction()
            : base(Math.E) { }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.E + ">()";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.E;
            }
        }
    }
}
