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
                            catch
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

                        //Try to get the definition type
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
        public static IConnectionDefinition GetDefinitionByTargetType(Type t)
        {
            if (!_init) Init();

            return _defs.FirstOrDefault(d => d.Type.Equals(t));
        }

        /// <summary>
        /// Gets a connection definition 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IConnectionDefinition GetDefinitionByType(Type t)
        {
            if (!_init) Init();

            return _defs.FirstOrDefault(d => d.GetType().Equals(t));
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
