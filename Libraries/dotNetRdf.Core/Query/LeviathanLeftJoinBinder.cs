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

using System.Collections.Generic;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query;

/// <summary>
/// Special Temporary Results Binder used during LeftJoin's.
/// </summary>
public class LeviathanLeftJoinBinder : SparqlResultBinder
{
    private BaseMultiset _input;

    /// <summary>
    /// Creates a new LeftJoin Binder.
    /// </summary>
    /// <param name="multiset">Input Multiset.</param>
    public LeviathanLeftJoinBinder(BaseMultiset multiset)
        : base()
    {
        _input = multiset;
    }

    /// <summary>
    /// Gets the Value for a given Variable from the Set with the given Binding ID.
    /// </summary>
    /// <param name="name">Variable.</param>
    /// <param name="bindingID">Set ID.</param>
    /// <returns></returns>
    public override INode Value(string name, int bindingID)
    {
        return _input[bindingID][name];
    }

    /// <summary>
    /// Gets the Variables in the Input Multiset.
    /// </summary>
    public override IEnumerable<string> Variables
    {
        get
        {
            return _input.Variables;
        }
    }

    /// <summary>
    /// Gets the IDs of Sets.
    /// </summary>
    public override IEnumerable<int> BindingIDs
    {
        get
        {
            return _input.SetIDs;
        }
    }
}
