using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aquiles;
using Aquiles.Model;
using VDS.RDF;
using VDS.RDF.Writing.Formatting;

namespace VDS.Alexandria.WideTable.ColumnSchema
{
    public class CassandraSchema : BaseSimpleColumnSchema<AquilesColumn>
    {
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        private AquilesColumn CreateColumn(String name)
        {
            AquilesColumn column = new AquilesColumn();
            column.ColumnName = Aquiles.Helpers.Encoders.ByteEncoderHelper.UTF8Encoder.ToByteArray(name);
            return column;
        }

        protected override AquilesColumn ToColumn(string columnName, INode value)
        {
            AquilesColumn column = this.CreateColumn(columnName);
            column.Value = Aquiles.Helpers.Encoders.ByteEncoderHelper.UTF8Encoder.ToByteArray(this._formatter.Format(value));
            return column;
        }

        protected override AquilesColumn ToColumn(string columnName, Uri uri)
        {
            AquilesColumn column = this.CreateColumn(columnName);
            column.Value = Aquiles.Helpers.Encoders.ByteEncoderHelper.UTF8Encoder.ToByteArray(this._formatter.FormatUri(uri));
            return column;
        }

        protected override AquilesColumn GetColumnWithName(IEnumerable<AquilesColumn> columns, string name)
        {
            AquilesColumn temp = columns.FirstOrDefault(c => c.ColumnName.Equals(name));
            if (temp == null) throw new AlexandriaException("Retrieved Columns do not contain the required Columns for use with this Column Schema");
            return temp;
        }

        protected override INode FromColumn(IGraph g, AquilesColumn column)
        {
            throw new NotImplementedException();
        }

        protected override Uri FromColumn(AquilesColumn column)
        {
            String temp = Aquiles.Helpers.Encoders.ByteEncoderHelper.UTF8Encoder.FromByteArray(column.Value);
            return new Uri(temp.Replace("\\>", ">"));
        }
    }
}
