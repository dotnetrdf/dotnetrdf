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
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Grouping;

/// <summary>
/// Abstract Base Class for classes representing Sparql GROUP BY clauses.
/// </summary>
public abstract class BaseGroupBy
    : ISparqlGroupBy
{
    /// <summary>
    /// Child Grouping.
    /// </summary>
    protected ISparqlGroupBy _child = null;

    private string _assignVariable;

    /// <summary>
    /// Gets/Sets the Child GROUP BY Clause.
    /// </summary>
    public ISparqlGroupBy Child
    {
        get
        {
            return _child;
        }
        set
        {
            _child = value;
        }
    }

    /// <summary>
    /// Gets the Variables involved in this Group By.
    /// </summary>
    public abstract IEnumerable<string> Variables
    {
        get;
    }

    /// <summary>
    /// Gets the Projectable Variables used in the GROUP BY i.e. Variables that are grouped upon and Assigned Variables.
    /// </summary>
    public abstract IEnumerable<string> ProjectableVariables
    {
        get;
    }

    /// <summary>
    /// Gets the Expression used to GROUP BY.
    /// </summary>
    public abstract ISparqlExpression Expression
    {
        get;
    }

    /// <summary>
    /// Gets/Sets the Variable that the grouped upon value should be assigned to.
    /// </summary>
    public string AssignVariable
    {
        get
        {
            return _assignVariable;
        }
        set
        {
            _assignVariable = value;
        }
    }
}

/// <summary>
/// Represents a Grouping on a given Variable.
/// </summary>
public class GroupByVariable
    : BaseGroupBy
{
    private string _name;

    /// <summary>
    /// Creates a new Group By which groups by a given Variable.
    /// </summary>
    /// <param name="name">Variable Name.</param>
    public GroupByVariable(string name)
    {
        _name = name;
    }

    /// <summary>
    /// Creates a new Group By which groups by a given Variable and assigns to another variable.
    /// </summary>
    /// <param name="name">Variable Name.</param>
    /// <param name="assignVariable">Assign Variable.</param>
    public GroupByVariable(string name, string assignVariable)
        : this(name)
    {
        AssignVariable = assignVariable;
    }


    /// <summary>
    /// Gets the Variables used in the GROUP BY.
    /// </summary>
    public override IEnumerable<string> Variables
    {
        get 
        {
            if (_child == null)
            {
                return _name.AsEnumerable<String>();
            }
            else
            {
                return _child.Variables.Concat(_name.AsEnumerable<String>());
            }
        }
    }

    /// <summary>
    /// Gets the Projectable Variables used in the GROUP BY i.e. Variables that are grouped upon and Assigned Variables.
    /// </summary>
    public override IEnumerable<string> ProjectableVariables
    {
        get
        {
            var vars = new List<string>();
            if (AssignVariable != null) vars.Add(AssignVariable);
            vars.Add(_name);

            if (_child != null) vars.AddRange(_child.ProjectableVariables);
            return vars.Distinct();
        }
    }

    /// <summary>
    /// Gets the Variable Expression Term used by this GROUP BY.
    /// </summary>
    public override ISparqlExpression Expression
    {
        get 
        {
            return new VariableTerm(_name); 
        }
    }

    /// <summary>
    /// Gets the String representation of the GROUP BY.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        if (AssignVariable != null && !AssignVariable.Equals(_name))
        {
            output.Append('(');
        }
        output.Append('?');
        output.Append(_name);
        if (AssignVariable != null && !AssignVariable.Equals(_name))
        {
            output.Append(" AS ?");
            output.Append(AssignVariable);
            output.Append(')');
        }

        if (_child != null)
        {
            output.Append(' ');
            output.Append(_child);
        }

        return output.ToString();
    }
}

/// <summary>
/// Represents a Grouping on a given Expression.
/// </summary>
public class GroupByExpression
    : BaseGroupBy
{
    private ISparqlExpression _expr;

    /// <summary>
    /// Creates a new Group By which groups by a given Expression.
    /// </summary>
    /// <param name="expr">Expression.</param>
    public GroupByExpression(ISparqlExpression expr)
    {
        _expr = expr;
    }

    /// <summary>
    /// Gets the Fixed Variables used in the Grouping.
    /// </summary>
    public override IEnumerable<string> Variables
    {
        get
        {
            return _child == null ? _expr.Variables : _expr.Variables.Concat(_child.Variables);
        }
    }

    /// <summary>
    /// Gets the Projectable Variables used in the GROUP BY i.e. Variables that are grouped upon and Assigned Variables.
    /// </summary>
    public override IEnumerable<string> ProjectableVariables
    {
        get
        {
            var vars = new List<string>();
            if (AssignVariable != null) vars.Add(AssignVariable);
            if (_expr is VariableTerm)
            {
                vars.AddRange(_expr.Variables);
            }

            if (_child != null) vars.AddRange(_child.ProjectableVariables);
            return vars.Distinct();
        }
    }

    /// <summary>
    /// Gets the Expression used to GROUP BY.
    /// </summary>
    public override ISparqlExpression Expression
    {
        get 
        {
            return _expr;
        }
    }

    /// <summary>
    /// Gets the String representation of the GROUP BY.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append('(');
        output.Append(_expr);
        if (AssignVariable != null)
        {
            output.Append(" AS ?");
            output.Append(AssignVariable);
        }
        output.Append(')');

        if (_child != null)
        {
            output.Append(' ');
            output.Append(_child);
        }

        return output.ToString();
    }
}