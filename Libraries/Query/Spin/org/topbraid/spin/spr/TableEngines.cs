using System.Collections.Generic;
using org.topbraid.spin.spr.spra;
using VDS.RDF;

namespace org.topbraid.spin.spr
{

    /**
     * A Singleton managing the registered TableEngines.
     * 
     * @author Holger Knublauch
     */
    public class TableEngines
    {

        private static TableEngines singleton = new TableEngines();

        public static TableEngines get()
        {
            return singleton;
        }

        public static void set(TableEngines value)
        {
            TableEngines.singleton = value;
        }

        private TableEngine defaultTableEngine = new ATableEngine();

        private Dictionary<INode, TableEngine> map = new Dictionary<INode, TableEngine>();

        public TableEngines()
        {
            map[SPRA.Table] = defaultTableEngine;
        }


        public TableEngine getDefaultTableEngine()
        {
            return defaultTableEngine;
        }


        public TableEngine getForType(INode type)
        {
            if (map.ContainsKey(type))
            {
                return map[type];
            }
            return null; 
        }


        public void register(INode type, TableEngine tableEngine)
        {
            map[type] = tableEngine;
        }
    }
}