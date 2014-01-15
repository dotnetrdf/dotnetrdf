using System;
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Properties;

namespace VDS.RDF.Utilities.StoreManager.Dialogues
{
    public partial class OptionsDialogue : Form
    {
        public OptionsDialogue()
        {
            InitializeComponent();
            this.LoadOptions();
        }

        private void LoadOptions()
        {
            this.chkAlwaysEdit.Checked = Settings.Default.AlwaysEdit;
            this.chkAlwaysRestoreActiveConnections.Checked = Settings.Default.AlwaysRestoreActiveConnections;
            this.chkEditorDetectUrls.Checked = Settings.Default.EditorDetectUrls;
            this.chkEditorHighlighting.Checked = Settings.Default.EditorHighlighting;
            this.chkEditorWordWrap.Checked = Settings.Default.EditorWordWrap;
            this.chkPromptRestoreActiveConnections.Checked = Settings.Default.PromptRestoreActiveConnections;
            this.chkShowStartPage.Checked = Settings.Default.ShowStartPage;
            this.chkUtf8Bom.Checked = Settings.Default.UseUtf8Bom;
            Options.UseBomForUtf8 = Settings.Default.UseUtf8Bom;

            this.numMaxRecentConnections.Value = Settings.Default.MaxRecentConnections;
            this.numPreviewSize.Value = Settings.Default.PreviewSize;
        }

        private void SaveOptions()
        {
            Settings.Default.AlwaysEdit = this.chkAlwaysEdit.Checked;
            Settings.Default.AlwaysRestoreActiveConnections = this.chkAlwaysRestoreActiveConnections.Checked;
            Settings.Default.EditorDetectUrls = this.chkEditorDetectUrls.Checked;
            Settings.Default.EditorHighlighting = this.chkEditorHighlighting.Checked;
            Settings.Default.EditorWordWrap = this.chkEditorWordWrap.Checked;
            Settings.Default.PromptRestoreActiveConnections = this.chkPromptRestoreActiveConnections.Checked;
            Settings.Default.ShowStartPage = this.chkShowStartPage.Checked;
            Settings.Default.UseUtf8Bom = this.chkUtf8Bom.Checked;
            Options.UseBomForUtf8 = Settings.Default.UseUtf8Bom;

            Settings.Default.MaxRecentConnections = (int) this.numMaxRecentConnections.Value;
            Settings.Default.PreviewSize = (int) this.numPreviewSize.Value;

            Settings.Default.Save();
        }

        private void ResetOptions(bool toDefault)
        {
            if (toDefault)
            {
                Settings.Default.Reset();
                Settings.Default.Save();
            }
            this.LoadOptions();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SaveOptions();
            this.Close();
        }

        private void btnResetDefault_Click(object sender, EventArgs e)
        {
            this.ResetOptions(true);
        }

        private void btnResetCurrent_Click(object sender, EventArgs e)
        {
            this.ResetOptions(false);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
