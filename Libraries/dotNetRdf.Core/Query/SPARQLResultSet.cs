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

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Writing;

namespace VDS.RDF.Query;

/// <summary>
/// Class for representing Sparql Result Sets.
/// </summary>
public sealed class SparqlResultSet 
    : IEnumerable<ISparqlResult>, IDisposable, IEquatable<SparqlResultSet>
{
    /// <summary>
    /// Lists of Variables in the Result Set.
    /// </summary>
    private readonly List<string> _variables = new List<string>();
    /// <summary>
    /// Boolean Result.
    /// </summary>
    private bool _result;

    /// <summary>
    /// Creates an Empty Sparql Result Set.
    /// </summary>
    /// <remarks>Useful where you need a possible guarantee of returning an result set even if it proves to be empty and also necessary for the implementation of Result Set Parsers.</remarks>
    public SparqlResultSet()
    {
        // No actions needed
    }

    /// <summary>
    /// Creates a Sparql Result Set for the Results of an ASK Query with the given Result value.
    /// </summary>
    /// <param name="result"></param>
    public SparqlResultSet(bool result)
    {
        _result = result;
        ResultsType = SparqlResultsType.Boolean;
    }

    /// <summary>
    /// Creates a Sparql Result Set for the collection of results.
    /// </summary>
    /// <param name="results">Results.</param>
    public SparqlResultSet(IEnumerable<ISparqlResult> results)
    {
        ResultsType = SparqlResultsType.VariableBindings;
        Results = results.ToList();

        if (Results.Any())
        {
            _variables = Results.First().Variables.ToList();
        }
    }

    #region Properties

    /// <summary>
    /// Gets the Type of the Results Set.
    /// </summary>
    public SparqlResultsType ResultsType { get; private set; } = SparqlResultsType.Unknown;

    /// <summary>
    /// Gets the Result of an ASK Query.
    /// </summary>
    /// <remarks>Result Set is deemed to refer to an ASK query if the Variables list is empty since an ASK Query result has an empty &lt;head&gt;.  It is always true for any other Query type where one/more variables were requested even if the Result Set is empty.</remarks>
    public bool Result
    {
        get
        {
            // If No Variables then must have been an ASK Query with an empty <head>
            if (_variables.Count == 0)
            {
                // In this case the _result field contains the boolean result
                return _result;
            }
            else
            {
                // In any other case then it will contain true even if the result set was empty
                return true;
            }
        }
    }

    /// <summary>
    /// Gets the number of Results in the Result Set.
    /// </summary>
    public int Count => Results.Count;

    /// <summary>
    /// Gets whether the Result Set is empty and can have Results loaded into it.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public bool IsEmpty
    {
        get
        {
            switch (ResultsType)
            {
                case SparqlResultsType.Boolean:
                    return false;
                case SparqlResultsType.Unknown:
                    return true;
                case SparqlResultsType.VariableBindings:
                    return Results.Count == 0;
                default:
                    return true;
            }
        }
    }

    /// <summary>
    /// Gets the List of Results.
    /// </summary>
    public List<ISparqlResult> Results { get; } = new List<ISparqlResult>();

    /// <summary>
    /// Index directly into the Results.
    /// </summary>
    /// <param name="index">Index of the Result you wish to retrieve.</param>
    /// <returns></returns>
    public ISparqlResult this[int index] => Results[index];

    /// <summary>
    /// Gets the Variables used in the Result Set.
    /// </summary>
    /// <remarks>
    /// As of 1.0 where possible dotNetRDF tries to preserve the ordering of variables however this may not be possible depending on where the result set originates from or how it is populated.
    /// </remarks>
    public IEnumerable<string> Variables => from v in _variables select v;

    #endregion

    /// <summary>
    /// Trims the Result Set to remove unbound variables from results.
    /// </summary>
    /// <remarks>
    /// <strong>Note: </strong> This does not remove empty results this only removes unbound variables from individual results.
    /// </remarks>
    public void Trim()
    {
        Results.ForEach(r => r.Trim());
    }

    #region Internal Methods for filling the ResultSet

    /// <summary>
    /// Adds a Variable to the Result Set.
    /// </summary>
    /// <param name="var">Variable Name.</param>
    internal void AddVariable(string var)
    {
        if (!_variables.Contains(var))
        {
            _variables.Add(var);
        }
        ResultsType = SparqlResultsType.VariableBindings;
    }

    /// <summary>
    /// Adds a Result to the Result Set.
    /// </summary>
    /// <param name="result">Result.</param>
    internal void AddResult(ISparqlResult result)
    {
        if (ResultsType == SparqlResultsType.Boolean) throw new RdfException("Cannot add a Variable Binding Result to a Boolean Result Set");
        Results.Add(result);
        ResultsType = SparqlResultsType.VariableBindings;
    }

    /// <summary>
    /// Sets the Boolean Result for the Result Set.
    /// </summary>
    /// <param name="result">Boolean Result.</param>
    internal void SetResult(bool result)
    {
        if (ResultsType != SparqlResultsType.Unknown) throw new RdfException("Cannot set the Boolean Result value for this Result Set as its Result Type has already been set");
        _result = result;
        if (ResultsType == SparqlResultsType.Unknown) ResultsType = SparqlResultsType.Boolean;
    }

    #endregion

    #region Enumerator

    /// <summary>
    /// Gets an Enumerator for the Results List.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<ISparqlResult> GetEnumerator()
    {
        return Results.GetEnumerator();
    }

    /// <summary>
    /// Gets an Enumerator for the Results List.
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return Results.GetEnumerator();
    }

    #endregion

    /// <summary>
    /// Determines whether two results sets are equal.
    /// </summary>
    /// <param name="results">The other result set to compare with this one.</param>
    /// <returns>True if the result sets contain the same set of results (using graph comparison where blank nodes are involved), false otherwise.</returns>
    public bool Equals(SparqlResultSet results)
    {
        if (results == null) return false;

        // Must contain same number of Results to be equal
        if (Count != results.Count) return false;

        // Must have same Boolean result to be equal
        if (Result != results.Result) return false;

        // Must contain the same set of variables
        if (Variables.Count() != results.Variables.Count()) return false;
        if (!Variables.All(v => results.Variables.Contains(v))) return false;
        if (results.Variables.Any(v => !_variables.Contains(v))) return false;

        // If both have no results then they are equal
        if (Count == 0 && results.Count == 0) return true;

        // All Ground Results from the Result Set must appear in the Other Result Set
        var otherResults = results.OrderByDescending(r => r.Variables.Count()).ToList();
        var localResults = new List<ISparqlResult>();
        var grCount = 0;
        foreach (ISparqlResult result in Results.OrderByDescending(r => r.Variables.Count()))
        {
            if (result.IsGroundResult)
            {
                // If a Ground Result in this Result Set is not in the other Result Set we're not equal
                if (!otherResults.Remove(result)) return false;
                grCount++;
            }
            else
            {
                localResults.Add(result);
            }
        }

        // If all the Results were ground results and we've emptied all the Results from the other Result Set
        // then we were equal
        if (Count == grCount && otherResults.Count == 0) return true;

        // If the Other Results still contains Ground Results we're not equal
        if (otherResults.Any(r => r.IsGroundResult)) return false;

        // Create Graphs of the two sets of non-Ground Results
        var local = new SparqlResultSet();
        var other = new SparqlResultSet();
        foreach (var var in _variables)
        {
            local.AddVariable(var);
            other.AddVariable(var);
        }

        foreach (ISparqlResult r in localResults)
        {
            local.AddResult(r);
        }

        foreach (ISparqlResult r in otherResults)
        {
            other.AddResult(r);
        }

        // Compare the two Graphs for equality
        var writer = new SparqlRdfWriter();
        IGraph g = writer.GenerateOutput(local);
        IGraph h = writer.GenerateOutput(other);
        return g.Equals(h);
    }

    /// <summary>
    /// Converts a Result Set into a Triple Collection.
    /// </summary>
    /// <param name="g">Graph to generate the Nodes in.</param>
    /// <returns></returns>
    /// <remarks>
    /// Assumes the Result Set contains three variables ?s, ?p and ?o to use as the Subject, Predicate and Object respectively.  Only Results for which all three variables have bound values will generate Triples.
    /// </remarks>
    public BaseTripleCollection ToTripleCollection(IGraph g)
    {
        return ToTripleCollection(g, "s", "p", "o");
    }

    /// <summary>
    /// Converts a Result Set into a Triple Collection.
    /// </summary>
    /// <param name="g">Graph to generate the Nodes in.</param>
    /// <param name="subjVar">Variable whose value should be used for Subjects of Triples.</param>
    /// <param name="predVar">Variable whose value should be used for Predicates of Triples.</param>
    /// <param name="objVar">Variable whose value should be used for Object of Triples.</param>
    /// <param name="fullTripleIndex">Indicates if the returned triple collection should include all indexes (true) or only the basic indexes (false).</param>
    /// <returns></returns>
    /// <remarks>
    /// Only Results for which all three variables have bound values will generate Triples.
    /// </remarks>
    public BaseTripleCollection ToTripleCollection(IGraph g, string subjVar, string predVar, string objVar, bool fullTripleIndex = true)
    {
        BaseTripleCollection tripleCollection = new TreeIndexedTripleCollection(fullTripleIndex);

        foreach (ISparqlResult r in Results)
        {
            // Must have values available for all three variables
            if (r.HasValue(subjVar) && r.HasValue(predVar) && r.HasValue(objVar))
            {
                // None of the values is allowed to be unbound (i.e. null)
                if (r[subjVar] == null || r[predVar] == null || r[objVar] == null) continue;

                // If this is all OK we can generate a Triple
                tripleCollection.Add(new Triple(r[subjVar], r[predVar], r[objVar]));
            }
        }

        return tripleCollection;
    }

    /// <summary>
    /// Disposes of a Result Set.
    /// </summary>
    public void Dispose()
    {
        Results.Clear();
        _variables.Clear();
        _result = false;
    }
}
