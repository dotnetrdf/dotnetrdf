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

using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class ExtendEvaluation(Extend extend, IEnumerableEvaluation inner)
    : IEnumerableEvaluation
{
    public IEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        return inner.Evaluate(context, input, activeGraph, cancellationToken).Select(solution => Extend(context, solution, activeGraph));
    }

    private ISet Extend(PullEvaluationContext context, ISet solution, IRefNode? activeGraph)
    {
        if (solution.ContainsVariable(extend.VariableName))
        {
            throw new RdfQueryException(
                $"Cannot assign to the variable ?{extend.VariableName} since it has previously been used in the query.");
        }

        try
        {
            INode value = extend.AssignExpression.Accept(context.ExpressionProcessor, context, new ExpressionContext(solution, activeGraph));
            solution.Add(extend.VariableName, value);
        }
        catch
        {
            // No assignment if there is an error, but the solution is preserved.
        }

        return solution;
    }
}