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
            this._name = (String)t.SelectToken("name");
            foreach (JToken svc in t.SelectToken("kb-services").Children())
            {
                PelletService s = PelletService.CreateService(svc);
                if (s != null) this._services.Add(s);
            }
        }

        /// <summary>
        /// Gets the Name of the Knowledge Base
        /// </summary>
        public String Name
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Gets the Services provided by this Knowledge Base
        /// </summary>
        public IEnumerable<PelletService> Services
        {
            get
            {
                return this._services;
            }
        }

        /// <summary>
        /// Gets whether a Service is supported by the Knowledge Base
        /// </summary>
        /// <param name="t">Service Type</param>
        /// <returns></returns>
        public bool SupportsService(Type t)
        {
            foreach (PelletService svc in this._services)
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
            return this._services.Any(s => target.Equals(s.GetType()));
        }

        /// <summary>
        /// Gets whether a Service is supported by the Knowledge Base
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <returns></returns>
        public bool SupportsService(String name)
        {
            return this._services.Any(s => s.Name.Equals(name));
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
            foreach (PelletService svc in this._services)
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
            foreach (PelletService svc in this._services)
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
            return this._services.FirstOrDefault(s => s.Name.Equals(name));
        }

        /// <summary>
        /// Gets all the available implementations of the given Service Type for this Knowledge Base
        /// </summary>
        /// <param name="t">Service Type</param>
        /// <returns></returns>
        public IEnumerable<PelletService> GetServices(Type t)
        {
            return (from svc in this._services
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
            return this._services.Where(s => s.Name.Equals(name));
        }
    }
}
