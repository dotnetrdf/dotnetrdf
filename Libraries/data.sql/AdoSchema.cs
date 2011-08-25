using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Represents a database schema that provides the required ADO store stored procedures
    /// </summary>
    public class AdoSchemaDefinition
    {
        private List<AdoSchemaScriptDefinition> _scripts = new List<AdoSchemaScriptDefinition>();

        public AdoSchemaDefinition(String name, String descrip, IEnumerable<AdoSchemaScriptDefinition> scripts)
        {
            this.Name = name;
            this.Description = descrip;
            this._scripts.AddRange(scripts);
        }

        public String Name
        {
            get;
            private set;
        }

        public String Description
        {
            get;
            private set;
        }

        public bool HasScript(AdoSchemaScriptType type, AdoSchemaScriptDatabase db)
        {
            return this._scripts.Any(s => s.ScriptType == type && s.Database == db);
        }

        public String GetScript(AdoSchemaScriptType type, AdoSchemaScriptDatabase db)
        {
            return this._scripts.Where(s => s.ScriptType == type && s.Database == db).Select(d => d.ScriptResource).FirstOrDefault();
        }
    }

    public enum AdoSchemaScriptType
    {
        Create,
        Drop
    }

    public enum AdoSchemaScriptDatabase
    {
        MicrosoftSqlServer
    }

    public class AdoSchemaScriptDefinition
    {
        public AdoSchemaScriptDefinition(AdoSchemaScriptType type, AdoSchemaScriptDatabase db, String resource)
        {
            this.ScriptType = type;
            this.Database = db;
            this.ScriptResource = resource;
        }

        public AdoSchemaScriptType ScriptType
        {
            get;
            private set;
        }

        public AdoSchemaScriptDatabase Database
        {
            get;
            private set;
        }

        public String ScriptResource
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Static Helper class which managers the available database schemas
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ADO Store was specifically designed to be completely based upon use of stored procedures so any schema can potentially be used provided it exposed the stored procedures that the code expects to exist.
    /// </para>
    /// <para>
    /// There are two schemas provided by default, the <strong>Hash</strong> schema is recommended but requires SQL Server 2005 or higher.  The <strong>Simple</strong> schema is less performant but has a version that works with earlier versions of SQL Server e.g. SQL Server 2000.  We <strong>strongly</strong> recommend the Hash schema for all new development!
    /// </para>
    /// </remarks>
    public static class AdoSchemaHelper
    {
        private static List<AdoSchemaDefinition> _defs = new List<AdoSchemaDefinition>();
        private static bool _init = false;
        private static AdoSchemaDefinition _default;

        private static void Init()
        {
            if (!_init)
            {
                //Register Hash schema and set as default
                _defs.Add(new AdoSchemaDefinition("Hash", "A schema that uses MD5 hash based indexes to provide better Node ID lookup speed",
                    new AdoSchemaScriptDefinition[] 
                    { 
                        new AdoSchemaScriptDefinition(AdoSchemaScriptType.Create, AdoSchemaScriptDatabase.MicrosoftSqlServer, "VDS.RDF.Storage.CreateMicrosoftAdoHashStore.sql"),
                        new AdoSchemaScriptDefinition(AdoSchemaScriptType.Drop, AdoSchemaScriptDatabase.MicrosoftSqlServer, "VDS.RDF.Storage.DropMicrosoftAdoHashStore.sql")
                    }));
                _default = _defs[0];

                //Register Simple schemas
                _defs.Add(new AdoSchemaDefinition("Simple", "A simple schema that uses partial value indexes to speed up Node ID lookups",
                    new AdoSchemaScriptDefinition[]
                    {
                        new AdoSchemaScriptDefinition(AdoSchemaScriptType.Create, AdoSchemaScriptDatabase.MicrosoftSqlServer, "VDS.RDF.Storage.CreateMicrosoftAdoSimpleStore.sql"),
                        new AdoSchemaScriptDefinition(AdoSchemaScriptType.Drop, AdoSchemaScriptDatabase.MicrosoftSqlServer, "VDS.RDF.Storage.DropMicrosoftAdoSimpleStore.sql")
                    }));
                _defs.Add(new AdoSchemaDefinition("Simple 2000", "A variant of the Simple schema that should work on pre-2005 SQL Server instances",
                    new AdoSchemaScriptDefinition[]
                    {
                        new AdoSchemaScriptDefinition(AdoSchemaScriptType.Create, AdoSchemaScriptDatabase.MicrosoftSqlServer, "VDS.RDF.Storage.CreateMicrosoftAdoSimple2000Store.sql"),
                        new AdoSchemaScriptDefinition(AdoSchemaScriptType.Drop, AdoSchemaScriptDatabase.MicrosoftSqlServer, "VDS.RDF.Storage.DropMicrosoftAdoSimple2000Store.sql")
                    }));
                _init = true;
            }
        }

        public static IEnumerable<AdoSchemaDefinition> SchemaDefinitions
        {
            get 
            {
                if (!_init) Init();
                return _defs;
            }
        }

        public static AdoSchemaDefinition DefaultSchema
        {
            get 
            {
                if (!_init) Init();
                if (_default != null)
                {
                    return _default;
                } 
                else
                {
                    return _defs.FirstOrDefault();
                }
            }
            set
            {
                if (!_init) Init();
                _default = value;
            }
        }

        public static void AddSchema(AdoSchemaDefinition def)
        {
            if (!_init) Init();
            _defs.Add(def);
        }

        public static void RemoveSchema(AdoSchemaDefinition def)
        {
            if (!_init) Init();
            _defs.Remove(def);
        }
    }
}
