using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.PullAsync;

namespace dotNetRdf.Query.PullAsync.Tests;

internal class AsyncIntegerEnumeration : IAsyncEvaluation
{
    private readonly INodeFactory _nodeFactory;
    private readonly string _varName;
    private readonly int _start;
    private readonly int _stop;
    private readonly int _step;
    private readonly Uri _xsdInteger;
    private readonly int _wait;
    
    public AsyncIntegerEnumeration(INodeFactory nodeFactory, string varName, int start, int stop, int step, int wait = 0)
    {
        _nodeFactory = nodeFactory;
        _varName = varName;
        _start = start;
        _step = step;
        _stop = stop;
        _xsdInteger = new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger);
        _wait = wait;
    }
        
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var mustMatch = input?[_varName]?.AsValuedNode().AsInteger();

        for (long i = _start; i <= _stop; i += _step)
        {
            if (_wait > 0)
            {
                await Task.Delay(_wait, cancellationToken);
            }

            if (mustMatch == null || mustMatch == i)
            {
                ISet s = new Set();
                s.Add(_varName, _nodeFactory.CreateLiteralNode(i.ToString(), _xsdInteger));
                yield return s;
            }
        }
    }

    public async IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context,
        IEnumerable<ISet?> batch, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var resultBatch = new List<ISet>();
        foreach (var input in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mustMatch = input?[_varName]?.AsValuedNode().AsInteger();
            for (long i = _start; i <= _stop; i += _step)
            {
                if (_wait > 0)
                {
                    await Task.Delay(_wait, cancellationToken);
                }

                if (mustMatch == null || mustMatch == i)
                {
                    ISet s = new Set();
                    s.Add(_varName, _nodeFactory.CreateLiteralNode(i.ToString(), _xsdInteger));
                    resultBatch.Add(s);
                    if (resultBatch.Count >= context.TargetBatchSize)
                    {
                        yield return resultBatch;
                        resultBatch = new List<ISet>();
                    }
                }
            }
        }
        if (resultBatch.Count > 0) yield return resultBatch;
    }
}