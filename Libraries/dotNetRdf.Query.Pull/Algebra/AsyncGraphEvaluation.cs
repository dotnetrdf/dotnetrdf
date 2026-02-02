/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncGraphEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _inner;
    private readonly string? _graphVarName;
    private readonly IRefNode? _graphName;

    public AsyncGraphEvaluation(IRefNode graphName, IAsyncEvaluation inner)
    {
        _graphName = graphName;
        _inner = inner;
    }

    public AsyncGraphEvaluation(string variableName, IAsyncEvaluation inner)
    {
        _graphVarName = variableName;
        _inner = inner;
    }
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        if (_graphName != null)
        {
            // Fixed graph filter
            return _inner.Evaluate(context, input, _graphName, cancellationToken);
        }
        if (input != null && input.ContainsVariable(_graphVarName) && input[_graphVarName] is IRefNode graphName)
        {
            // Graph variable is bound by input
            return _inner.Evaluate(context, input, graphName, cancellationToken);
        }

        return context.NamedGraphNames.Select(gn =>
            {
                ISet inputWithGraphBinding = input?.Copy() ?? new Set();
                if (_graphVarName != null)
                {
                    inputWithGraphBinding.Add(_graphVarName, gn);
                }

                return _inner.Evaluate(context, inputWithGraphBinding, gn, cancellationToken);
            })
            .Merge();
    }
}