using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;

namespace VDS.RDF.Utilities.Convert.Inputs
{
    class UriInput : BaseInput, IUriConversionInput
    {
        public UriInput(Uri u)
        {
            this.SourceUri = u;
        }

        public Uri SourceUri { get; private set; }

        public override void Convert()
        {
            if (this.ConversionHandler == null) throw new Exception("Cannot convert the Input URI '" + this.SourceUri.ToString() + "' as rdfConvert could not determine a Conversion Handler to use for the Conversion");

            try
            {
                UriLoader.Load(this.ConversionHandler, this.SourceUri);
            }
            catch
            {
                UriLoader.LoadDataset(this.ConversionHandler, this.SourceUri);
            }
        }

        public override string ToString()
        {
            return "URI '" + this.SourceUri.AbsoluteUri + "'";
        }
    }
}
