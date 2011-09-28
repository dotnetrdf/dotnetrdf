using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class DatasetFileConnectionDefinition
        : BaseConnectionDefinition
    {
        public DatasetFileConnectionDefinition()
            : base("Dataset File", "Allows you to access a Dataset File in NQuads, TriG or TriX format as a read-only store") { }

        [Connection(DisplayName="Dataset File", DisplayOrder=1, IsRequired=true, AllowEmptyString=false, Type=ConnectionSettingType.File, FileFilter="RDF Dataset Files|*.nq;*.trig;*.trix;*.xml")]
        public String File
        {
            get;
            set;
        }

        [Connection(DisplayName="Load the file asynchronously?", DisplayOrder=2, Type=ConnectionSettingType.Boolean),
         DefaultValue(false)]
        public bool Async
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new DatasetFileManager(this.File, this.Async);
        }
    }
}
