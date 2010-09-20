/* 
 * Copyright (C) 2007, Andrew Matthews http://aabs.wordpress.com/
 *
 * This file is Free Software and part of LinqToRdf http://code.google.com/fromName/linqtordf/
 *
 * It is licensed under the following license:
 *   - Berkeley License, V2.0 or any newer version
 *
 * You may not use this file except in compliance with the above license.
 *
 * See http://code.google.com/fromName/linqtordf/ for the complete text of the license agreement.
 *
 */
using System;
using System.Diagnostics;
using System.Reflection;
using VDS.RDF.Parsing;


namespace VDS.RDF.Linq
{
    public static class MemoryStoreExtensions
    {
        static Logger Logger  = new Logger(typeof(MemoryStoreExtensions));

        public static IGraph GetDefaultGraph(this IInMemoryQueryableStore ms)
        {
            if (ms.HasGraph(null))
            {
                ms.Add(new Graph());
            }
            return ms.Graph(null);
        }

        public static void Add(this IInMemoryQueryableStore ms, OwlInstanceSupertype oc)
        {
            using (var ls = new LoggingScope("MemoryStoreExtensions.Add"))
            {
                Type t = oc.GetType();
                Console.WriteLine(oc.InstanceUri);
                PropertyInfo[] pia = t.GetProperties();
                IGraph g = ms.GetDefaultGraph();
                g.Assert(g.CreateUriNode(new Uri(oc.InstanceUri)), g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri(OwlClassSupertype.GetOwlClassUri(t))));
                foreach (PropertyInfo pi in pia)
                {
                    if (pi.IsOntologyResource())
                    {
                        AddPropertyToStore(oc, pi, ms);
                    }
                }
            }
        }

        private static void AddPropertyToStore(OwlInstanceSupertype supertype, PropertyInfo pi, IInMemoryQueryableStore ms)
        {
            if (supertype == null)
                throw new ArgumentNullException("supertype");
            if (pi == null)
                throw new ArgumentNullException("pi");
            if (ms == null)
                throw new ArgumentNullException("ms");

            if (pi.GetValue(supertype, null) != null)
            {
                Add(supertype.InstanceUri, pi.GetOwlResourceUri(), pi.GetValue(supertype, null).ToString(), ms);
                #region Tracing
#line hidden
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Added property {0} to store.", pi.Name);
                }
#line default
                #endregion
            }
        }

        public static void Add(string s, string p, string o, IInMemoryQueryableStore ms)
        {
            if (!string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(p) && !string.IsNullOrEmpty(o))
            {
                IGraph g = ms.GetDefaultGraph();
                g.Assert(g.CreateUriNode(new Uri(s)), g.CreateUriNode(new Uri(p)), g.CreateLiteralNode(o));
                #region Tracing
#line hidden
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Added <{0}> <{1}> <{2}>.", s,p,o);
                }
#line default
                #endregion
            }
        }
    }

}