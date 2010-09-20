/* 
 * Copyright (C) 2007, Andrew Matthews http://aabs.wordpress.com/
 *
 * This file is Free Software and part of LinqToRdf http://code.google.com/fromName/linqtordf/
 *
 * It is licensed under the following license:
 *   - Berkeley License, V2.0 or any newer version
 *
 * You may not use this file except in compliance with the above license.
 *
 * See http://code.google.com/fromName/linqtordf/ for the complete text of the license agreement.
 *
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace VDS.RDF.Linq
{
    /// <summary>
    /// Supertype for all LINQ query types (that are defined by LinqToRdf)
    /// </summary>
    /// <typeparam name="T">the type of object being queried for</typeparam>
    public class QuerySupertype<T>
    {
        /// <summary>
        /// A collection of each sub expression that forms the query to be performed
        /// </summary>
        protected Dictionary<string, MethodCallExpression> expressions;

        /// <summary>
        /// A logger that external clients can attach to (imitating LINQ to SQL)
        /// </summary>
        protected TextWriter externalLogger;
        /// <summary>
        /// A <see cref="NamespaceManager"/> that keeps track of the namespaces that need to be added to the query as prefixes.
        /// </summary>
        protected NamespaceManager namespaceManager = new NamespaceManager();

        /// <summary>
        /// indicates whether subsequent enumerations of the results should rerun the query, or just use previous results.
        /// </summary>
        private bool shouldReuseResultset;
        /// <summary>
        /// Gets or sets the <see cref="IRdfContext"/> that this query is running within.
        /// </summary>
        public IRdfContext DataContext { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T"/> results that were retrieved when the query was enumerated. these will be reused if <see cref="ShouldReuseResultset"/> is true.
        /// </summary>
        /// <value>a collection of <see cref="T"/>.</value>
        public IEnumerable<T> CachedResults
        {
            get
            {
                string hashcode = GetHashCode().ToString();
                if (DataContext.ResultsCache.ContainsKey(hashcode))
                {
                    return DataContext.ResultsCache[hashcode] as IEnumerable<T>;
                }
                return null;
            }
            set
            {
                string hashcode = GetHashCode().ToString();
                DataContext.ResultsCache[hashcode] = value;
            }
        }

        /// <summary>
        /// A collection of each sub expression that forms the query to be performed
        /// </summary>
        public Dictionary<string, MethodCallExpression> Expressions
        {
            get
            {
                if (expressions == null)
                    expressions = new Dictionary<string, MethodCallExpression>();

                //Console.WriteLine(Environment.StackTrace);
                //Console.WriteLine();

                return expressions;
            }
            set { expressions = value; }
        }

        /// <summary>
        /// A partial expansion of the query into SPARQL. In this case for the FILTER parts of the query.
        /// </summary>
        public string FilterClause { get; set; }

        public string PropertyReferenceTriple { get; set; }

        /// <summary>
        /// Stores the original type that the query was being made against.
        /// </summary>
        public Type OriginalType { get; set; }

        /// <summary>
        /// stores <see cref="MemberInfo"/> details of each property that is referenced inside the query
        /// </summary>
        public HashSet<MemberInfo> QueryGraphParameters { get; set; }

        /// <summary>
        /// stores parameters that are used inside the projection (if any)
        /// </summary>
        public HashSet<MemberInfo> ProjectionParameters { get; set; }

        /// <summary>
        /// Stores the lambda of the select part (if it is performing some kind of projection onto an anonymous type)
        /// </summary>
        public Delegate ProjectionFunction { get; set; }

        /// <summary>
        /// Caches the SPARQL query that was generated when this query was first run.
        /// </summary>
        public string QueryText { get; set; }

        /// <summary>
        /// an abstract class factory that dispenses types appropriate for this kind of query.
        /// This will normally depend on the <see cref="QueryType"/> for this query.
        /// </summary>
        public QueryFactory<T> QueryFactory { get; set; }

        /// <summary>
        /// indicates whether subsequent enumerations of the results should rerun the query, or just use previous results.
        /// </summary>
        public bool ShouldReuseResultset
        {
            get { return shouldReuseResultset; }
            set
            {
                shouldReuseResultset = value;
                // if we stop caching & we already have results then erase them
                if (shouldReuseResultset == false && DataContext.ResultsCache != null)
                {
                    DataContext.ResultsCache.Clear();
                    DataContext.ResultsCache = null;
                }
            }
        }

        /// <summary>
        /// Builds the projection function (<see cref="ProjectionFunction"/>) and extracts the arguments to it 
        /// for use elsewhere.
        /// </summary>
        /// <param name="expression">The Select expression.</param>
        protected void BuildProjection(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
    
            var ue = ((MethodCallExpression) expression).Arguments[1] as UnaryExpression;
            if (ue == null)
                throw new LinqToRdfException("Incompatible expression type found when building ontology projection");
    
            var projectionFunctionExpression = (LambdaExpression) ue.Operand;
            if (projectionFunctionExpression == null)
                throw new LinqToRdfException("Incompatible expression type found when building ontology projection");

            if (Expressions.ContainsKey("GroupBy"))
            {
                throw new NotSupportedException("Group By is not supported by VDS.RDF.Linq");
            }

            // compile the projection's lambda expression into a function that can be used to instantiate new objects
            ProjectionFunction = projectionFunctionExpression.Compile();

            // work out what kind of project is being performed (identity, member or anonymous type)
            // identity projection is from queries like this: "from a in ctx select a"
            // member projection from ones like this "from a in ctx select a.b"
            // anonymous type projection from this "from a in ctx select new {a.b, a.b}"
            if (projectionFunctionExpression.Body is ParameterExpression) //  ie an identity projection
            {
                foreach (PropertyInfo i in OwlClassSupertype.GetAllPersistentProperties(OriginalType))
                {
                    ProjectionParameters.Add(i);
                }
            }
            else if (projectionFunctionExpression.Body is MemberExpression) // a member projection
            {
                var memex = projectionFunctionExpression.Body as MemberExpression;
                if (memex == null) throw new LinqToRdfException("Expected MemberExpression was null");
    
                ProjectionParameters.Add(memex.Member);
            }
            else if (projectionFunctionExpression.Body is NewExpression)
            {
                // create an anonymous type
                var mie = projectionFunctionExpression.Body as NewExpression;
                if (mie == null) throw new LinqToRdfException("Expected NewExpression was not present");

                foreach (MemberExpression me in mie.Arguments)
                {
                    ProjectionParameters.Add(me.Member);
                }
            }
            else if (projectionFunctionExpression.Body is MethodCallExpression)
            {
                throw new NotSupportedException("Calling a method on the selected variable is not supported - use the Select() extension method on the results of the query to achieve this");
            }
            else
            {
                throw new LinqToRdfException("The Projection used in this LINQ expression is not executable by VDS.RDF.Linq");
            }
        }

        public QuerySupertype()
        {
            ProjectionParameters = new HashSet<MemberInfo>();
            QueryGraphParameters = new HashSet<MemberInfo>();
            OriginalType = typeof (T);
        }

        protected Logger Logger  = new Logger(typeof(QuerySupertype<T>));
        public TextWriter Log { get; set; }
    }
}