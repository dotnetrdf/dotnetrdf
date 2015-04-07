using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.Core.Transactions;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Query.Spin.Core
{
    // Perhaps make one of this classes public one day to be consistent with other equivalent .Net APIs
    // => may this be ambiguous because of different processing of query and updates ? 
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
    /// TODO define how to handle temporary resources and execution errors/failures cases
    ///         NOTE: similar temporary resources may occur during a full transaction or a full command
    ///             for optimisation, it may be advisable to create a class to handle such cases to alleviate execution units pre/post processing
    ///             perhaps use a similar framework that is used for caching with dependancies ?
    /// TODO use the SpinStatistics namespace
    /// </remarks>
    public class SparqlCommand
        : SparqlExecutable
    {
        private Connection _connection;
        private SparqlFeaturesProvider _processor;
        private SparqlRewriteStrategyChain _rewriteStrategy = null;

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

        private bool IsRunnable {
            get {
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
                _processor = SparqlFeaturesProvider.Get(value);
                _rewriteStrategy = _processor.GetRewriteStrategyFor(this);
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
            else {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sparqlQuery"></param>
        /// <returns></returns>
        /// TODO envision some query cache ?
        /// TODO embed the query execution with a rdfHandler/sparqlResultHandler to allow for local evaluation of extension functions
        internal object ExecuteReader()
        {
            if (!IsRunnable) throw new InvalidOperationException();
            Prepare();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            object queryResult = null;
            RaiseExecutionStarted(new SparqlExecutableEventArgs());
            try
            {
                foreach (SparqlCommandUnit unit in _executionUnits)
                {
                    queryResult = unit.Execute();
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
            return queryResult;
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
            SparqlCommandUnit updateWrapper = new SparqlCommandUnit(this, query, flags);
            _rewriteStrategy.Rewrite(updateWrapper);
            return updateWrapper;
        }

        internal SparqlCommandUnit CreateUnit(SparqlUpdateCommand update, SparqlExecutableType flags = SparqlExecutableType.SparqlUpdate)
        {
            SparqlCommandUnit updateWrapper = new SparqlCommandUnit(this, update, flags);
            _rewriteStrategy.Rewrite(updateWrapper);
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
    internal class SparqlCommandUnit
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
            CommandType = mode.WithoutFlag(SparqlExecutableType.SparqlUpdate);
            _query = query;
            _defaultGraphs.UnionWith(query.DefaultGraphs);
            _namedGraphs.UnionWith(query.NamedGraphs);
        }

        internal SparqlCommandUnit(SparqlCommand context, SparqlUpdateCommand update, SparqlExecutableType mode)
            : this(context)
        {
            CommandType = mode.WithFlag(SparqlExecutableType.SparqlUpdate);
            _updateCommand = update;
            if (update is BaseModificationCommand)
            {
                _defaultGraphs.Clear();
                _namedGraphs.Clear();
                _defaultGraphs.UnionWith(((BaseModificationCommand)update).UsingUris);
                _namedGraphs.UnionWith(((BaseModificationCommand)update).UsingNamedUris);
            }
        }

        internal SparqlCommandUnit(SparqlCommand context, SparqlUpdateCommand update)
            : this(context, update, SparqlExecutableType.SparqlUpdate)
        { }

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

        public SparqlQuery Query
        {
            get
            {
                return _query;
            }
            internal set
            {
                _query = value;
            }
        }

        public SparqlUpdateCommand UpdateCommand
        {
            get
            {
                return _updateCommand;
            }
            internal set
            {
                _updateCommand = value;
            }
        }

        internal object Execute()
        {
            object queryResult = null;
            RaiseExecutionStarted(new SparqlExecutableEventArgs());
            try
            {
                // Relocate this into a ExecutionStarted event handler
                TransactionLog.Ping(Connection);

                if (CommandType.HasFlag(SparqlExecutableType.SparqlUpdate))
                {
                    ((IUpdateableStorage)Connection.UnderlyingStorage).Update(UpdateCommand.ToString());
                }
                else {
                    // TODO refactor this using a special IRdfHandler and ISparqlResultHandler to allow for streaming and dynamic events dispatch to the monitors
                    // TODO handle custom PropertyFunctions execution
                    queryResult = Connection.UnderlyingStorage.Query(Query.ToString());
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
            return queryResult;
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
