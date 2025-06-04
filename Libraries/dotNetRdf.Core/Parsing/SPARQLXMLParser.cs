/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.IO;
using System.Web;
using System.Xml;
using VDS.RDF.Parsing.Contexts;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing;

/// <summary>
/// Parser for SPARQL Results XML Format.
/// </summary>
public class SparqlXmlParser 
    : BaseSparqlResultsReader
{
    /// <summary>
    /// Namespace Uri for SPARQL Namespace.
    /// </summary>
    public const string SparqlNamespace = "http://www.w3.org/2005/sparql-results#";

    /// <summary>
    /// Get the settings passed to the underlying XML parser on read.
    /// </summary>
    /// <remarks>This property can be used to modify the configuration of the 
    /// XML parser used to parse SPARQL XML. In particular it makes it possible to 
    /// disable the processing of the XML DTD in environments where remote XML
    /// entity references are of concern.</remarks>
    public readonly XmlReaderSettings XmlReaderSettings = new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Parse,
        ConformanceLevel = ConformanceLevel.Document,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true,
    };

    /// <summary>
    /// Loads a Result Set from an Input.
    /// </summary>
    /// <param name="results">Result Set to load into.</param>
    /// <param name="input">Input to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(SparqlResultSet results, TextReader input, IUriFactory uriFactory)
    {
        if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        Load(new ResultSetHandler(results), input, uriFactory);
    }

    /// <summary>
    /// Loads a Result Set from an Input Stream.
    /// </summary>
    /// <param name="results">Result Set to load into.</param>
    /// <param name="input">Input Stream to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(SparqlResultSet results, StreamReader input, IUriFactory uriFactory)
    {
        if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        Load(new ResultSetHandler(results), input, uriFactory);
    }

    /// <summary>
    /// Loads a Result Set from a File.
    /// </summary>
    /// <param name="results">Result Set to load into.</param>
    /// <param name="filename">File to load from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(SparqlResultSet results, string filename, IUriFactory uriFactory)
    {
        if (results == null) throw new RdfParseException("Cannot read SPARQL Results into a null Result Set");
        if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        Load(results, new StreamReader(File.OpenRead(filename)), uriFactory);
    }

    /// <summary>
    /// Loads a Result Set from an Input using a Results Handler.
    /// </summary>
    /// <param name="handler">Results Handler to use.</param>
    /// <param name="input">Input to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(ISparqlResultsHandler handler, TextReader input, IUriFactory uriFactory)
    {
        if (handler == null) throw new RdfParseException("Cannot read SPARQL Results using a null Results Handler");
        if (input == null) throw new RdfParseException("Cannot read SPARQL Results from a null Input");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));

        try
        {
            // Parse the XML
            var reader = XmlReader.Create(input, XmlReaderSettings);
            Parse(new SparqlXmlParserContext(reader, handler, uriFactory));
        }
        catch
        {
            throw;
        }
        finally
        {
            try
            {
                input.Close();
            }
            catch
            {
                // No catch actions - this is a cleanup attempt only
            }
        }
    }

    /// <summary>
    /// Loads a Result Set from an Input using a Results Handler.
    /// </summary>
    /// <param name="handler">Results Handler to use.</param>
    /// <param name="input">Input Stream to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(ISparqlResultsHandler handler, StreamReader input, IUriFactory uriFactory)
    {
        if (input == null) throw new RdfParseException("Cannot read SPARQL Results from a null input");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        Load(handler, (TextReader)input, uriFactory);
    }

    /// <summary>
    /// Loads a Result Set from a file using a Results Handler.
    /// </summary>
    /// <param name="handler">Results Handler to use.</param>
    /// <param name="filename">File to read from.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public override void Load(ISparqlResultsHandler handler, string filename, IUriFactory uriFactory)
    {
        if (filename == null) throw new RdfParseException("Cannot read SPARQL Results from a null File");
        if (uriFactory == null) throw new ArgumentNullException(nameof(uriFactory));
        Load(handler, new StreamReader(File.OpenRead(filename)), uriFactory);
    }


    /// <summary>
    /// Parses the XML Result Set format into a set of SPARQLResult objects.
    /// </summary>
    /// <param name="context">Parser Context.</param>
    private void Parse(SparqlXmlParserContext context)
    {
        try
        {
            context.Handler.StartResults();

            // Get the Document Element and check it's a Sparql element
            if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as it was not possible to read a document element from the input");
            while (context.Input.NodeType != XmlNodeType.Element)
            {
                if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as it was not possible to read a document element from the input");
            }
            if (!context.Input.Name.Equals("sparql"))
            {
                throw new RdfParseException("Unable to Parse a SPARQL Result Set from the provided XML since the Document Element is not a <sparql> element!");
            }

            // Go through it's attributes and check the Namespace is specified
            var nsfound = false;
            if (context.Input.HasAttributes)
            {
                for (var i = 0; i < context.Input.AttributeCount; i++)
                {
                    context.Input.MoveToNextAttribute();
                    if (context.Input.Name.Equals("xmlns"))
                    {
                        if (!context.Input.Value.Equals(SparqlNamespace))
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <sparql> element has an incorrect Namespace!");
                        }
                        else
                        {
                            nsfound = true;
                        }
                    }
                }
            }
            if (!nsfound)
            {
                throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <sparql> element fails to specify the SPARQL Namespace!");
            }

            // Get the Variables from the Header
            if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as could not read a <head> element from the input");
            if (!context.Input.Name.Equals("head"))
            {
                throw new RdfParseException("Unable to Parse a SPARQL Result Set since the first Child Node of the <sparql> element is not the required <head> element!");
            }

            // Only parser <variable> and <link> elements if not an empty <head /> element
            if (!context.Input.IsEmptyElement)
            {
                while (context.Input.Read())
                {
                    // Stop reading when we hit the </head>
                    if (context.Input.NodeType == XmlNodeType.EndElement && context.Input.Name.Equals("head")) break;

                    // Looking for <variable> elements
                    if (context.Input.Name.Equals("variable"))
                    {
                        // Should only have 1 attribute
                        if (context.Input.AttributeCount != 1)
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <variable> element has too few/many attributes, only a 'name' attribute should be present!");
                        }
                        else
                        {
                            // Add the Variable to the list
                            context.Input.MoveToNextAttribute();
                            if (!context.Handler.HandleVariable(context.Input.Value)) ParserHelper.Stop();
                            context.Variables.Add(context.Input.Value);
                        }
                    }
                    else if (context.Input.Name.Equals("link"))
                    {
                        // Not bothered about <link> elements
                    }
                    else
                    {
                        // Some unexpected element
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <head> contains an unexpected element <" + context.Input.Name + ">!");
                    }
                }
            }

            if (!context.Input.Name.Equals("head"))
            {
                throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of the input before the closing </head> element was found");
            }

            // Look at the <results> or <boolean> element
            if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as could not read a <results> element from the input");
            if (context.Input.Name.Equals("results"))
            {
                // Only parser <result> elements if it's not an empty <results /> element
                if (!context.Input.IsEmptyElement)
                {
                    while (context.Input.Read())
                    {
                        // Stop reading when we hit the </results>
                        if (context.Input.NodeType == XmlNodeType.EndElement && context.Input.Name.Equals("results")) break;

                        // Must be a <result> element
                        if (!context.Input.Name.Equals("result"))
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <results> element contains an unexpected element <" + context.Input.Name + ">!");
                        }

                        // Empty Elements generate an Empty Result
                        if (context.Input.IsEmptyElement)
                        {
                            if (!context.Handler.HandleResult(new SparqlResult())) ParserHelper.Stop();
                            continue;
                        }

                        // Get the values of each Binding
                        string var;
                        INode value;
                        var result = new SparqlResult();
                        while (context.Input.Read())
                        {
                            // Stop reading when we hit the </binding>
                            if (context.Input.NodeType == XmlNodeType.EndElement && context.Input.Name.Equals("result")) break;

                            // Must be a <binding> element
                            if (!context.Input.Name.Equals("binding"))
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <result> element contains an unexpected element <" + context.Input.Name + ">!");
                            }

                            // Must have only 1 attribute
                            if (context.Input.AttributeCount != 1)
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element has too few/many attributes, only a 'name' attribute should be present!");
                            }

                            // Get the Variable this is a binding for and its Value
                            context.Input.MoveToNextAttribute();
                            var = context.Input.Value;
                            if (!context.Input.Read()) throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of input when the contents of a <binding> element was expected");
                            value = ParseValue(context);

                            // Check that the Variable was defined in the Header
                            if (!context.Variables.Contains(var))
                            {
                                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element attempts to bind a value to the variable '" + var + "' which is not defined in the <head> by a <variable> element!");
                            }

                            // Set the Variable to the Value
                            result.SetValue(var, value);
                        }

                        // Check that all Variables are bound for a given result binding nulls where appropriate
                        foreach (var v in context.Variables)
                        {
                            if (!result.HasValue(v))
                            {
                                result.SetValue(v, null);
                            }
                        }

                        if (!context.Input.Name.Equals("result"))
                        {
                            throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of the input before a closing </result> element was found");
                        }

                        // Add to results set
                        result.SetVariableOrdering(context.Variables);
                        if (!context.Handler.HandleResult(result)) ParserHelper.Stop();
                    }
                }

                if (!context.Input.Name.Equals("results"))
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set as reached the end of the input before the closing </results> element was found");
                }
            }
            else if (context.Input.Name.Equals("boolean"))
            {
                // Can't be any <variable> elements
                if (context.Variables.Count > 0)
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <boolean> element is specified but the <head> contained one/more <variable> elements which is not permitted!");
                }

                try
                {
                    // Get the value of the <boolean> element as a Boolean
                    var b = bool.Parse(context.Input.ReadInnerXml());
                    context.Handler.HandleBooleanResult(b);
                }
                catch (Exception)
                {
                    throw new RdfParseException("Unable to Parse a SPARQL Result Set since the <boolean> element contained a value that could not be understood as a Boolean value!");
                }
            }
            else
            {
                throw new RdfParseException("Unable to Parse a SPARQL Result Set since the second Child Node of the <sparql> element is not the required <results> or <boolean> element!");
            }

            context.Handler.EndResults(true);
        }
        catch (RdfParsingTerminatedException)
        {
            context.Handler.EndResults(true);
        }
        catch
        {
            // Some other Error
            context.Handler.EndResults(false);
            throw;
        }
    }

    /// <summary>
    /// Internal Helper method which parses the child element of a &lt;binding&gt; element into an <see cref="INode">INode</see>.
    /// </summary>
    /// <param name="context">Parser Context.</param>
    /// <returns></returns>
    private INode ParseValue(SparqlXmlParserContext context)
    {
        switch (context.Input.Name)
        {
            case "uri":
                return ParserHelper.TryResolveUri(context, context.Input.ReadElementContentAsString());
            case "literal":
                switch (context.Input.AttributeCount)
                {
                    case 0:
                        // Literal with no Data Type/Language Specifier
                        return context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(context.Input.ReadInnerXml()));
                    case >= 1:
                        {
                            string lang = null;
                            Uri dt = null;
                            while (context.Input.MoveToNextAttribute())
                            {
                                switch (context.Input.Name)
                                {
                                    case "xml:lang":
                                        // Language is specified
                                        lang = context.Input.Value;
                                        break;
                                    case "datatype":
                                        // Data Type is specified
                                        dt = ((IUriNode) ParserHelper.TryResolveUri(context, context.Input.Value)).Uri;
                                        break;
                                    default:
                                        RaiseWarning("SPARQL Result Set has a <literal> element with an unknown attribute '" + context.Input.Name + "'!");
                                        break;
                                }
                            }

                            if (lang != null && dt != null) throw new RdfParseException("Cannot have both a 'xml:lang' and a 'datatype' attribute on a <literal> element");

                            context.Input.MoveToContent();
                            if (lang != null)
                            {
                                return context.Handler.CreateLiteralNode(context.Input.ReadElementContentAsString(), lang);
                            }

                            return dt != null ? context.Handler.CreateLiteralNode(context.Input.ReadElementContentAsString(), dt) : context.Handler.CreateLiteralNode(HttpUtility.HtmlDecode(context.Input.ReadInnerXml()));
                        }
                    default:
                        throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <literal> element has too many Attributes, only 1 of 'xml:lang' or 'datatype' may be specified!");
                }

            case "bnode":
                {
                    var bnodeID = context.Input.ReadElementContentAsString();
                    if (bnodeID.StartsWith("_:"))
                    {
                        return context.Handler.CreateBlankNode(bnodeID.Substring(2));
                    }
                    else if (bnodeID.Contains("://"))
                    {
                        return context.Handler.CreateBlankNode(bnodeID.Substring(bnodeID.IndexOf("://") + 3));
                    }
                    else if (bnodeID.Contains(":"))
                    {
                        return context.Handler.CreateBlankNode(bnodeID.Substring(bnodeID.LastIndexOf(':') + 1));
                    }
                    else
                    {
                        return context.Handler.CreateBlankNode(bnodeID);
                    }
                }
            case "unbound":
                // HACK: This is a really ancient feature of the SPARQL Results XML format (from Working Draft in 2005) which we support to ensure compatability with old pre-standardisation SPARQL endpoints (like 3store based ones)
                context.Input.ReadInnerXml();
                return null;

            case "triple":
                context.Input.ReadStartElement("triple");
                INode s, p, o = null;
                if (!context.Input.IsStartElement("subject"))
                    throw new RdfParseException(
                        $"Unable to parse a SPARQL Result Set since a <triple> element. Expected to find <subject> as the first child of <triple>.");
                context.Input.Read();
                s = ParseValue(context);
                context.Input.ReadEndElement();
                if (!context.Input.IsStartElement("predicate"))
                    throw new RdfParseException(
                        $"Unable to parse a SPARQL Result Set since a <triple> element. Expected to find <predicate> as the second child of <triple>.");
                context.Input.Read();
                p = ParseValue(context);
                context.Input.ReadEndElement();
                if (!context.Input.IsStartElement("object"))
                    throw new RdfParseException(
                        $"Unable to parse a SPARQL Result Set since a <triple> element. Expected to find <object> as the third child of <triple>.");
                context.Input.Read();
                o = ParseValue(context);
                context.Input.ReadEndElement();
                context.Input.ReadEndElement();
                return context.Handler.CreateTripleNode(new Triple(s, p, o));

            default:
                throw new RdfParseException("Unable to Parse a SPARQL Result Set since a <binding> element contains an unexpected element <" + context.Input.Name + ">!");
        }
    }

    /// <summary>
    /// Helper Method which raises the Warning event when a non-fatal issue with the SPARQL Results being parsed is detected.
    /// </summary>
    /// <param name="message">Warning Message.</param>
    private void RaiseWarning(string message)
    {
        SparqlWarning d = Warning;
        if (d != null)
        {
            d(message);
        }
    }

    /// <summary>
    /// Event raised when a non-fatal issue with the SPARQL Results being parsed is detected
    /// </summary>
    public override event SparqlWarning Warning;

    /// <summary>
    /// Gets the String representation of the Parser which is a description of the syntax it parses.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "SPARQL Results XML";
    }
}
