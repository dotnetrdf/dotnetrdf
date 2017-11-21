/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
    /// <summary>
    /// Class representing the SPARQL REGEX function
    /// </summary>
    public class RegexFunction
        : ISparqlExpression
    {
        private string _pattern = null;
        private RegexOptions _options = RegexOptions.None;
        private bool _fixedPattern = false, _fixedOptions = false;
        // private bool _useInStr = false;
        private Regex _regex;
        private ISparqlExpression _textExpr = null;
        private ISparqlExpression _patternExpr = null;
        private ISparqlExpression _optionExpr = null;

        /// <summary>
        /// Creates a new Regex() function expression
        /// </summary>
        /// <param name="text">Text to apply the Regular Expression to</param>
        /// <param name="pattern">Regular Expression Pattern</param>
        public RegexFunction(ISparqlExpression text, ISparqlExpression pattern)
            : this(text, pattern, null) { }

        /// <summary>
        /// Creates a new Regex() function expression
        /// </summary>
        /// <param name="text">Text to apply the Regular Expression to</param>
        /// <param name="pattern">Regular Expression Pattern</param>
        /// <param name="options">Regular Expression Options</param>
        public RegexFunction(ISparqlExpression text, ISparqlExpression pattern, ISparqlExpression options)
        {
            _textExpr = text;
            _patternExpr = pattern;

            // Get the Pattern
            if (pattern is ConstantTerm)
            {
                // If the Pattern is a Node Expression Term then it is a fixed Pattern
                INode n = pattern.Evaluate(null, 0);
                if (n.NodeType == NodeType.Literal)
                {
                    // Try to parse as a Regular Expression
                    try
                    {
                        string p = ((ILiteralNode)n).Value;
                        Regex temp = new Regex(p);

                        // It's a Valid Pattern
                        _fixedPattern = true;
                        // this._useInStr = p.ToCharArray().All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c));
                        _pattern = p;
                    }
                    catch
                    {
                        // No catch actions
                    }
                }
            }

            // Get the Options
            if (options != null)
            {
                _optionExpr = options;
                if (options is ConstantTerm)
                {
                    ConfigureOptions(options.Evaluate(null, 0), false);
                    _fixedOptions = true;
                    if (_fixedPattern) _regex = new Regex(_pattern, _options);
                }
            }
            else
            {
                if (_fixedPattern) _regex = new Regex(_pattern);
            }
        }

        /// <summary>
        /// Configures the Options for the Regular Expression
        /// </summary>
        /// <param name="n">Node detailing the Options</param>
        /// <param name="throwErrors">Whether errors should be thrown or suppressed</param>
        private void ConfigureOptions(INode n, bool throwErrors)
        {
            // Start by resetting to no options
            _options = RegexOptions.None;

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
                    string ops = ((ILiteralNode)n).Value;
                    foreach (char c in ops.ToCharArray())
                    {
                        switch (c)
                        {
                            case 'i':
                                _options |= RegexOptions.IgnoreCase;
                                break;
                            case 'm':
                                _options |= RegexOptions.Multiline;
                                break;
                            case 's':
                                _options |= RegexOptions.Singleline;
                                break;
                            case 'x':
                                _options |= RegexOptions.IgnorePatternWhitespace;
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
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            // Configure Options
            if (_optionExpr != null && !_fixedOptions)
            {
                ConfigureOptions(_optionExpr.Evaluate(context, bindingID), true);
            }

            // Compile the Regex if necessary
            if (!_fixedPattern)
            {
                // Regex is not pre-compiled
                if (_patternExpr != null)
                {
                    IValuedNode p = _patternExpr.Evaluate(context, bindingID);
                    if (p != null)
                    {
                        if (p.NodeType == NodeType.Literal)
                        {
                            _pattern = p.AsString();
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot parse a Pattern String from a non-Literal Node");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Not a valid Pattern Expression");
                    }
                }
                else
                {
                    throw new RdfQueryException("Not a valid Pattern Expression or the fixed Pattern String was invalid");
                }
            }

            // Execute the Regular Expression
            IValuedNode textNode = _textExpr.Evaluate(context, bindingID);
            if (textNode == null)
            {
                throw new RdfQueryException("Cannot evaluate a Regular Expression against a NULL");
            }
            if (textNode.NodeType == NodeType.Literal)
            {
                // Execute
                string text = textNode.AsString();
                if (_regex != null)
                {
                    return new BooleanNode(null, _regex.IsMatch(text));
                }
                else
                {
                    return new BooleanNode(null, Regex.IsMatch(text, _pattern, _options));
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evaluate a Regular Expression against a non-Literal Node");
            }

        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("REGEX(");
            output.Append(_textExpr.ToString());
            output.Append(",");
            if (_fixedPattern)
            {
                output.Append('"');
                output.Append(_pattern);
                output.Append('"');
            }
            else
            {
                output.Append(_patternExpr.ToString());
            }
            if (_optionExpr != null)
            {
                output.Append("," + _optionExpr.ToString());
            }
            output.Append(")");

            return output.ToString();
        }

        /// <summary>
        /// Gets the enumeration of Variables involved in this Expression
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                List<string> vs = new List<string>();
                if (_textExpr != null) vs.AddRange(_textExpr.Variables);
                if (_patternExpr != null) vs.AddRange(_patternExpr.Variables);
                if (_optionExpr != null) vs.AddRange(_optionExpr.Variables);
                return vs;
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRegex;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (_optionExpr != null)
                {
                    return new ISparqlExpression[] { _textExpr, _patternExpr, _optionExpr };
                }
                else
                {
                    return new ISparqlExpression[] { _textExpr, _patternExpr };
                }
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return _textExpr.CanParallelise && _patternExpr.CanParallelise && (_optionExpr == null || _optionExpr.CanParallelise);
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (_optionExpr != null)
            {
                return new RegexFunction(transformer.Transform(_textExpr), transformer.Transform(_patternExpr), transformer.Transform(_optionExpr));
            }
            else
            {
                return new RegexFunction(transformer.Transform(_textExpr), transformer.Transform(_patternExpr));
            }
        }
    }
}
