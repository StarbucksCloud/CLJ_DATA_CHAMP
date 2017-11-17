<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" AutoEventWireup="true" CodeBehind="Categories.aspx.cs" Inherits="CLJ_HACK.Categories" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="contentBody" runat="server">
    <br /><br /><br />

    <pre style="width:600px;white-space:pre-wrap;">
        </pre>
    <asp:CheckBox ID="chkInventory"  Text="Inventory" runat="server"  AutoPostBack="True"  OnCheckedChanged="ShowData" /> <br />
    <asp:CheckBox ID="chkPartner"  Text="Partners" runat="server"   AutoPostBack="True"  OnCheckedChanged="ShowData" /><br />
    <asp:CheckBox ID="chkSales"  Text="Sales" runat="server"   AutoPostBack="True"  OnCheckedChanged="ShowData" /><br />
    <asp:CheckBox ID="chkLabor"  Text="Labor" runat="server"  AutoPostBack="True"  OnCheckedChanged="ShowData"  /><br />

    <br /><br />

    


    <asp:GridView ID="grdData" runat="server" ForeColor="#333333" 
            Width="856px" ItemStyle-Width="150" AutoGenerateColumns = "true" >
<%--              <Columns>    
                 <asp:BoundField DataField="PartitionKey" HeaderText="PartitionKey" ItemStyle-Width="150" />    
                 <asp:BoundField DataField="RowKey" HeaderText="RowKey" ItemStyle-Width="150" />    
                 <asp:BoundField DataField="DataEntity" HeaderText="DataEntity" ItemStyle-Width="150" />    
             </Columns> --%>




    </asp:GridView>




    <br /><br />
    <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="POST_RESTRequest"  />
</asp:Content>
