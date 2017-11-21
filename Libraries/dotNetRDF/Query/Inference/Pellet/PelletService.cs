/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
            _name = name;
            JToken mimeTypes = obj.SelectToken("response-mimetype");
            foreach (JToken mimeType in mimeTypes.Children())
            {
                _mimeTypes.Add((String)mimeType);
            }
            _endpoint = new ServiceEndpoint((JObject)obj.SelectToken("endpoint"));
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
                    // Classify Service
                    return new ClassifyService(serviceName, obj);

                case PelletHelper.ServiceCluster:
                    // Clustering Service
                    return new ClusterService(serviceName, obj);

                case PelletHelper.ServiceConsistency:
                    // Consistency Service
                    return new ConsistencyService(serviceName, obj);

                case PelletHelper.ServiceExplain:
                    // Explain Service
                    return new ExplainService(serviceName, obj);

                case PelletHelper.ServiceExplainInconsistent:
                    // Explain Inconstistent Service
                    return new ExplainInconsistentService(serviceName, obj);

                case PelletHelper.ServiceExplainInstance:
                    // Explain Instance Service
                    return new ExplainInstanceService(serviceName, obj);

                case PelletHelper.ServiceExplainProperty:
                    // Explain Property Service
                    return new ExplainPropertyService(serviceName, obj);

                case PelletHelper.ServiceExplainSubclass:
                    // Explain Subclass Service
                    return new ExplainSubclassService(serviceName, obj);

                case PelletHelper.ServiceExplainUnsat:
                    // Explain Unsat Service
                    return new ExplainUnsatService(serviceName, obj);

                case PelletHelper.ServiceIntegrityConstraintValidation:
                    // ICV Service
                    return new IntegrityConstraintValidationService(serviceName, obj);

                case PelletHelper.ServicePredict:
                    // Prediction Service
                    return new PredictService(serviceName, obj);

                case PelletHelper.ServiceQuery:
                    // SPARQL Query Service
                    return new QueryService(serviceName, obj);

                case PelletHelper.ServiceRealize:
                    // Knowledge Base Realization Service
                    return new RealizeService(serviceName, obj);

                case PelletHelper.ServiceSearch:
                    // Search Service
                    return new SearchService(serviceName, obj);

                case PelletHelper.ServiceSimilarity:
                    // Similarity Service
                    return new SimilarityService(serviceName, obj);

                case PelletHelper.ServiceNamespaces:
                    // Namespace Service
                    return new NamespaceService(serviceName, obj);

                case PelletHelper.ServiceKBDescription:
                case PelletHelper.ServiceServerDescription:
                    // Description Services don't have concrete implementations and are ignored
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
                return _name;
            }
        }

        /// <summary>
        /// Gets the Endpoint for this Service
        /// </summary>
        public ServiceEndpoint Endpoint
        {
            get
            {
                return _endpoint;
            }
        }

        /// <summary>
        /// Gets the Response MIME Types supported by the Service
        /// </summary>
        public IEnumerable<String> MimeTypes
        {
            get
            {
                return _mimeTypes;
            }
        }

    }
}
