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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Operators.Numeric;

/// <summary>
/// Represents the numeric multiplication operator.
/// </summary>
public class MultiplicationOperator
    : BaseNumericOperator
{
    /// <summary>
    /// Gets the operator type.
    /// </summary>
    public override SparqlOperatorType Operator => SparqlOperatorType.Multiply;

    /// <inheritdoc />
    public override bool IsExtension => false;

    /// <summary>
    /// Applies the operator.
    /// </summary>
    /// <param name="ns">Arguments.</param>
    /// <returns></returns>
    public override IValuedNode Apply(params IValuedNode[] ns)
    {
        if (ns == null) throw new RdfQueryException("Cannot apply to null arguments");
        if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply multiplication when any arguments are null");

        var type = (SparqlNumericType)ns.Max(n => (int)n.NumericType);

        switch (type)
        {
            case SparqlNumericType.Integer:
                return new LongNode(Multiply(ns.Select(n => n.AsInteger())));
            case SparqlNumericType.Decimal:
                return new DecimalNode(Multiply(ns.Select(n => n.AsDecimal())));
            case SparqlNumericType.Float:
                return new FloatNode(Multiply(ns.Select(n => n.AsFloat())));
            case SparqlNumericType.Double:
                return new DoubleNode(Multiply(ns.Select(n => n.AsDouble())));
            default:
                throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
        }
    }

    private long Multiply(IEnumerable<long> ls)
    {
        var first = true;
        long total = 0;
        foreach (var l in ls)
        {
            if (first)
            {
                total = l;
                first = false;
            }
            else
            {
                total *= l;
            }
        }
        return total;
    }

    private decimal Multiply(IEnumerable<decimal> ls)
    {
        var first = true;
        decimal total = 0;
        foreach (var l in ls)
        {
            if (first)
            {
                total = l;
                first = false;
            }
            else
            {
                total *= l;
            }
        }
        return total;
    }

    private float Multiply(IEnumerable<float> ls)
    {
        var first = true;
        float total = 0;
        foreach (var l in ls)
        {
            if (first)
            {
                total = l;
                first = false;
            }
            else
            {
                total *= l;
            }
        }
        return total;
    }

    private double Multiply(IEnumerable<double> ls)
    {
        var first = true;
        double total = 0;
        foreach (var l in ls)
        {
            if (first)
            {
                total = l;
                first = false;
            }
            else
            {
                total *= l;
            }
        }
        return total;
    }
}
