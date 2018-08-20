<%@ Page Title="Configuration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Config.aspx.cs"  Inherits="RouteNavigation._Config" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="locationErrorsPanel" runat="server" CssClass="ErrorSummary">
        <asp:CustomValidator ID="dataValidation" runat="server"
            Display="None" EnableClientScript="False"></asp:CustomValidator>
        <asp:ValidationSummary ID="ErrorSummary" runat="server"
            HeaderText="Errors occurred:"
            BackColor="#fe6363" CellPadding="5"></asp:ValidationSummary>
    </asp:Panel>


    <h3>General</h3>
    <div>Enter the origin location id of all routes that will be calculated (This can be searched for and identified in the 'Locations' page).</div>
    <asp:TextBox class="tableInput" ID="txtOriginLocationId" runat="server"></asp:TextBox>
    <asp:TextBox class="tableInput" ReadOnly="true" BackColor="lightgray" ID="txtOriginName" runat="server"></asp:TextBox>
    <asp:TextBox class="tableInput" ReadOnly="true" BackColor="lightgray" ID="txtOriginAddress" runat="server"></asp:TextBox>
    <h3>Matrix Calculation values</h3>

    <div>Linear multiplier of the client priority used in matrix weight calculations</div>
    <asp:TextBox class="tableInput" ID="txtMatrixPriorityMultiplier" runat="server"></asp:TextBox>
    <div>Absolute value of exponent applied to matrix calculation weight, based on days until due.  Can be a decimal point for fine grained control (e.g. 1.2).</div>
    <asp:TextBox class="tableInput" ID="txtMatrixDaysUntilDueExponent" runat="server"></asp:TextBox>
    <div>Linear mutliplier of the weight if the date is overdue.  Multiplied against the exponent above.  Emphasizes past due locations even further.</div>
    <asp:TextBox class="tableInput" ID="txtMatrixOverDueMultiplier" runat="server"></asp:TextBox>
    <div>If feature 'Prioritize nearest location' is enabled, the closest distance will be factored into matrix weight calculations.</div>
    <asp:TextBox class="tableInput" ID="txtMatrixDistanceFromSource" runat="server"></asp:TextBox>

    <h3>Calculation Parameters</h3>
    <div>Enter the minimum number of days that is acceptable to attempt a pickup (pickup interval - (now - last visited))</div>
    <asp:TextBox class="tableInput" ID="txtMinimumDaysUntilPickup" runat="server"></asp:TextBox>
    <div>Enter the maximum number of days overdue that is acceptable to attempt a pickup</div>
    <asp:TextBox class="tableInput" ID="txtMaximumDaysOverdue" runat="server"></asp:TextBox>
    <div>Maximum distance of a route</div>
    <asp:TextBox class="tableInput" ID="txtRouteDistanceMaxMiles" runat="server"></asp:TextBox>

    <div>Enter the time of day that the workday starts</div>
    <asp:TextBox class="tableInput" placeholder="08:00:00" ID="txtWorkDayStart" runat="server"></asp:TextBox>

    <div>Enter the time of day that the workday ends</div>
    <asp:TextBox class="tableInput" placeholder="20:00:00" ID="txtWorkDayEnd" runat="server"></asp:TextBox>

    <div>
        If the 'Vehicle Fill Level' feature is enabled, this margin of error will be used when estimating
    the current fluid level of a location (location level + margin), compared to the current level of the truck.
    </div>
    <asp:TextBox class="tableInput" ID="txtCurrentFillLevelErrorMargin" runat="server"></asp:TextBox>
    <div>Average length in minutes of an oil pickup</div>
    <asp:TextBox class="tableInput" ID="txtOilPickupAverageDuration" runat="server"></asp:TextBox>
    <div>Average length in minutes of a grease trap pickup</div>
    <asp:TextBox class="tableInput" ID="txtGreasePickupAverageDuration" runat="server"></asp:TextBox>

    <h3>Feature Toggles</h3>
    <div>
        <asp:CheckBox class="checkBox" ID="txtChkVehicleFillLevel" runat="server"></asp:CheckBox>
        Compare Vehicle Fill Level with estimated location level before assigning
    </div>
    <div>
        <asp:CheckBox class="checkBox" ID="txtChkPrioritizeNearestLocation" runat="server"></asp:CheckBox>
        Incorporate distance as part of the priority calculations.  The value of this feature is dubious, and should probably be left disabled.
    </div>
    <div>
        <asp:Button ID="btnUpdateSettings" Text="Update Settings" runat="server" OnClick="BtnUpdateSettings_Click"></asp:Button>
    </div>
</asp:Content>
