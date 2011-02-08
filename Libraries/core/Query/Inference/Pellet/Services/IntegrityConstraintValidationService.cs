/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    public class IntegrityConstraintValidationService : PelletService
    {
        /// <summary>
        /// Creates a new Integrity Constraint Validation Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal IntegrityConstraintValidationService(String name, JObject obj)
            : base(name, obj) { }

        /// <summary>
        /// Extracts an RDF Dataset which 
        /// </summary>
        /// <returns></returns>
        public ITripleStore Validate()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Endpoint.Uri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.ContentType = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes, MimeTypesHelper.SupportedRdfDatasetMimeTypes);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                IStoreReader parser = MimeTypesHelper.GetStoreParser(response.ContentType);
                TripleStore store = new TripleStore();
                StreamParams parameters = new StreamParams(response.GetResponseStream());
                parser.Load(store, parameters);

                response.Close();
                return store;
            }
        }

    }
}
