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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Class representing the SPARQL BNODE() function
    /// </summary>
    public class BNodeFunction : BaseUnaryExpression
    {
        private BNodeFunctionContext _funcContext;

        /// <summary>
        /// Creates a new BNode Function
        /// </summary>
        public BNodeFunction()
            : base(null) { }

        /// <summary>
        /// Creates a new BNode Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public BNodeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the value of the expression as evaluated in a given Context for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            this._funcContext = context[SparqlSpecsHelper.SparqlKeywordBNode] as BNodeFunctionContext;

            if (this._funcContext == null)
            {
                this._funcContext = new BNodeFunctionContext(context.InputMultiset.GetHashCode());
                context[SparqlSpecsHelper.SparqlKeywordBNode] = this._funcContext;
            }
            else if (this._funcContext.CurrentInput != context.InputMultiset.GetHashCode())
            {
                this._funcContext = new BNodeFunctionContext(context.InputMultiset.GetHashCode());
                context[SparqlSpecsHelper.SparqlKeywordBNode] = this._funcContext;
            }

            if (this._expr == null)
            {
                //If no argument then always a fresh BNode
                return new BlankNode(this._funcContext.Graph, this._funcContext.Mapper.GetNextID());
            }
            else
            {
                INode temp = this._expr.Value(context, bindingID);
                if (temp != null)
                {
                    if (temp.NodeType == NodeType.Literal)
                    {
                        ILiteralNode lit = (ILiteralNode)temp;

                        if (lit.DataType == null)
                        {
                            if (lit.Language.Equals(String.Empty))
                            {
                                if (!this._funcContext.BlankNodes.ContainsKey(bindingID))
                                {
                                    this._funcContext.BlankNodes.Add(bindingID, new Dictionary<string,INode>());
                                }

                                if (!this._funcContext.BlankNodes[bindingID].ContainsKey(lit.Value))
                                {
                                    this._funcContext.BlankNodes[bindingID].Add(lit.Value, new BlankNode(this._funcContext.Graph, this._funcContext.Mapper.GetNextID()));
                                }
                                return this._funcContext.BlankNodes[bindingID][lit.Value];
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create a Blank Node whne the argument Expression evaluates to a lanuage specified literal");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a typed literal node");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a non-literal node");
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to null");
                }
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordBNode;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Expression
        /// </summary>
        public override IEnumerable<string> Variables
        {
            get
            {
                if (this._expr == null) return Enumerable.Empty<String>();
                return base.Variables;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public override IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (this._expr == null) return Enumerable.Empty<ISparqlExpression>();
                return base.Arguments;
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordBNode + "(" + this._expr.ToSafeString() + ")";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new BNodeFunction(transformer.Transform(this._expr));
        }
    }

    class BNodeFunctionContext
    {
        private Dictionary<int, Dictionary<String, INode>> _bnodes = new Dictionary<int, Dictionary<string, INode>>();
        private BlankNodeMapper _mapper = new BlankNodeMapper("bnodeFunc");
        private Graph _g = new Graph();
        private int _currInput;

        public BNodeFunctionContext(int currInput)
        {
            this._currInput = currInput;
        }

        public int CurrentInput
        {
            get
            {
                return this._currInput;
            }
        }

        public BlankNodeMapper Mapper
        {
            get
            {
                return this._mapper;
            }
        }

        public IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        public Dictionary<int, Dictionary<String, INode>> BlankNodes
        {
            get
            {
                return this._bnodes;
            }
            set
            {
                this._bnodes = value;
            }
        }
    }

    /// <summary>
    /// Class representing the SPARQL BOUND() function
    /// </summary>
    public class BoundFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Bound() function expression
        /// </summary>
        /// <param name="varExpr">Variable Expression</param>
        public BoundFunction(VariableExpressionTerm varExpr) 
            : base(varExpr) { }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return (this._expr.Value(context, bindingID) != null);
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "BOUND(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordBound;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new BoundFunction((VariableExpressionTerm)transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the SPARQL Datatype() function
    /// </summary>
    public class DataTypeFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Datatype() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public DataTypeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Value(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the Data Type URI of an NULL");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                        ILiteralNode lit = (ILiteralNode)result;
                        if (lit.DataType == null)
                        {
                            if (!lit.Language.Equals(String.Empty))
                            {
                                throw new RdfQueryException("Cannot return the Data Type URI of Literals which have a Language Specifier");
                            }
                            else
                            {
                                return new UriNode(null, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                            }
                        }
                        else
                        {
                            return new UriNode(null, lit.DataType);
                        }

                    case NodeType.Uri:
                    case NodeType.Blank:
                    case NodeType.GraphLiteral:
                    default:
                        throw new RdfQueryException("Cannot return the Data Type URI of Nodes which are not Literal Nodes");
                }
            }
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "DATATYPE(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordDataType;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new DataTypeFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the SPARQL IRI() function
    /// </summary>
    public class IriFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new IRI() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public IriFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Value(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot create an IRI from a null");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                        ILiteralNode lit = (ILiteralNode)result;
                        String baseUri = String.Empty;
                        if (context.Query != null) baseUri = context.Query.BaseUri.ToSafeString();
                        String uri;
                        if (lit.DataType == null)
                        {
                            uri = Tools.ResolveUri(lit.Value, baseUri);
                            return new UriNode(null, new Uri(uri));
                        }
                        else
                        {
                            String dt = lit.DataType.ToString();
                            if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString, StringComparison.Ordinal))
                            {
                                uri = Tools.ResolveUri(lit.Value, baseUri);
                                return new UriNode(null, new Uri(uri));
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create an IRI from a non-string typed literal");
                            }
                        }
                        
                    case NodeType.Uri:
                        //Already a URI so nothing to do
                        return result;
                    default:
                        throw new RdfQueryException("Cannot create an IRI from a non-URI/String literal");
                }
            }
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The IRI() function does not have an effective boolean value");
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "IRI(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordIri;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IriFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the Sparql IsBlank() function
    /// </summary>
    public class IsBlankFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new IsBlank() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public IsBlankFunction(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Value(context, bindingID);
            if (result == null)
            {
                return false;
            }
            else
            {
                return (result.NodeType == NodeType.Blank);
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ISBLANK(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordIsBlank;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IsBlankFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the Sparql IsIRI() function
    /// </summary>
    public class IsIriFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new IsIRI() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public IsIriFunction(ISparqlExpression expr) 
            : base(expr) { }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Value(context, bindingID);
            if (result == null)
            {
                return false;
            }
            else
            {
                return (result.NodeType == NodeType.Uri);
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ISIRI(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordIsIri;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IsIriFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the Sparql IsLiteral() function
    /// </summary>
    public class IsLiteralFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new IsLiteral() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public IsLiteralFunction(ISparqlExpression expr) 
            : base(expr) { }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Value(context, bindingID);
            if (result == null)
            {
                return false;
            }
            else
            {
                return (result.NodeType == NodeType.Literal);
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ISLITERAL(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordIsLiteral;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IsLiteralFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the Sparql IsURI() function
    /// </summary>
    public class IsUriFunction 
        : IsIriFunction
    {
        /// <summary>
        /// Creates a new IsURI() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public IsUriFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ISURI(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordIsUri;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new IsUriFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the Sparql Lang() function
    /// </summary>
    public class LangFunction 
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Lang() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public LangFunction(ISparqlExpression expr) 
            : base(expr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Value(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the Data Type URI of an NULL");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                        return new LiteralNode(null, ((ILiteralNode)result).Language);

                    case NodeType.Uri:
                    case NodeType.Blank:
                    case NodeType.GraphLiteral:
                    default:
                        throw new RdfQueryException("Cannot return the Language Tag of Nodes which are not Literal Nodes");

                }
            }
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The LANG() function does not have an Effective Boolean Value");
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LANG(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordLang;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LangFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the Sparql LangMatches() function
    /// </summary>
    public class LangMatchesFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new LangMatches() function expression
        /// </summary>
        /// <param name="term">Expression to obtain the Language of</param>
        /// <param name="langRange">Expression representing the Language Range to match</param>
        public LangMatchesFunction(ISparqlExpression term, ISparqlExpression langRange) 
            : base(term, langRange) { }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._leftExpr.Value(context, bindingID);
            INode langRange = this._rightExpr.Value(context, bindingID);

            if (result == null)
            {
                return false;
            }
            else if (result.NodeType == NodeType.Literal)
            {
                if (langRange == null)
                {
                    return false;
                }
                else if (langRange.NodeType == NodeType.Literal)
                {
                    String range = ((ILiteralNode)langRange).Value;
                    String lang = ((ILiteralNode)result).Value;

                    if (range.Equals("*"))
                    {
                        return (!lang.Equals(String.Empty));
                    }
                    else
                    {
                        return lang.Equals(range, StringComparison.OrdinalIgnoreCase) || lang.StartsWith(range + "-", StringComparison.OrdinalIgnoreCase);
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LANGMATCHES(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordLangMatches;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LangMatchesFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Class representing the Sparql SameTerm() function
    /// </summary>
    public class SameTermFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new SameTerm() function expression
        /// </summary>
        /// <param name="term1">First Term</param>
        /// <param name="term2">Second Term</param>
        public SameTermFunction(ISparqlExpression term1, ISparqlExpression term2)
            : base(term1, term2) { }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode a, b;
            a = this._leftExpr.Value(context, bindingID);
            b = this._rightExpr.Value(context, bindingID);

            if (a == null)
            {
                if (b == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return a.Equals(b);
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "SAMETERM(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSameTerm;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new SameTermFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Class representing the Sparql Str() function
    /// </summary>
    public class StrFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Str() function expression
        /// </summary>
        /// <param name="expr">Expression to apply the function to</param>
        public StrFunction(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._expr.Value(context, bindingID);
            if (result == null)
            {
                throw new RdfQueryException("Cannot return the lexical value of an NULL");
            }
            else
            {
                switch (result.NodeType)
                {
                    case NodeType.Literal:
                        return new LiteralNode(null, ((ILiteralNode)result).Value);

                    case NodeType.Uri:
                        return new LiteralNode(null, ((IUriNode)result).Uri.ToString());

                    case NodeType.Blank:
                    case NodeType.GraphLiteral:
                    default:
                        throw new RdfQueryException("Cannot return the lexical value of Nodes which are not Literal/URI Nodes");

                }
            }
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The STR() function does not have an Effective Boolean Value");
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "STR(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function; 
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordStr; 
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StrFunction(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing the Sparql StrDt() function
    /// </summary>
    public class StrDtFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new STRDT() function expression
        /// </summary>
        /// <param name="stringExpr">String Expression</param>
        /// <param name="dtExpr">Datatype Expression</param>
        public StrDtFunction(ISparqlExpression stringExpr, ISparqlExpression dtExpr) 
            : base(stringExpr, dtExpr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode s = this._leftExpr.Value(context, bindingID);
            INode dt = this._rightExpr.Value(context, bindingID);

            if (s != null)
            {
                if (dt != null)
                {
                    Uri dtUri;
                    if (dt.NodeType == NodeType.Uri)
                    {
                        dtUri = ((IUriNode)dt).Uri;
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot create a datatyped literal when the datatype is a non-URI Node");
                    }
                    if (s.NodeType == NodeType.Literal)
                    {
                        ILiteralNode lit = (ILiteralNode)s;
                        if (lit.DataType == null)
                        {
                            if (lit.Language.Equals(String.Empty))
                            {
                                return new LiteralNode(null, lit.Value, dtUri);
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create a datatyped literal from a language specified literal");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot create a datatyped literal from a typed literal");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot create a datatyped literal from a non-literal Node");
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot create a datatyped literal from a null string");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot create a datatyped literal from a null string");
            }
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The STRDT() function does not have an Effective Boolean Value");
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "STRDT(" + this._leftExpr.ToString() + ", " + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordStrDt;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StrDtFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Class representing the Sparql StrDt() function
    /// </summary>
    public class StrLangFunction 
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new STRLANG() function expression
        /// </summary>
        /// <param name="stringExpr">String Expression</param>
        /// <param name="langExpr">Language Expression</param>
        public StrLangFunction(ISparqlExpression stringExpr, ISparqlExpression langExpr)
            : base(stringExpr, langExpr) { }

        /// <summary>
        /// Returns the value of the Expression as evaluated for a given Binding as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode s = this._leftExpr.Value(context, bindingID);
            INode lang = this._rightExpr.Value(context, bindingID);

            if (s != null)
            {
                if (lang != null)
                {
                    String langSpec;
                    if (lang.NodeType == NodeType.Literal)
                    {
                        ILiteralNode langLit = (ILiteralNode)lang;
                        if (langLit.DataType == null) 
                        {
                            langSpec = langLit.Value;
                        } 
                        else 
                        {
                            if (langLit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                            {
                                langSpec = langLit.Value;
                            } 
                            else 
                            {
                                throw new RdfQueryException("Cannot create a language specified literal when the language is a non-string literal");
                            }
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot create a language specified literal when the language is a non-literal Node");
                    }
                    if (s.NodeType == NodeType.Literal)
                    {
                        ILiteralNode lit = (ILiteralNode)s;
                        if (lit.DataType == null)
                        {
                            if (lit.Language.Equals(String.Empty))
                            {
                                return new LiteralNode(null, lit.Value, langSpec);
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot create a language specified literal from a language specified literal");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot create a language specified literal from a typed literal");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Cannot create a language specified literal from a non-literal Node");
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot create a language specified literal from a null string");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot create a language specified literal from a null string");
            }
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("The STRLANG() function does not have an Effective Boolean Value");
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "STRLANG(" + this._leftExpr.ToString() + ", " + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override String Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordStrLang;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new StrLangFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }

    /// <summary>
    /// Class representing the SPARQL REGEX function
    /// </summary>
    public class RegexFunction 
        : ISparqlExpression
    {
        private String _pattern = null;
        private RegexOptions _options = RegexOptions.None;
        private bool _fixedPattern = false, _fixedOptions = false;
        //private bool _useInStr = false;
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
            this._textExpr = text;
            this._patternExpr = pattern;

            //Get the Pattern
            if (pattern is NodeExpressionTerm)
            {
                //If the Pattern is a Node Expression Term then it is a fixed Pattern
                INode n = pattern.Value(null, 0);
                if (n.NodeType == NodeType.Literal)
                {
                    //Try to parse as a Regular Expression
                    try
                    {
                        String p = ((ILiteralNode)n).Value;
                        Regex temp = new Regex(p);

                        //It's a Valid Pattern
                        this._fixedPattern = true;
                        //this._useInStr = p.ToCharArray().All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c));
                        this._pattern = p;
                    }
                    catch
                    {
                        //No catch actions
                    }
                }
            }

            //Get the Options
            if (options != null)
            {
                this._optionExpr = options;
                if (options is NodeExpressionTerm)
                {
                    this.ConfigureOptions(options.Value(null, 0), false);
                    this._fixedOptions = true;
                    if (this._fixedPattern) this._regex = new Regex(this._pattern, this._options);
                }
            }
            else
            {
                if (this._fixedPattern) this._regex = new Regex(this._pattern);
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
            bool result = this.EffectiveBooleanValue(context, bindingID);
            return new LiteralNode(null, result.ToString().ToLower(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            //Configure Options
            if (this._optionExpr != null && !this._fixedOptions)
            {
                this.ConfigureOptions(this._optionExpr.Value(context, bindingID), true);
            }

            //Compile the Regex if necessary
            if (!this._fixedPattern)
            {
                //Regex is not pre-compiled
                if (this._patternExpr != null)
                {
                    INode p = this._patternExpr.Value(context, bindingID);
                    if (p != null)
                    {
                        if (p.NodeType == NodeType.Literal)
                        {
                            this._pattern = ((ILiteralNode)p).Value;
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
            INode textNode = this._textExpr.Value(context, bindingID);
            if (textNode == null)
            {
                throw new RdfQueryException("Cannot evaluate a Regular Expression against a NULL");
            }
            if (textNode.NodeType == NodeType.Literal)
            {
                //Execute
                String text = ((ILiteralNode)textNode).Value;
                if (this._regex != null)
                {
                    return this._regex.IsMatch(text);
                }
                else
                {
                    return Regex.IsMatch(text, this._pattern, this._options);
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
            output.Append(this._textExpr.ToString());
            output.Append(",");
            if (this._fixedPattern)
            {
                output.Append('"');
                output.Append(this._pattern);
                output.Append('"');
            }
            else
            {
                output.Append(this._patternExpr.ToString());
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
                if (this._patternExpr != null) vs.AddRange(this._patternExpr.Variables);
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
        public String Functor
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
                if (this._optionExpr != null)
                {
                    return new ISparqlExpression[] { this._textExpr, this._patternExpr, this._optionExpr };
                }
                else
                {
                    return new ISparqlExpression[] { this._textExpr, this._patternExpr };
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
                return new RegexFunction(transformer.Transform(this._textExpr), transformer.Transform(this._patternExpr), transformer.Transform(this._optionExpr));
            }
            else
            {
                return new RegexFunction(transformer.Transform(this._textExpr), transformer.Transform(this._patternExpr));
            }
        }
    }
}
