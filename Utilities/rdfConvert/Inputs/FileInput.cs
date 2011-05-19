using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Utilities.Convert.Inputs
{
    class FileInput : BaseInput, IFileConversionInput
    {
        public FileInput(String file)
        {
            this.SourceFile = file;
        }

        public string SourceFile
        {
            get;
            private set;
        }

        public override void Convert()
        {
            if (this.ConversionHandler == null) throw new Exception("Cannot convert the Input File '" + this.SourceFile + "' as rdfConvert could not determine a Conversion Handler to use for the Conversion");

            try
            {
                FileLoader.Load(this.ConversionHandler, this.SourceFile);
            }
            catch
            {
                FileLoader.LoadDataset(this.ConversionHandler, this.SourceFile);
            }
        }

        public override string ToString()
        {
            return "File '" + this.SourceFile + "'";
        }
    }
}
