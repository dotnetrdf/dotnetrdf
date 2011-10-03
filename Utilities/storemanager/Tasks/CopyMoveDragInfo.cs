using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    class CopyMoveDragInfo
    {
        public CopyMoveDragInfo(StoreManagerForm form, String sourceUri)
        {
            this.Form = form;
            this.Source = form.Manager;
            this.SourceUri = sourceUri;
        }

        public StoreManagerForm Form
        {
            get;
            private set;
        }

        public IGenericIOManager Source
        {
            get;
            private set;
        }

        public String SourceUri
        {
            get;
            private set;
        }
    }
}
