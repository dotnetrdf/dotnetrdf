using org.topbraid.spin.util;
using System;
using System.Text;
using VDS.RDF.Query.Spin;
using System.Collections.Generic;
using org.topbraid.spin.model;
using VDS.RDF;
using VDS.RDF.Query;

namespace org.topbraid.spin.system
{


    /**
     * A singleton that is used by SPINARQFunction to check whether all supplied arguments
     * match the definition of the declared spl:Arguments.
     * 
     * By default the singleton is null (indicating a no-op), but implementors can install a
     * subclass of this to report warnings, throw exceptions or whatever they like.
     * 
     * Note that activating this will have a severe performance impact.
     * 
     * @author Holger Knublauch
     */
    public abstract class SPINArgumentChecker
    {

        private static SPINArgumentChecker singleton;

        public static SPINArgumentChecker get()
        {
            return singleton;
        }

        public static void set(SPINArgumentChecker value)
        {
            singleton = value;
        }


        public void check(IModule module, SparqlResultSet bindings)
        {
            List<String> errors = new List<String>();
            foreach (IArgument arg in module.getArguments(false))
            {
                String varName = arg.getVarName();
                INode value = bindings.Values(varName);
                if (!arg.isOptional() && value == null)
                {
                    errors.Add("Missing required argument " + varName);
                }
                else if (value != null)
                {
                    Resource valueType = arg.getValueType();
                    if (valueType != null)
                    {
                        if (value.isResource())
                        {
                            if (!UriComparer.Equals(RDFS.Resource, valueType) && !JenaUtil.hasIndirectType((Resource)value, valueType.inModel(value.getModel())))
                            {
                                StringBuilder sb = new StringBuilder("Resource ");
                                sb.Append(SPINLabels.getLabel((Resource)value));
                                sb.Append(" for argument ");
                                sb.Append(varName);
                                sb.Append(" must have type ");
                                sb.Append(SPINLabels.getLabel(valueType));
                                errors.Add(sb.ToString());
                            }
                        }
                        else if (!UriComparer.Equals(valueType, RDFS.Literal))
                        {
                            Uri datatypeURI = value.asLiteral().getDatatypeURI();
                            if (datatypeURI == null)
                            {
                                datatypeURI = XSD.string_;
                            }
                            if (!UriComparer.Equals(valueType, datatypeURI))
                            {
                                StringBuilder sb = new StringBuilder("Literal ");
                                sb.Append(value.asLiteral().getLexicalForm());
                                sb.Append(" for argument ");
                                sb.Append(varName);
                                sb.Append(" must have datatype ");
                                sb.Append(SPINLabels.getLabel(valueType));
                                errors.Add(sb.ToString());
                            }
                        }
                    }
                }
            }
            if (errors.Count > 0)
            {
                handleErrors(module, bindings, errors);
            }
        }


        protected abstract void handleErrors(IModule module, SparqlResultSet bindings, List<String> errors);
    }
}