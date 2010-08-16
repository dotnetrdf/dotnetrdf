<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SPARQLEndpointDemo._Default" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>SPARQL Endpoint Demo</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h3>SPARQL Demo Interface</h3>
        <asp:TextBox ID="txtQuery" runat="server" Rows="10" Columns="100" TextMode="MultiLine">PREFIX rdf: &lt;http://www.w3.org/1999/02/22-rdf-syntax-ns#&gt;
PREFIX rdfs: &lt;http://www.w3.org/2000/01/rdf-schema#&gt;
PREFIX xsd: &lt;http://www.w3.org/2001/XMLSchema#&gt;
PREFIX aat: &lt;http://www.dotnetrdf.org/AllAboutThat/&gt; 
PREFIX ex: &lt;http://example.org/vehicles/&gt;  
SELECT * {?s ?p ?o}
        </asp:TextBox>
        <br />
        Default Graph URI: <asp:TextBox ID="txtDefaultGraph" Text="http://www.dotnetrdf.org/Tests/SQLStore/" Columns="100" runat="server" />
        <br />
        Timeout: <asp:TextBox ID="txtTimeout" Text="5000" runat="server" /> Milliseconds
        <br />
        <asp:CheckBox ID="chkPartialResults" Checked="true" runat="server" Text="Partial Results on Timeout?" />
        <br />
        Store:
        <asp:RadioButtonList ID="radStores" runat="server">
            <asp:ListItem Selected="True" Text="Vehicles" Value="~/sparql/" />
            <asp:ListItem Text="AAT Development" Value="~/sparql2/" />
        </asp:RadioButtonList>
        <br />
        <asp:Button ID="btnQuery" Text="Make Query" runat="server" 
            onclick="btnQuery_Click" />
    </div>
    </form>
</body>
</html>
