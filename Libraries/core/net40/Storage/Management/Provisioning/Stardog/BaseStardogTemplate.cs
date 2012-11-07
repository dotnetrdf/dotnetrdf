/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.ComponentModel;
#if SILVERLIGHT && !WINDOWS_PHONE
using System.ComponentModel.DataAnnotations;
#endif
using System.Linq;
using System.Text;
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
            //Index Options
            this.DatabaseType = dbtype;
            this.MinDifferentialIndexLimit = StardogServer.DatabaseOptions.DefaultMinDifferentialIndexLimit;
            this.MaxDifferentialIndexLimit = StardogServer.DatabaseOptions.DefaultMaxDifferentialIndexLimit;
            this.CanoncialiseLiterals = StardogServer.DatabaseOptions.DefaultCanonicaliseLiterals;
            this.IndexNamedGraphs = StardogServer.DatabaseOptions.DefaultNamedGraphIndexing;
            this.PersistIndexes = StardogServer.DatabaseOptions.DefaultPersistIndex;
            this.PersistIndexesSynchronously = StardogServer.DatabaseOptions.DefaultPersistIndexSync;
            this.AutoUpdateStatistics = StardogServer.DatabaseOptions.DefaultAutoUpdateStats;

            //Integrity Constraint Validation
            this.IcvActiveGraphs = new List<string>();
            this.IcvEnabled = StardogServer.DatabaseOptions.DefaultIcvEnabled;
            this.IcvReasoningMode = StardogServer.DatabaseOptions.DefaultIcvReasoningMode;

            //Reasoning
            this.ConsistencyChecking = StardogServer.DatabaseOptions.DefaultConsistencyChecking;
            this.EnablePunning = StardogServer.DatabaseOptions.DefaultPunning;
            this.SchemaGraphs = new List<string>();

            //Search
            this.FullTextSearch = StardogServer.DatabaseOptions.DefaultFullTextSearch;
            this.SearchReindexMode = StardogServer.DatabaseOptions.SearchReIndexModeAsync;

            //Transactions
            this.DurableTransactions = StardogServer.DatabaseOptions.DefaultDurableTransactions;
        }

        #region Index Options

        //index.differential.enable.limit
        //Sets the minimum size of the Stardog database before differential indexes are used.
        //index.differential.merge.limit
        //Sets the size in number of RDF triples before the differential indexes are merged to the main indexes.
        //index.literals.canonical
        //Enables RDF literal canonicalization. See literal canonicalization for details.
        //index.named.graphs
        //Enables optimized index support for named graphs; speeds SPARQL query evaluation with named graphs at the cost of some overhead for database loading and index maintenance.
        //index.persist
        //Enables persistent indexes.
        //index.persist.sync
        //Enables whether memory indexes are synchronously or asynchronously persisted to disk with respect to a transaction.
        //index.statistics.update.automatic
        //Sets whether statistics are maintained automatically.
        //index.type
        //Sets the index type (memory or disk).

        /// <summary>
        /// Gets the Database Type
        /// </summary>
        [Category("Index Options"), 
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Index Type"),
#else
         Display(Name="Index Type"),
#endif
#endif
         Description("The type of the index structures used for the database")]
        public String DatabaseType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/Sets the minimum differential index limit
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultMinDifferentialIndexLimit), 
         Category("Index Options"), 
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Differential Index Enabled Limit"), 
#else
         Display(Name="Differential Index Enabled Limit"),
#endif
#endif
         Description("The minimum size the Stardog database must be before differential indexes are used")]
        public int MinDifferentialIndexLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the maximum differential merge limit
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultMaxDifferentialIndexLimit), 
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Differential Index Merge Limit"), 
#else
         Display(Name="Differential Index Merge Limit"), 
#endif
#endif
         Description("The maximum size in triples of the differential index before it is merged into the main index")]
        public int MaxDifferentialIndexLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether the database should canonicalise literals
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultCanonicaliseLiterals),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Canonicalise Literals"),
#else
         Display(Name="Canonicalise Literals"),
#endif
#endif
         Description("Sets whether literals are canonicalised before being indexed.  If enabled then literals will be transformed e.g. '1'^^xsd:byte => '1'^^xsd:integer, leave disabled to preserve data exactly as input")]
        public bool CanoncialiseLiterals
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to optimize indexes for named graph queries
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultNamedGraphIndexing),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Index Named Graphs"),
#else
         Display(Name="Index Named Graphs"),
#endif
#endif
         Description("Enables optimized index support for named graphs, improves query performance at the cost of load performance.  If your data is all in one graph or you infrequently query named graphs you may wish to disable this")]
        public bool IndexNamedGraphs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to persist indexes
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultPersistIndex),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Persistent Indexes"),
#else
         Display(Name="Persistent Indexes"),
#endif
#endif
         Description("Sets whether indexes are persistent")]
        public bool PersistIndexes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultPersistIndexSync),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Persist Indexes Synchronously"),
