using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using InterVehicleFleetDriver.DatabaseService;
using InterVehicleFleetDriver.vhs;

// Directives
using Microsoft.Devices;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Media;
using System.Device.Location;

namespace InterVehicleFleetDriver
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region camera/byterarray
        DatabaseServiceSoapClient service = new DatabaseServiceSoapClient();
        PhotoCamera cam;
        private int photoCounter = 0;
        MediaLibrary library = new MediaLibrary();
        #endregion
        #region VehicleService
        VehicleServiceSoapClient vhs = new VehicleServiceSoapClient();
        Owner user;
        List<VehicleTracking> vt = new List<VehicleTracking>();
        VehicleInfo vinfo = new VehicleInfo();
        
        // Constructor
        GeoCoordinateWatcher watcher; //"watcher" reference to GeoCoordinateWatcher
        #endregion

        public MainPage()
        {
            InitializeComponent();
            if (watcher == null)
            {
                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High)    //get the highest accuracy
                {
                    //this is the minimum distance traveled before the next location update
                    MovementThreshold = 10
                };

                //executes when the position is changed
                watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
                watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
                 watcher.Start();

            }
            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }
        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.Landscape)
            {
                //txtStatus.HorizontalAlignment.ToString("Center");
                pageTitle.FontSize = 5.5;
            }
            else
            {
                pageTitle.FontSize = 10;
            }

        }

        #region vehicle tracking
        void vhs_GetVehicleInfoByVehicleIDCompleted(object sender, GetVehicleInfoByVehicleIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                vinfo = e.Result;
            }
        }

        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {       //event handler for when the location service changes it's status
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    txtStatus.Text = "Disabled";
                    break;
                case GeoPositionStatus.Initializing:
                    txtStatus.Text = "Initializing";
                    break;
                case GeoPositionStatus.NoData:
                    txtStatus.Text = "NoData";
                    break;
                case GeoPositionStatus.Ready:
                    txtStatus.Text = "Ready";
                    break;
            }
        }

        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            //  txtLatitude.Text = e.Position.Location.Latitude.ToString("0.000");                              //show the coordinates in the text boxes and format 
            //txtLongitude.Text = e.Position.Location.Longitude.ToString("0.000");                            // value to string
            //map1.Center = new GeoCoordinate(e.Position.Location.Latitude, e.Position.Location.Longitude); // update map to current location
            map1.SetView(new GeoCoordinate(e.Position.Location.Latitude, e.Position.Location.Longitude), 16); // automatically set map view to gps
            //map1.SetView(map1.Center, 12, e.Position.Location.Course); // automatically set map view to gps

            try
            {
                cam.CaptureImage();
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    txtMessage.Text = ex.Message;
                });
            }

            VehicleTracking vLoc = new VehicleTracking()
            {
                vehicle_id = "fleet_1",                          //add location info,data
                latitude = e.Position.Location.Latitude,
                longitude = e.Position.Location.Longitude,
                time = DateTime.Now
                //altitude = e.Position.Location.Altitude,
                //vehicleHeading = e.Position.Location.Course
            };

            vt.Add(vLoc);       //add vehicle location to vehicle list

            if (vt.Count > 5)  //every 15 positions update webservice accordingly
            {
                vhs.SetArrayOfVehicleTrackingAsync(vt.ToArray());   //convert list to array
                vhs.SetArrayOfVehicleTrackingAsync(vt.ToArray());
                vt.Clear(); //clear items so that resources are refreshed 
            }
        }

        void vhs_SetArrayOfVehicleTrackingCompleted(object sender, SetArrayOfVehicleTrackingCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }
#endregion  
        #region LiveCamera

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true)
            {
                cam = new PhotoCamera(CameraType.Primary);
                cam.CaptureImageAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureImageAvailable);
                viewfinderBrush.SetSource(cam);
            }
            else
            {
                txtMessage.Text = "The front facing camera is not available on this device";

            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                cam.Dispose();
            }
        }

        void viewfinder_Tapped(object sender, GestureEventArgs e)
        {
            {
                try
                {
                    cam.CaptureImage();
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        txtMessage.Text = ex.Message;
                    });
                }
            }
        }

        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            photoCounter++;
            //string fileName = photoCounter + ".jpg";
            //Deployment.Current.Dispatcher.BeginInvoke(delegate()
            //{
            //    txtMessage.Text = "Capture image available, saving picture";
            //});

            //library.SavePictureToCameraRoll(fileName, e.ImageStream);

            ////Deployment.Current.Dispatcher.BeginInvoke(delegate()
            ////{
            ////    txtMessage.Text = "Picture has been saved to camera roll";
            ////});


            byte[] cam = new byte[(int)e.ImageStream.Length];
            e.ImageStream.Read(cam, 0, (int)e.ImageStream.Length);

            //string stringCamByte = string.Empty;
            //for (int i = 0; i < cam.Length; i++)
            //{
            //    stringCamByte += cam[i] + ",";
            //}
            ImageCap imageData = new ImageCap()
            {
                DateTime = DateTime.Now,
                DriverID = 1,
                Latitude = 0.0,
                Longitude = 0.0,
                VehicleName = "fleet_1",
                Thumbnail = cam
            };
            service.UploadImageCaptureAsync(imageData);
        }
        #endregion
    }
        
}