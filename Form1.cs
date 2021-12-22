using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;



namespace goblinRevolver
{



    public partial class Form1 : Form
    {

        // ######### INIT #########################################################################################

        public Form1()
        {
            InitializeComponent();
        }

        // ########################################################################################################



        // ######### EVENTS #######################################################################################

        // ONLOAD HOOK
        private void Form1_Load(object sender, EventArgs e)
            {
                try
                {
                    lvwDevices.View = View.Details;
                    lvwDevices.GridLines = true;
                    lvwDevices.FullRowSelect = true;

                    lvwDevices.Columns.Add("DeviceID", 390);
                    lvwDevices.Columns.Add("PNPDeviceID", 390);
                    lvwDevices.Columns.Add("Description", 350);
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Exception: " + exp.Message);
                }


                try
                {
                    OS_bit.Text = getIs64BitOSText();
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Exception: " + exp.Message);
                }


                try
                {
                    update_USBDevicesTable();
                }
                catch (Exception exp)
                {
                    MessageBox.Show("Exception: " + exp.Message);
                }

        
            }

        // ########################################################################################################




        // ######### USB ##########################################################################################        

        // UPDATE USB DEVICES TABLE
        private void update_USBDevicesTable()
        {
            try
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
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }
            
        }

        // GET USB DEVICES
        static List<USBDeviceInfo> GetUSBDevices()
        {
            try
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
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }

            return null;
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
            try
            {
                if (lvwDevices.SelectedItems.Count == 1)
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
                else
                {
                    MessageBox.Show("Please select a device first.");
                    return;
                }
                
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }

            
        }

        // BUTTON: ENABLE USB
        private void btn_enableUSB(object sender, EventArgs e)
        {
            try
            {
                if (lvwDevices.SelectedItems.Count == 1)
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
                else
                {
                    MessageBox.Show("Please select a device first.");
                    return;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }

            

        }

        // BUTTON: REFRESH USB DEVICES
        private void btn_refreshUSBDevices(object sender, EventArgs e)
        {
            try
            {
                update_USBDevicesTable();
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }
            
        }

        // ########################################################################################################




        // ######### GOBLIN COMPILER AND UPLAODER #################################################################        

        // OPEN GOBLIN COMPILER
        private void openCompiler(object sender, EventArgs e)
        {
            try
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
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }
        
        }

        // OPEN GOBLIN UPLOADER
        private void openUploader(object sender, EventArgs e)
        {
            try
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
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }
        
        }

        // ########################################################################################################




        // ######### ENVIRONMENT ##################################################################################

        // CHECK IF OS IS 64-BIT
        private bool getIs64BitOS() 
        {
            try
            {
                return Environment.Is64BitOperatingSystem;
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }

            // default to 64 Bit 
            return true; // ToDo: build enumaration
        
        }


        // GET TEXT FOR IS 64-BIT OS LABEL
        private string getIs64BitOSText()
        {
            try
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
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }

            // default to empty string
            return "";
        }

        // ########################################################################################################

    }
}
