using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FleetManager.vhs;
using FleetManager.CameraService;
using System.Text;


namespace FleetManager
{
    public partial class Index : System.Web.UI.Page
    {
        DatabaseServiceSoapClient LiveCam = new DatabaseServiceSoapClient();
        List<ImageCap> ListOfImages;

        VehicleServiceSoapClient vhs = new VehicleServiceSoapClient();
        private static VehicleInfo[] vinfo; //vehicle array
        private static VehicleInfo vehicle = new VehicleInfo();

        double latitude = 0;
        double longitude = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            vhs.GetCurrentVehicleTrackingCompleted += vhs_GetCurrentVehicleTrackingCompleted;
            vhs.GetVehicleInfoByOwnerIDCompleted += vhs_GetVehicleInfoByOwnerIDCompleted;
            LiveCam.GetListOfImageCaptureCompleted += LiveCam_GetListOfImageCaptureCompleted;

            if (Session.Keys.Count > 0)
            {
                var driver = Session["lkiuytd"];
                if (driver != null)
                {
                    if (driver is Owner)
                    {
                        vhs.GetVehicleInfoByOwnerIDAsync((driver as Owner).owner_id);
                    }
                    //LiveCam.GetListOfImageCaptureAsync(1, "fleet_1");
                }
            }
            else
            {
                Server.Transfer("Login.aspx");
            }

            if (!IsPostBack)
            {
                StringBuilder str = new StringBuilder();

                str.Append("map = new Microsoft.Maps.Map(document.getElementById('mapDiv'),");
                str.Append("{credentials:'AqrJiFTsubyI00IKarR-PsFuIfMjw-D_WhUIEvud1rdxvNWBp3wnfv9yQJn2fZpD' });");

                ScriptManager.RegisterStartupScript(this, this.GetType(), "test", str.ToString(), true);
            }
        }

        void vhs_GetCurrentVehicleTrackingCompleted(object sender, GetCurrentVehicleTrackingCompletedEventArgs e)
        {
            LiveCam.GetListOfImageCaptureAsync(e.Result.time);

            lblLat.Text = e.Result.latitude.ToString("0.000");
            lblLon.Text = e.Result.longitude.ToString("0.000");
            StringBuilder str = new StringBuilder();

            // Define the pushpin location
            str.Append("var loc = new Microsoft.Maps.Location(" + lblLat.Text + ", " + lblLon.Text + ");");

            // Add a pin to the map
            str.Append("var pin = new Microsoft.Maps.Pushpin(loc); ");
            str.Append("map.entities.push(pin);");

            // Center the map on the location
            str.Append("map.setView({center: loc, zoom: 10});");

            ScriptManager.RegisterStartupScript(this, this.GetType(), "test1", str.ToString(), true);
        }

        void LiveCam_GetListOfImageCaptureCompleted(object sender, GetListOfImageCaptureCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    ImageCap img = e.Result.Last();
                    Image1.ImageUrl = "data:image/jpg;base64," + Convert.ToBase64String((byte[])img.Thumbnail);
                }
            }
            catch { }
        }

        void vhs_GetVehicleInfoByOwnerIDCompleted(object sender, GetVehicleInfoByOwnerIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                if (e.Result.Length > 0)
                {
                    cboDropDown.Items.Clear();
                    vinfo = e.Result;
                    for (int i = 0; i <e.Result.Length; i++) //loop through number of vehicles
                    {
                        cboDropDown.Items.Add(vinfo[i].vehicle_id);
                    }
                }
            }
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            if (cboDropDown.SelectedIndex > -1)
            {
                vhs.GetCurrentVehicleTrackingAsync(cboDropDown.SelectedItem.ToString());
            }
            
        }
    }
}