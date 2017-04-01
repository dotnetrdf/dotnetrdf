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
using System.Text;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.SparqlUtil;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Spin;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Spin.Core
{

    /**
     * Some static util methods for SPIN that don't fit anywhere else.
     * 
     * @author Holger Knublauch
     */
    public class SPINUtil
    {

        /**
         * The name of the variable that will be used in type binding
         * triple patterns (?this rdf:type ?TYPE_CLASS)
         */
        public const String TYPE_CLASS_VAR_NAME = "TYPE_CLASS";


        /**
         * Collects all queries or template calls at a given class.
         * @param cls  the class to get the queries at
         * @param predicate  the predicate such as <code>spin:rule</code>
         * @param results  the List to add the results to
         */
        public static void addQueryOrTemplateCalls(IResource cls, INode predicate, List<QueryOrTemplateCall> results)
        {
            IEnumerable<Triple> ss = cls.listProperties(predicate);

            // Special case: we might have an instance of a template call like spl:Attribute
            //               Then try to find the Template in the registry
            if (!ss.Any() && cls != null && cls.isUri())
            {
                ITemplate template = SPINModuleRegistry.getTemplate(cls.Uri(), null);
                if (template != null)
                {
                    ss = template.listProperties(predicate);
                }
            }

            foreach (Triple s in ss)
            {
                if (!(s.Object is ILiteralNode))
                {
                    ITemplateCall templateCall = SPINFactory.asTemplateCall(Resource.Get(s.Object, cls.getModel()));
                    if (templateCall != null)
                    {
                        results.Add(new QueryOrTemplateCall(cls, templateCall));
                    }
                    else
                    {
                        IQuery query = SPINFactory.asQuery(Resource.Get(s.Object, cls.getModel()));
                        if (query != null)
                        {
                            results.Add(new QueryOrTemplateCall(cls, query));
                        }
                    }
                }
            }
        }


        /**
         * Inserts a statement  ?this a ?TYPE_CLASS .  after the WHERE { keyword. 
         * Since we rely on the underlying engine to perform the job and cannot be sure that indference is supported on the given dataset, we need to force it.
         * @param str  the input String
         */
        public static String addThisTypeClause(String str)
        {
            // TODO check wheter we use a parameter or a variable binding for the Class value
            int index = str.IndexOf("WHERE {") + 7;
            StringBuilder sb = new StringBuilder(str);
            sb.Insert(index, " ?this a ?spinExecClass . FILTER ( sameTerm(?spinExecClass, @" + TYPE_CLASS_VAR_NAME + ")  || EXISTS {?spinExecClass rdfs:subClassOf+ @" + TYPE_CLASS_VAR_NAME + "} ) . ");
            return sb.ToString();
        }


        /**
         * Applies variable bindings, replacing the values of one map with
         * the values from a given variables map.
         * @param map  the Map to modify
         * @param bindings  the current variable bindings
         */
        public static void applyBindings(Dictionary<IResource, IResource> map, Dictionary<String, IResource> bindings)
        {
            foreach (IResource property in new List<IResource>(map.Keys))
            {
                IResource value = map[property];
                IVariable var = SPINFactory.asVariable(value);
                if (var != null)
                {
                    String varName = var.getName();
                    if (bindings.ContainsKey(varName))
                    {
                        IResource b = bindings[varName];
                        if (b != null)
                        {
                            map[property] = b;
                        }
                    }
                }
            }
        }


        /**
         * Binds the variable ?this with a given value.
         * @param qexec  the QueryExecution to modify
         * @param value  the value to bind ?this with
         */
        public static void bindThis(SparqlParameterizedString qexec, INode value)
        {
            if (value != null)
            {
                qexec.SetParameter(SPIN.THIS_VAR_NAME, value);
            }
        }


        /**
         * Checks whether a given query mentions the variable ?this anywhere.
         * This can be used to check whether ?this needs to be bound before
         * execution, etc.
         * @param command  the query to test
         * @return null to indicate it was not found, an int >= 0 representing
         * 			the max depth inside query element tree otherwise
         */
        public static int containsThis(ICommandWithWhere command)
        {
            return new ContainsVarChecker(SPIN.Property_this).checkDepth(command);
        }


        // TODO check that this could be handled in the store but this is not used in the project
        /**
         * Executes a given SELECT query and returns the first value of the first result
         * variable, if any exists.  The QueryExecution is closed at the end.
         * @param qexec  the QueryExecution to execute
         * @return the first result or null
         */
        public static INode getFirstResult(SparqlQuery qexec)
        {
            return null;
            //try
            //{
            //    SparqlResultSet rs = qexec.execSelect();
            //    if (rs.Count > 0)
            //    {
            //        String varName = rs.Variables.First();
            //        INode result = rs[0][varName];
            //        return result;
            //    }
            //    else
            //    {
            //        return null;
            //    }
            //}
            //finally
            //{
            //    qexec.close();
            //}
        }


        /**
         * Attempts to convert a given INode to a String so that it can be parsed into
         * a Jena query object.  The node must be either a string Literal, or a sp:Query node
         * or a template call.  If it's a template call then the resulting query string will
         * "hard-bind" the template variables.
         * @param node  the INode to convert
         * @param usePrefixes  true to use qname abbreviations
         * @return the String representation of node
         * @throws ArgumentException  if the node is not a valid SPIN Query or a String
         * @deprecated for the same reason as {@link TemplateCall.getQueryString}
         */
        public static String getQueryString(IResource node, bool usePrefixes)
        {
            if (node.isLiteral())
            {
                return ((IValuedNode)node.getSource()).AsString();
            }
            else
            {
                ICommand spinCommand = SPINFactory.asCommand(node);
                if (spinCommand != null)
                {
                    if (usePrefixes)
                    {
                        //StringSparqlPrinter p = new StringSparqlPrinter();
                        //p.setUsePrefixes(usePrefixes);
                        //spinCommand.print(p);
                        //return p.getString();
                        return String.Empty;
                    }
                    else
                    {
                        return "";//ARQFactory.get().createCommandString(spinCommand);
                    }
                }
                else
                {
                    ITemplateCall templateCall = SPINFactory.asTemplateCall(node);
                    if (templateCall != null)
                    {
                        return templateCall.getQueryString();
                    }
                    else
                    {
                        throw new ArgumentException("INode must be either literal or a SPIN query or a SPIN template call");
                    }
                }
            }
        }


        /**
         * Gets a HashSet of all query strings defined as values of a given property.
         * This will accept strings or SPIN expressions (including template calls).
         * The query model is the subject's getModel().
         * All sub-properties of property from the query model will also be queried.
         * @param subject  the subject to get the values of
         * @param property  the property to query
         * @return a Set of query strings
         * @deprecated for the same reasons as {@link TemplateCall.getQueryString}
         */
        public static IEnumerable<String> getQueryStrings(IResource subject, IResource property)
        {
            //JenaUtil.setGraphReadOptimization(true);
            try
            {
                Dictionary<Triple, String> map = getQueryStringMap(subject, property);
                return map.Values;
            }
            finally
            {
                //JenaUtil.setGraphReadOptimization(false);
            }
        }


        /**
         * Gets a Map of all query strings defined as values of a given property.
         * This will accept strings or SPIN expressions (including template calls).
         * The query model is the subject's getModel().
         * All sub-properties of property from the query model will also be queried.
         * The resulting Map will associate each query String with the Triple
         * that has created it.
         * @param subject  the subject to get the values of
         * @param property  the property to query
         * @return a Map of Statements to query strings
         */
        public static Dictionary<Triple, String> getQueryStringMap(IResource subject, IResource property)
        {
            //if(subject != null) {
            //    property = property;
            //}
            SpinProcessor model = property.getModel();
            Dictionary<Triple, String> queryStrings = new Dictionary<Triple, String>();
            IEnumerable<IResource> ps = property.getModel().GetAllSubProperties(property, true);
            foreach (IResource p in ps)
            {
                IEnumerator<Triple> it = subject.listProperties(p).GetEnumerator();
                while (it.MoveNext())
                {
                    Triple s = it.Current;
                    IResource obj = Resource.Get(s.Object, model);
                    String str = getQueryString(obj, false);
                    queryStrings[s] = str;
                }
            }
            return queryStrings;
        }


        public static HashSet<IResource> getURIResources(IPrintable query)
        {
            /*sealed*/
            HashSet<IResource> results = new HashSet<IResource>();
            //StringSparqlPrinter context = new StringSparqlPrinter();// {

            //    override 		public PrintContext clone() {
            //        return this;
            //    }

            //    override 		public void printURIResource(INode resource) {
            //        base.printURIResource(resource);
            //        results.Add(resource);
            //    }
            //};
            //query.print(context);
            return results;
        }


        /**
         * Checks whether a given Graph is a spin:LibraryOntology.
         * This is true for the SP and SPIN namespaces, as well as any Graph that
         * has [baseURI] rdf:type spin:LibraryOntology.
         * @param graph  the Graph to test
         * @param baseURI  the base URI of the Graph (to find the library ontology)
         * @return true if graph is a library ontology
         */
        public static bool isLibraryOntology(Graph graph, Uri baseURI)
        {
            if (baseURI != null)
            {
                if (SP.BASE_URI.Equals(baseURI.ToString()) || SPIN.BASE_URI.Equals(baseURI.ToString()))
                {
                    return true;
                }
                else
                {
                    INode ontology = graph.CreateUriNode(UriFactory.Create(baseURI.ToString()));
                    return graph.ContainsTriple(new Triple(ontology, RDF.PropertyType, SPIN.ClassLibraryOntology));
                }
            }
            else
            {
                return false;
            }
        }


        public static bool isRootClass(IResource cls)
        {
            return RDFUtil.sameTerm(RDFS.ClassResource, cls) || RDFUtil.sameTerm(OWL.Thing, cls);
        }


        /**
         * Converts a map from Properties to INode values to a Map from variable
         * names (Strings) to those values, for quicker look up.
         * @param map  the old Map
         * @return the new Map
         */
        public static Dictionary<String, IResource> mapProperty2VarNames(Dictionary<IResource, IResource> map)
        {
            Dictionary<String, IResource> results = new Dictionary<String, IResource>();
            foreach (IResource predicate in map.Keys)
            {
                IResource value = map[predicate];
                // TODO: change uri with qName if possible
                results[predicate.ToString()] = value;
            }
            return results;
        }
    }
}