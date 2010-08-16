using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace ParserGUI
{
    public partial class fclsParserGUI : Form
    {
        private IGraph _g = null;

        public fclsParserGUI()
        {
            InitializeComponent();
        }

        private void fclsParserGUI_Load(object sender, EventArgs e)
        {
            this.cboParser.SelectedIndex = 0;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (this.ofdFile.ShowDialog() == DialogResult.OK)
            {
                this.txtFile.Text = this.ofdFile.FileName;
            }
        }

        private void radSourceURI_CheckedChanged(object sender, EventArgs e)
        {
            this.txtURI.Enabled = this.radSourceURI.Checked;
            this.radParserManual.Enabled = !this.radSourceURI.Checked;
            if (this.radSourceURI.Checked)
            {
                this.radParserAuto.Checked = true;
            }
        }

        private void radSourceFile_CheckedChanged(object sender, EventArgs e)
        {
            this.txtFile.Enabled = this.radSourceFile.Checked;
        }

        private void radSourceRaw_CheckedChanged(object sender, EventArgs e)
        {
            this.txtRawData.Enabled = this.radSourceRaw.Checked;
        }

        private void radParserManual_CheckedChanged(object sender, EventArgs e)
        {
            this.cboParser.Enabled = this.radParserManual.Checked;
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            this._g = null;

            Graph g = new Graph();
            StringBuilder output = new StringBuilder();
            IRdfReader parser;

            Console.SetOut(new StringWriter(output));

            try
            {
                if (this.radSourceURI.Checked)
                {
                    output.AppendLine("Attempting to Retrieve and Parse RDF from URI '" + this.txtURI.Text + "'");
                    if (this.chkParserTrace.Checked || this.chkTokeniserTrace.Checked)
                    {
                        output.AppendLine("Unable to use selected Parser Options for Parsing URIs, Parser selection and configuration is handled automatically by the dotNetRDF URILoader class");
                    }
                    UriLoader.Load(g, new Uri(this.txtURI.Text));
                    output.AppendLine("Parsed OK");
                }
                else if (this.radSourceFile.Checked)
                {
                    if (File.Exists(this.txtFile.Text))
                    {
                        output.AppendLine("Attempting to Read and Parse RDF from File '" + this.txtFile.Text + "'");

                        if (this.radParserAuto.Checked)
                        {
                            parser = this.GetParser(Path.GetExtension(this.txtFile.Text));
                            output.AppendLine("Automatically selected Parser '" + parser.GetType().ToString() + "'");
                        }
                        else
                        {
                            parser = this.GetParser();
                            output.AppendLine("Using manually selected Parser '" + parser.GetType().ToString() + "'");
                        }

                        this.SetParserOptions(parser, output);
                        parser.Load(g, this.txtFile.Text);

                        output.AppendLine("Parsed OK");
                    }
                    else
                    {
                        output.AppendLine("ERROR - File does not exist");
                    }
                }
                else if (this.radSourceRaw.Checked)
                {
                    String data = this.txtRawData.Text;

                    output.AppendLine("Attempting to parse RDF from Raw Data input");

                    if (this.radParserAuto.Checked)
                    {
                        output.AppendLine("Using StringParser static class which will attempt to auto-detect the RDF Parser to use based on the input data");
                        if (this.chkParserTrace.Checked || this.chkTokeniserTrace.Checked)
                        {
                            output.AppendLine("Unable to use selected Parser Options for Parsing Raw Data, Parser selection and configuration is handled automatically by the dotNetRDF StringParser class");
                        }
                        StringParser.Parse(g, data);
                    }
                    else
                    {
                        parser = this.GetParser();
                        this.SetParserOptions(parser, output);
                        output.AppendLine("Using manually selected Parser '" + parser.GetType().ToString() + "'");
                        StringParser.Parse(g, data, parser);
                    }
                    output.AppendLine("Parsed OK");
                }

                this._g = g;
            }
            catch (UriFormatException)
            {
                output.AppendLine("ERROR - URI Format was invalid");
            }
            catch (RdfParseException parseEx)
            {
                output.AppendLine("ERROR - A Parsing Error occurred");
                output.AppendLine(parseEx.Message);
                output.AppendLine(parseEx.StackTrace);
            }
            catch (RdfException rdfEx)
            {
                output.AppendLine("ERROR - A RDF Error occurred");
                output.AppendLine(rdfEx.Message);
                output.AppendLine(rdfEx.StackTrace);
            }
            catch (WebException webEx)
            {
                output.AppendLine("ERROR - A HTTP Error occurred");
                output.AppendLine(webEx.Message);
                output.AppendLine(webEx.StackTrace);
            }
            catch (Exception ex)
            {
                output.AppendLine("ERROR - Some other Error occurred");
                output.AppendLine(ex.Message);
                output.AppendLine(ex.StackTrace);
            }
            finally
            {
                output.AppendLine();
                output.AppendLine("Graph contains the following Triples (may be empty or incomplete if an error occurred):");
                output.AppendLine();

                foreach (Triple t in g.Triples)
                {
                    output.AppendLine(t.ToString());
                }
            }

            this.txtResults.Text = output.ToString();

            this.btnOutput.Enabled = (this._g != null);
        }

        private IRdfReader GetParser()
        {
            switch (this.cboParser.SelectedIndex)
            {
                case 0:
                    return new NTriplesParser();
                case 1:
                    return new NTriplesParser();
                case 2:
                    return new TurtleParser();
                case 3:
                    return new Notation3Parser();
                case 4:
                    return new RdfXmlParser();
                case 5:
                    return new RdfJsonParser();
                default:
                    return new NTriplesParser();
            }
        }

        private IRdfReader GetParser(String ext)
        {
            if (ext.StartsWith(".")) ext = ext.Substring(1);

            if (ext.Equals("nt", StringComparison.OrdinalIgnoreCase))
            {
                return new NTriplesParser();
            }
            else if (ext.Equals("ttl", StringComparison.OrdinalIgnoreCase))
            {
                return new TurtleParser();
            }
            else if (ext.Equals("n3", StringComparison.OrdinalIgnoreCase))
            {
                return new Notation3Parser();
            }
            else if (ext.Equals("rdf", StringComparison.OrdinalIgnoreCase))
            {
                return new RdfXmlParser();
            }
            else if (ext.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                return new RdfJsonParser();
            }
            else
            {
                return new NTriplesParser();
            }
        }

        private void SetParserOptions(IRdfReader parser, StringBuilder output)
        {
            if (this.chkTokeniserTrace.Checked)
            {
                if (parser is ITraceableTokeniser)
                {
                    output.AppendLine("Enabling Tokeniser Tracing for the Parser");
                    ((ITraceableTokeniser)parser).TraceTokeniser = true;
                }
                else
                {
                    output.AppendLine("Tokeniser Tracing not supported for this Parser");
                }
            }

            if (this.chkParserTrace.Checked)
            {
                if (parser is ITraceableParser)
                {
                    output.AppendLine("Enabling Parser Tracing for the Parser");
                    ((ITraceableParser)parser).TraceParsing = true;
                }
                else
                {
                    output.AppendLine("Parser Tracing not supported for this Parser");
                }
            }
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            if (this._g != null)
            {
                fclsOutput output = new fclsOutput(this._g);
                output.ShowDialog();
            }
        }
    }
}
