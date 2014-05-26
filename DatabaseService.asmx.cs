using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebService
{
    /// <summary>
    /// Summary description for DatabaseService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class DatabaseService : System.Web.Services.WebService
    {
        SqlConnection sqlcon = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["WebService.Properties.Settings.Online"].ToString());

          [WebMethod]
        public List<ImageCap> GetListOfImageCapture(DateTime dateTime)
        {
            List<ImageCap> listOfImages = new List<ImageCap>();
            sqlcon.Open();
            SqlCommand com = new SqlCommand("SELECT * FROM ImageCapture WHERE DateTime='" + dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "'", sqlcon);
            SqlDataReader sr = com.ExecuteReader();

            while (sr.Read())
            {
                ImageCap images = new ImageCap()
                {
                    ImageID = sr.GetInt32(0),
                    DriverID = sr.GetInt32(1),
                    VehicleName = sr.GetString(2),
                    DateTime = sr.GetDateTime(3),
                    Latitude = sr.GetDouble(4),
                    Longitude = sr.GetDouble(5)
                };
                int bufferSize = 1024;
                long dataSize = sr.GetBytes(6, 0, null, 0, int.MaxValue);
                byte[] buf = new byte[dataSize];
                int startIndex = 0;
                long retval = sr.GetBytes(6, startIndex, buf, 0, bufferSize);

                while (retval == bufferSize)
                {
                    startIndex += bufferSize;
                    retval = sr.GetBytes(6, startIndex, buf, startIndex, bufferSize);//throws IndexOutOfRangeException
                }

                var jpegQuality = 25;
                Image image;
                using (var inputStream = new MemoryStream(buf))
                {
                    image = Image.FromStream(inputStream);
                    var jpegEncoder = ImageCodecInfo.GetImageDecoders()
                      .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);
                    //Byte[] outputBytes;
                    using (var outputStream = new MemoryStream())
                    {
                        image.Save(outputStream, jpegEncoder, encoderParameters);
                        images.Thumbnail = outputStream.ToArray();
                    }
                }

                //images.Thumbnail = buf;
                listOfImages.Add(images);
            }

            sqlcon.Close();
            return listOfImages;
        }


        [WebMethod]
        public bool UploadImageCapture(ImageCap ImageData)
        {
            bool UploadSuccess = false;
            //Stream image;
  
            //for (int j = 0; j < ImageData.ImageArr.Length; j++)
            //{
            //    if (j != ImageData.ImageArr.Length)
            //    {
            //        image. += ImageData.ImageArr[j].ToString() + ",";
            //    }
            //}

            sqlcon.Open();
            using (SqlCommand com = new SqlCommand("INSERT INTO ImageCapture (DriverID, VehicleName, DateTime, Latitude, Longitude, Thumbnail) values ('" + ImageData.DriverID + "' ,'" + ImageData.VehicleName + "' ,'" + ImageData.DateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "' ,'" + ImageData.Latitude + "' ,'" + ImageData.Longitude + "','" + "' ,@Thumbnail )", sqlcon))            
            {
                com.Parameters.Add(new SqlParameter("Thumbnail", ImageData.Thumbnail));
                int result = com.ExecuteNonQuery();
                sqlcon.Close();
                if (result == 1)
                {
                    UploadSuccess = true;
                }
            } 
            return UploadSuccess;
            //int i = com.ExecuteNonQuery();
            

            //if (i != 0)
            //    UploadSuccess = true;
            //return UploadSuccess;

            //string Image;
            //if (Image == null)
            //{
            //    return UploadSuccess;
            //}
            //try
            //{
            //    FileStream fs = new FileStream(@Image, FileMode.Open, FileAccess.Read);
            //    //initialize the byte array
            //    byte[] imgByteArray = new byte[fs.Length];
            //    //read the data from file and put into bytearray
            //    fs.Read(imgByteArray, 0, Convert.ToInt32(fs.Length));
            //    fs.Close();

            //    //save the binary data in the database 
            //}

            
        }


        
    }
}
