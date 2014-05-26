using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageGet.service;

namespace ImageGet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DatabaseServiceSoapClient service = new DatabaseServiceSoapClient();
        List<ImageCap> listOfImages;

        public MainWindow()
        {
            InitializeComponent();
            service.GetListOfImageCaptureCompleted += service_GetListOfImageCaptureCompleted;

            Loaded += MainWindow_Loaded;
        }

        void service_GetListOfImageCaptureCompleted(object sender, GetListOfImageCaptureCompletedEventArgs e)
        {
            for (int i = 0; i < e.Result.Count; i++)
            {
                combo1.Items.Add(e.Result[i].DateTime.ToString("h:mm:ss d/MMM/yyyy"));
            }
            listOfImages = e.Result;
           // image.Source = GetBitmapImage(e.Result[0].Thumbnail);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            service.GetListOfImageCaptureAsync(1, "fleet_1");
        }

        public BitmapImage GetBitmapImage(byte[] imageBytes)
        {
            var bitmapImage = new BitmapImage();

            try
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(imageBytes);
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return bitmapImage;
        }

        private void combo1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            image.Source = GetBitmapImage(listOfImages[(sender as ComboBox).SelectedIndex].Thumbnail);
        }
    }
}
