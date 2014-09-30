/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ afn:substring() function which is a sub-string with Java semantics
    /// </summary>
    public class SubstringFunction
        : BaseNAryExpression
    {
        /// <summary>
        /// Creates a new ARQ substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Expression giving an index at which to start the substring</param>
        public SubstringFunction(IExpression stringExpr, IExpression startExpr)
            : this(stringExpr, startExpr, null) {}

        /// <summary>
        /// Creates a new ARQ substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Expression giving an index at which to start the substring</param>
        /// <param name="endExpr">Expression giving an index at which to end the substring</param>
        public SubstringFunction(IExpression stringExpr, IExpression startExpr, IExpression endExpr)
            : base(MakeArguments(stringExpr, startExpr, endExpr)) {}

        private static IEnumerable<IExpression> MakeArguments(IExpression stringExpr, IExpression startExpr, IExpression endExpr)
        {
            return endExpr != null ? new IExpression[] {stringExpr, startExpr, endExpr} : new IExpression[] {stringExpr, startExpr};
        }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> arguments = args.ToList();
            if (arguments.Count < 2 || arguments.Count > 3) throw new ArgumentException("Requires 2/3 arguments");
            return new SubstringFunction(arguments[0], arguments[1], arguments.Count == 3 ? arguments[2] : null);
        }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            INode input = this.CheckArgument(this.Arguments[0], solution, context);
            IValuedNode start = this.CheckArgument(this.Arguments[1], solution, context, XPathFunctionFactory.AcceptNumericArguments);

            if (this.Arguments.Count == 3)
            {
                IValuedNode end = this.CheckArgument(this.Arguments[2], solution, context, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(String.Empty)) return new StringNode(String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.AsInteger());
                    int e = Convert.ToInt32(end.AsInteger());

                    if (s < 0) s = 0;
                    if (e < s)
                    {
                        //If no/negative characters are being selected the empty string is returned
                        return new StringNode(String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    if (s > input.Value.Length)
                    {
                        //If the start is after the end of the string the empty string is returned
                        return new StringNode(String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    if (e > input.Value.Length)
                    {
                        //If the end is greater than the length of the string the string from the starts onwards is returned
                        return new StringNode(input.Value.Substring(s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    //Otherwise do normal substring
                    return new StringNode(input.Value.Substring(s, e - s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start/End argument to an Integer");
                }
            }
            if (input.Value.Equals(String.Empty)) return new StringNode(String.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

            try
            {
                int s = Convert.ToInt32(start.AsInteger());
                if (s < 0) s = 0;

                return new StringNode(input.Value.Substring(s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
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
            if (temp == null) throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions evaluated to null");
            if (temp.NodeType != NodeType.Literal) throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions returned a non-literal");
            INode lit = temp;
            if (lit.DataType != null)
            {
                if (argumentTypeValidator(lit.DataType))
                {
                    //Appropriately typed literals are fine
                    return temp;
                }
                throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions returned a typed literal with an invalid type");
            }
            if (argumentTypeValidator(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)))
            {
                //Untyped Literals are treated as Strings and may be returned when the argument allows strings
                return temp;
            }
            throw new RdfQueryException("Unable to evalaute an ARQ substring as one of the argument expressions returned an untyped literal");
        }


        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get { return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring; }
        }

    }
}
