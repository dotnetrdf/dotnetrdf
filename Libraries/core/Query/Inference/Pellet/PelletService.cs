/*

Copyright Robert Vesse 2009-10
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
using System.Linq;
using Newtonsoft.Json.Linq;
using VDS.RDF.Query.Inference.Pellet.Services;

namespace VDS.RDF.Query.Inference.Pellet
{
    /// <summary>
    /// Class representing Services provided by a Pellet Server Knowledge Base
    /// </summary>
    public abstract class PelletService
    {
        private String _name;
        private ServiceEndpoint _endpoint;
        private List<String> _mimeTypes = new List<string>();

        /// <summary>
        /// Creates a new Pellet Service instance
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object representing the Service</param>
        protected PelletService(String name, JObject obj)
        {
            this._name = name;
            JToken mimeTypes = obj.SelectToken("response-mimetype");
            foreach (JToken mimeType in mimeTypes.Children())
            {
                this._mimeTypes.Add((String)mimeType);
            }
            this._endpoint = new ServiceEndpoint((JObject)obj.SelectToken("endpoint"));
        }

        /// <summary>
        /// Factory method for generating concrete Pellet Service instances representing different Pellet Services
        /// </summary>
        /// <param name="t">JSON Object representing the Service</param>
        /// <returns></returns>
        internal static PelletService CreateService(JToken t)
        {
            String serviceName = ((JProperty)t).Name;
            JObject obj = (JObject)t.Children().First();

            switch (serviceName)
            {
                case PelletHelper.ServiceClassify:
                    //Classify Service
                    return new ClassifyService(serviceName, obj);

                case PelletHelper.ServiceCluster:
                    //Clustering Service
                    return new ClusterService(serviceName, obj);

                case PelletHelper.ServiceConsistency:
                    //Consistency Service
                    return new ConsistencyService(serviceName, obj);

                case PelletHelper.ServiceExplain:
                    //Explain Service
                    return new ExplainService(serviceName, obj);

                case PelletHelper.ServiceExplainInconsistent:
                    //Explain Inconstistent Service
                    return new ExplainInconsistentService(serviceName, obj);

                case PelletHelper.ServiceExplainInstance:
                    //Explain Instance Service
                    return new ExplainInstanceService(serviceName, obj);

                case PelletHelper.ServiceExplainProperty:
                    //Explain Property Service
                    return new ExplainPropertyService(serviceName, obj);

                case PelletHelper.ServiceExplainSubclass:
                    //Explain Subclass Service
                    return new ExplainSubclassService(serviceName, obj);

                case PelletHelper.ServiceExplainUnsat:
                    //Explain Unsat Service
                    return new ExplainUnsatService(serviceName, obj);

                case PelletHelper.ServiceIntegrityConstraintValidation:
                    //ICV Service
                    return new IntegrityConstraintValidationService(serviceName, obj);

                case PelletHelper.ServicePredict:
                    //Prediction Service
                    return new PredictService(serviceName, obj);

                case PelletHelper.ServiceQuery:
                    //SPARQL Query Service
                    return new QueryService(serviceName, obj);

                case PelletHelper.ServiceRealize:
                    //Knowledge Base Realization Service
                    return new RealizeService(serviceName, obj);

                case PelletHelper.ServiceSearch:
                    //Search Service
                    return new SearchService(serviceName, obj);

                case PelletHelper.ServiceSimilarity:
                    //Similarity Service
                    return new SimilarityService(serviceName, obj);

                case PelletHelper.ServiceNamespaces:
                    //Namespace Service
                    return new NamespaceService(serviceName, obj);

                case PelletHelper.ServiceKBDescription:
                case PelletHelper.ServiceServerDescription:
                    //Description Services don't have concrete implementations and are ignored
                    return null;
                default:
                    return new UnsupportedService(serviceName, obj);
            }
        }

        /// <summary>
        /// Gets the Name of the Service
        /// </summary>
        public String Name
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Gets the Endpoint for this Service
        /// </summary>
        public ServiceEndpoint Endpoint
        {
            get
            {
                return this._endpoint;
            }
        }

        /// <summary>
        /// Gets the Response MIME Types supported by the Service
        /// </summary>
        public IEnumerable<String> MimeTypes
        {
            get
            {
                return this._mimeTypes;
            }
        }

    }
}
