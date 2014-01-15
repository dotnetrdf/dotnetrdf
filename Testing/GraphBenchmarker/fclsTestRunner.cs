using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Utilities.GraphBenchmarker.Test;
using VDS.RDF.Utilities.StoreManager.Forms;

namespace VDS.RDF.Utilities.GraphBenchmarker
{
    internal delegate void RunTestSuiteDelegate();

    public partial class fclsTestRunner : CrossThreadForm
    {
        private TestSuite _suite;
        private bool _cancelled = false, _hasCancelled = false, _hasFinished = false;
        private RunTestSuiteDelegate _d;

        public fclsTestRunner(TestSuite suite)
        {
            InitializeComponent();
            this._suite = suite;
            this._suite.Progress += new TestSuiteProgressHandler(_suite_Progress);
            this._suite.Cancelled += new TestSuiteProgressHandler(_suite_Cancelled);

            this.lstTestCases.DataSource = this._suite.TestCases;
            this.lstTests.DataSource = this._suite.Tests;
            this.lstTests.DisplayMember = "Name";

            this.prgTests.Minimum = 0;
            this.prgTests.Maximum = this._suite.TestCases.Count * this._suite.Tests.Count;
            this.prgTests.Value = 0;

            this.Shown += new EventHandler(fclsTestRunner_Shown);
        }

        void _suite_Cancelled()
        {
            this._hasCancelled = true;
            this.CrossThreadSetEnabled(this.btnCancel, true);
            this.CrossThreadSetText(this.lblProgress, "Test Suite cancelled");
        }

        void _suite_Progress()
        {
            int progress = (this._suite.CurrentTestCase * this._suite.Tests.Count) + this._suite.CurrentTest;
            this.CrossThreadUpdateProgress(this.prgTests, progress);
            if (!this._cancelled)
            {
                this.CrossThreadSetText(this.lblProgress, "Running Test " + (this._suite.CurrentTest + 1) + " of " + this._suite.Tests.Count + " for Test Case " + (this._suite.CurrentTestCase + 1) + " of " + this._suite.TestCases.Count);
            }
            this.ShowTestResult();
        }

        void fclsTestRunner_Shown(object sender, EventArgs e)
        {
            this._d = new RunTestSuiteDelegate(this._suite.Run);
            this._d.BeginInvoke(new AsyncCallback(this.TestsCompleteCallback), null);
        }

        private void lstTestCases_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ShowTestResult();
        }

        private void lstTests_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ShowTestInformation();
            this.ShowTestResult();
        }

        private void ShowTestInformation()
        {
            if (this.lstTests.SelectedItem != null)
            {
                ITest test = (ITest)this.lstTests.SelectedItem;
                this.grpTestInfo.Text = "Test Information - " + test.Name;
                this.txtDescription.Text = test.Description;
            }
            else
            {
                this.grpTestInfo.Text = "Test Information";
                this.txtDescription.Text = "No Test currently selected";
            }
        }

        private void ShowTestResult()
        {
            if (this.CrossThreadGetSelectedItem(this.lstTests) != null)
            {
                Object item = this.CrossThreadGetSelectedItem(this.lstTestCases);
                if (item != null)
                {
                    TestCase testCase = (TestCase)item;
                    int i = this.CrossThreadGetSelectedIndex(this.lstTests);
                    if (i < testCase.Results.Count)
                    {
                        TestResult r = testCase.Results[i];
                        StringBuilder output = new StringBuilder();
                        output.AppendLine("Total Elapsed Time: " + r.Elapsed.ToString());
                        output.AppendLine("Performance Metric: " + r.ToString());
                        this.CrossThreadSetText(this.txtResults, output.ToString());
                    }
                    else
                    {
                        this.CrossThreadSetText(this.txtResults, "Test has not yet been run for the currently selected Test Case");
                    }
                }
                else
                {
                    this.CrossThreadSetText(this.txtResults, "No Test Case currently selected");
                }
            }
            else
            {
                this.CrossThreadSetText(this.txtResults, "No Test currently selected");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!this._cancelled && !this._hasFinished)
            {
                this._cancelled = true;
                this._suite.Cancel();
                this.lblProgress.Text = "Waiting for Tests to cancel...";
                this.btnCancel.Text = "Close";
                this.btnCancel.Enabled = false;
            }
            else if (this._hasCancelled || this._hasFinished)
            {
                this.Close();
            }
        }

        private void TestsCompleteCallback(IAsyncResult result)
        {
            this._hasFinished = true;
            try
            {
                this._d.EndInvoke(result);
                if (this._cancelled)
                {
                    this.CrossThreadMessage("Test Suite cancelled", "Tests Cancelled", MessageBoxIcon.Information);
                }
                else
                {
                    this.CrossThreadUpdateProgress(this.prgTests, this.prgTests.Maximum);
                    this.CrossThreadMessage("Test Suite completed successfully", "Tests Completed", MessageBoxIcon.Information);
                }
                this.CrossThreadSetText(this.btnCancel, "Close");
            }
            catch (Exception ex)
            {
                this.CrossThreadMessage("Test Suite failed due to the following error: " + ex.Message, "Tests Failed", MessageBoxIcon.Error);
            }
            this.CrossThreadSetText(this.btnCancel, "Close");
            this.CrossThreadSetEnabled(this.btnExport, true);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (this.sfdExport.ShowDialog() == DialogResult.OK)
            {
                String file = this.sfdExport.FileName;
                try
                {
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        //First write a couple of Rows detailing the Test Setup
                        writer.WriteLine("Test Data," + this._suite.Data);
                        writer.WriteLine("Test Iterations," + this._suite.Iterations);
                        writer.WriteLine(",");

                        //Then write Header Row listing the Test Cases
                        writer.WriteLine(",Test Cases");
                        writer.Write(',');
                        foreach (TestCase c in this._suite.TestCases)
                        {
                            writer.Write(c.ToString());
                            writer.Write(',');
                        }
                        writer.WriteLine();

                        //Then for each Test dump the results
                        for (int i = 0; i < this._suite.Tests.Count; i++)
                        {
                            //First remember to show the Test Name in the Leftmost column
                            writer.Write(this._suite.Tests[i].Name);
                            writer.Write(',');

                            //Then show a Result for each Test Case
                            foreach (TestCase c in this._suite.TestCases)
                            {
                                if (i < c.Results.Count)
                                {
                                    TestResult r = c.Results[i];
                                    if (r.Actions > 0)
                                    {
                                        writer.Write(r.Speed);
                                    }
                                    else
                                    {
                                        writer.Write(r.Memory);
                                    }
                                }
                                else
                                {
                                    writer.Write("N/A,");
                                }
                                writer.Write(',');
                            }
                            writer.WriteLine();
                        }

                        writer.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Exporting Results: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
