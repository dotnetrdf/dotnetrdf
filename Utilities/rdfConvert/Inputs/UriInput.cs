/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
