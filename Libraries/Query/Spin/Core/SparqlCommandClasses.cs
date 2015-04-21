using System;
using System.Collections.Generic;
using System.Diagnostics;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

// TODO relocate this into the Runtime namespace ?

namespace VDS.RDF.Query.Spin.Core
{
    // Perhaps make one of this classes public one day to be consistent with other equivalent .Net APIs
    // => may this be ambiguous because of different processing of query and updates ? 

    // TODO check wether we need to make more versatile flags when implementing other rewriting strategies
    [Flags]
    public enum SparqlExecutableType
    {
        SparqlQuery = 0, // the default
        SparqlUpdate = 1,
        SparqlInternal = 2 // Reserved for internal use (currently only PropertyPath compilation; those internal units should be filtered out by some strategies like SPIN...)
    }

    #region Event args and delegates

    public class SparqlExecutableEventArgs : EventArgs
    {
        internal SparqlExecutableEventArgs()
            : base()
        {
        }
    }

    /// <summary>
    /// Delegate Type for SparqlCommand events
    /// </summary>
    /// <param name="sender">Originator of the Event</param>
    /// <param name="args">Triple Event Arguments</param>
    internal delegate void SparqlExecutableEventHandler(Object sender, SparqlExecutableEventArgs args);

    /// <summary>
    /// 
    /// </summary>
    public abstract class SparqlExecutable
        : SparqlTemporaryResourceMediator
    {

        internal event SparqlExecutableEventHandler ExecutionStarted;
        internal event SparqlExecutableEventHandler Failed;
        internal SparqlExecutableEventHandler Succeeded;

        protected void RaiseExecutionStarted(SparqlExecutableEventArgs args)
        {
            SparqlExecutableEventHandler handler = ExecutionStarted;
            if (handler != null)
            {
                handler.Invoke(this, args);
            }
        }

        protected void RaiseExecutionFailed(SparqlExecutableEventArgs args)
        {
            SparqlExecutableEventHandler handler = Failed;
            if (handler != null)
            {
                handler.Invoke(this, args);
            }
        }

        protected void RaiseExecutionSucceeded(SparqlExecutableEventArgs args)
        {
            SparqlExecutableEventHandler handler = Succeeded;
            if (handler != null)
            {
                handler.Invoke(this, args);
            }
        }

        internal abstract Connection Connection { get; set; }

    }

    #endregion

    // TODO perhaps find a better name to avoid confusion with the original Sparql namespace ?

