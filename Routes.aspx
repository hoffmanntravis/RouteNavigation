<%@ Page Title="Routes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Routes.aspx.cs" Inherits="RouteNavigation._Routes" %>

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

    <div class="divHeader">
        <asp:Button CssClass="headerRowRight" ID="BtnExportCsv" runat="server" Text="Export data to .CSV" Style="float: right;" OnClick="BtnExportCsv_Click" />
        <asp:Button CssClass="headerRowLeft" ID="BtnCalculateRoutes" OnClick="BtnCalculateRoutes_Click"  Text="Recalculate Routes" ToolTip="Use this button to recalcualte routes based on latest location and vehicle data." runat="server" Style="float: left;" />
    </div>

    <asp:ListView ID="RoutesListView" runat="server"
        DataKeyNames="id"
        OnPagePropertiesChanging="RoutesListView_PagePropertiesChanging">

        <EmptyDataTemplate>
            <p>No Records Found..</p>
        </EmptyDataTemplate>
        <LayoutTemplate>
            <div class="tableWrapper">
                <table runat="server">
                    <tr runat="server">
                        <td id="td1" runat="server">
                            <b>id</b>
                        </td>
                        <td id="td2" runat="server">
                            <b>Duration</b>
                        </td>
                        <td id="td3" runat="server">
                            <b>Date</b>
                        </td>
                        <td id="td4" runat="server">
                            <b>Average Location Distance (miles)</b>
                        </td>
                        <td id="td5" runat="server">
                            <b>Total Route Length (miles)</b>
                        </td>
                        <td id="td6" runat="server">
                            <b>Vehicle Id</b>
                        </td>
                        <td id="td7" runat="server">
                            <b>Vehicle Name</b>
                        </td>
                        <td id="td8" runat="server">
                            <b>Vehicle Model</b>
                        </td>
                        <td id="td9" runat="server">
                            <b>Maps Url</b>
                        </td>
                        <td id="td10" runat="server">
                            <b>Route Details</b>
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
                        <asp:Label ID="lblId" runat="server" Text='<%# Eval("id") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="Label1" runat="server" Text='<%# Eval("total_time") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label2" runat="server" Text='<%# Eval("route_date") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label3" runat="server" Text='<%# Eval("average_location_distance_miles") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label4" runat="server" Text='<%# Eval("distance_miles") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label5" runat="server" Text='<%# Eval("vehicle_id") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label6" runat="server" Text='<%# Eval("vehicle_name") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label7" runat="server" Text='<%# Eval("vehicle_model") %>'></asp:Label>
                    </td>
                    
                    <td>
                        <a href="/map?routeId=<%# Eval("id") %>">Map
                    </td>
                    <td>
                        <a href="/RouteDetails?routeId=<%# Eval("id") %>">Route Details
                    </td>
                </tr>
            </div>
        </ItemTemplate>
        <SelectedItemTemplate>
            <tr id="Tr3" runat="server">
                <td>
                    <asp:Label ID="lblId" runat="server" Text='<%# Eval("id") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("total_time") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label2" runat="server" Text='<%# Eval("route_date") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label3" runat="server" Text='<%# Eval("average_location_distance_miles") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label4" runat="server" Text='<%# Eval("distance_miles") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label5" runat="server" Text='<%# Eval("vehicle_id") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label6" runat="server" Text='<%# Eval("vehicle_name") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label7" runat="server" Text='<%# Eval("vehicle_model") %>'></asp:Label>
                </td>
                <td>
                    <a href="<%# Eval("maps_url") %>">Google Maps
                </td>
                <td>
                    <a href="/RouteDetails?routeId=<%# Eval("id") %>">Route Details
                </td>
            </tr>
        </SelectedItemTemplate>
    </asp:ListView>
</asp:Content>
