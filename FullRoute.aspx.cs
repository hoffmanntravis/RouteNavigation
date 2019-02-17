using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RouteNavigation
{
    public partial class _FullRoute : Page
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private DataTable table;
        private string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                DataAccess.PopulateConfig();
                //initialize objects in page load since they make a sync calls that fail while the page is still starting up

                if (!Page.IsPostBack)
                    BindListView();
            }
            catch (Exception exception)
            {
                routeValidation.IsValid = false;
                routeValidation.ErrorMessage = exception.Message;
                Logger.Error(exception);
            }
        }

        protected void RouteDetailsListView_PagePropertiesChanging(object sender, EventArgs e)
        {
        }

        protected void RouteDetailsListView_RowCancelingEdit(object sender, ListViewCancelEventArgs e)
        {
            RouteDetailsListView.EditIndex = -1;
            BindListView();
        }

        protected void RouteDetailsListView_RowUpdating(object sender, ListViewUpdateEventArgs e)
        {
            try
            {
                //Finding the controls from Gridview for the row which is going to update
                int locationId = int.Parse(((Label)RouteDetailsListView.EditItem.FindControl("lblLocationId")).Text.Trim());
                int routeId = int.Parse(((TextBox)RouteDetailsListView.EditItem.FindControl("txtRouteId")).Text.Trim());
                int order = int.Parse(((TextBox)RouteDetailsListView.EditItem.FindControl("lblOrder")).Text.Trim());

                if (locationId == Config.Calculation.origin.Id)
                {
                    Exception exception = new Exception(String.Format("Cannot move location with Id of {0}, since it is the depot and must be both the start and end of the route.", locationId));
                    throw exception;
                }
                DataAccess.UpdateRouteLocation(locationId, routeId, order);
                RouteDetailsListView.EditIndex = -1;

                BindListView();

            }
            catch (Exception exception)
            {
                routeValidation.IsValid = false;
                routeValidation.ErrorMessage = exception.Message;
                Logger.Error(exception);
            }
        }

        protected void RouteDetailsListView_RowEditing(object sender, ListViewEditEventArgs e)
        {

            RouteDetailsListView.EditIndex = e.NewEditIndex;
            BindListView();
        }

        protected void RouteDetailsListView_RowDeleting(object sender, ListViewDeleteEventArgs e)
        {
            int routeId = int.Parse(((HyperLink)RouteDetailsListView.EditItem.FindControl("urlRouteId")).Text);
            int locationId = int.Parse(((Label)RouteDetailsListView.EditItem.FindControl("lblLocationId")).Text);

            DataAccess.DeleteLocationFromRouteLocation(routeId, locationId);
            BindListView();
        }

        protected void RouteDetailsListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
        }

        protected void BindListView()
        {
            table = DataAccess.GetRouteDetailsData();
            RouteDetailsListView.DataSource = table;

            //panelFullRoute.ViewStateMode = ViewStateMode.Disabled;
            RouteDetailsListView.DataBind();
            //panelFullRoute.ViewStateMode = ViewStateMode.Enabled;
        }

    }
}