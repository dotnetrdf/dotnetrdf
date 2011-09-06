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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents the SPARQL ABS() Function
    /// </summary>
    public class AbsFunction 
        : XPathAbsoluteFunction
    {
        /// <summary>
        /// Creates a new SPARQL ABS() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public AbsFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordAbs;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordAbs + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new AbsFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL CEIL() Function
    /// </summary>
    public class CeilFunction 
        : XPathCeilingFunction
    {
        /// <summary>
        /// Creates a new SPARQL CEIL() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public CeilFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCeil;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordCeil + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new CeilFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL FLOOR() Function
    /// </summary>
    public class FloorFunction
        : XPathFloorFunction
    {
        /// <summary>
        /// Creates a new SPARQL FLOOR() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public FloorFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordFloor;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordFloor + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new FloorFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL RAND() Function
    /// </summary>
    public class RandFunction
        : BaseArithmeticExpression
    {
        private Random _rnd = new Random();

        /// <summary>
        /// Creates a new SPARQL RAND() Function
        /// </summary>
        public RandFunction()
            : base() { }

        /// <summary>
        /// Gets the Numeric Value of the Expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            return this._rnd.NextDouble();
        }

        /// <summary>
        /// Gets the Variables used in this Expression
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get 
            {
                return Enumerable.Empty<String>(); 
            }
        }

        /// <summary>
        /// Gets the Type of this Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function; 
            }
        }

        /// <summary>
        /// Gets the Arguments of this Expression
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get 
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRand;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordRand + "()";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }

    /// <summary>
    /// Represents the SPARQL ROUND() Function
    /// </summary>
    public class RoundFunction 
        : XPathRoundFunction
    {
        /// <summary>
        /// Creates a new SPARQL ROUND() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public RoundFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRound;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordRound + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new RoundFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the SPARQL ISNUMERIC() Function
    /// </summary>
    public class IsNumericFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new SPARQL ISNUMERIC() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public IsNumericFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Determines the Effective Boolean Value of the expression for the given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            try
            {
                //While we could use NumericType or NumericValue for Numeric Expressions we can't guarantee that
                //this would work properly

                INode temp = this._expr.Value(context, bindingID);
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;

                    //No DatatType means not numeric
                    if (lit.DataType == null) return false;

                    //Get the Numeric Type from the DataType URI
                    SparqlNumericType type = SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(lit.DataType);

                    //Now check the lexical value
                    switch (type)
                    {
                        case SparqlNumericType.Decimal:
                            //Decimal - just regex on lexical form
                            return SparqlSpecsHelper.IsDecimal(lit.Value);

                        case SparqlNumericType.Double:
                        case SparqlNumericType.Float:
                            //Double/Float just regex on lexical form
                            return SparqlSpecsHelper.IsDouble(lit.Value);

                        case SparqlNumericType.Integer:
                            //Integer Type so could be any of the supported types
                            switch (lit.DataType.ToString())
                            {
                                case XmlSpecsHelper.XmlSchemaDataTypeByte:
                                    //Byte - have to try parsing it
                                    SByte sb;
                                    return SByte.TryParse(lit.Value, out sb);

                                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte:
                                    //Unsigned Byte - have to try parsing it
                                    Byte b;
                                    return Byte.TryParse(lit.Value, out b) && b >= 0;

                                case XmlSpecsHelper.XmlSchemaDataTypeInt:
                                case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                                case XmlSpecsHelper.XmlSchemaDataTypeLong:
                                case XmlSpecsHelper.XmlSchemaDataTypeShort:
                                    //Standard Integer - can just regex on its lexical form
                                    return SparqlSpecsHelper.IsInteger(lit.Value);

                                case XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger:
                                case XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger:
                                    //Negative Integer - can just regex on its lexical form
                                    //plus ensure that the value starts with a -
                                    return lit.Value.StartsWith("-") && SparqlSpecsHelper.IsInteger(lit.Value);

                                case XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger:
                                case XmlSpecsHelper.XmlSchemaDataTypePositiveInteger:
                                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt:
                                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong:
                                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort:
                                    //Positive Integer - can just regex on its lexical form
                                    //plus ensure that the value doesn't start with a -
                                    return !lit.Value.StartsWith("-") && SparqlSpecsHelper.IsInteger(lit.Value);

                                default:
                                    //Otherwise not numeric
                                    return false;
                            }

                        default:
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            } 
            catch (RdfQueryException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Type of this Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordIsNumeric; 
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordIsNumeric + "(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IsNumericFunction(transformer.Transform(this._expr));
        }
    }
}
