/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

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
using System.Linq;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Represents a database schema that provides the required ADO store stored procedures
    /// </summary>
    public class AdoSchemaDefinition
    {
        private List<AdoSchemaScriptDefinition> _scripts = new List<AdoSchemaScriptDefinition>();

        /// <summary>
        /// Creates a new Schema Definition
        /// </summary>
        /// <param name="name">Schema Name</param>
        /// <param name="descrip">Description</param>
        /// <param name="scripts">Schema Scripts</param>
        public AdoSchemaDefinition(String name, String descrip, IEnumerable<AdoSchemaScriptDefinition> scripts)
        {
            this.Name = name;
            this.Description = descrip;
            this._scripts.AddRange(scripts);
        }

        /// <summary>
        /// Gets the Schema Name
        /// </summary>
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Schema Description
        /// </summary>
        public String Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether the definition has a specific script
        /// </summary>
        /// <param name="type">Script Type</param>
        /// <param name="db">Database Type</param>
        /// <returns></returns>
        public bool HasScript(AdoSchemaScriptType type, AdoSchemaScriptDatabase db)
        {
            return this._scripts.Any(s => s.ScriptType == type && s.Database == db);
        }

        /// <summary>
        /// Gets the Script resource name (or null if it doesn't exist) for a specific script
        /// </summary>
        /// <param name="type">Script Type</param>
        /// <param name="db">Database Type</param>
        /// <returns></returns>
        public String GetScript(AdoSchemaScriptType type, AdoSchemaScriptDatabase db)
        {
            return this._scripts.Where(s => s.ScriptType == type && s.Database == db).Select(d => d.ScriptResource).FirstOrDefault();
        }

        /// <summary>
        /// Gets the available scripts for this Schema Definition
        /// </summary>
        public IEnumerable<AdoSchemaScriptDefinition> ScriptDefinitions
        {
            get
            {
                return this._scripts;
            }
        }
    }

    /// <summary>
    /// Possible Schema Script Types
    /// </summary>
    public enum AdoSchemaScriptType
    {
        /// <summary>
        /// Script for creating the database
        /// </summary>
        Create,
        /// <summary>
        /// Script for dropping the database
        /// </summary>
        Drop
    }

    /// <summary>
    /// Supported Database Types
    /// </summary>
    public enum AdoSchemaScriptDatabase
    {
        /// <summary>
        /// Microsoft SQL Server (and SQL Azure)
        /// </summary>
        MicrosoftSqlServer
        ///// <summary>
        ///// SQL Server CE
        ///// </summary>
        //SqlServerCe
    }

    /// <summary>
    /// Represents the definition of a schema script
    /// </summary>
    public class AdoSchemaScriptDefinition
    {
        /// <summary>
        /// Creates a new Schema Script Definition
        /// </summary>
        /// <param name="type">Script Type</param>
        /// <param name="db">Database Type</param>
        /// <param name="resource">Resource Name</param>
        public AdoSchemaScriptDefinition(AdoSchemaScriptType type, AdoSchemaScriptDatabase db, String resource)
        {
            this.ScriptType = type;
            this.Database = db;
            this.ScriptResource = resource;
        }

        /// <summary>
        /// Gets the Script Type
        /// </summary>
        public AdoSchemaScriptType ScriptType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Database Type
        /// </summary>
        public AdoSchemaScriptDatabase Database
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Script Resource Name
        /// </summary>
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
                        new AdoSchemaScriptDefinition(AdoSchemaScriptType.Drop, AdoSchemaScriptDatabase.MicrosoftSqlServer, "VDS.RDF.Storage.DropMicrosoftAdoSimpleStore.sql"),
                        //new AdoSchemaScriptDefinition(AdoSchemaScriptType.Create, AdoSchemaScriptDatabase.SqlServerCe, "VDS.RDF.Storage.CreateSqlServerCeAdoSimpleStore.sql"),
                        //new AdoSchemaScriptDefinition(AdoSchemaScriptType.Drop, AdoSchemaScriptDatabase.SqlServerCe, "VDS.RDF.Storage.DropSqlServerCeAdoSimpleStore.sql")
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

        /// <summary>
        /// Gets the available built-in schemas
        /// </summary>
        public static IEnumerable<AdoSchemaDefinition> SchemaDefinitions
        {
            get 
            {
                if (!_init) Init();
                return _defs;
            }
        }

        /// <summary>
        /// Gets/Sets the Default Schema
        /// </summary>
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

        /// <summary>
        /// Adds a new Schema Definition
        /// </summary>
        /// <param name="def">Definition</param>
        public static void AddSchema(AdoSchemaDefinition def)
        {
            if (!_init) Init();
            _defs.Add(def);
        }

        /// <summary>
        /// Removes a Schema Definition
        /// </summary>
        /// <param name="def">Definition</param>
        public static void RemoveSchema(AdoSchemaDefinition def)
        {
            if (!_init) Init();
            _defs.Remove(def);
        }

        /// <summary>
        /// Gets the Schema with the given name or null if it does not exist
        /// </summary>
        /// <param name="name">Schema Name</param>
        /// <returns></returns>
        public static AdoSchemaDefinition GetSchema(String name)
        {
            return AdoSchemaHelper.SchemaDefinitions.FirstOrDefault(d => d.Name.Equals(name));
        }
    }
}
