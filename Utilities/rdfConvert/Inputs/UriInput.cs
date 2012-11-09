/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
