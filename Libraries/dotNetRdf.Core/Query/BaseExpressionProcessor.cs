/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
using VDS.RDF.Query.Expressions.Functions.Sparql.TripleNode;
using VDS.RDF.Query.Expressions.Functions.XPath;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;
using VDS.RDF.Query.Expressions.Functions.XPath.DateTime;
using VDS.RDF.Query.Expressions.Functions.XPath.Numeric;
using VDS.RDF.Query.Expressions.Functions.XPath.String;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Operators;
using AbsFunction = VDS.RDF.Query.Expressions.Functions.Sparql.Numeric.AbsFunction;
using BaseBinaryStringFunction = VDS.RDF.Query.Expressions.Functions.Sparql.String.BaseBinaryStringFunction;
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
using XPath = VDS.RDF.Query.Expressions.Functions.XPath;

namespace VDS.RDF.Query;

/// <summary>
/// Implements the core logic for processing SPARQL expressions.
/// </summary>
/// <typeparam name="TContext">The type of the context object used when processing an expression.</typeparam>
/// <typeparam name="TBinding">The type of the binding object used when processing an expression.</typeparam>
abstract internal class BaseExpressionProcessor<TContext, TBinding> 
    : ISparqlExpressionProcessor<IValuedNode, TContext, TBinding>
{
    private DateTimeNode _now;
    private readonly IValuedNode _eNode = new DoubleNode(Math.E);
    private readonly IValuedNode _piNode = new DoubleNode(Math.PI);
    private readonly Random _rnd = new Random();
    private readonly Dictionary<string, ISparqlExpression> _functionCache = new Dictionary<string, ISparqlExpression>();


    protected BaseExpressionProcessor(ISparqlNodeComparer nodeComparer, IUriFactory uriFactory, bool useStrictOperators)
    {
        NodeComparer = nodeComparer;
        UriFactory = uriFactory;
        UseStrictOperators = useStrictOperators;
    }

    /// <summary>
    /// Provides a boolean flag that specifies whether strict operators should be used.
    /// </summary>
    protected bool UseStrictOperators { get; }

    protected ISparqlNodeComparer NodeComparer { get; }

    protected IUriFactory UriFactory { get; }

    protected abstract IValuedNode GetBoundValue(string variableName, TContext context, TBinding binding);
    protected abstract IEnumerable<ISparqlCustomExpressionFactory> GetExpressionFactories(TContext context);

    private IValuedNode ApplyBinaryOperator(BaseBinaryExpression expr, SparqlOperatorType operatorType,
        TContext context, TBinding binding)
    {
        IValuedNode[] inputs = new[]
        {
            expr.LeftExpression.Accept(this, context, binding),
            expr.RightExpression.Accept(this, context, binding),
        };
        if (!SparqlOperators.TryGetOperator(operatorType, UseStrictOperators, out ISparqlOperator op, inputs))
        {
            throw new RdfQueryException($"Cannot apply operator {operatorType} to the given inputs.");
        }

        return op.Apply(inputs);
    }

    public abstract IValuedNode ProcessAggregateTerm(AggregateTerm aggregate, TContext context, TBinding binding);
    

    public IValuedNode ProcessAllModifier(AllModifier all, TContext context, TBinding binding)
    {
        throw new NotImplementedException("This class is a placeholder only - aggregates taking this as an argument should apply over all rows");
    }

    public IValuedNode ProcessConstantTerm(ConstantTerm constant, TContext context, TBinding binding)
    {
        return constant.Node;
    }

    public IValuedNode ProcessDistinctModifier(DistinctModifier distinct, TContext context, TBinding binding)
    {
        throw new NotImplementedException("This class is a placeholder only - aggregates taking this as an argument should apply a DISTINCT modifer");
    }

    public IValuedNode ProcessGraphPatternTerm(GraphPatternTerm graphPattern, TContext context, TBinding binding)
    {
        throw new RdfQueryException("Graph Pattern Terms do not have a Node Value");
    }

    public IValuedNode ProcessVariableTerm(VariableTerm variable, TContext context, TBinding binding)
    {
        return GetBoundValue(variable.Name, context, binding);
    }

    public IValuedNode ProcessTripleNodeTerm(TripleNodeTerm tnTerm, TContext context, TBinding binding)
    {
        INode s = BindNode(tnTerm.Node.Triple.Subject, context, binding);
        INode p = BindNode(tnTerm.Node.Triple.Predicate, context, binding);
        INode o = BindNode(tnTerm.Node.Triple.Object, context, binding);
        return new TripleNode(new Triple(s, p, o)).AsValuedNode();
    }

    private INode BindNode(INode n, TContext context, TBinding binding)
    {
        if (n is IVariableNode vn) return GetBoundValue(vn.VariableName, context, binding);
        return n;
    }

    public virtual IValuedNode ProcessAdditionExpression(AdditionExpression addition, TContext context, TBinding binding)
    {
        return ApplyBinaryOperator(addition, SparqlOperatorType.Add, context, binding);
    }

    public virtual IValuedNode ProcessDivisionExpression(DivisionExpression division, TContext context, TBinding binding)
    {
        return ApplyBinaryOperator(division, SparqlOperatorType.Divide, context, binding);
    }

    public virtual IValuedNode ProcessMinusExpression(MinusExpression minus, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessMultiplicationExpression(MultiplicationExpression multiplication, TContext context,
        TBinding binding)
    {
        return ApplyBinaryOperator(multiplication, SparqlOperatorType.Multiply, context, binding);
    }

    public virtual IValuedNode ProcessSubtractionExpression(SubtractionExpression subtraction, TContext context, TBinding binding)
    {
        return ApplyBinaryOperator(subtraction, SparqlOperatorType.Subtract, context, binding);
    }

    public virtual IValuedNode ProcessEqualsExpression(EqualsExpression @equals, TContext context, TBinding binding)
    {
        IValuedNode x = @equals.LeftExpression.Accept(this, context, binding);
        IValuedNode y = @equals.RightExpression.Accept(this, context, binding);

        return new BooleanNode(SparqlSpecsHelper.Equality(x, y));
    }

    public virtual IValuedNode ProcessGreaterThanExpression(GreaterThanExpression gt, TContext context, TBinding binding)
    {
        IValuedNode a, b;
        a = gt.LeftExpression.Accept(this, context, binding);
        b = gt.RightExpression.Accept(this, context, binding);

        if (a == null) throw new RdfQueryException("Cannot evaluate a > when one argument is Null");

        if (NodeComparer.TryCompare(a, b, out var compare))
        {
            return new BooleanNode(compare > 0);
        }

        return null;
    }

    public virtual IValuedNode ProcessGreaterThanOrEqualToExpression(GreaterThanOrEqualToExpression gte, TContext context,
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

        if (NodeComparer.TryCompare(a, b, out var compare))
        {
            return new BooleanNode(compare >= 0);
        }

        return null;
    }

    public virtual IValuedNode ProcessLessThanExpression(LessThanExpression lt, TContext context, TBinding binding)
    {
        IValuedNode a, b;
        a = lt.LeftExpression.Accept(this, context, binding);
        b = lt.RightExpression.Accept(this, context, binding);

        if (a == null) throw new RdfQueryException("Cannot evaluate a < when one argument is Null");
        if (NodeComparer.TryCompare(a, b, out int compare))
        {
            return new BooleanNode(compare < 0);
        }

        return null;
    }

    public virtual IValuedNode ProcessLessThanOrEqualToExpression(LessThanOrEqualToExpression lte, TContext context, TBinding binding)
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

        return NodeComparer.TryCompare(a, b, out var compare) ? new BooleanNode(compare <= 0) : null;
    }

    public virtual IValuedNode ProcessNotEqualsExpression(NotEqualsExpression ne, TContext context, TBinding binding)
    {
        IValuedNode a = ne.LeftExpression.Accept(this, context, binding);
        IValuedNode b = ne.RightExpression.Accept(this, context, binding);
        return new BooleanNode(SparqlSpecsHelper.Inequality(a, b));
    }

    public virtual IValuedNode ProcessAndExpression(AndExpression and, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessNotExpression(NotExpression not, TContext context, TBinding binding)
    {
        return new BooleanNode(!not.InnerExpression.Accept(this, context, binding).AsSafeBoolean());
    }

    public virtual IValuedNode ProcessOrExpression(OrExpression or, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessArqBNodeFunction(BNodeFunction bNode, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessEFunction(EFunction e, TContext context, TBinding binding)
    {
        return _eNode;
    }

    public virtual IValuedNode ProcessLocalNameFunction(LocalNameFunction localName, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessMaxFunction(MaxFunction max, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessMinFunction(MinFunction min, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessNamespaceFunction(NamespaceFunction ns, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessNowFunction(NowFunction now, TContext context, TBinding binding)
    {
        // The original Leviathan function checked the context.Query property and returned
        // a new value whenever that property changed. This implementation assumes that
        // the BaseExpressionProcessor is not reused across multiple query evaluations.
        if (_now == null) _now = new DateTimeNode(DateTime.Now);
        return _now;
    }

    public virtual IValuedNode ProcessPiFunction(PiFunction pi, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessSha1Function(Sha1Function sha1, TContext context, TBinding binding)
    {
        return ProcessHashFunction(sha1.InnerExpression.Accept(this, context, binding), new SHA1Managed());
    }

    public virtual IValuedNode ProcessStringJoinFunction(StringJoinFunction stringJoin, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessSubstringFunction(SubstringFunction substring, TContext context, TBinding binding)
    {
        var input = (ILiteralNode)CheckArgument(substring.StringExpression, context, binding, XPathFunctionFactory.AcceptStringArguments);
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

    public virtual IValuedNode ProcessLeviathanMD5HashFunction(MD5HashFunction md5, TContext context, TBinding binding)
    {
        return ProcessHashFunction(md5.InnerExpression.Accept(this, context, binding), MD5.Create());
    }

    public virtual IValuedNode ProcessLeviathanSha256HashFunction(Sha256HashFunction sha256, TContext context, TBinding binding)
    {
        return ProcessHashFunction(sha256.InnerExpression.Accept(this, context, binding), new SHA256Managed());
    }

    public virtual IValuedNode ProcessCosecantFunction(CosecantFunction cosec, TContext context, TBinding binding)
    {
        return ProcessTrigFunction(cosec, d => cosec.Inverse ? Math.Asin(1 / d) : 1 / Math.Sin(d), context,
            binding);
    }

    public virtual IValuedNode ProcessCosineFunction(CosineFunction cos, TContext context, TBinding binding)
    {
        return ProcessTrigFunction(cos, d => cos.Inverse ? Math.Acos(d) : Math.Cos(d), context, binding);
    }

    public virtual IValuedNode ProcessCotangentFunction(CotangentFunction cot, TContext context, TBinding binding)
    {
        return ProcessTrigFunction(cot, d => cot.Inverse ? Math.Atan(1 / d) : Math.Cos(d) / Math.Sin(d), context,
            binding);
    }

    public virtual IValuedNode ProcessDegreesToRadiansFunction(DegreesToRadiansFunction degToRad, TContext context, TBinding binding)
    {
        IValuedNode temp = degToRad.InnerExpression.Accept(this, context, binding);

        if (temp == null) throw new RdfQueryException("Cannot apply a numeric function to a null");
        if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a numeric function to a non-numeric argument");

        return new DoubleNode(Math.PI * (temp.AsDouble() / 180d));
    }

    public virtual IValuedNode ProcessRadiansToDegreesFunction(RadiansToDegreesFunction radToDeg, TContext context, TBinding binding)
    {
        IValuedNode temp = radToDeg.InnerExpression.Accept(this, context, binding);

        if (temp == null) throw new RdfQueryException("Cannot apply a numeric function to a null");
        if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a numeric function to a non-numeric argument");

        return new DoubleNode(temp.AsDouble() * (180d / Math.PI));
    }

    public virtual IValuedNode ProcessSecantFunction(SecantFunction sec, TContext context, TBinding binding)
    {
        return ProcessTrigFunction(sec, d => sec.Inverse ? Math.Acos(1 / d) : 1 / Math.Cos(d), context, binding);
    }

    public virtual IValuedNode ProcessSineFunction(SineFunction sin, TContext context, TBinding binding)
    {
        return ProcessTrigFunction(sin, d => sin.Inverse ? Math.Asin(d) : Math.Sin(d), context, binding);
    }

    public virtual IValuedNode ProcessTangentFunction(TangentFunction tan, TContext context, TBinding binding)
    {
        return ProcessTrigFunction(tan, d => tan.Inverse ? Math.Atan(d) : Math.Tan(d), context, binding);
    }

    public virtual IValuedNode ProcessCartesianFunction(CartesianFunction cart, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessCubeFunction(CubeFunction cube, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessLeviathanEFunction(Expressions.Functions.Leviathan.Numeric.EFunction eFunction, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessFactorialFunction(FactorialFunction factorial, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessLogFunction(LogFunction log, TContext context, TBinding binding)
    {
        IValuedNode arg = log.LeftExpression.Accept(this, context, binding);
        if (arg == null) throw new RdfQueryException("Cannot log a null");

        IValuedNode logBase = log.RightExpression.Accept(this, context, binding);
        if (logBase == null) throw new RdfQueryException("Cannot log to a null base");

        if (arg.NumericType == SparqlNumericType.NaN || logBase.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot log when one/both arguments are non-numeric");

        return new DoubleNode(Math.Log(arg.AsDouble(), logBase.AsDouble()));
    }

    public virtual IValuedNode ProcessNaturalLogFunction(LeviathanNaturalLogFunction logn, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessPowerFunction(PowerFunction powFn, TContext context, TBinding binding)
    {
        IValuedNode arg = powFn.LeftExpression.Accept(this, context, binding);
        if (arg == null) throw new RdfQueryException("Cannot raise a null to a power");
        IValuedNode pow = powFn.RightExpression.Accept(this, context, binding);
        if (pow == null) throw new RdfQueryException("Cannot raise to a null power");

        if (arg.NumericType == SparqlNumericType.NaN || pow.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot raise to a power when one/both arguments are non-numeric");

        return new DoubleNode(Math.Pow(arg.AsDouble(), pow.AsDouble()));
    }

    public virtual IValuedNode ProcessPythagoreanDistanceFunction(PythagoreanDistanceFunction pythagoreanDistance, TContext context,
        TBinding binding)
    {
        IValuedNode x = pythagoreanDistance.LeftExpression.Accept(this, context, binding);
        if (x == null) throw new RdfQueryException("Cannot calculate distance of a null");
        IValuedNode y = pythagoreanDistance.RightExpression.Accept(this, context, binding);
        if (y == null) throw new RdfQueryException("Cannot calculate distance of a null");

        if (x.NumericType == SparqlNumericType.NaN || y.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot calculate distance when one/both arguments are non-numeric");

        return new DoubleNode(Math.Sqrt(Math.Pow(x.AsDouble(), 2) + Math.Pow(y.AsDouble(), 2)));
    }

    public virtual IValuedNode ProcessRandomFunction(RandomFunction rand, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessReciprocalFunction(ReciprocalFunction reciprocal, TContext context, TBinding binding)
    {
        IValuedNode temp = reciprocal.InnerExpression.Accept(this, context, binding);
        if (temp == null) throw new RdfQueryException("Cannot evaluate reciprocal of a null");
        var d = temp.AsDouble();
        if (d == 0) throw new RdfQueryException("Cannot evaluate reciprocal of zero");

        return new DoubleNode(1d / d);
    }

    public virtual IValuedNode ProcessRootFunction(RootFunction rootFn, TContext context, TBinding binding)
    {
        IValuedNode arg = rootFn.LeftExpression.Accept(this, context, binding);
        if (arg == null) throw new RdfQueryException("Cannot root a null");
        IValuedNode root = rootFn.RightExpression.Accept(this, context, binding);
        if (root == null) throw new RdfQueryException("Cannot root to a null root");

        if (arg.NumericType == SparqlNumericType.NaN || root.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot root when one/both arguments are non-numeric");

        return new DoubleNode(Math.Pow(arg.AsDouble(), 1d / root.AsDouble()));
    }

    public virtual IValuedNode ProcessSquareFunction(SquareFunction square, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessSquareRootFunction(SquareRootFunction sqrt, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessTenFunction(TenFunction ten, TContext context, TBinding binding)
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

    public virtual IValuedNode ProcessBoundFunction(BoundFunction bound, TContext context, TBinding binding)
    {
        return new BooleanNode(bound.InnerExpression.Accept(this, context, binding) != null);
    }

    public abstract IValuedNode ProcessExistsFunction(ExistsFunction exists, TContext context, TBinding binding);

    public virtual IValuedNode ProcessIsBlankFunction(IsBlankFunction isBlank, TContext context, TBinding binding)
    {
        INode result = isBlank.InnerExpression.Accept(this, context, binding);
        return result == null ? new BooleanNode(false) : new BooleanNode(result.NodeType == NodeType.Blank);
    }

    public virtual IValuedNode ProcessIsIriFunction(IsIriFunction isIri, TContext context, TBinding binding)
    {
        INode result = isIri.InnerExpression.Accept(this, context, binding);
        return result == null ? new BooleanNode(false) : new BooleanNode(result.NodeType == NodeType.Uri);
    }

    public virtual IValuedNode ProcessIsLiteralFunction(IsLiteralFunction isLiteral, TContext context, TBinding binding)
    {
        INode result = isLiteral.InnerExpression.Accept(this, context, binding);
        return result == null ? new BooleanNode(false) : new BooleanNode(result.NodeType == NodeType.Literal);

    }

    public virtual IValuedNode ProcessIsTripleFunction(IsTripleFunction isTriple, TContext context,
        TBinding binding)
    {
        INode result = isTriple.InnerExpression.Accept(this, context, binding);
        return result == null ? new BooleanNode(false) : new BooleanNode(result.NodeType == NodeType.Triple);
    }

    public virtual IValuedNode ProcessIsNumericFunction(IsNumericFunction isNumeric, TContext context, TBinding binding)
    {
        IValuedNode result = isNumeric.InnerExpression.Accept(this, context, binding);
        return new BooleanNode(result.NumericType != SparqlNumericType.NaN);
    }

    public virtual IValuedNode ProcessLangMatchesFunction(LangMatchesFunction langMatches, TContext context, TBinding binding)
    {
        INode result = langMatches.LeftExpression.Accept(this, context, binding);
        INode langRange = langMatches.RightExpression.Accept(this, context, binding);

        if (result == null)
        {
            return new BooleanNode(false);
        }
        if (result.NodeType == NodeType.Literal)
        {
            if (langRange == null)
            {
                return new BooleanNode(false);
            }
            if (langRange.NodeType == NodeType.Literal)
            {
                var range = ((ILiteralNode)langRange).Value;
                var lang = ((ILiteralNode)result).Value;

                if (range.Equals("*"))
                {
                    return new BooleanNode(!lang.Equals(string.Empty));
                }
                return new BooleanNode(lang.Equals(range, StringComparison.OrdinalIgnoreCase) || lang.StartsWith(range + "-", StringComparison.OrdinalIgnoreCase));
            }
            return new BooleanNode(false);
        }
        return new BooleanNode(false);
    }

    public virtual IValuedNode ProcessRegexFunction(RegexFunction regex, TContext context, TBinding binding)
    {
        // Configure Options
        RegexOptions options = RegexOptions.None;
        var pattern = string.Empty;
        if (regex.OptionsExpression != null && !regex.FixedOptions)
        {
            options = RegexFunction.GetOptions(regex.OptionsExpression.Accept(this, context, binding), true);
        }

        // Compile the Regex if necessary
        if (!regex.FixedPattern)
        {
            // Regex is not pre-compiled
            if (regex.PatternExpression != null)
            {
                IValuedNode p = regex.PatternExpression.Accept(this, context, binding);
                if (p != null)
                {
                    if (p.NodeType == NodeType.Literal)
                    {
                        pattern = p.AsString();
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
        else
        {
            pattern = regex.Pattern;
        }

        // Execute the Regular Expression
        IValuedNode textNode = regex.TextExpression.Accept(this, context, binding);
        if (textNode == null)
        {
            throw new RdfQueryException("Cannot evaluate a Regular Expression against a NULL");
        }
        if (textNode.NodeType == NodeType.Literal)
        {
            // Execute
            var text = textNode.AsString();
            if (regex.CompiledRegex != null)
            {
                return new BooleanNode(regex.CompiledRegex.IsMatch(text));
            }
            else
            {
                return new BooleanNode(Regex.IsMatch(text, pattern, options));
            }
        }
        else
        {
            throw new RdfQueryException("Cannot evaluate a Regular Expression against a non-Literal Node");
        }
    }

    public virtual IValuedNode ProcessSameTermFunction(SameTermFunction sameTerm, TContext context, TBinding binding)
    {
        INode a = sameTerm.LeftExpression.Accept(this, context, binding);
        INode b = sameTerm.RightExpression.Accept(this, context, binding);

        return a == null ? new BooleanNode(b == null) : new BooleanNode(a.Equals(b));
    }



    public abstract IValuedNode ProcessBNodeFunction(Expressions.Functions.Sparql.Constructor.BNodeFunction bNode,
        TContext context, TBinding binding);


    public abstract IValuedNode ProcessIriFunction(IriFunction iri, TContext context, TBinding binding);

    public virtual IValuedNode ProcessStrDtFunction(StrDtFunction strDt, TContext context, TBinding binding)
    {
        INode s = strDt.LeftExpression.Accept(this, context, binding);
        INode dt = strDt.RightExpression.Accept(this, context, binding);

        if (s == null)
        {
            throw new RdfQueryException("Cannot create a datatyped literal from a null string");
        }

        if (dt == null)
        {
            throw new RdfQueryException("Cannot create a datatyped literal from a null string");
        }

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
            var lit = (ILiteralNode)s;
            switch (lit.DataType?.AbsoluteUri)
            {
                case null:
                case XmlSpecsHelper.XmlSchemaDataTypeString:
                    return new StringNode(lit.Value, dtUri);
                case RdfSpecsHelper.RdfLangString:
                    throw new RdfQueryException("Cannot create a datatyped literal from a language specified literal");
                default:
                    throw new RdfQueryException("Cannot create a datatyped literal from a typed literal");
            }
        }

        throw new RdfQueryException("Cannot create a datatyped literal from a non-literal Node");

    }

    public virtual IValuedNode ProcessStrLangFunction(StrLangFunction strLang, TContext context, TBinding binding)
    {
        INode s = strLang.LeftExpression.Accept(this, context, binding);
        INode lang = strLang.RightExpression.Accept(this, context, binding);

        if (s != null)
        {
            if (lang != null)
            {
                string langSpec;
                if (lang.NodeType == NodeType.Literal)
                {
                    var langLit = (ILiteralNode)lang;
                    if (langLit.DataType == null)
                    {
                        langSpec = langLit.Value;
                    }
                    else
                    {
                        if (langLit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
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
                    var lit = (ILiteralNode)s;
                    if (lit.DataType == null || lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        if (lit.Language.Equals(string.Empty))
                        {
                            return new StringNode(lit.Value, langSpec);
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

    public virtual IValuedNode ProcessDayFunction(DayFunction day, TContext context, TBinding binding)
    {
        return ProcessDayFromDateTimeFunction(day, context, binding);
    }

    public virtual IValuedNode ProcessHoursFunction(HoursFunction hours, TContext context, TBinding binding)
    {
        return ProcessHoursFromDateTimeFunction(hours, context, binding);
    }

    public virtual IValuedNode ProcessMinutesFunction(MinutesFunction minutes, TContext context, TBinding binding)
    {
        return ProcessMinutesFromDateTimeFunction(minutes, context, binding);
    }

    public virtual IValuedNode ProcessMonthFunction(MonthFunction month, TContext context, TBinding binding)
    {
        return ProcessMonthFromDateTimeFunction(month, context, binding);
    }

    public virtual IValuedNode ProcessNowFunction(Expressions.Functions.Sparql.DateTime.NowFunction now, TContext context, TBinding binding)
    {
        return ProcessNowFunction((NowFunction)now, context, binding);
    }

    public virtual IValuedNode ProcessSecondsFunction(SecondsFunction seconds, TContext context, TBinding binding)
    {
        return ProcessSecondsFromDateTimeFunction(seconds, context, binding);
    }

    public virtual IValuedNode ProcessTimezoneFunction(TimezoneFunction timezone, TContext context, TBinding binding)
    {
        IValuedNode temp = ProcessTimezoneFromDateTimeFunction(timezone, context, binding);

        if (temp == null)
        {
            // Unlike base function must error if no timezone component
            throw new RdfQueryException("Cannot get the Timezone from a Date Time that does not have a timezone component");
        }

        // Otherwise the base value is fine
        return temp;
    }

    public virtual IValuedNode ProcessTZFunction(TZFunction tz, TContext context, TBinding binding)
    {
        IValuedNode temp = tz.InnerExpression.Accept(this, context, binding);
        if (temp != null)
        {
            DateTimeOffset dt = temp.AsDateTimeOffset();
            // Regex based check to see if the value has a Timezone component
            // If not then the result is a null
            if (!Regex.IsMatch(temp.AsString(), "(Z|[+-]\\d{2}:\\d{2})$")) return new StringNode(null, string.Empty);

            // Now we have a DateTime we can try and return the Timezone
            if (dt.Offset.Equals(TimeSpan.Zero))
            {
                // If Zero it was specified as Z (which means UTC so zero offset)
                return new StringNode(null, "Z");
            }
            else
            {
                // If the Offset is outside the range -14 to 14 this is considered invalid
                if (dt.Offset.Hours < -14 || dt.Offset.Hours > 14) return null;

                // Otherwise it has an offset which is a given number of hours (and minutes)
                return new StringNode(null, dt.Offset.Hours.ToString("00") + ":" + dt.Offset.Minutes.ToString("00"));
            }
        }
        else
        {
            throw new RdfQueryException("Unable to evaluate a Date Time function on a null argument");
        }
    }

    public virtual IValuedNode ProcessYearFunction(YearFunction year, TContext context, TBinding binding)
    {
        return ProcessYearsFromDateTimeFunction(year, context, binding);
    }

    public virtual IValuedNode ProcessMd5HashFunction(Expressions.Functions.Sparql.Hash.MD5HashFunction md5, TContext context, TBinding binding)
    {
        return ProcessLeviathanMD5HashFunction(md5, context, binding);
    }

    public virtual IValuedNode ProcessSha1HashFunction(Sha1HashFunction sha1, TContext context, TBinding binding)
    {
        try
        {
            return ProcessHashFunction(sha1.InnerExpression.Accept(this, context, binding), new SHA1Managed());
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }

    public virtual IValuedNode ProcessSha256HashFunction(Expressions.Functions.Sparql.Hash.Sha256HashFunction sha256, TContext context, TBinding binding)
    {
        return ProcessHashFunction(sha256.InnerExpression.Accept(this, context, binding), new SHA256Managed());
    }

    public virtual IValuedNode ProcessSha384HashFunction(Sha384HashFunction sha384, TContext context, TBinding binding)
    {
        return ProcessHashFunction(sha384.InnerExpression.Accept(this, context, binding), new SHA384Managed());
    }

    public virtual IValuedNode ProcessSha512HashFunction(Sha512HashFunction sha512, TContext context, TBinding binding)
    {
        return ProcessHashFunction(sha512.InnerExpression.Accept(this, context, binding), new SHA512Managed());
    }

    public virtual IValuedNode ProcessAbsFunction(AbsFunction abs, TContext context, TBinding binding)
    {
        return ProcessAbsFunction((XPath.Numeric.AbsFunction)abs, context, binding);
    }

    public virtual IValuedNode ProcessCeilFunction(CeilFunction ceil, TContext context, TBinding binding)
    {
        return ProcessCeilFunction(ceil as CeilingFunction, context, binding);
    }

    public virtual IValuedNode ProcessFloorFunction(FloorFunction floor, TContext context, TBinding binding)
    {
        return ProcessFloorFunction(floor as XPath.Numeric.FloorFunction, context, binding);
    }

    public abstract IValuedNode ProcessRandFunction(RandFunction rand, TContext context, TBinding binding);

    public virtual IValuedNode ProcessRoundFunction(RoundFunction round, TContext context, TBinding binding)
    {
        return ProcessRoundFunction(round as XPath.Numeric.RoundFunction, context, binding);
    }

    public virtual IValuedNode ProcessInFunction(InFunction inFn, TContext context, TBinding binding)
    {
        IValuedNode result = inFn.InnerExpression.Accept(this, context, binding);
        if (result != null)
        {
            if (inFn.SetExpressions.Count == 0) return new BooleanNode(false);

            // Have to use SPARQL Value Equality here
            // If any expressions error and nothing in the set matches then an error is thrown
            var errors = false;
            foreach (ISparqlExpression expr in inFn.SetExpressions)
            {
                try
                {
                    IValuedNode temp = expr.Accept(this, context, binding);
                    if (SparqlSpecsHelper.Equality(result, temp)) return new BooleanNode(true);
                }
                catch
                {
                    errors = true;
                }
            }

            if (errors)
            {
                throw new RdfQueryException("One/more expressions in a Set function failed to evaluate");
            }
            else
            {
                return new BooleanNode(false);
            }
        }
        else
        {
            return new BooleanNode(false);
        }
    }

    public virtual IValuedNode ProcessNotInFunction(NotInFunction notIn, TContext context, TBinding binding)
    {
        INode result = notIn.InnerExpression.Accept(this, context, binding);
        if (result != null)
        {
            if (notIn.SetExpressions.Count == 0) return new BooleanNode(true);

            // Have to use SPARQL Value Equality here
            // If any expressions error and nothing in the set matches then an error is thrown
            var errors = false;
            foreach (ISparqlExpression expr in notIn.SetExpressions)
            {
                try
                {
                    INode temp = expr.Accept(this, context, binding);
                    if (SparqlSpecsHelper.Equality(result, temp)) return new BooleanNode(false);
                }
                catch
                {
                    errors = true;
                }
            }

            if (errors)
            {
                throw new RdfQueryException("One/more expressions in a Set function failed to evaluate");
            }
            else
            {
                return new BooleanNode(true);
            }
        }
        else
        {
            return new BooleanNode(true);
        }
    }

    public virtual IValuedNode ProcessConcatFunction(ConcatFunction concat, TContext context, TBinding binding)
    {
        string langTag = null;
        var allLangTagged = true;
        var allString = true;
        var allSameTag = true;

        var output = new StringBuilder();
        foreach (ISparqlExpression expr in concat.Arguments)
        {
            INode temp = expr?.Accept(this, context, binding) ?? new StringNode(String.Empty);
            if (temp == null) throw new RdfQueryException("Cannot evaluate the SPARQL CONCAT() function when an argument evaluates to a Null");
            if (temp is not ILiteralNode lit)
            {
                throw new RdfQueryException(
                    "Cannot evaluate the SPARQL CONCAT() function when an argument is not a Literal Node");
            }

            switch (lit.DataType?.AbsoluteUri)
            {
                case null:
                case XmlSpecsHelper.XmlSchemaDataTypeString:
                    allLangTagged = false;
                    break;
                case RdfSpecsHelper.RdfLangString:
                    langTag ??= lit.Language;
                    allSameTag = allSameTag && langTag.Equals(lit.Language);
                    allString = false;
                    break;
                default:
                    throw new RdfQueryException(
                        "Cannot evaluate the SPARQL CONCAT() function when an argument is not a string literal");
            }

            output.Append(lit.Value);
        }

        // Produce the appropriate literal form depending on our inputs
        if (allString)
        {
            return new StringNode(output.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }

        if (allLangTagged && allSameTag)
        {
            return new StringNode(output.ToString(), langTag);
        }

        return new StringNode(output.ToString());
    }


    private IValuedNode ProcessBinaryStringBooleanFunction(BaseBinaryStringFunction function, TContext context,
        TBinding binding, Func<ILiteralNode, ILiteralNode, bool> valueFunc)
    {
        INode x = function.LeftExpression.Accept(this, context, binding);
        INode y = function.RightExpression.Accept(this, context, binding);

        if (x.NodeType == NodeType.Literal && y.NodeType == NodeType.Literal)
        {
            var stringLit = (ILiteralNode)x;
            var argLit = (ILiteralNode)y;

            if (IsValidArgumentPair(stringLit, argLit))
            {
                return new BooleanNode(valueFunc(stringLit, argLit));
            }
            else
            {
                throw new RdfQueryException("The Literals provided as arguments to this SPARQL String function are not of valid forms (see SPARQL spec for acceptable combinations)");
            }
        }
        else
        {
            throw new RdfQueryException("Arguments to a SPARQL String function must both be Literal Nodes");
        }
    }

    private IValuedNode ProcessBinaryStringFunction(XPath.String.BaseBinaryStringFunction function, TContext context,
        TBinding binding, Func<ILiteralNode, ILiteralNode, IValuedNode> valueFunc)
    {
        INode temp = function.LeftExpression.Accept(this, context, binding);
        if (temp != null)
        {
            if (temp.NodeType == NodeType.Literal)
            {
                var lit = (ILiteralNode)temp;
                if (lit.DataType != null && !lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                {
                    throw new RdfQueryException("Unable to evaluate an XPath String function on a non-string typed Literal");
                }
            }
            else
            {
                throw new RdfQueryException("Unable to evaluate an XPath String function on a non-Literal input");
            }

            // Once we've got to here we've established that the First argument is an appropriately typed/untyped Literal
            if (function.RightExpression == null)
            {
                if (!function.AllowNullArgument)
                {
                    throw new RdfQueryException("This XPath function requires a non-null argument in addition to an input string");
                }
                return valueFunc((ILiteralNode)temp, null);
            }
            else
            {
                // Need to validate the argument
                INode tempArg = function.RightExpression.Accept(this, context, binding);
                if (tempArg != null)
                {
                    if (tempArg.NodeType == NodeType.Literal)
                    {
                        var litArg = (ILiteralNode)tempArg;
                        if (function.ValidateArgumentType(litArg.DataType))
                        {
                            return valueFunc((ILiteralNode)temp, litArg);
                        }
                        else
                        {
                            throw new RdfQueryException("Unable to evaluate an XPath String function since the type of the argument is not supported by this function");
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to evaluate an XPath String function where the argument is a non-Literal");
                    }
                }
                else if (function.AllowNullArgument)
                {
                    // Null argument permitted so just invoke the non-argument version of the function
                    return valueFunc((ILiteralNode)temp, null);
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
    /// Determines whether the Arguments are valid.
    /// </summary>
    /// <param name="stringLit">String Literal.</param>
    /// <param name="argLit">Argument Literal.</param>
    /// <returns></returns>
    protected bool IsValidArgumentPair(ILiteralNode stringLit, ILiteralNode argLit)
    {
        var arg1Datatype = stringLit.DataType?.AbsoluteUri;
        var arg2Datatype = argLit.DataType?.AbsoluteUri;
        switch (arg1Datatype)
        {
            case null:
            case XmlSpecsHelper.XmlSchemaDataTypeString:
                return arg2Datatype == null || arg2Datatype == XmlSpecsHelper.XmlSchemaDataTypeString;
            case RdfSpecsHelper.RdfLangString:
                return arg2Datatype == null || arg2Datatype == XmlSpecsHelper.XmlSchemaDataTypeString ||
                       (arg2Datatype == RdfSpecsHelper.RdfLangString && stringLit.Language.Equals(argLit.Language));
            default:
                // First argument is not a string literal
                return false;
        }
    }

    public virtual IValuedNode ProcessContainsFunction(ContainsFunction contains, TContext context, TBinding binding)
    {
        return ProcessBinaryStringBooleanFunction(contains, context, binding, (x, y) => x.Value.Contains(y.Value));
    }

    public virtual IValuedNode ProcessDataTypeFunction(DataTypeFunction dataType, TContext context, TBinding binding)
    {
        INode result = dataType.InnerExpression.Accept(this, context, binding);
        if (result == null)
        {
            throw new RdfQueryException("Cannot return the Data Type URI of a NULL");
        }
        else
        {
            switch (result.NodeType)
            {
                case NodeType.Literal:
                    var lit = (ILiteralNode)result;
                    if (lit.DataType == null)
                    {
                        if (!lit.Language.Equals(string.Empty))
                        {
                            throw new RdfQueryException("Cannot return the Data Type URI of Language Specified Literals in SPARQL 1.0");
                        }
                        else
                        {
                            return new UriNode(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                        }
                    }
                    else
                    {
                        return new UriNode(lit.DataType);
                    }

                default:
                    throw new RdfQueryException("Cannot return the Data Type URI of Nodes which are not Literal Nodes");
            }
        }
    }

    public virtual IValuedNode ProcessDataType11Function(DataType11Function dataType, TContext context, TBinding binding)
    {
        INode result = dataType.InnerExpression.Accept(this, context, binding);
        if (result == null)
        {
            throw new RdfQueryException("Cannot return the Data Type URI of a NULL");
        }

        switch (result.NodeType)
        {
            case NodeType.Literal:
                var lit = (ILiteralNode)result;
                if (lit.DataType == null)
                {
                    return !lit.Language.Equals(string.Empty)
                        ? new UriNode(UriFactory.Create(RdfSpecsHelper.RdfLangString))
                        : new UriNode(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                else
                {
                    return new UriNode(lit.DataType);
                }

            default:
                throw new RdfQueryException("Cannot return the Data Type URI of Nodes which are not Literal Nodes");
        }
    }

    protected virtual IValuedNode ProcessUnaryStringFunction(BaseUnaryStringFunction function, TContext context,
        TBinding binding, Func<ILiteralNode, IValuedNode> valueFunc)
    {
        IValuedNode temp = function.InnerExpression.Accept(this, context, binding);
        if (temp == null)
        {
            throw new RdfQueryException("Unable to evaluate an XPath String function on a null input");
        }

        if (temp.NodeType != NodeType.Literal)
        {
            throw new RdfQueryException("Unable to evaluate an XPath String function on a non-Literal input");
        }

        var lit = (ILiteralNode)temp;
        if (lit.DataType == null)
        {
            return valueFunc(lit);
        }

        return lit.DataType.AbsoluteUri switch
        {
            XmlSpecsHelper.XmlSchemaDataTypeString or RdfSpecsHelper.RdfLangString => valueFunc(lit),
            _ => throw new RdfQueryException(
                "Unable to evaluate an XPath String function on a non-string typed Literal")
        };
    }
    public virtual IValuedNode ProcessEncodeForUriFunction(EncodeForUriFunction encodeForUri, TContext context, TBinding binding)
    {
        return ProcessUnaryStringFunction(encodeForUri, context, binding,
            stringLit => new StringNode(null, Uri.EscapeUriString(stringLit.Value)));
    }

    public virtual IValuedNode ProcessLangFunction(LangFunction lang, TContext context, TBinding binding)
    {
        INode result = lang.InnerExpression.Accept(this, context, binding);
        if (result == null)
        {
            throw new RdfQueryException("Cannot return the Data Type URI of an NULL");
        }

        switch (result.NodeType)
        {
            case NodeType.Literal:
                return new StringNode(((ILiteralNode)result).Language);

            default:
                throw new RdfQueryException("Cannot return the Language Tag of Nodes which are not Literal Nodes");
        }
    }

    public virtual IValuedNode ProcessLCaseFunction(LCaseFunction lCase, TContext context, TBinding binding)
    {
        return ProcessUnaryStringFunction(lCase, context, binding,
            stringLit => stringLit.DataType == null
                ? new StringNode(stringLit.Value.ToLower(), stringLit.Language)
                : new StringNode(stringLit.Value.ToLower(), stringLit.DataType));
    }

    public virtual IValuedNode ProcessReplaceFunction(ReplaceFunction replace, TContext context, TBinding binding)
    {
        RegexOptions options = replace.FixedOptions
            ? replace.Options
            : replace.GetOptions(replace.OptionsExpression.Accept(this, context, binding), true);
        string findPattern, replacePattern;
        if (replace.FixedPattern)
        {
            findPattern = replace.Find;
        }
        else
        {
            if (replace.FindExpression != null)
            {
                IValuedNode p = replace.FindExpression.Accept(this, context, binding);
                if (p != null)
                {
                    if (p.NodeType == NodeType.Literal)
                    {
                        findPattern = p.AsString();
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

        if (replace.FixedReplace)
        {
            replacePattern = replace.Replace;
        }
        else
        {
            if (replace.ReplaceExpression != null)
            {
                IValuedNode r = replace.ReplaceExpression.Accept(this, context, binding);
                if (r != null)
                {
                    if (r.NodeType == NodeType.Literal)
                    {
                        replacePattern = r.AsString();
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

        IValuedNode textNode = replace.TextExpression.Accept(this, context, binding);
        if (textNode == null)
        {
            throw new RdfQueryException("Cannot evaluate a Regular Expression against a NULL");
        }

        if (textNode.NodeType != NodeType.Literal)
        {
            throw new RdfQueryException("Cannot evaluate a Regular Expression against a non-Literal Node");
        }

        // Execute
        var lit = (ILiteralNode)textNode;
        if (lit.DataType != null && !lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString) &&
            !lit.DataType.AbsoluteUri.Equals(RdfSpecsHelper.RdfLangString))
        {
            throw new RdfQueryException("Text Argument to Replace must be of type xsd:string if a datatype is specified");
        }
        var text = lit.Value;
        var output = Regex.Replace(text, findPattern, replacePattern, options);

        if (lit.DataType != null)
        {
            return new StringNode(output, lit.DataType);
        }

        return !lit.Language.Equals(string.Empty) ? new StringNode(output, lit.Language) : new StringNode(output);
    }

    public virtual IValuedNode ProcessStrAfterFunction(StrAfterFunction strAfter, TContext context, TBinding binding)
    {
        var input = CheckArgument(strAfter.StringExpression, context, binding, XPathFunctionFactory.AcceptStringArguments) as ILiteralNode;
        var starts = CheckArgument(strAfter.StartsExpression, context, binding, XPathFunctionFactory.AcceptStringArguments) as ILiteralNode;

        if (input == null || starts == null || !IsValidArgumentPair(input, starts))
        {
            throw new RdfQueryException("The Literals provided as arguments to this SPARQL String function are not of valid forms (see SPARQL spec for acceptable combinations)");
        }

        Uri datatype = input.DataType;//(input.DataType != null ? input.DataType : starts.DataType);
        var lang = input.Language;//(!input.Language.Equals(string.Empty) ? input.Language : starts.Language);

        if (input.Value.Contains(starts.Value))
        {
            var startIndex = input.Value.IndexOf(starts.Value, StringComparison.InvariantCulture) + starts.Value.Length;
            var resultValue = startIndex >= input.Value.Length ? string.Empty : input.Value.Substring(startIndex);

            if (datatype != null)
            {
                return new StringNode(resultValue, datatype);
            }
            else if (!lang.Equals(string.Empty))
            {
                return new StringNode(resultValue, lang);
            }
            else
            {
                return new StringNode(resultValue);
            }
        }
        else if (starts.Value.Equals(string.Empty))
        {
            if (datatype != null)
            {
                return new StringNode(string.Empty, datatype);
            }
            else
            {
                return new StringNode(string.Empty/*, lang*/);
            }
        }
        else
        {
            return new StringNode(string.Empty);
        }
    }

    public virtual IValuedNode ProcessStrBeforeFunction(StrBeforeFunction strBefore, TContext context, TBinding binding)
    {
        var input = CheckArgument(strBefore.StringExpression, context, binding, XPathFunctionFactory.AcceptStringArguments) as ILiteralNode;
        var ends = CheckArgument(strBefore.EndsExpression, context, binding, XPathFunctionFactory.AcceptStringArguments) as ILiteralNode;

        if (input == null || ends == null || !IsValidArgumentPair(input, ends))
        {
            throw new RdfQueryException("The Literals provided as arguments to this SPARQL String function are not of valid forms (see SPARQL spec for acceptable combinations)");
        }

        Uri datatype = input.DataType;//(input.DataType != null ? input.DataType : ends.DataType);
        var lang = input.Language;// (!input.Language.Equals(string.Empty) ? input.Language : ends.Language);

        if (input.Value.Contains(ends.Value))
        {
            var endIndex = input.Value.IndexOf(ends.Value, StringComparison.InvariantCulture);
            var resultValue = endIndex == 0 ? string.Empty : input.Value.Substring(0, endIndex);

            if (datatype != null)
            {
                return new StringNode(resultValue, datatype);
            }
            else if (!lang.Equals(string.Empty))
            {
                return new StringNode(resultValue, lang);
            }
            else
            {
                return new StringNode(resultValue);
            }
        }
        else if (ends.Value.Equals(string.Empty))
        {
            if (datatype != null)
            {
                return new StringNode(string.Empty, datatype);

            }
            else
            {
                return new StringNode(string.Empty, lang);
            }
        }
        else
        {
            return new StringNode(string.Empty);
        }
    }

    public virtual IValuedNode ProcessStrEndsFunction(StrEndsFunction strEnds, TContext context, TBinding binding)
    {
        return ProcessBinaryStringBooleanFunction(strEnds, context, binding, (x, y) => x.Value.EndsWith(y.Value));
    }

    public virtual IValuedNode ProcessStrFunction(StrFunction str, TContext context, TBinding binding)
    {
        IValuedNode result = str.InnerExpression.Accept(this, context, binding);
        if (result == null)
        {
            throw new RdfQueryException("Cannot return the lexical value of an NULL");
        }
        else
        {
            switch (result.NodeType)
            {
                case NodeType.Literal:
                case NodeType.Uri:
                    return new StringNode(result.AsString());

                default:
                    throw new RdfQueryException("Cannot return the lexical value of Nodes which are not Literal/URI Nodes");

            }
        }
    }

    public virtual IValuedNode ProcessStrLenFunction(StrLenFunction strLen, TContext context, TBinding binding)
    {
        return ProcessUnaryStringFunction(strLen, context, binding, x => new LongNode(x.Value.Length));
    }

    public virtual IValuedNode ProcessStrStartsFunction(StrStartsFunction strStarts, TContext context, TBinding binding)
    {
        return ProcessBinaryStringBooleanFunction(strStarts, context, binding, (x, y) => x.Value.StartsWith(y.Value));
    }

    public virtual IValuedNode ProcessSubStrFunction(SubStrFunction subStr, TContext context, TBinding binding)
    {
        var input = (ILiteralNode)CheckArgument(subStr.StringExpression, context, binding, XPathFunctionFactory.AcceptStringArguments);
        IValuedNode start = CheckArgument(subStr.StartExpression, context, binding, XPathFunctionFactory.AcceptNumericArguments);

        if (subStr.LengthExpression != null)
        {
            IValuedNode length = CheckArgument(subStr.LengthExpression, context, binding, XPathFunctionFactory.AcceptNumericArguments);

            if (input.Value.Equals(string.Empty))
            {
                return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }

            try
            {
                var s = Convert.ToInt32(start.AsInteger());
                var l = Convert.ToInt32(length.AsInteger());

                if (s < 1) s = 1;
                if (l < 1)
                {
                    // If no/negative characters are being selected the empty string is returned
                    if (input.DataType != null)
                    {
                        return new StringNode(string.Empty, input.DataType);
                    }
                    else
                    {
                        return new StringNode(string.Empty, input.Language);
                    }
                }
                else if (s - 1 > input.Value.Length)
                {
                    // If the start is after the end of the string the empty string is returned
                    if (input.DataType != null)
                    {
                        return new StringNode(string.Empty, input.DataType);
                    }
                    else
                    {
                        return new StringNode(string.Empty, input.Language);
                    }
                }
                else
                {
                    if (s - 1 + l > input.Value.Length)
                    {
                        // If the start plus the length is greater than the length of the string the string from the starts onwards is returned
                        if (input.DataType != null)
                        {
                            return new StringNode(input.Value.Substring(s - 1), input.DataType);
                        }
                        else
                        {
                            return new StringNode(input.Value.Substring(s - 1), input.Language);
                        }
                    }
                    else
                    {
                        // Otherwise do normal substring
                        if (input.DataType != null)
                        {
                            return new StringNode(input.Value.Substring(s - 1, l), input.DataType);
                        }
                        else
                        {
                            return new StringNode(input.Value.Substring(s - 1, l), input.Language);
                        }
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
            if (input.Value.Equals(string.Empty))
            {
                return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }

            try
            {
                var s = Convert.ToInt32(start.AsInteger());
                if (s < 1) s = 1;

                if (input.DataType != null)
                {
                    return new StringNode(input.Value.Substring(s - 1), input.DataType);
                }
                else
                {
                    return new StringNode(input.Value.Substring(s - 1), input.Language);
                }
            }
            catch
            {
                throw new RdfQueryException("Unable to convert the Start argument to an Integer");
            }
        }
    }

    public virtual IValuedNode ProcessUCaseFunction(UCaseFunction uCase, TContext context, TBinding binding)
    {
        return ProcessUnaryStringFunction(uCase, context, binding,
            x => x.DataType == null
                ? new StringNode(x.Value.ToUpper(), x.Language)
                : new StringNode(x.Value.ToUpper(), x.DataType));
    }

    public virtual IValuedNode ProcessUuidFunction(UUIDFunction uuid, TContext context, TBinding binding)
    {
        return new UriNode(new Uri("urn:uuid:" + Guid.NewGuid()));
    }

    public virtual IValuedNode ProcessStrUuidFunction(StrUUIDFunction uuid, TContext context, TBinding binding)
    {
        return new StringNode(Guid.NewGuid().ToString());
    }

    public virtual IValuedNode ProcessCallFunction(CallFunction call, TContext context, TBinding binding)
    {
        if (!call.Arguments.Any()) return null;

        IValuedNode funcIdent = call.Arguments.First().Accept(this, context, binding);
        if (funcIdent == null) throw new RdfQueryException("Function identifier is unbound");
        if (funcIdent.NodeType != NodeType.Uri) throw new RdfQueryException("Function identifier is not a URI");

        Uri funcUri = ((IUriNode)funcIdent).Uri;
        if (!_functionCache.TryGetValue(funcUri.AbsoluteUri, out ISparqlExpression func))
        {
            try
            {
                // Try to create the function and cache it - remember to respect the queries Expression Factories if present
                func = SparqlExpressionFactory.CreateExpression(funcUri, call.Arguments.Skip(1).ToList(),
                    GetExpressionFactories(context) ?? Enumerable.Empty<ISparqlCustomExpressionFactory>(), false);
                _functionCache.Add(funcUri.AbsoluteUri, func);
            }
            catch
            {
                // If something goes wrong creating the function cache a null so we ignore this function URI for later calls
                _functionCache.Add(funcUri.AbsoluteUri, null);
            }
        }

        // Now invoke the function
        if (func == null) throw new RdfQueryException("Function identifier does not identify a known function");
        return func.Accept(this, context, binding);
    }

    public virtual IValuedNode ProcessCoalesceFunction(CoalesceFunction coalesce, TContext context, TBinding binding)
    {
        foreach (ISparqlExpression expr in coalesce.InnerExpressions)
        {
            try
            {
                // Test the expression
                IValuedNode temp = expr.Accept(this, context, binding);
                if (temp != null) return temp;
            }
            catch (RdfQueryException)
            {
                // Ignore the error and try the next expression (if any)
            }
        }

        // Return error if all expressions are null/error
        throw new RdfQueryException("None of the arguments to the COALESCE function could be evaluated to give non-null/error responses for the given Binding");

    }

    public virtual IValuedNode ProcessIfElseFunction(IfElseFunction ifElse, TContext context, TBinding binding)
    {
        IValuedNode result = ifElse.ConditionExpression.Accept(this, context, binding);

        // Condition evaluated without error so we go to the appropriate branch of the IF ELSE
        // depending on whether it evaluated to true or false
        return result.AsSafeBoolean()
            ? ifElse.TrueBranchExpression.Accept(this, context, binding)
            : ifElse.FalseBranchExpression.Accept(this, context, binding);
    }

    public IValuedNode ProcessSubjectFunction(SubjectFunction subjectFunction, TContext context, TBinding binding)
    {
        IValuedNode innerValue = subjectFunction.InnerExpression.Accept(this, context, binding);
        if (innerValue is ITripleNode tn) return tn.Triple.Subject.AsValuedNode();
        throw new RdfQueryException("Value passed to SUBJECT function is not a Triple Node. Received a " +
                                    innerValue.NodeType);
    }

    public IValuedNode ProcessPredicateFunction(PredicateFunction predicateFunction, TContext context, TBinding binding)
    {
        IValuedNode innerValue = predicateFunction.InnerExpression.Accept(this, context, binding);
        if (innerValue is ITripleNode tn) return tn.Triple.Predicate.AsValuedNode();
        throw new RdfQueryException("Value passed to PREDICATE function is not a Triple Node. Received a " +
                                    innerValue.NodeType);
    }

    public IValuedNode ProcessObjectFunction(ObjectFunction objectFunction, TContext context, TBinding binding)
    {
        IValuedNode innerValue = objectFunction.InnerExpression.Accept(this, context, binding);
        if (innerValue is ITripleNode tn) return tn.Triple.Object.AsValuedNode();
        throw new RdfQueryException("Value passed to OBJECT function is not a Triple Node. Received a " +
                                    innerValue.NodeType);
    }

    public virtual IValuedNode ProcessBooleanCast(BooleanCast cast, TContext context, TBinding binding)
    {
        IValuedNode n = cast.InnerExpression.Accept(this, context, binding);//.CoerceToBoolean();

        switch (n.NodeType)
        {
            case NodeType.Blank:
            case NodeType.GraphLiteral:
            case NodeType.Uri:
                throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:boolean");

            case NodeType.Literal:
                // See if the value can be cast
                if (n is BooleanNode) return n;
                var lit = (ILiteralNode)n;
                if (lit.DataType != null)
                {
                    var dt = lit.DataType.ToString();

                    if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
                    {
                        // Already a Boolean
                        if (bool.TryParse(lit.Value, out var b))
                        {
                            return new BooleanNode(b);
                        }

                        if (lit.Value == "true" || lit.Value == "1") { return new BooleanNode(true); }

                        if (lit.Value == "false" || lit.Value == "0") { return new BooleanNode(false); }

                        throw new RdfQueryException("Invalid Lexical Form for xsd:boolean");
                    }

                    if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        // Can cast if lexical form matches xsd:boolean's lexical space
                        if (lit.Value.Equals("0") || lit.Value.Equals("false")) return new BooleanNode(false);
                        if (lit.Value.Equals("1") || lit.Value.Equals("true")) return new BooleanNode(true);
                        throw new RdfQueryException("Invalid string value for casting to xsd:boolean");
                    }

                    // Cast based on Numeric Type
                    SparqlNumericType type = NumericTypesHelper.GetNumericTypeFromDataTypeUri(dt);

                    switch (type)
                    {
                        case SparqlNumericType.Decimal:
                            if (decimal.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                            {
                                if (dec.Equals(decimal.Zero))
                                {
                                    return new BooleanNode(false);
                                }
                                else
                                {
                                    return new BooleanNode(true);
                                }
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:decimal as an intermediate stage in casting to a xsd:boolean");
                            }

                        case SparqlNumericType.Float:
                            if (float.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture,
                                    out var fl))
                            {
                                return float.IsNaN(fl) || fl == 0.0d
                                    ? new BooleanNode(false)
                                    : new BooleanNode(true);
                            }
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:float as an intermediate stage in casting to a xsd:boolean");

                        case SparqlNumericType.Double:
                            if (double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl))
                            {
                                if (double.IsNaN(dbl) || dbl == 0.0d)
                                {
                                    return new BooleanNode(false);
                                }
                                else
                                {
                                    return new BooleanNode(true);
                                }
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double as an intermediate stage in casting to a xsd:boolean");
                            }

                        case SparqlNumericType.Integer:
                            if (long.TryParse(lit.Value, out var i))
                            {
                                if (i == 0)
                                {
                                    return new BooleanNode(false);
                                }
                                else
                                {
                                    return new BooleanNode(true);
                                }
                            }
                            else
                            {
                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:integer as an intermediate stage in casting to a xsd:boolean");
                            }

                        case SparqlNumericType.NaN:
                            if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                            {
                                // DateTime cast forbidden
                                throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:boolean");
                            }
                            else
                            {
                                if (bool.TryParse(lit.Value, out var b))
                                {
                                    return new BooleanNode(b);
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
                    if (bool.TryParse(lit.Value, out var b))
                    {
                        return new BooleanNode(b);
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

    public virtual IValuedNode ProcessDateTimeCast(DateTimeCast cast, TContext context, TBinding binding)
    {
        IValuedNode n = cast.InnerExpression.Accept(this, context, binding);//.CoerceToDateTime();

        if (n == null)
        {
            throw new RdfQueryException("Cannot cast a Null to a xsd:dateTime");
        }

        // New method should be much faster
        // if (n is DateTimeNode) return n;
        // if (n is DateNode) return new DateTimeNode(n.Graph, n.AsDateTime());
        // return new DateTimeNode(null, n.AsDateTime());

        switch (n.NodeType)
        {
            case NodeType.Blank:
            case NodeType.GraphLiteral:
            case NodeType.Uri:
                throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:dateTime");

            case NodeType.Literal:
                if (n is DateTimeNode) return n;
                if (n is DateNode) return new DateTimeNode(n.AsDateTime());
                // See if the value can be cast
                var lit = (ILiteralNode)n;
                if (lit.DataType != null)
                {
                    var dt = lit.DataType.ToString();
                    if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                    {
                        // Already a xsd:dateTime
                        if (DateTimeOffset.TryParse(lit.Value, out DateTimeOffset d))
                        {
                            // Parsed OK
                            return new DateTimeNode(d);
                        }
                        else
                        {
                            throw new RdfQueryException("Invalid lexical form for xsd:dateTime");
                        }

                    }
                    else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                    {
                        if (DateTimeOffset.TryParse(lit.Value, out DateTimeOffset d))
                        {
                            // Parsed OK
                            return new DateTimeNode(d);
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
                    if (DateTimeOffset.TryParse(lit.Value, out DateTimeOffset d))
                    {
                        // Parsed OK
                        return new DateTimeNode(d);
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

    public virtual IValuedNode ProcessDecimalCast(DecimalCast cast, TContext context, TBinding binding)
    {
        IValuedNode n = cast.InnerExpression.Accept(this, context, binding);//.CoerceToDecimal();

        if (n == null)
        {
            throw new RdfQueryException("Cannot cast a Null to a xsd:decimal");
        }

        // New method should be much faster
        // if (n is DecimalNode) return n;
        // return new DecimalNode(null, n.AsDecimal());

        switch (n.NodeType)
        {
            case NodeType.Blank:
            case NodeType.GraphLiteral:
            case NodeType.Uri:
                throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:decimal");

            case NodeType.Literal:
                if (n is DecimalNode) return n;
                // See if the value can be cast
                var lit = (ILiteralNode)n;
                if (lit.DataType != null)
                {
                    var dt = lit.DataType.ToString();
                    switch (dt)
                    {
                        case XmlSpecsHelper.XmlSchemaDataTypeBoolean when lit.AsValuedNode() is BooleanNode booleanNode:
                            return new DecimalNode(booleanNode.AsBoolean() ? 1.0m : 0.0m);
                        case XmlSpecsHelper.XmlSchemaDataTypeFloat when lit.AsValuedNode() is FloatNode floatNode:
                            return new DecimalNode(floatNode.AsDecimal());
                        case XmlSpecsHelper.XmlSchemaDataTypeFloat:
                            throw new RdfQueryException("Invalid lexical form for xsd:float");
                        case XmlSpecsHelper.XmlSchemaDataTypeDouble when lit.AsValuedNode() is DoubleNode doubleNode:
                            return new DecimalNode(doubleNode.AsDecimal());
                        case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                            // DateTime cast forbidden
                            throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:decimal");
                        default:
                            {
                                if (decimal.TryParse(lit.Value, NumberStyles.Any ^ NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var d))
                                {
                                    // Parsed OK
                                    return new DecimalNode(d);
                                }

                                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:decimal");
                            }
                    }
                }

                if (decimal.TryParse(lit.Value, NumberStyles.Any ^ NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var decimalValue))
                {
                    // Parsed OK
                    return new DecimalNode(decimalValue);
                }

                throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:decimal");
            default:
                throw new RdfQueryException("Cannot cast an Unknown Node to a xsd:decimal");
        }
    }

    public virtual IValuedNode ProcessDoubleCast(DoubleCast cast, TContext context, TBinding binding)
    {
        IValuedNode n = cast.InnerExpression.Accept(this, context, binding);//.CoerceToDouble();

        if (n == null)
        {
            throw new RdfQueryException("Cannot cast a Null to a xsd:double");
        }

        // New method should be much faster
        // if (n is DoubleNode) return n;
        // return new DoubleNode(null, n.AsDouble());

        switch (n.NodeType)
        {
            case NodeType.Blank:
            case NodeType.GraphLiteral:
            case NodeType.Uri:
                throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:double");

            case NodeType.Literal:
                if (n is DoubleNode) return n;
                if (n is FloatNode) return new DoubleNode(n.AsDouble());
                if (n is DecimalNode) return new DoubleNode(n.AsDouble());
                if (n is BooleanNode) return new DoubleNode(n.AsBoolean() ? 1.0E0 : 0.0E0);
                // See if the value can be cast
                var lit = (ILiteralNode)n;
                if (lit.DataType != null)
                {
                    var dt = lit.DataType.ToString();
                    if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDouble) || dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
                    {
                        if (double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                        {
                            // Parsed OK
                            return new DoubleNode(d);
                        }
                        else
                        {
                            throw new RdfQueryException("Invalid lexical form for xsd:double");
                        }
                    }
                    else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                    {
                        // DateTime cast forbidden
                        throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:double");
                    }
                    else
                    {
                        if (double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                        {
                            // Parsed OK
                            return new DoubleNode(d);
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:double");
                        }
                    }
                }
                else
                {
                    if (double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    {
                        // Parsed OK
                        return new DoubleNode(d);
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

    public virtual IValuedNode ProcessFloatCast(FloatCast cast, TContext context, TBinding binding)
    {
        IValuedNode n = cast.InnerExpression.Accept(this, context, binding);//.CoerceToFloat();

        if (n == null)
        {
            throw new RdfQueryException("Cannot cast a Null to a xsd:float");
        }

        // New method should be much faster
        // if (n is FloatNode) return n;
        // return new FloatNode(null, n.AsFloat());

        switch (n.NodeType)
        {
            case NodeType.Blank:
            case NodeType.GraphLiteral:
            case NodeType.Uri:
                throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:float");

            case NodeType.Literal:
                if (n is FloatNode) return n;
                if (n is DoubleNode) return new FloatNode(n.AsFloat());
                if (n is DecimalNode) return new FloatNode(n.AsFloat());
                if (n is BooleanNode) return new FloatNode(n.AsBoolean() ? 1.0f : 0.0f);
                // See if the value can be cast
                var lit = (ILiteralNode)n;
                if (lit.DataType != null)
                {
                    if (lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeFloat))
                    {
                        if (float.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                        {
                            // Parsed OK
                            return new FloatNode(f);
                        }
                        else
                        {
                            throw new RdfQueryException("Invalid lexical form for a xsd:float");
                        }
                    }
                    else if (lit.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                    {
                        // DateTime cast forbidden
                        throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:float");
                    }
                    else
                    {
                        if (float.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                        {
                            // Parsed OK
                            return new FloatNode(f);
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:float");
                        }
                    }
                }
                else
                {
                    if (float.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                    {
                        // Parsed OK
                        return new FloatNode(f);
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

    public virtual IValuedNode ProcessIntegerCast(IntegerCast cast, TContext context, TBinding binding)
    {
        IValuedNode n = cast.InnerExpression.Accept(this, context, binding);//.CoerceToInteger();

        if (n == null)
        {
            throw new RdfQueryException("Cannot cast a Null to a xsd:integer");
        }

        ////New method should be much faster
        // if (n is LongNode) return n;
        // return new LongNode(null, n.AsInteger());

        switch (n.NodeType)
        {
            case NodeType.Blank:
            case NodeType.GraphLiteral:
            case NodeType.Uri:
                throw new RdfQueryException("Cannot cast a Blank/URI/Graph Literal Node to a xsd:integer");

            case NodeType.Literal:
                // See if the value can be cast
                if (n is LongNode) return n;
                if (n is DecimalNode || n is DoubleNode || n is FloatNode) return new LongNode(n.AsInteger());
                if (n is BooleanNode) return new LongNode(n.AsBoolean() ? 1 : 0);
                
                var lit = (ILiteralNode)n;
                if (lit.DataType != null)
                {
                    var dt = lit.DataType.AbsoluteUri;
                    if (NumericTypesHelper.IntegerDataTypes.Contains(dt))
                    {
                        // Already a integer type so valid as a xsd:integer
                        if (long.TryParse(lit.Value, out var i))
                        {
                            return new LongNode(i);
                        }
                        else
                        {
                            throw new RdfQueryException("Invalid lexical form for xsd:integer");
                        }
                    }
                    else if (dt.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime))
                    {
                        // DateTime cast forbidden
                        throw new RdfQueryException("Cannot cast a xsd:dateTime to a xsd:integer");
                    }
                    else
                    {
                        if (long.TryParse(lit.Value, out var i))
                        {
                            // Parsed OK
                            return new LongNode(i);
                        }
                        else
                        {
                            throw new RdfQueryException("Cannot cast the value '" + lit.Value + "' to a xsd:integer");
                        }
                    }
                }
                else
                {
                    if (long.TryParse(lit.Value, out var i))
                    {
                        // Parsed OK
                        return new LongNode(i);
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

    public virtual IValuedNode ProcessStringCast(StringCast cast, TContext context, TBinding binding)
    {
        IValuedNode n = cast.InnerExpression.Accept(this, context, binding);

        if (n == null)
        {
            throw new RdfQueryException("Cannot cast a Null to a xsd:string");
        }

        return new StringNode(n.AsString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
    }

    private IValuedNode ProcessUnaryDateTimeFunction(BaseUnaryDateTimeFunction expr, TContext context,
        TBinding binding, Func<DateTimeOffset, IValuedNode> valueFunction)
    {
        IValuedNode temp = expr.InnerExpression.Accept(this, context, binding);
        if (temp == null)
        {
            throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a null argument");
        }

        return valueFunction(temp.AsDateTimeOffset());
    }

    public virtual IValuedNode ProcessDayFromDateTimeFunction(DayFromDateTimeFunction day, TContext context, TBinding binding)
    {
        return ProcessUnaryDateTimeFunction(day, context, binding, dt => new LongNode(Convert.ToInt64(dt.Day)));
    }

    public virtual IValuedNode ProcessHoursFromDateTimeFunction(HoursFromDateTimeFunction hours, TContext context, TBinding binding)
    {
        return ProcessUnaryDateTimeFunction(hours, context, binding,
            dateTime => new LongNode(Convert.ToInt64(dateTime.Hour)));
    }

    public virtual IValuedNode ProcessMinutesFromDateTimeFunction(MinutesFromDateTimeFunction minutes, TContext context, TBinding binding)
    {
        return ProcessUnaryDateTimeFunction(minutes, context, binding,
            dateTime => new LongNode(Convert.ToInt64(dateTime.Minute)));
    }

    public virtual IValuedNode ProcessMonthFromDateTimeFunction(MonthFromDateTimeFunction month, TContext context, TBinding binding)
    {
        return ProcessUnaryDateTimeFunction(month, context, binding,
            dateTime => new LongNode(Convert.ToInt64(dateTime.Month)));
    }

    public virtual IValuedNode ProcessSecondsFromDateTimeFunction(SecondsFromDateTimeFunction seconds, TContext context, TBinding binding)
    {
        return ProcessUnaryDateTimeFunction(seconds, context, binding,
            dateTime => new DecimalNode(Convert.ToDecimal(dateTime.Second) + (dateTime.Millisecond / 1000m)));
    }

    public virtual IValuedNode ProcessTimezoneFromDateTimeFunction(TimezoneFromDateTimeFunction timezone, TContext context,
        TBinding binding)
    {
        IValuedNode temp = timezone.InnerExpression.Accept(this, context, binding);
        if (temp == null)
        {
            throw new RdfQueryException("Unable to evaluate an XPath Date Time function on a null argument");
        }

        DateTimeOffset dt = temp.AsDateTimeOffset();
        // Regex based check to see if the value has a Timezone component
        // If not then the result is a null
        if (!Regex.IsMatch(temp.AsString(), "(Z|[+-]\\d{2}:\\d{2})$")) return null;

        // Now we have a DateTime we can try and return the Timezone
        if (dt.Offset.Equals(TimeSpan.Zero))
        {
            // If Zero it was specified as Z (which means UTC so zero offset)
            return new StringNode("PT0S", UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));
        }

        // If the Offset is outside the range -14 to 14 this is considered invalid
        if (dt.Offset.Hours < -14 || dt.Offset.Hours > 14) return null;

        // Otherwise it has an offset which is a given number of hours and minutse
        var offset = "PT" + Math.Abs(dt.Offset.Hours) + "H";
        if (dt.Offset.Hours < 0) offset = "-" + offset;
        if (dt.Offset.Minutes != 0) offset = offset + Math.Abs(dt.Offset.Minutes) + "M";
        if (dt.Offset.Hours == 0 && dt.Offset.Minutes < 0) offset = "-" + offset;

        return new StringNode(offset, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration));

    }

    public virtual IValuedNode ProcessYearsFromDateTimeFunction(YearFromDateTimeFunction years, TContext context, TBinding binding)
    {
        return ProcessUnaryDateTimeFunction(years, context, binding,
            dateTime => new LongNode(Convert.ToInt64(dateTime.Year)));
    }

    public virtual IValuedNode ProcessAbsFunction(XPath.Numeric.AbsFunction abs, TContext context, TBinding binding)
    {
        IValuedNode a = abs.InnerExpression.Accept(this, context, binding);
        if (a == null) throw new RdfQueryException("Cannot calculate an arithmetic expression on a null");

        switch (a.NumericType)
        {
            case SparqlNumericType.Integer:
                return new LongNode(Math.Abs(a.AsInteger()));

            case SparqlNumericType.Decimal:
                return new DecimalNode(Math.Abs(a.AsDecimal()));

            case SparqlNumericType.Float:
                try
                {
                    return new FloatNode(Convert.ToSingle(Math.Abs(a.AsDouble())));
                }
                catch (RdfQueryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new RdfQueryException("Unable to cast absolute value of float to a float", ex);
                }

            case SparqlNumericType.Double:
                return new DoubleNode(Math.Abs(a.AsDouble()));

            default:
                throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
        }
    }

    public virtual IValuedNode ProcessCeilFunction(CeilingFunction ceil, TContext context, TBinding binding)
    {
        IValuedNode a = ceil.InnerExpression.Accept(this, context, binding);
        if (a == null) throw new RdfQueryException("Cannot calculate an arithmetic expression on a null");

        switch (a.NumericType)
        {
            case SparqlNumericType.Integer:
                try
                {
                    return new LongNode(Convert.ToInt64(Math.Ceiling(a.AsDecimal())));
                }
                catch (RdfQueryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new RdfQueryException("Unable to cast ceiling value of integer to an integer", ex);
                }

            case SparqlNumericType.Decimal:
                return new DecimalNode(Math.Ceiling(a.AsDecimal()));

            case SparqlNumericType.Float:
                try
                {
                    return new FloatNode(Convert.ToSingle(Math.Ceiling(a.AsDouble())));
                }
                catch (RdfQueryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new RdfQueryException("Unable to cast ceiling value of float to a float", ex);
                }

            case SparqlNumericType.Double:
                return new DoubleNode(Math.Ceiling(a.AsDouble()));

            default:
                throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
        }
    }

    public virtual IValuedNode ProcessFloorFunction(XPath.Numeric.FloorFunction floor, TContext context, TBinding binding)
    {
        IValuedNode a = floor.InnerExpression.Accept(this, context, binding);
        if (a == null) throw new RdfQueryException("Cannot calculate an arithmetic expression on a null");

        switch (a.NumericType)
        {
            case SparqlNumericType.Integer:
                try
                {
                    return new LongNode(Convert.ToInt64(Math.Floor(a.AsDecimal())));
                }
                catch (RdfQueryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new RdfQueryException("Unable to cast floor value of integer to an integer", ex);
                }

            case SparqlNumericType.Decimal:
                return new DecimalNode(Math.Floor(a.AsDecimal()));

            case SparqlNumericType.Float:
                try
                {
                    return new FloatNode(Convert.ToSingle(Math.Floor(a.AsDouble())));
                }
                catch (RdfQueryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new RdfQueryException("Unable to cast floor value of float to a float", ex);
                }

            case SparqlNumericType.Double:
                return new DoubleNode(Math.Floor(a.AsDouble()));

            default:
                throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
        }
    }

    public virtual IValuedNode ProcessRoundFunction(Expressions.Functions.XPath.Numeric.RoundFunction round, TContext context, TBinding binding)
    {
        IValuedNode a = round.InnerExpression.Accept(this, context, binding);
        if (a == null) throw new RdfQueryException("Cannot calculate an arithmetic expression on a null");

        switch (a.NumericType)
        {
            case SparqlNumericType.Integer:
                // Rounding an Integer has no effect
                return a;

            case SparqlNumericType.Decimal:
                return new DecimalNode(Math.Round(a.AsDecimal(), MidpointRounding.AwayFromZero));

            case SparqlNumericType.Float:
                try
                {
                    return new FloatNode(Convert.ToSingle(Math.Round(a.AsDouble(), MidpointRounding.AwayFromZero), CultureInfo.InvariantCulture));
                }
                catch (RdfQueryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new RdfQueryException("Unable to cast the float value of a round to a float", ex);
                }

            case SparqlNumericType.Double:
                return new DoubleNode(Math.Round(a.AsDouble(), MidpointRounding.AwayFromZero));

            default:
                throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
        }
    }

    public virtual IValuedNode ProcessRoundHalfToEvenFunction(RoundHalfToEvenFunction round, TContext context, TBinding binding)
    {
        IValuedNode a = round.InnerExpression.Accept(this, context, binding);
        if (a == null) throw new RdfQueryException("Cannot calculate an arithmetic expression on a null");

        var p = 0;
        if (round.Precision != null)
        {
            IValuedNode precision = round.Precision.Accept(this, context, binding);
            if (precision == null) throw new RdfQueryException("Cannot use a null precision for rounding");
            try
            {
                p = Convert.ToInt32(precision.AsInteger());
            }
            catch
            {
                throw new RdfQueryException("Unable to cast precision to an integer");
            }
        }

        switch (a.NumericType)
        {
            case SparqlNumericType.Integer:
                // Rounding an Integer has no effect
                return a;

            case SparqlNumericType.Decimal:
                return new DecimalNode(Math.Round(a.AsDecimal(), p, MidpointRounding.AwayFromZero));

            case SparqlNumericType.Float:
                try
                {
                    return new FloatNode(Convert.ToSingle(Math.Round(a.AsDouble(), p, MidpointRounding.AwayFromZero)));
                }
                catch (RdfQueryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new RdfQueryException("Unable to cast the float value of a round to a float", ex);
                }

            case SparqlNumericType.Double:
                return new DoubleNode(Math.Round(a.AsDouble(), p, MidpointRounding.AwayFromZero));

            default:
                throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
        }

    }

    public virtual IValuedNode ProcessCompareFunction(CompareFunction compare, TContext context, TBinding binding)
    {
        return ProcessBinaryStringFunction(compare, context, binding,
            (x, y) => new LongNode(String.CompareOrdinal(x.Value, y.Value)));
    }

    public virtual IValuedNode ProcessConcatFunction(Expressions.Functions.XPath.String.ConcatFunction concat, TContext context, TBinding binding)
    {
        var output = new StringBuilder();
        foreach (ISparqlExpression expr in concat.Expressions)
        {
            IValuedNode temp = expr.Accept(this, context, binding);
            if (temp == null) throw new RdfQueryException("Cannot evaluate the XPath concat() function when an argument evaluates to a Null");
            switch (temp.NodeType)
            {
                case NodeType.Literal:
                    output.Append(temp.AsString());
                    break;
                default:
                    throw new RdfQueryException("Cannot evaluate the XPath concat() function when an argument is not a Literal Node");
            }
        }

        return new StringNode(output.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
    }

    public virtual IValuedNode ProcessContainsFunction(Expressions.Functions.XPath.String.ContainsFunction contains, TContext context, TBinding binding)
    {
        return ProcessBinaryStringFunction(contains, context, binding, (stringLit, arg) =>
        {
            if (stringLit.Value.Equals(string.Empty))
            {
                // Empty string cannot contain anything
                return new BooleanNode(false);
            }
            else if (arg.Value.Equals(string.Empty))
            {
                // Any non-empty string contains the empty string
                return new BooleanNode(true);
            }
            else
            {
                // Evaluate the Contains
                return new BooleanNode(stringLit.Value.Contains(arg.Value));
            }
        });
    }

    public virtual IValuedNode ProcessEncodeForUriFunction(Expressions.Functions.XPath.String.EncodeForUriFunction encodeForUri, TContext context, TBinding binding)
    {
        return ProcessUnaryStringFunction(encodeForUri, context, binding,
            stringLit => new StringNode(Uri.EscapeUriString(stringLit.Value),
                UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)));
    }

    public virtual IValuedNode ProcessEndsWithFunction(EndsWithFunction endsWith, TContext context, TBinding binding)
    {
        return ProcessBinaryStringFunction(endsWith, context, binding, (stringLit, arg) =>
        {
            if (stringLit.Value.Equals(string.Empty))
            {
                if (arg.Value.Equals(string.Empty))
                {
                    // The Empty String ends with the Empty String
                    return new BooleanNode(true);
                }
                else
                {
                    // Empty String doesn't end with a non-empty string
                    return new BooleanNode(false);
                }
            }
            else if (arg.Value.Equals(string.Empty))
            {
                // Any non-empty string ends with the empty string
                return new BooleanNode(true);
            }
            else
            {
                // Otherwise evaluate the EndsWith
                return new BooleanNode(stringLit.Value.EndsWith(arg.Value));
            }
        });
    }

    public virtual IValuedNode ProcessEscapeHtmlUriFunction(EscapeHtmlUriFunction escape, TContext context, TBinding binding)
    {
        return ProcessUnaryStringFunction(escape, context, binding,
            stringLit => new StringNode(
                //HttpUtility.UrlEncode(stringLit.Value),
                Uri.EscapeDataString(stringLit.Value),
                UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)));
    }

    public virtual IValuedNode ProcessLowerCaseFunction(LowerCaseFunction lCase, TContext context, TBinding binding)
    {
        return ProcessUnaryStringFunction(lCase, context, binding,
            x => new StringNode(x.Value.ToLowerInvariant(),
                UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)));
    }

    public virtual IValuedNode ProcessNormalizeSpaceFunction(NormalizeSpaceFunction normalize, TContext context,
        TBinding binding)
    {
        return ProcessUnaryStringFunction(normalize, context, binding, stringLit =>
        {
            var temp = stringLit.Value.Trim();
            var normalizeSpace = new Regex("\\s{2,}");
            temp = normalizeSpace.Replace(temp, " ");

            return new StringNode(temp, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        });
    }

    public virtual IValuedNode ProcessNormalizeUnicodeFunction(NormalizeUnicodeFunction normalize, TContext context, TBinding binding)
    {
        return ProcessBinaryStringFunction(normalize, context, binding,
            (stringLit, arg) =>
            {
                if (arg == null)
                {
                    return new StringNode(stringLit.Value.Normalize(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                var normalized = stringLit.Value;

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
                        // No Normalization
                        break;
                    default:
                        throw new RdfQueryException("'" + arg.Value + "' is not a valid Normalization Form as defined by the XPath specification");
                }

                return new StringNode(normalized, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            });
    }

    public virtual IValuedNode ProcessReplaceFunction(Expressions.Functions.XPath.String.ReplaceFunction replace, TContext context, TBinding binding)
    {
        RegexOptions options = replace.FixedOptions
            ? replace.Options
            : replace.GetOptions(replace.OptionsExpression.Accept(this, context, binding), true);
        string findPattern, replacePattern;
        if (replace.FixedFindPattern)
        {
            findPattern = replace.FindPattern;
        }
        else
        {
            // Regex is not pre-compiled
            if (replace.FindExpression != null)
            {
                IValuedNode p = replace.FindExpression.Accept(this, context, binding);
                if (p != null)
                {
                    if (p.NodeType == NodeType.Literal)
                    {
                        findPattern = p.AsString();
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

        if (replace.FixedReplacePattern)
        {
            replacePattern = replace.ReplacePattern;
        }
        else
        {
            if (replace.ReplaceExpression != null)
            {
                IValuedNode r = replace.ReplaceExpression.Accept(this, context, binding);
                if (r != null)
                {
                    if (r.NodeType == NodeType.Literal)
                    {
                        replacePattern = r.AsString();
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

        IValuedNode textNode = replace.TestExpression.Accept(this, context, binding);
        if (textNode == null)
        {
            throw new RdfQueryException("Cannot evaluate a Regular Expression against a NULL");
        }

        if (textNode.NodeType != NodeType.Literal)
        {
            throw new RdfQueryException("Cannot evaluate a Regular Expression against a non-Literal Node");
        }

        // Execute
        var text = textNode.AsString();
        var output = Regex.Replace(text, findPattern, replacePattern, options);
        return new StringNode(output, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
    }

    public virtual IValuedNode ProcessStartsWithFunction(StartsWithFunction startsWith, TContext context, TBinding binding)
    {
        return this.ProcessBinaryStringFunction(startsWith, context, binding, (stringLit, arg) =>
        {
            if (stringLit.Value.Equals(string.Empty))
            {
                if (arg.Value.Equals(string.Empty))
                {
                    // The Empty String starts with the Empty String
                    return new BooleanNode(true);
                }
                else
                {
                    // Empty String doesn't start with a non-empty string
                    return new BooleanNode(false);
                }
            }
            else if (arg.Value.Equals(string.Empty))
            {
                // Any non-empty string starts with the empty string
                return new BooleanNode(true);
            }
            else
            {
                // Otherwise evalute the StartsWith
                return new BooleanNode(stringLit.Value.StartsWith(arg.Value));
            }
        });
    }

    public virtual IValuedNode ProcessStringLengthFunction(StringLengthFunction strLen, TContext context, TBinding binding)
    {
        return ProcessUnaryStringFunction(strLen, context, binding,
            stringLit => new LongNode(stringLit.Value.Length));
    }

    public virtual IValuedNode ProcessSubstringAfterFunction(SubstringAfterFunction substringAfter, TContext context, TBinding binding)
    {
        return ProcessBinaryStringFunction(substringAfter, context, binding, (stringLit, arg) =>
        {
            if (arg.Value.Equals(string.Empty))
            {
                // The substring after the empty string is the input string
                return new StringNode(stringLit.Value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            else
            {
                // Does the String contain the search string?
                if (stringLit.Value.Contains(arg.Value))
                {
                    var result = stringLit.Value.Substring(stringLit.Value.IndexOf(arg.Value) + arg.Value.Length);
                    return new StringNode(result, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                else
                {
                    // If it doesn't contain the search string the empty string is returned
                    return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
            }
        });
    }

    public virtual IValuedNode ProcessSubstringBeforeFunction(SubstringBeforeFunction substringBefore, TContext context,
        TBinding binding)
    {
        return ProcessBinaryStringFunction(substringBefore, context, binding, (stringLit, arg) =>
        {
            if (arg.Value.Equals(string.Empty))
            {
                // The substring before the empty string is the empty string
                return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            else
            {
                // Does the String contain the search string?
                if (stringLit.Value.Contains(arg.Value))
                {
                    var result = stringLit.Value.Substring(0, stringLit.Value.IndexOf(arg.Value));
                    return new StringNode(result, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                else
                {
                    // If it doesn't contain the search string the empty string is returned
                    return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
            }
        });
    }

    public virtual IValuedNode ProcessSubstringFunction(Expressions.Functions.XPath.String.SubstringFunction substring, TContext context, TBinding binding)
    {
        var input = (ILiteralNode)CheckArgument(substring.InnerExpression, context, binding, XPathFunctionFactory.AcceptStringArguments);
        IValuedNode start = CheckArgument(substring.StartExpression, context, binding, XPathFunctionFactory.AcceptNumericArguments);

        if (substring.LengthExpression != null)
        {
            IValuedNode length = CheckArgument(substring.LengthExpression, context, binding, XPathFunctionFactory.AcceptNumericArguments);

            if (input.Value.Equals(string.Empty)) return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

            var s = Convert.ToInt32(start.AsInteger());
            var l = Convert.ToInt32(length.AsInteger());

            if (s < 1) s = 1;
            if (l < 1)
            {
                // If no/negative characters are being selected the empty string is returned
                return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            else if (s - 1 > input.Value.Length)
            {
                // If the start is after the end of the string the empty string is returned
                return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            }
            else
            {
                if (s - 1 + l > input.Value.Length)
                {
                    // If the start plus the length is greater than the length of the string the string from the starts onwards is returned
                    return new StringNode(input.Value.Substring(s - 1), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
                else
                {
                    // Otherwise do normal substring
                    return new StringNode(input.Value.Substring(s - 1, l), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
                }
            }
        }
        else
        {
            if (input.Value.Equals(string.Empty)) return new StringNode(string.Empty, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

            var s = Convert.ToInt32(start.AsInteger());
            if (s < 1) s = 1;

            return new StringNode(input.Value.Substring(s - 1), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
        }
    }

    public virtual IValuedNode ProcessUpperCaseFunction(UpperCaseFunction uCase, TContext context, TBinding binding)
    {
        return ProcessUnaryStringFunction(uCase, context, binding,
            (stringLit) => new StringNode(stringLit.Value.ToUpper(),
                UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString)));
    }

    public virtual IValuedNode ProcessBooleanFunction(BooleanFunction boolean, TContext context, TBinding binding)
    {
        return new BooleanNode(boolean.InnerExpression.Accept(this, context, binding).AsSafeBoolean());
    }

    public virtual IValuedNode ProcessUnknownFunction(UnknownFunction unknownFunction, TContext context, TBinding binding)
    {
        return null;
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