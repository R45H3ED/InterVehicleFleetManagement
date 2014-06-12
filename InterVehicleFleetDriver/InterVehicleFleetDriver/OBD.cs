using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
//using System.IO.Ports;

using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
//using sdkBluetoothA2DWP8CS.Resources;
//using Windows.Networking.Proximity;
//using Windows.Networking.Sockets;


//find bluetooth resource!

namespace InterVehicleFleetDriver
{
    class OBD
    {
        #region Variables

        SerialPort SerialPortOBD = new SerialPort();
        DispatcherTimer dispatcherTimerOBD = new DispatcherTimer(); //check to see if the connection is still ok
        Thread sequenceThread;                                      //puts all the information in new thread CPU so doesn't clog UI
        int countMPG_average;                                       //average MPG
        double tempMPG_average;

        public PIDs PIDs { get; private set; }

        public string selected_COM { get; set; }               //needs Com value to connect to port
        public int selected_BaudRate { get; set; }          //default baudrate speed for bluetooth
        public bool isSerialPortOpen { get; private set; } //check serial port open
        public string[] Status { get; private set; }        

        public bool isInitializationComplete { get; private set; }  //check initialization complete
        public string _PROTOCOL { get; private set; }   //OBD protocol set to the unit
        public string _OBD_STANDARD { get; private set; }        //value returned for countries region

        public double _SPEEDKMH { get; set; }
        public int _SPEEDMPH { get; set; }
        public int _RPM { get; set; }
        public int _COOLANT_TEMP { get; set; }
        public int _FUEL_LEVEL { get; set; }
        public int _OIL_TEMP { get; set; }
        public double _ODOMETER { get; set; }
        public string _VOLTAGE { get; set; }
        public string _IGNITION { get; set; }
        public int _THROTTLE_POSITION { get; set; }
        public int _AIR_INTAKE_TEMP { get; set; }
        public int _AMBIENT_AIR { get; set; }
        public int _MANIFOLD_PRESSURE { get; set; }
        public int _FUEL_PRESSURE { get; set; }
        public double _SHORT_TERM_FUEL_TRIM_1 { get; set; }
        public double _SHORT_TERM_FUEL_TRIM_2 { get; set; }
        public double _LONG_TERM_FUEL_TRIM_1 { get; set; }
        public double _LONG_TERM_FUEL_TRIM_2 { get; set; }
        public int _MAF { get; set; }
        public int _ENGINE_LOAD { get; set; }
        public double _TIMING { get; set; }
        public double _MPG { get; set; }
        public double _MPG_AVERAGE { get; set; }
        public TimeSpan _RUN_TIME { get; set; }
        public string _VIN { get; private set; }
        public string[] _FUEL_TYPE { get; private set; }
        public List<string> _DTC { get; private set; }      //actual code
        public bool _MIL { get; private set; }          //check engine light
        public int _NumDTC { get; private set; }        //number of displayed trouble codes

        public bool get_PID_ATRV { get; set; }          //hex values for each statistic
        public bool get_PID_ATIGN { get; set; }         //
        public bool get_PID_0104 { get; set; }
        public bool get_PID_0105 { get; set; }
        public bool get_PID_0106 { get; set; }
        public bool get_PID_0107 { get; set; }
        public bool get_PID_0108 { get; set; }
        public bool get_PID_0109 { get; set; }
        public bool get_PID_010A { get; set; }
        public bool get_PID_010B { get; set; }
        public bool get_PID_010C { get; set; }
        public bool get_PID_010D { get; set; }
        public bool get_PID_010E { get; set; }
        public bool get_PID_010F { get; set; }
        public bool get_PID_0110 { get; set; }
        public bool get_PID_0111 { get; set; }
        public bool get_PID_011F { get; set; }
        public bool get_PID_012F { get; set; }
        public bool get_PID_0146 { get; set; }
        public bool get_PID_015C { get; set; }
        bool datareceivedOBD = false;

        bool isATZ;     //reset
        bool isATE;     //echo
        bool isATL;     //line feed
        bool isATSP;    //set protocol
        
        int slowerECU = 0;      //depending on car type (if something cannot be read fast enough, increase time till it can be read
        bool isLogging = false; //detects that i want all the values

        string HexVIN;

        #endregion

        public OBD()
        {
            #region Intialization
            //create new thread
            sequenceThread = new Thread(new ThreadStart(BackgroundThread));

            dispatcherTimerOBD.Tick += new EventHandler(dispatcherTimerOBD_Tick);
            dispatcherTimerOBD.Interval = new TimeSpan(0, 0, 0, 0, 999);    //timer ticks every 999 seconds
            SerialPortOBD.DataReceived += new SerialDataReceivedEventHandler(SerialPortOBD_DataReceived); //new data from serial port captured here

            get_PID_0104 = true;
            get_PID_0105 = true;
            get_PID_0106 = false;
            get_PID_0107 = false;
            get_PID_0108 = false;
            get_PID_0109 = false;
            get_PID_010A = true;
            get_PID_010B = true;
            get_PID_010C = true;
            get_PID_010D = true;
            get_PID_010E = true;
            get_PID_010F = true;
            get_PID_0110 = true;
            get_PID_0111 = true;
            get_PID_011F = true;
            get_PID_012F = true;
            get_PID_0146 = true;
            get_PID_015C = true;
            get_PID_ATIGN = true;
            get_PID_ATRV = true;

            Status = new string[] { "", "#FF049EB2" };      //serial port status at any given time
            selected_BaudRate = 38400;                      //string, colour of string

            #endregion
        }       

