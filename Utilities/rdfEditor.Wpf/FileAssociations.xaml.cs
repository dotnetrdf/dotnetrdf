/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BrendanGrant.Helpers.FileAssociation;
using VDS.RDF;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for FileAssociations.xaml
    /// </summary>
    public partial class FileAssociations : Window
    {
        public const String RegistryProgramID = "rdfEditor.exe";

        private List<FileAssociationInfo> _associations = new List<FileAssociationInfo>()
        {
            new FileAssociationInfo(".nt"),
            new FileAssociationInfo(".ttl"),
            new FileAssociationInfo(".n3"),
            new FileAssociationInfo(".rdf"),
            new FileAssociationInfo(".json"),
            new FileAssociationInfo(".rq"),
            new FileAssociationInfo(".srx"),
            new FileAssociationInfo(".trig"),
            new FileAssociationInfo(".nq")
        };

        private HashSet<String> _currentAssociations = new HashSet<string>();

        [RegistryPermission(SecurityAction.Demand, Unrestricted=true)]
        public FileAssociations()
        {
            InitializeComponent();

            this.chkAlwaysCheckFileAssociations.IsChecked = Properties.Settings.Default.AlwaysCheckFileAssociations;

            RegistryHelper.UseCurrentUser = true;

            //Ensure there is Program Association Info for us in the Registry
            ProgramAssociationInfo rdfEditorInfo = new ProgramAssociationInfo(RegistryProgramID);
            if (!rdfEditorInfo.Exists) rdfEditorInfo.Create();
            bool hasOpenVerb = false;
            foreach (ProgramVerb verb in rdfEditorInfo.Verbs)
            {
                if (verb.Name.Equals("open"))
                {
                    if (verb.Command.StartsWith(System.IO.Path.GetFullPath("rdfEditor.exe")))
                    {
                        hasOpenVerb = true;
                    }
                    else
                    {
                        rdfEditorInfo.RemoveVerb("open");
                    }
                }
            }
            if (!hasOpenVerb)
            {
                rdfEditorInfo.AddVerb(new ProgramVerb("open", System.IO.Path.GetFullPath("rdfEditor.exe") + " \"%1\""));
            }

            //See which extensions are currently associated to us
            foreach (FileAssociationInfo info in _associations)
            {
                if (!info.Exists)
                {
                    //If no association exists then we'll aim to create it
                    this.SetAssociationsChecked(info.Extension);
                }
                else
                {
                    //Check if the File Associations Program ID is equal to ours
                    if (info.ProgID.Equals(RegistryProgramID))
                    {
                        //Prog ID is equal to ours to we are associated with this extension
                        this._currentAssociations.Add(info.Extension);
                        this.SetAssociationsChecked(info.Extension);
                    }
                    else if (info.ProgID.Equals(String.Empty))
                    {
                        //No Prog ID specified so we'll aim to create it
                        this.SetAssociationsChecked(info.Extension);
                    }
                    else
                    {
                        ProgramAssociationInfo progInfo = new ProgramAssociationInfo(info.ProgID);
                        if (!progInfo.Exists)
                        {
                            //No program association exists so we'll aim to create it
                            this.SetAssociationsChecked(info.Extension);
                        }
                        else
                        {
                            //Associated with some other program currently
                            bool hasExistingOpen = false;
                            foreach (ProgramVerb verb in progInfo.Verbs)
                            {
                                if (verb.Name.Equals("open"))
                                {
                                    hasExistingOpen = true;
                                }
                            }
                            //No Open Verb so we'll try to associated with ourselves
                            if (!hasExistingOpen) this.SetAssociationsChecked(info.Extension);
                        }
                    }
                }
            }
        }

        public bool AllAssociated
        {
            get
            {
                return (this._currentAssociations.Count == _associations.Count);
            }
        }

        private void SetAssociationsChecked(String ext)
        {
            foreach (CheckBox cb in stackAssociations.Children.OfType<CheckBox>())
            {
                if (cb.Tag.Equals(ext)) cb.IsChecked = true;
            }
        }

        private bool IsAssociationChecked(String ext)
        {
            foreach (CheckBox cb in stackAssociations.Children.OfType<CheckBox>())
            {
                if (cb.Tag.Equals(ext))
                {
                    if (cb.IsChecked != null)
                    {
                        return (bool)cb.IsChecked;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        [RegistryPermission(SecurityAction.Demand, Unrestricted=true)]
        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            if (this.chkAlwaysCheckFileAssociations.IsChecked != null)
            {
                Properties.Settings.Default.AlwaysCheckFileAssociations = (bool)this.chkAlwaysCheckFileAssociations.IsChecked;
                Properties.Settings.Default.Save();
            }

            //Set Associations appropriately
            foreach (FileAssociationInfo info in this._associations)
            {
                //Ensure File Association exists in the registry
                if (!info.Exists) 
                {
                    info.Create();
                    try
                    {
                        info.ContentType = MimeTypesHelper.GetMimeType(info.Extension);
                    }
                    catch
                    {
                        //Ignore error, just means we don't know the MIME type for the Extension
                    }
                    info.PerceivedType = PerceivedTypes.Text;
                } 
                
                //Ensure there is always a Perceived Type and a Content Type registered
                if (info.PerceivedType == PerceivedTypes.None)
                {
                    info.PerceivedType = PerceivedTypes.Text;
                }
                if (info.ContentType.Equals(String.Empty))
                {
                    try
                    {
                        String mimeType = MimeTypesHelper.GetMimeType(info.Extension);
                        info.ContentType = mimeType;
                    } 
                    catch
                    {
                        //Ignore error, just means we don't know the MIME type for the Extension
                    }
                }

                //Add/Remove the association
                bool addAssociation = this.IsAssociationChecked(info.Extension);
                if (addAssociation)
                {
                    info.ProgID = RegistryProgramID;
                }
                else if (this._currentAssociations.Contains(info.Extension))
                {
                    //We want to remove the association to ourselves
                    info.ProgID = info.Extension.Substring(1) + "_auto_file";                    
                }

                //TODO: Ensure we are in the OpenWith List
            }

            this.Close();
        }

        private void btnSkip_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