    /// <summary>
    /// The SparqlCommand class acts as a wrapper for ACID execution of several sparql query/update commands.
    /// </summary>
    /// <remarks>
    /// TODO try to postopone rewriting just before execution so we can profit better from dynamic commands events
    /// TODO try to handle eventual parameters passing through a special function that would be evaluated for the query just prior to execution
    /// TODO define how to handle temporary resources and execution errors/failures cases
    ///         NOTE: similar temporary resources may occur during a full transaction or a full command
    ///             for optimisation, it may be advisable to create a class to handle such cases to alleviate execution units pre/post processing
    ///             perhaps use a similar framework that is used for caching with dependancies ?
    /// TODO define a ReturnType/ResultType property to bind the correct result handlers 
    /// TODO use the SpinStatistics namespace
    /// </remarks>
    public class SparqlCommand
        : SparqlExecutable
    {
        private Connection _connection;

        private String _commandText;
        private object _command;
        private bool _isReady = false;

        private List<SparqlCommandUnit> _executionUnits = new List<SparqlCommandUnit>();

        private SparqlCommand(Connection connection)
        {
            Connection = connection;
        }

        internal SparqlCommand(Connection connection, SparqlQuery query)
            : this(connection)
        {
            Command = query;
            CommandType = SparqlExecutableType.SparqlQuery;
            Prepare();
        }

        internal SparqlCommand(Connection connection, SparqlUpdateCommandSet updateSet)
            : this(connection)
        {
            Command = updateSet;
            CommandType = SparqlExecutableType.SparqlUpdate;
            Prepare();
        }

        #region public API

        private bool IsRunnable
        {
            get
            {
                return !String.IsNullOrEmpty(CommandText);
            }
        }

        internal override Connection Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                _isReady = false;
                _connection = value;
                Prepare();
            }
        }

        private object Command
        {
            get
            {
                return _command;
            }
            set
            {
                _command = value;
                if (value != null)
                {
                    _commandText = value.ToString();
                }
                else
                {
                    _commandText = "";
                    CommandType = SparqlExecutableType.SparqlQuery;
                }
            }
        }

        internal SparqlExecutableType CommandType { get; private set; }

        // TODO handle a public set that directly handles the query type ?
        public String CommandText
        {
            get
            {
                return _commandText;
            }
        }

        internal void Prepare()
        {
            if (_isReady || !IsRunnable) return;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            _executionUnits.Clear();
            if (CommandType.HasFlag(SparqlExecutableType.SparqlUpdate))
            {
                foreach (SparqlUpdateCommand update in ((SparqlUpdateCommandSet)Command).Commands)
                {
                    _executionUnits.Add(CreateUnit(update));
                }
                _isReady = true;
            }
            else
            {
                _executionUnits.Add(CreateUnit((SparqlQuery)Command));
                _isReady = true;
            }
            timer.Stop();
            CompilationTime = timer.Elapsed;
        }

        internal override IQueryableStorage UnderlyingStorage
        {
            get
            {
                return Connection.UnderlyingStorage;
            }
        }

        internal override SparqlTemporaryResourceMediator ParentContext
        {
            get
            {
                return Connection;
            }
        }

        internal object ExecuteReader()
        {
            switch (((SparqlQuery)_command).QueryType)
            {
                case SparqlQueryType.Ask:
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    SparqlResultSet results = new SparqlResultSet();
                    ExecuteReader(null, new ResultSetHandler((SparqlResultSet)results));
                    return results;
                case SparqlQueryType.Construct:
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    IGraph g = new Graph();
                    ExecuteReader(new GraphHandler(g), null);
                    return g;
                default:
                    throw new RdfQueryException("Cannot process unknown query types");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sparqlQuery"></param>
        /// <returns></returns>
        internal void ExecuteReader(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler)
        {
            if (!IsRunnable || CommandType.HasFlag(SparqlExecutableType.SparqlUpdate)) throw new InvalidOperationException("This command can not return a reader");
            Prepare();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            RaiseExecutionStarted(new SparqlExecutableEventArgs());
            try
            {
                foreach (SparqlCommandUnit unit in _executionUnits)
                {
                    unit.Execute(rdfHandler, resultsHandler);
                }
                RaiseExecutionSucceeded(new SparqlExecutableEventArgs());
            }
            catch (Exception any)
            {
                RaiseExecutionFailed(new SparqlExecutableEventArgs());
                throw any;
            }
            finally
            {
                RaiseReleased();
                //CleanUp();
            }
            timer.Stop();
            ExecutionTime = timer.Elapsed;
        }

        // Should we return something ?
        internal void ExecuteNonQuery()
        {
            if (!IsRunnable) throw new InvalidOperationException();
            Prepare();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            IUpdateableStorage storage = (IUpdateableStorage)Connection.UnderlyingStorage;
            RaiseExecutionStarted(new SparqlExecutableEventArgs());
            try
            {
                foreach (SparqlCommandUnit unit in _executionUnits)
                {
                    unit.Execute();
                }
                RaiseExecutionSucceeded(new SparqlExecutableEventArgs());
            }
            catch (Exception any)
            {
                RaiseExecutionFailed(new SparqlExecutableEventArgs());
                throw any;
            }
            finally
            {
                RaiseReleased();
                //CleanUp();
            }
            timer.Stop();
            ExecutionTime = timer.Elapsed;
        }

        #endregion

        #region Internal implementation

        internal SparqlCommandUnit CreateUnit(SparqlQuery query, SparqlExecutableType flags = SparqlExecutableType.SparqlQuery)
        {
            SparqlCommandUnit queryWrapper = new SparqlCommandUnit(this, query, flags);
            _connection.StorageProvider.Handle(queryWrapper);
            return queryWrapper;
        }

        internal SparqlCommandUnit CreateUnit(SparqlUpdateCommand update, SparqlExecutableType flags = SparqlExecutableType.SparqlUpdate)
        {
            SparqlCommandUnit updateWrapper = new SparqlCommandUnit(this, update, flags);
            _connection.StorageProvider.Handle(updateWrapper);
            return updateWrapper;
        }

        #endregion

        #region Command statistics

        public TimeSpan? CompilationTime { get; private set; }
        public TimeSpan? ExecutionTime { get; private set; }

        #endregion

    }

    /// <summary>
    /// A single sparql query of update in a SparqlCommand batch
    /// </summary>
    /// TODO some SPARQL queries may be reworked in a way that they required special result handling (thinking of local evaluation of custom PropertyFunctions here that would get the arguments back and compute the result locally)
    /// => we could also do this the other way around to allow for simpler support of Update commands that required those function computations
    ///     thus we also provide a serviceend point here for local computation and wrap the property function call into a SERVICE GraphPattern
    ///     => these function must cannot use blank nodes parameters
    public class SparqlCommandUnit
        : SparqlExecutable
    {

        private readonly Connection _connection;
        private SparqlQuery _query = null;
        private SparqlUpdateCommand _updateCommand;

        private HashSet<Uri> _defaultGraphs = new HashSet<Uri>(RDFHelper.uriComparer);
        private HashSet<Uri> _namedGraphs = new HashSet<Uri>(RDFHelper.uriComparer);

        private SparqlCommandUnit(SparqlCommand context)
        {
            Context = context;
            _connection = context.Connection;
        }

        internal SparqlCommandUnit(SparqlCommand context, SparqlQuery query, SparqlExecutableType mode)
            : this(context)
        {
            CommandType = mode.RemoveFlag(SparqlExecutableType.SparqlUpdate);
            _query = query.CopyWithExplicitVariables();
            _query.PropertyFunctionFactories = new List<IPropertyFunctionFactory>() { SpinModel.Get(Connection) };
            _query.Optimise();

            _defaultGraphs.UnionWith(query.DefaultGraphs);
            _namedGraphs.UnionWith(query.NamedGraphs);
        }

        internal SparqlCommandUnit(SparqlCommand context, SparqlUpdateCommand update, SparqlExecutableType mode)
            : this(context)
        {
            CommandType = mode.SetFlag(SparqlExecutableType.SparqlUpdate);
            _updateCommand = update;
            if (update is BaseModificationCommand)
            {
                _defaultGraphs.UnionWith(((BaseModificationCommand)update).UsingUris);
                _namedGraphs.UnionWith(((BaseModificationCommand)update).UsingNamedUris);
            }
        }

        internal override Connection Connection
        {
            get
            {
                return _connection;
            }
            set
            {
            }
        }

        internal override IQueryableStorage UnderlyingStorage
        {
            get
            {
                return Connection.UnderlyingStorage;
            }
        }

        internal override SparqlTemporaryResourceMediator ParentContext
        {
            get
            {
                return Context;
            }
        }

        internal SparqlCommand Context { get; private set; }

        internal SparqlExecutableType CommandType { get; private set; }

        internal SparqlQuery Query
        {
            get
            {
                return _query;
            }
            set
            {
                _query = value;
            }
        }

        internal SparqlUpdateCommand UpdateCommand
        {
            get
            {
                return _updateCommand;
            }
            set
            {
                _updateCommand = value;
            }
        }

        internal void Execute()
        {
            if (CommandType.HasFlag(SparqlExecutableType.SparqlUpdate))
            {
                Execute(null, null);
            }
        }

        internal void Execute(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler)
        {
            // Handle any parameter from the current connection
            SparqlParameterizedString parameterizedCommand = new SparqlParameterizedString(
                (CommandType.HasFlag(SparqlExecutableType.SparqlUpdate)) ? UpdateCommand.ToString() : Query.ToString()
                );
            foreach (String param in ((Dictionary<String, INode>)parameterizedCommand.Parameters).Keys)
            {
                parameterizedCommand.SetParameter(param, Connection[param]);
            }
            // Do the command execution
            RaiseExecutionStarted(new SparqlExecutableEventArgs());
            try
            {
                // Relocate this into a separated thread ?
                StorageRuntimeMonitor.Ping(Connection);

                if (CommandType.HasFlag(SparqlExecutableType.SparqlUpdate))
                {
                    ((IUpdateableStorage)Connection.UnderlyingStorage).Update(parameterizedCommand.ToString());
                }
                else
                {
                    // TODO handle custom PropertyFunctions evaluation: maybe we can wrap the whole query in a service clause an run a local leviathan engine instead ?
                    //  => problems : 1/ determine the service Uri from a IQueryableStorage object
                    //                2/ ensure that multiple service calls will handle blank nodes correctly
                    // TODO find a way to decouple the client's handlers and any internal required handler
                    Connection.UnderlyingStorage.Query(rdfHandler, resultsHandler, parameterizedCommand.ToString());
                }
                RaiseExecutionSucceeded(new SparqlExecutableEventArgs());
            }
            catch (Exception any)
            {
                RaiseExecutionFailed(new SparqlExecutableEventArgs());
                throw any;
            }
            finally
            {
                RaiseReleased();
            }
        }

        /// <summary>
        /// Gets the Default Graph URIs for the CommandUnit
        /// </summary>
        internal IEnumerable<Uri> DefaultGraphs
        {
            get
            {
                return _defaultGraphs;
                /*return (from u in this._defaultGraphs
                        select u);*/
            }
        }

        /// <summary>
        /// Gets the Named Graph URIs for the CommandUnit
        /// </summary>
        internal IEnumerable<Uri> NamedGraphs
        {
            get
            {
                return _namedGraphs;
                /*return (from u in this._namedGraphs
                        select u);*/
            }
        }

    }
}
