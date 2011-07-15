using System;
using System.IO;
using System.Printing;              // this *** needs System.Printing reference
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging; // these bastards are hidden in the ReachFramework reference

namespace PrintEngine
{

    /// <summary>
    /// Represents the PrintPreviewDialog class to preview documents
    /// of type FlowDocument, IDocumentPaginatorSource or DocumentPaginatorWrapper
    /// using the PrintPreviewDocumentViewer class.
    /// </summary>
    public partial class PrintPreviewDialog : Window
    {
        private object m_Document;

        /// <summary>
        /// Initialize a new instance of the PrintEngine.PrintPreviewDialog class.
        /// </summary>
        public PrintPreviewDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the document viewer.
        /// </summary>
        public PrintPreviewDocumentViewer DocumentViewer
        {
            get 
            { 
                return documentViewer;
            }
            set 
            { 
                documentViewer = value; 
            }
        }

        /// <summary>
        /// Loads the specified FlowDocument document for print preview.
        /// </summary>
        public void LoadDocument(FlowDocument document)
        {
            m_Document = document;
            string temp = System.IO.Path.GetTempFileName();
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }

            XpsDocument xpsDoc = new XpsDocument(temp, FileAccess.ReadWrite);
            XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
            xpsWriter.Write(((FlowDocument)document as IDocumentPaginatorSource).DocumentPaginator);

            documentViewer.Document = xpsDoc.GetFixedDocumentSequence();
            xpsDoc.Close();
        }

        /// <summary>
        /// Loads the specified DocumentPaginatorWrapper document for print preview.
        /// </summary>
        public void LoadDocument(DocumentPaginatorWrapper document)
        {
            m_Document = document;
            string temp = System.IO.Path.GetTempFileName();
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }

            XpsDocument xpsDoc = new XpsDocument(temp, FileAccess.ReadWrite);
            XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
            xpsWriter.Write(document);

            documentViewer.Document = xpsDoc.GetFixedDocumentSequence();
            xpsDoc.Close();
        }

        /// <summary>
        /// Loads the specified IDocumentPaginatorSource document for print preview.
        /// </summary>
        public void LoadDocument(IDocumentPaginatorSource document)
        {
            m_Document = document;
            documentViewer.Document = (IDocumentPaginatorSource)document;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    /// <summary>
    /// Represents the PrintPreviewDocumentViewer class with PrintQueue and PrintTicket properties for the document viewer.
    /// </summary>
    public class PrintPreviewDocumentViewer : DocumentViewer
    {
        private PrintQueue m_PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
        private PrintTicket m_PrintTicket;

        /// <summary>
        /// Gets or sets the print queue manager.
        /// </summary>
        public PrintQueue PrintQueue
        {
            get 
            { 
                return m_PrintQueue; 
            }
            set 
            { 
                m_PrintQueue = value; 
            }
        }

        /// <summary>
        /// Gets or sets the print settings for the print job.
        /// </summary>
        public PrintTicket PrintTicket
        {
            get 
            { 
                return m_PrintTicket; 
            }
            set 
            { 
                m_PrintTicket = value; 
            }
        }

        protected override void OnPrintCommand()
        {
            // get a print dialog, defaulted to default printer and default printer's preferences.
            PrintDialog printDialog = new PrintDialog();
            printDialog.PrintQueue = m_PrintQueue;
            printDialog.PrintTicket = m_PrintTicket;

            if (printDialog.ShowDialog() == true)
            {
                m_PrintQueue = printDialog.PrintQueue;
                m_PrintTicket = printDialog.PrintTicket;
                printDialog.PrintDocument(this.Document.DocumentPaginator, "PrintPreviewJob");
            }
        }
    }
}
