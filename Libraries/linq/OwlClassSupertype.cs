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
using System.Collections.Generic;
using System.Reflection;

namespace VDS.RDF.Linq
{
    /// <summary>
    /// This is the supertype of all classes that can be used with LinqToRdf. 
    /// It provides facilities for getting metadata, and doing reflection.
    /// </summary>
    public class OwlClassSupertype
    {
        /// <summary>
        /// Gets or sets the data context within which this instance is to be managed.
        /// </summary>
        /// <value>An <see cref="IRdfContext"/>.</value>
        /// <remarks>
        /// Not guaranteed to be in scope - it's an <see cref="IDisposable"/> (or will be).
        /// </remarks>
        public IRdfContext DataContext { get; set; }
        /// <summary>
        /// Gets all properties that are adorned with the <see cref="OwlResourceAttribute"/>.
        /// </summary>
        public IEnumerable<MemberInfo> AllPersistentProperties
        {
            get
            {
                foreach (MemberInfo propertyInfo in GetType().GetProperties())
                {
                    if (propertyInfo.IsOntologyResource())
                    {
                        yield return propertyInfo;
                    }
                }
            }
        }
        /// <summary>
        /// Gets all properties that are adorned with the <see cref="OwlResourceAttribute"/>.
        /// </summary>
        /// <param name="t">The <see cref="Type"/> that contains the persistent properties to search for.</param>
        /// <returns>A sequence of <see cref="MemberInfo"/> for each member that is persistent</returns>
        public static IEnumerable<MemberInfo> GetAllPersistentProperties(Type t)
        {
            foreach (PropertyInfo propertyInfo in t.GetProperties())
            //foreach (MemberInfo propertyInfo in t.GetProperties())
            {
                // CMSB:
                // Skip EntitySet / EntityRef type properties.
                if (propertyInfo.PropertyType.IsGenericType &&
                    propertyInfo.PropertyType.GetGenericTypeDefinition().Name.StartsWith("Entity"))
                    continue;

                if (propertyInfo.IsOntologyResource())
                {
                    yield return propertyInfo;
                }
            }
        }

        /// <summary>
        /// Gets the URI used for a corresponding class in the ontology. This is taken from the 
        /// <see cref="OwlResourceAttribute"/> attached to the class.
        /// </summary>
        /// <param name="t">The <see cref="Type"/> to get the URI for.</param>
        /// <returns>a string representation of the URI</returns>
        public static string GetOntologyBaseUri(Type t)
        {
            return t.GetOntology().BaseUri;
        }

        /// <summary>
        /// Gets the URI used for a corresponding class in the ontology. This is taken from the 
        /// <see cref="OwlResourceAttribute"/> attached to the class.
        /// </summary>
        /// <param name="t">The <see cref="Type"/> to get the URI for.</param>
        /// <returns>a string representation of the URI</returns>
        public static string GetOwlClassUri(Type t)
		{
			return t.GetOwlResourceUri();
		}

        /// <summary>
        /// Gets the base URI needed to convert the relative URI reference in the attached <see cref="OwlResourceAttribute"/> into
        /// an absolute URI.
        /// </summary>
        /// <param name="t">The <see cref="Type"/> to get the URI for.</param>
        /// <returns>a string representation of the base URI</returns>
        public static string GetInstanceBaseUri(Type t)
        {
            return t.GetOwlResourceUri();
        }
	}
}
