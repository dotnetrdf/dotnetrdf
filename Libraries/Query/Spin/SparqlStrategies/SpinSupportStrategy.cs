using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.Core.Runtime.Registries;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Query.Spin.SparqlStrategies
{
    /// <summary>
    /// This rewrite Strategy is responsible for :
    ///     => translating SPIN functions and magic properties into portable Sparql 1.1 commands
    ///     => providing evaluation of custom function that are not natively supported by the connection's underlying storage
    ///         2 possibilities here:
    ///             1) provide a local sparql endpoint for non-native functions evaluation usable by remote storages
    ///                 pro: the command can be evaluated completely by the storage
    ///             2) split the query to evaluate as much as possible remotely and get back precompiled multisets for further evaluation
    ///                 cons: requires to get the full resultset locally and possibly send back the results for update queries.
    ///     => handling evaluation of the SPIN features (constructors/rules/constraints) upon commands' completion or transactions' commit
    /// </summary>
    /// <remarks>
    /// The strategy will listen for connection events to check whether the SPIN configuration graphs are updated (by any connection)
    /// and will be responsible for associating the right model to the connection
    /// </remarks>
    public sealed class SpinSupportStrategy
        : ISparqlHandlingStrategy
    {

        private static readonly IUriNode RESOURCE = RDFHelper.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org/spin/sparqlStrategies#SpinSupport"));

        static SpinSupportStrategy()
        {
            // TODO find a way to bind a local Sparql endpoint for remote unsupported functions evaluation
        }

        // Maintain the correspondency between connection/storage and model.
        // TODO use INode instead of String
        private static Dictionary<String, SpinModel> _models = new Dictionary<String, SpinModel>();

        public SpinSupportStrategy()
        {
        }

        #region TODO events handlers
        #endregion

        #region ISparqlSDPlugin members

        public INode Resource
        {
            get
            {
                return RESOURCE;
            }
        }

        public IGraph SparqlSDContribution
        {
            get
            {
                // TODO use the current storage to get the SPIN imported graphs
                IGraph SD_CONTRIBUTION = new Graph();
                SD_CONTRIBUTION.Assert(RESOURCE, RDF.PropertyType, SD.ClassFeature);
                SD_CONTRIBUTION.Assert(RESOURCE, RDF.PropertyType, SD.ClassGraphCollection);
                return SD_CONTRIBUTION;
            }
        }

        #endregion

        public void Handle(SparqlCommandUnit command)
        {
            new CommandRewriter(this, SpinModel.Get(command.Connection)).Rewrite(command);
        }

        #region Event handlers

        /* Connection.Committed
         * => update the graphs using connection's pending additions/removals graphs
         */
        internal void OnConnectionCommitted(Object sender, ConnectionEventArgs args)
        {
        }

        internal void OnCommandSuccess(Object sender, SparqlExecutableEventArgs args)
        {
        }

        #endregion

        /// <summary>
        /// A unitary command rewriter helper to help local context maintenance
        /// </summary>
        /// TODO whether we can define a lightweight base class (if not already provided by dotNetRDF (check with Rob) ?
        private class CommandRewriter
        {
            private SpinSupportStrategy _monitor;
            private SpinModel _model;

            private SparqlCommandUnit _command;
            private SparqlExecutableType _currentMode = SparqlExecutableType.SparqlQuery;

            internal CommandRewriter(SpinSupportStrategy monitor, SpinModel model)
            {
                _monitor = monitor;
                _model = model;
            }

            #region Rewriting methods

            /// <summary>
            /// 
            /// </summary>
            /// TODO handle the all the query/update types
            internal void Rewrite(SparqlCommandUnit command)
            {
                _command = command;
                // TODO minor optimisation : handle this at the end of the rewriting so unused graphs are trimed out (i.e. graphs that are used only in compiled property paths)
                //_namedGraphs.UnionWith(_command.DefaultGraphs.Union(_command.NamedGraphs));

                if (_command.CommandType.HasFlag(SparqlExecutableType.SparqlUpdate))
                {
                    SparqlUpdateCommand sparqlUpdate = _command.UpdateCommand;
                    // TODO tranform all updates into a BaseModifyCommand
                    switch (_command.UpdateCommand.CommandType)
                    {
                        case SparqlUpdateCommandType.Delete:
                        case SparqlUpdateCommandType.Insert:
                        case SparqlUpdateCommandType.Modify:
                        case SparqlUpdateCommandType.InsertData:
                        case SparqlUpdateCommandType.DeleteData:
                            break;
                        case SparqlUpdateCommandType.Add:
                        case SparqlUpdateCommandType.Clear:
                        case SparqlUpdateCommandType.Copy:
                        case SparqlUpdateCommandType.Create:
                        case SparqlUpdateCommandType.Drop:
                        case SparqlUpdateCommandType.Move:
                        // This strategy has nothing to do with the SPARQL GRAPH commands
                        default:
                            return;
                    }
                    BaseModificationCommand modifyCommand = (BaseModificationCommand)_command.UpdateCommand;
                    GraphPattern wherePattern = (GraphPattern)Rewrite(modifyCommand.WherePattern).First();
                    _currentMode = SparqlExecutableType.SparqlUpdate;
                    GraphPattern insertPattern = (GraphPattern)Rewrite(modifyCommand.InsertPattern).FirstOrDefault();
                    GraphPattern deletePattern = (GraphPattern)Rewrite(modifyCommand.DeletePattern).FirstOrDefault();
                    ModifyCommand rewrittenCommand = rewrittenCommand = new ModifyCommand(deletePattern, insertPattern, wherePattern);
                    rewrittenCommand.GraphUri = modifyCommand.GraphUri;
                    _command.UpdateCommand = rewrittenCommand;
                }
                else
                {
                    RewriteQuery(_command.Query);
                }
            }


            private void RewriteQuery(SparqlQuery query)
            {
                query.RootGraphPattern = (GraphPattern)Rewrite(query.RootGraphPattern).First();
                List<object> rewrittenPatterns = new List<object>();
                foreach (SparqlVariable var in query.Variables)
                {
                    rewrittenPatterns.AddRange(Rewrite(var));
                }
                if (query.GroupBy != null)
                {
                    ISparqlGroupBy groupBy = query.GroupBy;
                    while (groupBy != null)
                    {
                        foreach (object rewrittenGroupBy in Rewrite(groupBy.Expression))
                        {
                            if (rewrittenGroupBy is ISparqlExpression)
                            {
                                groupBy.Expression = (ISparqlExpression)rewrittenGroupBy;
                            }
                            else
                            {
                                rewrittenPatterns.Add(rewrittenGroupBy);
                            }
                        }
                        groupBy = groupBy.Child;
                    }
                }
                if (query.Having != null)
                {
                    foreach (object rewrittenClause in Rewrite(query.Having))
                    {
                        if (rewrittenClause is ISparqlFilter)
                        {
                            query.Having = (ISparqlFilter)rewrittenClause;
                        }
                        else
                        {
                            rewrittenPatterns.Add(rewrittenClause);
                        }
                    }
                }
                if (query.OrderBy != null)
                {
                    ISparqlOrderBy orderBy = query.OrderBy;
                    while (orderBy != null)
                    {
                        foreach (object rewrittenClause in Rewrite(orderBy.Expression))
                        {
                            if (rewrittenClause is ISparqlExpression)
                            {
                                orderBy.Expression = (ISparqlExpression)rewrittenClause;
                            }
                            else
                            {
                                rewrittenPatterns.Add(rewrittenClause);
                            }
                        }
                        orderBy = orderBy.Child;
                    }
                }
                foreach (object o in rewrittenPatterns)
                {
                    if (o is GraphPattern)
                    {
                        query.RootGraphPattern.ChildGraphPatterns.Add((GraphPattern)o);
                    }
                    else if (o is ITriplePattern)
                    {
                        query.RootGraphPattern.TriplePatterns.Add((ITriplePattern)o);
                    }
                    else if (o is ISparqlFilter)
                    {
                        // appends the filter to the pattern
                        query.RootGraphPattern.AddFilter((ISparqlFilter)o);
                    }
                }
                query.Optimise();
            }

            public IEnumerable<object> Rewrite(SparqlVariable variable)
            {
                List<object> resultPatterns = new List<object>();
                List<object> rewrittenPatterns = new List<object>();
                if (variable.IsAggregate)
                {
                    rewrittenPatterns.AddRange(Rewrite(variable.Aggregate.Expression));
                }
                else if (variable.IsProjection)
                {
                    rewrittenPatterns.AddRange(Rewrite(variable.Projection));
                }
                foreach (object o in rewrittenPatterns)
                {
                    if (o is ISparqlExpression)
                    {
                        if (variable.IsAggregate)
                        {
                            variable.Aggregate.Expression = (ISparqlExpression)o;
                        }
                        else if (variable.IsProjection)
                        {
                            variable.Projection = (ISparqlExpression)o;
                        }
                    }
                    else
                    {
                        resultPatterns.Add(o);
                    }
                }
                return resultPatterns;
            }

            private IEnumerable<object> Rewrite(GraphPattern pattern)
            {
                // TODO find a way to preserve order of rewritten patterns 
                List<object> rewrittenPatterns = new List<object>();
                // TODO use organization of the graph pattern
                if (pattern == null) return rewrittenPatterns;
                for (int i = 0, l = pattern.ChildGraphPatterns.Count; i < l; i++)
                {
                    rewrittenPatterns.AddRange(Rewrite(pattern.ChildGraphPatterns[i]));
                }
                for (int i = 0, l = pattern.TriplePatterns.Count; i < l; i++)
                {
                    rewrittenPatterns.AddRange(Rewrite(pattern.TriplePatterns[i]));
                }
                foreach (IAssignmentPattern assignment in pattern.UnplacedAssignments)
                {
                    rewrittenPatterns.AddRange(Rewrite(assignment));
                }
                if (pattern.Filter != null)
                {
                    rewrittenPatterns.AddRange(Rewrite(pattern.Filter));
                }
                GraphPattern rewritenPattern = pattern.Clone(true);
                foreach (GraphPattern o in rewrittenPatterns.Where(o => o is GraphPattern).ToList())
                {
                    rewritenPattern.AddGraphPattern((GraphPattern)o);
                }
                foreach (object o in rewrittenPatterns.Where(o => !(o is GraphPattern)))
                {
                    if (o is ITriplePattern)
                    {
                        rewritenPattern.AddTriplePattern((ITriplePattern)o);
                    }
                    else if (o is ISparqlFilter)
                    {
                        // appends the filter to the pattern
                        rewritenPattern.AddFilter((ISparqlFilter)o);
                    }
                }
                return new List<object>() { rewritenPattern };
            }

            private IEnumerable<object> Rewrite(ITriplePattern pattern)
            {
                List<object> transformedPatterns = new List<object>();
                if (pattern is TriplePattern)
                {
                    transformedPatterns.AddRange(Rewrite((TriplePattern)pattern));
                }
                else if (pattern is FilterPattern)
                {
                    transformedPatterns.AddRange(Rewrite(((FilterPattern)pattern).Filter));
                }
                else if (pattern is PropertyFunctionPattern)
                {
                    transformedPatterns.AddRange(Rewrite((PropertyFunctionPattern)pattern));
                }
                else if (pattern is PropertyPathPattern)
                {
                    transformedPatterns.AddRange(Rewrite((PropertyPathPattern)pattern));
                }
                else if (pattern is SubQueryPattern)
                {
                    transformedPatterns.AddRange(Rewrite((SubQueryPattern)pattern));
                }
                else if (pattern is BindingsPattern)
                {
                    transformedPatterns.Add(pattern);
                }
                else if (pattern is IAssignmentPattern)
                {
                    IAssignmentPattern assignmentPattern = (IAssignmentPattern)pattern;
                    // TODO get back the only ISparqlExpression from the set and assign it to the bindPattern
                    IEnumerable<object> rewrittenPatterns = Rewrite(assignmentPattern.AssignExpression);
                    foreach (object rewritten in rewrittenPatterns)
                    {
                        if (rewritten is ISparqlExpression)
                        {
                            assignmentPattern = new BindPattern(assignmentPattern.VariableName, (ISparqlExpression)rewritten);
                        }
                        else
                        {
                            transformedPatterns.Add(rewritten);
                        }

                    }
                    transformedPatterns.Add(assignmentPattern);
                }
                return transformedPatterns;
            }

            private IEnumerable<object> Rewrite(TriplePattern pattern)
            {
                if (pattern.Predicate is NodeMatchPattern)
                {
                    Uri property = ((IUriNode)((NodeMatchPattern)pattern.Predicate).Node).Uri;
                    if (_model.IsMagicPropertyFunction(property))
                    {
                        if (_currentMode == SparqlExecutableType.SparqlUpdate)
                        {
                            // TODO use the right exception type
                            throw new NotSupportedException("PropertyFunction can not be used in INSERT or DELETE patterns.");
                        }
                        SpinMagicPropertyCall spinFunction = _model.GetMagicPropertyCall(property);
                        return Rewrite(new PropertyFunctionPattern(new List<ITriplePattern>() { pattern }, new List<PatternItem>() { pattern.Subject }, new List<PatternItem>() { pattern.Object }, spinFunction));
                    }
                }
                List<object> transformedPatterns = new List<object>();
                transformedPatterns.Add(pattern);
                return transformedPatterns;
            }

            private IEnumerable<object> Rewrite(ISparqlFilter pattern)
            {
                List<object> transformedPatterns = new List<object>();
                IEnumerable<object> rewrittenPatterns = Rewrite(pattern.Expression);
                // TODO get back the only ISparqlExpression from the set and replace it into the pattern
                foreach (object rewritten in rewrittenPatterns)
                {
                    if (rewritten is ISparqlExpression)
                    {
                        transformedPatterns.Add(new UnaryExpressionFilter((ISparqlExpression)rewritten));
                    }
                    else
                    {
                        transformedPatterns.Add(rewritten);
                    }

                }
                return transformedPatterns;
            }

            /// <summary>
            /// Rewrites property path patterns with property functions preprocessing if needed
            /// </summary>
            /// <param name="pattern"></param>
            /// <returns></returns>
            private IEnumerable<object> Rewrite(PropertyPathPattern pattern)
            {
                List<object> transformedPatterns = new List<object>();
                if (pattern.Path is Property)
                {
                    Uri property = ((IUriNode)((Property)pattern.Path).Predicate).Uri;
                    SpinMagicPropertyCall mp = _model.GetMagicPropertyCall(property);
                    if (mp != null)
                    {
                        PropertyFunctionPattern subPattern = new PropertyFunctionPattern(new PropertyFunctionInfo(property), mp);
                        ((List<PatternItem>)subPattern.SubjectArgs).Add(pattern.Subject);
                        ((List<PatternItem>)subPattern.ObjectArgs).Add(pattern.Object);
                        transformedPatterns.AddRange(Rewrite(subPattern));
                    }
                    else
                    {
                        transformedPatterns.Add(pattern);
                    }
                }
                else if (pattern.Path is SequencePath)
                {
                    SequencePath stepPath = (SequencePath)pattern.Path;
                    VariablePattern partialStep = new VariablePattern(RDFHelper.CreateTempVariableNode().VariableName);
                    transformedPatterns.AddRange(Rewrite(new PropertyPathPattern(pattern.Subject, stepPath.LhsPath, partialStep)));
                    transformedPatterns.AddRange(Rewrite(new PropertyPathPattern(partialStep, stepPath.RhsPath, pattern.Object)));
                }
                else if (pattern.Path is InversePath)
                {
                    InversePath invPath = (InversePath)pattern.Path;
                    Uri property = ((IUriNode)((Property)invPath.Path).Predicate).Uri;
                    if (_model.IsMagicPropertyFunction(property))
                    {
                        // TODO add a compiled graph for PropertyFunctions and adds it to the command set
                        // TODO should we check beforehand that the property function returns a Node ?
                        throw new NotSupportedException("Cannot process Cardinality on PropertyFunctions");
                    }
                }
                else if (pattern.Path is NegatedSet)
                {
                    NegatedSet negSet = (NegatedSet)pattern.Path;
                }
                else if (pattern.Path is AlternativePath)
                {
                    AlternativePath altPath = (AlternativePath)pattern.Path;
                    // TODO evaluate each alternative tho check whether if is a PropertyFunction
                    // create a alternate path for direct properties and union it with PropertyFunctions subqueries
                }
                else if (pattern.Path is Cardinality)
                {
                    Cardinality cardPath = (Cardinality)pattern.Path;
                    Uri property = ((IUriNode)((Property)cardPath.Path).Predicate).Uri;
                    if (_model.IsMagicPropertyFunction(property))
                    {
                        // TODO add a compiled graph for PropertyFunctions
                        // TODO should we check beforehand that the property function returns a Node ?
                        throw new NotSupportedException("Cannot yet process Cardinality on PropertyFunctions");
                    }
                    transformedPatterns.Add(pattern);
                }
                return transformedPatterns;
            }

            private IEnumerable<object> Rewrite(PropertyFunctionPattern pattern)
            {
                SpinMagicPropertyCall mpCall = (SpinMagicPropertyCall)pattern.PropertyFunction;
                // TODO handle this for UpdatesCommands rewriting
                //if (currentTranslationMode > SparqlTranslationMode.Query)
                //{
                //    if (currentTranslationMode != SparqlTranslationMode.Internal)
                //    {
                //        throw new SpinException("PropertyFunction " + _formatter.FormatUri(spinFunction.FunctionUri) + " can not be used as predicate in INSERT/DELETE patterns.");
                //    }
                //}
                List<object> transformedPatterns = new List<object>();
                // TODO find a way to handle both collections of subjectArgs and objectArgs
                if (pattern.SubjectArgs.Count() > 1)
                {
                    throw new NotSupportedException("Multiple subject arguments are not yet supported for magic properties");
                }
                if (pattern.ObjectArgs.OfType<BlankNodePattern>().Any() || pattern.ObjectArgs.OfType<FixedBlankNodePattern>().Any())
                {
                    throw new NotSupportedException("Magic properties arguments cannot be blank nodes");
                }
                SubQueryPattern subQuery = mpCall.ToSparql(pattern.SubjectArgs.First().AsEnumerable().Union(pattern.ObjectArgs).ToList());
                Rewrite(subQuery);
                transformedPatterns.Add(subQuery);
                return transformedPatterns;
            }

            private IEnumerable<object> Rewrite(SubQueryPattern pattern)
            {
                SparqlQuery subQuery = pattern.SubQuery;
                RewriteQuery(subQuery);
                List<object> transformedPatterns = new List<object>() { pattern };
                return transformedPatterns;
            }

            public IEnumerable<object> Rewrite(ISparqlExpression expression)
            {
                List<object> transformedPatterns = new List<object>();
                switch (expression.Type)
                {
                    case SparqlExpressionType.Function:
                        Uri functionUri;
                        // First rewrite arguments
                        List<ISparqlExpression> funcArgs = (List<ISparqlExpression>)expression.Arguments.ToList();
                        for (int i = 0, l = funcArgs.Count; i < l; i++)
                        {
                            foreach (object pattern in Rewrite(funcArgs[i]))
                            {
                                if (pattern is ISparqlExpression)
                                {
                                    funcArgs[i] = (ISparqlExpression)pattern;
                                }
                                else
                                {
                                    transformedPatterns.Add(pattern);
                                }
                            }
                        }
                        SpinFunctionCall spinFunction = null;
                        // TODO handle locally implemented extension functions
                        if (expression is UnknownFunction && Uri.TryCreate(expression.Functor, UriKind.Absolute, out functionUri))
                        {
                            spinFunction = _model.GetSpinFunctionCall(functionUri);
                            if (spinFunction == null)
                            {
                                // TODO raise an event to alert the engine that an unknown function is called
                                // it would be the responsibility of the listener to decide wether the query can still be evaluated safely or not.
                            }
                        }
                        if (spinFunction == null)
                        {
                            expression.Arguments = funcArgs;
                            transformedPatterns.Add(expression);
                            break;
                        }
                        else
                        {
                            for (int i = 0, l = funcArgs.Count; i < l; i++)
                            {
                                ISparqlExpression arg = funcArgs[i];
                                if (arg.Type != SparqlExpressionType.Primary)
                                {
                                    String subsituteArg = RDFHelper.CreateTempVariableNode().VariableName;
                                    transformedPatterns.Add(new BindPattern(subsituteArg, funcArgs[i]));
                                    funcArgs[i] = new VariableTerm(subsituteArg);
                                }
                            }
                            spinFunction.Arguments = funcArgs;
                            object subQuery = spinFunction.ToSparql();
                            if (subQuery != null)
                            {
                                transformedPatterns.Add(new VariableTerm(spinFunction.ResultVariable));
                                if (subQuery is SubQueryPattern)
                                {
                                    Rewrite((SubQueryPattern)subQuery);
                                    // TODO perform variable substitution
                                    transformedPatterns.Add(subQuery);
                                }
                                else
                                {
                                    transformedPatterns.Add(subQuery);
                                }
                            }
                            else
                            {
                                transformedPatterns.Add(spinFunction);
                            }
                        }
                        break;
                    case SparqlExpressionType.GraphOperator:
                        Rewrite(((Expressions.Primary.GraphPatternTerm)expression.Arguments.First()).Pattern);
                        transformedPatterns.Add(expression);
                        break;
                    case SparqlExpressionType.Primary:
                        transformedPatterns.Add(expression);
                        break;
                    case SparqlExpressionType.Aggregate:
                    case SparqlExpressionType.UnaryOperator:
                        List<ISparqlExpression> opArg = (List<ISparqlExpression>)expression.Arguments.ToList();
                        for (int i = 0, l = opArg.Count; i < l; i++)
                        {
                            foreach (object pattern in Rewrite(opArg[i]))
                            {
                                if (pattern is ISparqlExpression)
                                {
                                    opArg[i] = (ISparqlExpression)pattern;
                                }
                                else
                                {
                                    transformedPatterns.Add(pattern);
                                }
                            }
                        }
                        ((BaseUnaryExpression)expression).Arguments = opArg;
                        transformedPatterns.Add(expression);
                        break;
                    case SparqlExpressionType.BinaryOperator:
                        List<ISparqlExpression> opArgs = (List<ISparqlExpression>)expression.Arguments.ToList();
                        for (int i = 0, l = opArgs.Count; i < l; i++)
                        {
                            foreach (object pattern in Rewrite(opArgs[i]))
                            {
                                if (pattern is ISparqlExpression)
                                {
                                    opArgs[i] = (ISparqlExpression)pattern;
                                }
                                else
                                {
                                    transformedPatterns.Add(pattern);
                                }
                            }
                        }
                        ((BaseBinaryExpression)expression).Arguments = opArgs;
                        transformedPatterns.Add(expression);
                        break;
                    case SparqlExpressionType.SetOperator:
                        foreach (ISparqlExpression arg in expression.Arguments.Skip(1))
                        {
                            transformedPatterns.AddRange(Rewrite(arg));
                        }
                        break;
                    default:
                        transformedPatterns.Add(expression);
                        break;
                }
                return transformedPatterns;
            }

            #endregion

        }

    }

}
