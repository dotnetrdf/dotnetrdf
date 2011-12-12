using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast
{
    /// <summary>
    /// Class representing an XPath Decimal Cast Function
    /// </summary>
    public class DecimalCast
        : BaseCast
    {
        /// <summary>
        /// Creates a new XPath Decimal Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public DecimalCast(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Casts the Value of the inner Expression to a Decimal
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode n = this._expr.Evaluate(context, bindingID);//.CoerceToDecimal();

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:decimal");
            }

            //New method should be much faster
            //if (n is DecimalNode) return n;
            //return new DecimalNode(null, n.AsDecimal());

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:decimal");

                case NodeType.Literal:
                    if (n is DecimalNode) return n;
                    //See if the value can be cast
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        string dt = lit.DataType.ToString();
                        if (SparqlSpecsHelper.IntegerDataTypes.Contains(dt))
                        {
                            //Already an integer type so valid as a xsd:decimal
                            decimal d;
                            if (Decimal.TryParse(lit.Value, out d))
                            {
                                //Parsed OK
                                return new DecimalNode(lit.Graph, d);
                            }
                            else
                            {
                                throw new RdfQueryException("Invalid lexical form for xsd:decimal");
                            }
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
                                return new DecimalNode(lit.Graph, d);
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
                            return new DecimalNode(lit.Graph, d);
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
            return new DecimalCast(transformer.Transform(this._expr));
        }
    }
}
