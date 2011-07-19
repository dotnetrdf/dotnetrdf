using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public class NewBlankNodeData : BaseCompletionData
    {
        private String _id;

        public NewBlankNodeData(String id)
            : base("<New Blank Node>", "_:" + id, "Inserts a new Blank Node", 50.0d) { }
    }
}
