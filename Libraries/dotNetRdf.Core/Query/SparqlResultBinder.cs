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

namespace VDS.RDF.Query;

/// <summary>
/// Helper Class used in the execution of Sparql Queries.
/// </summary>
/// <remarks>
/// </remarks>
public abstract class SparqlResultBinder
    : IDisposable
{
    private SparqlQuery _query;
    private Dictionary<int,BindingGroup> _groups = null;

    /// <summary>
    /// Internal Empty Constructor for derived classes.
    /// </summary>
    protected internal SparqlResultBinder()
    {

    }

    /// <summary>
    /// Creates a new Results Binder.
    /// </summary>
    /// <param name="query">Query this provides Result Binding to.</param>
    public SparqlResultBinder(SparqlQuery query)
    {
        _query = query;
    }

    /// <summary>
    /// Gets the Variables that the Binder stores Bindings for.
    /// </summary>
    public abstract IEnumerable<string> Variables
    {
        get;
    }

    /// <summary>
    /// Gets the enumeration of valid Binding IDs.
    /// </summary>
    public abstract IEnumerable<int> BindingIDs
    {
        get;
    }

    /// <summary>
    /// Gets the set of Groups that result from the Query this Binder provides Binding to.
    /// </summary>
    public IEnumerable<BindingGroup> Groups
    {
        get
        {
            if (_groups != null)
            {
                return (from g in _groups.Values
                        select g);
            }
            else
            {
                return Enumerable.Empty<BindingGroup>();
            }
        }
    }

    /// <summary>
    /// Gets the Value bound to a given Variable for a given Binding ID.
    /// </summary>
    /// <param name="name">Variable Name.</param>
    /// <param name="bindingID">Binding ID.</param>
    /// <returns></returns>
    public abstract INode Value(string name, int bindingID);

    /// <summary>
    /// Gets the Group referred to by the given ID.
    /// </summary>
    /// <param name="groupID">Group ID.</param>
    /// <returns></returns>
    public virtual BindingGroup Group(int groupID)
    {
        if (_groups != null)
        {
            if (_groups.ContainsKey(groupID))
            {
                return _groups[groupID];
            }
            else
            {
                throw new RdfQueryException("The Group with ID " + groupID + " does not exist in the Result Binder");
            }
        }
        else
        {
            throw new RdfQueryException("Cannot lookup a Group when the Query has not been executed or does not contain Groups as part of it's Results");
        }
    }

    /// <summary>
    /// Checks whether the given ID refers to a Group.
    /// </summary>
    /// <param name="groupID">Group ID.</param>
    /// <returns></returns>
    public virtual bool IsGroup(int groupID)
    {
        if (_groups == null)
        {
            return false;
        }
        else
        {
            return _groups.ContainsKey(groupID);
        }
    }

    /// <summary>
    /// Sets the Group Context for the Binder.
    /// </summary>
    /// <param name="accessContents">Whether you want to access the Group Contents or the Groups themselves.</param>
    public virtual void SetGroupContext(bool accessContents)
    {
    }

    /// <summary>
    /// Disposes of a Result Binder.
    /// </summary>
    public virtual void Dispose()
    {
        _groups.Clear();
    }
}
