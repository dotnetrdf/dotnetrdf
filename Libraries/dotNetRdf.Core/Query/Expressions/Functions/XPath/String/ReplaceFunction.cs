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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String;

/// <summary>
/// Represents the XPath fn:replace() function.
/// </summary>
public class ReplaceFunction
    : ISparqlExpression
{
    /// <summary>
    /// Get the fixed string to be located.
    /// </summary>
    public string FindPattern { get; }

    /// <summary>
    /// Get the fixed string to use as the replacement.
    /// </summary>
    public string ReplacePattern { get; }

    /// <summary>
    /// Get the regular expression matching options to apply.
    /// </summary>
    public RegexOptions Options { get; private set; } = RegexOptions.None;
    
    /// <summary>
    /// True if the find pattern is specified by <see cref="FindPattern"/>, false if it is specified by <see cref="FindExpression"/>.
    /// </summary>
    public bool FixedFindPattern { get; }

    /// <summary>
    /// True if the replacement pattern is specified by <see cref="ReplacePattern"/>, false if it is specified by <see cref="ReplaceExpression"/>.
    /// </summary>
    public bool FixedReplacePattern { get; }

    /// <summary>
    /// True if the matching options are specified by <see cref="Options"/>, false if they are specified by <see cref="OptionsExpression"/>.
    /// </summary>
    public bool FixedOptions { get; }

    /// <summary>
    /// Get the expression that evaluates to the string to process.
    /// </summary>
    public ISparqlExpression TestExpression { get; }

    /// <summary>
    /// Get the expression that evaluates to the pattern to find.
    /// </summary>
    public ISparqlExpression FindExpression { get; }

    /// <summary>
    /// Get the expression that evaluates to the replacement pattern.
    /// </summary>
    public ISparqlExpression ReplaceExpression { get; }

    /// <summary>
    /// Get the expression that evaluates to the matching options.
    /// </summary>
    public ISparqlExpression OptionsExpression { get; }

    /// <summary>
    /// Creates a new XPath Replace function.
    /// </summary>
    /// <param name="text">Text Expression.</param>
    /// <param name="find">Search Expression.</param>
    /// <param name="replace">Replace Expression.</param>
    public ReplaceFunction(ISparqlExpression text, ISparqlExpression find, ISparqlExpression replace)
        : this(text, find, replace, null) { }

    /// <summary>
    /// Creates a new XPath Replace function.
    /// </summary>
    /// <param name="text">Text Expression.</param>
    /// <param name="find">Search Expression.</param>
    /// <param name="replace">Replace Expression.</param>
    /// <param name="options">Options Expression.</param>
    public ReplaceFunction(ISparqlExpression text, ISparqlExpression find, ISparqlExpression replace, ISparqlExpression options)
    {
        TestExpression = text;

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
                    FixedFindPattern = true;
                    FindPattern = p;
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
            // If the Replace is a Node Expression Term then it is a fixed Pattern
            IValuedNode n = constantReplace.Node;
            if (n.NodeType == NodeType.Literal)
            {
                ReplacePattern = n.AsString();
                FixedReplacePattern = true;
            }
        }
        ReplaceExpression = replace;

        // Get the Options
        if (options != null)
        {
            if (options is ConstantTerm constantOptions)
            {
                Options = GetOptions(constantOptions.Node, false);
                FixedOptions = true;
            }
            OptionsExpression = options;
        }
    }

    /// <summary>
    /// Configures the Options for the Regular Expression.
    /// </summary>
    /// <param name="n">Node detailing the Options.</param>
    /// <param name="throwErrors">Whether errors should be thrown or suppressed.</param>
    public RegexOptions GetOptions(IValuedNode n, bool throwErrors)
    {
        // Start by assuming no options
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
                var ops = n.AsString();
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
        output.Append(TestExpression);
        output.Append(",");
        if (FixedFindPattern)
        {
            output.Append('"');
            output.Append(FindPattern);
            output.Append('"');
        }
        else
        {
            output.Append(FindExpression);
        }
        output.Append(",");
        if (FixedReplacePattern)
        {
            output.Append('"');
            output.Append(ReplacePattern);
            output.Append('"');
        }
        else if (ReplaceExpression != null)
        {
            output.Append(ReplaceExpression);
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
        return processor.ProcessReplaceFunction(this, context, binding);
    }

    /// <inheritdoc />
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
            if (TestExpression != null) vs.AddRange(TestExpression.Variables);
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
            return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Replace;
        }
    }

    /// <summary>
    /// Gets the Arguments of the Expression.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return OptionsExpression != null 
                ? new[] { TestExpression, FindExpression, ReplaceExpression, OptionsExpression } 
                : new[] { TestExpression, FindExpression, ReplaceExpression };
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return TestExpression.CanParallelise &&
                   FindExpression.CanParallelise &&
                   ReplaceExpression.CanParallelise &&
                   (OptionsExpression == null || OptionsExpression.CanParallelise);
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return OptionsExpression != null 
            ? new ReplaceFunction(transformer.Transform(TestExpression), transformer.Transform(FindExpression), transformer.Transform(ReplaceExpression), transformer.Transform(OptionsExpression)) 
            : new ReplaceFunction(transformer.Transform(TestExpression), transformer.Transform(FindExpression), transformer.Transform(ReplaceExpression));
    }
}
