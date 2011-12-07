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
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace VDS.RDF.Linq
{
    /// <summary>
    /// Attribute to specify at the assembly level what ontologies are in use, and where to find them.
    /// </summary>
    /// <example>
    /// <code>
    /// [assembly: Ontology(
    ///     Name="MusicOntology",
    ///     UrlOfOntology="file:///c:/etc/dev/ontologies/2007/07/music.n3",
    ///     Prefix="music",
    ///     Uri="http://aabs.tempuri.com/ontologies/2007/07/music#",
    ///     GraphName="AABS_MUSIC_07_07",
    /// )]
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public sealed class OntologyAttribute : Attribute
    {
        /// <summary>
        /// internal name for the ontology, to be used elesewhere in this system.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Where to find the ontology file.
        /// </summary>
        /// <remarks>
        /// Note that this is not necessarily the URL used internally in the ontology, although it may be.
        /// </remarks>
        public string UrlOfOntology { get; set; }

        /// <summary>
        /// this is the _preferred_ prefix to be used for the ontology URI
        /// </summary>
        /// <remarks>
        /// Note that while the prefix will be used whereever possible, there may be prefix clashes
        /// that will require ontology substitution
        /// </remarks>
        public string Prefix { get; set; }

        /// <summary>
        /// The base internal URI used in the ontology.
        /// </summary>
        /// <remarks>
        /// This is not the same as the URL needed to get the ontology file.
        /// </remarks>
        public string BaseUri { get; set; }

        /// <summary>
        /// The Named Graph used in ontology triple store for that ontology. Equivalent to ontology .NET namespace.
        /// </summary>
        public string GraphName { get; set; }
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class OwlResourceAttribute : Attribute
    {
        /// <summary>
        /// internal name of the OntologyAttribute that defines the ontology this class is bound to
        /// </summary>
        /// <remarks>
        /// Must match the Name property of some assembly level instance of OntologyAttribute
        /// </remarks>
        public string OntologyName { get; set; }

        /// <summary>
        /// The _relative_ URI of the OWL resource this object corresponds to
        /// </summary>
        public string RelativeUriReference { get; set; }

        /// <summary>
        /// The full Datatype URI used when persisting the object
        /// </summary>
        public string DatatypeUri { get; set; }

        /// <summary>
        /// The relative URI of the XML Schema Datatype e.g. integer
        /// </summary>
        /// <remarks>
        /// Ignored if DatatypeUri property is specified prior to this
        /// </remarks>
        public string XmlSchemaDatatype
        {
            get
            {
                if (!String.IsNullOrEmpty(this.DatatypeUri))
                {
                    if (this.DatatypeUri.StartsWith(NamespaceMapper.XMLSCHEMA))
                    {
                        return this.DatatypeUri.Substring(NamespaceMapper.XMLSCHEMA.Length);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this.DatatypeUri == null)
                {
                    this.DatatypeUri = NamespaceMapper.XMLSCHEMA + value;
                }
            }
        }
    }

    /// <summary>
    /// Defines metadata for XSDT datatypes in the <see cref="XsdtPrimitiveDataType"/> 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class XsdtAttribute : Attribute
    {
        /// <summary>
        /// supply information on whether the enum (to which this attribute is attached) is represented using a quoted string and the 
        /// URI for the XSDT specification for the datatype.
        /// </summary>
        /// <param name="isQuoted">if set to <c>true</c> this type is represented using a quoted string. eg Dates and strings but not ints.</param>
        /// <param name="uri">The name.</param>
        public XsdtAttribute(bool isQuoted, string uri)
        {
            IsQuoted = isQuoted;
            TypeUri = uri;
        }

        /// <summary>
        /// Gets the unqualified URI of the type as a string.
        /// </summary>
        /// <value>The name.</value>
        public string TypeUri { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is quoted in strings.
        /// </summary>
        /// <value><c>true</c> if this instance is quoted; otherwise, <c>false</c>.</value>
        public bool IsQuoted { get; private set; }
    }

    public static class AttributeExtensions
    {
        public static bool IsOwlClass(this Type t)
        {
            var attributes = t.GetCustomAttributes(typeof (OwlResourceAttribute), true);
            return (attributes != null && attributes.Length > 0);
        }

        public static bool IsOntologyResource(this MemberInfo pi)
        {
            return ((pi is PropertyInfo)&&(pi.GetCustomAttributes(typeof(OwlResourceAttribute), true).Count() > 0));
        }

        public static T[] GetAttributes<T>(this Assembly assembly)
        {
            return assembly.GetCustomAttributes(typeof(T), true) as T[];
        }

        public static OntologyAttribute[] GetAllOntologies(this Assembly assembly)
        {
            return assembly.GetAttributes<OntologyAttribute>();
        }

        public static OntologyAttribute[] GetAllOntologies(this AppDomain ad)
        {
            List<OntologyAttribute> tmp = new List<OntologyAttribute>();
            foreach (var asm in ad.GetAssemblies())
            {
                tmp.AddRange(asm.GetAllOntologies());
            }
            return tmp.ToArray();
        }

        public static OntologyAttribute GetOntology(this Assembly assembly, string ontologyName)
        {
            return (from a in assembly.GetAllOntologies() where a.Name == ontologyName select a).FirstOrDefault();
        }

        public static OntologyAttribute GetOntology(this Type t)
        {
            return t.Assembly.GetOntology(t.GetOwlResource().OntologyName);
        }

        public static OntologyAttribute GetOntology(this MemberInfo mi)
        {
            return mi.DeclaringType.Assembly.GetOntology(mi.GetOwlResource().OntologyName);
        }

        public static OwlResourceAttribute GetOwlResource(this Type t)
        {
            return t.GetCustomAttributes(typeof(OwlResourceAttribute), true).FirstOrDefault() as OwlResourceAttribute;
        }

        public static OwlResourceAttribute GetOwlResource(this MemberInfo mi)
        {
            return mi.GetCustomAttributes(typeof(OwlResourceAttribute), true).FirstOrDefault() as OwlResourceAttribute;
        }

        public static string GetOwlResourceUri(this Type t)
        {
            return t.GetOntology().BaseUri + t.GetOwlResource().RelativeUriReference;
        }

        public static string GetOwlResourceUri(this MemberInfo mi)
        {
            return mi.GetOntology().BaseUri + mi.GetOwlResource().RelativeUriReference;
        }

        /// <summary>
        /// Gets all ontologoies declared for all app domains and finds the one 
        /// with the matching name to Name and returns the current prefix for it.
        /// </summary>
        /// <param name="Name">Ontology Name</param>
        /// <returns></returns>
        public static string GetOntologyPrefix(string Name)
        {
            return (from o in AppDomain.CurrentDomain.GetAllOntologies() 
                    where o.Name == Name 
                    select o).First().Prefix;
        }

        public static string PredicateUriForProperty(this Object o, MethodBase propAccessor)
        {
            string propertyName = propAccessor.Name.StartsWith("get_") ? propAccessor.Name.Substring(4) : null;
            PropertyInfo propInfo = o.GetType().GetProperty(propertyName);
            return propInfo.GetOwlResourceUri();
        }

        public static String GetDatatypeUri(this MemberInfo mi)
        {
            return mi.GetOwlResource().DatatypeUri;
        }
    }
}
