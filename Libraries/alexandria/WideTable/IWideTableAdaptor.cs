using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using Alexandria.WideTable.ColumnSchema;

namespace Alexandria.WideTable
{
    public interface IWideTableAdaptor<TKey, TColumn> : IDisposable
    {
        IColumnSchema<TColumn> ColumnSchema
        {
            get;
        }

        TKey GetRowKey(Triple t);

        bool InsertData(TKey rowKey, TColumn column);

        bool InsertData(TKey rowKey, IEnumerable<TColumn> columns);

        bool DeleteData(TKey rowKey, TColumn column);

        bool DeleteData(TKey rowKey, IEnumerable<TColumn> columns);

        bool DeleteRow(TKey rowKey);
    }
}
