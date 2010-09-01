using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.AutoComplete.Data
{
    public class NewDefaultPrefixDeclaration : NewPrefixDeclaration
    {
        public NewDefaultPrefixDeclaration()
            : base(String.Empty) { }

        public override object Content
        {
            get
            {
                return "<New Default Prefix Declaration>";
            }
        }
    }
}
