using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Convert.Inputs
{
    interface IConversionInput
    {
        IRdfHandler ConversionHandler
        {
            get;
            set;
        }

        void Convert();
    }

    interface IFileConversionInput : IConversionInput
    {
        String SourceFile
        {
            get;
        }
    }

    interface IUriConversionInput : IConversionInput
    {
        Uri SourceUri
        {
            get;
        }
    }
}
