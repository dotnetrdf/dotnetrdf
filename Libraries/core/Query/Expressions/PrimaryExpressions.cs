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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// Class representing Variable value expressions
    /// </summary>
    public class VariableExpressionTerm 
        : ISparqlNumericExpression
    {
        private String _name;

        /// <summary>
        /// Creates a new Variable Expression
        /// </summary>
        /// <param name="name">Variable Name</param>
        public VariableExpressionTerm(String name)
        {
            if (name.StartsWith("?") || name.StartsWith("$"))
            {
                this._name = name.Substring(1);
            }
            else
            {
                this._name = name;
            }
        }

        /// <summary>
        /// Gets the Value of the Variable for the given Binding (if any)
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return context.Binder.Value(this._name, bindingID);
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode value = this.Value(context, bindingID);
            return SparqlSpecsHelper.EffectiveBooleanValue(value);
        }

        /// <summary>
        /// Computes the Numeric Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public Object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            INode value = this.Value(context, bindingID);
            if (value != null)
            {
                if (value.NodeType == NodeType.Literal)
                {
                    ILiteralNode lit = (ILiteralNode)value;
                    if (!lit.Language.Equals(String.Empty))
                    {
                        //If there's a Language Tag implied type is string so no numeric value
                        throw new RdfQueryException("Cannot calculate the Numeric Value of literal with a language specifier");
                    }
                    else if (lit.DataType == null)
                    {
                        throw new RdfQueryException("Cannot calculate the Numeric Value of an untyped Literal");
                    }
                    else
                    {
                        try
                        {
                            switch (SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(lit.DataType))
                            {
                                case SparqlNumericType.Decimal:
                                    return Decimal.Parse(lit.Value);
                                case SparqlNumericType.Double:
                                    return Double.Parse(lit.Value);
                                case SparqlNumericType.Float:
                                    return Single.Parse(lit.Value);
                                case SparqlNumericType.Integer:
                                    return Int64.Parse(lit.Value);
                                case SparqlNumericType.NaN:
                                default:
                                    throw new RdfQueryException("Cannot calculate the Numeric Value of a literal since its Data Type URI does not correspond to a Data Type URI recognised as a Numeric Type in the SPARQL Specification");
                            }
                        }
                        catch (FormatException fEx)
                        {
                            throw new RdfQueryException("Cannot calculate the Numeric Value of a Literal since the Value contained is not a valid value in it's given Data Type", fEx);
                        }
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot calculate the Numeric Value of a non-literal RDF Term");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot calculate the Numeric Value of an unbound Variable");
            }
        }

        /// <summary>
        /// Computes the Numeric Type of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public SparqlNumericType NumericType(SparqlEvaluationContext context, int bindingID)
        {
            Object value;
            try
            {
                value = this.NumericValue(context, bindingID);
            }
            catch (RdfQueryException)
            {
                return SparqlNumericType.NaN;
            }
            if (value is Double)
            {
                return SparqlNumericType.Double;
            }
            else if (value is Single)
            {
                return SparqlNumericType.Float;
            }
            else if (value is Decimal)
            {
                return SparqlNumericType.Decimal;
            }
            else if (value is Int32 || value is Int64)
            {
                return SparqlNumericType.Integer;
            }
            else
            {
                throw new RdfQueryException("Bound Value of this Variable is not Numeric");
            }
        }

        /// <summary>
        /// Computes the Integer Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public long IntegerValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToInt64(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Computes the Decimal Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public decimal DecimalValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToDecimal(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Computes the Float Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public float FloatValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToSingle(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Computes the Double Value of this Expression as evaluated for a given Binding (if it has a numeric value)
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public double DoubleValue(SparqlEvaluationContext context, int bindingID)
        {
            return Convert.ToDouble(this.NumericValue(context, bindingID));
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "?" + this._name;
        }

        /// <summary>
        /// Gets the enumeration containing the single variable that this expression term represents
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._name.AsEnumerable();
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Primary;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }

    /// <summary>
    /// Class for representing Node Terms
    /// </summary>
    public class NodeExpressionTerm
        : ISparqlExpression
    {
        /// <summary>
        /// Node this Term represents
        /// </summary>
        protected INode _node;
        /// <summary>
        /// Effective Boolean Value of this Term
        /// </summary>
        protected bool _ebv = false;

        /// <summary>
        /// Creates a new Node Expression
        /// </summary>
        /// <param name="n">Node</param>
        public NodeExpressionTerm(INode n)
        {
            this._node = n;

            //Compute the EBV
            try
            {
                this._ebv = SparqlSpecsHelper.EffectiveBooleanValue(this._node);
            }
            catch
            {
                this._ebv = false;
            }
        }

        /// <summary>
        /// Gets the Node that this Expression represents
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return this._node;
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return this._ebv;
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.Formatter.Format(this._node);
        }

        /// <summary>
        /// Gets an Empty Enumerable since a Node Term does not use variables
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public virtual SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Primary;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public virtual String Functor
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }

    /// <summary>
    /// Class for representing Boolean Terms
    /// </summary>
    public class BooleanExpressionTerm
        : ISparqlExpression
    {
        private bool _value;
        private INode _node;

        /// <summary>
        /// Creates a new Boolean Expression
        /// </summary>
        /// <param name="value">Boolean value</param>
        public BooleanExpressionTerm(bool value)
        {
            this._value = value;
            this._node = new LiteralNode(null, this._value.ToString().ToLower(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Gets the Boolean Value this Expression represents as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return this._node;
        }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return this._value;
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._value.ToString().ToLower();
        }

        /// <summary>
        /// Gets an Empty enumerable since a Boolean expression term doesn't use variables
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Primary;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public String Functor
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }

    /// <summary>
    /// Class for representing Fixed Numeric Terms
    /// </summary>
    public class NumericExpressionTerm
        : ISparqlNumericExpression
    {
        private long _intvalue = 0;
        private decimal _decvalue = Decimal.Zero;
        private float _fltvalue = Single.NaN;
        private double _dblvalue = Double.NaN;
        private SparqlNumericType _type;
        private INode _value = null;

        /// <summary>
        /// Creates a new Numeric Expression
        /// </summary>
        /// <param name="value">Integer Value</param>
        public NumericExpressionTerm(long value)
        {
            this._type = SparqlNumericType.Integer;
            this._intvalue = value;
            this._decvalue = value;
            this._fltvalue = (float)value;
            this._dblvalue = (double)value;
            this._value = new LiteralNode(null, this._intvalue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
        }

        /// <summary>
        /// Creates a new Numeric Expression
        /// </summary>
        /// <param name="value">Decimal Value</param>
        public NumericExpressionTerm(decimal value)
        {
            this._type = SparqlNumericType.Decimal;
            this._decvalue = value;
            this._fltvalue = (float)value;
            this._dblvalue = (double)value;
            this._value = new LiteralNode(null, this._decvalue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal));
        }

        /// <summary>
        /// Creates a new Numeric Expression
        /// </summary>
        /// <param name="value">Float Value</param>
        public NumericExpressionTerm(float value)
        {
            this._type = SparqlNumericType.Float;
            this._fltvalue = (float)value;
            this._dblvalue = (double)value;
            this._value = new LiteralNode(null, this._dblvalue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat));
        }

        /// <summary>
        /// Creates a new Numeric Expression
        /// </summary>
        /// <param name="value">Double Value</param>
        public NumericExpressionTerm(double value)
        {
            this._type = SparqlNumericType.Double;
            this._dblvalue = value;
            this._value = new LiteralNode(null, this._dblvalue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
        }

        /// <summary>
        /// Gets the Numeric Value this Expression represents as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return this._value;
        }

        /// <summary>
        /// Gets the Numeric Value this Expression represents
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public Object NumericValue(SparqlEvaluationContext context, int bindingID)
        {
            switch (this._type)
            {
                case SparqlNumericType.Double:
                    return this._dblvalue;

                case SparqlNumericType.Float:
                    return this._fltvalue;

                case SparqlNumericType.Decimal:
                    return this._decvalue;

                case SparqlNumericType.Integer:
                    return this._intvalue;

                default:
                    throw new RdfQueryException("Unable to return the Numeric Value for a Numeric Literal since its Numeric Type cannot be determined");
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
            switch (this._type)
            {
                case SparqlNumericType.Double:
                case SparqlNumericType.Float:
                    return !(this._dblvalue == 0.0d);

                case SparqlNumericType.Decimal:
                    return !(this._decvalue == 0);

                case SparqlNumericType.Integer:
                    return !(this._intvalue == 0);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the Numeric Type of the Numeric Value this Expression represents
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public SparqlNumericType NumericType(SparqlEvaluationContext context, int bindingID)
        {
            return this._type;
        }

        /// <summary>
        /// Gets the Integer Value this Expression represents
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public long IntegerValue(SparqlEvaluationContext context, int bindingID)
        {
            return this._intvalue;
        }

        /// <summary>
        /// Gets the Decimal Value this Expression represents
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public decimal DecimalValue(SparqlEvaluationContext context, int bindingID)
        {
            return this._decvalue;
        }

        /// <summary>
        /// Gets the Float Value this Expression represents
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public float FloatValue(SparqlEvaluationContext context, int bindingID)
        {
            return this._fltvalue;
        }

        /// <summary>
        /// Gets the Double Value this Expression represents
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public double DoubleValue(SparqlEvaluationContext context, int bindingID)
        {
            return this._dblvalue;
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (this._type)
            {
                case SparqlNumericType.Decimal:
                    return "\"" + this._decvalue.ToString() + "\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeDecimal + ">";
                case SparqlNumericType.Double:
                    return "\"" + this._dblvalue.ToString() + "\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeDouble + ">";
                case SparqlNumericType.Float:
                    return "\"" + this._dblvalue.ToString() + "\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeFloat + ">";
                case SparqlNumericType.Integer:
                    return "\"" + this._intvalue.ToString() + "\"^^<" + XmlSpecsHelper.XmlSchemaDataTypeInteger + ">";
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Gets an Empty enumerable since a Numeric expression term doesn't use variables
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Primary;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public virtual String Functor
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public virtual ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }

    /// <summary>
    /// Class for representing Graph Pattern Terms (as used in EXISTS/NOT EXISTS)
    /// </summary>
    public class GraphPatternExpressionTerm
        : ISparqlExpression
    {
        private GraphPattern _pattern;

        /// <summary>
        /// Creates a new Graph Pattern Expression
        /// </summary>
        /// <param name="pattern">Graph Pattern</param>
        public GraphPatternExpressionTerm(GraphPattern pattern)
        {
            this._pattern = pattern;
        }

        /// <summary>
        /// Gets the value of this Term as evaluated for the given Bindings in the given Context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bindingID"></param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("Graph Pattern Terms do not have a Node Value");
        }

        /// <summary>
        /// Gets the Effective Boolean Value of this Term as evaluated for the given Binding in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("Graph Pattern Terms do not have an Effective Boolean Value");
        }

        /// <summary>
        /// Gets the Graph Pattern this term represents
        /// </summary>
        public GraphPattern Pattern
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Expression
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return this._pattern.Variables; 
            }
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Primary;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public string Functor
        {
            get
            {
                return String.Empty; 
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>(); 
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }
}
