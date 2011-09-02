/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.IO;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    /// <summary>
    /// Interface to be implemented by RDF Writers which generate RDF Concrete Syntax
    /// </summary>
    public interface IRdfWriter
    {
        /// <summary>
        /// Method for Saving a Graph to a Concrete RDF Syntax in a file based format
        /// </summary>
        /// <param name="g">The Graph to Save</param>
        /// <param name="filename">The filename to save the Graph in</param>
        /// <exception cref="RdfException">Thrown if the RDF in the Graph is not representable by the Writer</exception>
        /// <exception cref="IOException">Thrown if the Writer is unable to write to the File</exception>
        void Save(IGraph g, String filename);

        /// <summary>
        /// Method for Saving a Graph to a Concrete RDF Syntax via some arbitrary <see cref="TextWriter">TextWriter</see>
        /// </summary>
        /// <param name="g">The Graph to Save</param>
        /// <param name="output">The <see cref="TextWriter">TextWriter</see> to save the Graph to</param>
        /// <exception cref="RdfException">Thrown if the RDF in the Graph is not representable by the Writer</exception>
        /// <exception cref="IOException">Thrown if the Writer is unable to write to the underlying storage of the <see cref="TextWriter">TextWriter</see> specified in the <paramref name="output"/></exception>
        void Save(IGraph g, TextWriter output);

        /// <summary>
        /// Event which writers can raise to indicate possible ambiguities or issues in the syntax they are producing
        /// </summary>
        event RdfWriterWarning Warning;
    }
}

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Interface for Writers that Support Pretty Printing
    /// </summary>
    public interface IPrettyPrintingWriter
    {
        /// <summary>
        /// Gets/Sets whether Pretty Printing Mode should be used
        /// </summary>
        bool PrettyPrintMode
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for Writers that Support engaging High Speed Write Mode for some Graphs
    /// </summary>
    public interface IHighSpeedWriter
    {
        /// <summary>
        /// Gets/Sets whether the Writer can use High Speed Write Mode if the Graph is deemed suitable for this
        /// </summary>
        bool HighSpeedModePermitted
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for Writers that support varying levels of Syntax Compression
    /// </summary>
    public interface ICompressingWriter
    {
        /// <summary>
        /// Gets/Sets the Compression Level that the Writer is using
        /// </summary>
        /// <remarks>Compression Level is an arbitrary figure that the Writer can interpret as it wants, implementations of this interface should state in the XML Comments for this property what the different values mean.  The Standard Compression levels provided by the <see cref="WriterCompressionLevel">WriterCompressionLevel</see> enumeration are intended as guides and Writers may interpret these as they desire.</remarks>
        int CompressionLevel
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for Writers that support use of DTDs to compress output
    /// </summary>
    public interface IDtdWriter
    {
        /// <summary>
        /// Gets/Sets whether DTDs can be used
        /// </summary>
        bool UseDtd
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for Writers that can use attributes (e.g. XML or HTML based writers) which allows you to control whether the writer will choose to use attributes to encode data which could otherwise be expressed as elements
    /// </summary>
    public interface IAttributeWriter
    {
        /// <summary>
        /// Gets/Sets whether literal objects can be compressed as attributes
        /// </summary>
        bool UseAttributes
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for Writers that support the use of Namespaces and allows a set of Default Namespaces to be defined
    /// </summary>
    public interface INamespaceWriter
    {
        /// <summary>
        /// Gets/Sets the Default Namespaces used for writing
        /// </summary>
        INamespaceMapper DefaultNamespaces
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for Writers that support multi-threaded writing
    /// </summary>
    public interface IMultiThreadedWriter
    {
        /// <summary>
        /// Gets/Sets whether multi-threading is used
        /// </summary>
        bool UseMultiThreadedWriting
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for Writers that generate HTML
    /// </summary>
    public interface IHtmlWriter
    {
        /// <summary>
        /// Gets/Sets a Stylesheet file used to format the HTML
        /// </summary>
        String Stylesheet
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the anchor tags used to display the URIs of URI Nodes
        /// </summary>
        String CssClassUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Blank Node IDs
        /// </summary>
        String CssClassBlankNode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Literals
        /// </summary>
        String CssClassLiteral
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the anchor tags used to display Literal datatypes
        /// </summary>
        String CssClassDatatype
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the span tags used to display Literal language specifiers
        /// </summary>
        String CssClassLangSpec
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the CSS class used for the div tags used to group chunks of markup into a box
        /// </summary>
        String CssClassBox
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets a Prefix that is applied to all href attributes
        /// </summary>
        String UriPrefix
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Interface for writers which use formatters from the Formatting namespace
    /// </summary>
    public interface IFormatterBasedWriter
    {
        /// <summary>
        /// Gets the Type for the Triple Formatter this writer uses
        /// </summary>
        /// <remarks>
        /// This should be the type descriptor for a type that implements <see cref="ITripleFormatter">ITripleFormatter</see>
        /// </remarks>
        Type TripleFormatterType
        {
            get;
        }
    }


}
