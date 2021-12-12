using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace goblinRevolver
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lvwDevices.View = View.Details;
            lvwDevices.GridLines = true;
            lvwDevices.FullRowSelect = true;

            lvwDevices.Columns.Add("DeviceID", 390);
            lvwDevices.Columns.Add("PNPDeviceID", 390);
            lvwDevices.Columns.Add("Description", 350);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            lvwDevices.Items.Clear();

            var usbDevices = GetUSBDevices();

            foreach (var usbDevice in usbDevices)
            {
                //Console.WriteLine("Device ID: {0}, PNP Device ID: {1}, Description: {2}", usbDevice.DeviceID, usbDevice.PnpDeviceID, usbDevice.Description);          

                //Add items in the listview
                string[] arr = new string[4];
                ListViewItem itm;

                //Add first item
                arr[0] = usbDevice.DeviceID.ToString();
                arr[1] = usbDevice.PnpDeviceID.ToString();
                arr[2] = usbDevice.Description.ToString();
                itm = new ListViewItem(arr);
                lvwDevices.Items.Add(itm);

                /*
                ListViewItem new_item = lvwDevices.Items.Add(
                    usbDevice.DeviceID.ToString());
                new_item.SubItems.Add(
                    usbDevice.PnpDeviceID.ToString());
                new_item.SubItems.Add(
                    usbDevice.Description.ToString());
                */
            }

            //Console.Read();

            /*
            ManagementObjectSearcher device_searcher =
            new ManagementObjectSearcher("SELECT * FROM Win32_USBHub");
            foreach (ManagementObject usb_device in device_searcher.Get())
            {
                ListViewItem new_item = lvwDevices.Items.Add(
                    usb_device.Properties["DeviceID"].Value.ToString());
                new_item.SubItems.Add(
                    usb_device.Properties["PNPDeviceID"].Value.ToString());
                new_item.SubItems.Add(
                    usb_device.Properties["Description"].Value.ToString());
            }
            */
        }

        static List<ManagementBaseObject> GetLogicalDevices()
        {
            List<ManagementBaseObject> devices = new List<ManagementBaseObject>();
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher("root\\CIMV2",
                                  @"Select * From CIM_LogicalDevice"))
                collection = searcher.Get();
            foreach (var device in collection)
            {
                devices.Add(device);
            }
            collection.Dispose();
            return devices;
        }


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

        private void disable(object sender, EventArgs e)
        {
            if (lvwDevices.SelectedItems[0].Text != null)
            {
                string deviceID = lvwDevices.SelectedItems[0].Text;
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "C:\\repo\\devcon\\devcon64.exe";
                startInfo.Arguments = "disable \"@" + deviceID + "\"";
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();
            }
        }

        private void enable(object sender, EventArgs e)
        {
            if (lvwDevices.SelectedItems[0].Text != null)
            {
                string deviceID = lvwDevices.SelectedItems[0].Text;
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "C:\\repo\\devcon\\devcon64.exe";
                startInfo.Arguments = "enable \"@" + deviceID + "\"";
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();
            }
           
        }

        private void openCompiler(object sender, EventArgs e)
        {         
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "C:\\repo\\goblinRevolver\\ADR_Projekt_01_GoblinRevolver\\PE600\\GOB Compiler\\GOB Compiler-2.02.05.exe";
            startInfo.Arguments = "";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.Start();
        }

        private void openUploader(object sender, EventArgs e)
        {       
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "C:\\repo\\goblinRevolver\\ADR_Projekt_01_GoblinRevolver\\PE600\\GOB Uploarder\\GOB Uploader-U0.P0.05.J.exe";
            startInfo.Arguments = "";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
