// unset

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Expressions.Functions.Arq;
using VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric;
using VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.DateTime;
using VDS.RDF.Query.Expressions.Functions.Sparql.Hash;
using VDS.RDF.Query.Expressions.Functions.Sparql.Numeric;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Functions.XPath;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;
using VDS.RDF.Query.Expressions.Functions.XPath.DateTime;
using VDS.RDF.Query.Expressions.Functions.XPath.Numeric;
using VDS.RDF.Query.Expressions.Functions.XPath.String;
using VDS.RDF.Query.Operators;
using AbsFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.AbsFunction;
using BNodeFunction = VDS.RDF.Query.Expressions.Functions.Arq.BNodeFunction;
using ConcatFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ConcatFunction;
using ContainsFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ContainsFunction;
using EFunction = VDS.RDF.Query.Expressions.Functions.Arq.EFunction;
using EncodeForUriFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.EncodeForUriFunction;
using FloorFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.FloorFunction;
using MD5HashFunction = VDS.RDF.Query.Expressions.Functions.Leviathan.Hash.MD5HashFunction;
using NowFunction = VDS.RDF.Query.Expressions.Functions.Arq.NowFunction;
using ReplaceFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.ReplaceFunction;
using RoundFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.RoundFunction;
using Sha256HashFunction = VDS.RDF.Query.Expressions.Functions.Leviathan.Hash.Sha256HashFunction;
using SubstringFunction = VDS.RDF.Query.Expressions.Functions.Arq.SubstringFunction;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Implements the core logic for processing SPARQL expressions.
    /// </summary>
    /// <typeparam name="TContext">The type of the context object used when processing an expression.</typeparam>
    /// <typeparam name="TBinding">The type of the binding object used when processing an expression.</typeparam>
    abstract internal class 
        BaseExpressionProcessor<TContext, TBinding> : ISparqlExpressionProcessor<IValuedNode, TContext, TBinding>
    {
        private DateTimeNode _now;
        private readonly IValuedNode _eNode = new DoubleNode(Math.E);
        private readonly IValuedNode _piNode = new DoubleNode(Math.PI);
        private SHA1 _sha1;
        private MD5 _md5;
        private SHA256 _sha256;
        private Random _rnd = new Random();

        public BaseExpressionProcessor(ISparqlNodeComparer nodeComparer, IUriFactory uriFactory, bool useStrictOperators)
        {
            NodeComparer = nodeComparer;
            UriFactory = uriFactory;
            UseStrictOperators = useStrictOperators;
        }

        /// <summary>
        /// Provides the value bound for a specific variable in one of the bindings of the evaluation context.
        /// </summary>
        /// <param name="context">The current evaluation context.</param>
        /// <param name="binding">The binding to retrieve the value from.</param>
        /// <param name="var">The variable whose value is to be retrieved.</param>
        /// <returns>The value bound for <paramref name="var"/> in <paramref name="binding"/> of <paramref name="context"/> or null if no such bound value is found.</returns>
        protected abstract INode GetBoundValue(TContext context, TBinding binding, string var);

        /// <summary>
        /// Provides a boolean flag that specifies whether strict operators should be used.
        /// </summary>
        protected bool UseStrictOperators { get; }

        protected ISparqlNodeComparer NodeComparer { get; }

        protected IUriFactory UriFactory { get; }

        private IValuedNode ApplyBinaryOperator(BaseBinaryExpression expr, SparqlOperatorType operatorType,
            TContext context, TBinding binding)
        {
            IValuedNode[] inputs = new[]
            {
                expr.LeftExpression.Accept(this, context, binding),
                expr.RightExpression.Accept(this, context, binding)
            };
            if (!SparqlOperators.TryGetOperator(operatorType, UseStrictOperators, out ISparqlOperator op, inputs))
            {
                throw new RdfQueryException($"Cannot apply operator {operatorType} to the given inputs.");
            }

            return op.Apply(inputs);
        }

        public IValuedNode ProcessAdditionExpression(AdditionExpression addition, TContext context, TBinding binding)
        {
            return ApplyBinaryOperator(addition, SparqlOperatorType.Add, context, binding);
        }

        public IValuedNode ProcessDivisionExpression(DivisionExpression division, TContext context, TBinding binding)
        {
            return ApplyBinaryOperator(division, SparqlOperatorType.Divide, context, binding);
        }

        public IValuedNode ProcessMinusExpression(MinusExpression minus, TContext context, TBinding binding)
        {
            IValuedNode a = minus.InnerExpression.Accept(this, context, binding);
            if (a == null) throw new RdfQueryException("Cannot apply unary minus to a null");

            switch (a.NumericType)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(-1 * a.AsInteger());

                case SparqlNumericType.Decimal:
                    var decvalue = a.AsDecimal();
                    if (decvalue == decimal.Zero)
                    {
                        return new DecimalNode(decimal.Zero);
                    }
                    else
                    {
                        return new DecimalNode(-1 * decvalue);
                    }
                case SparqlNumericType.Float:
                    var fltvalue = a.AsFloat();
                    if (float.IsNaN(fltvalue))
                    {
                        return new FloatNode(float.NaN);
                    }
                    else if (float.IsPositiveInfinity(fltvalue))
                    {
                        return new FloatNode(float.NegativeInfinity);
                    }
                    else if (float.IsNegativeInfinity(fltvalue))
                    {
                        return new FloatNode(float.PositiveInfinity);
                    }
                    else
                    {
                        return new FloatNode(-1.0f * fltvalue);
                    }
                case SparqlNumericType.Double:
                    var dblvalue = a.AsDouble();
                    if (double.IsNaN(dblvalue))
                    {
                        return new DoubleNode(double.NaN);
                    }
                    else if (double.IsPositiveInfinity(dblvalue))
                    {
                        return new DoubleNode(double.NegativeInfinity);
                    }
                    else if (double.IsNegativeInfinity(dblvalue))
                    {
                        return new DoubleNode(double.PositiveInfinity);
                    }
                    else
                    {
                        return new DoubleNode(-1.0 * dblvalue);
                    }
                default:
                    throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        public IValuedNode ProcessMultiplicationExpression(MultiplicationExpression multiplication, TContext context,
            TBinding binding)
        {
            return ApplyBinaryOperator(multiplication, SparqlOperatorType.Multiply, context, binding);
        }

        public IValuedNode ProcessSubtractionExpression(SubtractionExpression subtraction, TContext context, TBinding binding)
        {
            return ApplyBinaryOperator(subtraction, SparqlOperatorType.Subtract, context, binding);
        }

        public IValuedNode ProcessEqualsExpression(EqualsExpression @equals, TContext context, TBinding binding)
        {
            IValuedNode x = @equals.LeftExpression.Accept(this, context, binding);
            IValuedNode y = @equals.RightExpression.Accept(this, context, binding);

            return new BooleanNode(SparqlSpecsHelper.Equality(x, y));
        }

        public IValuedNode ProcessGreaterThanExpression(GreaterThanExpression gt, TContext context, TBinding binding)
        {
            IValuedNode a, b;
            a = gt.LeftExpression.Accept(this, context, binding);
            b = gt.RightExpression.Accept(this, context, binding);

            if (a == null) throw new RdfQueryException("Cannot evaluate a > when one argument is Null");

            var compare = NodeComparer.Compare(a, b);//a.CompareTo(b);
            return new BooleanNode(compare > 0);
        }

        public IValuedNode ProcessGreaterThanOrEqualToExpression(GreaterThanOrEqualToExpression gte, TContext context,
            TBinding binding)
        {
            IValuedNode a, b;
            a = gte.LeftExpression.Accept(this, context, binding);
            b = gte.RightExpression.Accept(this, context, binding);
            if (a == null)
            {
                if (b == null)
                {
                    return new BooleanNode(true);
                }

                throw new RdfQueryException("Cannot evaluate a >= when one argument is null");
            }

            var compare = NodeComparer.Compare(a, b);// a.CompareTo(b);
            return new BooleanNode(compare >= 0);
        }

        public IValuedNode ProcessLessThanExpression(LessThanExpression lt, TContext context, TBinding binding)
        {
            IValuedNode a, b;
            a = lt.LeftExpression.Accept(this, context, binding);
            b = lt.RightExpression.Accept(this, context, binding);

            if (a == null) throw new RdfQueryException("Cannot evaluate a < when one argument is Null");
            var compare = NodeComparer.Compare(a, b);
            return new BooleanNode(compare < 0);

        }

        public IValuedNode ProcessLessThanOrEqualToExpression(LessThanOrEqualToExpression lte, TContext context, TBinding binding)
        {
            IValuedNode a = lte.LeftExpression.Accept(this, context, binding);
            IValuedNode b = lte.RightExpression.Accept(this, context, binding);
            if (a == null)
            {
                if (b == null)
                {
                    return new BooleanNode(true);
                }

                throw new RdfQueryException("Cannot evaluate a <= when one argument is a Null");
            }

            var compare = NodeComparer.Compare(a, b);
            return new BooleanNode(compare <= 0);
        }

        public IValuedNode ProcessNotEqualsExpression(NotEqualsExpression ne, TContext context, TBinding binding)
        {
            IValuedNode a = ne.LeftExpression.Accept(this, context, binding);
            IValuedNode b = ne.RightExpression.Accept(this, context, binding);
            return new BooleanNode(SparqlSpecsHelper.Inequality(a, b));
        }

        public IValuedNode ProcessAndExpression(AndExpression and, TContext context, TBinding binding)
        {
            try
            {
                return new BooleanNode(and.LeftExpression.Accept(this, context, binding).AsBoolean() &&
                                       and.RightExpression.Accept(this, context, binding).AsBoolean());
            }
            catch (Exception ex)
            {
                // If we encounter an error on the LHS then we return false only if the RHS is false
                // Otherwise we error
                var rightResult = and.RightExpression.Accept(this, context, binding).AsSafeBoolean();
                if (!rightResult)
                {
                    return new BooleanNode(false);
                }

                if (ex is RdfQueryException)
                {
                    throw;
                }

                throw new RdfQueryException("Error evaluating AND expression", ex);
            }
        }

        public IValuedNode ProcessNotExpression(NotExpression not, TContext context, TBinding binding)
        {
            return new BooleanNode(!not.InnerExpression.Accept(this, context, binding).AsSafeBoolean());
        }

        public IValuedNode ProcessOrExpression(OrExpression or, TContext context, TBinding binding)
        {
            try
            {
                return new BooleanNode(or.LeftExpression.Accept(this, context, binding).AsBoolean() ||
                                       or.RightExpression.Accept(this, context, binding).AsBoolean());
            }
            catch (Exception ex)
            {
                // If there's an Error on the LHS we return true only if the RHS evaluates to true
                // Otherwise we throw the Error
                var rightResult = or.RightExpression.Accept(this, context, binding).AsSafeBoolean();
                if (rightResult)
                {
                    return new BooleanNode(true);
                }

                // Ensure the error we throw is a RdfQueryException so as not to cause issues higher up
                if (ex is RdfQueryException)
                {
                    throw;
                }

                throw new RdfQueryException("Error evaluating OR expression", ex);
            }
        }

        public IValuedNode ProcessArqBNodeFunction(BNodeFunction bNode, TContext context, TBinding binding)
        {
            INode temp = bNode.InnerExpression.Accept(this, context, binding);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Blank)
                {
                    var b = (IBlankNode)temp;
                    return new StringNode(null, b.InternalID);
                }
                else
                {
                    throw new RdfQueryException("Cannot find the BNode Label for a non-Blank Node");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot find the BNode Label for a null");
            }
        }

        public IValuedNode ProcessEFunction(EFunction e, TContext context, TBinding binding)
        {
            return _eNode;
        }

        public IValuedNode ProcessLocalNameFunction(LocalNameFunction localName, TContext context, TBinding binding)
        {
            INode temp = localName.InnerExpression.Accept(this, context, binding);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Uri)
                {
                    var u = (IUriNode)temp;
                    if (!u.Uri.Fragment.Equals(string.Empty))
                    {
                        return new StringNode(null, u.Uri.Fragment.Substring(1));
                    }
                    else
                    {
                        return new StringNode(null, u.Uri.Segments.Last());
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot find the Local Name for a non-URI Node");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot find the Local Name for a null");
            }
        }

        public IValuedNode ProcessMaxFunction(MaxFunction max, TContext context, TBinding binding)
        {
            IValuedNode a = max.LeftExpression.Accept(this, context, binding);
            IValuedNode b = max.RightExpression.Accept(this, context, binding);

            var type = (SparqlNumericType)Math.Max((int)a.NumericType, (int)b.NumericType);

            switch (type)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(Math.Max(a.AsInteger(), b.AsInteger()));
                case SparqlNumericType.Decimal:
                    return new DecimalNode(Math.Max(a.AsDecimal(), b.AsDecimal()));
                case SparqlNumericType.Float:
                    return new FloatNode(Math.Max(a.AsFloat(), b.AsFloat()));
                case SparqlNumericType.Double:
                    return new DoubleNode(Math.Max(a.AsDouble(), b.AsDouble()));
                default:
                    throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        public IValuedNode ProcessMinFunction(MinFunction min, TContext context, TBinding binding)
        {
            IValuedNode a = min.LeftExpression.Accept(this, context, binding);
            IValuedNode b = min.RightExpression.Accept(this, context, binding);

            var type = (SparqlNumericType)Math.Max((int)a.NumericType, (int)b.NumericType);
            switch (type)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(Math.Min(a.AsInteger(), b.AsInteger()));
                case SparqlNumericType.Decimal:
                    return new DecimalNode(Math.Min(a.AsDecimal(), b.AsDecimal()));
                case SparqlNumericType.Float:
                    return new FloatNode(Math.Min(a.AsFloat(), b.AsFloat()));
                case SparqlNumericType.Double:
                    return new DoubleNode(Math.Min(a.AsDouble(), b.AsDouble()));
                default:
                    throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        public IValuedNode ProcessNamespaceFunction(NamespaceFunction ns, TContext context, TBinding binding)
        {
            INode temp = ns.InnerExpression.Accept(this, context, binding);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Uri)
                {
                    var u = (IUriNode)temp;
                    if (!u.Uri.Fragment.Equals(string.Empty))
                    {
                        return new StringNode(null, u.Uri.AbsoluteUri.Substring(0, u.Uri.AbsoluteUri.LastIndexOf('#') + 1));
                    }
                    else
                    {
                        return new StringNode(null, u.Uri.AbsoluteUri.Substring(0, u.Uri.AbsoluteUri.LastIndexOf('/') + 1));
                    }
                }
                else
                {
                    throw new RdfQueryException("Cannot find the Namespace for a non-URI Node");
                }
            }
            else
            {
                throw new RdfQueryException("Cannot find the Namespace for a null");
            }
        }

        public IValuedNode ProcessNowFunction(NowFunction now, TContext context, TBinding binding)
        {
            // The original Leviathan function checked the context.Query property and returned
            // a new value whenever that property changed. This implementation assumes that
            // the BaseExpressionProcessor is not reused across multiple query evaluations.
            if (_now == null) _now = new DateTimeNode(DateTime.Now);
            return _now;
        }

        public IValuedNode ProcessPiFunction(PiFunction pi, TContext context, TBinding binding)
        {
            return _piNode;
        }

        /// <summary>
        /// Computes Hashes.
        /// </summary>
        /// <param name="input">Input String.</param>
        /// <param name="algorithm">Hash algorithm to use.</param>
        /// <returns></returns>
        protected virtual string Hash(string input, HashAlgorithm algorithm)
        {
            var output = new StringBuilder();

            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = algorithm.ComputeHash(inputBytes);

            foreach (var t in hashBytes)
            {
                output.Append(t.ToString("x2"));
            }

            return output.ToString();
        }

        protected virtual IValuedNode ProcessHashFunction(IValuedNode valueToHash, HashAlgorithm algorithm)
        {
            if (valueToHash == null)
            {
                throw new RdfQueryException("Cannot calculate the Hash of a null");
            }

            switch (valueToHash.NodeType)
            {
                case NodeType.Blank:
                    throw new RdfQueryException("Cannot calculate the Hash of a Blank Node");
                case NodeType.GraphLiteral:
                    throw new RdfQueryException("Cannot calculate the Hash of a Graph Literal");
                case NodeType.Literal:
                    return new StringNode(Hash(((ILiteralNode)valueToHash).Value, algorithm));
                case NodeType.Uri:
                    return new StringNode(Hash(valueToHash.AsString(), algorithm));
                default:
                    throw new RdfQueryException("Cannot calculate the Hash of an Unknown Node Type");
            }
        }

        public IValuedNode ProcessSha1Function(Sha1Function sha1, TContext context, TBinding binding)
        {
            if (_sha1 == null) _sha1 = SHA1.Create();
            return ProcessHashFunction(sha1.InnerExpression.Accept(this, context, binding), _sha1);
        }

        public IValuedNode ProcessStringJoinFunction(StringJoinFunction stringJoin, TContext context, TBinding binding)
        {
            var output = new StringBuilder();
            for (var i = 0; i < stringJoin.Expressions.Count; i++)
            {
                IValuedNode temp = stringJoin.Expressions[i].Accept(this, context, binding);
                if (temp == null) throw new RdfQueryException("Cannot evaluate the ARQ string-join() function when an argument evaluates to a Null");
                switch (temp.NodeType)
                {
                    case NodeType.Literal:
                        output.Append(temp.AsString());
                        break;
                    default:
                        throw new RdfQueryException("Cannot evaluate the ARQ string-join() function when an argument is not a Literal Node");
                }
                if (i < stringJoin.Expressions.Count - 1)
                {
                    if (stringJoin.FixedSeparator)
                    {
                        output.Append(stringJoin.Separator);
                    }
                    else
                    {
                        IValuedNode sep = stringJoin.SeparatorExpression.Accept(this, context, binding);
                        if (sep == null) throw new RdfQueryException("Cannot evaluate the ARQ strjoin() function when the separator expression evaluates to a Null");
                        if (sep.NodeType == NodeType.Literal)
                        {
                            output.Append(sep.AsString());
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot evaluate the ARQ strjoin() function when the separator expression evaluates to a non-Literal Node");
                        }
                    }
                }
            }

            return new StringNode(output.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        public IValuedNode ProcessSubstringFunction(SubstringFunction substring, TContext context, TBinding binding)
        {
            var input = (ILiteralNode)CheckArgument(substring.StringExpression, context, binding);
            IValuedNode start = CheckArgument(substring.StartExpression, context, binding, XPathFunctionFactory.AcceptNumericArguments);

            if (substring.EndExpression != null)
            {
                IValuedNode end = CheckArgument(substring.EndExpression, context, binding, XPathFunctionFactory.AcceptNumericArguments);

                if (input.Value.Equals(string.Empty)) return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    var s = Convert.ToInt32(start.AsInteger());
                    var e = Convert.ToInt32(end.AsInteger());

                    if (s < 0) s = 0;
                    if (e < s)
                    {
                        // If no/negative characters are being selected the empty string is returned
                        return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else if (s > input.Value.Length)
                    {
                        // If the start is after the end of the string the empty string is returned
                        return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                    }
                    else
                    {
                        if (e > input.Value.Length)
                        {
                            // If the end is greater than the length of the string the string from the starts onwards is returned
                            return new StringNode(input.Value.Substring(s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                        }
                        else
                        {
                            // Otherwise do normal substring
                            return new StringNode(input.Value.Substring(s, e - s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                        }
                    }
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start/End argument to an Integer");
                }
            }
            else
            {
                if (input.Value.Equals(string.Empty)) return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

                try
                {
                    var s = Convert.ToInt32(start.AsInteger());
                    if (s < 0) s = 0;

                    return new StringNode(input.Value.Substring(s), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                catch
                {
                    throw new RdfQueryException("Unable to convert the Start argument to an Integer");
                }
            }
        }

        public IValuedNode ProcessLeviathanMD5HashFunction(MD5HashFunction md5, TContext context, TBinding binding)
        {
            if (_md5 == null) _md5 = MD5.Create();
            return ProcessHashFunction(md5.InnerExpression.Accept(this, context, binding), _md5);
        }

        public IValuedNode ProcessLeviathanSha256HashFunction(Sha256HashFunction sha256, TContext context, TBinding binding)
        {
            if (_sha256 == null) _sha256 = SHA256.Create();
            return ProcessHashFunction(sha256.InnerExpression.Accept(this, context, binding), _sha256);
        }

        public IValuedNode ProcessCosecantFunction(CosecantFunction cosec, TContext context, TBinding binding)
        {
            return ProcessTrigFunction(cosec, d => cosec.Inverse ? Math.Asin(1 / d) : 1 / Math.Sin(d), context,
                binding);
        }

        public IValuedNode ProcessCosineFunction(CosineFunction cos, TContext context, TBinding binding)
        {
            return ProcessTrigFunction(cos, d => cos.Inverse ? Math.Acos(d) : Math.Cos(d), context, binding);
        }

        public IValuedNode ProcessCotangentFunction(CotangentFunction cot, TContext context, TBinding binding)
        {
            return ProcessTrigFunction(cot, d => cot.Inverse ? Math.Atan(1 / d) : Math.Cos(d) / Math.Sin(d), context,
                binding);
        }

        public IValuedNode ProcessDegreesToRadiansFunction(DegreesToRadiansFunction degToRad, TContext context, TBinding binding)
        {
            IValuedNode temp = degToRad.InnerExpression.Accept(this, context, binding);

            if (temp == null) throw new RdfQueryException("Cannot apply a numeric function to a null");
            if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a numeric function to a non-numeric argument");

            return new DoubleNode(Math.PI * (temp.AsDouble() / 180d));
        }

        public IValuedNode ProcessRadiansToDegreesFunction(RadiansToDegreesFunction radToDeg, TContext context, TBinding binding)
        {
            IValuedNode temp = radToDeg.InnerExpression.Accept(this, context, binding);

            if (temp == null) throw new RdfQueryException("Cannot apply a numeric function to a null");
            if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a numeric function to a non-numeric argument");

            return new DoubleNode(temp.AsDouble() * (180d / Math.PI));
        }

        public IValuedNode ProcessSecantFunction(SecantFunction sec, TContext context, TBinding binding)
        {
            return ProcessTrigFunction(sec, d => sec.Inverse ? Math.Acos(1 / d) : 1 / Math.Cos(d), context, binding);
        }

        public IValuedNode ProcessSineFunction(SineFunction sin, TContext context, TBinding binding)
        {
            return ProcessTrigFunction(sin, d => sin.Inverse ? Math.Asin(d) : Math.Sin(d), context, binding);
        }

        public IValuedNode ProcessTangentFunction(TangentFunction tan, TContext context, TBinding binding)
        {
            return ProcessTrigFunction(tan, d => tan.Inverse ? Math.Atan(d) : Math.Tan(d), context, binding);
        }

        public IValuedNode ProcessCartesianFunction(CartesianFunction cart, TContext context, TBinding binding)
        {
            IValuedNode x1 = cart.X1.Accept(this, context, binding);
            if (x1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y1 = cart.Y1.Accept(this, context, binding);
            if (y1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode x2 = cart.X2.Accept(this, context, binding);
            if (x2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
            IValuedNode y2 = cart.Y2.Accept(this, context, binding);
            if (y2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");

            if (cart.Is3D)
            {
                IValuedNode z1 = cart.Z1.Accept(this, context, binding);
                if (z1 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");
                IValuedNode z2 = cart.Z2.Accept(this, context, binding);
                if (z2 == null) throw new RdfQueryException("Cannot calculate cartesian distance when a argument is null");

                var dX = x2.AsDouble() - x1.AsDouble();
                var dY = y2.AsDouble() - y1.AsDouble();
                var dZ = z2.AsDouble() - z1.AsDouble();

                return new DoubleNode(Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2) + Math.Pow(dZ, 2)));
            }
            else
            {
                var dX = x2.AsDouble() - x1.AsDouble();
                var dY = y2.AsDouble() - y1.AsDouble();

                return new DoubleNode(Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2)));
            }
        }

        public IValuedNode ProcessCubeFunction(CubeFunction cube, TContext context, TBinding binding)
        {
            IValuedNode temp = cube.InnerExpression.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot square a null");

            switch (temp.NumericType)
            {
                case SparqlNumericType.Integer:
                    var l = temp.AsInteger();
                    return new LongNode(l * l * l);
                case SparqlNumericType.Decimal:
                    var d = temp.AsDecimal();
                    return new DecimalNode(d * d * d);
                case SparqlNumericType.Float:
                    var f = temp.AsFloat();
                    return new FloatNode(f * f * f);
                case SparqlNumericType.Double:
                    var dbl = temp.AsDouble();
                    return new DoubleNode(Math.Pow(dbl, 3));
                case SparqlNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        public IValuedNode ProcessLeviathanEFunction(Expressions.Functions.Leviathan.Numeric.EFunction eFunction, TContext context, TBinding binding)
        {
            IValuedNode temp = eFunction.InnerExpression.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot square root a null");

            switch (temp.NumericType)
            {
                case SparqlNumericType.Integer:
                case SparqlNumericType.Decimal:
                case SparqlNumericType.Float:
                case SparqlNumericType.Double:
                    return new DoubleNode(Math.Pow(Math.E, temp.AsDouble()));
                case SparqlNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        public IValuedNode ProcessFactorialFunction(FactorialFunction factorial, TContext context, TBinding binding)
        {
            IValuedNode temp = factorial.InnerExpression.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot evaluate factorial of a null");
            var l = temp.AsInteger();

            if (l == 0) return new LongNode(0);
            long fac = 1;
            if (l > 0)
            {
                for (var i = l; i > 1; i--)
                {
                    fac *= i;
                }
            }
            else
            {
                for (var i = l; i < -1; i++)
                {
                    fac *= i;
                }
            }
            return new LongNode(fac);
        }

        public IValuedNode ProcessLogFunction(LogFunction log, TContext context, TBinding binding)
        {
            IValuedNode arg = log.LeftExpression.Accept(this, context, binding);
            if (arg == null) throw new RdfQueryException("Cannot log a null");

            IValuedNode logBase = log.RightExpression.Accept(this, context, binding);
            if (logBase == null) throw new RdfQueryException("Cannot log to a null base");

            if (arg.NumericType == SparqlNumericType.NaN || logBase.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot log when one/both arguments are non-numeric");

            return new DoubleNode(Math.Log(arg.AsDouble(), logBase.AsDouble()));
        }

        public IValuedNode ProcessNaturalLogFunction(LeviathanNaturalLogFunction logn, TContext context, TBinding binding)
        {
            IValuedNode temp = logn.InnerExpression.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot square root a null");

            switch (temp.NumericType)
            {
                case SparqlNumericType.Integer:
                case SparqlNumericType.Decimal:
                case SparqlNumericType.Float:
                case SparqlNumericType.Double:
                    return new DoubleNode(Math.Log(temp.AsDouble(), Math.E));
                case SparqlNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        public IValuedNode ProcessPowerFunction(PowerFunction powFn, TContext context, TBinding binding)
        {
            IValuedNode arg = powFn.LeftExpression.Accept(this, context, binding);
            if (arg == null) throw new RdfQueryException("Cannot raise a null to a power");
            IValuedNode pow = powFn.RightExpression.Accept(this, context, binding);
            if (pow == null) throw new RdfQueryException("Cannot raise to a null power");

            if (arg.NumericType == SparqlNumericType.NaN || pow.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot raise to a power when one/both arguments are non-numeric");

            return new DoubleNode(Math.Pow(arg.AsDouble(), pow.AsDouble()));
        }

        public IValuedNode ProcessPythagoreanDistanceFunction(PythagoreanDistanceFunction pythagoreanDistance, TContext context,
            TBinding binding)
        {
            IValuedNode x = pythagoreanDistance.LeftExpression.Accept(this, context, binding);
            if (x == null) throw new RdfQueryException("Cannot calculate distance of a null");
            IValuedNode y = pythagoreanDistance.RightExpression.Accept(this, context, binding);
            if (y == null) throw new RdfQueryException("Cannot calculate distance of a null");

            if (x.NumericType == SparqlNumericType.NaN || y.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot calculate distance when one/both arguments are non-numeric");

            return new DoubleNode(Math.Sqrt(Math.Pow(x.AsDouble(), 2) + Math.Pow(y.AsDouble(), 2)));
        }

        public IValuedNode ProcessRandomFunction(RandomFunction rand, TContext context, TBinding binding)
        {
            IValuedNode min = rand.LeftExpression.Accept(this, context, binding);
            if (min == null) throw new RdfQueryException("Cannot randomize with a null minimum");
            IValuedNode max = rand.RightExpression.Accept(this, context, binding);
            if (max == null) throw new RdfQueryException("Cannot randomize with a null maximum");

            if (min.NumericType == SparqlNumericType.NaN || max.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot randomize when one/both arguments are non-numeric");

            var x = min.AsDouble();
            var y = max.AsDouble();

            if (x > y) throw new RdfQueryException("Cannot generate a random number in the given range since the minumum is greater than the maximum");
            var range = y - x;
            var rnd = _rnd.NextDouble() * range;
            rnd += x;
            return new DoubleNode(rnd);
        }

        public IValuedNode ProcessReciprocalFunction(ReciprocalFunction reciprocal, TContext context, TBinding binding)
        {
            IValuedNode temp = reciprocal.InnerExpression.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot evaluate reciprocal of a null");
            var d = temp.AsDouble();
            if (d == 0) throw new RdfQueryException("Cannot evaluate reciprocal of zero");

            return new DoubleNode(1d / d);
        }

        public IValuedNode ProcessRootFunction(RootFunction rootFn, TContext context, TBinding binding)
        {
            IValuedNode arg = rootFn.LeftExpression.Accept(this, context, binding);
            if (arg == null) throw new RdfQueryException("Cannot root a null");
            IValuedNode root = rootFn.RightExpression.Accept(this, context, binding);
            if (root == null) throw new RdfQueryException("Cannot root to a null root");

            if (arg.NumericType == SparqlNumericType.NaN || root.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot root when one/both arguments are non-numeric");

            return new DoubleNode(Math.Pow(arg.AsDouble(), (1d / root.AsDouble())));
        }

        public IValuedNode ProcessSquareFunction(SquareFunction square, TContext context, TBinding binding)
        {
            IValuedNode temp = square.InnerExpression.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot square a null");

            switch (temp.NumericType)
            {
                case SparqlNumericType.Integer:
                    var l = temp.AsInteger();
                    return new LongNode(l * l);
                case SparqlNumericType.Decimal:
                    var d = temp.AsDecimal();
                    return new DecimalNode(d * d);
                case SparqlNumericType.Float:
                    var f = temp.AsFloat();
                    return new FloatNode(f * f);
                case SparqlNumericType.Double:
                    var dbl = temp.AsDouble();
                    return new DoubleNode(Math.Pow(dbl, 2));
                case SparqlNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        public IValuedNode ProcessSquareRootFunction(SquareRootFunction sqrt, TContext context, TBinding binding)
        {
            IValuedNode temp = sqrt.InnerExpression.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot square root a null");

            switch (temp.NumericType)
            {
                case SparqlNumericType.Integer:
                case SparqlNumericType.Decimal:
                case SparqlNumericType.Float:
                case SparqlNumericType.Double:
                    return new DoubleNode(Math.Sqrt(temp.AsDouble()));
                case SparqlNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        public IValuedNode ProcessTenFunction(TenFunction ten, TContext context, TBinding binding)
        {
            IValuedNode temp = ten.InnerExpression.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot square root a null");

            switch (temp.NumericType)
            {
                case SparqlNumericType.Integer:
                case SparqlNumericType.Decimal:
                case SparqlNumericType.Float:
                case SparqlNumericType.Double:
                    return new DoubleNode(Math.Pow(10, temp.AsDouble()));
                case SparqlNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        public IValuedNode ProcessBoundFunction(BoundFunction bound, TContext context, TBinding binding)
        {
            return new BooleanNode(bound.InnerExpression.Accept(this, context, binding) != null);
        }

        public IValuedNode ProcessExistsFunction(ExistsFunction exists, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessIsBlankFunction(IsBlankFunction isBlank, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessIsIriFunction(IsIriFunction isIri, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessIsLiteralFunction(IsLiteralFunction isLiteral, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessIsNumericFunction(IsNumericFunction isNumeric, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessLangMatchesFunction(LangMatchesFunction langMatches, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessRegexFunction(RegexFunction regex, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessSameTermFunction(SameTermFunction sameTerm, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessBNodeFunction(Expressions.Functions.Sparql.Constructor.BNodeFunction bNode, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessIriFunction(IriFunction iri, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessStrDtFunction(StrDtFunction strDt, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessStrLangFunction(StrLangFunction strLang, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessDayFunction(DayFunction day, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessHoursFunction(HoursFunction hours, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessMinutesFunction(MinutesFunction minutes, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessMonthFunction(MonthFunction month, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessNowFunction(Expressions.Functions.Sparql.DateTime.NowFunction now, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessSecondsFunction(SecondsFunction seconds, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessTimezoneFunction(TimezoneFunction timezone, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessTZFunction(TZFunction tz, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessYearFunction(YearFunction year, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessMd5HashFunction(Expressions.Functions.Sparql.Hash.MD5HashFunction md5, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessSha1HashFunction(Sha1HashFunction sha1, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessSha256HashFunction(Expressions.Functions.Sparql.Hash.Sha256HashFunction sha256, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessSha384HashFunction(Sha384HashFunction sha384, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessSha512HashFunction(Sha512HashFunction sha512, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessAbsFunction(AbsFunction abs, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessCeilFunction(CeilFunction ceil, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessFloorFunction(FloorFunction floor, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessRandFunction(RandFunction rand, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessRoundFunction(RoundFunction round, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessInFunction(InFunction inFn, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessNotInFunction(NotInFunction notIn, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessConcatFunction(ConcatFunction concat, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessContainsFunction(ContainsFunction contains, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessDataTypeFunction(DataTypeFunction dataType, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessDataType11Function(DataType11Function dataType, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessEncodeForUriFunction(EncodeForUriFunction encodeForUri, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessLangFunction(LangFunction lang, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessLCaseFunction(LCaseFunction lCase, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessReplaceFunction(ReplaceFunction replace, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessStrAfterFunction(StrAfterFunction strAfter, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessStrBeforeFunction(StrBeforeFunction strBefore, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessStrEndsFunction(StrEndsFunction strEnds, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessStrFunction(StrFunction str, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessStrLenFunction(StrLangFunction strLen, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessStrStartsFunction(StrStartsFunction strStarts, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessSubStrFunction(SubStrFunction subStr, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessUCaseFunction(UCaseFunction uCase, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessUuidFunction(UUIDFunction uuid, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessCallFunction(CallFunction call, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessCoalesceFunction(CoalesceFunction coalesce, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessIfElseFunction(IfElseFunction ifElse, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessBooleanCase(BooleanCast cast, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessDateTimeCast(DateTimeCast cast, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessDecimalCast(DecimalCast cast, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessDoubleCast(DoubleCast cast, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessFloatCast(FloatCast cast, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessIntegerCast(IntegerCast cast, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessStringCast(StringCast cast, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessDayFromDateTimeFunction(DayFromDateTimeFunction day, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessHoursFromDateTimeFunction(HoursFromDateTimeFunction hours, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessMinutesFromDateTimeFunction(MinutesFromDateTimeFunction minutes, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessMonthFromDateTimeFunction(MonthFromDateTimeFunction month, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessSecondsFromDateTimeFunction(SecondsFromDateTimeFunction seconds, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessTimezoneFromDateTimeFunction(TimezoneFromDateTimeFunction timezone, TContext context,
            TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessYearsFromDateTimeFunction(YearFromDateTimeFunction years, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathAbsFunction(AbsFunction abs, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathCeilFunction(CeilingFunction ceil, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathFloorFunction(Expressions.Functions.XPath.Numeric.FloorFunction floor, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathRoundFunction(Expressions.Functions.XPath.Numeric.RoundFunction round, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathRoundHalfToEvenFunction(RoundHalfToEvenFunction round, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessCompareFunction(CompareFunction compare, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathConcatFunction(Expressions.Functions.XPath.String.ConcatFunction concat, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathContainsFunction(Expressions.Functions.XPath.String.ContainsFunction contains, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathEncodeForUriFunction(Expressions.Functions.XPath.String.EncodeForUriFunction encodeForUri, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathEndsWithFunction(EndsWithFunction endsWith, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathEscapeHtmlUriFunction(EscapeHtmlUriFunction escape, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathLowerCaseFunction(LowerCaseFunction lCase, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathNormalizeSpaceFunction(NormalizeSpaceFunction normalize, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathNormalizeUnicodeFunction(NormalizeUnicodeFunction normalize, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathReplaceFunction(Expressions.Functions.XPath.String.ReplaceFunction replace, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathStartsWithFunction(StartsWithFunction startsWith, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathStringLengthFunction(StringLengthFunction strLen, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathSubstringAfterFunction(SubstringAfterFunction substringAfter, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathSubstringBeforeFunction(SubstringBeforeFunction substringBefore, TContext context,
            TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathSubstringFunction(Expressions.Functions.XPath.String.SubstringFunction substring, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessXpathUpperCaseFunction(UpperCaseFunction uCase, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessBooleanFunction(BooleanFunction boolean, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        public IValuedNode ProcessUnknownFunction(UnknownFunction unknownFunction, TContext context, TBinding binding)
        {
            throw new NotImplementedException();
        }

        private IValuedNode CheckArgument(ISparqlExpression expr, TContext context, TBinding binding)
        {
            return CheckArgument(expr, context, binding, XPathFunctionFactory.AcceptStringArguments);
        }

        private IValuedNode CheckArgument(ISparqlExpression expr, TContext context, TBinding binding, Func<Uri, bool> argumentTypeValidator)
        {
            IValuedNode temp = expr.Accept(this, context, binding);
            if (temp != null)
            {
                if (temp.NodeType == NodeType.Literal)
                {
                    var lit = (ILiteralNode)temp;
                    if (lit.DataType != null)
                    {
                        if (argumentTypeValidator(lit.DataType))
                        {
                            // Appropriately typed literals are fine
                            return temp;
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions returned a typed literal with an invalid type");
                        }
                    }
                    else if (argumentTypeValidator(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)))
                    {
                        // Untyped Literals are treated as Strings and may be returned when the argument allows strings
                        return temp;
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions returned an untyped literal");
                    }
                }
                else
                {
                    throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions returned a non-literal");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an ARQ substring as one of the argument expressions evaluated to null");
            }
        }

        private IValuedNode ProcessTrigFunction(BaseTrigonometricFunction expr, Func<double, double> func, TContext context, TBinding binding)
        {
            IValuedNode temp = expr.InnerExpression.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot apply a trigonometric function to a null");

            if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a trigonometric function to a non-numeric argument");

            return new DoubleNode(func(temp.AsDouble()));
        }
    }
}