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

internal class AsyncGraphEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _inner;
    private readonly string? _graphVarName;
    private readonly IRefNode? _graphName;
    private readonly IRefNode _unboundGraphName = new UriNode(new Uri("urn:x-dotNetRdf:unbound"));

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
    
    [Obsolete("Replaced by EvaluateBatch()")]
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

    public async IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context, IEnumerable<ISet?> batch, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_graphName != null)
        {
            await foreach (IEnumerable<ISet>? result in _inner.EvaluateBatch(context, batch, _graphName, cancellationToken))
            {
                yield return result;
            }
        }
        else
        {
            IEnumerable<IGrouping<IRefNode, ISet?>> groupedBatches = batch.GroupBy(GetBoundGraphName);
            foreach (IGrouping<IRefNode, ISet?> group in groupedBatches)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (group.Key.Equals(_unboundGraphName))
                {
                    foreach (IRefNode? gn in context.NamedGraphNames)
                    {
                        IEnumerable<ISet> graphBatch = group.Select(s =>
                        {
                            ISet inputWithGraphBinding = s?.Copy() ?? new Set();
                            if (_graphVarName != null)
                            {
                                inputWithGraphBinding.Add(_graphVarName, gn);
                            }

                            return inputWithGraphBinding;
                        });
                        await foreach (IEnumerable<ISet> innerBatchResult in _inner.EvaluateBatch(context, graphBatch, gn,
                                           cancellationToken))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            yield return innerBatchResult;
                        }
                    }
                }
                else
                {
                    await foreach (IEnumerable<ISet> innerBatchResult in _inner.EvaluateBatch(context, group, group.Key,
                                       cancellationToken))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        yield return innerBatchResult;
                    }
                }
            }
        }
    }

    private IRefNode GetBoundGraphName(ISet? s)
    {
        if (s != null && s.ContainsVariable(_graphVarName) && s[_graphVarName] is IRefNode graphName) return graphName;
        return _unboundGraphName;
    }
}