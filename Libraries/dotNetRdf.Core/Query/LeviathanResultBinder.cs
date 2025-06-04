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
/// Results Binder used by Leviathan.
/// </summary>
public class LeviathanResultBinder
    : SparqlResultBinder
{
    private SparqlEvaluationContext _context;
    private GroupMultiset _groupSet;

    /// <summary>
    /// Creates a new Leviathan Results Binder.
    /// </summary>
    /// <param name="context">Evaluation Context.</param>
    public LeviathanResultBinder(SparqlEvaluationContext context)
        : base()
    {
        _context = context;
    }

    /// <summary>
    /// Gets the Value for a given Variable from the Set with the given Binding ID.
    /// </summary>
    /// <param name="name">Variable.</param>
    /// <param name="bindingID">Set ID.</param>
    /// <returns></returns>
    public override INode Value(string name, int bindingID)
    {
        return _context.InputMultiset is IdentityMultiset ? null : _context.InputMultiset[bindingID][name];
        // return _context.InputMultiset[bindingID][name];
    }

    /// <summary>
    /// Gets the Variables contained in the Input.
    /// </summary>
    public override IEnumerable<string> Variables
    {
        get
        {
            return _context.InputMultiset.Variables;
        }
    }

    /// <summary>
    /// Gets the IDs of Sets.
    /// </summary>
    public override IEnumerable<int> BindingIDs
    {
        get
        {
            return _context.InputMultiset.SetIDs;
        }
    }

    /// <summary>
    /// Determines whether a given ID is for of a Group.
    /// </summary>
    /// <param name="groupID">Group ID.</param>
    /// <returns></returns>
    public override bool IsGroup(int groupID)
    {
        if (_context.InputMultiset is GroupMultiset || _groupSet != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the Group with the given ID.
    /// </summary>
    /// <param name="groupID">Group ID.</param>
    /// <returns></returns>
    public override BindingGroup Group(int groupID)
    {
        if (_context.InputMultiset is GroupMultiset)
        {
            var groupSet = (GroupMultiset)_context.InputMultiset;
            return groupSet.Group(groupID);
        }
        else if (_groupSet != null)
        {
            return _groupSet.Group(groupID);
        }
        else
        {
            throw new RdfQueryException("Cannot retrieve a Group when the Input Multiset is not a Group Multiset");
        }
    }

    /// <summary>
    /// Sets the Group Context for the Binder.
    /// </summary>
    /// <param name="accessContents">Whether you want to access the Group Contents or the Groups themselves.</param>
    public override void SetGroupContext(bool accessContents)
    {
        if (accessContents)
        {
            if (_context.InputMultiset is GroupMultiset)
            {
                _groupSet = (GroupMultiset)_context.InputMultiset;
                _context.InputMultiset = _groupSet.Contents;

            }
            else
            {
                throw new RdfQueryException("Cannot set Group Context to access Contents data when the Input is not a Group Multiset, you may be trying to use a nested aggregate which is illegal");
            }
        }
        else
        {
            if (_groupSet != null)
            {
                _context.InputMultiset = _groupSet;
                _groupSet = null;
            }
            else
            {
                throw new RdfQueryException("Cannot set Group Context to acess Group data when there is no Group data available");
            }
        }
    }
}
