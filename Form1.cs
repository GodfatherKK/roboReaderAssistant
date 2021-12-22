using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;



namespace goblinRevolver
{



    public partial class Form1 : Form
    {
        // INIT
        public Form1()
        {
            InitializeComponent();
        }


        // ######### EVENTS #######################################################################################

        // ONLOAD HOOK
        private void Form1_Load(object sender, EventArgs e)
        {
            lvwDevices.View = View.Details;
            lvwDevices.GridLines = true;
            lvwDevices.FullRowSelect = true;

            lvwDevices.Columns.Add("DeviceID", 390);
            lvwDevices.Columns.Add("PNPDeviceID", 390);
            lvwDevices.Columns.Add("Description", 350);

            OS_bit.Text = getIs64BitOSText();

            update_USBDevicesTable();
        }

        // ########################################################################################################




        // ######### USB ##########################################################################################        


        // UPDATE USB DEVICES TABLE
        private void update_USBDevicesTable()
        {
            lvwDevices.Items.Clear();

            var usbDevices = GetUSBDevices();

            foreach (var usbDevice in usbDevices)
            {
                //Add items in the listview
                string[] arr = new string[4];
                ListViewItem itm;

                //Add first item
                arr[0] = usbDevice.DeviceID.ToString();
                arr[1] = usbDevice.PnpDeviceID.ToString();
                arr[2] = usbDevice.Description.ToString();
                itm = new ListViewItem(arr);
                lvwDevices.Items.Add(itm);
            }
        }

        // GET USB DEVICES
        static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description")
                ));
            }

            collection.Dispose();
            return devices;
        }

        // TYPE: USBDeviceInfo
        class USBDeviceInfo
        {
            public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
            {
                this.DeviceID = deviceID;
                this.PnpDeviceID = pnpDeviceID;
                this.Description = description;
            }
            public string DeviceID { get; private set; }
            public string PnpDeviceID { get; private set; }
            public string Description { get; private set; }
        }

        // BUTTON: DISABLE USB
        private void btn_disableUSB(object sender, EventArgs e)
        {
            if (lvwDevices.SelectedItems[0].Text != null)
            {
                string deviceID = lvwDevices.SelectedItems[0].Text;
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                if (getIs64BitOS())
                {
                    startInfo.FileName = "devcon64.exe";
                }
                else
                {
                    startInfo.FileName = "devcon.exe";
                }                    
                startInfo.Arguments = "disable \"@" + deviceID + "\"";
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();
            }
        }

        // BUTTON: ENABLE USB
        private void btn_enableUSB(object sender, EventArgs e)
        {
            if (lvwDevices.SelectedItems[0].Text != null)
            {
                string deviceID = lvwDevices.SelectedItems[0].Text;
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                if (getIs64BitOS())
                {
                    startInfo.FileName = "devcon64.exe";
                }
                else
                {
                    startInfo.FileName = "devcon.exe";
                }
                startInfo.Arguments = "enable \"@" + deviceID + "\"";
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();
            }

        }

        // BUTTON: REFRESH USB DEVICES
        private void btn_refreshUSBDevices(object sender, EventArgs e)
        {
            update_USBDevicesTable();
        }



        // ########################################################################################################




        // ######### GOBLIN COMPILER AND UPLAODER #################################################################        

        // OPEN GOBLIN COMPILER
        private void openCompiler(object sender, EventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "Compiler.exe";
            startInfo.Arguments = "";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.Start();
        }

        // OPEN GOBLIN UPLOADER
        private void openUploader(object sender, EventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "Uploader.exe";
            startInfo.Arguments = "";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.Start();
        }

        // ########################################################################################################




        // ######### ENVIRONMENT ##################################################################################


        // CHECK IF OS IS 64-BIT
        private bool getIs64BitOS() 
        {
            return Environment.Is64BitOperatingSystem;
        }


        // GET TEXT FOR IS 64-BIT OS LABEL
        private string getIs64BitOSText()
        {
            bool is64Bit = getIs64BitOS();

            if (is64Bit)
            { 
                return "64 Bit System"; 
            }
            else
            { 
                return "32 Bit System"; 
            }
            return "";

        }

        // ########################################################################################################

    }
}
