using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace Alexandria.WideTable.ColumnSchema
{
    public interface IColumnSchema<TColumn>
    {
        IEnumerable<TColumn> ToColumns(Triple t);

        Triple FromColumns(IGraph g, IEnumerable<TColumn> columns);

        Triple FromColumns(IEnumerable<TColumn> columns);
    }
}
