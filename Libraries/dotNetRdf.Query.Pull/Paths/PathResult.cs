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

using System.Diagnostics.CodeAnalysis;

namespace VDS.RDF.Query.Pull.Paths;

internal class PathResult
{
    public readonly INode StartNode;
    public readonly INode EndNode;
    public readonly HashSet<INode> PathNodes;

    public PathResult(INode startNode, INode endNode)
    {
        StartNode = startNode;
        EndNode = endNode;
        PathNodes = [];
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