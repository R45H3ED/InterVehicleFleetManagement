using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebService
{
    public class ImageCap
    {
        public int ImageID { get; set; }
        public int DriverID { get; set; }
        public string VehicleName { get; set; }
        public DateTime DateTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public byte[] Thumbnail { get; set; }
        public int InterVechID { get; set; }
        //public string ImageArr { get; set; }

    }
}