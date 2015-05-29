/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the SPARQL SUBSTR Function
    /// </summary>
    public class SubStrFunction
        : BaseNAryExpression
    {

        /// <summary>
        /// Creates a new XPath Substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Start</param>
        public SubStrFunction(IExpression stringExpr, IExpression startExpr)
            : this(stringExpr, startExpr, null) { }

        /// <summary>
        /// Creates a new XPath Substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Start</param>
        /// <param name="lengthExpr">Length</param>
        public SubStrFunction(IExpression stringExpr, IExpression startExpr, IExpression lengthExpr)
            : base(MakeArguments(stringExpr, startExpr, lengthExpr)) { }

        private static IEnumerable<IExpression> MakeArguments(IExpression stringExpr, IExpression startExpr, IExpression lengthExpr)
        {
            return lengthExpr != null ? new IExpression[] {stringExpr, startExpr, lengthExpr} : new IExpression[] {stringExpr, startExpr};
        }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> arguments = args.ToList();
            if (arguments.Count < 2 || arguments.Count > 3) throw new ArgumentException("Requires 2/3 arguments");
            return new SubStrFunction(arguments[0], arguments[1], arguments.Count == 3 ? arguments[2] : null);
        }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode input = this.CheckArgument(this.Arguments[0], solution, context);
            IValuedNode start = this.CheckArgument(this.Arguments[1], solution, context, XPathFunctionFactory.AcceptNumericArguments);

            if (this.Arguments.Count == 2)
            {
                IValuedNode length = this.CheckArgument(this.Arguments[2], solution, context, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(string.Empty)) return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.AsInteger());
                    int l = Convert.ToInt32(length.AsInteger());

                    if (s < 1) s = 1;
                    if (l < 1)
                    {
                        //If no/negative characters are being selected the empty string is returned
                        return input.DataType != null ? new StringNode(string.Empty, input.DataType) : new StringNode(string.Empty, input.Language);
                    }
                    if ((s - 1) > input.Value.Length)
                    {
                        //If the start is after the end of the string the empty string is returned
                        return input.HasDataType ? new StringNode(string.Empty, input.DataType) : new StringNode(string.Empty, input.Language);
                    }
                    if (((s - 1) + l) > input.Value.Length)
                    {
                        //If the start plus the length is greater than the length of the string the string from the starts onwards is returned
                        return input.HasDataType ? new StringNode(input.Value.Substring(s - 1), input.DataType) : new StringNode(input.Value.Substring(s - 1), input.Language);
                    }
                    //Otherwise do normal substring
                    return input.HasDataType ? new StringNode(input.Value.Substring(s - 1, l), input.DataType) : new StringNode(input.Value.Substring(s - 1, l), input.Language);
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start/Length argument to an Integer");
                }
            }
            if (input.Value.Equals(string.Empty)) return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

            try
            {
                int s = Convert.ToInt32(start.AsInteger());
                if (s < 1) s = 1;

                return input.HasDataType ? new StringNode(input.Value.Substring(s - 1), input.DataType) : new StringNode(input.Value.Substring(s - 1), input.Language);
            }
            catch
            {
                throw new RdfQueryException("Unable to convert the Start argument to an Integer");
            }
        }

        private IValuedNode CheckArgument(IExpression expr, ISolution solution, IExpressionContext context)
        {
            return this.CheckArgument(expr, solution, context, XPathFunctionFactory.AcceptStringArguments);
        }

        private IValuedNode CheckArgument(IExpression expr, ISolution solution, IExpressionContext context, Func<Uri, bool> argumentTypeValidator)
        {
            IValuedNode temp = expr.Evaluate(solution, context);
            if (temp == null) throw new RdfQueryException("Unable to evaluate a substring as one of the argument expressions evaluated to null");
            if (temp.NodeType != NodeType.Literal) throw new RdfQueryException("Unable to evaluate a substring as one of the argument expressions returned a non-literal");
            INode lit = temp;
            if (lit.DataType != null)
            {
                if (argumentTypeValidator(lit.DataType))
                {
                    //Appropriately typed literals are fine
                    return temp;
                }
                throw new RdfQueryException("Unable to evaluate a substring as one of the argument expressions returned a typed literal with an invalid type");
            }
            if (argumentTypeValidator(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)))
            {
                //Untyped Literals are treated as Strings and may be returned when the argument allows strings
                return temp;
            }
            throw new RdfQueryException("Unable to evalaute a substring as one of the argument expressions returned an untyped literal");
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSubStr;
            }
        }
    }
}
