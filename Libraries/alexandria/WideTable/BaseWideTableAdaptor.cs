using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;
using Alexandria.WideTable.ColumnSchema;

namespace Alexandria.WideTable
{
    public abstract class BaseWideTableAdaptor<TColumn> : IWideTableAdaptor<String, TColumn>
    {
        private SHA256Managed _hash;
        private NQuadsFormatter _formatter = new NQuadsFormatter();

        public abstract IColumnSchema<TColumn> ColumnSchema
        {
            get;
        }

        public String GetRowKey(Triple t)
        {
            //Only instantiate the SHA256 class when we first use it
            if (_hash == null) _hash = new SHA256Managed();

            Byte[] input = Encoding.UTF8.GetBytes(this._formatter.Format(t));
            Byte[] output = _hash.ComputeHash(input);

            StringBuilder hash = new StringBuilder();
            foreach (Byte b in output)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }

        public abstract bool InsertData(String rowKey, TColumn column);

        public abstract bool InsertData(String rowKey, IEnumerable<TColumn> columns);

        public abstract bool DeleteData(String rowKey, TColumn column);

        public abstract bool DeleteData(String rowKey, IEnumerable<TColumn> columns);

        public abstract bool DeleteRow(String rowKey);

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
    }
}
