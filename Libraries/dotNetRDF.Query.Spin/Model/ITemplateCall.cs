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
using VDS.RDF;
using VDS.RDF.Query;
namespace VDS.RDF.Query.Spin.Model
{

    /**
     * A template call.
     * 
     * @author Holger Knublauch
     */
    internal interface ITemplateCall : IModuleCall
    {

        /**
         * Creates a QueryExecution that can be used to execute the associated query
         * with the correct variable bindings.
         * @param dataset  the Dataset to operate on
         * @return the QueryExecution
         */
        SparqlQuery createQueryExecution(IEnumerable<Uri> dataset);


        /**
         * Gets a Map from ArgumentDescriptors to RDFNodes.
         * @return a Map from ArgumentDescriptors to RDFNodes
         */
        Dictionary<IArgument, IResource> getArgumentsMap();


        /**
         * Gets a Map from Properties to RDFNodes derived from the
         * ArgumentDescriptors.
         * @return a Map from Properties to RDFNodes
         */
        Dictionary<IResource, IResource> getArgumentsMapByProperties();


        /**
         * Gets a Map from variable names to RDFNodes derived from the
         * ArgumentDescriptors.
         * @return a Map from variable names to RDFNodes
         */
        Dictionary<String, IResource> getArgumentsMapByVarNames();


        /**
         * Gets the name-value pairs of the template call's arguments as a Jena-friendly
         * initial binding object.
         * @return the initial binding
         */
        Dictionary<String, IResource> getInitialBinding();


        /**
         * Gets this template call as a parsable SPARQL string, with all
         * pre-bound argument variables inserted as constants.
         * @return a SPARQL query string
         * @deprecated  should not be used: has issues if sp:text is used only,
         *              and may produce queries that in fact cannot be parsed back.
         *              As an alternative, consider getting the Command and a
         *              initial bindings mapping, then feed the QueryExecution with
         *              that initial binding for execution.
         */
        String getQueryString();


        /**
         * Gets the associated Template, from the SPINModules registry.
         * @return the template
         */
        ITemplate getTemplate();
    }
}