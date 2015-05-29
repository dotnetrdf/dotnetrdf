/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Elements;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Results;
using VDS.RDF.Query.Sorting;
using VDS.RDF.Query.Templates;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Interface for SPARQL Queries
    /// </summary>
    public interface IQuery
        : IEquatable<IQuery>
    {
        /// <summary>
        /// Gets/Sets the namespaces for the query
        /// </summary>
        INamespaceMapper Namespaces { get; set; }

        /// <summary>
        /// Gets/Sets the Base URI for the Query
        /// </summary>
        Uri BaseUri { get; set; }

        /// <summary>
        /// Gets/Sets the default graphs for the query
        /// </summary>
        IList<INode> DefaultGraphs { get; set; }

        /// <summary>
        /// Gets/Sets the named graphs for the query
        /// </summary>
        IList<INode> NamedGraphs { get; set; }

        /// <summary>
        /// Gets/Sets the Query Type
        /// </summary>
        QueryType QueryType { get; set; }

        /// <summary>
        /// Gets/Sets the limit for the query
        /// </summary>
        long Limit { get; set; }

        /// <summary>
        /// Gets/Sets the offset for the query
        /// </summary>
        long Offset { get; set; }

        /// <summary>
        /// Gets whether there is a limit on this query
        /// </summary>
        /// <returns>True if the limit is greater than or equal to zero, false otherwise</returns>
        bool HasLimit { get; }

        /// <summary>
        /// Gets whether there is an offset on this query
        /// </summary>
        /// <returns>True if the offset is greater than zero, false otherwise</returns>
        bool HasOffset { get; }

        /// <summary>
        /// Gets/Sets the sort conditions that make up the ORDER BY for the query
        /// </summary>
        IList<ISortCondition> SortConditions { get; set; }

        /// <summary>
        /// Gets/Sets the WHERE clause of the query
        /// </summary>
        IElement WhereClause { get; set; }

        /// <summary>
        /// Gets/Sets the HAVING conditions of the query
        /// </summary>
        IList<IExpression> HavingConditions { get; set; }

        /// <summary>
        /// Gets/Sets the GROUP BY expressions of the query
        /// </summary>
        IList<KeyValuePair<IExpression, string>> GroupExpressions { get; set; }

        /// <summary>
        /// Gets/Sets the VALUES clause of the query
        /// </summary>
        ITabularResults ValuesClause { get; set; }

        /// <summary>
        /// Gets/Sets the CONSTRUCT template for the query
        /// </summary>
        ITemplate ConstructTemplate { get; set; }

        /// <summary>
        /// Gets/Sets the projections from the query
        /// </summary>
        IEnumerable<KeyValuePair<String, IExpression>> Projections { get; set; }

        /// <summary>
        /// Adds a variable to the projections of the query
        /// </summary>
        /// <param name="var">Variable to project</param>
        void AddProjectVariable(String var);

        /// <summary>
        /// Adds a project expression to the projections of the query
        /// </summary>
        /// <param name="var">Variable to assign value to</param>
        /// <param name="expr">Expression</param>
        void AddProjectExpression(String var, IExpression expr);

        /// <summary>
        /// Removes the projection for the given variable
        /// </summary>
        /// <param name="var">Variable</param>
        void RemoveProjection(String var);

        /// <summary>
        /// Removes all projections for the query
        /// </summary>
        void ClearProjections();

        /// <summary>
        /// Gets the variables that will be the results of the query
        /// </summary>
        IEnumerable<String> ResultVariables { get; } 
    }
}
