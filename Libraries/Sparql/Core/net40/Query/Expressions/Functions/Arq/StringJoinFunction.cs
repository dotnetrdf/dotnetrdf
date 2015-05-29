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
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ afn:strjoin() function which is a string concatenation function with a separator
    /// </summary>
    public class StringJoinFunction 
        : BaseNAryExpression
    {
        private readonly String _separator;
        private readonly bool _fixedSeparator = false;

        /// <summary>
        /// Creates a new ARQ String Join function
        /// </summary>
        /// <param name="sepExpr">Separator Expression</param>
        /// <param name="expressions">Expressions to concatentate</param>
        public StringJoinFunction(IExpression sepExpr, IEnumerable<IExpression> expressions)
            : base(sepExpr.AsEnumerable().Concat(expressions))
        {
            if (sepExpr is ConstantTerm)
            {
                IValuedNode temp = sepExpr.Evaluate(null, null);
                if (temp.NodeType == NodeType.Literal)
                {
                    this._separator = temp.AsString();
                    this._fixedSeparator = true;
                }
            }
        }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            IList<IExpression> arguments = args as IList<IExpression> ?? args.ToList();
            return new StringJoinFunction(arguments.First(), arguments.Skip(1));
        }

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            String sep;
            if (this._fixedSeparator)
            {
                sep = this._separator;
            }
            else
            {
                IValuedNode sepNode = this.Arguments[0].Evaluate(solution, context);
                if (sepNode == null) throw new RdfQueryException("Cannot evaluate the ARQ strjoin() function when the separator expression evaluates to a Null");
                if (sepNode.NodeType == NodeType.Literal)
                {
                    sep = sepNode.Value;
                }
                else
                {
                    throw new RdfQueryException("Cannot evaluate the ARQ strjoin() function when the separator expression evaluates to a non-Literal Node");
                }
            }

            StringBuilder output = new StringBuilder();
            for (int i = 1; i < this.Arguments.Count; i++)
            {
                IValuedNode temp = this.Arguments[i].Evaluate(solution, context);
                if (temp == null) throw new RdfQueryException("Cannot evaluate the ARQ string-join() function when an argument evaluates to a Null");
                switch (temp.NodeType)
                {
                    case NodeType.Literal:
                        output.Append(temp.AsString());
                        break;
                    default:
                        throw new RdfQueryException("Cannot evaluate the ARQ string-join() function when an argument is not a Literal Node");
                }
                if (i < this.Arguments.Count - 1)
                {
                    output.Append(sep);
                }
            }

            return new StringNode(output.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        public override string Functor
        {
            get { return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.StrJoin; }
        }
    }
}
