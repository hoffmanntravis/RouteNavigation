<%@ Page Title="Location" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Location.aspx.cs" Inherits="RouteNavigation._Locations" %>


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
        OnItemDataBound="LocationstListView_ItemDataBound"
        OnPagePropertiesChanging="LocationsListView_PagePropertiesChanging"
        OnItemEditing="LocationsListView_RowEditing"
        OnItemCanceling="LocationsListView_RowCancelingEdit"
        OnItemUpdating="LocationsListView_RowUpdating"
        OnItemDeleting="LocationsListView_RowDeleting"
        OnItemInserting="LocationsListView_RowInsert"
        OnItemCreated="LocationsListView_ItemCreated"

        InsertItemPosition="LastItem">

        <EmptyDataTemplate>
            <p>No Records Found..</p>
        </EmptyDataTemplate>
        <LayoutTemplate>
            <div class="tableWrapper">
                <table runat="server">
                    <tr runat="server">
                        <td id="td1" runat="server">
                            <asp:ImageButton ID="imgLocationIdSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByLocationId_Click" />
                            <b>id</b>
                        </td>
                        <td id="tdLocationName" runat="server">
                            <b>Location Name</b>
                            <asp:ImageButton ID="imgLocationNameSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByLocationName_Click" />
                        </td>
                        <td id="tdClientPriority" onclick="tdClientPriority_click" runat="server">
                            <asp:ImageButton ID="imgClientPrioritySort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByClientPriority_Click" />
                            <b>Client Priority</b>
                        </td>
                        <td id="tdAddress" runat="server">
                            <asp:ImageButton ID="imgAddressSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByAddress_Click" />
                            <b>Address</b>
                        </td>
                        <td id="tdPickupIntervalDays" runat="server">
                            <asp:ImageButton ID="imgPickupIntervalSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByPickupInterval_Click" />
                            <b>Pickup Interval Days</b>
                        </td>
                        <td id="tdPickupWindowStartTime" runat="server">
                            <asp:ImageButton ID="imgPickupWindowStartTimeSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByPickupWindowStartTime_Click" />
                            <b>Pickup Time Start</b>
                        </td>
                        <td id="tdPickupWindowEndTime" runat="server">
                            <asp:ImageButton ID="imgPickupWindowEndTimeSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByPickupWindowEndTime_Click" />
                            <b>Pickup Time End</b>
                        </td>
                        <td id="tdLastVisited" runat="server">
                            <asp:ImageButton ID="imgLastVisitedSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByLastVisited_Click" />
                            <b>Last Visited</b>
                        </td>
                        <td id="tdCapacity" runat="server">
                            <asp:ImageButton ID="imgCapacitySort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByCapacity_Click" />
                            <b>Capacity (glns)</b>
                        </td>
                        <td id="tdVehicleSize" runat="server">
                            <asp:ImageButton ID="imgVehicleSizeSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByVehicleSize_Click" />
                            <b>Vehicle Size</b>
                        </td>
                        <td id="tdContactName" width="100" runat="server">
                            <asp:ImageButton ID="imgContactNameSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByContactName_Click" />
                            <b>Contact Name</b>
                        </td>
                        <td id="tdContactEmail" width="100" runat="server">
                            <asp:ImageButton ID="imgContactEmailSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByContactEmail_Click" />
                            <b>Contact Email</b>
                        </td>
                        <td id="td2" width="100" runat="server">
                            <asp:ImageButton ID="imgType" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByType_Click" />
                            <b>type</b>
                        </td>

                        <td id="tdDaysUntilDue" width="100" runat="server">
                            <asp:ImageButton ID="imgDaysUntilDueSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByDaysUntilDue_Click" />
                            <b>Days Until Due</b>
                        </td>
                        <td id="tdMatrixWeight" width="100" runat="server">
                            <asp:ImageButton ID="imgMatrixWeightSort" ImageUrl="~/images/up_arrow.svg" Height="10px" runat="server" OnClick="SortByMatrixWeight_Click" />
                            <b>Matrix Weight</b>
                        </td>
                        <td id="tdAction" width="100" runat="server">
                            <b>Action</b>
                        </td>
                        <td id="itemPlaceHolder" runat="server"></td>
                    </tr>
                    <tr>
                        <td colspan="14">
                            <asp:DataPager ID="locationDataPager" runat="server" PagedControlID="LocationsListView" PageSize="10">
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
                        <asp:Label ID="label1" runat="server" Text='<%# Eval("location_name") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label2" runat="server" Text='<%# Eval("client_priority") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label3" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label4" runat="server" Text='<%# Eval("pickup_interval_days") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label5" runat="server" Text='<%# Eval("pickup_window_start_time") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label6" runat="server" Text='<%# Eval("pickup_window_end_time") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label7" runat="server" Text='<%# Eval("last_visited") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label8" runat="server" Text='<%# Eval("capacity_gallons") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label9" runat="server" Text='<%# Eval("vehicle_size") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label10" runat="server" Text='<%# Eval("contact_name") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="labe11" runat="server" Text='<%# Eval("contact_email") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="labe12" runat="server" Text='<%# Eval("type_text") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="labe13" runat="server" Text='<%# Eval("days_until_due") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="labe14" runat="server" Text='<%# Eval("matrix_weight") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:LinkButton ID="btnEdit" Text="Edit" runat="server" CommandName="Edit" />
                        <asp:LinkButton ID="Button1" Text="Delete" runat="server" OnClientClick="return disp_confirm();" CommandName="Delete" />
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
                    <asp:Label ID="label1" runat="server" Text='<%# Eval("location_name") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label2" runat="server" Text='<%# Eval("client_priority") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label3" runat="server" Text='<%# Eval("address") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label4" runat="server" Text='<%# Eval("pickup_interval_days") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label5" runat="server" Text='<%# Eval("pickup_window_start_time") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label6" runat="server" Text='<%# Eval("pickup_window_end_time") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label7" runat="server" Text='<%# Eval("last_visited") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label8" runat="server" Text='<%# Bind("capacity_gallons") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label9" runat="server" Text='<%# Bind("vehicle_size") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label0" runat="server" Text='<%# Eval("contact_name") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label11" runat="server" Text='<%# Eval("contact_email") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label12" runat="server" Text='<%# Bind("type_text") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label13" runat="server" Text='<%# Eval("days_until_due") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label14" runat="server" Text='<%# Eval("matrix_weight") %>'></asp:Label>
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
                    <asp:TextBox class="tableInput" ID="txtEditClientName" runat="server" Text='<%# Bind("location_name") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditClientPriority" runat="server" Text='<%# Bind("client_priority") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditAddress" runat="server" Text='<%# Bind("address") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditPickupIntervalDays" runat="server" Text='<%# Bind("pickup_interval_days") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditPickupWindowStartTime" runat="server" Text='<%# Bind("pickup_window_start_time") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditPickupWindowEndTime" runat="server" Text='<%# Bind("pickup_window_end_time") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditLastVisited" runat="server" Text='<%# Bind("last_visited") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditCapacityGallons" runat="server" Text='<%# Bind("capacity_gallons") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditVehicleSize" runat="server" Text='<%# Bind("vehicle_size") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditContactName" runat="server" Text='<%# Bind("contact_name") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:TextBox class="tableInput" ID="txtEditContactEmail" runat="server" Text='<%# Bind("contact_email") %>'></asp:TextBox>
                </td>
                <td>
                    <asp:DropDownList class="tableInput" ID="ddlEditLocationType"  runat="server"></asp:DropDownList>
                </td>
                <td>
                    <asp:Label ID="lblDaysUntilDue" runat="server" Text='<%# Eval("days_until_due") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="lblEditMatrixWeight" runat="server" Text='<%# Eval("matrix_weight") %>'></asp:Label>
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
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertClientName" placeholder="Sample Location Name" runat="server" Text='<%# Bind("location_name") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertClientPriority" placeholder="1" runat="server" Text='<%# Bind("client_priority") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertAddress" placeholder="1000 Awesome Drive" runat="server" Text='<%# Bind("address") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertPickupIntervalDays" placeholder="30" runat="server" Text='<%# Bind("pickup_interval_days") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertPickupWindowStartTime" placeholder="06:00" runat="server" Text='<%# Bind("pickup_window_start_time") %>'></asp:TextBox>
                    </asp:Panel>
                </td>

                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertPickupWindowEndTime" placeholder="16:00" runat="server" Text='<%# Bind("pickup_window_end_time") %>'></asp:TextBox>
                    </asp:Panel>
                </td>

                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertLastVisited" placeholder="01/01/2018" runat="server" Text='<%# Bind("last_visited") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertCapacityGallons" placeholder="500" runat="server" Text='<%# Bind("capacity_gallons")%>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertVehicleSize" placeholder="1-10" runat="server" Text='<%# Bind("vehicle_size") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertContactName" runat="server" Text='<%# Bind("contact_name") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertContactEmail" runat="server" Text='<%# Bind("contact_email") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:DropDownList class="tableInput" ID="ddlInsertLocationType" runat="server"></asp:DropDownList>
                </td>
                <td></td>
                <td></td>
                <td>
                    <asp:LinkButton ID="btnInsert" Text="Insert" runat="server" CommandName="Insert" />
                </td>
            </tr>
        </InsertItemTemplate>

    </asp:ListView>

    <script>
        function disp_confirm() {
            var r = confirm("This will delete the record permanently.  Are you sure?");
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
