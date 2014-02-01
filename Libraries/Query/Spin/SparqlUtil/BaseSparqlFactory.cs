/*******************************************************************************
 * Copyright (c) 2009 TopQuadrant, Inc.
 * All rights reserved. 
 *******************************************************************************/
using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Util;

namespace VDS.RDF.Query.Spin.SparqlUtil
{

    // TODO make this class abstract and accessible only through a specific SPINFactory implementation

    /// <summary>
    /// The default implementation of the ISparqlFactory interface.
    /// 
    /// Except for IInsertData and IDeleteData commands, this implementation rewrites SPARQL update command to insert QuadEvents into a temporary graph for further SPIN monitoring and processing.
    /// </summary>
    public class BaseSparqlFactory : ISparqlFactory
    {

        //private const PrefixMapping noPrefixMapping = new PrefixMapping();

        private int indentation;

        private Dictionary<String, IResource> initialBindings;

        protected String indentationString = "    ";

        private bool namedBNodeMode;

        private bool nested;

        private Dictionary<INode, String> nodeToLabelMap;

        private bool printPrefixes;

        private SparqlContext _sparqlContext;

        private SpinWrappedDataset queryModel;

        private StringBuilder sb;

        private bool useExtraPrefixes;

        private bool usePrefixes = true;


        #region Initialisation

        public BaseSparqlFactory(SpinWrappedDataset queryModel)
            : this(queryModel, new StringBuilder())
        {
        }


        public BaseSparqlFactory(SpinWrappedDataset queryModel, StringBuilder sb)
            : this(queryModel, sb, new Dictionary<String, IResource>())
        {
        }

        public BaseSparqlFactory(SpinWrappedDataset queryModel, StringBuilder sb, Dictionary<String, IResource> initialBindings)
        {
            this.queryModel = queryModel;
            this.sb = sb;
            this.initialBindings = initialBindings;
        }

        #endregion

        #region Utilities

        public SpinWrappedDataset Dataset
        {
            get { return queryModel; }
            private set { }
        }

        public SparqlContext CurrentSparqlContext
        {
            get
            {
                return _sparqlContext;
            }
            set
            {
                _sparqlContext = value;
            }
        }

        public ISparqlFactory clone()
        {
            BaseSparqlFactory cl = new BaseSparqlFactory(queryModel, sb);
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


        public void println() {
            println(0);
        }

        protected void println(int indent)
        {
            print("\n");
            indentation += indent;
            printIndentation(indentation);
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

        #endregion

        #region Specific SPIN2Sparql implementation

        public SparqlParameterizedString Print(IResource spinResource)
        {
            if (spinResource == null) {
                return new SparqlParameterizedString("");
            }
            if (spinResource.canAs(SP.ClassQuery))
            {
                return Print(SPINFactory.asQuery(spinResource));
            }
            else if (spinResource is IDeleteData || spinResource.canAs(SP.ClassDeleteData))
            {
                return Print((IDeleteData)spinResource.As(typeof(DeleteDataImpl)));
            }
            else if (spinResource is IInsertData || spinResource.canAs(SP.ClassInsertData))
            {
                return Print((IInsertData)spinResource.As(typeof(InsertDataImpl)));
            }
            else if (spinResource.canAs(SP.ClassModify))
            {
                return Print((IModify)spinResource.As(typeof(ModifyImpl)));
            }
            else
            {
                return new SparqlParameterizedString("");
            }
        }

        public virtual SparqlParameterizedString Print(IQuery spinQuery)
        {
            if (queryModel.QueryExecutionMode != SpinWrappedDataset.QueryMode.UserQuerying
                && spinQuery.canAs(SP.ClassConstruct))
            {
                // TODO convert the CONSTRUCT query into an INSERT command
            }
            return new SparqlParameterizedString("");
        }

        /// <summary>
        /// Returns the SparqlParameterizedString template required to process DELETE DATA commands.
        /// </summary>
        /// <param name="spinUpdate">a IDeleteData instance</param>
        /// <returns>a SparqlParameterizedString translation of the command</returns>
        public virtual SparqlParameterizedString Print(IDeleteData spinUpdate)
        {
            IResource data = spinUpdate.getResource(SP.PropertyData);
            if (data != null)
            {
                foreach (Resource graph in data.AsList())
                {
                    Uri graphUri = graph.getResource(SP.PropertyGraphNameNode).Uri();
                    IGraph ucg = queryModel.GetModifiableGraph(graphUri);
                    foreach (Resource t in graph.getResource(SP.PropertyElements).AsList())
                    {
                        ITriplePattern triple = (ITriplePattern)t.As(typeof(TriplePatternImpl));
                        ucg.Retract(triple.getSubject().getSource(), triple.getPredicate().getSource(), triple.getObject().getSource());
                    }
                }
            }
            SparqlParameterizedString template = new SparqlParameterizedString(SparqlTemplates.DeleteData);
            return template;
        }

        /// <summary>
        /// Returns the SparqlParameterizedString template required to process INSERT DATA commands.
        /// </summary>
        /// <param name="spinUpdate">a IInsertData instance</param>
        /// <returns>a SparqlParameterizedString translation of the command</returns>
        public virtual SparqlParameterizedString Print(IInsertData spinUpdate)
        {
            IResource data = spinUpdate.getResource(SP.PropertyData);
            if (data != null)
            {
                foreach (Resource graph in data.AsList())
                {
                    Uri graphUri = graph.getResource(SP.PropertyGraphNameNode).Uri();
                    IGraph ucg = queryModel.GetModifiableGraph(graphUri);
                    foreach (Resource t in graph.getResource(SP.PropertyElements).AsList())
                    {
                        ITriplePattern triple = (ITriplePattern)t.As(typeof(TriplePatternImpl));
                        ucg.Assert(triple.getSubject().getSource(), triple.getPredicate().getSource(), triple.getObject().getSource());
                    }
                }
            }
            SparqlParameterizedString template = new SparqlParameterizedString(SparqlTemplates.InsertData);
            // TODO get the activeGraphs list
            return template;
        }

        /// <summary>
        /// Returns the SparqlParameterizedString template required to process INSERT DATA commands.
        /// </summary>
        /// <param name="spinUpdate">a IModify instance</param>
        /// <returns>a SparqlParameterizedString translation of the command</returns>
        public virtual SparqlParameterizedString Print(IModify spinUpdate)
        {
            print("INSERT {");
            println(1);
            print("GRAPH @outputGraph {");
            println(1);
            PrintUpdatePattern(spinUpdate.getResource(SP.PropertyDeletePattern));
            PrintUpdatePattern(spinUpdate.getResource(SP.PropertyInsertPattern));
            print("}");
            println(-1);
            print("}");
            println(-1);
            print("WHERE {");
            println(1);
            PrintWherePattern(spinUpdate.getResource(SP.PropertyWhere));
            print("}");
            println(-1);

            SparqlParameterizedString template = new SparqlParameterizedString(getString());
            return template;
        }


        protected virtual void PrintUpdatePattern(IResource spinUpdatePattern)
        {
            if (spinUpdatePattern == null) return;
        }

        protected virtual void PrintWherePattern(IResource spinUpdatePattern)
        {
            if (spinUpdatePattern == null) return;
        }

        #endregion

    }
}