using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public class QNameData : BaseCompletionData
    {
        private String _qname, _description = String.Empty;
        private double _priority = 0.0d;

        public QNameData(String qname)
            : this(qname, String.Empty) { }

        public QNameData(String qname, String description)
            : base(qname, qname, description) { }
    }
}
