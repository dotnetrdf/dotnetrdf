/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#if !NO_WEB
using System.Web;
#endif
using VDS.RDF.Parsing;
using VDS.RDF.Query.Aggregates;

namespace VDS.RDF.Query.Expressions.Functions
{

    #region Base Classes

    /// <summary>
    /// Abstract Base Class for XPath Unary String functions
    /// </summary>
    public abstract class BaseUnaryXPathStringFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Expression the function applies over
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        /// Creates a new XPath Unary String function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public BaseUnaryXPathStringFunction(ISparqlExpression stringExpr)
        {
            this._expr = stringExpr;
        }

        /// <summary>
        /// Gets the Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                        {
                            return this.ValueInternal(lit);
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evalaute an XPath String function on a non-string typed Literal");
                        }
                    }
                    else
                    {
                        return this.ValueInternal(lit);
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an XPath String function on a non-Literal input");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath String function on a null input");
            }
        }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected abstract INode ValueInternal(ILiteralNode stringLit);

        /// <summary>
        /// Gets the Effective Boolean Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public virtual IEnumerable<string> Variables
        {
            get
            {
                return this._expr.Variables;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

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
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._expr.AsEnumerable();
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }

    /// <summary>
    /// Abstract Base class for XPath Binary String functions
    /// </summary>
    public abstract class BaseBinaryXPathStringFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Expression the function applies over
        /// </summary>
        protected ISparqlExpression _expr;
        /// <summary>
        /// Argument expression
        /// </summary>
        protected ISparqlExpression _arg;
        /// <summary>
        /// Whether the argument can be null
        /// </summary>
        protected bool _allowNullArgument = false;
        /// <summary>
        /// Type validation function for the argument
        /// </summary>
        protected Func<Uri, bool> _argumentTypeValidator;

        /// <summary>
        /// Creates a new XPath Binary String function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="argExpr">Argument</param>
        /// <param name="allowNullArgument">Whether the argument may be null</param>
        /// <param name="argumentTypeValidator">Type validator for the argument</param>
        public BaseBinaryXPathStringFunction(ISparqlExpression stringExpr, ISparqlExpression argExpr, bool allowNullArgument, Func<Uri, bool> argumentTypeValidator)
        {
            this._expr = stringExpr;
            this._arg = argExpr;
            this._allowNullArgument = allowNullArgument;
            if (this._arg == null && !this._allowNullArgument) throw new RdfParseException("Cannot create a XPath String Function which takes a String and a single argument since the expression for the argument is null");
            this._argumentTypeValidator = argumentTypeValidator;
        }

        /// <summary>
        /// Gets the Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;
                    if (lit.DataType != null && !lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        throw new RdfQueryException("Unable to evalaute an XPath String function on a non-string typed Literal");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an XPath String function on a non-Literal input");
                }

                //Once we've got to here we've established that the First argument is an appropriately typed/untyped Literal
                if (this._arg == null)
                {
                    return this.ValueInternal((ILiteralNode)temp);
                }
                else
                {
                    //Need to validate the argument
                    INode tempArg = this._arg.Value(context, bindingID);
                    if (tempArg != null)
                    {
                        if (tempArg.NodeType == NodeType.Literal)
                        {
                            ILiteralNode litArg = (ILiteralNode)tempArg;
                            if (this._argumentTypeValidator(litArg.DataType))
                            {
                                return this.ValueInternal((ILiteralNode)temp, litArg);
                            }
                            else
                            {
                                throw new RdfQueryException("Unale to evaluate an XPath String function since the type of the argument is not supported by this function");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate an XPath String function where the argument is a non-Literal");
                        }
                    }
                    else if (this._allowNullArgument)
                    {
                        //Null argument permitted so just invoke the non-argument version of the function
                        return this.ValueInternal((ILiteralNode)temp);
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evaluate an XPath String function since the argument expression evaluated to a null and a null argument is not permitted by this function");
                    }
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath String function on a null input");
            }
        }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        public virtual INode ValueInternal(ILiteralNode stringLit)
        {
            if (!this._allowNullArgument) 
            {
                throw new RdfQueryException("This XPath function requires a non-null argument in addition to an input string");
            }
            else 
            {
                throw new RdfQueryException("Derived classes which are functions which permit a null argument must override this method");
            }
        }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public abstract INode ValueInternal(ILiteralNode stringLit, ILiteralNode arg);

        /// <summary>
        /// Gets the Effective Boolean Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public virtual IEnumerable<string> Variables
        {
            get
            {
                if (this._arg == null)
                {
                    return this._expr.Variables;
                }
                else
                {
                    return this._expr.Variables.Concat(this._arg.Variables);
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

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
        public abstract String Functor
        {
            get;
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return new ISparqlExpression[] { this._expr, this._arg };
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
    }

    #endregion

    #region Unary String Functions

    /// <summary>
    /// Represents the XPath fn:string-length() function
    /// </summary>
    public class XPathStringLengthFunction
        : BaseUnaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath String Length function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public XPathStringLengthFunction(ISparqlExpression stringExpr)
            : base(stringExpr) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {
            return new LiteralNode(null, stringLit.Value.Length.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StringLength + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StringLength;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathStringLengthFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath fn:encode-for-uri() function
    /// </summary>
    public class XPathEncodeForUriFunction 
        : BaseUnaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Encode for URI function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public XPathEncodeForUriFunction(ISparqlExpression stringExpr)
            : base(stringExpr) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {
            return new LiteralNode(null, Uri.EscapeUriString(stringLit.Value), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.EncodeForURI + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.EncodeForURI;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathEncodeForUriFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath fn:escape-html-uri() function
    /// </summary>
    public class XPathEscapeHtmlUriFunction
        : BaseUnaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Escape HTML for URI function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public XPathEscapeHtmlUriFunction(ISparqlExpression stringExpr)
            : base(stringExpr) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {

            return new LiteralNode(null, HttpUtility.UrlEncode(stringLit.Value), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.EscapeHtmlURI + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.EscapeHtmlURI;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathEscapeHtmlUriFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath fn:lower-case() function
    /// </summary>
    public class XPathLowerCaseFunction
        : BaseUnaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Lower Case function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public XPathLowerCaseFunction(ISparqlExpression stringExpr)
            : base(stringExpr) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {
            return new LiteralNode(null, stringLit.Value.ToLower(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.LowerCase + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.LowerCase;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathLowerCaseFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath fn:upper-case() function
    /// </summary>
    public class XPathUpperCaseFunction
        : BaseUnaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Upper Case function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public XPathUpperCaseFunction(ISparqlExpression stringExpr)
            : base(stringExpr) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {
            return new LiteralNode(null, stringLit.Value.ToUpper(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.UpperCase + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.UpperCase;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathUpperCaseFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Represents the XPath fn:normalize-space() function
    /// </summary>
    public class XPathNormalizeSpaceFunction
        : BaseUnaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Normalize Space function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public XPathNormalizeSpaceFunction(ISparqlExpression stringExpr)
            : base(stringExpr) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        protected override INode ValueInternal(ILiteralNode stringLit)
        {
            String temp = stringLit.Value.Trim();
            Regex normalizeSpace = new Regex("\\s{2,}");
            temp = normalizeSpace.Replace(temp, " ");

            return new LiteralNode(null, temp, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeSpace + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeSpace;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathNormalizeSpaceFunction(transformer.Transform(this._expr));
        }
    }

    #endregion

    #region Binary String Functions

#if !NO_NORM

    /// <summary>
    /// Represents the XPath fn:normalize-unicode() function
    /// </summary>
    public class XPathNormalizeUnicodeFunction 
        : BaseBinaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Normalize Unicode function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        public XPathNormalizeUnicodeFunction(ISparqlExpression stringExpr)
            : base(stringExpr, null, true, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Creates a new XPath Normalize Unicode function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="normalizationFormExpr">Normalization Form</param>
        public XPathNormalizeUnicodeFunction(ISparqlExpression stringExpr, ISparqlExpression normalizationFormExpr)
            : base(stringExpr, normalizationFormExpr, true, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <returns></returns>
        public override INode ValueInternal(ILiteralNode stringLit)
        {
            return new LiteralNode(null, stringLit.Value.Normalize(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override INode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            if (arg == null)
            {
                return this.ValueInternal(stringLit);
            }
            else
            {
                String normalized = stringLit.Value;

                switch (arg.Value)
                {
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormC:
                        normalized = normalized.Normalize();
                        break;
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormD:
                        normalized = normalized.Normalize(NormalizationForm.FormD);
                        break;
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormFull:
                        throw new RdfQueryException(".Net does not support Fully Normalized Unicode Form");
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormKC:
                        normalized = normalized.Normalize(NormalizationForm.FormKC);
                        break;
                    case XPathFunctionFactory.XPathUnicodeNormalizationFormKD:
                        normalized = normalized.Normalize(NormalizationForm.FormKD);
                        break;
                    case "":
                        //No Normalization
                        break;
                    default:
                        throw new RdfQueryException("'" + arg.Value + "' is not a valid Normalization Form as defined by the XPath specification");
                }

                return new LiteralNode(null, normalized, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._arg != null)
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeUnicode + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
            }
            else
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeUnicode + ">(" + this._expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.NormalizeUnicode;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (this._arg != null)
            {
                return new XPathNormalizeUnicodeFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
            }
            else
            {
                return new XPathNormalizeUnicodeFunction(transformer.Transform(this._expr));
            }
        }
    }

#endif

    /// <summary>
    /// Represents the XPath fn:contains() function
    /// </summary>
    public class XPathContainsFunction
        : BaseBinaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Contains function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="searchExpr">Search Expression</param>
        public XPathContainsFunction(ISparqlExpression stringExpr, ISparqlExpression searchExpr)
            : base(stringExpr, searchExpr, false, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override INode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            if (stringLit.Value.Equals(String.Empty))
            {
                //Empty string cannot contain anything
                return new LiteralNode(null, "false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
            }
            else if (arg.Value.Equals(String.Empty))
            {
                //Any non-empty string contains the empty string
                return new LiteralNode(null, "true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
            }
            else
            {
                //Evalute the Contains
                return new LiteralNode(null, stringLit.Value.Contains(arg.Value).ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Contains + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Contains;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathContainsFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }

    /// <summary>
    /// Represents the XPath fn:ends-with() function
    /// </summary>
    public class XPathEndsWithFunction
        : BaseBinaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Ends With function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="suffixExpr">Suffix Expression</param>
        public XPathEndsWithFunction(ISparqlExpression stringExpr, ISparqlExpression suffixExpr)
            : base(stringExpr, suffixExpr, false, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override INode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            if (stringLit.Value.Equals(String.Empty))
            {
                if (arg.Value.Equals(String.Empty))
                {
                    //The Empty String ends with the Empty String
                    return new LiteralNode(null, "true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                }
                else
                {
                    //Empty String doesn't end with a non-empty string
                    return new LiteralNode(null, "false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                }
            }
            else if (arg.Value.Equals(String.Empty))
            {
                //Any non-empty string ends with the empty string
                return new LiteralNode(null, "true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
            }
            else
            {
                //Otherwise evalute the EndsWith
                return new LiteralNode(null, stringLit.Value.EndsWith(arg.Value).ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.EndsWith + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.EndsWith;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathEndsWithFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }

    /// <summary>
    /// Represents the XPath fn:starts-with() function
    /// </summary>
    public class XPathStartsWithFunction
        : BaseBinaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Starts With function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="prefixExpr">Prefix Expression</param>
        public XPathStartsWithFunction(ISparqlExpression stringExpr, ISparqlExpression prefixExpr)
            : base(stringExpr, prefixExpr, false, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override INode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            if (stringLit.Value.Equals(String.Empty))
            {
                if (arg.Value.Equals(String.Empty))
                {
                    //The Empty String starts with the Empty String
                    return new LiteralNode(null, "true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                }
                else
                {
                    //Empty String doesn't start with a non-empty string
                    return new LiteralNode(null, "false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                }
            }
            else if (arg.Value.Equals(String.Empty))
            {
                //Any non-empty string starts with the empty string
                return new LiteralNode(null, "true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
            }
            else
            {
                //Otherwise evalute the StartsWith
                return new LiteralNode(null, stringLit.Value.StartsWith(arg.Value).ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StartsWith + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StartsWith;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathStartsWithFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }

    /// <summary>
    /// Represents the XPath fn:substring-before() function
    /// </summary>
    public class XPathSubstringBeforeFunction
        : BaseBinaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Substring Before function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="findExpr">Search Expression</param>
        public XPathSubstringBeforeFunction(ISparqlExpression stringExpr, ISparqlExpression findExpr)
            : base(stringExpr, findExpr, false, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override INode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            if (arg.Value.Equals(String.Empty))
            {
                //The substring before the empty string is the empty string
                return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            else
            {
                //Does the String contain the search string?
                if (stringLit.Value.Contains(arg.Value))
                {
                    String result = stringLit.Value.Substring(0, stringLit.Value.IndexOf(arg.Value));
                    return new LiteralNode(null, result, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                else
                {
                    //If it doesn't contain the search string the empty string is returned
                    return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SubstringBefore + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SubstringBefore;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathSubstringBeforeFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }

    /// <summary>
    /// Represents the XPath fn:substring-after() function
    /// </summary>
    public class XPathSubstringAfterFunction
        : BaseBinaryXPathStringFunction
    {
        /// <summary>
        /// Creates a new XPath Substring After function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="findExpr">Search Expression</param>
        public XPathSubstringAfterFunction(ISparqlExpression stringExpr, ISparqlExpression findExpr)
            : base(stringExpr, findExpr, false, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override INode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            if (arg.Value.Equals(String.Empty))
            {
                //The substring after the empty string is the input string
                return new LiteralNode(null, stringLit.Value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            else
            {
                //Does the String contain the search string?
                if (stringLit.Value.Contains(arg.Value))
                {
                    String result = stringLit.Value.Substring(stringLit.Value.IndexOf(arg.Value) + arg.Value.Length);
                    return new LiteralNode(null, result, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                else
                {
                    //If it doesn't contain the search string the empty string is returned
                    return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SubstringAfter + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SubstringAfter;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathSubstringAfterFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }

    /// <summary>
    /// Represents the XPath fn:compare() function
    /// </summary>
    public class XPathCompareFunction 
        : BaseBinaryXPathStringFunction, ISparqlNumericExpression
    {
        /// <summary>
        /// Creates a new XPath Compare function
        /// </summary>
        /// <param name="a">First Comparand</param>
        /// <param name="b">Second Comparand</param>
        public XPathCompareFunction(ISparqlExpression a, ISparqlExpression b)
            : base(a, b, false, XPathFunctionFactory.AcceptStringArguments) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        public override INode ValueInternal(ILiteralNode stringLit, ILiteralNode arg)
        {
            return new LiteralNode(null, String.Compare(stringLit.Value, arg.Value).ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Compare + ">(" + this._expr.ToString() + "," + this._arg.ToString() + ")";
        }

        /// <summary>
        /// Gets the Numeric Type of this function as evaluated in the given Context for the given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns>
        /// Either <see cref="SparqlNumericType.Integer">Integer</see> or <see cref="SparqlNumericType.NaN">NaN</see>
        /// </returns>
        public SparqlNumericType NumericType(SparqlEvaluationContext context, int bindingID)
        {
            try
            {
                INode temp = this.Value(context, bindingID);
                return SparqlNumericType.Integer;
            }
            catch
            {
                return SparqlNumericType.NaN;
            }
        }

        /// <summary>
        /// Returns the numeric value of the Expression as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.ToInteger((ILiteralNode)this.Value(context, bindingID));
        }

        /// <summary>
        /// Returns the integer value of the Expression as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public long IntegerValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToInt64(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Returns the decimal value of the Expression as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public decimal DecimalValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToDecimal(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Returns the float value of the Expression as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public float FloatValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToSingle(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Returns the double value of the Expression as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public double DoubleValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToDouble(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Compare;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathCompareFunction(transformer.Transform(this._expr), transformer.Transform(this._arg));
        }
    }

    #endregion

    /// <summary>
    /// Represents the XPath fn:substring() function
    /// </summary>
    public class XPathSubstringFunction
        : ISparqlExpression
    {
        private ISparqlExpression _expr, _start, _length;

        /// <summary>
        /// Creates a new XPath Substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Start</param>
        public XPathSubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
            : this(stringExpr, startExpr, null) { }

        /// <summary>
        /// Creates a new XPath Substring function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="startExpr">Start</param>
        /// <param name="lengthExpr">Length</param>
        public XPathSubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression lengthExpr)
        {
            this._expr = stringExpr;
            this._start = startExpr;
            this._length = lengthExpr;
        }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            ILiteralNode input = this.CheckArgument(this._expr, context, bindingID);
            ILiteralNode start = this.CheckArgument(this._start, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

            if (this._length != null)
            {
                ILiteralNode length = this.CheckArgument(this._length, context, bindingID, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(String.Empty)) return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.Value);
                    int l = Convert.ToInt32(length.Value);

                    if (s < 1) s = 1;
                    if (l < 1)
                    {
                        //If no/negative characters are being selected the empty string is returned
                        return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else if ((s-1) > input.Value.Length)
                    {
                        //If the start is after the end of the string the empty string is returned
                        return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else
                    {
                        if (((s - 1) + l) > input.Value.Length)
                        {
                            //If the start plus the length is greater than the length of the string the string from the starts onwards is returned
                            return new LiteralNode(null, input.Value.Substring(s - 1), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                        }
                        else
                        {
                            //Otherwise do normal substring
                            return new LiteralNode(null, input.Value.Substring(s - 1, l), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                        }
                    }
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start/Length argument to an Integer");
                }
            }
            else
            {
                if (input.Value.Equals(String.Empty)) return new LiteralNode(null, String.Empty, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    int s = Convert.ToInt32(start.Value);
                    if (s < 1) s = 1;

                    return new LiteralNode(null, input.Value.Substring(s-1), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start argument to an Integer");
                }
            }
        }

        private ILiteralNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID)
        {
            return this.CheckArgument(expr, context, bindingID, XPathFunctionFactory.AcceptStringArguments);
        }

        private ILiteralNode CheckArgument(ISparqlExpression expr, SparqlEvaluationContext context, int bindingID, Func<Uri, bool> argumentTypeValidator)
        {
            INode temp = expr.Value(context, bindingID);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (argumentTypeValidator(lit.DataType))
                        {
                            //Appropriately typed literals are fine
                            return lit;
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate an XPath substring as one of the argument expressions returned a typed literal with an invalid type");
                        }
                    }
                    else if (argumentTypeValidator(new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        //Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return lit;
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evalaute an XPath substring as one of the argument expressions returned an untyped literal");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an XPath substring as one of the argument expressions returned a non-literal");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath substring as one of the argument expressions evaluated to null");
            }
        }

        /// <summary>
        /// Returns the Effective Boolean Value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                if (this._length != null)
                {
                    return this._expr.Variables.Concat(this._start.Variables).Concat(this._length.Variables);
                }
                else
                {
                    return this._expr.Variables.Concat(this._start.Variables);
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._length != null)
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Substring + ">(" + this._expr.ToString() + "," + this._start.ToString() + "," + this._length.ToString() + ")";
            }
            else
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Substring + ">(" + this._expr.ToString() + "," + this._start.ToString() + ")";
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
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Substring;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (this._length != null)
                {
                    return new ISparqlExpression[] { this._expr, this._start, this._length };
                }
                else
                {
                    return new ISparqlExpression[] { this._expr, this._start };
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (this._length != null)
            {
                return new XPathSubstringFunction(transformer.Transform(this._expr), transformer.Transform(this._start), transformer.Transform(this._length));
            }
            else
            {
                return new XPathSubstringFunction(transformer.Transform(this._expr), transformer.Transform(this._start));
            }
        }
    }

    /// <summary>
    /// Represents the XPath fn:replace() function
    /// </summary>
    public class XPathReplaceFunction 
        : ISparqlExpression
    {
        private String _find = null;
        private String _replace = null;
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
        public XPathReplaceFunction(ISparqlExpression text, ISparqlExpression find, ISparqlExpression replace)
            : this(text, find, replace, null) { }

        /// <summary>
        /// Creates a new XPath Replace function
        /// </summary>
        /// <param name="text">Text Expression</param>
        /// <param name="find">Search Expression</param>
        /// <param name="replace">Replace Expression</param>
        /// <param name="options">Options Expression</param>
        public XPathReplaceFunction(ISparqlExpression text, ISparqlExpression find, ISparqlExpression replace, ISparqlExpression options)
        {
            this._textExpr = text;

            //Get the Pattern
            if (find is NodeExpressionTerm)
            {
                //If the Pattern is a Node Expression Term then it is a fixed Pattern
                INode n = find.Value(null, 0);
                if (n.NodeType == NodeType.Literal)
                {
                    //Try to parse as a Regular Expression
                    try
                    {
                        String p = ((ILiteralNode)n).Value;
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
            else
            {
                this._findExpr = find;
            }

            //Get the Replace
            if (replace is NodeExpressionTerm)
            {
                //If the Replace is a Node Expresison Term then it is a fixed Pattern
                INode n = replace.Value(null, 0);
                if (n.NodeType == NodeType.Literal)
                {
                    this._replace = ((ILiteralNode)n).Value;
                    this._fixedReplace = true;
                }
            }
            else
            {
                this._replaceExpr = replace;
            }

            //Get the Options
            if (options != null)
            {
                if (options is NodeExpressionTerm)
                {
                    this.ConfigureOptions(options.Value(null, 0), false);
                }
                else
                {
                    this._optionExpr = options;
                }
            }
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
                    String ops = ((ILiteralNode)n).Value;
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
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            //Configure Options
            if (this._optionExpr != null)
            {
                this.ConfigureOptions(this._optionExpr.Value(context, bindingID), true);
            }

            //Compile the Regex if necessary
            if (!this._fixedPattern)
            {
                //Regex is not pre-compiled
                if (this._findExpr != null)
                {
                    INode p = this._findExpr.Value(context, bindingID);
                    if (p != null)
                    {
                        if (p.NodeType == NodeType.Literal)
                        {
                            this._find = ((ILiteralNode)p).Value;
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
                    INode r = this._replaceExpr.Value(context, bindingID);
                    if (r != null)
                    {
                        if (r.NodeType == NodeType.Literal)
                        {
                            this._replace = ((ILiteralNode)r).Value;
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
            INode textNode = this._textExpr.Value(context, bindingID);
            if (textNode == null)
            {
                throw new RdfQueryException("Cannot evaluate a Regular Expression against a NULL");
            }
            if (textNode.NodeType == NodeType.Literal)
            {
                //Execute
                String text = ((ILiteralNode)textNode).Value;
                String output = Regex.Replace(text, this._find, this._replace, this._options);
                return new LiteralNode(null, output, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            else
            {
                throw new RdfQueryException("Cannot evaluate a Regular Expression against a non-Literal Node");
            }
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
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
        public IEnumerable<String> Variables
        {
            get
            {
                List<String> vs = new List<String>();
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
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (this._optionExpr != null)
            {
                return new XPathReplaceFunction(transformer.Transform(this._textExpr), transformer.Transform(this._findExpr), transformer.Transform(this._replaceExpr), transformer.Transform(this._optionExpr));
            }
            else
            {
                return new XPathReplaceFunction(transformer.Transform(this._textExpr), transformer.Transform(this._findExpr), transformer.Transform(this._replaceExpr));
            }
        }
    }

    /// <summary>
    /// Represents the XPath fn:concat() function
    /// </summary>
    public class XPathConcatFunction 
        : ISparqlExpression
    {
        private List<ISparqlExpression> _exprs = new List<ISparqlExpression>();

        /// <summary>
        /// Creates a new XPath Concatenation function
        /// </summary>
        /// <param name="first">First Expression</param>
        /// <param name="second">Second Expression</param>
        public XPathConcatFunction(ISparqlExpression first, ISparqlExpression second)
        {
            this._exprs.Add(first);
            this._exprs.Add(second);
        }

        /// <summary>
        /// Creates a new XPath Concatenation function
        /// </summary>
        /// <param name="expressions">Enumeration of expressions</param>
        public XPathConcatFunction(IEnumerable<ISparqlExpression> expressions)
        {
            this._exprs.AddRange(expressions);
        }

        /// <summary>
        /// Gets the Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            StringBuilder output = new StringBuilder();
            foreach (ISparqlExpression expr in this._exprs)
            {
                INode temp = expr.Value(context, bindingID);
                if (temp == null) throw new RdfQueryException("Cannot evaluate the XPath concat() function when an argument evaluates to a Null");
                switch (temp.NodeType)
                {
                    case NodeType.Literal:
                        output.Append(((ILiteralNode)temp).Value);
                        break;
                    default:
                        throw new RdfQueryException("Cannot evaluate the XPath concat() function when an argument is not a Literal Node");
                }
            }

            return new LiteralNode(null, output.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the Effective Boolean Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the Arguments the function applies to
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get 
            {
                return this._exprs;
            }
        }

        /// <summary>
        /// Gets the Variables used in the function
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return (from expr in this._exprs
                        from v in expr.Variables
                        select v);
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(XPathFunctionFactory.XPathFunctionsNamespace);
            output.Append(XPathFunctionFactory.Concat);
            output.Append(">(");
            for (int i = 0; i < this._exprs.Count; i++)
            {
                output.Append(this._exprs[i].ToString());
                if (i < this._exprs.Count - 1) output.Append(", ");
            }
            output.Append(")");
            return output.ToString();
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
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Concat;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathConcatFunction(this._exprs.Select(e => transformer.Transform(e)));
        }
    }

    /// <summary>
    /// Represents the XPath fn:string-join() aggregate
    /// </summary>
    public class XPathStringJoinFunction
        : BaseAggregate
    {
        /// <summary>
        /// Separator Expression
        /// </summary>
        protected ISparqlExpression _sep;
        private bool _customSep = true;

        /// <summary>
        /// Creates a new XPath String Join aggregate which uses no separator
        /// </summary>
        /// <param name="expr">Expression</param>
        public XPathStringJoinFunction(ISparqlExpression expr)
            : this(expr, new NodeExpressionTerm(new LiteralNode(null, String.Empty))) 
        {
            this._customSep = false;
        }

        /// <summary>
        /// Creates a new XPath String Join aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="sep">Separator Expression</param>
        public XPathStringJoinFunction(ISparqlExpression expr, ISparqlExpression sep)
            : base(expr)
        {
            this._sep = sep;
        }

        /// <summary>
        /// Applies the Aggregate in the given Context over the given Binding IDs
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        public override INode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            List<int> ids = bindingIDs.ToList();
            StringBuilder output = new StringBuilder();
            HashSet<String> values = new HashSet<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                try
                {
                    String temp = this.ValueInternal(context, ids[i]);

                    //Apply DISTINCT modifer if required
                    if (this._distinct)
                    {
                        if (values.Contains(temp))
                        {
                            continue;
                        }
                        else
                        {
                            values.Add(temp);
                        }
                    }
                    output.Append(temp);
                }
                catch (RdfQueryException)
                {
                    output.Append(String.Empty);
                }

                //Append Separator if required
                if (i < ids.Count - 1)
                {
                    String sep = this.GetSeparator(context, ids[i]);
                    output.Append(sep);
                }
            }

            return new LiteralNode(null, output.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        /// <summary>
        /// Gets the value of a member of the Group for concatenating as part of the result for the Group
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private String ValueInternal(SparqlEvaluationContext context, int bindingID) 
        {
                INode temp = this._expr.Value(context, bindingID);
                if (temp == null) throw new RdfQueryException("Cannot do an XPath string-join on a null");
                if (temp.NodeType == NodeType.Literal)
                {
                    ILiteralNode l = (ILiteralNode)temp;
                    if (l.DataType != null)
                    {
                        if (l.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                        {
                            return l.Value;
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot do an XPath string-join on a Literal which is not typed as a String");
                        }
                    }
                    else
                    {
                        return l.Value;
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot do an XPath string-join on a non-Literal Node");
                }
        }

        /// <summary>
        /// Gets the separator to use in the concatenation
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        private String GetSeparator(SparqlEvaluationContext context, int bindingID)
        {
            INode temp = this._sep.Value(context, bindingID);
            if (temp == null)
            {
                return String.Empty;
            }
            else if (temp.NodeType == NodeType.Literal)
            {
                ILiteralNode l = (ILiteralNode)temp;
                if (l.DataType != null)
                {
                    if (l.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        return l.Value;
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot evaluate an XPath string-join since the separator expression returns a typed Literal which is not a String");
                    }
                }
                else
                {
                    return l.Value;
                }
            }
            else
            {
                throw new RdfQueryException("Cannot evaluate an XPath string-join since the separator expression does not return a Literal");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(XPathFunctionFactory.XPathFunctionsNamespace);
            output.Append(XPathFunctionFactory.StringJoin);
            output.Append(">(");
            if (this._distinct) output.Append("DISTINCT ");
            output.Append(this._expr.ToString());
            if (this._customSep)
            {
                output.Append(this._sep.ToString());
            }
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StringJoin;
            }
        }
    }
}
