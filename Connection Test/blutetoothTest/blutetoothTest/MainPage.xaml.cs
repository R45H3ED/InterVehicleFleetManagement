using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using blutetoothTest.Resources;
//MainPage.xaml.cs
using Windows.Networking.Proximity;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading;

namespace blutetoothTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;
        }

        void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            Connect(args.PeerInformation);
        }

        async void Connect(PeerInformation peerToConnect)
        {
            StreamSocket socket = await PeerFinder.ConnectAsync(peerToConnect);
        }

        private async void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            var peerList = await PeerFinder.FindAllPeersAsync();

            if (peerList.Count > 0)
            {
                PeerInformation pInfo = peerList[lb.SelectedIndex];

                App.Socket = new StreamSocket();

                await App.Socket.ConnectAsync(pInfo.HostName, "1");
            }
        }

        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            var dataBuffer = GetBufferFromByteArray(GetBytes(txtATcmd.Text + "\r"));
            //var dataBuffer = GenerateData("ati");
            uint x = await App.Socket.OutputStream.WriteAsync(dataBuffer);
        }

        private async void buttonReceive_Click(object sender, RoutedEventArgs e)
        {
            var buffer = new Windows.Storage.Streams.Buffer(1024);
            var readBytes = await App.Socket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);

            GetString(GetByteArrayFromBuffer(buffer));
            //txtMsg.Text += GetString(GetByteArrayFromBuffer(buffer));
        }

        public byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public string[] GetString(byte[] bytes)
        {
            string[] cleanVal = new string[0];
            string result = System.Text.Encoding.UTF8.GetString(bytes,0, bytes.Length);

            //char[] chars = new char[bytes.Length / sizeof(char)];
            //System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            //return new string(chars);

            result.Replace("\n", "");
            char[] delimiter = new char[] { '\r', '\n', '\0', '>' };
            cleanVal = result.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            MessageBox.Show(String.Join(Environment.NewLine, cleanVal));

            return cleanVal;
        }

        private IBuffer GetBufferFromByteArray(byte[] package)
        {
            using (DataWriter dw = new DataWriter())
            {
                dw.WriteBytes(package);
                return dw.DetachBuffer();
            }
        }

        private IBuffer GenerateData(string msg)
        {
            using (var dataWriter = new DataWriter())
            {
                dataWriter.UnicodeEncoding = UnicodeEncoding.Utf8;
                dataWriter.WriteString(msg);
                return dataWriter.DetachBuffer();
            }
        }

        private byte[] GetByteArrayFromBuffer(IBuffer buffer)
        {
            using (var dr = DataReader.FromBuffer(buffer))
            {
                var byteArr = new byte[buffer.Capacity];
                for (int i = 0; i < byteArr.Length; i++)
                {
                    try
                    {
                        byteArr[i] = dr.ReadByte();
                    }
                    catch
                    { break; }
                }
                return byteArr;
            }
        }

        private async void buttonDiscoverDevices_Click(object sender, RoutedEventArgs e)
        {
            PeerFinder.AlternateIdentities["Bluetooth:Paired"] = "";
            var peerList = await PeerFinder.FindAllPeersAsync();
            lb.Items.Clear();
            if (peerList.Count > 0)
            {
                for (int i = 0; i < peerList.Count; i++)
                {
                    if (peerList[i].DisplayName.Contains("OBDII"))
                    {
                        ListBoxItem lbi = new ListBoxItem() { Content = peerList[i].DisplayName };
                        lb.Items.Add(lbi);
                    }
                }
            }
            else
            {
                MessageBox.Show("No active peers");
                btn_Connect.IsEnabled = false;
            }
        }

        private void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_Connect.IsEnabled = true;
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}