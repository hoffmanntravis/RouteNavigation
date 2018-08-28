<%@ Page Title="Vehicle" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Vehicle.aspx.cs" Inherits="RouteNavigation._Vehicle" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <link rel="StyleSheet" href="/content/table.css" type="text/css">
    <link rel="StyleSheet" href="/content/Site.css" type="text/css">
    <asp:Panel ID="vehicleErrorsPanel" runat="server" CssClass="ErrorSummary">
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
            <asp:TextBox CssClass="headerRowLeft" defaultButton="btnSearch" ID="TxtSearchFilter" placeholder="Filter: Location Name" runat="server" Style="float: left;" />
            <asp:Button CssClass="headerRowLeft" ID="btnSearch" OnClick="FilterVehiclesListView_Click" Text="Search" runat="server" Style="float: left;" />
        </asp:Panel>
    </div>

    <asp:ListView ID="VehiclesListView" runat="server"
        DataKeyNames="id"
        OnPagePropertiesChanging="VehiclesListView_PagePropertiesChanging"
        OnItemEditing="VehiclesListView_RowEditing"
        OnItemCanceling="VehiclesListView_RowCancelingEdit"
        OnItemUpdating="VehiclesListView_RowUpdating"
        OnItemDeleting="VehiclesListView_RowDeleting"
        OnItemInserting="VehiclesListView_RowInsert"
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
                        </td>
                        <td runat="server">
                            <b>Name</b>
                        </td>
                        <td id="td2" runat="server">
                            <b>Model</b>
                        </td>
                        <td id="td3" runat="server">
                            <b>Capacity Gallons</b>
                        </td>
                        <td id="td4" runat="server">
                            <b>Physical Size</b>
                        </td>
                        <td id="td5" runat="server">
                            <b>Operational</b>
                        </td>
                        <td id="td10" width="100" runat="server">
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
                        <asp:Label ID="lblId" runat="server" Text='<%# Eval("id") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label1" runat="server" Text='<%# Eval("name") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label2" runat="server" Text='<%# Eval("model") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label3" runat="server" Text='<%# Eval("capacity_gallons") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label4" runat="server" Text='<%# Eval("physical_size") %>'></asp:Label>
                    </td>
                    <td>
                        <asp:Label ID="label5" runat="server" Text='<%# Eval("operational") %>'></asp:Label>
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
                    <asp:Label ID="label1" runat="server" Text='<%# Eval("name") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label2" runat="server" Text='<%# Eval("model") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label3" runat="server" Text='<%# Eval("capacity_gallons") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label4" runat="server" Text='<%# Eval("physical_size") %>'></asp:Label>
                </td>
                <td>
                    <asp:Label ID="label5" runat="server" Text='<%# Eval("operational") %>'></asp:Label>
                </td>
                <td>
                    <asp:LinkButton ID="btnEdit2" Text="Edit" runat="server" CommandName="Edit" />
                </td>
            </tr>
        </SelectedItemTemplate>
        <EditItemTemplate>
            <tr id="Tr1" runat="server">
                <td>
                    <asp:Panel runat="server" DefaultButton="btnUpdate">
                        <asp:Label ID="lblId" runat="server" Text='<%# Eval("id") %>'></asp:Label>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnUpdate">
                        <asp:TextBox class="tableInput" ID="txtEditName" runat="server" Text='<%# Bind("name") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnUpdate">
                        <asp:TextBox class="tableInput" ID="txtEditModel" runat="server" Text='<%# Bind("model") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnUpdate">
                        <asp:TextBox class="tableInput" ID="txtEditCapacityGallons" runat="server" Text='<%# Bind("capacity_gallons") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnUpdate">
                        <asp:TextBox class="tableInput" ID="txtEditPhysicalSize" runat="server" Text='<%# Bind("physical_size") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnUpdate">
                        <asp:TextBox class="tableInput" ID="txtEditOperational" runat="server" Text='<%# Bind("operational") %>'></asp:TextBox>
                    </asp:Panel>
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
                        <asp:TextBox class="tableInput" ID="txtInsertName" placeholder="Vehicle Name" runat="server" Text='<%# Bind("name") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertModel" placeholder="Vehicle Model" runat="server" Text='<%# Bind("model") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertCapacityGallons" placeholder="500" runat="server" Text='<%# Bind("capacity_gallons") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertPhysicalSize" placeholder="1 - 10" runat="server" Text='<%# Bind("physical_size") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:Panel runat="server" DefaultButton="btnInsert">
                        <asp:TextBox class="tableInput" ID="txtInsertOperational" placeholder="True / False" runat="server" Text='<%# Bind("operational") %>'></asp:TextBox>
                    </asp:Panel>
                </td>
                <td>
                    <asp:LinkButton ID="btnInsert" Text="Insert" runat="server" CommandName="Insert" />
                </td>
            </tr>
        </InsertItemTemplate>

    </asp:ListView>

    <!--<asp:DataPager ID="vehicleDataPager" runat="server" PagedControlID="VehiclesListView" PageSize="5000">
        <Fields>
            <asp:NextPreviousPagerField ButtonType="Link" />
        </Fields>
    </asp:DataPager>-->

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
            var r = confirm("Warning! this will overwrite all vehicles in the database.  Are you sure?");
            if (r == true) {
            }
            else {
                return false;
            }
        }
    </script>
</asp:Content>
