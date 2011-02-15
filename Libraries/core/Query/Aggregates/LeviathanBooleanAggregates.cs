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
    /// A Custom aggregate which requires the Expression to evaluate to true for all Sets in the Group
    /// </summary>
    public class AllAggregate : BaseAggregate
    {
        /// <summary>
        /// Creates a new All Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public AllAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new All Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public AllAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Aggregate to see if the expression evaluates true for every member of the Group
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        /// <remarks>
        /// Does lazy evaluation - as soon as it encounters a false/error it will return false
        /// </remarks>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            foreach (int id in bindingIDs)
            {
                try
                {
                    if (!this._expr.EffectiveBooleanValue(context, id))
                    {
                        //As soon as we see a false we can return false
                        return new LiteralNode(null, Boolean.FalseString, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                    }
                }
                catch (RdfQueryException)
                {
                    //An error is a failure so we return false
                    return new LiteralNode(null, Boolean.FalseString, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                }
            }

            //If everything is true then we return true;
            return new LiteralNode(null, Boolean.TrueString, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Gets the String Representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.All);
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.All;
            }
        }
    }

    /// <summary>
    /// A Custom aggregate which requires the Expression to evaluate to true for at least one of the Sets in the Group
    /// </summary>
    public class AnyAggregate : BaseAggregate
    {
        /// <summary>
        /// Creates a new Any Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public AnyAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new Any Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifer applies</param>
        public AnyAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Aggregate to see if the expression evaluates true for any member of the Group
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        /// <remarks>
        /// Does lazy evaluation - as soon as it encounters a true it will return true
        /// </remarks>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            foreach (int id in bindingIDs)
            {
                try
                {
                    if (this._expr.EffectiveBooleanValue(context, id))
                    {
                        //As soon as we see a True we can return true
                        return new LiteralNode(null, Boolean.TrueString, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                    }
                }
                catch (RdfQueryException)
                {
                    //Ignore errors in an any
                }
            }

            //If we don't see any Trues we return false
            return new LiteralNode(null, Boolean.FalseString, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Gets the String Representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.Any);
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Any;
            }
        }
    }

    /// <summary>
    /// A Custom aggregate which requires the Expression to evaluate to false/error for all Sets in the Group
    /// </summary>
    public class NoneAggregate : BaseAggregate
    {
        /// <summary>
        /// Creates a new None Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public NoneAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new None Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifer applies</param>
        public NoneAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Aggregate to see if the expression evaluates false/error for every member of the Group
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        /// <remarks>
        /// Does lazy evaluation - as soon as it encounters a true it will return false
        /// </remarks>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            foreach (int id in bindingIDs)
            {
                try
                {
                    if (this._expr.EffectiveBooleanValue(context, id))
                    {
                        //As soon as we see a true we can return false
                        return new LiteralNode(null, Boolean.FalseString, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                    }
                }
                catch (RdfQueryException)
                {
                    //An error is a failure so we keep going
                }
            }

            //If everything is false then we return true;
            return new LiteralNode(null, Boolean.TrueString, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Gets the String Representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.None);
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.None;
            }
        }
    }
}
