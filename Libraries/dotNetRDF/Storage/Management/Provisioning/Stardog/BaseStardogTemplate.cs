/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Storage.Management.Provisioning.Stardog
{
    /// <summary>
    /// Abstract base implementation of a Store Template for creating Stardog Stores
    /// </summary>
    public abstract class BaseStardogTemplate
        : StoreTemplate
    {
        /// <summary>
        /// Creates a new Stardog Template
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="name">Template Name</param>
        /// <param name="descrip">Template Description</param>
        /// <param name="dbtype">Stardog Database Type</param>
        public BaseStardogTemplate(String id, String name, String descrip, String dbtype)
            : base(id, name, descrip)
        {
            // Index Options
            DatabaseType = dbtype;
            MinDifferentialIndexLimit = BaseStardogServer.DatabaseOptions.DefaultMinDifferentialIndexLimit;
            MaxDifferentialIndexLimit = BaseStardogServer.DatabaseOptions.DefaultMaxDifferentialIndexLimit;
            CanoncialiseLiterals = BaseStardogServer.DatabaseOptions.DefaultCanonicaliseLiterals;
            IndexNamedGraphs = BaseStardogServer.DatabaseOptions.DefaultNamedGraphIndexing;
            PersistIndexes = BaseStardogServer.DatabaseOptions.DefaultPersistIndex;
            PersistIndexesSynchronously = BaseStardogServer.DatabaseOptions.DefaultPersistIndexSync;
            AutoUpdateStatistics = BaseStardogServer.DatabaseOptions.DefaultAutoUpdateStats;

            // Integrity Constraint Validation
            IcvActiveGraphs = new List<string>();
            IcvEnabled = BaseStardogServer.DatabaseOptions.DefaultIcvEnabled;
            IcvReasoningMode = BaseStardogServer.DatabaseOptions.DefaultIcvReasoningMode;

            // Reasoning
            ConsistencyChecking = BaseStardogServer.DatabaseOptions.DefaultConsistencyChecking;
            EnablePunning = BaseStardogServer.DatabaseOptions.DefaultPunning;
            SchemaGraphs = new List<string>();

            // Search
            FullTextSearch = BaseStardogServer.DatabaseOptions.DefaultFullTextSearch;
            SearchReindexMode = BaseStardogServer.DatabaseOptions.SearchReIndexModeAsync;

            // Transactions
            DurableTransactions = BaseStardogServer.DatabaseOptions.DefaultDurableTransactions;
        }

        #region Index Options

        // index.differential.enable.limit
        // Sets the minimum size of the Stardog database before differential indexes are used.
        // index.differential.merge.limit
        // Sets the size in number of RDF triples before the differential indexes are merged to the main indexes.
        // index.literals.canonical
        // Enables RDF literal canonicalization. See literal canonicalization for details.
        // index.named.graphs
        // Enables optimized index support for named graphs; speeds SPARQL query evaluation with named graphs at the cost of some overhead for database loading and index maintenance.
        // index.persist
        // Enables persistent indexes.
        // index.persist.sync
        // Enables whether memory indexes are synchronously or asynchronously persisted to disk with respect to a transaction.
        // index.statistics.update.automatic
        // Sets whether statistics are maintained automatically.
        // index.type
        // Sets the index type (memory or disk).

        /// <summary>
        /// Gets the Database Type
        /// </summary>
        [Category("Index Options"), 
         DisplayName("Index Type"),
         Description("The type of the index structures used for the database")]
        public String DatabaseType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/Sets the minimum differential index limit
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultMinDifferentialIndexLimit), 
         Category("Index Options"), 
         DisplayName("Differential Index Enabled Limit"), 
         Description("The minimum size the Stardog database must be before differential indexes are used")]
        public int MinDifferentialIndexLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the maximum differential merge limit
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultMaxDifferentialIndexLimit), 
         Category("Index Options"),
         DisplayName("Differential Index Merge Limit"), 
         Description("The maximum size in triples of the differential index before it is merged into the main index")]
        public int MaxDifferentialIndexLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether the database should canonicalise literals
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultCanonicaliseLiterals),
         Category("Index Options"),
         DisplayName("Canonicalise Literals"),
         Description("Sets whether literals are canonicalised before being indexed.  If enabled then literals will be transformed e.g. '1'^^xsd:byte => '1'^^xsd:integer, leave disabled to preserve data exactly as input")]
        public bool CanoncialiseLiterals
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to optimize indexes for named graph queries
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultNamedGraphIndexing),
         Category("Index Options"),
         DisplayName("Index Named Graphs"),
         Description("Enables optimized index support for named graphs, improves query performance at the cost of load performance.  If your data is all in one graph or you infrequently query named graphs you may wish to disable this")]
        public bool IndexNamedGraphs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to persist indexes
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultPersistIndex),
         Category("Index Options"),
         DisplayName("Persistent Indexes"),
         Description("Sets whether indexes are persistent")]
        public bool PersistIndexes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to persist indexes synchronously
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultPersistIndexSync),
         Category("Index Options"),
         DisplayName("Persist Indexes Synchronously"),
         Description("Sets whether indexes are persisted synchronously or asynchronously")]
        public bool PersistIndexesSynchronously
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to automatically update statistics
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultAutoUpdateStats),
         Category("Index Options"),
         DisplayName("Auto Update Statistics"),
         Description("Sets whether statistics are automatically updated")]
        public bool AutoUpdateStatistics
        {
            get;
            set;
        }

        #endregion

        #region Integrity Constraint Validation Options

        // icv.active.graphs
        // Specifies which part of the database, in terms of named graphs, is checked with IC validation. to validate all the named graphs in the database.
        // icv.enabled
        // Determines whether ICV is active for the database; if true, all database mutations are subject to IC validation (i.e., "guard mode").
        // icv.reasoning-type
        // Determines what "reasoning level" is used during IC validation.

        /// <summary>
        /// Gets/Sets the active graphs for ICV
        /// </summary>
        [Category("Integrity Constraint Validation"),
         DisplayName("Active Graphs"), 
         Description("Sets the named graphs upon which integrity constraints are enforced")]
        public List<String> IcvActiveGraphs
        {
            get;
            set;
        }

        /// <summary>
        /// Enables/Disables ICV
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultIcvEnabled),
         Category("Integrity Constraint Validation"), 
         DisplayName("Enabled"), 
         Description("Enables integrity constraint validation for the database")]
        public bool IcvEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the reasoning mode for ICV
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultIcvReasoningMode),
         Category("Integrity Constraint Validation"),
         DisplayName("Reasoning Mode"), 
         Description("Sets what reasoning mode is used during integrity constraint validation")]
        public StardogReasoningMode IcvReasoningMode
        {
            get;
            set;
        }


        #endregion

        #region Reasoning Options
        // reasoning.consistency.automatic
        // Enables automatic consistency checking with respect to a transaction.
        // reasoning.punning.enabled
        // Enables punning.
        // reasoning.schema.graphs
        // Determines which, if any, named graph or graphs contains the "tbox", i.e., the schema part of the data.

        /// <summary>
        /// Gets/Sets whether to perform automatic consistency checking on transactions
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultConsistencyChecking),
         Category("Reasoning Options"),
         DisplayName("Automatic Consistency Checking"),
         Description("Sets whether consistency checking is done with respect to transactions")]
        public bool ConsistencyChecking
        {
            get;
            set;
        }

        /// <summary>
        /// Enables/Disables punning
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultPunning),
         Category("Reasoning Options"),
         DisplayName("Enable Punning")
        ]
        public bool EnablePunning
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the graphs that contain the schema (TBox) that are used for reasoning
        /// </summary>
        [Category("Reasoning Options"),
         DisplayName("Schema Graphs"),
         Description("Sets the graphs considered to contain the schema (TBox) used for reasoning")]
        public List<String> SchemaGraphs
        {
            get;
            set;
        }


        #endregion

        #region Search Options

        // search.enabled
        // Enables semantic search on the database.
        // search.reindex.mode
        // Sets how search indexes are maintained.

        /// <summary>
        /// Enables/Disables Full Text search
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultFullTextSearch),
         Category("Search Options"),
         DisplayName("Enable Full Text Search"),
         Description("Enables full text search")]
        public bool FullTextSearch
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Search re-indexing mode
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.SearchReIndexModeAsync),
         Category("Search Options"),
         DisplayName("Search Re-index Mode"),
         Description("Controls when search indexes are re-indexed, valid values are sync or async")]
        public String SearchReindexMode
        {
            get;
            set;
        }

        #endregion

        #region Transaction Options
        // transactions.durable
        // Enables durable transactions.

        /// <summary>
        /// Gets/Sets whether to use durable transactions
        /// </summary>
        [DefaultValue(BaseStardogServer.DatabaseOptions.DefaultDurableTransactions),
         Category("Transaction Options"),
         DisplayName("Durable Transactions"),
         Description("Enables durable transactions")]
        public bool DurableTransactions
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Validates that the template is filled out such that a store can be created from it
        /// </summary>
        /// <returns>Enumeration of errors that occurred</returns>
        public override IEnumerable<string> Validate()
        {
            List<String> errors = new List<string>();
            if (!BaseStardogServer.DatabaseOptions.IsValidDatabaseName(ID))
            {
                errors.Add("Database Name " + ID + " is invalid, Stardog database names must match the regular expression " + BaseStardogServer.DatabaseOptions.ValidDatabaseNamePattern);
            }
            if (!BaseStardogServer.DatabaseOptions.IsValidDatabaseType(DatabaseType))
            {
                errors.Add("Database Type " + DatabaseType + " is invalid");
            }
            if (!BaseStardogServer.DatabaseOptions.IsValidSearchReIndexMode(SearchReindexMode))
            {
                errors.Add("Search Re-index Mode " + SearchReindexMode + " is invalid, only sync or async are currently permitted");
            }
            foreach (String uri in SchemaGraphs)
            {
                if (!BaseStardogServer.DatabaseOptions.IsValidNamedGraph(uri))
                {
                    errors.Add("Schema Graphs contains invalid Graph URI '" + uri + "' - must use a valid URI, default or *");
                }
            }
            foreach (String uri in IcvActiveGraphs)
            {
                if (!BaseStardogServer.DatabaseOptions.IsValidNamedGraph(uri))
                {
                    errors.Add("ICV Active Graphs contains invalid Graph URI '" + uri + "' - must use a valid URI, default or *");
                }
            }
            ValidateInternal(errors);
            return errors;
        }

        /// <summary>
        /// Does any additional validation a derived template may require
        /// </summary>
        /// <param name="errors">Error collection to add to</param>
        protected virtual void ValidateInternal(List<String> errors) { }

        /// <summary>
        /// Gets the JSON Template for creating a store
        /// </summary>
        /// <returns></returns>
        public JObject GetTemplateJson()
        {
            // Set up the basic template
            JObject template = new JObject();
            template.Add("dbname", new JValue(ID));

            // Build up the options object
            // Don't bother included non-required options if the user hasn't adjusted them from their defaults
            JObject options = new JObject();

            // Index Options
            options.Add(BaseStardogServer.DatabaseOptions.IndexType, new JValue(DatabaseType.ToLower()));
            if (MinDifferentialIndexLimit != BaseStardogServer.DatabaseOptions.DefaultMinDifferentialIndexLimit) options.Add(BaseStardogServer.DatabaseOptions.IndexDifferentialEnableLimit, new JValue(MinDifferentialIndexLimit));
            if (MaxDifferentialIndexLimit != BaseStardogServer.DatabaseOptions.DefaultMaxDifferentialIndexLimit) options.Add(BaseStardogServer.DatabaseOptions.IndexDifferentialMergeLimit, new JValue(MaxDifferentialIndexLimit));
            if (CanoncialiseLiterals != BaseStardogServer.DatabaseOptions.DefaultCanonicaliseLiterals) options.Add(BaseStardogServer.DatabaseOptions.IndexLiteralsCanonical, new JValue(CanoncialiseLiterals));
            if (IndexNamedGraphs != BaseStardogServer.DatabaseOptions.DefaultNamedGraphIndexing) options.Add(BaseStardogServer.DatabaseOptions.IndexNamedGraphs, new JValue(IndexNamedGraphs));
            if (PersistIndexes != BaseStardogServer.DatabaseOptions.DefaultPersistIndex) options.Add(BaseStardogServer.DatabaseOptions.IndexPersistTrue, new JValue(PersistIndexes));
            if (PersistIndexesSynchronously != BaseStardogServer.DatabaseOptions.DefaultPersistIndexSync) options.Add(BaseStardogServer.DatabaseOptions.IndexPersistSync, new JValue(PersistIndexesSynchronously));
            if (AutoUpdateStatistics != BaseStardogServer.DatabaseOptions.DefaultAutoUpdateStats) options.Add(BaseStardogServer.DatabaseOptions.IndexStatisticsAutoUpdate, new JValue(AutoUpdateStatistics));

            // ICV Options
            if (IcvActiveGraphs.Count > 0) options.Add(BaseStardogServer.DatabaseOptions.IcvActiveGraphs, new JValue(String.Join(",", IcvActiveGraphs.ToArray())));
            if (IcvEnabled != BaseStardogServer.DatabaseOptions.DefaultIcvEnabled) options.Add(BaseStardogServer.DatabaseOptions.IcvEnabled, new JValue(IcvEnabled));
            if (IcvReasoningMode != BaseStardogServer.DatabaseOptions.DefaultIcvReasoningMode) options.Add(BaseStardogServer.DatabaseOptions.IcvReasoningType, new JValue(IcvReasoningMode.ToString()));
            
            // Reasoning
            if (ConsistencyChecking != BaseStardogServer.DatabaseOptions.DefaultConsistencyChecking) options.Add(BaseStardogServer.DatabaseOptions.ReasoningAutoConsistency, new JValue(ConsistencyChecking));
            if (EnablePunning != BaseStardogServer.DatabaseOptions.DefaultPunning) options.Add(BaseStardogServer.DatabaseOptions.ReasoningPunning, new JValue(EnablePunning));
            if (SchemaGraphs.Count > 0) options.Add(BaseStardogServer.DatabaseOptions.ReasoningSchemaGraphs, new JValue(String.Join(",", SchemaGraphs.ToArray())));

            // Search
            if (FullTextSearch != BaseStardogServer.DatabaseOptions.DefaultFullTextSearch) options.Add(BaseStardogServer.DatabaseOptions.SearchEnabled, new JValue(FullTextSearch));
            if (SearchReindexMode.ToLower() != BaseStardogServer.DatabaseOptions.SearchReIndexModeAsync) options.Add(BaseStardogServer.DatabaseOptions.SearchReIndexMode, new JValue(SearchReindexMode.ToLower()));

            // Transactions
            if (DurableTransactions != BaseStardogServer.DatabaseOptions.DefaultDurableTransactions) options.Add(BaseStardogServer.DatabaseOptions.TransactionsDurable, new JValue(DurableTransactions));

            // Add options to the Template
            template.Add("options", options);

            // Add empty files list
            template.Add("files", new JArray());

            return template;
        }
    }
}
