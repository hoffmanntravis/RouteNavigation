﻿<%@ Page Title="Location" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Location.aspx.cs" Inherits="RouteNavigation._Locations" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link rel="StyleSheet" href="/content/table.css" type="text/css">
    <link rel="StyleSheet" href="/content/Site.css" type="text/css">
    <asp:Panel ID="locationErrorsPanel" runat="server" CssClass="ErrorSummary">
        <asp:CustomValidator ID="dataValidation" runat="server"
            Display="None" EnableClientScript="False"></asp:CustomValidator>
        <asp:ValidationSummary ID="ErrorSummary" runat="server"
            HeaderText="Errors occurred:"
            BackColor="#fe6363" CellPadding="5"></asp:ValidationSummary>
    </asp:Panel>

    <div class="divHeader">
        <asp:Button CssClass="headerRowRight" ID="btnExportCsv" runat="server" Text="Export data to .CSV" Style="float: right;" OnClick="BtnExportCsv_Click" />
        <asp:Button CssClass="headerRowRight" ID="btnImportCsv" runat="server" Text="Upload .CSV data" Style="float: right;" OnClientClick="return disp_confirm_severe();" OnClick="BtnImportCsv_Click" />

        <asp:FileUpload CssClass="headerRowRight" ID="fileUpload" runat="server" Style="float: right;" />
        <asp:Panel runat="server" DefaultButton="BtnSearch">
            <asp:DropDownList CssClass="headerRowLeft" ID="lstSearchFilters" runat="server" Style="float: left;" />
            <asp:TextBox CssClass="headerRowLeft" defaultButton="btnSearch" ID="TxtSearchFilter" placeholder="Filter: Search string" runat="server" Style="float: left;" />
            <asp:Button CssClass="headerRowLeft" ID="btnSearch" OnClick="FilterLocationsListView_Click" Text="Search" runat="server" Style="float: left;" />
        </asp:Panel>
        <asp:Button CssClass="headerRowLeft" ID="btnRefreshApiCache" OnClick="RefreshApiCache_Click" Text="Refresh API Cache" ToolTip="Use this to refresh app cache with latest google location data." runat="server" Style="float: left;" />
    </div>

    <asp:ListView ID="LocationsListView" runat="server"
        DataKeyNames="id"
        OnPagePropertiesChanging="LocationsListView_PagePropertiesChanging"
        OnItemEditing="LocationsListView_RowEditing"
        OnItemCanceling="LocationsListView_RowCancelingEdit"
        OnItemUpdating="LocationsListView_RowUpdating"
        OnItemDeleting="LocationsListView_RowDeleting"
        OnItemInserting="LocationsListView_RowInsert"
        InsertItemPosition="LastItem">

        <EmptyDataTemplate>
            <p>No Records Found..</p>
        </EmptyDataTemplate>
        <LayoutTemplate>
            <div class="tableWrapper">
                <table runat="server">
                    <tr runat="server">
                        <td id="td1" runat="server">
                            <b>id</b>
                            <asp:ImageButton ID="imgSortLocationId" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByLocationId_Click" />
                        </td>
                        <td id="tdTrackingNumber" runat="server">
                            <b>Track #</b>
                            <asp:ImageButton ID="imgTrackingNumber" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByTrackingNumber_Click" />
                        </td>
                        <td id="tdaccount" width="400" runat="server">
                            <b>Acct</b>
                            <asp:ImageButton ID="imgSortAccount" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByAccount_Click" />
                        </td>
                        <td id="tdDistanceFromDepot" onclick="tdDistanceFromDepot_click" runat="server">
                            <b>Distance To Depot</b>
                            <asp:ImageButton ID="imgSortDistanceFromDepot" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByDistanceFromDepot_Click" />
                        </td>
                        <td id="tdAddress" width="400" runat="server">
                            <b>Addr</b>
                            <asp:ImageButton ID="imgSortAddress" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByAddress_Click" />
                        </td>
                        <td id="tdOilPickupSchedule" width="60" runat="server">
                            <b>Oil Schdl</b>
                            <asp:ImageButton ID="imgSortPickupInterval" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByOilPickupSchedule_Click" />
                        </td>
                        <td id="tdGreaseTrapSchedule" width="160" runat="server">
                            <b>Grease Schdl</b>
                            <asp:ImageButton ID="imgSortGreaseTrapSchedule" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByGreaseTrapSchedule_Click" />
                        </td>
                        <td id="tdgreaseTrapPreferredTimeStart" width="90" runat="server">
                            <b>Pickup Start</b>
                            <asp:ImageButton ID="imgSortGreaseTrapPreferredTimeStart" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByGreaseTrapPreferredTimeStart_Click" />
                        </td>
                        <td id="tdgreaseTrapPreferredTimeEnd" width="80" runat="server">
                            <b>Pickup End</b>
                            <asp:ImageButton ID="imgSortGreaseTrapPreferredTimeEnd" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByGreaseTrapPreferredTimeEnd_Click" />
                        </td>

                        <td id="tdCapacity" runat="server">
                            <b>Oil Size</b>
                            <asp:ImageButton ID="imgSortOilPickupSize" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByOilPickupCustomer_Click" />
                        </td>
                        <td id="td2" runat="server">
                            <b>G.T. Size</b>
                            <asp:ImageButton ID="imgSortGreaseTrapSize" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByGreaseTrapSize_Click" />
                        </td>
                        <td id="tdVehicleSize" runat="server">
                            <b>Vehicle Size</b>
                            <asp:ImageButton ID="imgSortVehicleSize" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByVehicleSize_Click" />
                        </td>
                        <td id="tdOil" runat="server">
                            <b>Oil</b>
                            <asp:ImageButton ID="imgSortType" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByOilPickupCustomer_Click" />
                        </td>
                        <td id="tdGrease" runat="server">
                            <b>Grease</b>
                            <asp:ImageButton ID="ImageButton1" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByGreaseTrapCustomer_Click" />
                        </td>
                        <td id="tdOilPickupDaysUntilDue" runat="server">
                            <b>Oil Days til Due</b>
                            <asp:ImageButton ID="imgSortOilPickupDaysUntilDue" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByOilPickupDaysUntilDue_Click" />
                        </td>
                        <td id="tdGreaseTrapDaysUntilDue" runat="server">
                            <b>G.T. Days til Due</b>
                            <asp:ImageButton ID="imgSortGreaseTrapDaysUntilDue" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByGreaseTrapDaysUntilDue_Click" />
                        </td>
                        <td id="tdAction" width="80" runat="server">
                            <b>Action</b>
                        </td>
                        <td id="itemPlaceHolder" runat="server"></td>
                    </tr>
                    <tr>
                        <td colspan="14">
                            <asp:DataPager ID="locationDataPager" runat="server" PagedControlID="LocationsListView" PageSize="9">
                                <Fields>
                                    <asp:NextPreviousPagerField ButtonType="Link" ShowFirstPageButton="true" ShowPreviousPageButton="true" ShowNextPageButton="false" />
                                    <asp:NumericPagerField ButtonType="Link" ButtonCount="10" />
                                    <asp:NextPreviousPagerField ButtonType="Link" ShowNextPageButton="true" ShowLastPageButton="true" ShowPreviousPageButton="false" />
                                </Fields>
                            </asp:DataPager>
                        </td>
                        <td></td>
                        <td></td>
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
                        <asp:Label ID="Label11" runat="server" Text='<%# Eval("tracking_number") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label1" runat="server" Text='<%# Eval("account") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label2" runat="server" Text='<%# Eval("distance_from_source") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label3" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label4" runat="server" Text='<%# Eval("oil_pickup_schedule") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label7" runat="server" Text='<%# Eval("grease_trap_schedule") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label5" runat="server" Text='<%# Eval("grease_trap_preferred_time_start") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label6" runat="server" Text='<%# Eval("grease_trap_preferred_time_end") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label8" runat="server" Text='<%# Eval("oil_tank_size") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label9" runat="server" Text='<%# Eval("grease_trap_size") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label10" runat="server" Text='<%# Eval("vehicle_size") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:CheckBox ID="chkOilPickupCustomer" runat="server" Checked='<%# Convert.ToBoolean(Eval("oil_pickup_customer")) %>' Enabled="false" />
                    </td>
                    <td>
                        <asp:CheckBox ID="chkGreaseTrapCustomer" runat="server" Checked='<%# Convert.ToBoolean(Eval("grease_trap_customer")) %>' Enabled="false" />
                    </td>
                    <td>
                        <asp:Label ID="label13" runat="server" Text='<%# Eval("oil_pickup_days_until_due") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label14" runat="server" Text='<%# Eval("grease_trap_days_until_due") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:LinkButton ID="btnEdit" Text="Edit" runat="server" CommandName="Edit" />
                        <asp:LinkButton ID="btnDelete" Text="Delete" runat="server" OnClientClick="return disp_confirm();" CommandName="Delete" />
                    </td>
                </tr>
            </div>
        </ItemTemplate>
        <SelectedItemTemplate>
            <tr id="Tr1" runat="server">
                <td>
                    <asp:Label ID="lblSelectedId" runat="server" Text='<%# Eval("id") %>'></asp:Label>
                </td>
                                                      <td>
                        <asp:Label ID="Label11" runat="server" Text='<%# Eval("tracking_number") %>'></asp:Label>
                    </td>
                <td>
                    <asp:Label ID="label1" runat="server" Text='<%# Eval("account") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label2" runat="server" Text='<%# Eval("distance_from_source") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label3" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label4" runat="server" Text='<%# Eval("oil_pickup_schedule") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label7" runat="server" Text='<%# Eval("grease_trap_schedule") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label5" runat="server" Text='<%# Eval("grease_trap_preferred_time_start") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label6" runat="server" Text='<%# Eval("grease_trap_preferred_time_end") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label8" runat="server" Text='<%# Bind("oil_tank_size") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label9" runat="server" Text='<%# Eval("grease_trap_size") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label10" runat="server" Text='<%# Bind("vehicle_size") %>'></asp:Label>
                </td>
                <td>
                    <asp:CheckBox ID="chkOilPickupCustomer" runat="server" Checked='<%# Convert.ToBoolean(Eval("oil_pickup_customer")) %>' />
                </td>
                <td>
                    <asp:CheckBox ID="chkGreaseTrapCustomer" runat="server" Checked='<%# Convert.ToBoolean(Eval("grease_trap_customer")) %>' />
                </td>
                <td>
                    <asp:Label ID="label13" runat="server" Text='<%# Eval("oil_pickup_days_until_due") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label14" runat="server" Text='<%# Eval("grease_trap_days_until_due") %>'></asp:Label>
                </td>
                <td>
                    <asp:LinkButton ID="btnEdit2" Text="Edit" runat="server" CommandName="Edit" />
                </td>

            </tr>
        </SelectedItemTemplate>
        <EditItemTemplate>
            <tr id="Tr1" runat="server">
                <td>
                    <asp:Label ID="lblId" runat="server" Text='<%# Eval("id") %>'></asp:Label>
                </td>
                                                      <td>
                        <asp:Label ID="Label11" runat="server" Text='<%# Eval("tracking_number") %>'></asp:Label>
                    </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditClientName" runat="server" Text='<%# Bind("account") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:Label class="tableInput" ID="lblDistanceFromDepot" runat="server" Text='<%# Bind("distance_from_source") %>'></asp:Label>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditAddress" runat="server" Text='<%# Bind("address") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditOilPickupSchedule" runat="server" Text='<%# Bind("oil_pickup_schedule") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditGreaseTrapSchedule" runat="server" Text='<%# Bind("grease_trap_schedule") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditGreaseTrapPreferredTimeStart" runat="server" Text='<%# Bind("grease_trap_preferred_time_start") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditGreaseTrapPreferredTimeEnd" runat="server" Text='<%# Bind("grease_trap_preferred_time_end") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditOilTankSize" runat="server" Text='<%# Bind("oil_tank_size") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditGreaseTrapSize" runat="server" Text='<%# Eval("grease_trap_size") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditVehicleSize" runat="server" Text='<%# Bind("vehicle_size") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:CheckBox class="tableInput" ID="chkOilPickupCustomer" runat="server" Checked='<%# Convert.ToBoolean(Eval("oil_pickup_customer")) %>' />
                </td>
                <td>
                    <asp:CheckBox class="tableInput" ID="chkGreaseTrapCustomer" runat="server" Checked='<%# Convert.ToBoolean(Eval("grease_trap_customer")) %>' />
                </td>
                <td>
                    <asp:Label ID="lblOilPickupDaysUntilDue" runat="server" Text='<%# Eval("oil_pickup_days_until_due") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblGreaseTrapUntilDue" runat="server" Text='<%# Eval("grease_trap_days_until_due") %>'></asp:Label>
                </td>
                <td>
                    <asp:LinkButton ID="btnCancel" Text="Cancel" runat="server" CommandName="Cancel" />
                    <asp:LinkButton ID="btnUpdate" Text="Update" runat="server" CommandName="Update" />
                </td>
            </tr>
        </EditItemTemplate>
        <InsertItemTemplate>
            <tr id="Tr1" runat="server">
                <td></td>
                <td></td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertClientName" placeholder="Sample Location Name" runat="server" Text='<%# Bind("account") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td></td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertAddress" placeholder="1000 Awesome Drive" runat="server" Text='<%# Bind("address") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertoilPickupSchedule" placeholder="30" runat="server" Text='<%# Bind("oil_pickup_schedule") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertLastVisited" placeholder="01/01/2018" runat="server" Text='<%# Bind("grease_trap_schedule") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtGreaseTrapPreferredTimeStart" placeholder="06:00" runat="server" Text='<%# Bind("grease_trap_preferred_time_start") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtGreaseTrapPreferredTimeEnd" placeholder="16:00" runat="server" Text='<%# Bind("grease_trap_preferred_time_end") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertoilTankSize" placeholder="500" runat="server" Text='<%# Bind("oil_tank_size")%>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertGreaseTrapSize" placeholder="500" runat="server" Text='<%# Bind("grease_trap_size")%>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertVehicleSize" placeholder="1-10" runat="server" Text='<%# Bind("vehicle_size") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:CheckBox ID="chkOilPickupCustomer" runat="server" Checked='<%# Convert.ToBoolean(Eval("oil_pickup_customer")) %>' />
                </td>
                <td>
                    <asp:CheckBox ID="chkGreaseTrapCustomer" runat="server" Checked='<%# Convert.ToBoolean(Eval("grease_trap_customer")) %>' />
                </td>

                <td>
                    <asp:Label ID="lblOilPickupDaysUntilDue" runat="server" Text='<%# Eval("oil_pickup_days_until_due") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblGreaseTrapUntilDue" runat="server" Text='<%# Eval("grease_trap_days_until_due") %>'></asp:Label>
                </td>

                <td>
                    <asp:LinkButton ID="btnInsert" Text="Insert" runat="server" CommandName="Insert" />
                </td>
            </tr>
        </InsertItemTemplate>

    </asp:ListView>

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

    <script>
        function disp_confirm_severe() {
            var r = confirm("Warning! this will overwrite all locations in the database.  Are you sure?");
            if (r == true) {
            }
            else {
                return false;
            }

        }
    </script>

</asp:Content>
