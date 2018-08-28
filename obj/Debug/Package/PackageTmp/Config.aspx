<%@ Page Title="Configuration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true" CodeBehind="Config.aspx.cs"  Inherits="RouteNavigation._Config" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div></div><asp:Panel ID="locationErrorsPanel" runat="server" CssClass="ErrorSummary">
        <div></div><asp:CustomValidator ID="dataValidation" runat="server"
            Display="None" EnableClientScript="False"></asp:CustomValidator>
        <div></div><asp:ValidationSummary ID="ErrorSummary" runat="server"
            HeaderText="Errors occurred:"
            BackColor="#fe6363" CellPadding="5"></asp:ValidationSummary>
    </asp:Panel>


    <h3>General</h3>
    <div>Enter the origin location id of all routes that will be calculated (This can be searched for and identified in the 'Locations' page).</div>
    <div></div><asp:TextBox class="tableInput" ID="txtOriginLocationId" runat="server"></asp:TextBox>
    <div></div><asp:TextBox class="tableInput" ReadOnly="true" BackColor="lightgray" ID="txtOriginName" runat="server"></asp:TextBox>
    <div></div><asp:TextBox class="tableInput" ReadOnly="true" BackColor="lightgray" ID="txtOriginAddress" runat="server"></asp:TextBox>
    <h3>Matrix Calculation values</h3>

    <div>Linear multiplier of the client priority used in matrix weight calculations</div>
    <div></div><asp:TextBox class="tableInput" ID="txtMatrixPriorityMultiplier" runat="server"></asp:TextBox>
    <div>Absolute value of exponent applied to matrix calculation weight, based on days until due.  Can be a decimal point for fine grained control (e.g. 1.2).</div>
    <div></div><asp:TextBox class="tableInput" ID="txtMatrixDaysUntilDueExponent" runat="server"></asp:TextBox>
    <div>Linear mutliplier of the weight if the date is overdue.  Multiplied against the exponent above.  Emphasizes past due locations even further.</div>
    <div></div><asp:TextBox class="tableInput" ID="txtMatrixOverDueMultiplier" runat="server"></asp:TextBox>
    <div>If feature 'Prioritize nearest location' is enabled, the closest distance will be factored into matrix weight calculations.</div>
    <div></div><asp:TextBox class="tableInput" ID="txtMatrixDistanceFromSource" runat="server"></asp:TextBox>

    <h3>Calculation Parameters</h3>
    <div>Enter the minimum number of days that is acceptable to attempt a pickup (pickup interval - (now - last visited))</div>
    <div></div><asp:TextBox class="tableInput" ID="txtMinimumDaysUntilPickup" runat="server"></asp:TextBox>
    <div>Enter the maximum number of days overdue that is acceptable to attempt a pickup</div>
    <div></div><asp:TextBox class="tableInput" ID="txtMaximumDaysOverdue" runat="server"></asp:TextBox>
    <div>Maximum distance of a route</div>
    <div></div><asp:TextBox class="tableInput" ID="txtRouteDistanceMaxMiles" runat="server"></asp:TextBox>

    <div>Enter the time of day that the workday starts</div>
    <div></div><asp:TextBox class="tableInput" placeholder="08:00:00" ID="txtWorkDayStart" runat="server"></asp:TextBox>

    <div>Enter the time of day that the workday ends</div>
    <div></div><asp:TextBox class="tableInput" placeholder="20:00:00" ID="txtWorkDayEnd" runat="server"></asp:TextBox>

    <div>
        If the 'Vehicle Fill Level' feature is enabled, this margin of error will be used when estimating
    the current fluid level of a location (location level + margin), compared to the current level of the truck.
    </div>
    <div></div><asp:TextBox class="tableInput" ID="txtCurrentFillLevelErrorMargin" runat="server"></asp:TextBox>
    <div>Average length in minutes of an oil pickup</div>
    <div></div><asp:TextBox class="tableInput" ID="txtOilPickupAverageDuration" runat="server"></asp:TextBox>
    <div>Average length in minutes of a grease trap pickup</div>
    <div></div><asp:TextBox class="tableInput" ID="txtGreasePickupAverageDuration" runat="server"></asp:TextBox>

    <h3>Genetic Algorithm Parameters</h3>
    <div></div><asp:label Text="Iterations" runat="server" tooltip="The number of iterations the genetic algorithm should run.  More iterations are more accurate, but take longer and offer diminishing returns."/>
    <div></div><asp:TextBox class="tableInput" ID="txtIterations" PlaceHolder="200" runat="server"></asp:TextBox>
    <div></div><asp:label Text="Population Size" runat="server" tooltip="Number of randomized permutations of routes the algorithm should start with.  This can have a medium impact on calculation accuracy.  Larger numbers explore more potential options but take longer." />
    <div></div><asp:TextBox class="tableInput" ID="txtPopulationSize" PlaceHolder="200"  runat="server"></asp:TextBox>
    <div></div><asp:label Text="Neighbor Count" runat="server" tooltip="Number of locations that are considered neighbors of another location, sorted by distance ascending.  Larger numbers are more accurate, but take more time."  />
    <div></div><asp:TextBox class="tableInput" ID="txtNeighborCount" PlaceHolder="400" runat="server"></asp:TextBox>
    <div></div><asp:Label Text="Tournament Size" runat="server" tooltip="Number of route permutations that 'compete' in the algorithm for the right to procreate into new routes.  Larger numbers converge faster but explore less space." />
    <div></div><asp:TextBox class="tableInput" ID="txtTournamentSize" runat="server"></asp:TextBox>
    <div></div><asp:Label Text="Tournament Winner Count" runat="server" tooltip="Number of tournament winners that have a chance to breed.  If the breeders count is smaller than the tournament winner count, tournament winners will be randomely selected to breed." />
    <div></div><asp:TextBox class="tableInput" ID="txtTournamentWinnerCount"  runat="server"></asp:TextBox>
    <div></div><asp:Label Text="Breeder Count" runat="server" tooltip="Number of tournament winners that will be selected to breed.  If there are not enough tournament winners, more tournaments will be run until the pool is full." />
    <div></div><asp:TextBox class="tableInput" ID="txtBreederCount"  runat="server"></asp:TextBox>
    <div></div><asp:Label Text="Offspring Pool Size" runat="server" tooltip="Number of offspring to be produced by breeders, to be selected for potential genetic mutation thereafter." />
    <div></div><asp:TextBox class="tableInput" ID="txtOffspringPoolSize"  runat="server"></asp:TextBox>
    <div></div><asp:Label Text="Crossover Probability" runat="server" tooltip="Probability that two breeders will perform an edge recombination based genetic composition crossover.  Otherwise, the parent will pass alleles to the offspring directly without variation.  This can have a major impact on convergence, and should be kept fairly high (10 to 50 percent)."/>
    <div></div><asp:TextBox class="tableInput" ID="txtCrossoverProbability"  runat="server"></asp:TextBox>
    <div></div><asp:Label Text="Elitism Ratio" runat="server" tooltip="The percentage of routes will be selected as an 'elite' and will not be selected for breeding or genetic mutation, and instead preserved as is.  This is useful for ensuring a high quality solution is not lost incidentally.  Chance should typically be smaller than 1 percent." />
    <div></div><asp:TextBox class="tableInput" ID="txtElitismRatio"  runat="server"></asp:TextBox>
    <div></div><asp:Label Text="Mutation Probability" runat="server" tooltip="The chance that an offspring will mutate by having one of it's alleles moved to another position arbitrarily.  Percentage should typically be small, 1 to 10 percent.  Useful for avoiding local optima / preconvergence." />
    <div></div><asp:TextBox class="tableInput" ID="txtMutationProbability"  runat="server"></asp:TextBox>
    <div></div><asp:Label Text="Mutation Allele Max" tooltip="When a mutation occurs, this is the number of potential swaps that may occur.  The count is based on a dice roll, so 1 to n where n is max swaps may occur." runat="server" />
    <div></div><asp:TextBox class="tableInput" ID="TextBox1" runat="server"></asp:TextBox>
    <div></div><asp:Label Text="Growth / Decay exponent" runat="server" tooltip="Value should be 1 by default, and adjusted positive or negatively by a decimal point (e.g. .8 or 1.2).  This value impacts the rate at which various probabilities increase / decrease as iterations complete.  Certain variables like mutation are less useful as the iterations run and should be reduced, and this value helps accomplish that." />
    <div></div><asp:TextBox class="tableInput" ID="txtGrowthDecayExponent" runat="server"></asp:TextBox>

    <h3>Feature Toggles</h3>
    <div>
        <div></div><asp:CheckBox class="checkBox" ID="txtChkVehicleFillLevel" runat="server"></asp:CheckBox>
        Compare Vehicle Fill Level with estimated location level before assigning
    </div>
    <div>
        <div></div><asp:CheckBox class="checkBox" ID="txtChkPrioritizeNearestLocation" runat="server"></asp:CheckBox>
        Incorporate distance as part of the priority calculations.  The value of this feature is dubious, and should probably be left disabled.
    </div>
    <div>
        <div></div><asp:Button ID="btnUpdateSettings" Text="Update Settings" runat="server" OnClick="BtnUpdateSettings_Click"></asp:Button>
    </div>
</asp:Content>
