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
using VDS.RDF.Query.Aggregates;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Class for representing the Distinct Modifier
    /// </summary>
    public class DistinctModifierExpression : ISparqlExpression
    {
        /// <summary>
        /// Throws a <see cref="NotImplementedException">NotImplementedException</see> since this class is a placeholder and only used in parsing
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            throw new NotImplementedException("This class is a placeholder only - aggregates taking this as an argument should apply a DISTINCT modifer");
        }

        /// <summary>
        /// Throws a <see cref="NotImplementedException">NotImplementedException</see> since this class is a placeholder and only used in parsing
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new NotImplementedException("This class is a placeholder only - aggregates taking this as an argument should apply a DISTINCT modifer");
        }

        /// <summary>
        /// Returns an empty enumerable
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return Enumerable.Empty<String>(); 
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Primary;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordDistinct;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "DISTINCT";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }

    /// <summary>
    /// Class for representing the All Modifier
    /// </summary>
    public class AllModifierExpression : ISparqlExpression
    {
        /// <summary>
        /// Throws a <see cref="NotImplementedException">NotImplementedException</see> since this class is a placeholder and only used in parsing
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            throw new NotImplementedException("This class is a placeholder only - aggregates taking this as an argument should apply over all rows");
        }

        /// <summary>
        /// Throws a <see cref="NotImplementedException">NotImplementedException</see> since this class is a placeholder and only used in parsing
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new NotImplementedException("This class is a placeholder only - aggregates taking this as an argument should apply over all rows");
        }

        /// <summary>
        /// Returns an empty enumerable
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Primary;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get
            {
                return "*";
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string  ToString()
        {
 	        return "*";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }

    /// <summary>
    /// Class for representing Aggregate Expressions which have Numeric Results
    /// </summary>
    public class AggregateExpressionTerm : BaseUnaryArithmeticExpression
    {
        private ISparqlAggregate _aggregate;

        /// <summary>
        /// Creates a new Aggregate Expression Term that uses the given Aggregate
        /// </summary>
        /// <param name="aggregate">Aggregate</param>
        public AggregateExpressionTerm(ISparqlAggregate aggregate)
            : base(null)
        {
            this._aggregate = aggregate;
        }

        /// <summary>
        /// Calculates the Numeric Value of the Aggregate as evaluated for the given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            INode aggValue;
            if (context.Binder.IsGroup(bindingID))
            {
                BindingGroup group = context.Binder.Group(bindingID);
                context.Binder.SetGroupContext(true);
                aggValue = this._aggregate.Apply(context, group.BindingIDs);
                context.Binder.SetGroupContext(false);
            }
            else
            {
                aggValue = this._aggregate.Apply(context);
            }

            if (aggValue.NodeType == NodeType.Literal)
            {
                ILiteralNode lit = (ILiteralNode)aggValue;
                if (!lit.Language.Equals(String.Empty))
                {
                    //If there's a Language Tag implied type is string so no numeric value
                    throw new RdfQueryException("Cannot calculate the Numeric Value of literal with a language specifier");
                }
                else if (lit.DataType == null)
                {
                    //Try and infer the Data Type
                    if (SparqlSpecsHelper.IsInteger(lit.Value))
                    {
                        return Int64.Parse(lit.Value);
                    }
                    else if (SparqlSpecsHelper.IsDecimal(lit.Value))
                    {
                        return Decimal.Parse(lit.Value);
                    }
                    else if (SparqlSpecsHelper.IsDouble(lit.Value))
                    {
                        return Double.Parse(lit.Value);
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot calculate the Numeric Value of a literal since it does not appear to be a valid Integer/Decimal/Double");
                    }
                }
                else
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
            }
            else
            {
                throw new RdfQueryException("Cannot calculate the Numeric Value of a non-Literal Node");
            }
        }

        /// <summary>
        /// Gets the Aggregate this Expression represents
        /// </summary>
        public ISparqlAggregate Aggregate
        {
            get
            {
                return this._aggregate;
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._aggregate.ToString();
        }

        /// <summary>
        /// Gets the enumeration of variables that are used in the the aggregate expression
        /// </summary>
        public override IEnumerable<String> Variables
        {
            get
            {
                return this._aggregate.Expression.Variables;
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Aggregate; 
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return this._aggregate.Functor; 
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._aggregate.Arguments;
            }
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
    /// Class for representing Aggregate Expressions
    /// </summary>
    public class NonNumericAggregateExpressionTerm : ISparqlExpression
    {
        private ISparqlAggregate _aggregate;

        /// <summary>
        /// Creates a new non-numeric Aggregate Expression Term
        /// </summary>
        /// <param name="agg"></param>
        public NonNumericAggregateExpressionTerm(ISparqlAggregate agg)
        {
            this._aggregate = agg;
        }

        /// <summary>
        /// Gets the Value of the Aggregate
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode aggValue;
            if (context.Binder.IsGroup(bindingID))
            {
                BindingGroup group = context.Binder.Group(bindingID);
                context.Binder.SetGroupContext(true);
                aggValue = this._aggregate.Apply(context, group.BindingIDs);
                context.Binder.SetGroupContext(false);
            }
            else
            {
                aggValue = this._aggregate.Apply(context);
            }
            return aggValue;
        }

        /// <summary>
        /// Gets the Effective Boolean Value of the Aggregate
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the Aggregate this Expression represents
        /// </summary>
        public ISparqlAggregate Aggregate
        {
            get
            {
                return this._aggregate;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Aggregate
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return this._aggregate.Expression.Variables;
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._aggregate.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Aggregate;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get
            {
                return this._aggregate.Functor;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._aggregate.Arguments;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }
}
