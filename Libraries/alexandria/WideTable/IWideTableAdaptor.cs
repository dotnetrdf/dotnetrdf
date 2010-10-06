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

        bool AddGraph(IGraph g);

        bool RemoveGraph(Uri graphUri);

        bool HasGraph(Uri graphUri);

        bool GetGraph(IGraph g, Uri graphUri);

        bool AppendToGraph(Uri graphUri, IEnumerable<Triple> ts);

        bool RemoveFromGraph(Uri graphUri, IEnumerable<Triple> ts);
    }
}