#else
         Display(Name="Persist Indexes Synchronously"),
#endif
#endif
         Description("Sets whether indexes are persisted synchronously or asynchronously")]
        public bool PersistIndexesSynchronously
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to automatically update statistics
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultAutoUpdateStats),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Auto Update Statistics"),
#else
         Display(Name="Auto Update Statistics"),
#endif
#endif
         Description("Sets whether statistics are automatically updated")]
        public bool AutoUpdateStatistics
        {
            get;
            set;
        }

        #endregion

        #region Integrity Constraint Validation Options

        //icv.active.graphs
        //Specifies which part of the database, in terms of named graphs, is checked with IC validation. to validate all the named graphs in the database.
        //icv.enabled
        //Determines whether ICV is active for the database; if true, all database mutations are subject to IC validation (i.e., "guard mode").
        //icv.reasoning-type
        //Determines what "reasoning level" is used during IC validation.

        /// <summary>
        /// Gets/Sets the active graphs for ICV
        /// </summary>
        [Category("Integrity Constraint Validation"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Active Graphs"), 
#else
         Display(Name="Active Graphs"), 
#endif
#endif
         Description("Sets the named graphs upon which integrity constraints are enforced")]
        public List<String> IcvActiveGraphs
        {
            get;
            set;
        }

        /// <summary>
        /// Enables/Disables ICV
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultIcvEnabled),
         Category("Integrity Constraint Validation"), 
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Enabled"), 
#else
         Display(Name="Enabled"), 
#endif
#endif
         Description("Enables integrity constraint validation for the database")]
        public bool IcvEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the reasoning mode for ICV
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultIcvReasoningMode),
         Category("Integrity Constraint Validation"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Reasoning Mode"), 
#else
         Display(Name="Reasoning Mode"),
#endif
#endif
         Description("Sets what reasoning mode is used during integrity constraint validation")]
        public StardogReasoningMode IcvReasoningMode
        {
            get;
            set;
        }


        #endregion

        #region Reasoning Options
        //reasoning.consistency.automatic
        //Enables automatic consistency checking with respect to a transaction.
        //reasoning.punning.enabled
        //Enables punning.
        //reasoning.schema.graphs
        //Determines which, if any, named graph or graphs contains the "tbox", i.e., the schema part of the data.

        /// <summary>
        /// Gets/Sets whether to perform automatic consistency checking on transactions
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultConsistencyChecking),
         Category("Reasoning Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Automatic Consistency Checking"),
#else
         Display(Name="Automatic Consistency Checking"),
#endif
#endif
         Description("Sets whether consistency checking is done with respect to transactions")]
        public bool ConsistencyChecking
        {
            get;
            set;
        }

        /// <summary>
        /// Enables/Disables punning
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultPunning),
         Category("Reasoning Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Enable Punning")
#else
         Display(Name="Enable Punning")
#endif
#endif
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
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Schema Graphs"),
#else
         Display(Name="Schema Graphs"),
#endif
#endif
         Description("Sets the graphs considered to contain the schema (TBox) used for reasoning")]
        public List<String> SchemaGraphs
        {
            get;
            set;
        }


        #endregion

        #region Search Options

        //search.enabled
        //Enables semantic search on the database.
        //search.reindex.mode
        //Sets how search indexes are maintained.

        /// <summary>
        /// Enables/Disables Full Text search
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultFullTextSearch),
         Category("Search Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Enable Full Text Search"),
#else
         Display(Name="Enable Full Text Search"),
#endif
#endif
         Description("Enables full text search")]
        public bool FullTextSearch
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Search re-indexing mode
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.SearchReIndexModeAsync),
         Category("Search Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Search Re-index Mode"),
#else
         Display(Name="Search Re-index Mode"),
