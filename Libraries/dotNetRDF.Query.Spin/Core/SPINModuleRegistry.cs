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
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.Core
{


    /**
     * A singleton that keeps track of all registered SPIN functions
     * and templates.  For example, in TopBraid this is populated by
     * walking all .spin. files in the Eclipse workspace.  Other
     * implementations may need to register their modules "manually".
     * 
     * @author Holger Knublauch
     */
    internal class SPINModuleRegistry
    {

        /**
         * Remembers all function definitions (in their original Model) so that they
         * can be retrieved later.
         */
        private static Dictionary<Uri, IFunction> functions = new Dictionary<Uri, IFunction>(RDFUtil.uriComparer);

        /**
         * Remembers the source object (e.g. file) that a Function has been loaded from.
         */
        private static Dictionary<IResource, Object> sources = new Dictionary<IResource, Object>();

        /**
         * Remembers all template definitions (in their original Model) so that they
         * can be retrieved later.
         */
        private static Dictionary<Uri, ITemplate> templates = new Dictionary<Uri, ITemplate>(RDFUtil.uriComparer);

        /**
         * Sets the SPINModuleRegistry to another value.
         * @param value  the new value (not null)
         */
        public static void set(SPINModuleRegistry value)
        {
            // TODO Do we really need an instance ???
            //singleton = value;
        }


        /**
         * Gets a registered Function with a given URI.
         * @param uri  the URI of the Function to get
         * @param model  an (optional) Model that should also be used to look up
         *               locally defined functions (currently not used)
         * @return the Function or null if none was found
         */
        public static IFunction getFunction(Uri uri, SpinProcessor model)
        {
            IFunction function = null;
            if (functions.ContainsKey(uri))
            {
                function = functions[uri];
            }
            if (function != null)
            {
                return function;
            }
            if (model != null)
            {
                function = (IFunction)Resource.Get(RDFUtil.CreateUriNode(uri), model).As(typeof(FunctionImpl));
                if (function.hasProperty(RDF.PropertyType, SPIN.ClassFunction))
                {
                    return function;
                }
            }
            return null;
        }


        /**
         * Gets a Collection of all registered Functions.
         * @return the Templates
         */
        public static IEnumerable<IFunction> getFunctions()
        {
            return functions.Values;
        }


        // TODO perhaps the one and only time we will refer to graphs instead of models ?
        /**
         * Gets all Models that are associated to registered functions and templates.
         * @return the Models
         */
        public static HashSet<SpinProcessor> getModels()
        {
            HashSet<SpinProcessor> spinModels = new HashSet<SpinProcessor>();
            foreach (IFunction function in SPINModuleRegistry.getFunctions())
            {
                spinModels.Add(function.getModel());
            }
            foreach (ITemplate template in SPINModuleRegistry.getTemplates())
            {
                spinModels.Add(template.getModel());
            }
            return spinModels;
        }

        public static Object getSource(IFunction function)
        {
            if (sources.ContainsKey(function))
            {
                return sources[function];
            }
            return null;
        }


        /**
         * Gets a Template with a given URI in its defining Model.
         * @param uri  the URI of the Template to look up
         * @param model  an (optional) Model that should also be used for look up
         * @return a Template or null
         */
        public static ITemplate getTemplate(Uri uri, SpinProcessor model)
        {
            if (model != null)
            {
                IResource r = Resource.Get(RDFUtil.CreateUriNode(uri), model);
                if (r.hasProperty(RDF.PropertyType, SPIN.ClassTemplate))
                {
                    return (ITemplate)r.As(typeof(TemplateImpl));
                }
            }
            if (templates.ContainsKey(uri))
            {
                return templates[uri];
            }
            return null;
        }


        /**
         * Gets a Collection of all registered Templates.
         * @return the Templates
         */
        public static IEnumerable<ITemplate> getTemplates()
        {
            return templates.Values;
        }


        /* *
         * Initializes this registry with all system functions and templates
         * from the SPL namespace.
         */
        //public void init()
        //{
        //    IGraph splModel = SPL.GetModel();
        //    IGraph spinModel = SPIN.GetModel();
        //    //TODO find as way to register common modules into 
        //    MultiUnion multiUnion = JenaUtil.createMultiUnion(new Graph[] {
        //    splModel.getGraph(),
        //    spinModel.getGraph()
        //});
        //    multiUnion.setBaseGraph(splModel.getGraph());
        //    Model unionModel = ModelFactory.createModelForGraph(multiUnion);
        //    registerAll(unionModel, null);

        //    FunctionRegistry.get().put(SPIN.eval.Uri, new EvalFunction());
        //    PropertyFunctionRegistry.get().put("http://topbraid.org/spin/owlrl#propertyChainHelper", PropertyChainHelperPFunction);
        //}

        /**
         * Registers a Function with its URI to this registry.
         * As an optional side effect, if the provided function has a spin:body,
         * this method can also register an ARQ FunctionFactory at the current
         * Jena FunctionRegistry, using <code>registerARQFunction()</code>.
         * <b>Note that the Model attached to the function should be an OntModel
         * that also imports the system namespaces spin.owl and sp.owl - otherwise
         * the system may not be able to transform the SPIN RDF into the correct
         * SPARQL string.</b>
         * @param function  the Function (must be a URI resource)
         * @param source  an optional source for the function (e.g. a File)
         * @param addARQFunction  true to also add an entry to the ARQ function registry
         */
        public static void register(IFunction function, Object source, bool addARQFunction)
        {
            functions[function.Uri] = function;
            if (source != null)
            {
                sources[function] = source;
            }
            ExtraPrefixes.Add(function);
            if (addARQFunction)
            {
                registerARQFunction(function);
                if (function.isMagicProperty())
                {
                    registerARQPFunction(function);
                }
            }
        }


        /**
         * Registers a Template with its URI.
         * <b>Note that the Model attached to the template should be an OntModel
         * that also imports the system namespaces spin.owl and sp.owl - otherwise
         * the system may not be able to transform the SPIN RDF into the correct
         * SPARQL string.</b>
         * @param template  the Template (must be a URI resource)
         */
        public static void register(ITemplate template)
        {
            templates[template.Uri] = template;
        }


        /**
         * Registers all functions and templates from a given Model.
         * <b>Note that the Model should contain the triples from the
         * system namespaces spin.owl and sp.owl - otherwise the system
         * may not be able to transform the SPIN RDF into the correct
         * SPARQL string.  In a typical use case, the Model would be
         * an OntModel that also imports the SPIN system namespaces.</b>
         * @param model  the Model to iterate over
         */
        public static void registerAll(SpinProcessor model)
        {
            registerFunctions(model);
            registerTemplates(model);
        }


        /**
         * If the provided Function has an executable body (spin:body), then
         * register an ARQ function for it with the current FunctionRegistry.
         * If there is an existing function with the same URI already registered,
         * then it will only be replaced if it is also a SPINARQFunction.
         * @param spinFunction  the function to register
         */
        protected static void registerARQFunction(IFunction spinFunction)
        {
            //TODO
            //FunctionFactory oldFF = FunctionRegistry.get().get(spinFunction.Uri);
            //if (oldFF == null || oldFF is SPINFunctionFactory)
            //{ // Never overwrite native Java functions
            //    SPINFunctionFactory newFF = SPINFunctionDrivers.get().create(spinFunction);
            //    if (newFF != null)
            //    {
            //        FunctionRegistry.get().put(spinFunction.Uri, newFF);
            //    }
            //}
        }


        /**
         * If the provided Function has an executable body (spin:body), then
         * register an ARQ function for it with the current FunctionRegistry. 
         * If there is an existing function with the same URI already registered,
         * then it will only be replaced if it is also a SPINARQPFunction.
         * @param function  the function to register
         */
        public static void registerARQPFunction(IFunction function)
        {
            //TODO
            if (function.hasProperty(SPIN.PropertyBody))
            {
                //PropertyFunctionFactory old = PropertyFunctionRegistry.get().get(function.Uri);
                //if (old == null || old is SPINARQPFunction)
                //{
                //    SPINARQPFunction arqFunction = new SPINARQPFunction(function);
                //    PropertyFunctionRegistry.get().put(function.Uri, arqFunction);
                //}
            }
        }


        /**
         * Registers all functions defined in a given Model.
         * This basically iterates over all instances of spin:Function and calls
         * <code>register(function)</code> for each of them.
         * @param model  the Model to add the functions of
         * @param source  an optional source of the Model
         */
        public static void registerFunctions(SpinProcessor model)
        {
            foreach (IResource resource in model.GetAllInstances(SPIN.ClassFunction))
            {
                IFunction function = SPINFactory.asFunction(resource);
                register(function, function.getSource().Graph, true);
            }
        }


        /**
         * Registers all templates defined in a given Model.
         * This basically iterates over all instances of spin:Template and calls
         * <code>register(template)</code> for each of them.
         * @param model  the Model to add the templates of
         */
        public static void registerTemplates(SpinProcessor model)
        {
            foreach (IResource resource in model.GetAllInstances(SPIN.ClassTemplate))
            {
                if (resource.isUri())
                {
                    ITemplate template = (ITemplate)resource.As(typeof(TemplateImpl));
                    register(template);
                    ExtraPrefixes.Add(template);
                }
            }
        }


        /**
         * Resets this registry, supporting things like server restarts.
         */
        public static void reset()
        {
            functions.Clear();
            sources.Clear();
            templates.Clear();
        }
    }
}