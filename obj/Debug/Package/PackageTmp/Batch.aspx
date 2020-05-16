<%@ Page Title="Routes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Batch.aspx.cs" Inherits="RouteNavigation._Batch" %>

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

    <asp:ListView ID="BatchListView" runat="server"
        DataKeyNames="id"
        OnPagePropertiesChanging="BatchListView_PagePropertiesChanging">

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
                            <b>Intake Locations Count</b>
                        </td>
                        <td id="td3" runat="server">
                            <b>Processed Locations Count</b>
                        </td>
                        <td id="td4" runat="server">
                            <b>Orphaned Locations Count</b>
                        </td>
                        <td id="td5" runat="server">
                            <b>Total Distance Miles</b>
                        </td>
                        <td id="td6" runat="server">
                            <b>Total Time</b>
                        </td>
                        <td id="td7" runat="server">
                            <b>Average Route Distance Miles</b>
                        </td>
                        <td id="td8" runat="server">
                            <b>Route Distance Std Dev</b>
                        </td>
                        <td id="td9" runat="server">
                            <b>Process Start Time</b>
                        </td>
                        <td id="td10" runat="server">
                            <b>Process End Time</b>
                        </td>
                        <td id="td11" runat="server">
                            <b>Calculation Time</b>
                        </td>
                        <td id="itemPlaceHolder" runat="server"></td>
                    </tr>
                    <tr>
                        <td colspan="14">
                            <asp:DataPager ID="batchDataPager" runat="server" PagedControlID="BatchListView" PageSize="10">
                                <Fields>
                                    <asp:NextPreviousPagerField ButtonType="Link" ShowFirstPageButton="true" ShowPreviousPageButton="true" ShowNextPageButton="false" />
                                    <asp:NumericPagerField ButtonType="Link" ButtonCount="10" />
                                    <asp:NextPreviousPagerField ButtonType="Link" ShowNextPageButton="true" ShowLastPageButton="true" ShowPreviousPageButton="false" />
                                </Fields>
                            </asp:DataPager>
                        </td>
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
                        <asp:Label ID="Label1" runat="server" Text='<%# Eval("locations_intake_count") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label2" runat="server" Text='<%# Eval("locations_processed_count") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label3" runat="server" Text='<%# Eval("locations_orphaned_count") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label4" runat="server" Text='<%# Eval("total_distance_miles") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label5" runat="server" Text='<%# Eval("total_time") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label6" runat="server" Text='<%# Eval("average_route_distance_miles") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label7" runat="server" Text='<%# Eval("route_distance_std_dev") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label8" runat="server" Text='<%# Eval("date_started") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label9" runat="server" Text='<%# Eval("date_completed") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label10" runat="server" Text='<%# Eval("calculation_time") %>'></asp:Label>
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
                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("locations_intake_count") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label2" runat="server" Text='<%# Eval("locations_processed_count") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label3" runat="server" Text='<%# Eval("locations_orphaned_count") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label4" runat="server" Text='<%# Eval("total_distance_miles") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label5" runat="server" Text='<%# Eval("total_time") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label6" runat="server" Text='<%# Eval("average_route_distance_miles") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label7" runat="server" Text='<%# Eval("route_distance_std_dev") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label8" runat="server" Text='<%# Eval("date_started") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label9" runat="server" Text='<%# Eval("date_completed") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label10" runat="server" Text='<%# Eval("calculation_time") %>'></asp:Label>
                </td>
            </tr>
        </SelectedItemTemplate>
    </asp:ListView>
</asp:Content>
