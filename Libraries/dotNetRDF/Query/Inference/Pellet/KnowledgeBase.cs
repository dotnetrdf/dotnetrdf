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

namespace VDS.RDF.Query.Inference.Pellet
{
    /// <summary>
    /// Represents a Knowledge Base on a Pellet Server
    /// </summary>
    public class KnowledgeBase
    {
        private String _name;
        private List<PelletService> _services = new List<PelletService>();

        /// <summary>
        /// Creates a new Knowledge Base
        /// </summary>
        /// <param name="t">JSON Token for the Object that represents the Service</param>
        internal KnowledgeBase(JToken t)
        {
            _name = (String)t.SelectToken("name");
            foreach (JToken svc in t.SelectToken("kb-services").Children())
            {
                PelletService s = PelletService.CreateService(svc);
                if (s != null) _services.Add(s);
            }
        }

        /// <summary>
        /// Gets the Name of the Knowledge Base
        /// </summary>
        public String Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the Services provided by this Knowledge Base
        /// </summary>
        public IEnumerable<PelletService> Services
        {
            get
            {
                return _services;
            }
        }

        /// <summary>
        /// Gets whether a Service is supported by the Knowledge Base
        /// </summary>
        /// <param name="t">Service Type</param>
        /// <returns></returns>
        public bool SupportsService(Type t)
        {
            foreach (PelletService svc in _services)
            {
                if (t.Equals(svc.GetType())) return true;
            }
            return false;
        }

        /// <summary>
        /// Gets whether a Service is supported by the Knowledge Base
        /// </summary>
        /// <typeparam name="T">Service Type</typeparam>
        /// <returns></returns>
        public bool SupportsService<T>()
            where T : PelletService
        {
            Type target = typeof(T);
            return _services.Any(s => target.Equals(s.GetType()));
        }

        /// <summary>
        /// Gets whether a Service is supported by the Knowledge Base
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <returns></returns>
        public bool SupportsService(String name)
        {
            return _services.Any(s => s.Name.Equals(name));
        }

        /// <summary>
        /// Gets the first available implementation of the given Service Type for this Knowledge Base
        /// </summary>
        /// <param name="t">Service Type</param>
        /// <returns>
        /// Either the Service or a Null if the Knowledge Base does not expose a Service of the given Type
        /// </returns>
        public PelletService GetService(Type t)
        {
            foreach (PelletService svc in _services)
            {
                if (t.Equals(svc.GetType())) return svc;
            }
            return null;
        }

        /// <summary>
        /// Gets the first available implementation of the desired Service Type
        /// </summary>
        /// <typeparam name="T">Desired Service Type</typeparam>
        /// <returns></returns>
        public T GetService<T>()
            where T : PelletService
        {
            Type target = typeof(T);
            foreach (PelletService svc in _services)
            {
                if (target.Equals(svc.GetType())) return (T)svc;
            }
            return (T)null;
        }

        /// <summary>
        /// Gets the first available Service with the given name for this Knowledge Base
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <returns>
        /// Either the Service or a Null if the Knowledge Base does not expose a Service with the given name
        /// </returns>
        public PelletService GetService(String name)
        {
            return _services.FirstOrDefault(s => s.Name.Equals(name));
        }

        /// <summary>
        /// Gets all the available implementations of the given Service Type for this Knowledge Base
        /// </summary>
        /// <param name="t">Service Type</param>
        /// <returns></returns>
        public IEnumerable<PelletService> GetServices(Type t)
        {
            return (from svc in _services
                    where t.Equals(svc.GetType())
                    select svc);
        }

        /// <summary>
        /// Gets all the available services with the given name for this Knowledge Base
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <returns></returns>
        public IEnumerable<PelletService> GetServices(String name)
        {
            return _services.Where(s => s.Name.Equals(name));
        }
    }
}
