using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Utilities.GraphBenchmarker.Test;

namespace VDS.RDF.Utilities.GraphBenchmarker
{
    public partial class fclsGraphBenchmarker : Form
    {
        private BindingList<Type> _graphTypes = new BindingList<Type>();
        private BindingList<Type> _tripleCollectionTypes = new BindingList<Type>();
        private BindingList<Type> _nodeCollectionTypes = new BindingList<Type>();
        private BindingList<TestCase> _testCases = new BindingList<TestCase>();
        private BindingList<String> _dataFiles = new BindingList<string>();

        public fclsGraphBenchmarker()
        {
            InitializeComponent();

            this.lstIGraphImpl.DataSource = this._graphTypes;
            this.lstIGraphImpl.DisplayMember = "FullName";
            this.lstTripleCollectionImpl.DataSource = this._tripleCollectionTypes;
            this.lstTripleCollectionImpl.DisplayMember = "FullName";
            this.lstNodeCollectionImpl.DataSource = this._nodeCollectionTypes;
            this.lstNodeCollectionImpl.DisplayMember = "FullName";
            this.lstTestCases.DataSource = this._testCases;
            this.lstTestData.DataSource = this._dataFiles;

            this.FindTypes(Assembly.GetAssembly(typeof(IGraph)));
            this.FindTestData("Data\\");
        }

        private void FindTypes(Assembly assm)
        {
            Type igraph = typeof(IGraph);
            Type tcol = typeof(BaseTripleCollection);
            Type ncol = typeof(BaseNodeCollection);

            foreach (Type t in assm.GetTypes())
            {
                if (t.GetInterfaces().Contains(igraph))
                {
                    if (this.IsTestableGraphType(t)) this._graphTypes.Add(t);
                }
                else if (t.IsSubclassOf(tcol))
                {
                    if (this.IsTestableCollectionType(t)) this._tripleCollectionTypes.Add(t);
                }
                else if (t.IsSubclassOf(ncol))
                {
                    if (this.IsTestableCollectionType(t)) this._nodeCollectionTypes.Add(t);
                }
            }
        }

        private bool IsTestableGraphType(Type t)
        {
            return !t.IsAbstract && t.IsPublic && t.GetConstructors().Any(c => c.GetParameters().Length == 0 || (c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType.Equals(typeof(BaseTripleCollection))));
        }

        private bool IsTestableCollectionType(Type t)
        {
            return !t.IsAbstract && t.IsPublic && t.GetConstructors().Any(c => c.GetParameters().Length == 0);
        }

        private void FindTestData(String dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (String file in Directory.GetFiles(dir))
                {
                    String ext = MimeTypesHelper.GetTrueFileExtension(file);
                    if (ext.StartsWith(".")) ext = ext.Substring(1);
                    if (MimeTypesHelper.Definitions.Any(d => d.FileExtensions.Contains(ext)))
                    {
                        this._dataFiles.Add(file);
                    }
                }
            }
        }

        private void chkUseDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkUseDefault.Checked)
            {
                //Check whether it can be enabled
                if (this.lstIGraphImpl.SelectedItem != null)
                {
                    Type t = (Type)this.lstIGraphImpl.SelectedItem;
                    if (t.GetConstructors().Any(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType.Equals(typeof(BaseTripleCollection))))
                    {
                        this.lstTripleCollectionImpl.Enabled = true;
                    }
                    else
                    {
                        this.chkUseDefault.Checked = false;
                        this.lstTripleCollectionImpl.Enabled = false;
                    }
                }
                else
                {
                    this.chkUseDefault.Checked = false;
                    this.lstTripleCollectionImpl.Enabled = false;
                }
            }
            else
            {
                this.lstTripleCollectionImpl.Enabled = false;
            }
        }

        private void lstIGraphImpl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.chkUseDefault.Checked)
            {
                chkUseDefault_CheckedChanged(sender, e);
            }
        }

        private void btnAddTestCase_Click(object sender, EventArgs e)
        {
            if (this.lstIGraphImpl.SelectedItem != null)
            {
                if (this.chkUseDefault.Checked && this.lstTripleCollectionImpl.SelectedItem != null)
                {
                    if (this.chkUseNodeDefault.Checked && this.lstNodeCollectionImpl.SelectedItem != null)
                    {
                        this._testCases.Add(new TestCase((Type)this.lstIGraphImpl.SelectedItem, (Type)this.lstTripleCollectionImpl.SelectedItem, (Type)this.lstNodeCollectionImpl.SelectedItem));
                    }
                    else
                    {
                        this._testCases.Add(new TestCase((Type)this.lstIGraphImpl.SelectedItem, (Type)this.lstTripleCollectionImpl.SelectedItem));
                    }
                }
                else if (this.chkUseNodeDefault.Checked && this.lstNodeCollectionImpl.SelectedItem != null)
                {
                    this._testCases.Add(new TestCase((Type)this.lstIGraphImpl.SelectedItem, null, (Type)this.lstNodeCollectionImpl.SelectedItem));
                }
                else
                {
                    this._testCases.Add(new TestCase((Type)this.lstIGraphImpl.SelectedItem));
                }
            }
        }

        private void btnRemoveTestCase_Click(object sender, EventArgs e)
        {
            if (this.lstTestCases.SelectedItem != null)
            {
                this._testCases.Remove((TestCase)this.lstTestCases.SelectedItem);
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (this._testCases.Count > 0)
            {
                if (this.lstTestData.SelectedItem != null)
                {
                    TestSet set = TestSet.Standard;
                    if (this.radLoadAndMem.Checked) set = TestSet.LoadAndMemory;

                    TestSuite suite = new TestSuite(this._testCases, (String)this.lstTestData.SelectedItem, (int)this.numIterations.Value, set);
                    fclsTestRunner runner = new fclsTestRunner(suite);
                    runner.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Please selected Test Data to use...", "Test Data Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please create one/more Test Cases...", "Test Case(s) Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void chkUseNodeDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkUseNodeDefault.Checked)
            {
                //Check whether it can be enabled
                if (this.lstIGraphImpl.SelectedItem != null)
                {
                    Type t = (Type)this.lstIGraphImpl.SelectedItem;
                    if (t.GetConstructors().Any(c => (c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType.Equals(typeof(BaseNodeCollection))) || (c.GetParameters().Length == 2 && c.GetParameters()[0].ParameterType.Equals(typeof(BaseTripleCollection)) && c.GetParameters()[1].ParameterType.Equals(typeof(BaseNodeCollection)))))
                    {
                        this.lstNodeCollectionImpl.Enabled = true;
                    }
                    else
                    {
                        this.chkUseNodeDefault.Checked = false;
                        this.lstNodeCollectionImpl.Enabled = false;
                    }
                }
                else
                {
                    this.chkUseNodeDefault.Checked = false;
                    this.lstNodeCollectionImpl.Enabled = false;
                }
            }
            else
            {
                this.lstNodeCollectionImpl.Enabled = false;
            }
        }
    }
}
