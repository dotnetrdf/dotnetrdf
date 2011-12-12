using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast
{
    /// <summary>
    /// Class representing an XPath Integer Cast Function
    /// </summary>
    public class IntegerCast
        : BaseCast
    {
        /// <summary>
        /// Creates a new XPath Integer Cast Function Expression
        /// </summary>
        /// <param name="expr">Expression to be cast</param>
        public IntegerCast(ISparqlExpression expr) 
            : base(expr) { }

        /// <summary>
        /// Casts the value of the inner Expression to an Integer
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode n = this._expr.Evaluate(context, bindingID);//.CoerceToInteger();

            if (n == null)
            {
                throw new RdfQueryException("Cannot cast a Null to a xsd:integer");
            }

            ////New method should be much faster
            //if (n is LongNode) return n;
            //return new LongNode(null, n.AsInteger());

            switch (n.NodeType)
            {
                case NodeType.Blank:
                case NodeType.GraphLiteral:
                case NodeType.Uri:
                    throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:integer");

                case NodeType.Literal:
                    //See if the value can be cast
                    if (n is LongNode) return n;
                    ILiteralNode lit = (ILiteralNode)n;
                    if (lit.DataType != null)
                    {
                        string dt = lit.DataType.ToString();
                        if (SparqlSpecsHelper.IntegerDataTypes.Contains(dt))
                        {
                            //Already a integer type so valid as a xsd:integer
                            long i;
                            if (Int64.TryParse(lit.Value, out i))
                            {
                                return new LongNode(lit.Graph, i);
                            }
                            else
                            {
                                throw new RdfQueryException("Invalid lexical form for xsd:integer");
                            }
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
                                return new LongNode(lit.Graph, i);
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
                            return new LongNode(lit.Graph, i);
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
            return new IntegerCast(transformer.Transform(this._expr));
        }
    }
}
