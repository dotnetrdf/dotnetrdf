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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Abstract Expression class used as the base class for implementation of XPath Casting Function expressions
    /// </summary>
    public abstract class BaseXPathCast
        : ISparqlExpression 
    {
        /// <summary>
        /// Expression to be Cast by the Cast Function
        /// </summary>
        protected ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Base XPath Cast Expression
        /// </summary>
        /// <param name="expr">Expression to be Cast</param>
        public BaseXPathCast(ISparqlExpression expr) {
            this._expr = expr;
        }

        /// <summary>
        /// Gets the value of Casting the inner Expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual INode Value(SparqlEvaluationContext context, int bindingID) 
        {
            throw new NotImplementedException("Derived Class does not implement a Value method");
        }

        /// <summary>
        /// Gets the Effective Boolean Value of this Expressions result
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Gets the enumeration of Variables involved in this expression
        /// </summary>
        public virtual IEnumerable<String> Variables
        {
            get
            {
                return this._expr.Variables;
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
    /// Class representing an XPath Boolean Cast Function
    /// </summary>
    public class XPathBooleanCast
        : BaseXPathCast
    {
        /// <summary>
        /// Creates a new XPath Boolean Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public XPathBooleanCast(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Casts the value of the inner Expression to a Boolean
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode n = this._expr.Value(context, bindingID);

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:boolean");
            }

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:boolean");

                case NodeType.Literal:
                    //See if the value can be cast
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        String dt = lit.DataType.ToString();

                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                        {
                            //Already a Boolean
                            return lit;
                        }

                        //Cast based on Numeric Type
                        SparqlNumericType type = SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(dt);

                        switch (type)
                        {
                            case SparqlNumericType.Decimal:
                                Decimal dec;
                                if (Decimal.TryParse(lit.Value, out dec))
                                {
                                    if (dec.Equals(Decimal.Zero))
                                    {
                                        return new LiteralNode(lit.Graph, "false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                                    }
                                    else
                                    {
                                        return new LiteralNode(lit.Graph, "true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                                    }
                                }
                                else
                                {
                                    throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:decimal as an intermediate stage in casting to a xsd:boolean");
                                }

                            case SparqlNumericType.Double:
                                Double dbl;
                                if (Double.TryParse(lit.Value, out dbl))
                                {
                                    if (Double.IsNaN(dbl) || dbl == 0.0d)
                                    {
                                        return new LiteralNode(lit.Graph, "false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                                    }
                                    else
                                    {
                                        return new LiteralNode(lit.Graph, "true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                                    }
                                }
                                else
                                {
                                    throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double as an intermediate stage in casting to a xsd:boolean");
                                }

                            case SparqlNumericType.Integer:
                                Int64 i;
                                if (Int64.TryParse(lit.Value, out i))
                                {
                                    if (i == 0)
                                    {
                                        return new LiteralNode(lit.Graph, "false", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                                    }
                                    else
                                    {
                                        return new LiteralNode(lit.Graph, "true", new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                                    }
                                }
                                else
                                {
                                    throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:integer as an intermediate stage in casting to a xsd:boolean");
                                }

                            case SparqlNumericType.NaN:
                                if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                                {
                                    //DateTime cast forbidden
                                    throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:boolean");
                                }
                                else
                                {
                                    Boolean b;
                                    if (Boolean.TryParse(lit.Value, out b))
                                    {
                                        return new LiteralNode(lit.Graph, b.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                                    }
                                    else
                                    {
                                        throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:boolean");
                                    }
                                }

                            default:
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:boolean");
                        }
                    }
                    else
                    {
                        Boolean b;
                        if (Boolean.TryParse(lit.Value, out b))
                        {
                            return new LiteralNode(lit.Graph, b.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:boolean");
                        }
                    }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:decimal");
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeBoolean + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeBoolean; 
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathBooleanCast(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing an XPath Double Cast Function
    /// </summary>
    public class XPathDoubleCast 
        : BaseXPathCast
    {
        /// <summary>
        /// Creates a new XPath Double Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public XPathDoubleCast(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Casts the value of the inner Expression to a Double
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode n = this._expr.Value(context, bindingID);

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:double");
            }

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:double");

                case NodeType.Literal:
                    //See if the value can be cast
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        String dt = lit.DataType.ToString();
                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble))
                        {
                            //Already a xsd:double
                            return lit;
                        }
                        else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
                        {
                            //Already a xsd:float so valid as a xsd:double
                            return new LiteralNode(lit.Graph, lit.Value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
                        }
                        else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            //DateTime cast forbidden
                            throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:double");
                        }
                        else
                        {
                            double d;
                            if (Double.TryParse(lit.Value, out d))
                            {
                                //Parsed OK
                                return new LiteralNode(lit.Graph, d.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double");
                            }
                        }
                    }
                    else
                    {
                        double d;
                        if (Double.TryParse(lit.Value, out d))
                        {
                            //Parsed OK
                            return new LiteralNode(lit.Graph, d.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double");
                        }
                    }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:double");
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeDouble + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeDouble;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathDoubleCast(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing an XPath Float Cast Function
    /// </summary>
    public class XPathFloatCast
        : BaseXPathCast
    {
        /// <summary>
        /// Creates a new XPath Float Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public XPathFloatCast(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Casts the value of the inner Expression to a Float
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Vinding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode n = this._expr.Value(context, bindingID);

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:float");
            }

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:float");

                case NodeType.Literal:
                    //See if the value can be cast
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        if (lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
                        {
                            //Already an xsd:float
                            return lit;
                        }
                        else if (lit.DataType.ToString().Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            //DateTime cast forbidden
                            throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:float");
                        }
                        else
                        {
                            float f;
                            if (Single.TryParse(lit.Value, out f))
                            {
                                //Parsed OK
                                return new LiteralNode(lit.Graph, f.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat));
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:float");
                            }
                        }
                    }
                    else
                    {
                        float f;
                        if (Single.TryParse(lit.Value, out f))
                        {
                            //Parsed OK
                            return new LiteralNode(lit.Graph, f.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat));
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:float");
                        }
                    }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:float");
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeFloat + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeFloat;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathFloatCast(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing an XPath Decimal Cast Function
    /// </summary>
    public class XPathDecimalCast 
        : BaseXPathCast
    {
        /// <summary>
        /// Creates a new XPath Decimal Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public XPathDecimalCast(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Casts the Value of the inner Expression to a Decimal
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode n = this._expr.Value(context, bindingID);

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:decimal");
            }

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:decimal");

                case NodeType.Literal:
                    //See if the value can be cast
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        String dt = lit.DataType.ToString();
                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDecimal))
                        {
                            //Already a xsd:decimal
                            return lit;
                        }
                        else if (SparqlSpecsHelper.IntegerDataTypes.Contains(dt))
                        {
                            //Already an integer type so valid as a xsd:decimal
                            return new LiteralNode(lit.Graph, lit.Value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
                        }
                        else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            //DateTime cast forbidden
                            throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:decimal");
                        }
                        else
                        {
                            decimal d;
                            if (Decimal.TryParse(lit.Value, out d))
                            {
                                //Parsed OK
                                return new LiteralNode(lit.Graph, d.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:decimal");
                            }
                        }
                    }
                    else
                    {
                        decimal d;
                        if (Decimal.TryParse(lit.Value, out d))
                        {
                            //Parsed OK
                            return new LiteralNode(lit.Graph, d.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:decimal");
                        }
                    }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:decimal");
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeDecimal + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeDecimal;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathDecimalCast(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing an XPath Integer Cast Function
    /// </summary>
    public class XPathIntegerCast 
        : BaseXPathCast
    {
        /// <summary>
        /// Creates a new XPath Integer Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public XPathIntegerCast(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Casts the value of the inner Expression to an Integer
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode n = this._expr.Value(context, bindingID);

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:integer");
            }

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:integer");

                case NodeType.Literal:
                    //See if the value can be cast
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        String dt = lit.DataType.ToString();
                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeInteger))
                        {
                            //Already a xsd:integer
                            return lit;
                        }
                        else if (SparqlSpecsHelper.IntegerDataTypes.Contains(dt))
                        {
                            //Already a integer type so valid as a xsd:integer
                            return new LiteralNode(lit.Graph, lit.Value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
                        }
                        else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            //DateTime cast forbidden
                            throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:integer");
                        }
                        else
                        {
                            Int64 i;
                            if (Int64.TryParse(lit.Value, out i))
                            {
                                //Parsed OK
                                return new LiteralNode(lit.Graph, i.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:integer");
                            }
                        }
                    }
                    else
                    {
                        Int64 i;
                        if (Int64.TryParse(lit.Value, out i))
                        {
                            //Parsed OK
                            return new LiteralNode(lit.Graph, i.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:integer");
                        }
                    }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:integer");
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeInteger + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeInteger;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathIntegerCast(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing an XPath Date Time Cast Function
    /// </summary>
    public class XPathDateTimeCast
        : BaseXPathCast
    {
        /// <summary>
        /// Creates a new XPath Date Time Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public XPathDateTimeCast(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Casts the value of the inner Expression to a Date Time
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode n = this._expr.Value(context, bindingID);

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:dateTime");
            }

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:dateTime");

                case NodeType.Literal:
                    //See if the value can be cast
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        String dt = lit.DataType.ToString();
                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                        {
                            //Already a xsd:dateTime
                            return lit;
                        }
                        else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                        {
                            double d;
                            if (Double.TryParse(lit.Value, out d))
                            {
                                //Parsed OK
                                return new LiteralNode(lit.Graph, d.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double");
                            }
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast a Literal typed <" + dt + "> to a xsd:dateTime");
                        }
                    }
                    else
                    {
                        DateTime dt;
                        if (DateTime.TryParse(lit.Value, out dt))
                        {
                            //Parsed OK
                            return new LiteralNode(lit.Graph, dt.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:dateTime");
                        }
                    }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:string");
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeDateTime + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeDateTime;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathDateTimeCast(transformer.Transform(this._expr));
        }
    }

    /// <summary>
    /// Class representing an XPath String Cast Function
    /// </summary>
    public class XPathStringCast 
        : BaseXPathCast
    {
        /// <summary>
        /// Creates a new XPath String Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public XPathStringCast(ISparqlExpression expr) : base(expr) { }

        /// <summary>
        /// Casts the results of the inner expression to a Literal Node typed xsd:string
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override INode Value(SparqlEvaluationContext context, int bindingID)
        {
            INode n = this._expr.Value(context, bindingID);

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:string");
            }

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                    throw new RdfQueryException("Cannot cast a Blank/Graph Literal Node to a xsd:string");

                case NodeType.Literal:
                    //Just copy the lexical value into a new Literal Node
                    ILiteralNode lit = (ILiteralNode)n;
                    return new LiteralNode(lit.Graph, lit.Value, new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                case NodeType.Uri:
                    //Just copy the Uri into a new Literal Node
                    IUriNode uri = (IUriNode)n;
                    return new LiteralNode(uri.Graph, uri.Uri.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));

                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:string");
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XmlSpecsHelper.XmlSchemaDataTypeString + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeString;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathStringCast(transformer.Transform(this._expr));
        }
    }
}

