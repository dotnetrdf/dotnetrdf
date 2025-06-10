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
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Aggregates.Leviathan;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Aggregates.XPath;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query;

internal class LeviathanAggregateProcessor : ISparqlAggregateProcessor<IValuedNode, SparqlEvaluationContext, int>
{
    private readonly ISparqlExpressionProcessor<IValuedNode, SparqlEvaluationContext, int> _expressionProcessor;
    public LeviathanAggregateProcessor(ISparqlExpressionProcessor<IValuedNode, SparqlEvaluationContext, int> expressionProcessor)
    {
        _expressionProcessor = expressionProcessor ?? throw new ArgumentNullException(nameof(expressionProcessor));
    }

    public IValuedNode ProcessAverage(AverageAggregate average, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        //if (average.Variable != null) AssertVariable(average.Variable, average, context);

        // Prep Variables
        var values = new HashSet<IValuedNode>();
        var count = 0;
        // long lngtotal = 0;
        var dectotal = 0.0m;
        var flttotal = 0.0f;
        var dbltotal = 0.0d;
        SparqlNumericType maxtype = SparqlNumericType.NaN;
        SparqlNumericType numtype;

        foreach (var id in bindings)
        {
            IValuedNode temp;
            try
            {
                temp = average.Expression.Accept(_expressionProcessor, context, id);
                if (temp == null) return null;
                // Apply DISTINCT modifier if required
                if (average.Distinct)
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
                numtype = temp.NumericType;
            }
            catch
            {
                // SPARQL Working Group changed spec so this should now return no binding
                return null;
            }

            // No result if anything resolves to non-numeric
            if (numtype == SparqlNumericType.NaN) return null;

            // Track the Numeric Type
            if ((int)numtype > (int)maxtype)
            {
                maxtype = numtype;
            }

            // Increment the Totals based on the current Numeric Type
            switch (maxtype)
            {
                case SparqlNumericType.Integer:
                    // lngtotal += numExpr.IntegerValue(context, id);
                    dectotal += temp.AsDecimal();
                    flttotal += temp.AsFloat();
                    dbltotal += temp.AsDouble();
                    break;
                case SparqlNumericType.Decimal:
                    dectotal += temp.AsDecimal();
                    flttotal += temp.AsFloat();
                    dbltotal += temp.AsDouble();
                    break;
                case SparqlNumericType.Float:
                    flttotal += temp.AsFloat();
                    dbltotal += temp.AsDouble();
                    break;
                case SparqlNumericType.Double:
                    dbltotal += temp.AsDouble();
                    break;
            }

            count++;
        }

        // Calculate the Average
        if (count == 0)
        {
            return new LongNode(0);
        }
        else
        {
            // long lngavg;
            decimal decavg;
            float fltavg;
            double dblavg;

            switch (maxtype)
            {
                case SparqlNumericType.Integer:
                ////Integer Values
                // lngavg = lngtotal / (long)count;
                // return new LiteralNode(null, lngavg.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

                case SparqlNumericType.Decimal:
                    // Decimal Values
                    decavg = dectotal / (decimal)count;
                    return new DecimalNode(decavg);

                case SparqlNumericType.Float:
                    // Float values
                    fltavg = flttotal / (float)count;
                    return new FloatNode(fltavg);

                case SparqlNumericType.Double:
                    // Double Values
                    dblavg = dbltotal / (double)count;
                    return new DoubleNode(dblavg);

                default:
                    throw new RdfQueryException("Failed to calculate a valid Average");
            }
        }
    }

