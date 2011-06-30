using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Formatting
{
    public class SparqlXmlFormatter : IResultSetFormatter
    {
        private String GetBaseHeader()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""" + SparqlSpecsHelper.SparqlNamespace + "\">";
        }

        public string FormatResultSetHeader(IEnumerable<string> variables)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(this.GetBaseHeader());
            output.AppendLine("<head>");
            foreach (String var in variables)
            {
                output.AppendLine(" <variable name=\"" + var + "\" />"); 
            }
            output.AppendLine("</head>");
            output.Append("<results>");
            return output.ToString();
        }

        public string FormatResultSetHeader()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(this.GetBaseHeader());
            output.AppendLine("<head />");
            output.Append("<results>");
            return output.ToString();
        }

        public string FormatResultSetFooter()
        {
            return "</results>\n</sparql>";
        }

        public string Format(SparqlResult result)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(" <result>");
            foreach (String var in result.Variables)
            {
                if (result.HasValue(var))
                {
                    INode value = result[var];
                    if (value != null)
                    {
                        output.Append("  <binding name=\"" + var + "\">");
                        switch (value.NodeType)
                        {
                            case NodeType.Blank:
                                output.Append("<bnode>" + ((IBlankNode)value).InternalID + "</bnode>");
                                break;
                            case NodeType.GraphLiteral:
                                throw new RdfOutputException(WriterErrorMessages.GraphLiteralsUnserializable("SPARQL XML Results"));
                            case NodeType.Literal:
                                ILiteralNode lit = (ILiteralNode)value;
                                output.Append("<literal");
                                if (lit.DataType != null)
                                {
                                    output.Append(" datatype=\"" + WriterHelper.EncodeForXml(lit.DataType.ToString()) + "\">" + lit.Value + "</literal>");
                                }
                                else if (!lit.Language.Equals(String.Empty))
                                {
                                    output.Append(" xml:lang=\"" + lit.Language + "\">" + lit.Value + "</literal>");
                                }
                                else
                                {
                                    output.Append(">" + lit.Value + "</literal>");
                                }
                                break;
                            case NodeType.Uri:
                                output.Append("<uri>" + WriterHelper.EncodeForXml(value.ToString()) + "</uri>");
                                break;
                            case NodeType.Variable:
                                throw new RdfOutputException(WriterErrorMessages.VariableNodesUnserializable("SPARQL XML Results"));
                            default:
                                throw new RdfOutputException(WriterErrorMessages.UnknownNodeTypeUnserializable("SPARQL XML Results"));
                        }
                        output.AppendLine("</binding>");
                    }
                }
            }
            output.Append(" </result>");
            return output.ToString();
        }

        public string FormatBooleanResult(bool result)
        {
            return " <boolean>" + result.ToString().ToLower() + "</boolean>";
        }

        public override string ToString()
        {
            return "SPARQL XML Results";
        }
    }
}
