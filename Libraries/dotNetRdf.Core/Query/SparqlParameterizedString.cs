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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;

/// <summary>
/// A SPARQL Parameterized String is a String that can contain parameters in the same fashion as a SQL command string.
/// </summary>
/// <remarks>
/// <para>
/// This is intended for use in applications which may want to dynamically build SPARQL queries/updates where user input may comprise individual values in the triples patterns and the applications want to avoid SPARQL injection attacks which change the meaning of the query/update.
/// </para>
/// <para>
/// It works broadly in the same way as a SqlCommand would in that you specify a string with paramters specified in the form <strong>@name</strong> and then use various set methods to set the actual values that should be used.  The values are only substituted for parameters when you actually call the <see cref="SparqlParameterizedString.ToString">ToString()</see> method to get the final string representation of the command. E.g.
/// </para>
/// <code>
/// SparqlParameterizedString queryString = new SparqlParameterizedString();
/// queryString.CommandText = @"SELECT * WHERE
/// {
///     ?s a @type .
/// }";
/// queryString.SetUri("type", new Uri("http://example.org/myType"));
/// Console.WriteLine(queryString.ToString());
/// </code>
/// <para>
/// Would result in the following being printed to the Console:
/// </para>
/// <code>
/// SELECT * WHERE
/// {
///     ?s a &lt;http://example.org/myType&gt;
/// }
/// </code>
/// <para>
/// Calling a Set method to set a parameter that has already been set changes that value and the new value will be used next time you call <see cref="SparqlParameterizedString.ToString">ToString()</see> - this may be useful if you plan to execute a series of queries/updates using a series of values since you need not instantiate a completely new parameterized string each time.
/// </para>
/// <para>
/// This class was added to a library based on a suggestion by Alexander Sidorov and ideas from slides from <a href="http://www.slideshare.net/Morelab/sparqlrdqlsparul-injection">Slideshare</a> by Almedia et al.
/// </para>
/// <para>
/// <strong>PERFORMANCE TIPS:</strong> if building the command text incrementally, avoid using <c> CommandText += </c> and use the AppendSubQuery or Append methods instead.
/// </para>
/// </remarks>
public class SparqlParameterizedString
{
    private static readonly char[] InvalidIriCharacters = " >\"{}|^`[]".ToCharArray();

    private static readonly IGraph _g = new NonIndexedGraph();
    private static readonly SparqlFormatter EmptyFormatter = new SparqlFormatter();

    private INamespaceMapper _nsmap = new NamespaceMapper(true);
    private readonly List<string> _commandText = new List<string>();
    private readonly Dictionary<string, INode> _parameters = new Dictionary<string, INode>();
    private readonly Dictionary<string, INode> _variables = new Dictionary<string, INode>();

    private readonly SparqlFormatter _formatter;

    private static readonly Regex ValidParameterNamePattern = new Regex("^@?[\\w\\-_]+$", RegexOptions.IgnoreCase);
    private static readonly Regex ValidVariableNamePattern = new Regex("^[?$]?[\\w\\-_]+$", RegexOptions.IgnoreCase);

    private static readonly Regex PreambleCapturePattern = new Regex("(BASE|PREFIX\\s+([\\w\\-_]*):)\\s*<([^>]+)>", RegexOptions.IgnoreCase);

    /// <summary>
    /// Creates a new empty parameterized String.
    /// </summary>
    public SparqlParameterizedString()
    {
        _formatter = new SparqlFormatter(_nsmap);
    }

    /// <summary>
    /// Creates a new parameterized String.
    /// </summary>
    /// <param name="command">Command Text.</param>
    public SparqlParameterizedString(string command)
        : this()
    {
        CommandText = command;
    }

    /// <summary>
    /// Gets/Sets the Namespace Map that is used to prepend PREFIX declarations to the command.
    /// </summary>
    public INamespaceMapper Namespaces
    {
        get => _nsmap;
        set
        {
            if (value != null) _nsmap = value;
        }
    }

    /// <summary>
    /// Gets/Sets the Base URI which will be used to prepend BASE declarations to the command.
    /// </summary>
    public Uri BaseUri { get; set; }


    /// <summary>
    /// Gets/Sets the parameterized Command Text.
    /// </summary>
    public virtual string CommandText
    {
        get => string.Join("", _commandText);
        set
        {
            ResetText();
            PreprocessText(TrimPreamble(value)); // TODO deprecate the _command field and replace it with the join of _cache
        }
    }

    #region Sparql extension methods