        // Processes stuff
        private void dispatcherTimerOBD_Tick(object sender, EventArgs e)
        {
            if (SerialPortOBD.IsOpen)
            {
                if (isLogging)
                {
                    if (!sequenceThread.IsAlive)
                    {
                        sequenceThread = new Thread(new ThreadStart(BackgroundThread));
                        sequenceThread.Start();
                    //if serial port open, logging enabled new thread starts if not already started and data begins to flow
                    }                    
                }

                GetMPG();
                GetOdometer();

                if (datareceivedOBD)
                {
                    Status = new string[] { "Recieving...", "#FF049EB2" };

                    datareceivedOBD = false;
                }
                else
                {
                    Status = new string[] { "Ready...", "#FFEBA942" };
                }
            }
            else
            {
                Status = new string[] { "Error", "#FFB20404" };
            }
        }        
        
        // Serial port communication
        #region Serial Port

        // Set serial port configurations
        public void SerialPortOBD_Config()
        {
            SerialPortOBD.PortName = selected_COM;
            SerialPortOBD.BaudRate = selected_BaudRate;
            SerialPortOBD.DataBits = 8;
            SerialPortOBD.DiscardNull = false;
            SerialPortOBD.DtrEnable = false;
            SerialPortOBD.Handshake = Handshake.None;
            SerialPortOBD.Parity = Parity.None;
            SerialPortOBD.ParityReplace = 63;
            SerialPortOBD.ReadBufferSize = 4096;
            SerialPortOBD.ReadTimeout = 150;
            SerialPortOBD.ReceivedBytesThreshold = 1;
            SerialPortOBD.RtsEnable = false;
            SerialPortOBD.StopBits = StopBits.One;
            SerialPortOBD.WriteBufferSize = 2048;
            SerialPortOBD.WriteTimeout = -1;
        }

        // Open the serial port
        public void SerialPortOBD_Open()
        {
            if (!SerialPortOBD.IsOpen)
            {
                SerialPortOBD.Open();
                isSerialPortOpen = SerialPortOBD.IsOpen;
                dispatcherTimerOBD.Start();
            }
        }

        // Close the serial port
        public void SerialPortOBD_Close()
        {
            if (SerialPortOBD.IsOpen)
            {
                SerialPortOBD.Close();
                isSerialPortOpen = SerialPortOBD.IsOpen;
                dispatcherTimerOBD.Stop();
            }
        }

        // Send and receive commands 
        private string[] WritePID(string PID, bool delay_read)
        {
            SystemConsole("<<< SENDING <<< " + PID, true);
            SerialPortOBD.Write(PID + "\r");
            if (delay_read)
            {
                Thread.Sleep(1000);
            }
            else
            {
                Thread.Sleep(slowerECU);
            }
            bool cont = true;
            string retVal = string.Empty;
            while (cont)
            {
                retVal += SerialPortOBD.ReadExisting(); //data added to string
                if (retVal.Contains(">"))
                {
                    cont = false;   //keep reading from serial port until value from OBD is '>'
                }
            }
            retVal.Replace("\n", "");                   //if it wants a new line, replace it with text instead
            char[] delimiters = new char[] { '\r', '\n', '>' }; //any special characters get rid of
            string[] cleanVal = retVal.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (cleanVal.Contains("STOPPED"))
            {
                slowerECU += 10;
            }

            SystemConsole(">>> RECEIVING >>> " + String.Join(Environment.NewLine, cleanVal), true);

            return cleanVal;
        }

        // When serial port receives data
        private void SerialPortOBD_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            datareceivedOBD = true; //changes data received to true
        }

        #endregion        

        // ELM327 related communication commands
        #region ELM

