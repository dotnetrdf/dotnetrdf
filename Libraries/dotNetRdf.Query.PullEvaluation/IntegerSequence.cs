using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

public class IntegerSequence: IAsyncEnumerator<ISet>
{
    private readonly INodeFactory _nodeFactory;
    private readonly string _varName;
    private readonly int _start;
    private readonly int _step;
    private readonly int _stop;
    private int _counter = -1;
    private readonly int _wait;
    private bool _started;
    private ISet? _current;
    private readonly Uri _xsdInteger;

    public IntegerSequence (INodeFactory nodeFactory, string varName, int start, int stop, int step, int wait = 0)
    {
        _nodeFactory = nodeFactory;
        _varName = varName;
        _start = start;
        _step = step;
        _stop = stop;
        _started = false;
        _xsdInteger = new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger);
        _wait = wait;
    }


    public async ValueTask DisposeAsync()
    {
        _current = null;
    }

    public async ValueTask<bool> MoveNextAsync()
    {
        if (_wait > 0)
        {
            await Task.Delay(_wait);
        }
        if (!_started)
        {
            _counter = _start;
            _started = true;
        }
        else
        {
            _counter += _step;
        }

        if (_counter > _stop)
        {
            _current = null;
            return false;
        }

        _current = new Set();
        _current.Add(_varName, _nodeFactory.CreateLiteralNode(_counter.ToString(), _xsdInteger));
        return true;
    }

    public ISet Current
    {
        get
        {
            if (_current == null)
            {
                throw new InvalidOperationException();
            }

            return _current;
        }
    }
}