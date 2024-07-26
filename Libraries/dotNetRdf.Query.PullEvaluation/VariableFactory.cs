namespace dotNetRdf.Query.PullEvaluation;

internal class VariableFactory(string prefix)
{
    private int _nextId = 0;

    public string NextId()
    {
        return prefix + _nextId++;
    }
    public string Prefix { get => prefix; }
}