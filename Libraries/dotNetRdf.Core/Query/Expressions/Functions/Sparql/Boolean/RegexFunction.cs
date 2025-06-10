/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;

/// <summary>
/// Class representing the SPARQL REGEX function.
/// </summary>
public class RegexFunction
    : ISparqlExpression
{
    private RegexOptions _options = RegexOptions.None;

    // private bool _useInStr = false;

    /// <summary>
    /// Creates a new Regex() function expression.
    /// </summary>
    /// <param name="text">Text to apply the Regular Expression to.</param>
    /// <param name="pattern">Regular Expression Pattern.</param>
    public RegexFunction(ISparqlExpression text, ISparqlExpression pattern)
        : this(text, pattern, null) { }

    /// <summary>
    /// Creates a new Regex() function expression.
    /// </summary>
    /// <param name="text">Text to apply the Regular Expression to.</param>
    /// <param name="pattern">Regular Expression Pattern.</param>
    /// <param name="options">Regular Expression Options.</param>
    public RegexFunction(ISparqlExpression text, ISparqlExpression pattern, ISparqlExpression options)
    {
        TextExpression = text;
        PatternExpression = pattern;

        // Get the Pattern
        if (pattern is ConstantTerm constantPattern)
        {
            // If the Pattern is a Node Expression Term then it is a fixed Pattern
            INode n = constantPattern.Node;
            if (n.NodeType == NodeType.Literal)
            {
                // Try to parse as a Regular Expression
                try
                {
                    var p = ((ILiteralNode)n).Value;
                    var temp = new Regex(p);

                    // It's a Valid Pattern
                    FixedPattern = true;
                    // this._useInStr = p.ToCharArray().All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c));
                    Pattern = p;
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
            OptionsExpression = options;
            if (options is ConstantTerm constantOptions)
            {
                _options = GetOptions(constantOptions.Node, false);
                FixedOptions = true;
                if (FixedPattern) CompiledRegex = new Regex(Pattern, _options);
            }
        }
        else
        {
            if (FixedPattern) CompiledRegex = new Regex(Pattern);
        }
    }

    /// <summary>
    /// Get the expression that yields the text to be tested.
    /// </summary>
    public ISparqlExpression TextExpression { get; } = null;

    /// <summary>
    /// Get the expression that yields the regular expression pattern to use.
    /// </summary>
    public ISparqlExpression PatternExpression { get; } = null;
    /// <summary>
    /// Get the expression that yields the options to apply to the regular expression evaulation.
    /// </summary>
    public ISparqlExpression OptionsExpression { get; } = null;
    /// <summary>
    /// True if <see cref="PatternExpression"/> is a constant.
    /// </summary>
    public bool FixedPattern { get; } = false;

    /// <summary>
    /// Get the pattern part of the regular expression if <see cref="FixedPattern"/> is true.
    /// </summary>
    public string Pattern { get; } = null;

    /// <summary>
    /// True if <see cref="OptionsExpression"/> is a constant.
    /// </summary>
    public bool FixedOptions { get; } = false;

    /// <summary>
    /// Get the compiled constant regular expression. This is only
    /// non-null if the pattern expression is constant and the
    /// options expression is either null or constant.
    /// </summary>
    public Regex CompiledRegex { get; }

    /// <summary>
    /// Configures the Options for the Regular Expression.
    /// </summary>
    /// <param name="n">Node detailing the Options.</param>
    /// <param name="throwErrors">Whether errors should be thrown or suppressed.</param>
    public  static RegexOptions GetOptions(INode n, bool throwErrors)
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
        output.Append("REGEX(");
        output.Append(TextExpression);
        output.Append(",");
        if (FixedPattern)
        {
            output.Append('"');
            output.Append(Pattern);
            output.Append('"');
        }
        else
        {
            output.Append(PatternExpression);
        }
        if (OptionsExpression != null)
        {
            output.Append("," + OptionsExpression);
        }
        output.Append(")");

        return output.ToString();
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessRegexFunction(this, context, binding);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitRegexFunction(this);
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
            if (PatternExpression != null) vs.AddRange(PatternExpression.Variables);
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
            return SparqlSpecsHelper.SparqlKeywordRegex;
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
                return new ISparqlExpression[] { TextExpression, PatternExpression, OptionsExpression };
            }
            else
            {
                return new ISparqlExpression[] { TextExpression, PatternExpression };
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
            return TextExpression.CanParallelise && PatternExpression.CanParallelise && (OptionsExpression == null || OptionsExpression.CanParallelise);
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
            return new RegexFunction(transformer.Transform(TextExpression), transformer.Transform(PatternExpression), transformer.Transform(OptionsExpression));
        }
        else
        {
            return new RegexFunction(transformer.Transform(TextExpression), transformer.Transform(PatternExpression));
        }
    }
}
