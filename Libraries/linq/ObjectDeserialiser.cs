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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Linq
{
    public class ObjectDeserialiserQuerySink : BaseLinqResultsSink
    {
        /// <summary>
        /// the type of the object that must be returned (maybe the original type or a projection)
        /// </summary>
        private readonly Type instanceType;
        /// <summary>
        /// A logger based on log4net
        /// </summary>
        private Logger Logger = new Logger(typeof(ObjectDeserialiserQuerySink));
        /// <summary>
        /// the original type that the query was made against (i.e. the type that the datacontext defined for its standard queries)
        /// </summary>
        private Type originalType;

        private static UncompressedNotation3Formatter _formatter = new UncompressedNotation3Formatter();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDeserialiserQuerySink"/> class.
        /// </summary>
        /// <param name="originalType">the original type that the query was made against.</param>
        /// <param name="instanceType">the type of the object that must be returned.</param>
        /// <param name="instanceName">Name of the instance (i.e the alias used in both the LINQ and SPARQL queries).</param>
        /// <param name="distinct">if set to <c>true</c> discard duplicate answers.</param>
        /// <param name="selectExpression">The select expression (derived from the the LINQ query). Used to help in deserialisation.</param>
        /// <param name="context">The data context that will monitor the objects created (not yet used).</param>
        public ObjectDeserialiserQuerySink(
            Type originalType,
            Type instanceType,
            string instanceName,
            bool distinct,
            MethodCallExpression selectExpression,
            RdfDataContext context)
        {
            #region Tracing

#line hidden
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Deserialising {0}.", instanceType.Name);
            }
#line default

            #endregion

            SelectExpression = selectExpression;
            this.originalType = originalType;
            this.instanceType = instanceType;
            InstanceName = instanceName;
            Distinct = distinct;
            DataContext = context;
            IncomingResults = new ArrayList();
        }

        /// <summary>
        /// Stores the results as they get deserialised.
        /// </summary>
        public IList IncomingResults { get; private set; }

        /// <summary>
        /// if set to <c>true</c> discard duplicate answers, otherwise keep everything that comes in.
        /// </summary>
        public bool Distinct { get; private set; }

        /// <summary>
        /// The select expression (derived from the the LINQ query). Used to help in deserialisation.
        /// </summary>
        private MethodCallExpression SelectExpression { get; set; }

        /// <summary>
        /// the original type that the query was made against.
        /// </summary>
        public Type OriginalType
        {
            get { return originalType; }
            set { originalType = value; }
        }

        /// <summary>
        /// Name of the instance (i.e the alias used in both the LINQ and SPARQL queries).
        /// </summary>
        private string InstanceName { get; set; }

        /// <summary>
        /// The data context that will monitor the objects created (not yet used).
        /// </summary>
        private RdfDataContext DataContext { get; set; }

        /// <summary>
        /// A callback interface that gets called by SemWeb when results are sent 
        /// back from a remote SPARQL source. This gets each unique set of bindings 
        /// in turn. It can either store the results or deserialise them on the spot.
        /// </summary>
        /// <returns>true if the deserialiser was able to use the result or false otherwise</returns>
        protected override void ProcessResult(SparqlResult result)
        {
            #region Tracing

#line hidden
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Got Result {0}.", result.ToString());
            }
#line default

            #endregion

            if (IsSelectMember(SelectExpression))
            {
                IncomingResults.Add(ExtractMemberAccess(result));
                return;
            }

            if (originalType == null) throw new LinqToRdfException("need ontology type to create");
            object t;

            IEnumerable<MemberInfo> props = GetPropertiesToPopulate(originalType, instanceType);

            if (originalType == instanceType)
            {
                #region not using a projection

                t = Activator.CreateInstance(instanceType);

                #region Tracing

#line hidden
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("created new instance of {0}.", t.GetType().Name);
                }
#line default

                #endregion

                AssignDataContext(t as OwlInstanceSupertype, DataContext);
                AssignInstanceUri(t as OwlInstanceSupertype, InstanceName, result);

                foreach (PropertyInfo pi in props)
                {
                    if (pi.PropertyType.IsGenericType &&
                        pi.PropertyType.GetGenericTypeDefinition().Name.StartsWith("Entity"))
                        continue;
                    try
                    {
                        PopulateProperty(result, t, pi);
                    }
                    catch (ArgumentException ae)
                    {
                        #region Tracing

#line hidden
                        if (Logger.IsErrorEnabled)
                        {
                            Logger.ErrorEx("Unable to populate property " + pi.Name, ae);
                            Logger.Error("continuing");
                        }
#line default

                        #endregion
                    }
                    catch (Exception e)
                    {
                        #region Tracing

#line hidden
                        if (Logger.IsErrorEnabled)
                        {
                            Logger.ErrorEx("Unable to populate property " + pi.Name, e);
                        }
#line default

                        #endregion

                        return;
                    }
                }

                #endregion
            }
            else
            {
                #region using a projection

                var args = new List<object>();
                foreach (PropertyInfo pi in props)
                {
                    try
                    {
                        if (result.HasValue(pi.Name))
                        {
                            if (result[pi.Name] != null)
                            {
                               
                                string vVal = result[pi.Name].ToString();
                                vVal = RemoveEnclosingQuotesOnString(vVal, pi);
                                if (IsXsdtEncoded(vVal))
                                    vVal = DecodeXsdtString(vVal);
                                args.Add(Convert.ChangeType(vVal, pi.PropertyType));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return;
                    }
                }
                t = Activator.CreateInstance(instanceType, args.ToArray());

                #endregion
            }
            if (Distinct)
            {
                if (ObjectIsUniqueSoFar(t as OwlInstanceSupertype))
                    IncomingResults.Add(t);
            }
            else
                IncomingResults.Add(t);

            return;
        }

        /// <summary>
        /// stores the result from a SemWeb result into a <see cref="PropertyInfo"/> doing whatever conversions are required.
        /// </summary>
        /// <param name="semwebBindings">The incoming result from semweb.</param>
        /// <param name="obj">The object instance on wqhich the property is to be set</param>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> identifying the property to be populated with the value stored in <see cref="semwebBindings"/>.</param>
        public static void PopulateProperty(SparqlResult semwebBindings, object obj, PropertyInfo propertyInfo)
        {
            if (semwebBindings.HasValue(propertyInfo.Name))
            {
                if (semwebBindings[propertyInfo.Name] == null) return;
                var tc = new XsdtTypeConverter();
                object x = tc.Parse(semwebBindings[propertyInfo.Name]);

                if (x is IConvertible)
                    propertyInfo.SetValue(obj, Convert.ChangeType(x, propertyInfo.PropertyType), null);
                else 
                    // if it's not convertible, it could be because the type is an MS XSDT type rather than a .NET primitive 
                    if (x.GetType().Namespace == "System.Runtime.Remoting.Metadata.W3cXsd2001")
                    {
                        switch (x.GetType().Name)
                        {
                            case "SoapDate":
                                var d = (SoapDate) x;
                                propertyInfo.SetValue(obj, Convert.ChangeType(d.Value, propertyInfo.PropertyType), null);
                                break;
                            default:
                                break;
                        }
                    }
                    else if (propertyInfo.PropertyType == typeof (string))
                    {
                        propertyInfo.SetValue(obj, x.ToString(), null);
                    }
            }
        }

        /// <summary>
        /// Get those properties that were adorned by the <see cref="OwlResourceAttribute"/>. 
        /// If there are none, then just get every property (and let later processors take their chances).
        /// </summary>
        /// <param name="originalType">the original type that the query was made against.</param>
        /// <param name="instanceType">the type of the object that must be returned.</param>
        /// <returns></returns>
        private IEnumerable<MemberInfo> GetPropertiesToPopulate(Type originalType, Type instanceType)
        {
            IEnumerable<MemberInfo> props;
            if (originalType == instanceType)
                //  i.e. identity projection, meaning we can use GetAllPersistentProperties safely
            {
                props = OwlClassSupertype.GetAllPersistentProperties(OriginalType);
            }
            else
            {
                props = instanceType.GetProperties();
            }
            return props;
        }

        /// <summary>
        /// Assigns the instance URI from the SPARQL result bindings.
        /// </summary>
        /// <param name="obj">The object to assign the URI to.</param>
        /// <param name="queryAlias">The query alias that was used in LINQ and SPARQL.</param>
        /// <param name="semwebResult">The semweb result to take the value from.</param>
        private void AssignInstanceUri(OwlInstanceSupertype obj, string queryAlias,
                                                        SparqlResult result)
        {
            // if there is no alias, then there's no way to work out what contains the instance URI
            if (string.IsNullOrEmpty(queryAlias))
            {
                return;
            }

            // if there is a binding with the same name as the alias
            if (result.Variables.Contains(queryAlias))
            {
                // get string representation of the instance URI
                string uri = result[queryAlias].ToString();

                // is it enclosed in angle brackets? then strip them.
                if (uri.StartsWith("<") && uri.EndsWith(">"))
                {
                    uri = uri.Substring(1, uri.Length - 2);
                }

                // can this be parsed as a URI? if so then assign to instance URI property of obj
                if (Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute))
                {
                    obj.InstanceUri = uri;
                }
            }
        }

        /// <summary>
        /// Assigns the <see cref="RdfDataContext"/> to the instance.
        /// </summary>
        /// <remarks>
        /// this is used in case it needs it to lazily load references later on.
        /// </remarks>
        /// <param name="obj">the object that has just been deserialised.</param>
        /// <param name="dataContext">The <see cref="RdfDataContext"/> through which the query was run that led to the instance being deserialised.</param>
        private void AssignDataContext(OwlInstanceSupertype obj, RdfDataContext dataContext)
        {
            if (obj != null)
            {
                obj.DataContext = DataContext;
            }
            // TODO: assign event handlers for NotifyPropertyChanged
        }

        /// <summary>
        /// Returns true if the result is unique among the results so far received.
        /// </summary>
        /// <param name="obj">The object that is potentially to be added to the results collection.</param>
        /// <returns>true if there is no object among the <see cref="IncomingResults"/> with the same <see cref="OwlInstanceSupertype.InstanceUri"/>.</returns>
        private bool ObjectIsUniqueSoFar(OwlInstanceSupertype obj)
        {
            if (obj == null)
            {
                return true;
            }
            return IncomingResults
                       .Cast<OwlInstanceSupertype>()
                       .Where(o => o.InstanceUri == obj.InstanceUri)
                       .Count() == 0;
        }

        /// <summary>
        /// A simple mechanism to extract the value from a string formatted using XML Schema Datatype definitions (in Turtle format?).
        /// </summary>
        /// <param name="val">The value to be decoded.</param>
        /// <returns>a string representation of the value that was encoded</returns>
        /// <remarks>This is a completely half-arsed solution.
        /// TODO: devise a solution using the std .NET decoding system (or the <see cref="XsdtTypeConverter"/>)
        /// </remarks>
        private string DecodeXsdtString(string val)
        {
            var delims = new[] {"^^"};
            string[] sa = val.Split(delims, StringSplitOptions.None);
            string sValue = sa[0];
            string xsdtType = sa[1];
            if (xsdtType.EndsWith("integer>"))
                return sValue.Substring(1, sValue.Length - 2);
            return sValue;
        }

        /// <summary>
        /// Determines whether the specified val is XSDT encoded.
        /// </summary>
        /// <param name="val">The value to be decoded.</param>
        /// <returns>
        /// 	<c>true</c> if the specified val is XSDT encoded; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>This is a completely half-arsed solution.
        /// TODO: devise a solution using the std .NET decoding system (or the <see cref="XsdtTypeConverter"/>)
        /// </remarks>
        private bool IsXsdtEncoded(string val)
        {
            return val.Contains("^^");
        }

        /// <summary>
        /// Strip quotes from the value if it is a string or a timespan.
        /// </summary>
        /// <param name="val">The value to be decoded.</param>
        /// <param name="pi">The <see cref="PropertyInfo"/> of the property that will get the value.</param>
        /// <returns></returns>
        private string RemoveEnclosingQuotesOnString(string val, PropertyInfo pi)
        {
            if (pi.PropertyType == typeof (string) || pi.PropertyType == typeof (TimeSpan))
            {
                if (val.StartsWith("\"") && val.EndsWith("\"") && val.Length > 1)
                {
                    return val.Substring(1, val.Length - 2);
                }
            }
            return val;
        }

        /// <summary>
        /// Determines whether <see cref="e"/> is a member that has been selected.
        /// </summary>
        /// <param name="e">The expression for the <c>select</c> statement.</param>
        /// <returns>
        /// 	<c>true</c> if e contains a <see cref="MemberExpression"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <example>
        /// i.e. a select member will come through for queries like this:
        /// <code>
        /// var q = from a in ctx.Albums select a.Name;
        /// </code>
        /// i.e. a property of the <see cref="OriginalType"/> was chosen for the results.
        /// </example>
        private bool IsSelectMember(MethodCallExpression e)
        {
            if (e == null)
            {
                return false;
            }
            var ue = e.Arguments[1] as UnaryExpression;
            var le = (LambdaExpression) ue.Operand;
            return le.Body is MemberExpression;
        }

        /// <summary>
        /// Extracts the corresponding value from the SemWeb results for a member access projection.
        /// </summary>
        /// <param name="vb">The SemWeb results.</param>
        /// <returns>the value that was extracted (and converted) from the results.</returns>
        private object ExtractMemberAccess(SparqlResult vb)
        {
            // work out if the SelectExpression really is a member access
            var ue = (SelectExpression).Arguments[1] as UnaryExpression;
            if (ue == null)
                throw new ArgumentException("incompatible expression type");
    
            var le = ue.Operand as LambdaExpression;
            if (le == null)
                throw new LinqToRdfException("Incompatible expression type found when building ontology projection");

            if (le.Body is MemberExpression)
            {
                // work out which member is being queried on
                var memberExpression = (MemberExpression) le.Body;
                MemberInfo memberInfo = memberExpression.Member;
                // get its name and use that as a key into the results
                //string vVal = vb[memberInfo.Name].ToString();
                // convert the result from XSDT format to .NET types
                if (!vb.HasValue(memberInfo.Name)) return null;
                var tc = new XsdtTypeConverter();
                return tc.Parse(vb[memberInfo.Name]);
                //return tc.Parse(vVal);
            }
            return null;
        }
    }
}