    /// <summary>
    /// Appends the given query as a sub-query to the existing command text, any prefixes in the sub-query are moved to the parent query.
    /// </summary>
    /// <param name="query">Query.</param>
    public void AppendSubQuery(SparqlQuery query)
    {
        PreprocessText(TrimPreamble(EmptyFormatter.Format(new SubQueryPattern(query))));
        // NOTE: the namespaces are already updated through during the TrimPreamble call
    }

    /// <summary>
    /// Appends the given query as a sub-query to the existing command text, any prefixes in the sub-query are moved to the parent query but any parameter/variable assignments will be lost.
    /// </summary>
    /// <param name="query">Query.</param>
    public void AppendSubQuery(SparqlParameterizedString query)
    {
        // TODO shouldn't we ensure that the text is wrapped in { } to get a correct subquery pattern ?
        //      this may cause compatibility issues though
        Append(query);
    }

    /// <summary>
    /// Appends the given text to the existing command text, any prefixes in the sub-query are moved to the parent query but any parameter/variable assignments will be lost.
    /// </summary>
    /// <param name="text">Text.</param>
    public void Append(SparqlParameterizedString text)
    {
        // Merges the instances caches and placeholders
        var offset = _commandText.Count;
        _commandText.AddRange(text._commandText);
        // Update the namespaces
        _nsmap.Import(text.Namespaces);
    }

    /// <summary>
    /// Appends the given text to the existing command text, any prefixes in the command are moved to the parent query.
    /// </summary>
    /// <param name="text">Text.</param>
    public void Append(string text)
    {
        PreprocessText(TrimPreamble(text));
    }

    #endregion

    /// <summary>
    /// Gets/Sets the Query processor which is used when you call the <see cref="SparqlParameterizedString.ExecuteQuery()">ExecuteQuery()</see> method.
    /// </summary>
    public ISparqlQueryProcessor QueryProcessor { get; set; }

    /// <summary>
    /// Gets/Sets the Query processor which is used when you call the <see cref="SparqlParameterizedString.ExecuteUpdate()">ExecuteUpdate()</see> method.
    /// </summary>
    public ISparqlUpdateProcessor UpdateProcessor { get; set; }

    /// <summary>
    /// Gets an enumeration of the Variables for which Values have been set.
    /// </summary>
    public IEnumerable<KeyValuePair<string, INode>> Variables => _variables;

    /// <summary>
    /// Gets an enumeration of the Parameters for which Values have been set.
    /// </summary>
    public IEnumerable<KeyValuePair<string, INode>> Parameters => _parameters;

    #region Parameters and Variables assignment methods

    /// <summary>
    /// Clears all set Parameters and Variables.
    /// </summary>
    public virtual void Clear()
    {
        ClearParameters();
        ClearVariables();
    }

    /// <summary>
    /// Clears all set Parameters.
    /// </summary>
    public virtual void ClearParameters()
    {
        _parameters.Clear();
    }

    /// <summary>
    /// Clears all set Variables.
    /// </summary>
    public virtual void ClearVariables()
    {
        _variables.Clear();
    }

    /// <summary>
    /// Sets the Value of a Parameter. 
    /// </summary>
    /// <param name="name">Parameter Name.</param>
    /// <param name="value">Value.</param>
    /// <remarks>
    /// Can be used in derived classes to set the value of parameters if the derived class defines additional methods for adding values for parameters.
    /// </remarks>
    public void SetParameter(string name, INode value)
    {
        if (value == null)
        {
            UnsetParameter(name);
            return;
        }
        // Only allow the setting of valid parameter names
        if (!ValidParameterNamePattern.IsMatch(name)) throw new FormatException("The parameter name '" + name + "' is not a valid parameter name, parameter names must consist only of alphanumeric characters and hypens/underscores");

        // OPT: Could ensure that the parameter name actually appears in the command?
        if (name[0] == '@') name = name.Substring(1);

        // Finally can set/update parameter value
        _parameters[name] = value;
    }

    /// <summary>
    /// Removes a previously set value for a Parameter.
    /// </summary>
    /// <param name="name">Parameter Name.</param>
    /// <remarks>
    /// There is generally no reason to do this since you can just set a parameters value to change it.
    /// </remarks>
    public void UnsetParameter(string name)
    {
        if (name == null) return;
        if (name[0] == '@') name = name.Substring(1);
        _parameters.Remove(name);
    }

