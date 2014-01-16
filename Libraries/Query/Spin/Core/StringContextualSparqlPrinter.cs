/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF;
using org.topbraid.spin.model;

namespace VDS.RDF.Query.Spin.SparqlUtil
{
    /**
     * A simple implementation of PrintContext that operates on a StringBuilder.
     * 
     * By default this is using prefixes but not extra prefixes.
     * 
     * @author Holger Knublauch
     */
    public class StringContextualSparqlPrinter : IContextualSparqlPrinter
    {

        //private const PrefixMapping noPrefixMapping = new PrefixMapping();

        private int indentation;

        private Dictionary<String, IResource> initialBindings;

        protected String indentationString = "    ";

        private bool namedBNodeMode;

        private bool nested;

        private Dictionary<INode, String> nodeToLabelMap;

        private bool printPrefixes;

        private SpinWrappedDataset queryModel;

        private StringBuilder sb;

        private bool useExtraPrefixes;

        private bool usePrefixes = true;


        public StringContextualSparqlPrinter(SpinWrappedDataset queryModel)
            : this(queryModel, new StringBuilder())
        {
        }


        public StringContextualSparqlPrinter(SpinWrappedDataset queryModel, StringBuilder sb)
            : this(queryModel, sb, new Dictionary<String, IResource>())
        {
        }

        public StringContextualSparqlPrinter(SpinWrappedDataset queryModel, StringBuilder sb, Dictionary<String, IResource> initialBindings)
        {
            this.queryModel = queryModel;
            this.sb = sb;
            this.initialBindings = initialBindings;
        }


        public SpinWrappedDataset Dataset()
        {
            return queryModel;
        }

        private SparqlContext _sparqlContext;
        public SparqlContext CurrentSparqlContext
        {
            get {
                return _sparqlContext;
            }
            set {
                _sparqlContext = value;
            }
        }

        public IContextualSparqlPrinter clone()
        {
            StringContextualSparqlPrinter cl = new StringContextualSparqlPrinter(queryModel, sb);
            cl.setIndentation(getIndentation());
            cl.setNested(isNested());
            cl.setUseExtraPrefixes(getUseExtraPrefixes());
            cl.setUsePrefixes(getUsePrefixes());
            cl.initialBindings = initialBindings;
            cl.sb = this.sb;
            return cl;
        }


        public int getIndentation()
        {
            return indentation;
        }


        public IResource getInitialBinding(String varName)
        {
            if (initialBindings.ContainsKey(varName))
            {
                return initialBindings[varName];
            }
            return null;
        }


        public Dictionary<INode, String> getNodeToLabelMap()
        {
            if (nodeToLabelMap == null)
            {
                nodeToLabelMap = new Dictionary<INode, String>();
            }
            return nodeToLabelMap;
        }


        public bool getPrintPrefixes()
        {
            return printPrefixes;
        }


        public String getString()
        {
            return sb.ToString();
        }


        public StringBuilder getStringBuilder()
        {
            return sb;
        }


        public bool getUseExtraPrefixes()
        {
            return useExtraPrefixes;
        }


        public bool getUsePrefixes()
        {
            return usePrefixes;
        }


        public bool hasInitialBindings()
        {
            return initialBindings != null && initialBindings.Count != 0;
        }

        public bool isNamedBNodeMode()
        {
            return namedBNodeMode;
        }


        public bool isNested()
        {
            return nested;
        }


        /**
         * @param str Non-null string.
         */
        public void print(String str)
        {
            sb.Append(str.ToString());
        }


        public void printIndentation(int depth)
        {
            for (int i = 0; i < depth; i++)
            {
                print(indentationString);
            }
        }


        public void printKeyword(String str)
        {
            print(str);
        }


        public void println()
        {
            print("\n");
        }


        public void printVariable(String str)
        {
            IResource binding = getInitialBinding(str);
            if (binding == null || binding.isBlank())
            {
                print("?" + str);
            }
            else if (binding.isUri())
            {
                printURIResource(binding);
            }
            else
            {
                String lit = binding.ToString();//FmtUtils.stringForNode(binding, noPrefixMapping);
                print(lit);
            }
        }


        public void printURIResource(IResource resource)
        {
            if (getUsePrefixes())
            {
                String qname = qnameFor(resource);
                if (qname != null)
                {
                    print(qname);
                    return;
                }
                else if (getUseExtraPrefixes())
                {
                    INamespaceMapper extras = new NamespaceMapper(); //ExtraPrefixes.getExtraPrefixes();
                    foreach (String prefix in extras.Prefixes)
                    {
                        String ns = extras.GetNamespaceUri(prefix).ToString();
                        if (resource.Uri().ToString().StartsWith(ns))
                        {
                            print(prefix);
                            print(":");
                            print(resource.Uri().ToString().Substring(ns.Length));
                            return;
                        }
                    }
                }
            }
            print("<");
            print(resource.Uri().ToString());
            print(">");
        }


        /**
         * Work-around for a bug in Jena: Jena would use the default
         * namespace of an imported Graph in a MultiUnion.
         * @param resource  the INode to get the qname for
         * @return the qname or null
         */
        public static String qnameFor(IResource resource)
        {
            // TODO use model namespace mapper;
            return null; // resource.Uri().ToString();
        }


        public void setIndentation(int value)
        {
            this.indentation = value;
        }


        public void setIndentationString(String value)
        {
            this.indentationString = value;
        }


        public void setNamedBNodeMode(bool value)
        {
            this.namedBNodeMode = value;
        }


        public void setNested(bool value)
        {
            this.nested = value;
        }


        public void setPrintPrefixes(bool value)
        {
            this.printPrefixes = value;
        }


        public void setUseExtraPrefixes(bool value)
        {
            this.useExtraPrefixes = value;
        }


        public void setUsePrefixes(bool value)
        {
            this.usePrefixes = value;
        }

    }
}