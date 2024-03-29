﻿<%@ Page Title="Configuration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Config.aspx.cs" Inherits="RouteNavigation._Config" %>

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

    <h3>Calculation Parameters</h3>
    <div></div>
    <asp:Label Text="Oil Early Service Ratio" ToolTip="A ratio of the maximum number of days before next oil service that will be tolerated to consider a pickup (e.g. .1 of 30 days would be 3 days before service.  A pickup before 3 days early would not be allowed.)" runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="15" ID="txtOilEarlyServiceRatio" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Grease Trap Early Service Ratio" ToolTip="A ratio of the maximum number of days before next grease service that will be tolerated to consider a pickup (e.g. .1 of 30 days would be 3 days before service.  A pickup before 3 days early would not be allowed.)" runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="15" ID="txtGreaseEarlyServiceRatio" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Maximum days overdue" ToolTip="Maximum number of days overdue that is acceptable to attempt a pickup before the location is orphaned (assumed a defunct client). Set to a very large number if you want to ignore this feature." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="90" ID="txtMaximumDaysOverdue" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Maximum Distance From Depot" ToolTip="Maximum distance from the depot that will be considered before the location is orphaned and expected to be handled by manual route planning or other means." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="100" ID="txtMaxDistanceFromDepot" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Workday Start Time" ToolTip="Starting time of a route (workday) when calculating travel time and pickup time, which is compared to the end time to determine if more locations can be added to a route." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="08:00:00" ID="txtWorkDayStart" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Workday End Time" ToolTip="End time of a route (workday). Once waypoints added exceed this time, no more will be added to the route." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="20:00:00" ID="txtWorkDayEnd" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Grease Trap Cutoff Time" ToolTip="Ceiling on the last time a grease trap can be picked up for the day." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="13:00:00" ID="txtGreaseTrapCutoffTime" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Vehicle Fill Level Error Margin" ToolTip="If the 'Vehicle Fill Level' feature is enabled, this margin of error will be used when estimating the current fluid level of a location (location level + margin), compared to the current level of the truck." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder=".05" ID="txtCurrentFillLevelErrorMargin" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Average oil duration (minutes)" ToolTip="Average length in minutes of an oil pickup. Used in time calculations when adding routes." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="30:00:00" ID="txtOilPickupAverageDuration" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Average grease trap duration (minutes)" ToolTip="Average length in minutes of a grease trap pickup. Used in time calculations when adding routes." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="30:00:00" ID="txtGreasePickupAverageDuration" runat="server"></asp:TextBox>

    <div></div>
    <asp:Label Text="Next Location Minimum Search Distance" ToolTip="The length in miles of a minimum distance to search when retrieving the next location of a route (beyond the first location)." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" ID="txtSearchMinimumDistance" placeholder="5" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Search Radius Ratio (Distance from Depot)" ToolTip="A decimal fraction value multiplied by the distance traveled from the depot, used to calculate the maximum distance to search for an additional location.  This is used to increase route density, by limiting the algorithm from connecting locations that are far apart in order to achieve global optimal route length.  Value increases with distance to avoid efficiency losses if a long distance has already been traveled to get to the current location.  Numbers closer or equal to 1 will result in more net efficiency, but also some erratic route creation to handle outlier locations." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" ID="txtSearchRadiusPercent" placeholder=".25" runat="server"></asp:TextBox>

    <h3>Genetic Algorithm Parameters</h3>
    <div></div>
    <asp:Label Text="Iterations" runat="server" ToolTip="The number of iterations the genetic algorithm should run. More iterations are more accurate, but take longer and offer diminishing returns." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="200" ID="txtIterations" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Population Size" runat="server" ToolTip="Number of randomized permutations of routes the algorithm should start with. This can have a medium impact on calculation accuracy. Larger numbers explore more potential options but take longer." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="200" ID="txtPopulationSize" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Neighbor Count" runat="server" ToolTip="Number of locations that are considered neighbors of another location, sorted by distance ascending. Larger numbers are more accurate, but take more time." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="200" ID="txtNeighborCount" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Tournament Size" runat="server" ToolTip="Number of route permutations that 'compete' in the algorithm for the right to procreate into new routes. Larger numbers converge faster but explore less space." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="10" ID="txtTournamentSize" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Tournament Winner Count" runat="server" ToolTip="Number of tournament winners that have a chance to breed. If the breeders count is smaller than the tournament winner count, tournament winners will be randomely selected to breed." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="1" ID="txtTournamentWinnerCount" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Breeder Count" runat="server" ToolTip="Number of tournament winners that will be selected to breed. If there are not enough tournament winners, more tournaments will be run until the pool is full." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="2" ID="txtBreederCount" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Offspring Pool Size" runat="server" ToolTip="Number of offspring to be produced by breeders, to be selected for potential genetic mutation thereafter." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="1" ID="txtOffspringPoolSize" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Crossover Probability" runat="server" ToolTip="Probability that two breeders will perform an edge recombination based genetic composition crossover. Otherwise, the parent will pass alleles to the offspring directly without variation. This can have a major impact on convergence, and should be kept fairly high (10 to 50 percent)." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder=".25" ID="txtCrossoverProbability" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Elitism Ratio" runat="server" ToolTip="The percentage of routes will be selected as an 'elite' and will not be selected for breeding or genetic mutation, and instead preserved as is. This is useful for ensuring a high quality solution is not lost incidentally. Chance should typically be smaller than 1 percent." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder=".001" ID="txtElitismRatio" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Mutation Probability" runat="server" ToolTip="The chance that an offspring will mutate by having one of it's alleles moved to another position arbitrarily. Percentage should typically be small, 1 to 10 percent. Useful for avoiding local optima / preconvergence." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder=".05" ID="txtMutationProbability" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Mutation Allele Max" ToolTip="When a mutation occurs, this is the number of potential swaps that may occur. The count is based on a dice roll, so 1 to n where n is max swaps may occur." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="1" ID="txtMutationAlleleMax" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Growth / Decay exponent" runat="server" ToolTip="This value impacts the rate at which various probabilities increase / decrease as iterations of the algorithm complete. Certain variables like mutation are less useful as more iterations complete and should be reduced dynmically, and this value helps accomplish that. Value should be 1 by default if the feature is enabled below, and adjusted positive or negatively by a decimal point (e.g. .8 or 1.2). Typically a value higher than 2 would be excessive and should be avoided." />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="1" ID="txtGrowthDecayExponent" runat="server"></asp:TextBox>

    <h3>Feature Toggles</h3>
    <div>
        <asp:CheckBox class="checkBox" ID="chkVehicleFillLevel" ToolTip="Compare Vehicle Fill Level with estimated location level before assigning" runat="server"></asp:CheckBox>
        Calculations - Vehicle Fill Level
    </div>

    <div>
        <asp:CheckBox class="checkBox" ID="chkExcludeJettingLocationsCalculation" ToolTip="Exclude jetting locations from calculations" runat="server"></asp:CheckBox>
        Calculations - Exclude Jetting Locations
    </div>

    <div>
        <asp:CheckBox class="checkBox" ID="chkExcludeGreaseLocationsOver500FromCalc" ToolTip="Exclude grease locations over 500 gallons from calculations" runat="server"></asp:CheckBox>
        Calculations - Exclude Grease Locations Over 500 Gallons
    </div>

    <div>
        <asp:CheckBox class="checkBox" ID="chkExcludeJettingLocationsImport" ToolTip="Exclude jetting locations on import" runat="server"></asp:CheckBox>
        Data Management - Exclude Jetting Locations on Import
    </div>

    <div>
        <asp:CheckBox class="checkBox" ID="chkGrowthDecayExponent" ToolTip="Enabling this feature dynamically changes the rate at which various probabilities increase / decrease as iterations of the algorithm complete, as opposed to the probabilities static after each iteration completes. Under ideal circumstances this may help reach convergence faster, or possibly explore more space faster in the genetic algorithm. If you're unsure, leave it unchecked." runat="server"></asp:CheckBox>
        Genetic Algorithm - Growth / Decay Exponent
    </div>

    <div>
        <asp:Button ID="btnUpdateSettings" Text="Update Settings" runat="server" OnClick="BtnUpdateSettings_Click"></asp:Button>
    </div>
</asp:Content>
