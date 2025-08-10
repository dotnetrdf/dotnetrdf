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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VDS.RDF.Shacl.Validation;

/// <summary>
/// Represents a collection of SHACL validation results.
/// </summary>
public class ResultCollection : ICollection<Result>
{
    private readonly Report report;

    [DebuggerStepThrough]
    internal ResultCollection(Report shaclValidationReport)
    {
        report = shaclValidationReport;
    }

    int ICollection<Result>.Count
    {
        get
        {
            return Results.Count();
        }
    }

    bool ICollection<Result>.IsReadOnly
    {
        get
        {
            return false;
        }
    }

    private IEnumerable<Result> Results
    {
        get
        {
            return
                from result in Vocabulary.Result.ObjectsOf(report, report.Graph)
                select Result.Parse(report.Graph, result);
        }
    }

    void ICollection<Result>.Add(Result item)
    {
        report.Graph.Assert(report, Vocabulary.Result, item);
    }

    void ICollection<Result>.Clear()
    {
        foreach (Result result in Results.ToList())
        {
            ((ICollection<Result>)this).Remove(result);
        }
    }

    bool ICollection<Result>.Contains(Result item)
    {
        return Results.Contains(item);
    }

    void ICollection<Result>.CopyTo(Result[] array, int arrayIndex)
    {
        Results.ToList().CopyTo(array, arrayIndex);
    }

    IEnumerator<Result> IEnumerable<Result>.GetEnumerator()
    {
        return Results.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<Result>)this).GetEnumerator();
    }

    bool ICollection<Result>.Remove(Result result)
    {
        var contained = ((ICollection<Result>)this).Contains(result);
        report.Graph.Retract(report, Vocabulary.Result, result);
        return contained;
    }
}