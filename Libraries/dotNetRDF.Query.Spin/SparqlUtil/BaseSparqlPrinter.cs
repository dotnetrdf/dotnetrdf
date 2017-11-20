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
using System.Resources;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Spin.LibraryOntology;
using VDS.RDF.Query.Spin.Util;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Spin.SparqlUtil
{

    /// <summary>
    /// The default implementation of the ISparqlPrinter interface.
    /// 
    /// Except for IInsertData and IDeleteData commands, updates should redirect (as necessary) their Insert and Delete templates to the corresponding addTriplesTo and deleteTriplesFrom graphs so triples changes can be monitored correctly.
    /// </summary>
    internal class BaseSparqlPrinter : ISparqlPrinter
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

        public BaseSparqlPrinter(SpinWrappedDataset queryModel)
            : this(queryModel, new StringBuilder())
        {
        }


        public BaseSparqlPrinter(SpinWrappedDataset queryModel, StringBuilder sb)
            : this(queryModel, sb, new Dictionary<String, IResource>())
        {
        }

        public BaseSparqlPrinter(SpinWrappedDataset queryModel, StringBuilder sb, Dictionary<String, IResource> initialBindings)
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

        public ISparqlPrinter clone()
        {
            BaseSparqlPrinter cl = new BaseSparqlPrinter(queryModel, sb);
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
                        if (resource.Uri.ToString().StartsWith(ns))
                        {
                            print(prefix);
                            print(":");
                            print(resource.Uri.ToString().Substring(ns.Length));
                            return;
                        }
                    }
                }
            }
            print("<");
            print(resource.Uri.ToString());
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
            return null; // resource.Uri.ToString();
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

        #region Basic SPIN2Sparql implementation

        /* TODO 
         *  The current query evaluation process works with many transformations (ie String => SparqlQuery => SPIN Resources => String)
         *  In time we should remove the SPIN Resource stage to work directly with native dotNetRDF algebras instead
         */

        public SparqlParameterizedString GetCommandText(ICommand spinQuery)
        {
            if (spinQuery == null)
            {
                return new SparqlParameterizedString("");
            }
            spinQuery.PrintEnhancedSPARQL(this);
            String commandText = this.getString();
            StringBuilder sb = new StringBuilder();
            foreach (Uri graphUri in Dataset.DefaultGraphUris)
            {
                sb.AppendLine("USING <" + graphUri.ToString() + ">");
            }
            commandText = commandText.Replace("@USING_DEFAULT", sb.ToString());
            sb.Clear();
            sb.AppendLine("USING NAMED @datasetUri");
            foreach (Uri graphUri in Dataset.ActiveGraphUris)
            {
                if (Dataset.HasGraph(graphUri))
                {
                    if (spinQuery is IDeleteData)
                    {
                        sb.AppendLine("USING NAMED <" + Dataset.Configuration.GetTripleRemovalsMonitorUri(graphUri).ToString() + ">");
                    }
                    else if (spinQuery is InsertDataImpl)
                    {
                        sb.AppendLine("USING NAMED <" + Dataset.Configuration.GetTripleAdditionsMonitorUri(graphUri).ToString() + ">");
                    }
                    Uri ucg = Dataset.Configuration.GetUpdateControlUri(graphUri, false);
                    if (ucg != null)
                    {
                        sb.AppendLine("USING NAMED <" + ucg.ToString() + ">");
                    }
                }
                if (!Dataset.Configuration.IsGraphReplaced(graphUri))
                {
                    sb.AppendLine("USING NAMED <" + graphUri.ToString() + ">");
                }
            }
            commandText = commandText.Replace("@USING_NAMED", sb.ToString());
            //TODO handle namespace prefixes correctly
            if (!commandText.Contains("PREFIX"))
            {
                commandText = @"PREFIX rdf:<http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            PREFIX sd:<http://www.w3.org/ns/sparql-service-description#>
            PREFIX dnr:<dotnetrdf-spin:>
            " + commandText;
            }
            return new SparqlParameterizedString(commandText);
        }

        #endregion

        private Stack<IResource> _currentGraphContext = new Stack<IResource>();

        public void PrintEnhancedSPARQL(IResource resource)
        {
            if (resource == null) return;
            IEnumerable<IResource> elements = resource.AsList();
            foreach (Resource element in elements)
            {
                if (element.canAs(SP.ClassQuery))
                {
                    SPINFactory.asQuery(element).PrintEnhancedSPARQL(this);
                    continue;
                }
                else if (element.canAs(SP.ClassCommand))
                {
                    SPINFactory.asUpdate(element).PrintEnhancedSPARQL(this);
                    continue;
                }
                else if (element.canAs(SP.ClassVariable))
                {
                    SPINFactory.asVariable(element).PrintEnhancedSPARQL(this);
                    continue;
                }
                else if (element.isUri())
                {
                    printURIResource(element);
                    continue;
                }
                IElement asElement = SPINFactory.asElement(element);
                if (asElement != null)
                {
                    asElement.PrintEnhancedSPARQL(this);
                }
            }
        }

        public virtual void PrintEnhancedSPARQL(IQuery spinQuery)
        {
            if (queryModel.QueryExecutionMode != SpinWrappedDataset.QueryMode.UserQuerying
                && spinQuery.canAs(SP.ClassConstruct))
            {
                // TODO convert the CONSTRUCT query into an INSERT command
            }
        }

        /// <summary>
        /// Prints the template required to process DELETE DATA commands.
        /// </summary>
        /// <param name="command">a IDeleteData instance</param>
        public virtual void PrintEnhancedSPARQL(IDeleteData command)
        {
            IResource data = command.getResource(SP.PropertyData);
            String template = SparqlTemplates.DeleteData;
            if (data != null)
            {
                foreach (Resource graph in data.AsList())
                {
                    Uri graphUri = graph.getResource(SP.PropertyGraphNameNode).Uri;
                    Dataset.SetActiveGraph(graphUri);
                    if (Dataset.HasGraph(graphUri))
                    {
                        IGraph ucg = queryModel.GetModifiableGraph(graphUri);
                        foreach (Resource t in graph.getResource(SP.PropertyElements).AsList())
                        {
                            ITriplePattern triple = (ITriplePattern)t.As(typeof(TriplePatternImpl));
                            ucg.Retract(triple.getSubject().getSource(), triple.getPredicate().getSource(), triple.getObject().getSource());
                        }
                    }
                    else
                    {
                        // TODO transform those into a static SPARQL string
                        print("DELETE DATA { GRAPH <g> { <urn:a> <urn:b> <urn:c> . }}; ");
                    }
                }
            }
            print(template);
        }

        /// <summary>
        /// Prints the template required to process INSERT DATA commands.
        /// </summary>
        /// <param name="command">a IInsertData instance</param>
        public virtual void PrintEnhancedSPARQL(IInsertData command)
        {
            IResource data = command.getResource(SP.PropertyData);
            String template = SparqlTemplates.InsertData;
            if (data != null)
            {
                foreach (Resource graph in data.AsList())
                {
                    Uri graphUri = graph.getResource(SP.PropertyGraphNameNode).Uri;
                    Dataset.SetActiveGraph(graphUri);
                    if (Dataset.HasGraph(graphUri))
                    {
                        IGraph ucg = queryModel.GetModifiableGraph(graphUri);
                        foreach (Resource t in graph.getResource(SP.PropertyElements).AsList())
                        {
                            ITriplePattern triple = (ITriplePattern)t.As(typeof(TriplePatternImpl));
                            ucg.Assert(triple.getSubject().getSource(), triple.getPredicate().getSource(), triple.getObject().getSource());
                        }
                    }
                    else
                    {
                        // TODO transform those into a static SPARQL string
                        print("INSERT DATA { GRAPH <g> { <urn:a> <urn:b> <urn:c> . }}; ");
                    }
                }
            }
            print(template);
        }


        /// <summary>
        /// Prints the SPARQL translation for MODIFY commands.
        /// </summary>
        /// <param name="command">a IModify instance</param>
        public virtual void PrintEnhancedSPARQL(IModify command)
        {
            print("INSERT {");
            println(1);

            CurrentSparqlContext = SparqlContext.DeleteTemplateContext;
            PrintEnhancedSPARQL(command.getResource(SP.PropertyDeletePattern));
            CurrentSparqlContext = SparqlContext.InsertTemplateContext;
            PrintEnhancedSPARQL(command.getResource(SP.PropertyInsertPattern));

            print("}");
            println(-1);
            print("@USING_DEFAULT\n");
            print("@USING_NAMED\n");
            print("WHERE {");
            println(1);

            CurrentSparqlContext = SparqlContext.QueryContext;
            PrintEnhancedSPARQL(command.getResource(SP.PropertyWhere));

            print("}");
            println(-1);
        }

        private int anonVarIndex = 0;

        private String GetAnonVariable()
        {
            String varName = "_anonVar" + anonVarIndex.ToString();
            anonVarIndex++;
            return varName;
        }

        private String CreateGraphMapping(IResource nameNode)
        {
            if (_graphMaps.ContainsKey(nameNode))
            {
                return _graphMaps[nameNode];
            }
            String varName = GetAnonVariable();
            _graphMaps.Add(nameNode, varName);
            String filter = "OPTIONAL { GRAPH @datasetUri { ?" + varName + " dnr:updatesGraph @graphNode . @graphNode a sd:Graph . } } . ";
            if (nameNode.isUri())
            {
                filter = filter.Replace("@graphNode", "<" + nameNode.Uri.ToString() + ">");
            }
            else
            {
                filter = filter.Replace("@graphNode", "?" + nameNode.getString(SP.PropertyVarName));
            }
            print(filter);
            println(1);
            return varName;
        }

        private Dictionary<IResource, String> _graphMaps = new Dictionary<IResource, String>();

        /// <summary>
        /// Handles translating NamedGraph patterns into 
        /// </summary>
        /// <param name="pattern"></param>
        public virtual void PrintEnhancedSPARQL(INamedGraph pattern)
        {
            // creates a mapping for the graph node
            IResource nameNode = pattern.getNameNode();
            _currentGraphContext.Push(nameNode);
            if (nameNode.isUri())
            {
                Dataset.SetActiveGraph(nameNode.Uri);
            }
            else {
                // TODO find a better way to set those
                Dataset.SetActiveGraph(Dataset.DefaultGraphUris);
            }
            if (CurrentSparqlContext == SparqlContext.QueryContext)
            {
                if (nameNode.isUri())
                {
                    if (!Dataset.HasGraph(nameNode.Uri))
                    {
                        _graphMaps.Add(nameNode, String.Empty);
                    }
                    else
                    {
                        CreateGraphMapping(nameNode);
                    }
                }
                else
                {
                    CreateGraphMapping(nameNode);
                }
            }
            else
            {
                print("GRAPH ");
                if (nameNode.isUri())
                {
                    if (CurrentSparqlContext == SparqlContext.InsertTemplateContext)
                    {
                        print("<" + Dataset.Configuration.GetTripleAdditionsMonitorUri(nameNode.Uri).ToString() + ">");
                    }
                    else
                    {
                        print("<" + Dataset.Configuration.GetTripleRemovalsMonitorUri(nameNode.Uri).ToString() + ">");
                    }
                }
                else
                {
                    // TODO add the pattern to the additiontriple of the variable
                    printVariable(nameNode.getString(SP.PropertyVarName));
                }
                print("{");
                println(1);
            }
            PrintEnhancedSPARQL(pattern.getResource(SP.PropertyElements));
            if (CurrentSparqlContext != SparqlContext.QueryContext)
            {
                println(-1);
                print("}");
            }
            _currentGraphContext.Pop();
        }


        public virtual void PrintEnhancedSPARQL(TripleImpl pattern)
        {
            if (CurrentSparqlContext != SparqlContext.QueryContext)
            {
                pattern.Print(this);
                return;
            }
            IResource graphContext = null;
            if (_currentGraphContext.Count > 0)
            {
                graphContext = _currentGraphContext.Peek();
            }
            else
            {
                graphContext = SPINFactory.createVariable(Dataset.spinProcessor, GetAnonVariable());
            }
            String surrogate = null;
            if (CurrentSparqlContext == SparqlContext.QueryContext)
            {
                surrogate = CreateGraphMapping(graphContext);
                if (!String.IsNullOrEmpty(surrogate))
                {
                    print("{ ");
                }
            }

            print(" GRAPH ");
            if (graphContext.isUri())
            {
                printURIResource(graphContext);
            }
            else
            {
                printVariable(graphContext.getString(SP.PropertyVarName));
            }
            print(" {");
            println(1);
            pattern.Print(this);
            if (!String.IsNullOrEmpty(surrogate))
            {
                print(". FILTER (NOT EXISTS{ GRAPH ?" + surrogate + " { ");
                pattern.print(pattern.getSubject(), this);
                print(" dnr:resets ");
                pattern.print(pattern.getPredicate(), this, false);
                print("} })");
            }
            println(-1);
            print("} ");
            if (!String.IsNullOrEmpty(surrogate))
            {
                print("} UNION { GRAPH ");
                printVariable(surrogate);
                print(" {");
                println(1);
                pattern.Print(this);
                println(-1);
                print("} }");
            }
            print(" .");
            println();
        }
    }
}