#endif
#endif
         Description("Controls when search indexes are re-indexed, valid values are sync or async")]
        public String SearchReindexMode
        {
            get;
            set;
        }

        #endregion

        #region Transaction Options
        //transactions.durable
        //Enables durable transactions.

        /// <summary>
        /// Gets/Sets whether to use durable transactions
        /// </summary>
        [DefaultValue(StardogServer.DatabaseOptions.DefaultDurableTransactions),
         Category("Transaction Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Durable Transactions"),
#else
         Display(Name="Durable Transactions"),
#endif
#endif
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
            if (!StardogServer.DatabaseOptions.IsValidDatabaseName(this.ID))
            {
                errors.Add("Database Name " + this.ID + " is invalid, Stardog database names must match the regular expression " + StardogServer.DatabaseOptions.ValidDatabaseNamePattern);
            }
            if (!StardogServer.DatabaseOptions.IsValidDatabaseType(this.DatabaseType))
            {
                errors.Add("Database Type " + this.DatabaseType + " is invalid");
            }
            if (!StardogServer.DatabaseOptions.IsValidSearchReIndexMode(this.SearchReindexMode))
            {
                errors.Add("Search Re-index Mode " + this.SearchReindexMode + " is invalid, only sync or async are currently permitted");
            }
            foreach (String uri in this.SchemaGraphs)
            {
                if (!StardogServer.DatabaseOptions.IsValidNamedGraph(uri))
                {
                    errors.Add("Schema Graphs contains invalid Graph URI '" + uri + "' - must use a valid URI, default or *");
                }
            }
            foreach (String uri in this.IcvActiveGraphs)
            {
                if (!StardogServer.DatabaseOptions.IsValidNamedGraph(uri))
                {
                    errors.Add("ICV Active Graphs contains invalid Graph URI '" + uri + "' - must use a valid URI, default or *");
                }
            }
            this.ValidateInternal(errors);
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
            //Set up the basic template
            JObject template = new JObject();
            template.Add("dbname", new JValue(this.ID));

            //Build up the options object
            //Don't bother included non-required options if the user hasn't adjusted them from their defaults
            JObject options = new JObject();

            //Index Options
            options.Add(StardogServer.DatabaseOptions.IndexType, new JValue(this.DatabaseType.ToLower()));
            if (this.MinDifferentialIndexLimit != StardogServer.DatabaseOptions.DefaultMinDifferentialIndexLimit) options.Add(StardogServer.DatabaseOptions.IndexDifferentialEnableLimit, new JValue(this.MinDifferentialIndexLimit));
            if (this.MaxDifferentialIndexLimit != StardogServer.DatabaseOptions.DefaultMaxDifferentialIndexLimit) options.Add(StardogServer.DatabaseOptions.IndexDifferentialMergeLimit, new JValue(this.MaxDifferentialIndexLimit));
            if (this.CanoncialiseLiterals != StardogServer.DatabaseOptions.DefaultCanonicaliseLiterals) options.Add(StardogServer.DatabaseOptions.IndexLiteralsCanonical, new JValue(this.CanoncialiseLiterals));
            if (this.IndexNamedGraphs != StardogServer.DatabaseOptions.DefaultNamedGraphIndexing) options.Add(StardogServer.DatabaseOptions.IndexNamedGraphs, new JValue(this.IndexNamedGraphs));
            if (this.PersistIndexes != StardogServer.DatabaseOptions.DefaultPersistIndex) options.Add(StardogServer.DatabaseOptions.IndexPersistTrue, new JValue(this.PersistIndexes));
            if (this.PersistIndexesSynchronously != StardogServer.DatabaseOptions.DefaultPersistIndexSync) options.Add(StardogServer.DatabaseOptions.IndexPersistSync, new JValue(this.PersistIndexesSynchronously));
            if (this.AutoUpdateStatistics != StardogServer.DatabaseOptions.DefaultAutoUpdateStats) options.Add(StardogServer.DatabaseOptions.IndexStatisticsAutoUpdate, new JValue(this.AutoUpdateStatistics));

            //ICV Options
            if (this.IcvActiveGraphs.Count > 0) options.Add(StardogServer.DatabaseOptions.IcvActiveGraphs, new JValue(String.Join(",", this.IcvActiveGraphs.ToArray())));
            if (this.IcvEnabled != StardogServer.DatabaseOptions.DefaultIcvEnabled) options.Add(StardogServer.DatabaseOptions.IcvEnabled, new JValue(this.IcvEnabled));
            if (this.IcvReasoningMode != StardogServer.DatabaseOptions.DefaultIcvReasoningMode) options.Add(StardogServer.DatabaseOptions.IcvReasoningType, new JValue(this.IcvReasoningMode.ToString()));
            
            //Reasoning
            if (this.ConsistencyChecking != StardogServer.DatabaseOptions.DefaultConsistencyChecking) options.Add(StardogServer.DatabaseOptions.ReasoningAutoConsistency, new JValue(this.ConsistencyChecking));
            if (this.EnablePunning != StardogServer.DatabaseOptions.DefaultPunning) options.Add(StardogServer.DatabaseOptions.ReasoningPunning, new JValue(this.EnablePunning));
            if (this.SchemaGraphs.Count > 0) options.Add(StardogServer.DatabaseOptions.ReasoningSchemaGraphs, new JValue(String.Join(",", this.SchemaGraphs.ToArray())));

            //Search
            if (this.FullTextSearch != StardogServer.DatabaseOptions.DefaultFullTextSearch) options.Add(StardogServer.DatabaseOptions.SearchEnabled, new JValue(this.FullTextSearch));
            if (this.SearchReindexMode.ToLower() != StardogServer.DatabaseOptions.SearchReIndexModeAsync) options.Add(StardogServer.DatabaseOptions.SearchReIndexMode, new JValue(this.SearchReindexMode.ToLower()));

            //Transactions
            if (this.DurableTransactions != StardogServer.DatabaseOptions.DefaultDurableTransactions) options.Add(StardogServer.DatabaseOptions.TransactionsDurable, new JValue(this.DurableTransactions));

            //Add options to the Template
            template.Add("options", options);

            //Add empty files list
            template.Add("files", new JArray());

            return template;
        }
    }
}
