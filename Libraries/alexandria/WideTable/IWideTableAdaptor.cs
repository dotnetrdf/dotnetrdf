using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.WideTable
{
    public interface IWideTableAdaptor<TKey, TColumn> : IDisposable
    {
        bool CreateRow(TKey rowKey);

        bool InsertData(TKey rowKey, TColumn column);

        bool InsertData(TKey rowKey, IEnumerable<TColumn> columns);

        bool DeleteData(TKey rowKey, TColumn column);

        bool DeleteData(TKey rowKey, IEnumerable<TColumn> columns);
    }
}
