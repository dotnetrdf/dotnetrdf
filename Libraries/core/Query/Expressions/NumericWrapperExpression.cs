using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions
{
    public class NumericWrapperExpression
        : BaseUnaryArithmeticExpression
    {
        public NumericWrapperExpression(ISparqlExpression expr)
            : base(expr) { }
        
        public override string ToString()
        {
            return this._expr.ToString();
        }

        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            INode value = this._expr.Value(context, bindingID);
            if (value != null)
            {
                if (value.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)value;
                    if (!lit.Language.Equals(String.Empty))
                    {
                        //If there's a Language Tag implied type is string so no numeric value
                        throw new RdfQueryException("Cannot calculate the Numeric Value of literal with a language specifier");
                    }
                    else if (lit.DataType == null)
                    {
                        throw new RdfQueryException("Cannot calculate the Numeric Value of an untyped Literal");
                    }
                    else
                    {
                        try
                        {
                            switch (SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(lit.DataType))
                            {
                                case SparqlNumericType.Decimal:
                                    return Decimal.Parse(lit.Value);
                                case SparqlNumericType.Double:
                                    return Double.Parse(lit.Value);
                                case SparqlNumericType.Float:
                                    return Single.Parse(lit.Value);
                                case SparqlNumericType.Integer:
                                    return Int64.Parse(lit.Value);
                                case SparqlNumericType.NaN:
                                default:
                                    throw new RdfQueryException("Cannot calculate the Numeric Value of a literal since its Data Type URI does not correspond to a Data Type URI recognised as a Numeric Type in the SPARQL Specification");
                            }
                        }
                        catch (FormatException fEx)
                        {
                            throw new RdfQueryException("Cannot calculate the Numeric Value of a Literal since the Value contained is not a valid value in it's given Data Type", fEx);
                        }
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot evaluate a numeric expression when the inner expression evaluates to a non-literal node");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evaluate a numeric expression when the inner expression evaluates to null");
            }
        }

        public override SparqlExpressionType Type
        {
            get 
            {
                return this._expr.Type; 
            }
        }

        public override string Functor
        {
            get 
            {
                return this._expr.Functor;
            }
        }

        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new NumericWrapperExpression(transformer.Transform(this._expr));
        }
    }
}
