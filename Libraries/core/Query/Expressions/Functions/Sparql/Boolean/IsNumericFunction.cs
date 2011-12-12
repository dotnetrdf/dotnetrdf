using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
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

        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            try
            {
                //TODO: This new approach relies on AsValuedNode() correctly validating numeric types which it doesn't yet do for all types here
                IValuedNode result = this._expr.Evaluate(context, bindingID);
                return new BooleanNode(null, result.NumericType != SparqlNumericType.NaN);

                ////While we could use NumericType or NumericValue for Numeric Expressions we can't guarantee that
                ////this would work properly

                //INode temp = this._expr.Value(context, bindingID);
                //if (temp.NodeType == NodeType.Literal)
                //{
                //    ILiteralNode lit = (ILiteralNode)temp;

                //    //No DatatType means not numeric
                //    if (lit.DataType == null) return false;

                //    //Get the Numeric Type from the DataType URI
                //    SparqlNumericType type = SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(lit.DataType);

                //    //Now check the lexical value
                //    switch (type)
                //    {
                //        case SparqlNumericType.Decimal:
                //            //Decimal - just regex on lexical form
                //            return SparqlSpecsHelper.IsDecimal(lit.Value);

                //        case SparqlNumericType.Double:
                //        case SparqlNumericType.Float:
                //            //Double/Float just regex on lexical form
                //            return SparqlSpecsHelper.IsDouble(lit.Value);

                //        case SparqlNumericType.Integer:
                //            //Integer Type so could be any of the supported types
                //            switch (lit.DataType.ToString())
                //            {
                //                case XmlSpecsHelper.XmlSchemaDataTypeByte:
                //                    //Byte - have to try parsing it
                //                    SByte sb;
                //                    return SByte.TryParse(lit.Value, out sb);

                //                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte:
                //                    //Unsigned Byte - have to try parsing it
                //                    Byte b;
                //                    return Byte.TryParse(lit.Value, out b) && b >= 0;

                //                case XmlSpecsHelper.XmlSchemaDataTypeInt:
                //                case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                //                case XmlSpecsHelper.XmlSchemaDataTypeLong:
                //                case XmlSpecsHelper.XmlSchemaDataTypeShort:
                //                    //Standard Integer - can just regex on its lexical form
                //                    return SparqlSpecsHelper.IsInteger(lit.Value);

                //                case XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger:
                //                case XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger:
                //                    //Negative Integer - can just regex on its lexical form
                //                    //plus ensure that the value starts with a -
                //                    return lit.Value.StartsWith("-") && SparqlSpecsHelper.IsInteger(lit.Value);

                //                case XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger:
                //                case XmlSpecsHelper.XmlSchemaDataTypePositiveInteger:
                //                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt:
                //                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong:
                //                case XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort:
                //                    //Positive Integer - can just regex on its lexical form
                //                    //plus ensure that the value doesn't start with a -
                //                    return !lit.Value.StartsWith("-") && SparqlSpecsHelper.IsInteger(lit.Value);

                //                default:
                //                    //Otherwise not numeric
                //                    return false;
                //            }

                //        default:
                //            return false;
                //    }
                //}
                //else
                //{
                //    return false;
                //}
            }
            catch (RdfQueryException)
            {
                return new BooleanNode(null, false);
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
