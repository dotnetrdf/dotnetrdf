using System.Diagnostics.CodeAnalysis;
using VDS.RDF;
using VDS.RDF.Query;

namespace dotNetRdf.Query.PullEvaluation.Paths;

internal class PathResult
{
    public readonly INode StartNode;
    public readonly INode EndNode;
    public readonly HashSet<INode> PathNodes;

    public PathResult(INode startNode, INode endNode)
    {
        StartNode = startNode;
        EndNode = endNode;
        PathNodes = new HashSet<INode>();
    }

    private PathResult(PathResult previous, INode stepEndNode)
    {
        StartNode = previous.StartNode;
        EndNode = stepEndNode;
        PathNodes = new HashSet<INode>(previous.PathNodes) { stepEndNode };
    }

    public bool TryExtend(IPatternEvaluationContext context, INode stepEnd, [NotNullWhen(returnValue:true)] out PathResult? extended)
    {
        if (PathNodes.Contains(stepEnd))
        {
            extended = null;
            return false;
        }
        extended = new PathResult(this, stepEnd);
        return true;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is PathResult other)
        {
            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartNode, EndNode, PathNodes);
    }

    public bool Equals(PathResult other)
    {
        if (ReferenceEquals(this, other)) return true;
        return PathNodes.SetEquals(other.PathNodes) &&
               StartNode.Equals(other.StartNode) &&
               EndNode.Equals(other.EndNode);
    }
}