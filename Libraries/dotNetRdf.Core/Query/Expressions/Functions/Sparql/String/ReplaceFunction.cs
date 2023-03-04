/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the XPath fn:replace() function.
    /// </summary>
    public class ReplaceFunction
        : ISparqlExpression
    {
        public ISparqlExpression TextExpression { get; }
        public ISparqlExpression FindExpression { get; }
        public ISparqlExpression OptionsExpression { get; }
        public ISparqlExpression ReplaceExpression { get; }
        public bool FixedPattern { get; }
        public bool FixedReplace { get; }
        public bool FixedOptions { get; }
        public string Find { get; }
        public string Replace { get; }
        public RegexOptions Options { get; } = RegexOptions.None;

        /// <summary>
        /// Creates a new SPARQL Replace function.
        /// </summary>
        /// <param name="text">Text Expression.</param>
        /// <param name="find">Search Expression.</param>
        /// <param name="replace">Replace Expression.</param>
        public ReplaceFunction(ISparqlExpression text, ISparqlExpression find, ISparqlExpression replace)
            : this(text, find, replace, null) { }

        /// <summary>
        /// Creates a new SPARQL Replace function.
        /// </summary>
        /// <param name="text">Text Expression.</param>
        /// <param name="find">Search Expression.</param>
        /// <param name="replace">Replace Expression.</param>
        /// <param name="options">Options Expression.</param>
        public ReplaceFunction(ISparqlExpression text, ISparqlExpression find, ISparqlExpression replace, ISparqlExpression options)
        {
            TextExpression = text;

            // Get the Pattern
            if (find is ConstantTerm constantFind)
            {
                // If the Pattern is a Node Expression Term then it is a fixed Pattern
                IValuedNode n = constantFind.Node;
                if (n.NodeType == NodeType.Literal)
                {
                    // Try to parse as a Regular Expression
                    try
                    {
                        var p = n.AsString();
                        var temp = new Regex(p);

                        // It's a Valid Pattern
                        FixedPattern = true;
                        Find = p;
                    }
                    catch
                    {
                        // No catch actions
                    }
                }
            }
            FindExpression = find;

            // Get the Replace
            if (replace is ConstantTerm constantReplace)
            {
                // If the Replace is a Node Expresison Term then it is a fixed Pattern
                IValuedNode n = constantReplace.Node;
                if (n.NodeType == NodeType.Literal)
                {
                    Replace = n.AsString();
                    FixedReplace = true;
                }
            }
            ReplaceExpression = replace;

            // Get the Options
            if (options != null)
            {
                if (options is ConstantTerm constantOpts)
                {
                    Options = GetOptions(constantOpts.Node, false);
                    FixedOptions = true;
                }
                OptionsExpression = options;
            }
            else
            {
                FixedOptions = true;
            }
        }

        /// <summary>
        /// Returns the Options for the Regular Expression.
        /// </summary>
        /// <param name="n">Node detailing the Options.</param>
        /// <param name="throwErrors">Whether errors should be thrown or suppressed.</param>
        public RegexOptions GetOptions(INode n, bool throwErrors)
        {
            // Start by resetting to no options
            RegexOptions options = RegexOptions.None;

            if (n == null)
            {
                if (throwErrors)
                {
                    throw new RdfQueryException("REGEX Options Expression does not produce an Options string");
                }
            }
            else
            {
                if (n.NodeType == NodeType.Literal)
                {
                    var ops = ((ILiteralNode)n).Value;
                    foreach (var c in ops.ToCharArray())
                    {
                        switch (c)
                        {
                            case 'i':
                                options |= RegexOptions.IgnoreCase;
                                break;
                            case 'm':
                                options |= RegexOptions.Multiline;
                                break;
                            case 's':
                                options |= RegexOptions.Singleline;
                                break;
                            case 'x':
                                options |= RegexOptions.IgnorePatternWhitespace;
                                break;
                            default:
                                if (throwErrors)
                                {
                                    throw new RdfQueryException("Invalid flag character '" + c + "' in Options string");
                                }
                                break;
                        }
                    }
                }
                else
                {
                    if (throwErrors)
                    {
                        throw new RdfQueryException("REGEX Options Expression does not produce an Options string");
                    }
                }
            }

            return options;
        }

        /// <summary>
        /// Gets the String representation of this Expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var output = new StringBuilder();
            output.Append("<");
            output.Append(XPathFunctionFactory.XPathFunctionsNamespace);
            output.Append(XPathFunctionFactory.Replace);
            output.Append(">(");
            output.Append(TextExpression.ToString());
            output.Append(",");
            if (FixedPattern)
            {
                output.Append('"');
                output.Append(Find);
                output.Append('"');
            }
            else
            {
                output.Append(FindExpression.ToString());
            }
            output.Append(",");
            if (FixedReplace)
            {
                output.Append('"');
                output.Append(Replace);
                output.Append('"');
            }
            else if (ReplaceExpression != null)
            {
                output.Append(ReplaceExpression.ToString());
            }
            if (OptionsExpression != null)
            {
                output.Append("," + OptionsExpression.ToString());
            }
            output.Append(")");

            return output.ToString();
        }

        public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
        {
            return processor.ProcessReplaceFunction(this, context, binding);
        }

        public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
        {
            return visitor.VisitReplaceFunction(this);
        }

        /// <summary>
        /// Gets the enumeration of Variables involved in this Expression.
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                var vs = new List<string>();
                if (TextExpression != null) vs.AddRange(TextExpression.Variables);
                if (FindExpression != null) vs.AddRange(FindExpression.Variables);
                if (ReplaceExpression != null) vs.AddRange(ReplaceExpression.Variables);
                if (OptionsExpression != null) vs.AddRange(OptionsExpression.Variables);
                return vs;
            }
        }

        /// <summary>
        /// Gets the Type of the Expression.
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression.
        /// </summary>
        public string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordReplace;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression.
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (OptionsExpression != null)
                {
                    return new ISparqlExpression[] { TextExpression, FindExpression, ReplaceExpression, OptionsExpression };
                }
                else
                {
                    return new ISparqlExpression[] { TextExpression, FindExpression, ReplaceExpression };
                }
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel.
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return TextExpression.CanParallelise && FindExpression.CanParallelise && ReplaceExpression.CanParallelise && (OptionsExpression == null || OptionsExpression.CanParallelise);
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer.
        /// </summary>
        /// <param name="transformer">Expression Transformer.</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (OptionsExpression != null)
            {
                return new ReplaceFunction(transformer.Transform(TextExpression), transformer.Transform(FindExpression), transformer.Transform(ReplaceExpression), transformer.Transform(OptionsExpression));
            }
            else
            {
                return new ReplaceFunction(transformer.Transform(TextExpression), transformer.Transform(FindExpression), transformer.Transform(ReplaceExpression));
            }
        }
    }
}
