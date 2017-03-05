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
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:replace() function
    /// </summary>
    public class ReplaceFunction
        : ISparqlExpression
    {
        private string _find = null;
        private string _replace = null;
        private RegexOptions _options = RegexOptions.None;
        private bool _fixedPattern = false;
        private bool _fixedReplace = false;
        private ISparqlExpression _textExpr = null;
        private ISparqlExpression _findExpr = null;
        private ISparqlExpression _optionExpr = null;
        private ISparqlExpression _replaceExpr = null;

        /// <summary>
        /// Creates a new XPath Replace function
        /// </summary>
        /// <param name="text">Text Expression</param>
        /// <param name="find">Search Expression</param>
        /// <param name="replace">Replace Expression</param>
        public ReplaceFunction(ISparqlExpression text, ISparqlExpression find, ISparqlExpression replace)
            : this(text, find, replace, null) { }

        /// <summary>
        /// Creates a new XPath Replace function
        /// </summary>
        /// <param name="text">Text Expression</param>
        /// <param name="find">Search Expression</param>
        /// <param name="replace">Replace Expression</param>
        /// <param name="options">Options Expression</param>
        public ReplaceFunction(ISparqlExpression text, ISparqlExpression find, ISparqlExpression replace, ISparqlExpression options)
        {
            this._textExpr = text;

            //Get the Pattern
            if (find is ConstantTerm)
            {
                //If the Pattern is a Node Expression Term then it is a fixed Pattern
                IValuedNode n = find.Evaluate(null, 0);
                if (n.NodeType == NodeType.Literal)
                {
                    //Try to parse as a Regular Expression
                    try
                    {
                        string p = n.AsString();
                        Regex temp = new Regex(p);

                        //It's a Valid Pattern
                        this._fixedPattern = true;
                        this._find = p;
                    }
                    catch
                    {
                        //No catch actions
                    }
                }
            }
            this._findExpr = find;

            //Get the Replace
            if (replace is ConstantTerm)
            {
                //If the Replace is a Node Expresison Term then it is a fixed Pattern
                IValuedNode n = replace.Evaluate(null, 0);
                if (n.NodeType == NodeType.Literal)
                {
                    this._replace = n.AsString();
                    this._fixedReplace = true;
                }
            }
            this._replaceExpr = replace;

            //Get the Options
            if (options != null)
            {
                if (options is ConstantTerm)
                {
                    this.ConfigureOptions(options.Evaluate(null, 0), false);
                }
                this._optionExpr = options;
            }
        }

        /// <summary>
        /// Configures the Options for the Regular Expression
        /// </summary>
        /// <param name="n">Node detailing the Options</param>
        /// <param name="throwErrors">Whether errors should be thrown or suppressed</param>
        private void ConfigureOptions(IValuedNode n, bool throwErrors)
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
                    string ops = n.AsString();
                    foreach (char c in ops.ToCharArray())
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

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            //Configure Options
            if (this._optionExpr != null)
            {
                this.ConfigureOptions(this._optionExpr.Evaluate(context, bindingID), true);
            }

            //Compile the Regex if necessary
            if (!this._fixedPattern)
            {
                //Regex is not pre-compiled
                if (this._findExpr != null)
                {
                    IValuedNode p = this._findExpr.Evaluate(context, bindingID);
                    if (p != null)
                    {
                        if (p.NodeType == NodeType.Literal)
                        {
                            this._find = p.AsString();
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
            //Compute the Replace if necessary
            if (!this._fixedReplace)
            {
                if (this._replaceExpr != null)
                {
                    IValuedNode r = this._replaceExpr.Evaluate(context, bindingID);
                    if (r != null)
                    {
                        if (r.NodeType == NodeType.Literal)
                        {
                            this._replace = r.AsString();
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot parse a Replace String from a non-Literal Node");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Not a valid Replace Expression");
                    }
                }
                else
                {
                    throw new RdfQueryException("Not a valid Replace Expression");
                }
            }

            //Execute the Regular Expression
            IValuedNode textNode = this._textExpr.Evaluate(context, bindingID);
            if (textNode == null)
            {
                throw new RdfQueryException("Cannot evaluate a Regular Expression against a NULL");
            }
            if (textNode.NodeType == NodeType.Literal)
            {
                //Execute
                string text = textNode.AsString();
                string output = Regex.Replace(text, this._find, this._replace, this._options);
                return new StringNode(null, output, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
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
            output.Append("<");
            output.Append(XPathFunctionFactory.XPathFunctionsNamespace);
            output.Append(XPathFunctionFactory.Replace);
            output.Append(">(");
            output.Append(this._textExpr.ToString());
            output.Append(",");
            if (this._fixedPattern)
            {
                output.Append('"');
                output.Append(this._find);
                output.Append('"');
            }
            else
            {
                output.Append(this._findExpr.ToString());
            }
            output.Append(",");
            if (this._fixedReplace)
            {
                output.Append('"');
                output.Append(this._replace);
                output.Append('"');
            }
            else if (this._replaceExpr != null)
            {
                output.Append(this._replaceExpr.ToString());
            }
            if (this._optionExpr != null)
            {
                output.Append("," + this._optionExpr.ToString());
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
                if (this._textExpr != null) vs.AddRange(this._textExpr.Variables);
                if (this._findExpr != null) vs.AddRange(this._findExpr.Variables);
                if (this._replaceExpr != null) vs.AddRange(this._replaceExpr.Variables);
                if (this._optionExpr != null) vs.AddRange(this._optionExpr.Variables);
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
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Replace;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (this._optionExpr != null)
                {
                    return new ISparqlExpression[] { this._textExpr, this._findExpr, this._replaceExpr, this._optionExpr };
                }
                else
                {
                    return new ISparqlExpression[] { this._textExpr, this._findExpr, this._replaceExpr };
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
                return this._textExpr.CanParallelise && this._findExpr.CanParallelise && this._replaceExpr.CanParallelise && (this._optionExpr == null || this._optionExpr.CanParallelise);
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (this._optionExpr != null)
            {
                return new ReplaceFunction(transformer.Transform(this._textExpr), transformer.Transform(this._findExpr), transformer.Transform(this._replaceExpr), transformer.Transform(this._optionExpr));
            }
            else
            {
                return new ReplaceFunction(transformer.Transform(this._textExpr), transformer.Transform(this._findExpr), transformer.Transform(this._replaceExpr));
            }
        }
    }
}
