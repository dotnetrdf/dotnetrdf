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

using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Describe;

namespace VDS.RDF.Query.Pull;

internal class DescriberContext(
    SparqlQuery query,
    PullEvaluationContext evaluationContext,
    IAsyncEnumerable<ISet> solutionBindings)
    : ISparqlDescribeContext
{
    private readonly SparqlQuery _query = query;

    public ITripleIndex TripleIndex { get; } = new TripleStoreTripleIndex(evaluationContext.Data, evaluationContext.UnionDefaultGraph);
    public SparqlQuery Query { get; } = query;

    public IEnumerable<INode> GetNodes(INodeFactory nodeFactory)
    {
        INodeFactory factory = evaluationContext.NodeFactory;
        INamespaceMapper nsmap = evaluationContext.NodeFactory.NamespaceMap;
        Uri? baseUri = evaluationContext.BaseUri;
        foreach (IToken? t in _query.DescribeVariables.Where(t=>t.TokenType == Token.QNAME || t.TokenType == Token.URI))
        {
            yield return factory.CreateUriNode(
                UriFactory.Create(Tools.ResolveUriOrQName(t, nsmap, baseUri)));
        }
        List<string> descVars =
            _query.QueryType == SparqlQueryType.DescribeAll ? 
                _query.Variables.Select(v=>v.Name).ToList() :
                _query.DescribeVariables.Where(v=>v.TokenType == Token.VARIABLE).Select(t=>t.Value.Substring(1)).ToList();
        if (descVars.Any())
        {
            foreach (ISet? solution in solutionBindings.ToEnumerable()) // TODO: Eliminate sync over async
            {
                if (solution != null)
                {
                    foreach (var dv in descVars)
                    {
                        INode? tmp = solution[dv];
                        if (tmp != null) yield return tmp;
                    }
                }
            }
        }
    }
}