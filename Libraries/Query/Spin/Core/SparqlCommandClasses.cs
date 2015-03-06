using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Spin.Core.Runtime;
using VDS.RDF.Query.Spin.SparqlStrategies;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Query.Spin.Core
{
    // Perhaps make one of this classes public one day to be consistent with other equivalent .Net APIs
    // => may this be ambiguous because of different processing of query and updates ? 

    public enum SparqlCommandType
    {
        SparqlQuery = 0,
        SparqlUpdate = 1,
        SparqlInternal = 2
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class SparqlCommand
    {
        private static SparqlQueryParser _parser = new SparqlQueryParser();

        private String _commandText;
        private Connection _connection;
        private FeaturedSparqlProcessor _processor;
        private SparqlRewriteStrategy _rewriteStrategy = null;

        private List<SparqlCommandUnit> _commands = new List<SparqlCommandUnit>();

        internal SparqlCommand()
        {
        }

        public String CommandText
        {
            get
            {
                return _commandText;
            }
        }

        internal SparqlCommandType CommandType { get; private set; }

        internal IEnumerable<SparqlCommandUnit> Units
        {
            get
            {
                return _commands.AsReadOnly();
            }
        }

        internal Connection Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                _connection = value;
                _processor = FeaturedSparqlProcessor.Get(value);
                _rewriteStrategy = null;
            }
        }

        internal void AddUnit(SparqlUpdateCommand update)
        {
            SparqlCommandUnit command = new SparqlCommandUnit(this, update);
            if (_commands.Count == 0) CommandType = SparqlCommandType.SparqlUpdate;
            _commands.Add(command);
            _rewriteStrategy.Rewrite(command);
        }

        internal void AddUnit(SparqlQuery query)
        {
            SparqlCommandUnit command = new SparqlCommandUnit(this, query);
            if (_commands.Count == 0) CommandType = SparqlCommandType.SparqlQuery;
            _commands.Add(command);
            _rewriteStrategy.Rewrite(command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sparqlQuery"></param>
        /// <returns></returns>
        /// TODO envision some query cache ?
        internal object ExecuteReader(string sparqlQuery)
        {
            _commandText = sparqlQuery;
            if (_connection.State != System.Data.ConnectionState.Open) throw new ConnectionStateException();
            if (_rewriteStrategy == null) _rewriteStrategy = _processor.GetRewriteStrategyFor(this);
            SparqlParameterizedString commandText = new SparqlParameterizedString(sparqlQuery);
            // TODO replace connection env parameters by function calls for correct parsing and query caching possibility
            AddUnit(_parser.ParseFromString(commandText));
            return null;
        }

    }

    /// <summary>
    /// A single command of a Sparql Batch
    /// </summary>
    internal class SparqlCommandUnit
    {
        internal static String ASSERT = "assetions";
        internal static String RETRACT = "removals";

        internal static String ASSERTIONS_PREFIX = "tag:dotnetrdf.org:" + ASSERT + ":";
        internal static String REMOVALS_PREFIX = "tag:dotnetrdf.org:" + RETRACT + ":";

        private String _id = Guid.NewGuid().ToString().Replace("-", "");

        private Dictionary<IToken, IToken> _unitRemovalTokens = new Dictionary<IToken, IToken>();
        private Dictionary<IToken, IToken> _unitAdditionTokens = new Dictionary<IToken, IToken>();

        internal SparqlCommandUnit(SparqlCommand context, SparqlQuery query)
        {
            Context = context;
            CommandType = SparqlCommandType.SparqlQuery;
            Query = query;
        }

        internal SparqlCommandUnit(SparqlCommand context, SparqlUpdateCommand update)
        {
            Context = context;
            CommandType = SparqlCommandType.SparqlUpdate;
            UpdateCommand = update;
        }

        internal String ID
        {
            get
            {
                return _id;
            }
        }

        internal String UnitAssertionsPrefix
        {
            get
            {
                return ASSERTIONS_PREFIX + this.ID + "#";
            }
        }

        internal String UnitRemovalsPrefix
        {
            get
            {
                return REMOVALS_PREFIX + this.ID + "#";
            }
        }

        /// <summary>
        /// Returns the graph this command unit adds assertions to for the given token
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        /// TODO find a way to unify the output with the expression used in the TransactionSupportStrategy
        internal String GetAssertionsGraph(IToken graph = null)
        {
            switch (graph.TokenType)
            {
                case Token.VARIABLE:
                    return graph.Value.Substring(1) + "_" + ASSERT + this.ID;
                case Token.URI:
                    return ASSERTIONS_PREFIX + this.ID + "#" + Uri.EscapeDataString(graph.Value.Substring(1, graph.Value.Length - 2));
                default:
                    throw new ArgumentException("Invalid token. Expected a graph IRI or variable");
            }
        }

        internal String GetRemovalsGraph(IToken graph = null)
        {
            switch (graph.TokenType)
            {
                case Token.VARIABLE:
                    return graph.Value.Substring(1) + "_" + RETRACT + this.ID;
                default:
                    return REMOVALS_PREFIX + this.ID + "#" + Uri.EscapeDataString(graph.Value.Substring(1, graph.Value.Length - 2));
            }
        }

        internal Connection Connection
        {
            get
            {
                return Context.Connection;
            }
            set
            {
                Context.Connection = value;
            }
        }

        internal SparqlCommand Context { get; private set; }
        internal SparqlCommandType CommandType { get; private set; }
        internal SparqlQuery Query { get; private set; }
        internal SparqlUpdateCommand UpdateCommand { get; private set; }

        internal IEnumerable<Uri> DefaultGraphs
        {
            get
            {
                switch (CommandType)
                {
                    case SparqlCommandType.SparqlQuery:
                        return Query.DefaultGraphs;
                    case SparqlCommandType.SparqlUpdate:
                        if (UpdateCommand is BaseModificationCommand)
                        {
                            return ((BaseModificationCommand)UpdateCommand).UsingUris;
                        }
                        break;
                }
                return new List<Uri>();
            }
        }

        internal IEnumerable<Uri> NamedGraphs
        {
            get
            {
                switch (CommandType)
                {
                    case SparqlCommandType.SparqlQuery:
                        return Query.NamedGraphs;
                    case SparqlCommandType.SparqlUpdate:
                        if (UpdateCommand is BaseModificationCommand)
                        {
                            return ((BaseModificationCommand)UpdateCommand).UsingNamedUris;
                        }
                        break;
                }
                return new List<Uri>();
            }
        }

    }
}
