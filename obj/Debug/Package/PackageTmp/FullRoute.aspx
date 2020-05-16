<%@ Page Title="Full Route" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="FullRoute.aspx.cs" Inherits="RouteNavigation._FullRoute" %>

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

    <asp:UpdateProgress ID="loadingPanel" runat="server" DynamicLayout="false">
        <ProgressTemplate>
            <img src="images/spinner.gif" width="40" height="40" alt="Loading" />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="panelFullRoute" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:ListView ID="RouteDetailsListView" runat="server"
                DataKeyNames="route_id"
                OnPagePropertiesChanging="RouteDetailsListView_PagePropertiesChanging"
                OnItemDataBound="RouteDetailsListView_ItemDataBound"
                OnItemEditing="RouteDetailsListView_RowEditing"
                OnItemCanceling="RouteDetailsListView_RowCancelingEdit"
                OnItemUpdating="RouteDetailsListView_RowUpdating"
                OnItemDeleting="RouteDetailsListView_RowDeleting">


                <EmptyDataTemplate>
                    <p>No Records Found..</p>
                </EmptyDataTemplate>
                <LayoutTemplate>
                    <div class="tableWrapper">
                        <table runat="server">
                            <tr runat="server">
                                <td id="td1" runat="server">
                                    <b>Loc Id</b>
                                </td>
                                <td id="td2" runat="server">
                                    <b>Route Id</b>
                                </td>
                                <td id="td3" runat="server">
                                    <b>Name</b>
                                </td>
                                <td id="td4" runat="server">
                                    <b>Priority</b>
                                </td>
                                <td id="td5" runat="server">
                                    <b>Address</b>
                                </td>
                                <td id="td6" runat="server">
                                    <b>Next Oil Visit</b>
                                </td>
                                <td id="td7" runat="server">
                                    <b>Next Grease Visit</b>
                                </td>
                                <td id="td8" runat="server">
                                    <b>Intended Pickup Date</b>
                                </td>
                                <td id="td9" runat="server">
                                    <b>Oil Days Until Due</b>
                                </td>
                                <td id="td10" runat="server">
                                    <b>G.T. Days Until Due</b>
                                </td>
                                <td id="td11" runat="server">
                                    <b>Route Date</b>
                                </td>
                                <td id="td12" runat="server">
                                    <b>Visit Reason</b>
                                </td>
                                <td id="td13" runat="server">
                                    <b>Order</b>
                                </td>
                                <td id="tdAction" runat="server">
                                    <b>Action</b>
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
                                <asp:Label ID="lblLocationId" runat="server" Text='<%# Eval("location_id") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:HyperLink ID="urlRouteId" runat="server" Text='<%# Eval("route_id") %>' NavigateUrl='<%# Eval("route_id","/RouteDetails?routeId={0}") %>' />
                            </td>
                            <td>
                                <asp:Label ID="lblaccount" runat="server" Text='<%# Eval("account") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblClientPriority" runat="server" Text='<%# Eval("client_priority") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblAddress" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblOilPickupNextDate" runat="server" Text='<%# Eval("oil_pickup_next_date") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblGreaseTrapPickupNextDate" runat="server" Text='<%# Eval("grease_trap_pickup_next_date") %>'></asp:Label>
                            </td>

                            <td>
                                <asp:Label ID="lblIntendedPickupDate" runat="server" Text='<%# Eval("intended_pickup_date") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblOilPickupDaysUntilDue" runat="server" Text='<%# Eval("oil_pickup_days_until_due") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblGreaseTrapDaysUntilDue" runat="server" Text='<%# Eval("grease_trap_days_until_due") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblRoutedate" runat="server" Text='<%# Eval("route_date") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="Label1" runat="server" Text='<%# Eval("reason_for_visit") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblOrder" runat="server" Text='<%# Eval("insert_order") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:LinkButton ID="btnEdit" Text="Edit" runat="server" CommandName="Edit" />
                                <asp:LinkButton ID="btnDelete" Text="Delete" runat="server" OnClientClick="return disp_confirm();" CommandName="Delete" />
                            </td>
                        </tr>
                    </div>
                </ItemTemplate>
                <EditItemTemplate>
                    <div class="tableWrapper">
                        <tr id="Tr1" runat="server">
                            <td>
                                <asp:Label ID="lblLocationId" runat="server" Text='<%# Eval("location_id") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtRouteId" runat="server" Text='<%# Eval("route_id") %>'></asp:TextBox>
                            </td>
                            <td>
                                <asp:Label ID="lblaccount" runat="server" Text='<%# Eval("account") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblClientPriority" runat="server" Text='<%# Eval("client_priority") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblAddress" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblOilPickupNextDate" runat="server" Text='<%# Eval("oil_pickup_next_date") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblGreaseTrapPickupNextDate" runat="server" Text='<%# Eval("grease_trap_pickup_next_date") %>'></asp:Label>
                            </td>

                            <td>
                                <asp:Label ID="lblIntendedPickupDate" runat="server" Text='<%# Eval("intended_pickup_date") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblOilPickupDaysUntilDue" runat="server" Text='<%# Eval("oil_pickup_days_until_due") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblGreaseTrapDaysUntilDue" runat="server" Text='<%# Eval("grease_trap_days_until_due") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblRoutedate" runat="server" Text='<%# Eval("route_date") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="Label1" runat="server" Text='<%# Eval("reason_for_visit") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="lblOrder" runat="server" Text='<%# Eval("insert_order") %>'></asp:TextBox>
                            </td>
                            <td>
                                <asp:LinkButton ID="btnCancel" Text="Cancel" runat="server" CommandName="Cancel" />
                                <asp:LinkButton ID="btnUpdate" Text="Update" runat="server" CommandName="Update" />
                            </td>
                        </tr>
                    </div>
                </EditItemTemplate>
                <SelectedItemTemplate>
                    <div class="tableWrapper">
                        <tr id="Tr1" runat="server">
                            <td>
                                <asp:Label ID="lblLocationId" runat="server" Text='<%# Eval("location_id") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:HyperLink ID="urlRouteId" runat="server" Text='<%# Eval("route_id") %>'></asp:HyperLink>
                            </td>
                            <td>
                                <asp:Label ID="lblaccount" runat="server" Text='<%# Eval("account") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblClientPriority" runat="server" Text='<%# Eval("client_priority") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblAddress" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblOilPickupNextDate" runat="server" Text='<%# Eval("oil_pickup_next_date") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblGreaseTrapPickupNextDate" runat="server" Text='<%# Eval("grease_trap_pickup_next_date") %>'></asp:Label>
                            </td>

                            <td>
                                <asp:Label ID="lblIntendedPickupDate" runat="server" Text='<%# Eval("intended_pickup_date") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblOilPickupDaysUntilDue" runat="server" Text='<%# Eval("oil_pickup_days_until_due") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblGreaseTrapDaysUntilDue" runat="server" Text='<%# Eval("grease_trap_days_until_due") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblRoutedate" runat="server" Text='<%# Eval("route_date") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="Label1" runat="server" Text='<%# Eval("reason_for_visit") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblOrder" runat="server" Text='<%# Eval("insert_order") %>'></asp:Label>
                            </td>
                            <td>
                                <asp:LinkButton ID="btnEdit2" Text="Edit" runat="server" CommandName="Edit" />
                            </td>
                        </tr>
                    </div>
                </SelectedItemTemplate>

            </asp:ListView>
        </ContentTemplate>
    </asp:UpdatePanel>

    <script>
        function disp_confirm() {
            var r = confirm("This will delete the record from this route, but not the locations table itself.  Are you sure?");
            if (r == true) {
            }
            else {
                return false;
            }

        }
    </script>

</asp:Content>


