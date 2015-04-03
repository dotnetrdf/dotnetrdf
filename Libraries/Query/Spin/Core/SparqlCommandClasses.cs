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
    public enum SparqlCommandType
    {
        Unknown = 0,
        SparqlQuery = 1,
        SparqlUpdate = 2,
        SparqlInternal = 4
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

    public abstract class SparqlExecutable
        : BaseTemporaryGraphConsumer
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
            CommandType = SparqlCommandType.SparqlQuery;
            Prepare();
        }

        internal SparqlCommand(Connection connection, SparqlUpdateCommandSet updateSet)
            : this(connection)
        {
            Command = updateSet;
            CommandType = SparqlCommandType.SparqlUpdate;
            Prepare();
        }

        #region public API

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
                    CommandType = SparqlCommandType.Unknown;
                }
            }
        }

        internal SparqlCommandType CommandType { get; private set; }

        // TODO handle a public set ?
        public String CommandText
        {
            get
            {
                return _commandText;
            }
        }

        internal void Prepare()
        {
            if (_isReady) return;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            _executionUnits.Clear();
            switch (CommandType)
            {
                case SparqlCommandType.SparqlQuery:
                    SparqlCommandUnit queryWrapper = new SparqlCommandUnit(this, (SparqlQuery)Command);
                    _executionUnits.Add(queryWrapper);
                    _rewriteStrategy.Rewrite(queryWrapper);
                    _isReady = true;
                    break;
                case SparqlCommandType.SparqlUpdate:
                    foreach (SparqlUpdateCommand update in ((SparqlUpdateCommandSet)Command).Commands)
                    {
                        SparqlCommandUnit updateWrapper = new SparqlCommandUnit(this, update);
                        _executionUnits.Add(updateWrapper);
                        _rewriteStrategy.Rewrite(updateWrapper);
                    }
                    _isReady = true;
                    break;
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

        internal override BaseTemporaryGraphConsumer ParentContext
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
                RaiseDisposable();
                //CleanUp();
            }
            timer.Stop();
            ExecutionTime = timer.Elapsed;
            return queryResult;
        }

        // Should we return something ?
        internal void ExecuteNonQuery()
        {
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
                RaiseDisposable();
                //CleanUp();
            }
            timer.Stop();
            ExecutionTime = timer.Elapsed;
        }

        #endregion

        #region Internal implementation

        // This is not really clean
        internal SparqlCommandUnit CreateInternalUnit(SparqlUpdateCommand update)
        {
            SparqlCommandUnit command = new SparqlCommandUnit(this, update, SparqlCommandType.SparqlInternal | SparqlCommandType.SparqlUpdate);
            _rewriteStrategy.Rewrite(command);
            return command;
        }

        #endregion

        #region Command statistics

        public TimeSpan? CompilationTime { get; private set; }
        public TimeSpan? ExecutionTime { get; private set; }

        #endregion

    }

    /// <summary>
    /// A single internal command of a Sparql ACID batch
    /// </summary>
    internal class SparqlCommandUnit
        : SparqlExecutable
    {

        private readonly Connection _connection;
        private SparqlQuery _query = null;
        private SparqlUpdateCommand _updateCommand;

        private List<SparqlCommandUnit> _preProcessingUnits = new List<SparqlCommandUnit>();

        private HashSet<Uri> _defaultGraphs = new HashSet<Uri>(RDFHelper.uriComparer);
        private HashSet<Uri> _namedGraphs = new HashSet<Uri>(RDFHelper.uriComparer);

        private SparqlCommandUnit(SparqlCommand context)
        {
            //Uri = UriFactory.Create(BaseTemporaryGraphConsumer.NS_URI + "command-unit:" + ID);
            Context = context;
            _connection = context.Connection;
        }

        internal SparqlCommandUnit(SparqlCommand context, SparqlQuery query)
            : this(context)
        {
            CommandType = SparqlCommandType.SparqlQuery;
            _query = query;
            _defaultGraphs.UnionWith(query.DefaultGraphs);
            _namedGraphs.UnionWith(query.NamedGraphs);
        }

        internal SparqlCommandUnit(SparqlCommand context, SparqlUpdateCommand update, SparqlCommandType mode)
            : this(context)
        {
            CommandType = mode;
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
            : this(context, update, SparqlCommandType.SparqlUpdate)
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

        internal override BaseTemporaryGraphConsumer ParentContext
        {
            get
            {
                return Context;
            }
        }

        internal SparqlCommand Context { get; private set; }

        internal SparqlCommandType CommandType { get; private set; }

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

                foreach (SparqlCommandUnit pre in PreProcessingUnits)
                {
                    pre.Execute();
                }
                // TODO add the named and default graphs into the transaction log
                if (CommandType.HasFlag(SparqlCommandType.SparqlQuery))
                {
                    queryResult = Connection.UnderlyingStorage.Query(Query.ToString());
                }
                else
                {
                    ((IUpdateableStorage)Connection.UnderlyingStorage).Update(UpdateCommand.ToString());
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
                RaiseDisposable();
                //CleanUp();
            }
            return queryResult;
        }

        /// <summary>
        /// Returns the list of preprocessing commands required to evaluate this unit
        /// </summary>
        /// <remarks>Eventually this list may be used by the full command to allow results caching during the whole processing</remarks>
        /// <returns></returns>
        /// TODO refactor this as a single CompilationUnit
        internal IEnumerable<SparqlCommandUnit> PreProcessingUnits
        {
            get
            {
                return (from u in _preProcessingUnits
                        select u);
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

        internal void AddPreProcessingUnit(SparqlParameterizedString update)
        {
            AddPreProcessingUnit(new SparqlUpdateParser().ParseFromString(update).Commands.First());
        }

        internal void AddPreProcessingUnit(SparqlUpdateCommand update)
        {
            SparqlCommandUnit command = Context.CreateInternalUnit(update);
            _preProcessingUnits.Add(command);
        }

    }
}
