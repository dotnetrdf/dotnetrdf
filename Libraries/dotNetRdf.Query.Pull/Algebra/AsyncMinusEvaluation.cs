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

using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class MinusEvaluation : IEnumerableEvaluation
{
    private readonly IEnumerableEvaluation _lhs;
    private readonly IEnumerableEvaluation _rhs;
    private readonly List<string> _minusVars;
    
    public MinusEvaluation(Minus minus, IEnumerableEvaluation lhs, IEnumerableEvaluation rhs)
    {
        _minusVars = minus.Lhs.Variables.Intersect(minus.Rhs.Variables).ToList();
        _lhs = lhs;
        _rhs = rhs;
    }
    
    public IEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        // TODO: Is there a way to optimise this to either avoid computing a full list of rhs bindings or to store them in an indexed structure to make IsCompatible more efficient?
        var rhsSolutions = _rhs.Evaluate(context, input, activeGraph, cancellationToken).ToList();
        foreach (ISet? lhsSolution in _lhs.Evaluate(context, input, activeGraph, cancellationToken))
        {
            if (rhsSolutions.Any(r => IsCompatible(lhsSolution, r, _minusVars)))
            {
                continue;
            }
            yield return lhsSolution;
        }
    }

    private static bool IsCompatible(ISet l, ISet r, List<string> vars)
    {
        var testVars = vars.Where(v => l[v] != null && r[v] != null).ToList();
        return testVars.Any() && testVars.All(v => l[v].Equals(r[v]));
    }
}