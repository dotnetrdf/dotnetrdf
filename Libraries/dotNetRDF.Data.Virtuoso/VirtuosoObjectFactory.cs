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
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// An Object Factory that can create objects of the classes provided by the dotNetRDF.Data.Virtuoso library
    /// </summary>
    public class VirtuosoObjectFactory
        : IObjectFactory
    {
        private const String Virtuoso = "VDS.RDF.Storage.VirtuosoManager";

        /// <summary>
        /// Attempts to load an Object of the given type identified by the given Node and returned as the Type that this loader generates
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Created Object</param>
        /// <returns>True if the loader succeeded in creating an Object</returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            String server, port, db, user, pwd;
            int p = -1, timeout = 0;

            //Create the URI Nodes we're going to use to search for things
            INode propServer = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer)),
                  propDb = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDatabase));

            //Get Server and Database details
            server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
            if (server == null) server = "localhost";
            db = ConfigurationLoader.GetConfigurationString(g, objNode, propDb);
            if (db == null) db = VirtuosoManager.DefaultDB;

            //Get Port
            port = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPort)));
            if (!Int32.TryParse(port, out p)) p = VirtuosoManager.DefaultPort;

            //Get timeout
            timeout = ConfigurationLoader.GetConfigurationInt32(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyTimeout)), timeout);

            //Get user credentials
            ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);
            if (user == null || pwd == null) return false;

            //Based on this information create a Manager if possible
            switch (targetType.FullName)
            {
                case Virtuoso:
                    //Get Server settings
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;

                    obj = new VirtuosoManager(server, p, db, user, pwd, timeout);

                    break;
            }

            return (obj != null);
        }

        /// <summary>
        /// Returns whether this Factory is capable of creating objects of the given type
        /// </summary>
        /// <param name="t">Target Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case Virtuoso:
                    return true;
                default:
                    return false;
            }
        }
    }
}
