using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace ParserGUI
{
    public partial class fclsOutput : Form
    {
        private IGraph _g;

        public fclsOutput(IGraph g)
        {
            InitializeComponent();

            this._g = g;

            foreach (Triple t in this._g.Triples)
            {
                String[] items = new String[] { t.Subject.ToString(), t.Predicate.ToString(), t.Object.ToString() };
                ListViewItem item = new ListViewItem(items);
                this.lvwTriples.Items.Add(item);
            }

            this.cboWriter.SelectedIndex = 0;
            this.cboCompression.SelectedIndex = 1;
        }

        private void btnOutput_Click(object sender, EventArgs e)
        {
            //Select the Writer
            IRdfWriter writer;
            switch (this.cboWriter.SelectedIndex)
            {
                case 0:
                    writer = new NTriplesWriter();
                    break;
                case 1:
                    writer = new TurtleWriter();
                    break;
                case 2:
                    writer = new CompressingTurtleWriter();
                    break;
                case 3:
                    writer = new Notation3Writer();
                    break;
                case 4:
                    writer = new RdfXmlWriter();
                    break;
                case 5:
                    writer = new FastRdfXmlWriter();
                    break;
                case 6:
                    writer = new RdfJsonWriter();
                    break;
                default:
                    writer = new NTriplesWriter();
                    break;
            }

            //Select the Parser
            IRdfReader reader;
            switch (this.cboWriter.SelectedIndex)
            {
                case 1:
                case 2:
                    reader = new TurtleParser();
                    break;
                case 3:
                    reader = new Notation3Parser();
                    break;
                case 4:
                case 5:
                case 6:
                    reader = new RdfXmlParser();
                    break;
                case 7:
                    reader = new RdfJsonParser();
                    break;
                case 0:
                default:
                    reader = new NTriplesParser();
                    break;
            }

            //Configure Options on the Writer
            if (writer is IPrettyPrintingWriter)
            {
                ((IPrettyPrintingWriter)writer).PrettyPrintMode = this.chkPrettyPrinting.Checked;
            }
            if (writer is IHighSpeedWriter)
            {
                ((IHighSpeedWriter)writer).HighSpeedModePermitted = this.chkHighSpeed.Checked;
            }
            if (writer is ICompressingWriter)
            {
                int c = WriterCompressionLevel.Default;
                switch (this.cboCompression.SelectedIndex)
                {
                    case 0:
                        c = WriterCompressionLevel.None;
                        break;
                    case 1:
                        c = WriterCompressionLevel.Default;
                        break;
                    case 2:
                        c = WriterCompressionLevel.Minimal;
                        break;
                    case 3:
                        c = WriterCompressionLevel.Medium;
                        break;
                    case 4:
                        c = WriterCompressionLevel.More;
                        break;
                    case 5:
                        c = WriterCompressionLevel.High;
                        break;
                    default:
                        c = WriterCompressionLevel.Default;
                        break;
                }
                ((ICompressingWriter)writer).CompressionLevel = c;
            }

            //Write the Output to a String
            StringBuilder output = new StringBuilder();
            String data;

            try
            {
                output.AppendLine("Attempting to serialize the Graph using your chosen Writer...");

                data = StringWriter.Write(this._g, writer);

                output.AppendLine("Serialized OK");
                output.AppendLine();
                output.AppendLine(new String('-', 50));
                output.AppendLine(data);
                output.AppendLine(new String('-', 50));

                output.AppendLine("Attempting to parse this serialized output back in to check it is a valid serialization...");

                Graph h = new Graph();
                StringParser.Parse(h, data, reader);

                output.AppendLine("Parsed OK");

                if (h.Triples.Count == this._g.Triples.Count)
                {
                    output.AppendLine("Correct number of Triples were parsed from the serialized output");

                    Dictionary<INode, INode> mapping;
                    if (this._g.Equals(h, out mapping))
                    {
                        output.AppendLine("Graphs are Equal");
                        if (mapping != null)
                        {
                            output.AppendLine("Blank Node Mapping between Original Graph and Output is as follows:");
                            foreach (KeyValuePair<INode, INode> pair in mapping)
                            {
                                output.AppendLine(pair.Key.ToString() + " => " + pair.Value.ToString());
                            }
                        }
                    }
                    else
                    {
                        output.AppendLine("ERROR - Graphs are not equal");
                        output.AppendLine("Expected the two Graphs to be equal");
                    }
                }
                else
                {
                    output.AppendLine("ERROR - Incorrect number of Triples were parsed from the serialized output");
                    output.AppendLine("Expected " + this._g.Triples.Count + " Triples but got " + h.Triples.Count);
                }
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
            catch (Exception ex)
            {
                output.AppendLine("ERROR - Some other Error occurred");
                output.AppendLine(ex.Message);
                output.AppendLine(ex.StackTrace);
            }

            this.txtResults.Text = output.ToString();
        }
    }
}