        // Initialises the ELM for OBD bus information transfer
        public void InitializeELM()
        {
            if (SerialPortOBD.IsOpen)
            {
                MessageBox.Show("OBD is being initialized, this might take a few seconds...", "Starting Initialization", MessageBoxButton.OK, MessageBoxImage.Information);

                try
                {
                    isInitializationComplete = false;

                    ResetOBD();
                    SetEcho(false);
                    SetLinefeed(false);
                    SetProtocolOBD(0);

                    if (isATZ && isATE && isATL && isATSP)
                    {                        
                        isInitializationComplete = true;
                    }
                    else
                    {
                        if (MessageBox.Show("Could not initialise OBD, try again?", "Initialisation Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                        {
                            InitializeELM();
                        }
                    }

                    if (isInitializationComplete)
                    {
                        SupportedPIDs(); //pids= processid 
                            GetVIN();
                            GetFuelType();
                            GetMIL(true);
                            OBD_Standards();
                        MessageBox.Show("OBD initialisation is complete", "Initialization Complete", MessageBoxButton.OK, MessageBoxImage.Information);                        
                    }
                }
                catch
                {
                    if (MessageBox.Show("Could not initialise OBD, try again?", "Initialisation Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    {
                        InitializeELM();
                    }
                }
            }
        }

        // ATRV
        // Get vehicle voltage level
        public string GetVoltage(bool Request)
        {
            if (Request)
            {
                string[] voltArr = WritePID("ATRV", false);
                string volt = Array.FindLast(voltArr, element => (element.Contains(".") && (element.Contains("V"))));
                if (volt != null)
                {
                    if (volt.EndsWith("V"))
                    {
                        _VOLTAGE = volt;
                    }
                }
                else
                {
                    _VOLTAGE = "NO DATA";
                    get_PID_ATIGN = false;
                }
            }
            else
            {
                _VOLTAGE = "N/A";
            }

            return _VOLTAGE;
        }

        // ATIGN
        // Get vehicle ignition position
        public string GetIgnition(bool Request)
        {
            if (Request)
            {
                string[] ignArr = WritePID("ATIGN", false);

                string ignON = Array.FindLast(ignArr, element => element.Contains("ON"));
                string ignOFF = Array.FindLast(ignArr, element => element.Contains("OFF"));
                if (ignON != null)
                {
                    _IGNITION = ignON;
                }
                if (ignOFF != null)
                {
                    _IGNITION = ignOFF;
                }
                if ((ignON == null) && (ignOFF == null))
                {
                    _IGNITION = "NO DATA";
                    get_PID_ATIGN = false;
                }
            }
            else
            {
                _IGNITION = "N/A";
            }
            return _IGNITION;
        }

        // ATZ
        // ELM Reset
        public void ResetOBD()
        {
            isATZ = false;
            try
            {
                Status = new string[] { "Resetting ELM", "#FFEBA942" };
                string[] atz = WritePID("ATZ", true);
                string _atz = Array.Find(atz, element => element.Contains("ELM"));
                if (_atz != null)
                {
                    SystemConsole(_atz, true);
                    isATZ = true;
                    slowerECU = 0;
                }
            }
            catch
            {
                MessageBox.Show("Cannot reset OBD", "Reset", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        // ATE
        // ELM Echo
        public void SetEcho(bool enable)
        {
            isATE = false;
            string stringEnable = string.Empty;
            string statusEnable = string.Empty;
            if (enable)
            {
                stringEnable = "1";
                statusEnable = "Enabling";
            }
            else
            {
                stringEnable = "0";
                statusEnable = "Disabling";
            }
            try
            {
                Status = new string[] { statusEnable + " echo", "#FFEBA942" };
                string[] ate = WritePID("ATE" + stringEnable, true);
                string _ate = Array.Find(ate, element => element.Contains("OK"));
                if (_ate != null)
                {
                    SystemConsole(_ate, true);
                    isATE = true;
                }
            }
            catch
            {
                MessageBox.Show("Cannot disable echo", "Echo Off", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        // ATL
        // ELM Line feeds
        public void SetLinefeed(bool enable)
        {
            isATL = false;
            string stringEnable = string.Empty;
            string statusEnable = string.Empty;
            if (enable)
            {
                stringEnable = "1";
                statusEnable = "Enabling";
            }
            else
            {
                stringEnable = "0";
                statusEnable = "Disabling";
            }
            try
            {
                Status = new string[] { statusEnable + " linefeed", "#FFEBA942" };
                string[] atl = WritePID("ATL" + stringEnable, true);
                string _atl = Array.Find(atl, element => element.Contains("OK"));
                if (_atl != null)
                {

                    SystemConsole(_atl, true);
                    isATL = true;
                }
            }
            catch
            {
                MessageBox.Show("Cannot disable linefeed", "Linefeed Off", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        // ATSP
        // Set protocol for ELM327 
        public string SetProtocolOBD(int protocol)
        {
            isATSP = false;
            try
            {
                Status = new string[] { "Setting protocol", "#FFEBA942" };
                string[] atsp = WritePID("ATSP" + protocol.ToString(), true);
                string _atsp = Array.Find(atsp, element => element.Contains("OK"));
                if (_atsp != null)
                {
                    isATSP = true;
                }
                if (isATSP)
                {
                    string[] currentprotocol = WritePID("ATDP", true);
                    _PROTOCOL = currentprotocol[0];

                    if (currentprotocol == null)
                    {
                        string[] currentprotocolnum = WritePID("ATDPN", true);
                        int protoNum = Convert.ToInt32(currentprotocolnum[0]);
                        switch (protoNum)
                        {
                            case 0: _PROTOCOL = "Automatic"; break;
                            case 1: _PROTOCOL = "SAE J1850 PWM (41.6 kbaud)"; break;
                            case 2: _PROTOCOL = "SAE J1850 VPW (10.4 kbaud)"; break;
                            case 3: _PROTOCOL = "ISO 9141-2 (5 baud init)"; break;
                            case 4: _PROTOCOL = "ISO 14230-4 KWP (5 baud init)"; break;
                            case 5: _PROTOCOL = "ISO 14230-4 KWP (fast init)"; break;
                            case 6: _PROTOCOL = "ISO 15765-4 CAN (11 bit ID, 500 kbaud"; break;
                            case 7: _PROTOCOL = "ISO 15765-4 CAN (29 bit ID, 500 kbaud"; break;
                            case 8: _PROTOCOL = "ISO 15765-4 CAN (11 bit ID, 250 kbaud"; break;
                            case 9: _PROTOCOL = "ISO 15765-4 CAN (29 bit ID, 250 kbaud"; break;
                            case 10: _PROTOCOL = "SAE J1939 CAN (29 bit ID, 250 kbaud"; break;
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Cannot set OBD protocol", "Protocol", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            return _PROTOCOL;
        }

        #endregion

        // OBD-II PIDs (On-board diagnostics Parameter IDs) used to request data from a vehicle
        #region PIDs

        // Gets alist of supported PIDs 
        public List<string> SupportedPIDs()
        {
            List<string> SupportedPIDs = new List<string>();

            string[] at0100 = WritePID("0100", true);
            string _at0100 = Array.Find(at0100, element => element.Contains("41 00"));
            if (_at0100 != null)
            {
                SupportedPIDs.Add(_at0100 + " ---> PIDs supported [01 - 20]");
            }
            string[] at0120 = WritePID("0120", true);
            string _at0120 = Array.Find(at0120, element => element.Contains("41 20"));
            if (_at0120 != null)
            {
                SupportedPIDs.Add(_at0120 + " ---> PIDs supported [21 - 40]");
            }
            string[] at0140 = WritePID("0140", true);
            string _at0140 = Array.Find(at0140, element => element.Contains("41 40"));
            if (_at0140 != null)
            {
                SupportedPIDs.Add(_at0140 + " ---> PIDs supported [41 - 60]");
            }
            string[] at0160 = WritePID("0160", true);
            string _at0160 = Array.Find(at0160, element => element.Contains("41 60"));
            if (_at0160 != null)
            {
                SupportedPIDs.Add(_at0160 + " ---> PIDs supported [61 - 80]");
            }
            string[] at0180 = WritePID("0180", true);
            string _at0180 = Array.Find(at0180, element => element.Contains("41 80"));
            if (_at0180 != null)
            {
                SupportedPIDs.Add(_at0180 + " ---> PIDs supported [81 - 100]");
            }

            return SupportedPIDs;
        }        

        #region $01. Show current data
        
        // 0101
        // Monitor status since DTCs cleared. (Includes malfunction indicator lamp (MIL) status and number of DTCs.)
        public bool GetMIL(bool onlyLight)
        {
            if (_RPM == 0)
            {
                string[] resultArr = WritePID("0101", true);
                string result = Array.Find(resultArr, element => element.StartsWith("41 01", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');
                    _NumDTC = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    if (_NumDTC > 0)
                    {
                        _MIL = true;
                        if (!onlyLight)
                        {
                            GetTroubleCodes();
                        }
                    }
                    else
                    {
                        _MIL = false;
                        if (!onlyLight)
                        {
                            MessageBox.Show("There are no trouble codes for this vehicle", "DTC", MessageBoxButton.OK, MessageBoxImage.None);
                        }
                    }
                }
                else
                {
                    _MIL = false;
                }
            }
            else
            {
                MessageBox.Show("Trouble codes can only be checked if the vehicle if not running, please turn off the vehicle before trying again.", "Cannot continue", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            SystemConsole("MIL - " + _MIL + " - #" + _NumDTC, true);
            return _MIL;
        }

        // 0104
        // Calculated engine load value
        public int GetEngineLoad(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("0104", false);

                string result = Array.Find(resultArr, element => element.StartsWith("41 04", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int engineLoad = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    _ENGINE_LOAD = (engineLoad * 100) / 255;
                    SystemConsole("Engine Load - " + _ENGINE_LOAD.ToString() + "%", true);
                }
                else
                {
                    get_PID_0104 = false;
                }
            }
            else
            {
                _ENGINE_LOAD = -999;
            }
            return _ENGINE_LOAD;
        }

        // 0105
        // Engine coolant temperature
        public int GetCoolantTemperature(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("0105", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 05", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int coolantTemp = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    _COOLANT_TEMP = coolantTemp - 40;
                    SystemConsole("Coolant Temperature - " + _COOLANT_TEMP.ToString() + "°C", true);
                }
                else
                {
                    get_PID_0105 = false;
                }
            }
            else
            {
                _COOLANT_TEMP = -999;
            }
            return _COOLANT_TEMP;
        }

        // 0106
        // Short term fuel % trim—Bank 1
        public double GetShortTermFuelTrimBank1(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("0106", false);

                string result = Array.Find(resultArr, element => element.StartsWith("41 06", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] dataArr = result.Split(' ');

                    int data = Int32.Parse((dataArr[2]), System.Globalization.NumberStyles.HexNumber);
                    _SHORT_TERM_FUEL_TRIM_1 = (data - 128) * 100 / 128;
                    SystemConsole("Short term fuel - " + _SHORT_TERM_FUEL_TRIM_1.ToString() + "% trim—Bank 1", true);
                }
                else
                {
                    get_PID_0106 = false;
                }
            }
            else
            {
                _SHORT_TERM_FUEL_TRIM_1 = -999;
            }
            return _SHORT_TERM_FUEL_TRIM_1;
        }

        // 0107
        // Long term fuel % trim—Bank 1
        public double GetLongTermFuelTrimBank1(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("0107", false);

                string result = Array.Find(resultArr, element => element.StartsWith("41 07", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] dataArr = result.Split(' ');

                    int data = Int32.Parse((dataArr[2]), System.Globalization.NumberStyles.HexNumber);
                    _LONG_TERM_FUEL_TRIM_1 = (data - 128) * 100 / 128;
                    SystemConsole("Long term fuel - " + _LONG_TERM_FUEL_TRIM_1.ToString() + "% trim—Bank 1", true);
                }
                else
                {
                    get_PID_0107 = false;
                }
            }
            else
            {
                _LONG_TERM_FUEL_TRIM_1 = -999;
            }
            return _LONG_TERM_FUEL_TRIM_1;
        }

        // 0108
        // Short term fuel % trim—Bank 2
        public double GetShortTermFuelTrimBank2(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("0108", false);

                string result = Array.Find(resultArr, element => element.StartsWith("41 08", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] dataArr = result.Split(' ');

                    int data = Int32.Parse((dataArr[2]), System.Globalization.NumberStyles.HexNumber);
                    _SHORT_TERM_FUEL_TRIM_2 = (data - 128) * 100 / 128;
                    SystemConsole("Short term fuel - " + _SHORT_TERM_FUEL_TRIM_2.ToString() + "% trim—Bank 2", true);
                }
                else
                {
                    get_PID_0108 = false;
                }
            }
            else
            {
                _SHORT_TERM_FUEL_TRIM_2 = -999;
            }
            return _SHORT_TERM_FUEL_TRIM_2;
        }

        // 0109
        // Long term fuel % trim—Bank 2
        public double GetLongTermFuelTrimBank2(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("0109", false);

                string result = Array.Find(resultArr, element => element.StartsWith("41 09", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] dataArr = result.Split(' ');

                    int data = Int32.Parse((dataArr[2]), System.Globalization.NumberStyles.HexNumber);
                    _LONG_TERM_FUEL_TRIM_2 = (data - 128) * 100 / 128;
                    SystemConsole("Long term fuel - " + _LONG_TERM_FUEL_TRIM_2.ToString() + "% trim—Bank 2", true);
                }
                else
                {
                    get_PID_0109 = false;
                }
            }
            else
            {
                _LONG_TERM_FUEL_TRIM_2 = -999;
            }
            return _LONG_TERM_FUEL_TRIM_2;
        }

        // 010A
        // Fuel pressure
        public int GetFuelPressure(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("010A", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 0A", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int fuelPressure = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    _FUEL_PRESSURE = fuelPressure * 3;
                    SystemConsole("Fuel Pressure - " + _FUEL_PRESSURE.ToString() + "kPa", true);
                }
                else
                {
                    get_PID_010A = false;
                }
            }
            else
            {
                _FUEL_PRESSURE = -999;
            }
            return _FUEL_PRESSURE;
        }

        // 010B
        // Intake manifold absolute pressure
        public int GetIntakeManifoldPressure(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("010B", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 0B", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    _MANIFOLD_PRESSURE = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    SystemConsole("Intake Manifold Pressure - " + _MANIFOLD_PRESSURE.ToString() + "kPa", true);
                }
                else
                {
                    get_PID_010B = false;
                }
            }
            else
            {
                _MANIFOLD_PRESSURE = -999;
            }
            return _MANIFOLD_PRESSURE;
        }

        // 010C
        // Engine RPM
        public int GetRPM(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("010C", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 0C", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int rpm1 = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    int rpm2 = Int32.Parse((data[3]), System.Globalization.NumberStyles.HexNumber);
                    _RPM = ((rpm1 * 256) + rpm2) / 4;
                    SystemConsole("RPM - " + _RPM.ToString(), true);
                }
                else
                {
                    get_PID_010C = false;
                }
            }
            else
            {
                _RPM = -999;
            }
            return _RPM;
        }

        // 010D
        // Vehicle speed
        public int GetSpeed(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("010D", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 0D", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');
                    _SPEEDKMH = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    double mph = _SPEEDKMH * 0.621371;
                    _SPEEDMPH = Convert.ToInt32(mph);
                    SystemConsole("Speed - " + _SPEEDMPH.ToString() + "MPH", true);
                }
                else
                {
                    get_PID_010D = false;
                }
            }
            else
            {
                _SPEEDKMH = -999;
                _SPEEDMPH = -999;
            }
            return _SPEEDMPH;
        }

        // 010E
        // Timing advance
        public double GetTimingAdvance(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("010E", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 0E", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    double ta = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    _TIMING = (ta / 2) - 64;
                    SystemConsole("Timing Advance - " + _TIMING + "°", true);
                }
                else
                {
                    get_PID_010E = false;
                }
            }
            else
            {
                _TIMING = -999;
            }
            return _TIMING;
        }

        // 010F
        // Intake air temperature
        public int GetIntakeAirTemperature(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("010F", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 0F", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int intakeAirTemp = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    _AIR_INTAKE_TEMP = intakeAirTemp - 40;
                    SystemConsole("Intake Air Temperature - " + _AIR_INTAKE_TEMP.ToString() + "°C", true);
                }
                else
                {
                    get_PID_010F = false;
                }
            }
            else
            {
                _AIR_INTAKE_TEMP = -999;
            }
            return _AIR_INTAKE_TEMP;
        }

        // 0110
        // MAF air flow rate
        public int GetMAF(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("0110", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 10", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int maf1 = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    int maf2 = Int32.Parse((data[3]), System.Globalization.NumberStyles.HexNumber);
                    _MAF = ((maf1 * 256) + maf2) / 100;
                    SystemConsole("MAF - " + _MAF.ToString() + " grams/sec", true);
                }
                else
                {
                    get_PID_0110 = false;
                }
            }
            else
            {
                _MAF = -999;
            }
            return _MAF;
        }

        // 0111
        // Throttle position
        public int GetThrottlePosition(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("0111", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 11", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int throttlePosition = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    _THROTTLE_POSITION = (throttlePosition * 100) / 255;
                    SystemConsole("Throttle Position - " + _THROTTLE_POSITION.ToString() + "%", true);
                }
                else
                {
                    get_PID_0111 = false;
                }
            }
            else
            {
                _THROTTLE_POSITION = -999;
            }
            return _THROTTLE_POSITION;
        }

        // 011C
        // OBD standards this vehicle conforms to
        public string OBD_Standards()
        {
            string[] resultArr = WritePID("011C", false);
            string result = Array.Find(resultArr, element => element.StartsWith("41 1C", StringComparison.Ordinal));
            if (result != null)
            {
                string[] data = result.Split(' ');

                int standard = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                switch (standard)
                {
                    case 1: _OBD_STANDARD = "OBD-II as defined by the CARB"; break;
                    case 2: _OBD_STANDARD = "OBD as defined by the EPA"; break;
                    case 3: _OBD_STANDARD = "OBD and OBD-II"; break;
                    case 4: _OBD_STANDARD = "OBD-I"; break;
                    case 5: _OBD_STANDARD = "Not meant to comply with any OBD standard"; break;
                    case 6: _OBD_STANDARD = "EOBD (Europe)"; break;
                    case 7: _OBD_STANDARD = "EOBD and OBD-II (Europe)"; break;
                    case 8: _OBD_STANDARD = "EOBD and OBD (Europe)"; break;
                    case 9: _OBD_STANDARD = "EOBD, OBD and OBD II (Europe)"; break;
                    case 10: _OBD_STANDARD = "JOBD (Japan)"; break;
                    case 11: _OBD_STANDARD = "JOBD and OBD II (Japan)"; break;
                    case 12: _OBD_STANDARD = "JOBD and EOBD (Japan)"; break;
                    case 13: _OBD_STANDARD = "JOBD, EOBD, and OBD II (Japan)"; break;
                }
                if (standard > 13)
                {
                    _OBD_STANDARD = "NO DATA";
                }
                else
                {
                    SystemConsole("OBD complies with - " + _OBD_STANDARD, true);
                }
            }

            return _OBD_STANDARD;
        }

        // 011F
        // Run time since engine start
        public TimeSpan GetEngineRunTime(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("011F", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 1F", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int time1 = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    int time2 = Int32.Parse((data[3]), System.Globalization.NumberStyles.HexNumber);
                    int time3 = ((time1 * 256) + time2);
                    _RUN_TIME = new TimeSpan(0, 0, time3);
                    SystemConsole("Current Running Time - " + _RUN_TIME.ToString(), true);
                }
                else
                {
                    get_PID_011F = false;
                }
            }
            else
            {
                _RUN_TIME = new TimeSpan(0, 0, 0);
            }
            return _RUN_TIME;
        }

        // 012F
        // Fuel Level Input
        public int GetFuelLevel(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("012F", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 2F", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int fuelLevel = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    _FUEL_LEVEL = (fuelLevel * 100) / 255;
                    SystemConsole("Fuel Level - " + _FUEL_LEVEL.ToString() + "%", true);
                }
                else
                {
                    get_PID_012F = false;
                }
            }
            else
            {
                _FUEL_LEVEL = -999;
            }
            return _FUEL_LEVEL;
        }

        // 0146
        // Ambient air temperature
        public int GetAmbientAirTemperature(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("0146", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 46", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int ambientTemp = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    _AMBIENT_AIR = ambientTemp - 40;
                    SystemConsole("Ambient Air Temperature - " + _AMBIENT_AIR.ToString() + "°C", true);
                }
                else
                {
                    get_PID_0146 = false;
                }
            }
            else
            {
                _AMBIENT_AIR = -999;
            }
            return _AMBIENT_AIR;
        }

        // 0151
        // Fuel Type
        public string[] GetFuelType()
        {
            string[] resultArr = WritePID("0151", true);
            string result = Array.Find(resultArr, element => element.StartsWith("41 51", StringComparison.Ordinal));
            if (result != null)
            {
                string[] data = result.Split(' ');

                int intFuelType = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                switch (intFuelType)
                {
                    case 1: _FUEL_TYPE = new string[] { "Gasoline", "14.7", "6.217" }; break;
                    case 2: _FUEL_TYPE = new string[] { "Methanol", "6.4", "7.935" }; break;
                    case 3: _FUEL_TYPE = new string[] { "Ethanol", "9.0", "7.907" }; break;
                    case 4: _FUEL_TYPE = new string[] { "Diesel", "14.6", "6.943" }; break;
                    case 5: _FUEL_TYPE = new string[] { "LPG", "15.5", "5.111" }; break;
                    case 6: _FUEL_TYPE = new string[] { "CNG" }; break;
                    case 7: _FUEL_TYPE = new string[] { "Propane" }; break;
                    case 8: _FUEL_TYPE = new string[] { "Electric" }; break;
                    case 9: _FUEL_TYPE = new string[] { "Bifuel running Gasoline" }; break;
                    case 10: _FUEL_TYPE = new string[] { "Bifuel running Methanol" }; break;
                    case 11: _FUEL_TYPE = new string[] { "Bifuel running Ethanol" }; break;
                    case 12: _FUEL_TYPE = new string[] { "Bifuel running LPG" }; break;
                    case 13: _FUEL_TYPE = new string[] { "Bifuel running CNG" }; break;
                    case 14: _FUEL_TYPE = new string[] { "Bifuel running Prop" }; break;
                    case 15: _FUEL_TYPE = new string[] { "Bifuel running Electricity" }; break;
                    case 16: _FUEL_TYPE = new string[] { "Bifuel mixed gas/electric" }; break;
                    case 17: _FUEL_TYPE = new string[] { "Hybrid gasoline" }; break;
                    case 18: _FUEL_TYPE = new string[] { "Hybrid Ethanol" }; break;
                    case 19: _FUEL_TYPE = new string[] { "Hybrid Diesel" }; break;
                    case 20: _FUEL_TYPE = new string[] { "Hybrid Electric" }; break;
                    case 21: _FUEL_TYPE = new string[] { "Hybrid Mixed fuel" }; break;
                    case 22: _FUEL_TYPE = new string[] { "Hybrid Regenerative" }; break;
                }
            }
            else
            {
                _FUEL_TYPE = new string[] { "NO DATA", "14.7", "6.217" };
            }

            SystemConsole(String.Join(", ", _FUEL_TYPE), true);
            return _FUEL_TYPE;
        }

        // 015C
        // Engine oil temperature
        public int GetEngineOilTemperature(bool Request)
        {
            if (Request)
            {
                string[] resultArr = WritePID("015C", false);
                string result = Array.Find(resultArr, element => element.StartsWith("41 5C", StringComparison.Ordinal));
                if (result != null)
                {
                    string[] data = result.Split(' ');

                    int engineOilTemp = Int32.Parse((data[2]), System.Globalization.NumberStyles.HexNumber);
                    _OIL_TEMP = engineOilTemp - 40;
                    SystemConsole("Engine Oil Temperature - " + _OIL_TEMP.ToString() + "°C", true);
                }
                else
                {
                    get_PID_015C = false;
                }
            }
            else
            {
                _OIL_TEMP = -999;
            }
            return _OIL_TEMP;
        }        

        #endregion

        #region $03. Show stored Diagnostic Trouble Codes

        /// 03
        /// Request trouble codes
        private List<string> GetTroubleCodes()
        {
            if (_MIL)
            {
                if (_DTC != null)
                {
                    _DTC.Clear();
                }

                if (MessageBox.Show("The scan has has found [" + _NumDTC + "] trouble code(s) for this vehicle, would you like to see the list of errors?", "DTC", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    string listDTC = string.Empty;
                    string[] dtcArr = WritePID("03", true);
                    string dtc = Array.Find(dtcArr, element => element.StartsWith("43", StringComparison.Ordinal));

                    if (dtc != null)
                    {
                        for (int i = 0; i < dtcArr.Length; i++)
                        {
                            string[] dtcLine = dtcArr[i].Split();
                            int hexDTC = Int32.Parse((dtcLine[3]), System.Globalization.NumberStyles.HexNumber);
                            string hexLetter = string.Empty;
                            switch (hexDTC)
                            {
                                case 1: hexLetter = "P0"; break;
                                case 2: hexLetter = "P1"; break;
                                case 3: hexLetter = "P2"; break;
                                case 4: hexLetter = "P3"; break;
                                case 5: hexLetter = "C0"; break;
                                case 6: hexLetter = "C1"; break;
                                case 7: hexLetter = "C2"; break;
                                case 8: hexLetter = "C3"; break;
                                case 9: hexLetter = "B0"; break;
                                case 10: hexLetter = "B1"; break;
                                case 11: hexLetter = "B2"; break;
                                case 12: hexLetter = "B3"; break;
                                case 13: hexLetter = "U0"; break;
                                case 14: hexLetter = "U1"; break;
                                case 15: hexLetter = "U2"; break;
                                case 16: hexLetter = "U3"; break;
                            }

                            string dtcCode = hexLetter + dtcLine[4] + dtcLine[6] + dtcLine[7];

                            _DTC.Add(dtcCode);
                            for (int x = 0; x < _DTC.Count; x++)
                            {
                                listDTC += _DTC[x] + Environment.NewLine;
                            }
                        }
                        if (MessageBox.Show(listDTC + Environment.NewLine + "Would you like to clear all trouble codes?", "DTC", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            ClearTroubleCodes();
                        }
                    }
                }
            }

            return _DTC;
        }

        #endregion

        #region $04. Clear Diagnostic Trouble Codes and stored values

        /// 04
        /// Clear trouble codes / Malfunction indicator lamp (MIL) / Check engine light
        public void ClearTroubleCodes()
        {
            if (_RPM == 0)
            {
                if (MessageBox.Show("Are you sure you want to clear all trouble codes?", "Clear DTC", MessageBoxButton.YesNo, MessageBoxImage.Hand) == MessageBoxResult.Yes)
                {
                    WritePID("04", false);
                    _MIL = false;
                    _NumDTC = 0;
                }
            }
            else
            {
                MessageBox.Show("Trouble codes can only be checked if the vehicle if not running, please turn off the vehicle before trying again.", "Cannot continue", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        #endregion

        #region $09. Request vehicle information

        // 0902
        // Vehicle identification number (VIN)
        public string GetVIN()
        {
            try
            {
                bool hexASCII = false;
                string[] retVIN = WritePID("0902", true);

                if (retVIN != null)
                {
                    string[] vinLine1;
                    string[] vinLine2;
                    string[] vinLine3;
                    string[] vinLine4;
                    string[] vinLine5;

                    if (retVIN[1].StartsWith("014"))
                    {
                        vinLine1 = retVIN[2].Split(' ');
                        vinLine2 = retVIN[3].Split(' ');
                        vinLine3 = retVIN[4].Split(' ');
                        HexVIN = vinLine1[4] + vinLine1[5] + vinLine1[6] +
                            vinLine2[1] + vinLine2[2] + vinLine2[3] + vinLine2[4] + vinLine2[5] + vinLine2[6] + vinLine2[7] +
                            vinLine3[1] + vinLine3[2] + vinLine3[3] + vinLine3[4] + vinLine3[5] + vinLine3[6] + vinLine3[7];

                        hexASCII = true;
                    }

                    if (retVIN[1].StartsWith("49 02"))
                    {
                        vinLine1 = retVIN[2].Split(' ');
                        vinLine2 = retVIN[3].Split(' ');
                        vinLine3 = retVIN[4].Split(' ');
                        vinLine4 = retVIN[5].Split(' ');
                        vinLine5 = retVIN[6].Split(' ');
                        HexVIN = vinLine1[6] +
                            vinLine2[3] + vinLine2[4] + vinLine2[5] + vinLine2[6] +
                            vinLine3[3] + vinLine3[4] + vinLine3[5] + vinLine3[6] +
                            vinLine4[3] + vinLine4[4] + vinLine4[5] + vinLine4[6] +
                            vinLine5[3] + vinLine5[4] + vinLine5[5] + vinLine5[6];

                        hexASCII = true;
                    }

                    if (hexASCII)
                    {
                        _VIN = HexString2Ascii(HexVIN);
                    }
                    else
                    {
                        return "NO DATA";
                    }
                }
            }
            catch
            {
                //MessageBox.Show("Could not find VIN for connected vehicle", "VIN Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _VIN = "NO DATA";
            }

            SystemConsole(_VIN, true);
            return _VIN;
        }

        #endregion  

        #endregion

        // Methods for conversions and calculations
        #region Helpers

        // Calculate vehicle Miles Per Gallon
        private double GetMPG()
        {
            if (_FUEL_TYPE != null)
            {
                if (_MAF != -999 && get_PID_010D)
                {                    
                    _MPG = (Convert.ToDouble(_FUEL_TYPE[1]) * Convert.ToDouble(_FUEL_TYPE[2]) * 453.592 * _SPEEDKMH * 0.621371) / (3600 * _MAF);
                    if (isLogging)
                    {
                        GetMPG_Average(_MPG);
                    }
                }
                else
                {
                    _MPG = -999;
                }
                //SystemConsole("MPG - " + Math.Round(MPG, 1), true);

            }
            return _MPG;

            /// 14.7 grams of air to 1 gram of gasoline - ideal air/fuel ratio
            /// 6.217 pounds per gallon - density of gasoline
            /// 453.592 grams per pound - conversion
            /// SPEEDKMH - vehicle speed in kilometers per hour
            /// 0.621371 miles per hour/kilometers per hour - conversion
            /// 3600 seconds per hour - conversion
            /// MAF - mass air flow rate in 100 grams per second
            /// 100 - to correct MAF to give grams per second
        }

        // Calculate vehicle average Miles Per Gallon
        private double GetMPG_Average(double MPG)
        {
            if (MPG >= 0 && MPG < 99999)
            {
                countMPG_average++;
                tempMPG_average += MPG;

                _MPG_AVERAGE = tempMPG_average / countMPG_average;
            }
            return _MPG_AVERAGE;
        }

        // Calculate vehicle odometer reading
        private double GetOdometer()
        {
            if (get_PID_010D)
            {
                _ODOMETER += _SPEEDMPH * (0.999 / 3600);
            }

            return Math.Round(_ODOMETER, 1);
        }

        // Convert Hex to ASCII
        private string HexString2Ascii(string hexString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= hexString.Length - 2; i += 2)
            {
                sb.Append(Convert.ToString(Convert.ToChar(Int32.Parse(hexString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber))));
            }
            return sb.ToString();
        }

        #endregion        

        // Performs a request off all enables PIDs
        #region Logging Sequence

        // Toggles boolean to ask for telemetry sequence
        public void ToggleTelemetry(bool toggle)
        {
            if (toggle)
            {
                isLogging = true;
            }
            else
            {
                isLogging = false;
            }
        }

        // Performs a sequence to send PIDs to the OBD
        private void PID_Sequence()
        {
            GetVoltage(get_PID_ATRV);
            GetIgnition(get_PID_ATIGN);
            GetEngineLoad(get_PID_0104);
            GetShortTermFuelTrimBank1(get_PID_0106);
            GetLongTermFuelTrimBank1(get_PID_0107);
            GetShortTermFuelTrimBank2(get_PID_0108);
            GetLongTermFuelTrimBank2(get_PID_0109);
            GetFuelPressure(get_PID_010A);
            GetIntakeManifoldPressure(get_PID_010B);
            GetRPM(get_PID_010C);
            GetSpeed(get_PID_010D);
            GetTimingAdvance(get_PID_010E);
            GetMAF(get_PID_0110);
            GetThrottlePosition(get_PID_0111);
            GetEngineRunTime(get_PID_011F);


            CheckLowPriorityPIDs();
        }

        // Low Priority PIDs (delayed request)
        private void CheckLowPriorityPIDs()
        {
            //if (DateTime.Now.Second == 1 || DateTime.Now.Second == 31)
            //{            
            GetCoolantTemperature(get_PID_0105);
            GetIntakeAirTemperature(get_PID_010F);
            GetFuelLevel(get_PID_012F);
            GetAmbientAirTemperature(get_PID_0146);
            GetEngineOilTemperature(get_PID_015C);
            //}
        }

        // Run new thread here
        private void BackgroundThread()
        {
            try
            {
                while (isLogging)
                {
                    SystemConsole("+++++++++++++++++", true);
                    PID_Sequence();
                    SystemConsole("-----------------", true);
                }

                Thread.Sleep(100);
                SystemConsole("Thread has ended", true);
            }

            catch (Exception ex)
            {
                SystemConsole(ex.Message, true);
            }
        }

        #endregion

        #region Debug

        private void SystemConsole(string msg, bool enableWrite)
        {
            if (enableWrite)
            {
                //Console.WriteLine(msg);
            }
        }

        #endregion
    }
}
