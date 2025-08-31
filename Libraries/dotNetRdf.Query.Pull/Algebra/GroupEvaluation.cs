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

using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull.Aggregation;

namespace VDS.RDF.Query.Pull.Algebra;

internal class GroupEvaluation
{
    private readonly List<IAggregateEvaluation> _aggregations = new();
    public GroupEvaluation(IEnumerable<IAggregateEvaluation> aggregations)
    {
        _aggregations.AddRange(aggregations);
        foreach (IAggregateEvaluation agg in _aggregations)
        {
            agg.Start();
        }
    }

    public void Accept(ISet solutionBinding, IRefNode? activeGraph)
    {
        foreach (IAggregateEvaluation agg in _aggregations)
        {
            agg.Accept(new ExpressionContext(solutionBinding, activeGraph));
        }
    }

    public ISet GetBindings()
    {
        ISet ret = new Set();
        foreach (IAggregateEvaluation agg in _aggregations)
        {
            agg.End();
            ret.Add(agg.VariableName, agg.Value);
        }

        return ret;
    }
}