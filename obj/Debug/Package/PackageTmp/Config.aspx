<%@ Page Title="Configuration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Config.aspx.cs" Inherits="RouteNavigation._Config" %>

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

    <div></div>
    <asp:Label Text="Client Priority Multiplier" ToolTip="Linear multiplier of the client priority used in matrix weight calculations" runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" ID="txtMatrixPriorityMultiplier" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Days Until Due Priority Exponent" ToolTip="Absolute value of exponent applied to matrix calculation weight, based on days until due. Can be a decimal point for fine grained control (e.g. 1.2)." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" ID="txtMatrixDaysUntilDueExponent" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Overdue Priority Multiplier" ToolTip="Linear mutliplier of the weight if the date is overdue. Multiplied against the exponent above. Emphasizes past due locations even further." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" ID="txtMatrixOverDueMultiplier" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Distance Priority" ToolTip="If feature 'Prioritize nearest location' is enabled, the distance of a location from the depot will be factored into matrix weight calculations. A closer distance will be prioritized higher than a farther location, due to the cost incurred on the business." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" ID="txtMatrixDistanceFromSource" runat="server"></asp:TextBox>

    <h3>Calculation Parameters</h3>
    <div></div>
    <asp:Label Text="Minimum days before pickup" ToolTip="Minimum number of days that is acceptable to attempt a pickup (pickup interval - (now - last visited))" runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="15" ID="txtMinimumDaysUntilPickup" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Maximum days overdue" ToolTip="Maximum number of days overdue that is acceptable to attempt a pickup before the location is orphaned (assumed a defunct client). Set to a very large number if you want to ignore this feature." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="90" ID="txtMaximumDaysOverdue" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Maximum Route Distance" ToolTip="Maximum distance from the depot that will be considered before the location is orphaned and expected to be handled by manual route planning or other means." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="100" ID="txtRouteDistanceMaxMiles" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Workday End Time" ToolTip="Starting time of a route (workday) when calculating travel time and pickup time, which is compared to the end time to determine if more locations can be added to a route." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="08:00:00" ID="txtWorkDayStart" runat="server"></asp:TextBox>
    <div></div>
    <asp:Label Text="Workday Start Time" ToolTip="End time of a route (workday). Once waypoints added exceed this time, no more will be added to the route." runat="server" />
    <div></div>
    <asp:TextBox class="tableInput" placeholder="20:00:00" ID="txtWorkDayEnd" runat="server"></asp:TextBox>
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
        <asp:CheckBox class="checkBox" ID="chkPrioritizeNearestLocation" ToolTip="Incorporate distance as part of the priority calculations. The value of this feature is dubious, and should probably be left disabled." runat="server"></asp:CheckBox>
        Calculations - Include Distance in Priority
    </div>

    <div>
        <asp:CheckBox class="checkBox" ID="chkGrowthDecayExponent" ToolTip="Enabling this feature dynamically changes the rate at which various probabilities increase / decrease as iterations of the algorithm complete, as opposed to the probabilities static after each iteration completes. Under ideal circumstances this may help reach convergence faster, or possibly explore more space faster in the genetic algorithm. If you're unsure, leave it unchecked." runat="server"></asp:CheckBox>
        Genetic Algorithm - Growth / Decay Exponent
    </div>

    <div>
        <asp:Button ID="btnUpdateSettings" Text="Update Settings" runat="server" OnClick="BtnUpdateSettings_Click"></asp:Button>
    </div>
</asp:Content>
