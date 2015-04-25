/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Spin.SparqlStrategies;

namespace VDS.RDF.Query.Spin
{

    public class ConfigurationObjectFactory
        : IObjectFactory
    {
        private Type _factoryType = typeof(ISparqlHandlingStrategy);

        private const String SpinStorageProviderClassName = "VDS.RDF.Query.Spin.SpinStorageProvider",
                             SpinSupportStrategyClassName = "VDS.RDF.Query.Spin.SpinSupportStrategy",
                             TransactionSupportStrategyClassName = "VDS.RDF.Query.Spin.TransactionSupportStrategy";

        // TODO add a static instance registry indexed by objNode ?
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            //Create the URI Nodes we're going to use to search for things
            INode propStorageProvider = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

            switch (targetType.FullName)
            {
                case SpinStorageProviderClassName:
                    // Get the configuration node required to create IStorageProvider instances
                    INode storageNode = g.GetTriplesWithSubjectPredicate(objNode, propStorageProvider).Select(t => t.Object).FirstOrDefault();
                    if (storageNode == null) throw new DotNetRdfConfigurationException("Missing the underlying storage configuration");

                    // creates the SpinStorageProvider
                    obj = new SpinStorageProvider(g, objNode, storageNode);
                    // TODO try to build the spin model associated to this storage ?
                    return true;

                default:
                    try
                    {
                        obj = (ISparqlHandlingStrategy)Activator.CreateInstance(targetType);
                        return true;
                    }
                    catch
                    {
                        //Any error means this loader can't load this type
                        return false;
                    }
            }
        }

        public bool CanLoadObject(Type t)
        {
            //We can load SpinStorageProvider or any object which implements ISparqlHandlingStrategy and has a public unparameterized constructor
            return t.FullName == SpinStorageProviderClassName
                || (t.GetInterfaces().Contains(this._factoryType) && t.GetConstructors().Any(c => c.GetParameters().Length == 0 && c.IsPublic));
        }
    }
}