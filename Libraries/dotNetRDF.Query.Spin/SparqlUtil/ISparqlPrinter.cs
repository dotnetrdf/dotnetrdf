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
using VDS.RDF.Query.Spin.Model;

namespace VDS.RDF.Query.Spin.SparqlUtil
{

    internal enum SparqlContext
    {
        QueryContext = 0,
        DeleteTemplateContext = 1,
        InsertTemplateContext = 2
    }

    #region Printing utility interface
    /// <summary>
    /// A status object responsible for converting SPIN expressions to SPARQL queries 
    /// TODO relocate each IPrintable.print method into a single design-handling ISparqlFactory class
    /// </summary>
    /// <author>Holger Knublauch</author>
    /// <author>Max Bronnimann</author>
    internal interface ISparqlPrinter
    {

        /* *
         * Returns the dataset to operate on
         */
        SpinWrappedDataset Dataset
        {
            get;
        }

        SparqlContext CurrentSparqlContext
        {
            get;
            set;
        }


        /* *
         * Creates a clone of this PrintContext so that it can be used recursively.
         * @return a clone
         */
        ISparqlPrinter clone();


        /* *
         * Gets the indentation level starting at 0.
         * Indentation increases in element groups.
         * @return the indentation level
         * @see #setIndentation(int)
         */
        int getIndentation();


        /* *
         * Gets an initial binding for a variable, so that a constant
         * will be inserted into the query string instead of the variable name.
         * @param varName  the name of the variable to match
         * @return a literal or URI resource, or null
         */
        IResource getInitialBinding(String varName);


        /* *
         * Gets the Jena NodeToLabelMap associated with this.
         * @return the NodeToLabelMap
         */
        //NodeToLabelMap getNodeToLabelMap();


        /* *
         * Checks whether prefix declarations shall be printed into the
         * head of the query.  By default this is switched off, but if
         * turned on then the system should print all used prefixes.
         * @return true  to print prefixes
         */
        bool getPrintPrefixes();


        /* *
         * Checks if the extra prefixes (such as afn:) shall be used to resolve
         * qnames, even if they are not imported by the current model.
         * @return true  if the extra prefixes shall be used
         * @see #setUseExtraPrefixes(boolean)
         */
        bool getUseExtraPrefixes();


        /* *
         * Checks if resource URIs shall be abbreviated with qnames at all.  If
         * not, then the URIs are rendered using the <...> notation.
         * @return true  if this is using prefixes
         */
        bool getUsePrefixes();


        /* *
         * Checks whether any initial bindings have been declared for this context.
         * @return true  if bindings of at least one variable exist
         */
        bool hasInitialBindings();


        /* *
         * Checks if we are inside of a mode (such as INSERT { GRAPH { ... } } or
         * CONSTRUCT { ... } in which blank nodes shall be mapped to named variables
         * such as _:b0.  This can be set temporarily using the corresponding setter
         * but needs to be reset when done by the surrounding block. 
         * @return true  if bnodes shall be rendered as named variables
         */
        bool isNamedBNodeMode();


        /* *
         * Checks if we are inside braces such as a nested expression.
         * @return if the context is currently in nested mode
         */
        bool isNested();


        /* *
         * Prints a given string to the output stream.
         * @param str  the String to print
         */
        void print(String str);


        /* *
         * Prints the indentation string depth times.  For example,
         * for depth=2 this might print "        ". 
         * @param depth  the number of indentations to print
         */
        void printIndentation(int depth);


        /* *
         * Prints a keyword to the output stream.  This can be overloaded
         * by subclasses to do special rendering such as syntax highlighting.
         * @param str  the keyword string
         */
        void printKeyword(String str);


        /* *
         * Prints a line break to the output stream.  Typically this
         * would be a /n but implementations may also do <br />.
         */
        void println();


        /* *
         * Prints a URI to the output stream.  This can be overloaded
         * by subclasses to do special rendering such as syntax highlighting.
         * @param resource  the URI of the resource to print
         */
        void printURIResource(IResource resource);


        /* *
         * Prints a variable to the output stream.  This can be overloaded
         * by subclasses to do special rendering such as syntax highlighting.
         * @param str  the variable string excluding the ?
         */
        void printVariable(String str);


        /* *
         * Changes the indentation level.
         * @param value  the new indentation level
         */
        void setIndentation(int value);


        /* *
         * Activates or deactivates the mode in which bnodes are rendered as named
         * variables, such as _:b0.
         * @param value  true to activate, false to deactivate
         */
        void setNamedBNodeMode(bool value);


        /* *
         * Sets the nested flag.
         * @param value  the new value
         * @see #isNested()
         */
        void setNested(bool value);


        /* *
         * Sets the printPrefixes flag.
         * @param value  the new value
         * @see #getPrintPrefixes()
         */
        void setPrintPrefixes(bool value);


        /* *
         * Specifies whether the context shall use extra prefixes.
         * @param value  the new value
         * @see #getUseExtraPrefixes()
         */
        void setUseExtraPrefixes(bool value);


        /* *
         * Specifies whether the context shall use any prefixes at all.
         * @param value  the new value
         * @see #getUsePrefixes()
         */
        void setUsePrefixes(bool value);

#endregion 

        /// <summary>
        /// Convert a SPIN resource into it's SPARQL representation
        /// </summary>
        /// <param name="spinResource"></param>
        /// <returns></returns>
        SparqlParameterizedString GetCommandText(ICommand spinResource);

        void PrintEnhancedSPARQL(IResource command);

        void PrintEnhancedSPARQL(IDeleteData command);

        void PrintEnhancedSPARQL(IInsertData command);

        void PrintEnhancedSPARQL(IModify command);

        void PrintEnhancedSPARQL(INamedGraph pattern);

        void PrintEnhancedSPARQL(TripleImpl pattern);
    }
}