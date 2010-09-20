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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace VDS.RDF.Linq.Sparql
{
    public class LinqToSparqlQuery<T> : QuerySupertype<T>, IRdfQuery<T>
    {
        private IQueryFormatTranslator parser;

        private LinqTripleStore tripleStore;

        public LinqToSparqlQuery(IRdfContext context)
        {
            DataContext = context;
            tripleStore = null;
            OriginalType = typeof (T);
            parser = new LinqToSparqlExpTranslator<T>();
        }

        public IQueryFormatTranslator Parser
        {
            get { return parser; }
            set { parser = value; }
        }

        public LinqTripleStore TripleStore
        {
            get { return tripleStore; }
            set { tripleStore = value; }
        }

        #region IRdfQuery<T> Members

        public Type ElementType
        {
            get { return OriginalType; }
        }

        public Expression Expression
        {
            get { return Expression.Constant(this); }
        }

        public IQueryable<S> CreateQuery<S>(Expression expression)
        {
            Expression exp;
            if (!ExpressionWillCauseInfiniteLoop(expression))
                exp = Evaluator.PartialEval(expression);
            else
                exp = expression;
            LinqToSparqlQuery<S> newQuery = CloneQueryForNewType<S>();

            var call = exp as MethodCallExpression;
            if (call != null)
            {
                newQuery.Expressions[call.Method.Name] = call;
            }
            return newQuery;
        }

        private bool ExpressionWillCauseInfiniteLoop(Expression e)
        {
            MethodCallExpression mce = e as MethodCallExpression;
            if(mce == null) return false;
            switch (mce.Method.Name)
            {
                case "Skip":
                case "Take":
                case "Distinct":
                case "Reduced":
                    return true;
                default:
                    return false;
            }
        }

        public S Execute<S>(Expression expression)
        {
            var e = Enumerable.AsEnumerable(this);
            MethodCallExpression mce = expression as MethodCallExpression;
            object x = null;
            if (mce != null)
            {
                switch(mce.Method.Name)
                {
                    case "Skip":
                        return (S) e.Skip<T>(1);
                    case "Count":
                        x = e.Count();
                        break;
                    case "First":
                        x = e.First();
                        break;
                    case "FirstOrDefault":
                        x = e.FirstOrDefault();
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            return (S)x;
        }

        ///<summary>
        ///Returns an enumerator that iterates through the collection.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return RunQuery();
        }

        ///<summary>
        ///Returns an enumerator that iterates through ontology collection.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public IEnumerator GetEnumerator()
        {
            return RunQuery();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<T>(expression);
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryProvider Provider
        {
            get { return this; }
        }

        #endregion

        protected internal LinqToSparqlQuery<S> CloneQueryForNewType<S>()
        {
            var newQuery = new LinqToSparqlQuery<S>(DataContext);
            newQuery.TripleStore = tripleStore;
            newQuery.OriginalType = OriginalType;
            newQuery.ProjectionFunction = ProjectionFunction;
            newQuery.QueryGraphParameters = QueryGraphParameters;
            newQuery.FilterClause = FilterClause;
            newQuery.Log = externalLogger;
            newQuery.QueryFactory = new QueryFactory<S>(QueryFactory.QueryType, DataContext);
            newQuery.Parser = QueryFactory.CreateExpressionTranslator();
            newQuery.Parser.StringBuilder = new StringBuilder(parser.StringBuilder.ToString());
            newQuery.Expressions = expressions;
            return newQuery;
        }

        protected void ParseQuery(Expression expression, StringBuilder sb)
        {
            Logger.Debug("#Query {0:d}", DateTime.Now);
            StringBuilder tmp = Parser.StringBuilder;
            Parser.StringBuilder = sb;
            Parser.Dispatch(expression);
            Parser.StringBuilder = tmp;
        }

        protected IEnumerator<T> RunQuery()
        {
            if (CachedResults != null && ShouldReuseResultset)
                return CachedResults.GetEnumerator();
            if (QueryText == null)
            {
                var sb = new StringBuilder();
                CreateSelectQuery(sb);
                QueryText = sb.ToString();
            }
            IRdfConnection<T> conn = QueryFactory.CreateConnection(this);
            IRdfCommand<T> cmd = conn.CreateCommand();
            cmd.ElideDuplicates = Expressions.ContainsKey("Distinct") || Expressions.ContainsKey("Reduced");
            cmd.CommandText = QueryText;
            cmd.InstanceName = GetInstanceName();
            return cmd.ExecuteQuery();
        }

        private void CreateSelectQuery(StringBuilder sb)
        {
            if (Expressions.ContainsKey("Where"))
            {
                // first parse the where expression to get the list of parameters to/from the query.
                var sbTmp = new StringBuilder();
                var ue = Expressions["Where"].Arguments[1] as UnaryExpression;
                ParseQuery(ue.Operand, sbTmp);

                if (ExpressionIsObjectPropertyReference(ue.Operand))
                {
                    PropertyReferenceTriple = sbTmp.ToString();
                }
                else
                {
                    //sbTmp now contains the FILTER clause so save it somewhere useful.
                    FilterClause = sbTmp.ToString();
                }
                // now store the parameters where they can be used later on.
                if (Parser.Parameters != null)
                {
                    foreach (MemberInfo item in Parser.Parameters)
                    {
                        QueryGraphParameters.Add(item);
                    }
                }
                // we need to add the original type to the prolog to allow elements of the where clause to be optimised
            }
            CreateProlog(sb);
            CreateProjection(sb);
            CreateDataSetClause(sb); 
            CreateWhereClause(sb);
            CreateSolutionModifier(sb);

            Console.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Works out whether the expression is refering to to some other class via an OWL ObjectProperty.
        /// </summary>
        /// TODO: this documentation needs a clean up
        /// <param name="e">The <see cref="Expression"/> being tested.</param>
        /// <returns>True if <see cref="e"/> refers via a <see cref="MethodCallExpression"/> to a property associated with an OWL ObjectProperty</returns>
        /// <remarks>
        /// This will return true if the Expression is a <see cref="MethodCallExpression"/> that references one of:
        /// <list type="">
        ///   <item>OccursAsStmtObjectWithUri - matches objects with a specific URI</item>
        ///   <item>StmtObjectWithSubjectAndPredicate - matches statements with given subject and predicate URIs</item>
        ///   <item>StmtSubjectWithObjectAndPredicate - matches statements with given object and predicate URIs</item>
        /// </list>
        /// </remarks>
        private bool ExpressionIsObjectPropertyReference(Expression e)
        {
            if (e is LambdaExpression)
            {
                LambdaExpression le = (LambdaExpression) e;
                if (le.Body is MethodCallExpression)
                {
                    MethodCallExpression mce = (MethodCallExpression) le.Body;
                    if (mce.Method.Name == "OccursAsStmtObjectWithUri" ||
                        mce.Method.Name == "StmtObjectWithSubjectAndPredicate" ||
                        mce.Method.Name == "StmtSubjectWithObjectAndPredicate")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Creates the prolog section of a SPARQL query. 
        /// This consists of the prefixes stored in the <see cref="NamespaceManager"/>
        /// </summary>
        /// <param name="sb">A <see cref="StringBuilder"/> onto which the prolog should be appended.</param>
        private void CreateProlog(StringBuilder sb)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (OntologyAttribute ontology in assembly.GetAllOntologies())
                {
                    if (namespaceManager[ontology.Name] != null &&
                        namespaceManager[ontology.Name].BaseUri != ontology.BaseUri)
                    {
                        ontology.Prefix = namespaceManager.CreateNewPrefixFor(ontology);
                    }
                    namespaceManager[ontology.Prefix] = ontology;
                }
            }
            // now insert namespaces needed for the OwlClasses we're working with in this query
            foreach (OntologyAttribute ont in namespaceManager.Ontologies)
            {
                sb.AppendFormat("PREFIX {0}: <{1}>\n", ont.Prefix, ont.BaseUri);
            }
            sb.Append("\n");
        }

        /// <summary>
        /// Creates the data set section of the SPARQL query.
        /// </summary>
        /// TODO: Determine whether the changes to the Data Set clause break the case for named graphs.
        /// <param name="sb">A <see cref="StringBuilder"/> onto which the clause should be appended.</param>
        private void CreateDataSetClause(StringBuilder sb)
        {
#if false   // As was
            // related Issue: #12 http://code.google.com/p/linqtordf/issues/detail?id=12
            string graph = OriginalType.GetOntology().GraphName;
            if (!graph.Empty())
            {
                sb.AppendFormat("FROM NAMED <{0}>\n", graph);
            }
            return; // no named graphs just yet ()
#else
            string defaultGraph = DataContext.DefaultGraph;
            if (!string.IsNullOrEmpty(defaultGraph))
            {
                sb.AppendFormat("FROM <{0}>\n", defaultGraph);
            }
            return;
#endif
        }

        /// <summary>
        /// Creates the projection part of the SPARQL query - the part that specifies which data to return back.
        /// </summary>
        /// <param name="sb">A <see cref="StringBuilder"/> into which the projection is inserted.</param>
        private void CreateProjection(StringBuilder sb)
        {
            string distinct = Expressions.ContainsKey("Distinct") ? "DISTINCT " : String.Empty;
            //Can use REDUCED if Distinct not specified
            if (distinct.Equals(String.Empty))
            {
                if (Expressions.ContainsKey("Reduced")) distinct = "REDUCED ";
            }

            if (Expressions.ContainsKey("Select"))
            {
                BuildProjection(Expressions["Select"]);
            }

            sb.Append("SELECT " + distinct);

            if (Expressions.ContainsKey("GroupBy"))
            {
                throw new NotSupportedException("Group By is not supported by VDS.RDF.Linq");
            }

            // if it is of the form "from a in b select a"
            if (ProjectionParameters.Count == 0)
            {
                foreach (MemberInfo mi in OwlClassSupertype.GetAllPersistentProperties(typeof (T)))
                {
                    sb.Append(" ?");
                    sb.Append(mi.Name);
                }
            }
            else
                // if it is of the form "from a in b select new {a.c, a.d, a.e}" then add c,d,e to the projection
            {
                foreach (MemberInfo mi in ProjectionParameters)
                {
                    sb.Append(" ?");
                    sb.Append(mi.Name);
                }
            }

            // add a parameter to get back the instance URI to allow relationship tracking etc
            sb.AppendLine(" $" + GetInstanceName());
        }

        /// <summary>
        /// Creates the where clause of the SPARQL query - the part that indicates what the properties of selected objects should be.
        /// </summary>
        /// <param name="sb">A <see cref="StringBuilder"/> into which the SPARQL should be inserted.</param>
        private void CreateWhereClause(StringBuilder sb)
        {
            // if using an identity projection then every available property of the type must be returned
            bool isIdentityProjection = OriginalType == typeof (T);
            // if there is no where clause then we want every instance back (and we won't be filtering)
            // the logic around this is a little tricky (or debatable) - 
            // Given that you could have instances that are partially complete in the triple store (i.e. not all triples are present)
            // you need to be able to ensure that a query that does not explicitly include the missing properties does not
            // exclude any instances where those properties are missing.
            // I've reasoned that if you perform an identity projection, then you're saying "get me whatever you can", whereas if 
            // you specifically request a certain property (via a projection) then you really must want a value for that, and thus
            // instances must be excluded where there is no value _known_ - Hence the '|| IsIdentityProjection'.
            bool getAnythingThatYouCan = !(Expressions.ContainsKey("Where")) || isIdentityProjection /* */;
            // using "$" distinguishes this varName from anything that could be introduced from the properties of the type
            // therefore the varName is 'safe' in the sense that there can never be a name clash.
            string varName = "$" + GetInstanceName();

            sb.Append("WHERE {\n");
            // if parameters have been defined somewhere. If using an identity projection then we will not be getting anything from projectionParameters
            // if we have no WHERE expression, then we also won't be getting anything from queryGraphParameters
            var parameters = new List<MemberInfo>(QueryGraphParameters.Union(ProjectionParameters));

            if (parameters.Count == 0)
            {
                // is it an identity projection? If so, place all persistent properties into parameters
                if (isIdentityProjection)
                {
                    foreach (PropertyInfo info in OwlClassSupertype.GetAllPersistentProperties(OriginalType))
                    {
                        parameters.Add(info);
                    }
                }
            }

//            if (parameters.Count > 0)
//            {
                sb.AppendFormat("{0} a {1}:{2} .\n", varName, OriginalType.GetOntology().Prefix,
                                OriginalType.GetOwlResource().RelativeUriReference);
//            }
//            else
//            {
//                // I don't think there is any way to get into to this point unless the object is persistent, but has no 
//                throw new ApplicationException(
//                    "No persistent properties defined on the entity. Unable to generate a query.");
//            }
//
            // temp var to get the object variables list
            IEnumerable<MemberInfo> args;
            // a temp string to get the tripleFormat that will be used to generate query triples.
            string tripleFormat = "OPTIONAL{{{0} {1}:{2} ?{3} .}}\n";

            if (!getAnythingThatYouCan)
                tripleFormat = "{0} {1}:{2} ?{3} .\n";

            if (isIdentityProjection)
                args = OwlClassSupertype.GetAllPersistentProperties(OriginalType);
            else
                args = parameters;

            foreach (MemberInfo arg in args)
            {
                // The ontology and prefix assigned to a class property need not match those assigned to the class itself.
                // e.g. The class could have a property which maps to foaf:name, or dc:title.
                OwlResourceAttribute ora = arg.GetOwlResource();
                sb.AppendFormat(tripleFormat,
                                varName,
                                AttributeExtensions.GetOntologyPrefix(ora.OntologyName), //WAS: originalType.GetOntology().Prefix,
                                ora.RelativeUriReference,
                                arg.Name);
            }

            if (!string.IsNullOrEmpty(PropertyReferenceTriple))
            {
                sb.AppendLine(PropertyReferenceTriple);
            }

            if (!string.IsNullOrEmpty(FilterClause))
            {
                sb.AppendFormat("FILTER( {0} )\n", FilterClause);
            }

            sb.Append("}\n");
        }

        /// <summary>
        /// Gets the alias used in both the LINQ and the SPARQL query for the class instance that is being sought.
        /// </summary>
        /// i.e. if the query was "from a in ctx ..." then the instance name would be "a".
        /// <returns>A string with the name used in the query.</returns>
        private string GetInstanceName()
        {
            if (Expressions.ContainsKey("Where"))
            {
                MethodCallExpression whereExp = Expressions["Where"];
                var ue = (whereExp).Arguments[1] as UnaryExpression;
                var le = (LambdaExpression) ue.Operand;
                ParameterExpression instance = le.Parameters[0];
                if (IsInvalidIdentifier(instance.Name))
                    return CreateValidIdentifier(instance.Name);
                return instance.Name;
            }
            // no name supplied by LINQ so just give one at random.
            return "tmpInt";
        }

        /// <summary>
        /// Used to create a valid identifier in the case where the identifier 
        /// derived from the LINQ query is either non-existent or invalid.
        /// </summary>
        /// <param name="name">The name derived from the LINQ query.</param>
        /// <returns>the same as <see cref="name"/> but with illegal (punctuation) characters removed</returns>
        private string CreateValidIdentifier(string name)
        {
            return name.Replace("<", "")
                       .Replace(">", "")
                       .Replace(":", "")
                       .Replace(",", "")
                       .Replace(".", "")
                       .Replace("{", "")
                       .Replace("}", "");
        }

        /// <summary>
        /// Determines whether <see cref="name"/> contains illegal characters thaqt would make it an invalid identifier for SPARQL.
        /// </summary>
        /// <param name="name">The identifier to be tested.</param>
        /// <returns>
        /// 	<c>true</c> if no invalid chars were found in <see cref="name"/>; otherwise, <c>false</c>.
        /// </returns>
        private bool IsInvalidIdentifier(string name)
        {
            return name.ToCharArray().ContainsAnyOf(new[]{'<','>',':',',','.','{','}'});
        }

        /// <summary>
        /// appends solution modifiers to the SPARQL query.
        /// </summary>
        /// This part handles the generation of 'Order', 'Limit' and 'offset' clauses.
        /// <param name="sb">A <see cref="StringBuilder"/> into which the SPARQL should be inserted.</param>
        private void CreateSolutionModifier(StringBuilder sb)
        {
            CreateOrderClause(sb);
            CreateLimitClause(sb);
            CreateOffsetClause(sb);
        }

        /// <summary>
        /// Inserts an order clause into the SPARQL query if present in the LINQ query.
        /// </summary>
        /// <param name="sb">A <see cref="StringBuilder"/> into which the SPARQL should be inserted.</param>
        private void CreateOrderClause(StringBuilder sb)
        {
            if (Expressions.ContainsKey("OrderBy"))
            {
                MethodCallExpression orderExp = Expressions["OrderBy"];
                var ue = (UnaryExpression) orderExp.Arguments[1];
                var descriminatingFunction = (LambdaExpression) ue.Operand;
                var me = (MemberExpression) descriminatingFunction.Body;
                sb.AppendFormat("ORDER BY ?{0}\n", me.Member.Name);
            }
        }

        /// <summary>
        /// Inserts a limit clause into the SPARQL query if present in the LINQ query.
        /// </summary>
        /// <param name="sb">A <see cref="StringBuilder"/> into which the SPARQL should be inserted.</param>
        private void CreateLimitClause(StringBuilder sb)
        {
            if (Expressions.ContainsKey("Take"))
            {
                MethodCallExpression takeExpression = Expressions["Take"];
                var constantExpression = (ConstantExpression) takeExpression.Arguments[1];
                if (constantExpression.Value != null)
                {
                    sb.AppendFormat("LIMIT {0}\n", constantExpression.Value);
                }
            }
        }

        /// <summary>
        /// Inserts an offset clause into the SPARQL if present in the LINQ query.
        /// </summary>
        /// <param name="sb">A <see cref="StringBuilder"/> into which the SPARQL should be inserted.</param>
        private void CreateOffsetClause(StringBuilder sb)
        {
            if (Expressions.ContainsKey("Skip"))
            {
                MethodCallExpression skipExpression = Expressions["Skip"];
                var constantExpression = (ConstantExpression) skipExpression.Arguments[1];
                if (constantExpression.Value != null)
                {
                    sb.AppendFormat("OFFSET {0}\n", constantExpression.Value);
                }
            }
        }
    }
}