    public IValuedNode ProcessCount(CountAggregate count, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        var c = 0;
        if (count.Variable != null)
        {
            // Just Count the number of results where the variable is bound
            c = bindings.Count(id => count.Expression.Accept(_expressionProcessor, context, id) != null);
        }
        else
        {
            // Count the number of results where the result in not null/error
            foreach (var id in bindings)
            {
                try
                {
                    if (count.Expression.Accept(_expressionProcessor, context, id) != null) c++;
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        return new LongNode(c);
    }

    public IValuedNode ProcessCountAll(CountAllAggregate countAll, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        return new LongNode(bindings.Count());
    }

    public IValuedNode ProcessCountAllDistinct(CountAllDistinctAggregate countAllDistinct, SparqlEvaluationContext context,
        IEnumerable<int> bindings)
    {
        return new LongNode(bindings.Select(id=>context.InputMultiset[id]).Distinct().Count());
    }

    public IValuedNode ProcessCountDistinct(CountDistinctAggregate countDistinct, SparqlEvaluationContext context,
        IEnumerable<int> bindings)
    {
        int c;
        var values = new HashSet<IValuedNode>();

        if (countDistinct.Variable != null)
        {
            AssertVariable(countDistinct.Variable, countDistinct, context);

            // Just Count the number of results where the variable is bound
            var varExpr = (VariableTerm)countDistinct.Expression;
            foreach (IValuedNode value in bindings
                .Select(id => countDistinct.Expression.Accept(_expressionProcessor, context, id))
                .Where(x => x != null))
            {
                values.Add(value);
            }
            c = values.Count();
        }
        else
        {
            // Count the distinct non-null results
            foreach (var id in bindings)
            {
                try
                {
                    IValuedNode temp = countDistinct.Expression.Accept(_expressionProcessor, context, id);
                    if (temp != null)
                    {
                        values.Add(temp);
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }
            c = values.Count;
        }
        return new LongNode(c);
    }

    public IValuedNode ProcessGroupConcat(GroupConcatAggregate groupConcat, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        IValuedNode n = ProcessConcat(
            (id)=>GroupConcatValue(groupConcat.Expression, context, id),
            (id)=>StringJoinSeparator(groupConcat.SeparatorExpression, context, id),
            context, bindings, groupConcat.Distinct);
        return new StringNode(n.AsString());
    }

    private string GroupConcatValue(ISparqlExpression expression, SparqlEvaluationContext context, int bindingId)
    {
        IValuedNode temp = expression.Accept(_expressionProcessor, context, bindingId);
        if (temp == null) throw new RdfQueryException("Cannot do an XPath string-join on a null");
        switch (temp.NodeType)
        {
            case NodeType.Literal:
            case NodeType.Uri:
                return temp.AsString();
            default:
                throw new RdfQueryException("Cannot do an XPath string-join on a non-Literal Node");
        }
    }

    public IValuedNode ProcessMax(MaxAggregate max, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {

        if (max.Variable != null)
        {
            AssertVariable(max.Variable, max, context);
        }

        // TODO: Could this just be replaced with a running max value without the need for the intermediate list?
        var values = new List<IValuedNode>();
        foreach (var id in bindings)
        {
            try
            {
                values.Add(max.Expression.Accept(_expressionProcessor, context, id));
            }
            catch
            {
                // Ignore errors
            }
        }

        values.Sort(new SparqlOrderingComparer(context.NodeComparer.Culture, context.NodeComparer.Options));
        values.Reverse();
        return values.FirstOrDefault();
    }

    public IValuedNode ProcessMin(MinAggregate min, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        if (min.Variable != null)
        {
            AssertVariable(min.Variable, min, context);
        }

        // TODO: This could also be reworked to use a single temp var instead of the list
        var values = new List<IValuedNode>();
        foreach (var id in bindings)
        {
            try
            {
                values.Add( min.Expression.Accept(_expressionProcessor, context, id));
            }
            catch
            {
                // Ignore errors
            }
        }

        values.Sort(new SparqlOrderingComparer(context.NodeComparer.Culture, context.NodeComparer.Options));
        return values.FirstOrDefault();
    }

    public IValuedNode ProcessSample(SampleAggregate sample, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        // Try the expression with each member of the Group until we find a non-null
        foreach (var id in bindings)
        {
            try
            {

                // First non-null result we find is returned
                IValuedNode temp = sample.Expression.Accept(_expressionProcessor, context, id);
                if (temp != null) return temp;
            }
            catch (RdfQueryException)
            {
                // Ignore errors - we'll loop round and try the next
            }
        }

        // If the Group is Empty of the Expression fails to evaluate for the entire Group then the result is null
        return null;
    }

    public IValuedNode ProcessSum(SumAggregate sum, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        // Prep Variables
        long lngtotal = 0;
        var dectotal = 0.0m;
        var flttotal = 0.0f;
        var dbltotal = 0.0d;
        SparqlNumericType maxtype = SparqlNumericType.NaN;
        SparqlNumericType numtype;
        var values = new HashSet<IValuedNode>();

        foreach (var id in bindings)
        {
            IValuedNode temp;
            try
            {
                temp = sum.Expression.Accept(_expressionProcessor, context, id);
                if (sum.Distinct)
                {

                    if (temp == null) continue;
                    if (values.Contains(temp))
                    {
                        continue;
                    }
                    else
                    {
                        values.Add(temp);
                    }
                }
                numtype = temp.NumericType;
            }
            catch
            {
                continue;
            }

            // Skip if Not a Number
            if (numtype == SparqlNumericType.NaN) continue;

            // Track the Numeric Type
            if ((int)numtype > (int)maxtype)
            {
                maxtype = numtype;
            }

            // Increment the Totals based on the current Numeric Type
            switch (maxtype)
            {
                case SparqlNumericType.Integer:
                    lngtotal += temp.AsInteger();
                    dectotal += temp.AsDecimal();
                    flttotal += temp.AsFloat();
                    dbltotal += temp.AsDouble();
                    break;
                case SparqlNumericType.Decimal:
                    dectotal += temp.AsDecimal();
                    flttotal += temp.AsFloat();
                    dbltotal += temp.AsDouble();
                    break;
                case SparqlNumericType.Float:
                    flttotal += temp.AsFloat();
                    dbltotal += temp.AsDouble();
                    break;
                case SparqlNumericType.Double:
                    dbltotal += temp.AsDouble();
                    break;
            }
        }

        // Return the Sum
        switch (maxtype)
        {
            case SparqlNumericType.NaN:
                // No Numeric Values
                return new LongNode(0);

            case SparqlNumericType.Integer:
                // Integer Values
                return new LongNode(lngtotal);

            case SparqlNumericType.Decimal:
                // Decimal Values
                return new DecimalNode(dectotal);

            case SparqlNumericType.Float:
                // Float Values
                return new FloatNode(flttotal);

            case SparqlNumericType.Double:
                // Double Values
                return new DoubleNode(dbltotal);

            default:
                throw new RdfQueryException("Failed to calculate a valid Sum");
        }
    }

    public IValuedNode ProcessStringJoin(StringJoinAggregate stringJoin, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        return ProcessConcat(
            (bindingId) => StringJoinValue(stringJoin.Expression, context, bindingId), 
            bindingId => StringJoinSeparator(stringJoin.SeparatorExpression, context, bindingId),
            context, bindings, stringJoin.Distinct);
    }

    private IValuedNode ProcessConcat(
        Func<int, string> valueFunc,
        Func<int, string> sepFunc,
        SparqlEvaluationContext context, IEnumerable<int> bindings, bool distinct)
    {
        var ids = bindings.ToList();
        var output = new StringBuilder();
        var values = new HashSet<string>();
        for (var i = 0; i < ids.Count; i++)
        {
            try
            {
                var temp = valueFunc(ids[i]);

                // Apply DISTINCT modifer if required
                if (distinct)
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
                output.Append(string.Empty);
            }

            // Append Separator if required
            if (i < ids.Count - 1)
            {
                var sep = sepFunc(ids[i]);
                output.Append(sep);
            }
        }

        return new StringNode(output.ToString(), context.UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

    }

    private string StringJoinValue(ISparqlExpression expression, SparqlEvaluationContext context, int bindingId)
    {
        IValuedNode temp = expression.Accept(_expressionProcessor, context, bindingId);
        if (temp == null) throw new RdfQueryException("Cannot do an XPath string-join on a null");
        if (temp.NodeType == NodeType.Literal)
        {
            var l = (ILiteralNode)temp;
            if (l.DataType != null)
            {
                if (l.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
                {
                    return temp.AsString();
                }
                else
                {
                    throw new RdfQueryException("Cannot do an XPath string-join on a Literal which is not typed as a String");
                }
            }
            else
            {
                return temp.AsString();
            }
        }
        else
        {
            throw new RdfQueryException("Cannot do an XPath string-join on a non-Literal Node");
        }
    }

    private string StringJoinSeparator(ISparqlExpression expression, SparqlEvaluationContext context, int bindingId)
    {
        INode temp = expression.Accept(_expressionProcessor, context, bindingId);
        if (temp == null)
        {
            return string.Empty;
        }
        else if (temp.NodeType == NodeType.Literal)
        {
            var l = (ILiteralNode)temp;
            if (l.DataType != null)
            {
                if (l.DataType.AbsoluteUri.Equals(XmlSpecsHelper.XmlSchemaDataTypeString))
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

    public IValuedNode ProcessAll(AllAggregate all, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        foreach (var id in bindings)
        {
            try
            {
                if (!all.Expression.Accept(_expressionProcessor, context, id).AsSafeBoolean())
                {
                    // As soon as we see a false we can return false
                    return new BooleanNode(false);
                }
            }
            catch (RdfQueryException)
            {
                // An error is a failure so we return false
                return new BooleanNode(false);
            }
        }

        // If everything is true then we return true;
        return new BooleanNode(true);
    }

    public IValuedNode ProcessAny(AnyAggregate any, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        foreach (var id in bindings)
        {
            try
            {
                if (any.Expression.Accept(_expressionProcessor, context, id).AsSafeBoolean())
                {
                    // As soon as we see a True we can return true
                    return new BooleanNode(true);
                }
            }
            catch (RdfQueryException)
            {
                // Ignore errors in an any
            }
        }

        // If we don't see any Trues we return false
        return new BooleanNode(false);
    }

    public IValuedNode ProcessMedian(MedianAggregate median, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        if (median.Variable != null)
        {
            AssertVariable(median.Variable, median, context);
        }

        var values = new List<IValuedNode>();
        var distinctValues = new HashSet<IValuedNode>();
        var nullSeen = false;
        foreach (var id in bindings)
        {
            try
            {
                IValuedNode temp = median.Expression.Accept(_expressionProcessor, context, id);
                if (median.Distinct)
                {
                    if (temp != null)
                    {
                        if (distinctValues.Contains(temp))
                        {
                            continue;
                        }
                        else
                        {
                            distinctValues.Add(temp);
                        }
                    }
                    else if (!nullSeen)
                    {
                        nullSeen = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                values.Add(temp);
            }
            catch
            {
                // Ignore errors
            }
        }

        if (values.Count == 0) return null;

        // Find the middle value and return
        values.Sort();
        var skip = values.Count / 2;
        return values.Skip(skip).First();
    }

    public IValuedNode ProcessMode(ModeAggregate mode, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        if (mode.Variable != null)
        {
            AssertVariable(mode.Variable, mode, context);
        }

        var values = new Dictionary<IValuedNode, int>();
        var nullCount = 0;
        foreach (var id in bindings)
        {
            try
            {
                IValuedNode temp = mode.Expression.Accept(_expressionProcessor, context, id);
                if (temp == null)
                {
                    nullCount++;
                }
                else
                {
                    if (values.ContainsKey(temp))
                    {
                        values[temp]++;
                    }
                    else
                    {
                        values.Add(temp, 1);
                    }
                }
            }
            catch
            {
                // Errors count as nulls
                nullCount++;
            }
        }

        var mostPopular = values.Values.Max();
        if (mostPopular > nullCount)
        {
            return values.FirstOrDefault(p => p.Value == mostPopular).Key;
        }
        else
        {
            // Null is the most popular item
            return null;
        }
    }

    public IValuedNode ProcessNone(NoneAggregate none, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        foreach (var id in bindings)
        {
            try
            {
                if (none.Expression.Accept(_expressionProcessor, context, id).AsSafeBoolean())
                {
                    // As soon as we see a true we can return false
                    return new BooleanNode(false);
                }
            }
            catch (RdfQueryException)
            {
                // An error is a failure so we keep going
            }
        }

        // If everything is false then we return true;
        return new BooleanNode(true);
    }

    private static void AssertVariable(string var, ISparqlAggregate aggregate, SparqlEvaluationContext context)
    {
            // Ensured the aggregated variable is in the Variables of the Results
            if (!context.Binder.Variables.Contains(var))
            {
                throw new RdfQueryException($"Cannot use the Variable ?{var} in a {aggregate.Functor} Aggregate since the Variable does not occur in a Graph Pattern");
            }
    }

    public IValuedNode ProcessNumericMax(NumericMaxAggregate numericMax, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        if (numericMax.Variable != null)
        {
            AssertVariable(numericMax.Variable, numericMax, context);
        }

        // Prep Variables
        long lngmax = 0;
        var decmax = 0.0m;
        var fltmax = 0.0f;
        var dblmax = 0.0d;
        SparqlNumericType maxtype = SparqlNumericType.NaN;
        SparqlNumericType numtype;

        foreach (var id in bindings)
        {
            IValuedNode temp;
            try
            {
                temp = numericMax.Expression.Accept(_expressionProcessor, context, id);
                if (temp == null) continue;
                numtype = temp.NumericType;
            }
            catch
            {
                continue;
            }

            // Skip if Not a Number
            if (numtype == SparqlNumericType.NaN) continue;

            // Track the Numeric Type
            if ((int)numtype > (int)maxtype)
            {
                if (maxtype == SparqlNumericType.NaN)
                {
                    // Initialise Maximums
                    switch (numtype)
                    {
                        case SparqlNumericType.Integer:
                            lngmax = temp.AsInteger();
                            decmax = temp.AsDecimal();
                            fltmax = temp.AsFloat();
                            dblmax = temp.AsDouble();
                            break;
                        case SparqlNumericType.Decimal:
                            decmax = temp.AsDecimal();
                            fltmax = temp.AsFloat();
                            dblmax = temp.AsDouble();
                            break;
                        case SparqlNumericType.Float:
                            fltmax = temp.AsFloat();
                            dblmax = temp.AsDouble();
                            break;
                        case SparqlNumericType.Double:
                            dblmax = temp.AsDouble();
                            break;
                    }
                    maxtype = numtype;
                    continue;
                }
                else
                {
                    maxtype = numtype;
                }
            }

            long lngval;
            decimal decval;
            float fltval;
            double dblval;
            switch (maxtype)
            {
                case SparqlNumericType.Integer:
                    lngval = temp.AsInteger();

                    if (lngval > lngmax)
                    {
                        lngmax = lngval;
                        decmax = temp.AsDecimal();
                        fltmax = temp.AsFloat();
                        dblmax = temp.AsDouble();
                    }
                    break;
                case SparqlNumericType.Decimal:
                    decval = temp.AsDecimal();

                    if (decval > decmax)
                    {
                        decmax = decval;
                        fltmax = temp.AsFloat();
                        dblmax = temp.AsDouble();
                    }
                    break;
                case SparqlNumericType.Float:
                    fltval = temp.AsFloat();

                    if (fltval > fltmax)
                    {
                        fltmax = fltval;
                        dblmax = temp.AsDouble();
                    }
                    break;
                case SparqlNumericType.Double:
                    dblval = temp.AsDouble();

                    if (dblval > dblmax)
                    {
                        dblmax = dblval;
                    }
                    break;
            }
        }

        // Return the Max
        switch (maxtype)
        {
            case SparqlNumericType.NaN:
                // No Numeric Values
                return null;

            case SparqlNumericType.Integer:
                // Integer Values
                return new LongNode(lngmax);

            case SparqlNumericType.Decimal:
                // Decimal Values
                return new DecimalNode(decmax);

            case SparqlNumericType.Float:
                // Float values
                return new FloatNode(fltmax);

            case SparqlNumericType.Double:
                // Double Values
                return new DoubleNode(dblmax);

            default:
                throw new RdfQueryException("Failed to calculate a valid Maximum");
        }
    }

    public IValuedNode ProcessNumericMin(NumericMinAggregate numericMin, SparqlEvaluationContext context, IEnumerable<int> bindings)
    {
        if (numericMin.Variable != null)
        {
            AssertVariable(numericMin.Variable, numericMin, context);
        }

        // Prep Variables
        long lngmin = 0;
        var decmin = 0.0m;
        var fltmin = 0.0f;
        var dblmin = 0.0d;
        SparqlNumericType mintype = SparqlNumericType.NaN;
        SparqlNumericType numtype;

        foreach (var id in bindings)
        {
            IValuedNode temp;
            try
            {
                temp = numericMin.Expression.Accept(_expressionProcessor, context, id);
                if (temp == null) continue;
                numtype = temp.NumericType;
            }
            catch
            {
                continue;
            }

            // Skip if Not a Number
            if (numtype == SparqlNumericType.NaN) continue;

            // Track the Numeric Type
            if ((int)numtype > (int)mintype)
            {
                if (mintype == SparqlNumericType.NaN)
                {
                    // Initialise Minimums
                    switch (numtype)
                    {
                        case SparqlNumericType.Integer:
                            lngmin = temp.AsInteger();
                            decmin = temp.AsDecimal();
                            fltmin = temp.AsFloat();
                            dblmin = temp.AsDouble();
                            break;
                        case SparqlNumericType.Decimal:
                            decmin = temp.AsDecimal();
                            fltmin = temp.AsFloat();
                            dblmin = temp.AsDouble();
                            break;
                        case SparqlNumericType.Float:
                            fltmin = temp.AsFloat();
                            dblmin = temp.AsDouble();
                            break;
                        case SparqlNumericType.Double:
                            dblmin = temp.AsDouble();
                            break;
                    }
                    mintype = numtype;
                    continue;
                }
                else
                {
                    mintype = numtype;
                }
            }

            long lngval;
            decimal decval;
            float fltval;
            double dblval;
            switch (mintype)
            {
                case SparqlNumericType.Integer:
                    lngval = temp.AsInteger();

                    if (lngval < lngmin)
                    {
                        lngmin = lngval;
                        decmin = temp.AsDecimal();
                        fltmin = temp.AsFloat();
                        dblmin = temp.AsDouble();
                    }
                    break;
                case SparqlNumericType.Decimal:
                    decval = temp.AsDecimal();

                    if (decval < decmin)
                    {
                        decmin = decval;
                        fltmin = temp.AsFloat();
                        dblmin = temp.AsDouble();
                    }
                    break;
                case SparqlNumericType.Float:
                    fltval = temp.AsFloat();

                    if (fltval < fltmin)
                    {
                        fltmin = fltval;
                        dblmin = temp.AsDouble();
                    }
                    break;
                case SparqlNumericType.Double:
                    dblval = temp.AsDouble();

                    if (dblval < dblmin)
                    {
                        dblmin = dblval;
                    }
                    break;
            }
        }

        // Return the Min
        switch (mintype)
        {
            case SparqlNumericType.NaN:
                // No Numeric Values
                return null;

            case SparqlNumericType.Integer:
                // Integer Values
                return new LongNode(lngmin);

            case SparqlNumericType.Decimal:
                // Decimal Values
                return new DecimalNode(decmin);

            case SparqlNumericType.Float:
                // Float Values
                return new FloatNode(fltmin);

            case SparqlNumericType.Double:
                // Double Values
                return new DoubleNode(dblmin);

            default:
                throw new RdfQueryException("Failed to calculate a valid Minimum");
        }
    }
}
