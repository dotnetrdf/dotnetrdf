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

using System;
using System.Globalization;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast
{
    /// <summary>
    /// Class representing an XPath Boolean Cast Function
    /// </summary>
    public class BooleanCast
        : BaseCast
    {
        /// <summary>
        /// Creates a new XPath Boolean Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public BooleanCast(IExpression expr)
            : base(expr) {}

        public override IExpression Copy(IExpression argument)
        {
            return new BooleanCast(argument);
        }

        /// <summary>
        /// Casts the value of the inner Expression to a Boolean
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            IValuedNode n = this.Argument.Evaluate(solution, context);

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:boolean");

                case NodeType.Literal:
                    //See if the value can be cast
                    if (n is BooleanNode) return n;
                    INode lit = n;
                    if (lit.DataType != null)
                    {
                        string dt = lit.DataType.ToString();

                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                        {
                            //Already a Boolean
                            bool b;
                            if (Boolean.TryParse(lit.Value, out b))
                            {
                                return new BooleanNode(b);
                            }
                            throw new RdfQueryException("Invalid Lexical Form for xsd:boolean");
                        }

                        //Cast based on Numeric Type
                        EffectiveNumericType type = SparqlSpecsHelper.GetNumericTypeFromDataTypeUri(dt);

                        switch (type)
                        {
                            case EffectiveNumericType.Decimal:
                                Decimal dec;
                                if (Decimal.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
                                {
                                    return dec.Equals(Decimal.Zero) ? new BooleanNode(false) : new BooleanNode(true);
                                }
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:decimal as an intermediate stage in casting to a xsd:boolean");

                            case EffectiveNumericType.Double:
                                Double dbl;
                                if (Double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
                                {
                                    if (Double.IsNaN(dbl) || dbl == 0.0d)
                                    {
                                        return new BooleanNode(false);
                                    }
                                    return new BooleanNode(true);
                                }
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double as an intermediate stage in casting to a xsd:boolean");

                            case EffectiveNumericType.Integer:
                                Int64 i;
                                if (Int64.TryParse(lit.Value, out i))
                                {
                                    return i == 0 ? new BooleanNode(false) : new BooleanNode(true);
                                }
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:integer as an intermediate stage in casting to a xsd:boolean");

                            case EffectiveNumericType.NaN:
                                if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                                {
                                    //DateTime cast forbidden
                                    throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:boolean");
                                }
                                Boolean b;
                                if (Boolean.TryParse(lit.Value, out b))
                                {
                                    return new BooleanNode(b);
                                }
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:boolean");

                            default:
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:boolean");
                        }
                    }
                {
                    Boolean b;
                    if (Boolean.TryParse(lit.Value, out b))
                    {
                        return new BooleanNode(b);
                    }
                    throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:boolean");
                }
                default:
                    throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:decimal");
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return XmlSpecsHelper.XmlSchemaDataTypeBoolean; }
        }
    }
}