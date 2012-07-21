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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Manager which manages available definitions
    /// </summary>
    public static class ConnectionDefinitionManager
    {
        private static HashSet<Type> _connectionDefTypes = new HashSet<Type>();
        private static List<IConnectionDefinition> _defs = new List<IConnectionDefinition>();

        private static bool _init = false;

        private static void Init()
        {
            if (!_init)
            {
                //First find all types in this assembly
                Assembly assm = Assembly.GetExecutingAssembly();
                ConnectionDefinitionManager.DiscoverTypes(assm);

                //Then scan for plugins
                ScanPlugins();

                _init = true;
            }
        }

        /// <summary>
        /// Scans for plugins in the default location
        /// </summary>
        public static void ScanPlugins()
        {
            ScanPlugins("plugins");
        }

        /// <summary>
        /// Scans for plugins in a custom location
        /// </summary>
        /// <param name="dir">Directory to scan</param>
        /// <remarks>
        /// <para>
        /// Scanning looks for files with a <em>.dll</em> extension, loads them and then searches for types implementing the <see cref="IConnectionDefinition">IConnectionDefinition</see> interface
        /// </para>
        /// </remarks>
        public static void ScanPlugins(String dir)
        {
            //Then find types in Plugin assemblies
            if (Directory.Exists(dir))
            {
                foreach (String file in Directory.GetFiles(dir))
                {
                    switch (Path.GetExtension(file))
                    {
                        case ".dll":
                            try
                            {
                                Assembly assm = Assembly.LoadFrom(file);
                                ConnectionDefinitionManager.DiscoverTypes(assm);
                            }
                            catch (Exception ex)
                            {
                                //Ignore errors in loading assemblies
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }
        }

        private static void DiscoverTypes(Assembly assm)
        {
            Type conDefType = typeof(IConnectionDefinition);
            foreach (Type t in assm.GetTypes())
            {
                if (conDefType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                {
                    if (_connectionDefTypes.Contains(t))
                    {
                        //Skip if we've already seen, may happen on a rescan
                        continue;
                    }
                    else
                    {
                        _connectionDefTypes.Add(t);
                    }
                }
                IConnectionDefinition def = null;
                try
                {
                    def = Activator.CreateInstance(t) as IConnectionDefinition;
                }
                catch
                {
                    //Ignore Errors
                }
                if (def != null) _defs.Add(def);
            }
        }

        /// <summary>
        /// Gets available connection definitions
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IConnectionDefinition> GetDefinitions()
        {
            if (!_init) Init();

            return _defs;
        }

        /// <summary>
        /// Gets a connection definition that will generate connections of the given type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public static IConnectionDefinition GetDefinition(Type t)
        {
            if (!_init) Init();

            return _defs.FirstOrDefault(d => d.Type.Equals(t));

        }

        /// <summary>
        /// Gets available connection definition types i.e. the types that implement <see cref="IConnectionDefinition"/>
        /// </summary>
        public static IEnumerable<Type> DefinitionTypes
        {
            get
            {
                if (!_init) Init();

                return _connectionDefTypes;
            }
        }
    }
}
