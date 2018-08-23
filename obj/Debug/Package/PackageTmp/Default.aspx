<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Default.aspx.cs" Inherits="RouteNavigation.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
</asp:Content>
<script runat="server">
  protected override void OnLoad(EventArgs e)
  {
      Response.Redirect("Routes.aspx",false);
  }
</script>