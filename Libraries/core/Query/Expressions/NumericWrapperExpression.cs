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

namespace VDS.RDF.Query.Expressions
{
    class NumericWrapperExpression
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
