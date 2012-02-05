using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Nodes;

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
        public BooleanCast(ISparqlExpression expr) 
            : base(expr) { }

        /// <summary>
        /// Casts the value of the inner Expression to a Boolean
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode n = this._expr.Evaluate(context, bindingID);//.CoerceToBoolean();

            //if (n == null)
            //{
            //    throw new RdfQueryException("Cannot cast a Null to a xsd:boolean");
            //}

            ////New method should be much faster
            //if (n is BooleanNode) return n;
            //return new BooleanNode(null, n.AsBoolean());

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:boolean");

                case NodeType.Literal:
                    //See if the value can be cast
                    if (n is BooleanNode) return n;
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        string dt = lit.DataType.ToString();

                        if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                        {
                            //Already a Boolean
                            bool b;
                            if (Boolean.TryParse(lit.Value, out b))
                            {
                                return new BooleanNode(lit.Graph, b);
                            }
                            else
                            {
                                throw new RdfQueryException("Invalid Lexical Form for xsd:boolean");
                            }
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
                                        return new BooleanNode(lit.Graph, false);
                                    }
                                    else
                                    {
                                        return new BooleanNode(lit.Graph, true);
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
                                        return new BooleanNode(lit.Graph, false);
                                    }
                                    else
                                    {
                                        return new BooleanNode(lit.Graph, true);
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
                                        return new BooleanNode(lit.Graph, false);
                                    }
                                    else
                                    {
                                        return new BooleanNode(lit.Graph, true);
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
                                        return new BooleanNode(lit.Graph, b);
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
                            return new BooleanNode(lit.Graph, b);
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
            return new BooleanCast(transformer.Transform(this._expr));
        }
    }
}
