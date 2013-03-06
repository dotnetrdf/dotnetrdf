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
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the Integrity Constraint Validation Service provided by a Pellet Knowledge Base
    /// </summary>
    public class IntegrityConstraintValidationService : PelletService
    {
        /// <summary>
        /// Creates a new Integrity Constraint Validation Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal IntegrityConstraintValidationService(String name, JObject obj)
            : base(name, obj) { }

#if !SILVERLIGHT

        /// <summary>
        /// Extracts an RDF Dataset which details the Constraints violated (if any) and whether Constraints are satisified
        /// </summary>
        /// <returns></returns>
        public ITripleStore Validate()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Endpoint.Uri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes, MimeTypesHelper.SupportedRdfDatasetMimeTypes);

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    IStoreReader parser = MimeTypesHelper.GetStoreParser(response.ContentType);
                    TripleStore store = new TripleStore();
                    parser.Load(store, new StreamReader(response.GetResponseStream()));

                    response.Close();
                    return store;
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                throw new RdfReasoningException("A HTTP error occurred while communicating with the Pellet Server", webEx);
            }
        }

#endif

        /// <summary>
        /// Extracts an RDF Dataset which details the Constraints violated (if any) and whether Constraints are satisified
        /// </summary>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void Validate(TripleStoreCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Endpoint.Uri);
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes, MimeTypesHelper.SupportedRdfDatasetMimeTypes);

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            request.BeginGetResponse(result =>
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result))
                    {
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugResponse(response);
                        }
#endif
                        IStoreReader parser = MimeTypesHelper.GetStoreParser(response.ContentType);
                        TripleStore store = new TripleStore();
                        parser.Load(store, new StreamReader(response.GetResponseStream()));

                        response.Close();
                        callback(store, state);
                    }
                }, null);
        }

    }
}
