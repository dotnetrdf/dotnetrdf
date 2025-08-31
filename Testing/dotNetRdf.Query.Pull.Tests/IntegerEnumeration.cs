using System.Collections;
using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull;

namespace dotNetRdf.Query.Pull.Tests;

internal class IntegerEnumeration : IEnumerableEvaluation
{
    private readonly INodeFactory _nodeFactory;
    private readonly string _varName;
    private readonly int _start;
    private readonly int _stop;
    private readonly int _step;
    private readonly Uri _xsdInteger;
    
    public IntegerEnumeration(INodeFactory nodeFactory, string varName, int start, int stop, int step)
    {
        _nodeFactory = nodeFactory;
        _varName = varName;
        _start = start;
        _step = step;
        _stop = stop;
        _xsdInteger = new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger);
    }
        
    public IEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        var mustMatch = input?[_varName]?.AsValuedNode().AsInteger();

        for (long i = _start; i <= _stop; i += _step)
        {
            if (mustMatch == null || mustMatch == i)
            {
                ISet s = new Set();
                s.Add(_varName, _nodeFactory.CreateLiteralNode(i.ToString(), _xsdInteger));
                yield return s;
            }
        }
    }
}