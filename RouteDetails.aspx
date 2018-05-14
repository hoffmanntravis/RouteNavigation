﻿<%@ Page Title="Route Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="RouteDetails.aspx.cs" Inherits="RouteNavigation._RouteDetails" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link rel="StyleSheet" href="/content/table.css" type="text/css">
    <link rel="StyleSheet" href="/content/Site.css" type="text/css">
    <asp:Panel ID="ErrorsPanel" runat="server" CssClass="ErrorSummary">
        <asp:CustomValidator ID="routeValidation" runat="server"
            Display="None" EnableClientScript="False"></asp:CustomValidator>
        <asp:ValidationSummary ID="routeErrorSummary" runat="server"
            HeaderText="Errors occurred:"
            BackColor="#fe6363" CellPadding="5"></asp:ValidationSummary>
    </asp:Panel>

    <asp:ListView ID="RouteDetailsListView" runat="server"
        DataKeyNames="route_id"
        OnPagePropertiesChanging="RoutesListView_PagePropertiesChanging">

        <EmptyDataTemplate>
            <p>No Records Found..</p>
        </EmptyDataTemplate>
        <LayoutTemplate>
            <div class="tableWrapper">
                <table runat="server">
                    <tr runat="server">
                        <td id="td1" runat="server">
                            <b>Loc id</b>
                        </td>
                        <td id="td2" runat="server">
                            <b>Name</b>
                        </td>
                        <td id="td3" runat="server">
                            <b>Priority</b>
                        </td>
                        <td id="td4" runat="server">
                            <b>Address</b>
                        </td>
                        <td id="td5" runat="server">
                            <b>Last Visited</b>
                        </td>
                        <td id="td6" runat="server">
                            <b>Days Until Due</b>
                        </td>
                        <td id="td7" runat="server">
                            <b>Matrix Weight</b>
                        </td>
                        <td id="td8" runat="server">
                            <b>Route Date</b>
                        </td>
                        <td id="itemPlaceHolder" runat="server"></td>
                    </tr>
                </table>
            </div>
        </LayoutTemplate>
        <ItemTemplate>
            <div class="tableWrapper">
                <tr id="Tr1" runat="server">
                    <td>
                        <asp:Label ID="lblId" runat="server" Text='<%# Eval("location_id") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="Label1" runat="server" Text='<%# Eval("location_name") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label2" runat="server" Text='<%# Eval("client_priority") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label3" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label4" runat="server" Text='<%# Eval("last_visited") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label5" runat="server" Text='<%# Eval("days_until_due") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label6" runat="server" Text='<%# Eval("matrix_weight") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label7" runat="server" Text='<%# Eval("route_date") %>'></asp:Label>
                    </td>
                </tr>
            </div>
        </ItemTemplate>
        <SelectedItemTemplate>
            <div class="tableWrapper">
                <tr id="Tr1" runat="server">
                    <td>
                        <asp:Label ID="lblId" runat="server" Text='<%# Eval("location_id") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="Label1" runat="server" Text='<%# Eval("location_name") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label2" runat="server" Text='<%# Eval("client_priority") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label3" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label4" runat="server" Text='<%# Eval("last_visited") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label5" runat="server" Text='<%# Eval("days_until_due") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label6" runat="server" Text='<%# Eval("matrix_weight") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label7" runat="server" Text='<%# Eval("route_date") %>'></asp:Label>
                    </td>
                </tr>
            </div>
        </SelectedItemTemplate>
    </asp:ListView>
</asp:Content>
