using VDS.RDF.Storage;
using System;
using VDS.RDF;
using System.Collections.Generic;
namespace org.topbraid.spin.spr
{

    /**
     * Convenience base class for TableEngine implementations.
     * 
     * @author Holger Knublauch
     */
    public abstract class AbstractTableEngine : TableEngine
    {


        protected void addVarNames(String ns, INode table, List<String> varNames)
        {
            Model model = table.getModel();
            for(int col = 0; col < varNames.size(); col++)
            {
                String varName = varNames.get(col);
                INode varNameProperty = getVarNameProperty(ns, col);
                table.addProperty(varNameProperty, model.createTypedLiteral(varName));
            }
        }


        protected Uri getColCountProperty(String ns)
        {
            return UriFactory.Create(ns + "colCount");
        }


        protected Uri getRowCountProperty(String ns)
        {
            return UriFactory.Create(ns + "rowCount");
        }


        protected Uri getValueProperty(String ns)
        {
            return UriFactory.Create(ns + "value");
        }


        protected Uri getVarNameProperty(String ns, int colIndex)
        {
            return UriFactory.Create(ns + "colName" + colIndex);
        }
    }
}