using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FleetManager.vhs;

namespace FleetManager
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        VehicleServiceSoapClient vhs = new VehicleServiceSoapClient();
        

        protected void Page_Load(object sender, EventArgs e)
        {
            Session.RemoveAll();
            vhs.GetOwnerCompleted += vhs_GetOwnerCompleted;
        }

        void vhs_GetOwnerCompleted(object sender, GetOwnerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                if (e.Result.owner_id > 0)
                {
                    Owner driver = new Owner();
                    driver = e.Result;
                    Session.Add("lkiuytd", driver); //session for secure login
                    Response.Redirect("Index.aspx");
                    //Server.Transfer("Index.aspx");
                }
                else
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "ErrorAlert", "alert('Wrong username/password entered');", true); //msgbox
                }

            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            vhs.GetOwnerAsync(txtUname.Text, txtPass.Text);
        }


    }
}