    /// <summary>
    /// Removes a previously set value for a Variable.
    /// </summary>
    /// <param name="name">Variable Name.</param>
    /// <remarks>
    /// May be useful if you have a skeleton query/update into which you sometimes substitute values for variables but don't always do so.
    /// </remarks>
    public void UnsetVariable(string name)
    {
        if (name == null) return;
        if (name[0] == '?' || name[0] == '$') name = name.Substring(1);
        _variables.Remove(name);
    }

    /// <summary>
    /// Sets the Value of a Variable.
    /// </summary>
    /// <param name="name">Variable Name.</param>
    /// <param name="value">Value.</param>
    public virtual void SetVariable(string name, INode value)
    {
        if (value == null)
        {
            UnsetVariable(name);
            return;
        }
        // Only allow the setting of valid variable names
        if (!ValidVariableNamePattern.IsMatch(name)) throw new FormatException("The variable name '" + name + "' is not a valid variable name, variable names must consist only of alphanumeric characters and hyphens/underscores");

        // OPT: Could ensure that the variable name actually appears in the command?
        if (name[0] == '?' || name[0] == '$') name = name.Substring(1);
        _variables[name] = value;
    }

    /// <summary>
    /// Sets the Parameter to an Integer Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, int value)
    {
        SetParameter(name, value.ToLiteral(_g));
    }

    /// <summary>
    /// Sets the Parameter to an Integer Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, long value)
    {
        SetParameter(name, value.ToLiteral(_g));
    }

    /// <summary>
    /// Sets the Parameter to an Integer Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, short value)
    {
        SetParameter(name, value.ToLiteral(_g));
    }

    /// <summary>
    /// Sets the Parameter to a Decimal Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, decimal value)
    {
        SetParameter(name, value.ToLiteral(_g));
    }

    /// <summary>
    /// Sets the Parameter to a Float Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, float value)
    {
        SetParameter(name, value.ToLiteral(_g));
    }

    /// <summary>
    /// Sets the Parameter to a Double Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, double value)
    {
        SetParameter(name, value.ToLiteral(_g));
    }

    /// <summary>
    /// Sets the Parameter to a Date Time Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, DateTime value)
    {
        SetLiteral(name, value, true);
    }

    /// <summary>
    /// Sets the Parameter to a Date Time Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds.</param>
    public void SetLiteral(string name, DateTime value, bool precise)
    {
        SetParameter(name, value.ToLiteral(_g, precise));
    }

    /// <summary>
    /// Sets the Parameter to a Date Time Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, DateTimeOffset value)
    {
        SetLiteral(name, value, true);
    }

    /// <summary>
    /// Sets the Parameter to a Date Time Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    /// <param name="precise">Whether to preserve precisely i.e. include fractional seconds.</param>
    public void SetLiteral(string name, DateTimeOffset value, bool precise)
    {
        SetParameter(name, value.ToLiteral(_g, precise));
    }

    /// <summary>
    /// Sets the Parameter to a Duration Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, TimeSpan value)
    {
        SetParameter(name, value.ToLiteral(_g));
    }

    /// <summary>
    /// Sets the Parameter to a Boolean Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Integer.</param>
    public void SetLiteral(string name, bool value)
    {
        SetParameter(name, value.ToLiteral(_g));
    }

    /// <summary>
    /// Sets the Parameter to an Untyped Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">The literal value.</param>
    /// <param name="normalizeValue">Whether to normalize the string value of <paramref name="value"/>.</param>
    public void SetLiteral(string name, string value, bool normalizeValue)
    {
        if (value == null) throw new ArgumentNullException(nameof(value), "Cannot set a Literal to be null");
        SetParameter(name, new LiteralNode(value, normalizeValue));
    }

    /// <summary>
    /// Sets the Parameter to a Typed Literal.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">The literal value.</param>
    /// <param name="datatype">Datatype URI.</param>
    /// <param name="normalizeLiteralValue">Whether to normalize the string value of <paramref name="value"/>.</param>
    public void SetLiteral(string name, string value, Uri datatype, bool normalizeLiteralValue)
    {
        if (value == null) throw new ArgumentNullException(nameof(value), "Cannot set a Literal to be null");
        SetParameter(name, datatype == null ? new LiteralNode(value, normalizeLiteralValue) : new LiteralNode(value, datatype, normalizeLiteralValue));
    }

