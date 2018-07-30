using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RouteNavigation
{
    public partial class _Batch : Page
    {
        protected DataTable table;
        protected string conString = System.Configuration.ConfigurationManager.ConnectionStrings["RouteNavigation"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            //initialize objects in page load since they make a sync calls that fail while the page is still starting up
            if (!Page.IsPostBack)
            {
                BindListView();
            }
        }
        protected void BatchListView_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            (BatchListView.FindControl("batchDataPager") as DataPager).SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            BindListView();
        }


        protected void BindListView()
        {
            table = DataAccess.getRouteBatchData();
            BatchListView.DataSource = table;
            BatchListView.ItemPlaceholderID = "itemPlaceHolder";
            BatchListView.DataBind();
        }
    }

}