using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    public class BlankNodeData : BaseCompletionData
    {
        public BlankNodeData(String id)
            : base(id, id, "Blank Node with ID " + id.Substring(2)) { }
    }
}
