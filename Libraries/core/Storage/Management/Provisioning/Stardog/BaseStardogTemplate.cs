/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
    public class BaseStardogTemplate
        : StoreTemplate
    {
        public BaseStardogTemplate(String id, String name, String descrip, String dbtype)
            : base(id, name, descrip)
        {
            //Index Options
            this.DatabaseType = dbtype;
            this.MinDifferentialIndexLimit = StardogConnector.DatabaseOptions.DefaultMinDifferentialIndexLimit;
            this.MaxDifferentialIndexLimit = StardogConnector.DatabaseOptions.DefaultMaxDifferentialIndexLimit;
            this.CanoncialiseLiterals = StardogConnector.DatabaseOptions.DefaultCanonicaliseLiterals;
            this.PersistIndexes = StardogConnector.DatabaseOptions.DefaultPersistIndex;
            this.PersistIndexesSynchronously = StardogConnector.DatabaseOptions.DefaultPersistIndexSync;
            this.AutoUpdateStatistics = StardogConnector.DatabaseOptions.DefaultAutoUpdateStats;

            //Integrity Constraint Validation
            this.IcvActiveGraphs = new List<string>();
            this.IcvEnabled = false;
            this.IcvReasoningMode = StardogConnector.DatabaseOptions.DefaultIcvReasoningMode;

            //Reasoning
            this.ConsistencyChecking = StardogConnector.DatabaseOptions.DefaultConsistencyChecking;
            this.EnablePunning = StardogConnector.DatabaseOptions.DefaultPunning;
            this.SchemaGraphs = new List<string>();

            //Search
            this.FullTextSearch = StardogConnector.DatabaseOptions.DefaultFullTextSearch;
            this.SearchReindexMode = StardogConnector.DatabaseOptions.SearchReIndexModeAsync;

            //Transactions
            this.DurableTransactions = StardogConnector.DatabaseOptions.DefaultDurableTransactions;
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

        [Category("Index Options"), 
         DisplayName("Index Type"),
        Description("The type of the index structures used for the database")]
        public String DatabaseType
        {
            get;
            private set;
        }

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultMinDifferentialIndexLimit), 
         Category("Index Options"), 
         DisplayName("Differential Index Enabled Limit"), 
         Description("The minimum size the Stardog database must be before differential indexes are used")]
        public int MinDifferentialIndexLimit
        {
            get;
            set;
        }

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultMaxDifferentialIndexLimit), 
         Category("Index Options"),
         DisplayName("Different Index Merge Limit"), 
         Description("The maximum size in triples of the differential index before it is merged into the main index")]
        public int MaxDifferentialIndexLimit
        {
            get;
            set;
        }

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultCanonicaliseLiterals),
         Category("Index Options"),
         DisplayName("Canonicalise Literals"),
         Description("Sets whether literals are canonicalised before being indexed.  If enabled then literals will be transformed e.g. '1'^^xsd:byte => '1'^^xsd:integer, leave disabled to preserve data exactly as input")]
        public bool CanoncialiseLiterals
        {
            get;
            set;
        }

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultNamedGraphIndexing),
         Category("Index Options"),
         DisplayName("Index Named Graphs"),
         Description("Enables optimized index support for named graphs, improves query performance at the cost of load performance.  If your data is all in one graph or you infrequently query named graphs you may wish to disable this")]
        public bool IndexNamedGraphs
        {
            get;
            set;
        }

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultPersistIndex),
         Category("Index Options"),
         DisplayName("Persist Indexes"),
         Description("Sets whether indexes are persistent")]
        public bool PersistIndexes
        {
            get;
            set;
        }

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultPersistIndexSync),
         Category("Index Options"),
         DisplayName("Synchronous Index Persistence"),
         Description("Sets whether indexes are persisted synchronously or asynchronously")]
        public bool PersistIndexesSynchronously
        {
            get;
            set;
        }

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultAutoUpdateStats),
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

        //icv.active.graphs
        //Specifies which part of the database, in terms of named graphs, is checked with IC validation. to validate all the named graphs in the database.
        //icv.enabled
        //Determines whether ICV is active for the database; if true, all database mutations are subject to IC validation (i.e., "guard mode").
        //icv.reasoning-type
        //Determines what "reasoning level" is used during IC validation.

        [Category("Integrity Constraint Validation"), 
         DisplayName("Active Graphs"), 
         Description("Sets the named graphs upon which integrity constraints are enforced")]
        public List<String> IcvActiveGraphs
        {
            get;
            set;
        }


        [Category("Integrity Constraint Validation"), 
         DisplayName("Enabled"), 
         Description("Enables integrity constraint validation for the database")]
        public bool IcvEnabled
        {
            get;
            set;
        }

        [Category("Integrity Constraint Validation"),
         DisplayName("Reasoning Mode"), 
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

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultConsistencyChecking),
         Category("Reasoning Options"),
         DisplayName("Automatic Consistency Checking"),
         Description("Sets whether consistency checking is done with respect to transactions")]
        public bool ConsistencyChecking
        {
            get;
            set;
        }

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultPunning),
         Category("Reasoning Options"),
         DisplayName("Enable Punning")]
        public bool EnablePunning
        {
            get;
            set;
        }

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

        //search.enabled
        //Enables semantic search on the database.
        //search.reindex.mode
        //Sets how search indexes are maintained.

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultFullTextSearch),
         Category("Search Options"),
         DisplayName("Enable Full Text Search"),
         Description("Enables full text search")]
        public bool FullTextSearch
        {
            get;
            set;
        }

        [DefaultValue(StardogConnector.DatabaseOptions.SearchReIndexModeAsync),
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
        //transactions.durable
        //Enables durable transactions.

        [DefaultValue(StardogConnector.DatabaseOptions.DefaultDurableTransactions),
         Category("Transaction Options"),
         DisplayName("Durable Transactions"),
         Description("Enables durable transactions")]
        public bool DurableTransactions
        {
            get;
            set;
        }

        #endregion

        public override IEnumerable<string> Validate()
        {
            List<String> errors = new List<string>();
            if (!StardogConnector.DatabaseOptions.IsValidDatabaseName(this.ID))
            {
                errors.Add("Database Name " + this.ID + " is invalid, Stardog database names must match the regular expression " + StardogConnector.DatabaseOptions.ValidDatabaseNamePattern);
            }
            if (!StardogConnector.DatabaseOptions.IsValidDatabaseType(this.DatabaseType))
            {
                errors.Add("Database Type " + this.DatabaseType + " is invalid");
            }
            if (!StardogConnector.DatabaseOptions.IsValidSearchReIndexMode(this.SearchReindexMode))
            {
                errors.Add("Search Re-index Mode " + this.SearchReindexMode + " is invalid, only sync or async are currently permitted");
            }
            foreach (String uri in this.SchemaGraphs)
            {
                if (!StardogConnector.DatabaseOptions.IsValidNamedGraph(uri))
                {
                    errors.Add("Schema Graphs contains invalid Graph URI '" + uri + "' - must use a valid URI, default or *");
                }
            }
            foreach (String uri in this.IcvActiveGraphs)
            {
                if (!StardogConnector.DatabaseOptions.IsValidNamedGraph(uri))
                {
                    errors.Add("ICV Active Graphs contains invalid Graph URI '" + uri + "' - must use a valid URI, default or *");
                }
            }
            this.ValidateInternal(errors);
            return errors;
        }

        protected virtual void ValidateInternal(List<String> errors) { }

        public JObject GetTemplateJson()
        {
            //Set up the basic template
            JObject template = new JObject();
            template.Add("dbname", new JValue(this.ID));

            //Build up the options object
            JObject options = new JObject();

            //Index Options
            options.Add(StardogConnector.DatabaseOptions.IndexType, new JValue(this.DatabaseType.ToLower()));
            options.Add(StardogConnector.DatabaseOptions.IndexDifferentialEnableLimit, new JValue(this.MinDifferentialIndexLimit));
            options.Add(StardogConnector.DatabaseOptions.IndexDifferentialMergeLimit, new JValue(this.MaxDifferentialIndexLimit));
            options.Add(StardogConnector.DatabaseOptions.IndexLiteralsCanonical, new JValue(this.CanoncialiseLiterals));
            options.Add(StardogConnector.DatabaseOptions.IndexNamedGraphs, new JValue(this.IndexNamedGraphs));
            options.Add(StardogConnector.DatabaseOptions.IndexPersistTrue, new JValue(this.PersistIndexes));
            options.Add(StardogConnector.DatabaseOptions.IndexPersistSync, new JValue(this.PersistIndexesSynchronously));
            options.Add(StardogConnector.DatabaseOptions.IndexStatisticsAutoUpdate, new JValue(this.AutoUpdateStatistics));

            //ICV Options
            options.Add(StardogConnector.DatabaseOptions.IcvActiveGraphs, new JValue(String.Join(",", this.IcvActiveGraphs.ToArray())));
            options.Add(StardogConnector.DatabaseOptions.IcvEnabled, new JValue(this.IcvEnabled));
            options.Add(StardogConnector.DatabaseOptions.IcvReasoningType, new JValue(this.IcvReasoningMode.ToString()));
            
            //Reasoning
            options.Add(StardogConnector.DatabaseOptions.ReasoningAutoConsistency, new JValue(this.ConsistencyChecking));
            options.Add(StardogConnector.DatabaseOptions.ReasoningPunning, new JValue(this.EnablePunning));
            options.Add(StardogConnector.DatabaseOptions.ReasoningSchemaGraphs, new JValue(String.Join(",", this.SchemaGraphs.ToArray())));

            //Search
            options.Add(StardogConnector.DatabaseOptions.SearchEnabled, new JValue(this.FullTextSearch));
            options.Add(StardogConnector.DatabaseOptions.SearchReIndexMode, new JValue(this.SearchReindexMode.ToLower()));

            //Transactions
            options.Add(StardogConnector.DatabaseOptions.TransactionsDurable, new JValue(this.DurableTransactions));

            //Add options to the Template
            template.Add("options", options);

            //Add empty files list
            template.Add("files", new JArray());

            return template;
        }
    }
}
