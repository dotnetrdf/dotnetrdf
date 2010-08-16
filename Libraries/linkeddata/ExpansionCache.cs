using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using VDS.RDF.LinkedData.Profiles;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace VDS.RDF.LinkedData
{
    /// <summary>
    /// Cache for caching Expansions and Expansion Profiles
    /// </summary>
    class ExpansionCache
    {
        private String _cacheDir;
        private String _resultsDir;
        private String _profilesDir;
        private int _freshness = 7;
        private TriGParser _parser = new TriGParser();
        private TriGWriter _writer = new TriGWriter();
        private bool _enabled = true;

        /// <summary>
        /// Creates a new Cache that uses the given Directory
        /// </summary>
        /// <param name="cacheDir">Cache Directory</param>
        public ExpansionCache(String cacheDir)
            : this(cacheDir, 7) { }

        /// <summary>
        /// Creates a new Cache that uses the given Directory and Cache Freshness
        /// </summary>
        /// <param name="cacheDir">Cache Directory</param>
        /// <param name="freshness">Cache Freshness - Number of days to keep cached copies for</param>
        public ExpansionCache(String cacheDir, int freshness)
        {
            this._freshness = freshness;
            if (!Path.IsPathRooted(cacheDir)) cacheDir = Path.GetFullPath(cacheDir);
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }
            this._cacheDir = cacheDir;

            this._resultsDir = Path.Combine(this._cacheDir, "results\\");
            this._profilesDir = Path.Combine(this._cacheDir, "profiles\\");
            if (!Directory.Exists(this._resultsDir)) Directory.CreateDirectory(this._resultsDir);
            if (!Directory.Exists(this._profilesDir)) Directory.CreateDirectory(this._profilesDir);
        }

        /// <summary>
        /// Gets/Sets whether the Cache is disabled
        /// </summary>
        public bool DisableCaching
        {
            get
            {
                return !this._enabled;
            }
            set
            {
                this._enabled = !value;
            }
        }

        /// <summary>
        /// Checks whether a given Expansion is Cached
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public bool IsExpansionCached(Uri u)
        {
            if (!this._enabled) return false;

            String cachePath = Path.Combine(this._resultsDir, u.GetSha256Hash());
            return File.Exists(cachePath) && this.IsFresh(cachePath);
        }

        /// <summary>
        /// Checks whether a given Expansion is Cached
        /// </summary>
        /// <param name="u">Uri</param>
        /// <param name="profile">Expansion Profile Uri</param>
        /// <returns></returns>
        public bool IsExpansionCached(Uri u, Uri profile)
        {
            if (!this._enabled) return false;

            String cachePath = Path.Combine(this._resultsDir, profile.GetSha256Hash() + "_" + u.GetSha256Hash());
            return File.Exists(cachePath) && this.IsFresh(cachePath);
        }

        /// <summary>
        /// Checks whether a given file in the Cache meets the Cache Freshness criteria
        /// </summary>
        /// <param name="file">Cached File</param>
        /// <returns></returns>
        /// <remarks>
        /// Removes the file from the cache if it is stale
        /// </remarks>
        private bool IsFresh(String file)
        {
            DateTime created = File.GetCreationTime(file);
            TimeSpan freshness = DateTime.Now - created;
            if (freshness.Days >= this._freshness)
            {
                //Remove from Cache when no longer Fresh
                File.Delete(file);
            }
            return (freshness.Days < this._freshness);
        }

        /// <summary>
        /// Gets an Expansion from the Cache
        /// </summary>
        /// <param name="u">Uri</param>
        /// <returns></returns>
        public IInMemoryQueryableStore GetExpansion(Uri u)
        {
            String cachePath = Path.Combine(this._resultsDir, u.GetSha256Hash());

            TripleStore store = new TripleStore();
            this._parser.Load(store, new StreamParams(cachePath));
            return store;
        }

        /// <summary>
        /// Gets an Expansion from the Cache
        /// </summary>
        /// <param name="u">Uri</param>
        /// <param name="profile">Expansion Profile Uri</param>
        /// <returns></returns>
        public IInMemoryQueryableStore GetExpansion(Uri u, Uri profile)
        {
            String cachePath = Path.Combine(this._resultsDir, profile.GetSha256Hash() + "_" + u.GetSha256Hash());

            TripleStore store = new TripleStore();
            this._parser.Load(store, new StreamParams(cachePath));
            return store;
        }

        /// <summary>
        /// Adds an Expansion to the Cache
        /// </summary>
        /// <param name="u">Uri</param>
        /// <param name="expansion">Expansion</param>
        /// <returns></returns>
        public void Add(Uri u, IInMemoryQueryableStore expansion)
        {
            String cachePath = Path.Combine(this._resultsDir, u.GetSha256Hash());
            this._writer.Save(expansion, new StreamParams(cachePath));
        }

        /// <summary>
        /// Adds an Expansion to the Cache
        /// </summary>
        /// <param name="u">Uri</param>
        /// <param name="profile">Expansion Profile Uri</param>
        /// <param name="expansion">Expansion</param>
        /// <returns></returns>
        public void Add(Uri u, Uri profile, IInMemoryQueryableStore expansion)
        {
            String cachePath = Path.Combine(this._resultsDir, profile.GetSha256Hash() + "_" + u.GetSha256Hash());
            this._writer.Save(expansion, new StreamParams(cachePath));
        }
    }
}
