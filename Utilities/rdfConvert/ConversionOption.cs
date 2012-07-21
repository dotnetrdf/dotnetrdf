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
using VDS.RDF;
using VDS.RDF.Writing;

namespace VDS.RDF.Utilities.Convert
{
    /// <summary>
    /// Interface for Conversion Options
    /// </summary>
    public interface IConversionOption
    {
        /// <summary>
        /// Applies the option to a RDF Writer
        /// </summary>
        /// <param name="writer">Writer</param>
        void Apply(IRdfWriter writer);

        /// <summary>
        /// Applies the option to a Store Writer
        /// </summary>
        /// <param name="writer">Writer</param>
        void Apply(IStoreWriter writer);
    }

    public class HighSpeedOption : IConversionOption
    {
        private bool _hiSpeedAllowed;

        public HighSpeedOption(bool allowed)
        {
            this._hiSpeedAllowed = allowed;
        }

        public void Apply(IRdfWriter writer)
        {
            if (writer is IHighSpeedWriter)
            {
                ((IHighSpeedWriter)writer).HighSpeedModePermitted = this._hiSpeedAllowed;
            }
        }

        public void Apply(IStoreWriter writer)
        {
            if (writer is IHighSpeedWriter)
            {
                ((IHighSpeedWriter)writer).HighSpeedModePermitted = this._hiSpeedAllowed;
            }
        }
    }

    public class PrettyPrintingOption : IConversionOption
    {
        private bool _prettyPrint;

        public PrettyPrintingOption(bool prettyPrint)
        {
            this._prettyPrint = prettyPrint;
        }

        public void Apply(IRdfWriter writer)
        {
            if (writer is IPrettyPrintingWriter)
            {
                ((IPrettyPrintingWriter)writer).PrettyPrintMode = this._prettyPrint;
            }
        }

        public void Apply(IStoreWriter writer)
        {
            if (writer is IPrettyPrintingWriter)
            {
                ((IPrettyPrintingWriter)writer).PrettyPrintMode = this._prettyPrint;
            }
        }
    }

    public class CompressionLevelOption : IConversionOption
    {
        private int _compressionLevel = WriterCompressionLevel.Default;

        public CompressionLevelOption(int compressionLevel)
        {
            this._compressionLevel = compressionLevel;
        }

        public void Apply(IRdfWriter writer)
        {
            if (writer is ICompressingWriter)
            {
                ((ICompressingWriter)writer).CompressionLevel = this._compressionLevel;
            }
        }

        public void Apply(IStoreWriter writer)
        {
            if (writer is ICompressingWriter)
            {
                ((ICompressingWriter)writer).CompressionLevel = this._compressionLevel;
            }
        }
    }

    public class StylesheetOption : IConversionOption
    {
        private String _stylesheet;

        public StylesheetOption(String stylesheet)
        {
            this._stylesheet = stylesheet;
        }

        #region IConversionOption Members

        public void Apply(IRdfWriter writer)
        {
            if (writer is IHtmlWriter)
            {
                ((IHtmlWriter)writer).Stylesheet = this._stylesheet;
            }
        }

        public void Apply(IStoreWriter writer)
        {
            if (writer is IHtmlWriter)
            {
                ((IHtmlWriter)writer).Stylesheet = this._stylesheet;
            }
        }

        #endregion
    }
}
