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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Represents the XPath fn:replace() function
    /// </summary>
    public class ReplaceFunction
        : BaseNAryExpression
    {
        private string _find = null;
        private string _replace = null;
        private RegexOptions _options = RegexOptions.None;
        private readonly bool _fixedPattern = false, _fixedReplace = false, _fixedOptions = false, _invalidFindPattern = false;

        /// <summary>
        /// Creates a new SPARQL Replace function
        /// </summary>
        /// <param name="text">Text Expression</param>
        /// <param name="find">Search Expression</param>
        /// <param name="replace">Replace Expression</param>
        public ReplaceFunction(IExpression text, IExpression find, IExpression replace)
            : this(text, find, replace, null) { }

        /// <summary>
        /// Creates a new SPARQL Replace function
        /// </summary>
        /// <param name="text">Text Expression</param>
        /// <param name="find">Search Expression</param>
        /// <param name="replace">Replace Expression</param>
        /// <param name="options">Options Expression</param>
        public ReplaceFunction(IExpression text, IExpression find, IExpression replace, IExpression options)
            : base(MakeArguments(text, find, replace, options))
        {
            //Get the Pattern
            if (find is ConstantTerm)
            {
                //If the Pattern is a Node Expression Term then it is a fixed Pattern
                IValuedNode n = find.Evaluate(null, null);
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
                        this._invalidFindPattern = true;
                    }
                }
            }

            //Get the Replace
            if (replace is ConstantTerm)
            {
                //If the Replace is a Node Expresison Term then it is a fixed Pattern
                IValuedNode n = replace.Evaluate(null, null);
                if (n.NodeType == NodeType.Literal)
                {
                    this._replace = n.AsString();
                    this._fixedReplace = true;
                }
            }

            //Get the Options
            if (options == null) return;
            if (options is ConstantTerm)
            {
                this.ConfigureOptions(options.Evaluate(null, null), false);
                this._fixedOptions = true;
            }
        }

        private static IEnumerable<IExpression> MakeArguments(IExpression text, IExpression find, IExpression replace, IExpression options)
        {
            if (options != null) return new IExpression[] { text, find, replace, options };
            return new IExpression[] { text, find, replace };
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
            return argList.Count == 3 ? new ReplaceFunction(argList[0], argList[1], argList[2]) : new ReplaceFunction(argList[0], argList[1], argList[2], argList[3]);
        }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            //Configure Options
            if (this.Arguments.Count == 4 && !this._fixedOptions)
            {
                this.ConfigureOptions(this.Arguments[3].Evaluate(solution, context), true);
            }

            //Compile the Regex if necessary
            if (!this._fixedPattern)
            {
                //Regex is not pre-compiled
                if (!this._invalidFindPattern)
                {
                    IValuedNode p = this.Arguments[1].Evaluate(solution, context);
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
                    IValuedNode r = this.Arguments[2].Evaluate(solution, context);
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

            //Execute the Regular Expression
            IValuedNode textNode = this.Arguments[0].Evaluate(solution, context);
            if (textNode == null)
            {
                throw new RdfQueryException("Cannot evaluate a Regular Expression against a NULL");
            }
            if (textNode.NodeType == NodeType.Literal)
            {

                //Execute
                INode lit = textNode;
                if (lit.DataType != null && !lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString)) throw new RdfQueryException("Text Argument to Replace must be of type xsd:string if a datatype is specified");
                string text = lit.Value;
                string output = Regex.Replace(text, this._find, this._replace, this._options);

                return CreateOutputNode(lit, output);
            }
            throw new RdfQueryException("Cannot evaluate a Regular Expression against a non-Literal Node");
        }

        protected virtual IValuedNode CreateOutputNode(INode lit, string output)
        {
            if (lit.HasLanguage)
            {
                return new StringNode(output, lit.Language);
            }
            return lit.HasDataType ? new StringNode(output, lit.DataType) : new StringNode(output);
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is ReplaceFunction)) return false;

            ReplaceFunction func = (ReplaceFunction) other;
            if (this.Arguments.Count != func.Arguments.Count) return false;
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (!this.Arguments[i].Equals(func.Arguments[i])) return false;
            }
            return true;
        }


        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordReplace;
            }
        }
    }
}
