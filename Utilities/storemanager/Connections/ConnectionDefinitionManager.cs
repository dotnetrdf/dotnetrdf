using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    static class ConnectionDefinitionManager
    {
        private static List<Type> _connectionDefTypes = new List<Type>();

        private static bool _init = false;

        private static void Init()
        {
            if (!_init)
            {
                //First find all types in this assembly
                Assembly assm = Assembly.GetExecutingAssembly();
                ConnectionDefinitionManager.DiscoverTypes(assm);

                //Then find types in Plugin assemblies
                if (Directory.Exists("plugins"))
                {
                    foreach (String file in Directory.GetFiles("plugins"))
                    {
                        switch (Path.GetExtension(file))
                        {
                            case ".dll":
                                try
                                {
                                    assm = Assembly.LoadFrom(file);
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

                _init = true;
            }
        }

        private static void DiscoverTypes(Assembly assm)
        {
            Type conDefType = typeof(IConnectionDefinition);
            foreach (Type t in assm.GetTypes())
            {
                if (conDefType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                {
                    _connectionDefTypes.Add(t);
                }
            }
        }

        public static IEnumerable<IConnectionDefinition> GetDefinitions()
        {
            if (!_init) Init();

            foreach (Type t in _connectionDefTypes)
            {
                IConnectionDefinition def = null;
                try
                {
                    def = Activator.CreateInstance(t) as IConnectionDefinition;
                }
                catch
                {
                    //Ignore Errors
                }
                if (def != null) yield return def;
            }
        }

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