    /// <summary>
    /// Sets the Parameter to a Literal with a Language Specifier.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">The Literal value.</param>
    /// <param name="lang">The language specifier.</param>
    /// <param name="normalizeLiteralValue">Whether to normalize the string value of <paramref name="value"/>.</param>
    public void SetLiteral(string name, string value, string lang, bool normalizeLiteralValue)
    {
        if (value == null) throw new ArgumentNullException(nameof(value), "Cannot set a Literal to be null");
        if (lang == null) throw new ArgumentNullException(nameof(lang), "Cannot set a Literal to have a null Language");
        SetParameter(name, new LiteralNode(value, lang, normalizeLiteralValue));
    }

    /// <summary>
    /// Sets the Parameter to a URI.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">URI.</param>
    public void SetUri(string name, Uri value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value), "Cannot set a URI to be null");
        SetParameter(name, new UriNode(value));
    }

    /// <summary>
    /// Sets the Parameter to be a Blank Node with the given ID.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <param name="value">Node ID.</param>
    /// <remarks>
    /// Only guarantees that the Blank Node ID will not clash with any other Blank Nodes added by other calls to this method or it's overload which generates anonymous Blank Nodes.  If the base query text into which you are inserting parameters contains Blank Nodes then the IDs generated here may clash with those IDs.
    /// </remarks>
    public void SetBlankNode(string name, string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value), "Cannot set a Blank Node to have a null ID");
        if (value.Equals(string.Empty)) throw new ArgumentException("Cannot set a Blank Node to have an empty ID", nameof(value));
        SetParameter(name, _g.CreateBlankNode(value));
    }

    /// <summary>
    /// Sets the Parameter to be a new anonymous Blank Node.
    /// </summary>
    /// <param name="name">Parameter.</param>
    /// <remarks>
    /// Only guarantees that the Blank Node ID will not clash with any other Blank Nodes added by other calls to this method or it's overload which takes an explicit Node ID.  If the base query text into which you are inserting parameters contains Blank Nodes then the IDs generated here may clash with those IDs.
    /// </remarks>
    public void SetBlankNode(string name)
    {
        SetParameter(name, _g.CreateBlankNode());
    }

    #endregion

    #region Runtime Evaluation

    /// <summary>
    /// Executes this command as a query.
    /// </summary>
    /// <returns></returns>
    public SparqlResultSet ExecuteQuery()
    {
        var rset = new SparqlResultSet();
        ExecuteQuery(null, new ResultSetHandler(rset));
        return rset;
    }

    /// <summary>
    /// Executes this command as a query.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler)
    {
        if (QueryProcessor == null) throw new RdfQueryException("Cannot call ExecuteQuery() when the QueryProcessor property has not been set");

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(ToString());
        QueryProcessor.ProcessQuery(rdfHandler, resultsHandler, q);
    }

    /// <summary>
    /// Executes this command as an update.
    /// </summary>
    public void ExecuteUpdate()
    {
        if (UpdateProcessor == null) throw new SparqlUpdateException("Cannot call ExecuteUpdate() when the UpdateProcessor property has not been set");

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(ToString());
        UpdateProcessor.ProcessCommandSet(cmds);
    }

    #endregion

    #region Command text processing and Serialization

    /// <summary>
    /// Clears the preprocessing structures.
    /// </summary>
    private void ResetText()
    {
        // this._placeHolders.Clear();
        _commandText.Clear();
    }


    /// <summary>
    /// Trims out the SPARQL preamble (BASE and PREFIX definitions) from the command text.
    /// </summary>
    /// <remarks>
    /// This is done so the instance can be directly merged into another SparqlParameterizedString through the Append methods.
    /// </remarks>
    private string TrimPreamble(string value)
    {
        var commandStart = 0;
        foreach (Match preambleItem in PreambleCapturePattern.Matches(value))
        {
            if (preambleItem.Groups[1].Value.ToUpper().StartsWith("BASE"))
            {
                BaseUri = new Uri(preambleItem.Groups[3].Value);
                Namespaces.AddNamespace("", BaseUri);
            }
            else
            {
                Namespaces.AddNamespace(preambleItem.Groups[2].Value, new Uri(preambleItem.Groups[3].Value));
            }
            commandStart = preambleItem.Index + preambleItem.Length;
        }
        return value.Substring(commandStart);
    }

    private static bool IsIdentifierChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

    /// <summary>
    /// Provides some fast string exploration to determine valid parameter/variable placeholders and leave out any constant SPARQL ambiguous patterns (language tags, parameter- or variable-like patterns in IRIs or in string literals...)
    /// </summary>
    private void PreprocessText(string value)
    {
        var inIri = false;
        var literalOpeningChar = '\0';
        var inLiteral = false;
        var inLongLiteral = false;
        var escaping = false;

        var currentSegment = new StringBuilder();
        for (int i = 0, l = value.Length; i < l; i++)
        {
            var c = value[i];
            switch (c)
            {
                case '\\':
                    if (inLiteral)
                    {
                        escaping = !escaping;
                    }
                    break;
                case '\'':
                case '"':
                    if (!inLiteral)
                    {
                        literalOpeningChar = c;
                        inLiteral = true;
                        if (i < l - 2 && value[i + 1] == c && value[i + 2] == c)
                        {
                            inLongLiteral = true;
                            currentSegment.Append(c, 2);
                            i += 2;
                        }
                    }
                    else if (!escaping && c == literalOpeningChar)
                    {
                        if (!inLongLiteral)
                        {
                            inLiteral = false;
                        }
                        else if (i < l - 2 && value[i + 1] == c && value[i + 2] == c)
                        {
                            inLongLiteral = false;
                            currentSegment.Append(c, 2);
                            i += 2;
                        }
                    }
                    break;
                case '<':
                    // toggle whether me may start a Iri or break one
                    inIri = !inIri;
                    break;
                case '@':
                case '$':
                case '?':
                    if (!inLiteral && !inIri)
                    {
                        if (c == '@' && i > 0 && value[i - 1] == '"')
                        {
                            // encountered a language tag => do nothing
                        }
                        else if (c == '?' && i < l && !IsIdentifierChar(value[i + 1]))
                        {
                            // must be a propertyPath ? modifier => do nothing
                        }
                        else
                        {
                            // Start variable or parameter capture
                            _commandText.Add(currentSegment.ToString());
                            currentSegment.Clear();
                            currentSegment.Append(c);
                            // Capture the identifier
                            var hasRemainingChar = false;
                            for (var idCharIndex = i + 1; idCharIndex < l; idCharIndex++)
                            {
                                var idc = value[idCharIndex];
                                i = idCharIndex;
                                // check that the character is in valid identifier range
                                if (IsIdentifierChar(idc))
                                {
                                    currentSegment.Append(idc);
                                }
                                else
                                {
                                    hasRemainingChar = true;
                                    break;
                                }
                            }
                            // stop the capture 
                            // TODO should we check that the identifier is not empty, just to be sure ?
                            var assignment = currentSegment.ToString();
                            _commandText.Add(assignment);
                            currentSegment.Clear();

                            // Add the last character found (if any) to the new segment, since it is not part of the variable or parameter name
                            if (hasRemainingChar) currentSegment.Append(value[i]);

                            // nothing more to do here
                            continue;
                        }
                    }
                    break;
                default:
                    if (inIri)
                    {
                        // check wether the character is a valid in IRIs
                        if (char.IsControl(c) || InvalidIriCharacters.Contains(c))
                        {
                            inIri = false;
                        }
                    }
                    break;
            }
            currentSegment.Append(c);
        }
        if (currentSegment.Length > 0) _commandText.Add(currentSegment.ToString());
    }

    /// <summary>
    /// Returns the actual Query/Update String with parameter and variable values inserted.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();

        // First prepend Base declaration
        if (BaseUri != null)
        {
            output.AppendLine("BASE <" + _formatter.FormatUri(BaseUri) + ">");
        }

        // Next prepend any Namespace Declarations
        foreach (var prefix in _nsmap.Prefixes)
        {
            output.AppendLine("PREFIX " + prefix + ": <" + _formatter.FormatUri(_nsmap.GetNamespaceUri(prefix)) + ">");
        }

        // Then append the text with variable and parameters replaced by their values if set
        INode value = null;
        for (int i = 0, l = _commandText.Count; i < l; i++)
        {
            var segment = _commandText[i];
            switch (segment[0])
            {
                case '@':
                    var paramName = segment.Substring(1);
                    _parameters.TryGetValue(paramName, out value);
                    if (value != null)
                    {
                        output.Append(_formatter.Format(value));
                    }
                    else
                    {
                        output.Append(segment);
                    }
                    break;
                case '?':
                case '$':
                    var varName = segment.Substring(1);
                    _variables.TryGetValue(varName, out value);
                    if (value != null)
                    {
                        output.Append(_formatter.Format(value));
                    }
                    else
                    {
                        output.Append(segment);
                    }
                    break;
                default:
                    output.Append(_commandText[i]);
                    break;
            }
        }
        return output.ToString();
    }

    #endregion
}
