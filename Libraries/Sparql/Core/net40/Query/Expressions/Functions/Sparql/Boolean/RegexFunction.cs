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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Nodes;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
    /// <summary>
    /// Class representing the SPARQL REGEX function
    /// </summary>
    public class RegexFunction
        : BaseNAryExpression
    {
        private string _pattern = null;
        private RegexOptions _options = RegexOptions.None;
        private readonly bool _fixedPattern = false, _fixedOptions = false, _invalidPattern = false;
        //private bool _useInStr = false;
        private readonly Regex _regex;

        /// <summary>
        /// Creates a new Regex() function expression
        /// </summary>
        /// <param name="text">Text to apply the Regular Expression to</param>
        /// <param name="pattern">Regular Expression Pattern</param>
        public RegexFunction(IExpression text, IExpression pattern)
            : this(text, pattern, null) { }

        /// <summary>
        /// Creates a new Regex() function expression
        /// </summary>
        /// <param name="text">Text to apply the Regular Expression to</param>
        /// <param name="pattern">Regular Expression Pattern</param>
        /// <param name="options">Regular Expression Options</param>
        public RegexFunction(IExpression text, IExpression pattern, IExpression options)
            : base(MakeArguments(text, pattern, options))
        {
            //Get the Pattern
            if (pattern is ConstantTerm)
            {
                //If the Pattern is a Constant Term then it is a fixed Pattern
                INode n = pattern.Evaluate(null, null);
                if (n.NodeType == NodeType.Literal)
                {
                    //Try to parse as a Regular Expression
                    try
                    {
                        string p = (n).Value;
                        // ReSharper disable once UnusedVariable
                        Regex temp = new Regex(p);

                        //It's a Valid Pattern
                        this._fixedPattern = true;
                        this._pattern = p;
                    }
                    catch
                    {
                        this._fixedPattern = true;
                        this._invalidPattern = true;
                    }
                }
            }

            //Get the Options
            if (options != null)
            {
                if (options is ConstantTerm)
                {
                    this.ConfigureOptions(options.Evaluate(null, 0), false);
                    this._fixedOptions = true;
                    if (this._fixedPattern) this._regex = new Regex(this._pattern, this._options);
                }
            }
            else
            {
                if (this._fixedPattern) this._regex = new Regex(this._pattern);
            }
        }

        private static IEnumerable<IExpression> MakeArguments(IExpression text, IExpression pattern, IExpression options)
        {
            if (options != null) return new IExpression[] { text, pattern, options };
            return new IExpression[] {text, pattern};
        }

        /// <summary>
        /// Configures the Options for the Regular Expression
        /// </summary>
        /// <param name="n">Node detailing the Options</param>
        /// <param name="throwErrors">Whether errors should be thrown or suppressed</param>
        private void ConfigureOptions(INode n, bool throwErrors)
        {
            //Start by resetting to no options
            this._options = RegexOptions.None;

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
                    string ops = (n).Value;
                    foreach (char c in ops)
                    {
                        switch (c)
                        {
                            case 'i':
                                this._options |= RegexOptions.IgnoreCase;
                                break;
                            case 'm':
                                this._options |= RegexOptions.Multiline;
                                break;
                            case 's':
                                this._options |= RegexOptions.Singleline;
                                break;
                            case 'x':
                                this._options |= RegexOptions.IgnorePatternWhitespace;
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

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> argList = args.ToList();
            return argList.Count == 3 ? new RegexFunction(argList[0], argList[1], argList[2]) : new RegexFunction(argList[0], argList[1], argList[2], argList[3]);
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            //Configure Options
            if (this.Arguments.Count == 3 && !this._fixedOptions)
            {
                this.ConfigureOptions(this.Arguments[2].Evaluate(solution, context), true);
            }

            //Compile the Regex if necessary
            if (!this._fixedPattern)
            {
                //Regex is not pre-compiled
                if (!this._invalidPattern)
                {
                    IValuedNode p = this.Arguments[1].Evaluate(solution, context);
                    if (p != null)
                    {
                        if (p.NodeType == NodeType.Literal)
                        {
                            this._pattern = p.AsString();
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

            //Execute the Regular Expression
            IValuedNode textNode = this.Arguments[0].Evaluate(solution, context);
            if (textNode == null)
            {
                throw new RdfQueryException("Cannot evaluate a Regular Expression against a null");
            }
            if (textNode.NodeType != NodeType.Literal) throw new RdfQueryException("Cannot evaluate a Regular Expression against a non-Literal Node");

            //Execute
            string text = textNode.AsString();
            return this._regex != null ? new BooleanNode(this._regex.IsMatch(text)) : new BooleanNode(Regex.IsMatch(text, this._pattern, this._options));
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRegex;
            }
        }
    }
}
