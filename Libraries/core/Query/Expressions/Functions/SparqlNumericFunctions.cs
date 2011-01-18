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
    public class AbsFunction : XPathAbsoluteFunction
    {
        public AbsFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordAbs;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordAbs + "(" + this._expr.ToString() + ")";
        }
    }

    public class CeilFunction : XPathCeilingFunction
    {
        public CeilFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCeil;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordCeil + "(" + this._expr.ToString() + ")";
        }
    }

    public class FloorFunction : XPathFloorFunction
    {
        public FloorFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordFloor;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordFloor + "(" + this._expr.ToString() + ")";
        }
    }

    public class RoundFunction : XPathRoundFunction
    {
        public RoundFunction(ISparqlExpression expr)
            : base(expr) { }

        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRound;
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordRound + "(" + this._expr.ToString() + ")";
        }
    }

    public class IsNumericFunction : BaseUnaryExpression
    {
        public IsNumericFunction(ISparqlExpression expr)
            : base(expr) { }

        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            try
            {
                //While we could use NumericType or NumericValue for Numeric Expressions we can't guarantee that
                //this would work properly

                INode temp = this._expr.Value(context, bindingID);
                if (temp.NodeType == NodeType.Literal)
                {
                    LiteralNode lit = (LiteralNode)temp;

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

        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function;
            }
        }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordIsNumeric; 
            }
        }

        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordIsNumeric + "(" + this._expr.ToString() + ")";
        }
    }
}
