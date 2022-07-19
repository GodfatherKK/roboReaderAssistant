using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows; // for WPF support
using System.Windows.Interop;
using MessageBox = System.Windows.Forms.MessageBox;

namespace goblinRevolver
{
    


    public partial class Form1 : Form
    {
        string path_Instructions = "";
        string path_SolMeds = "";
        string path_PremMeds = "";
        string path_Diagnostic = "";

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





                readFileAndUpdatePath();


                comboBox1.SelectedIndex = 0;
                textBox1.Text = path_Instructions;


            /*
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
            */

        }

        static void lineChanger(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit - 1] = newText;
            File.WriteAllLines(fileName, arrLine);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    //string[] files = Directory.GetFiles(fbd.SelectedPath);

                    //System.Windows.Forms.MessageBox.Show("Files found: " + files.Length.ToString(), "Message");

                    Console.WriteLine(fbd.SelectedPath);
                    
                    int index = comboBox1.SelectedIndex;

                    switch (index)
                    {
                        case 0:                            
                            lineChanger("Instructions=" + fbd.SelectedPath, "config.conf", 2);                            
                            break;

                        case 1:
                            lineChanger("SolMeds=" + fbd.SelectedPath, "config.conf", 3);                            
                            break;

                        case 2:
                            lineChanger("PremMeds=" + fbd.SelectedPath, "config.conf", 4);                            
                            break;

                        case 3:
                            lineChanger("Diagnostic=" + fbd.SelectedPath, "config.conf", 5);                            
                            break;

                        default:
                            break;
                    }

                    readFileAndUpdatePath();



                }
            }
        }

        private void readFileAndUpdatePath ()
        {
            // Example #2
            // Read each line of the config file
            string[] config_lines = System.IO.File.ReadAllLines("config.conf");

            if (config_lines[0] == "[config-file]")
            {
                path_Instructions = config_lines[1].Split('=')[1];
                path_SolMeds = config_lines[2].Split('=')[1];
                path_PremMeds = config_lines[3].Split('=')[1];
                path_Diagnostic = config_lines[4].Split('=')[1];
            }

            Console.WriteLine("path_Instructions: " + path_Instructions);
            Console.WriteLine("path_SolMeds: " + path_SolMeds);
            Console.WriteLine("path_PremMeds: " + path_PremMeds);
            Console.WriteLine("path_Diagnostic: " + path_Diagnostic);


            int index = comboBox1.SelectedIndex;

            switch (index)
            {
                case 0:
                    textBox1.Text = path_Instructions;
                    break;

                case 1:
                    textBox1.Text = path_SolMeds;
                    break;

                case 2:
                    textBox1.Text = path_PremMeds;
                    break;

                case 3:
                    textBox1.Text = path_Diagnostic;
                    break;

                default:
                    break;
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {            

            int index = comboBox1.SelectedIndex;

            switch (index)
            {
                case 0:
                    textBox1.Text = path_Instructions;
                    break;

                case 1:
                    textBox1.Text = path_SolMeds;
                    break;

                case 2:
                    textBox1.Text = path_PremMeds;
                    break;

                case 3:
                    textBox1.Text = path_Diagnostic;
                    break;

                default:                    
                    break;
            }
        }

        // ########################################################################################################




        // ######### TABLE ########################################################################################

        // SELECTION CHANGED
        private void selectionChanged(object sender, EventArgs e)
        {
         
        }

        // UPDATE USB DEVICES TABLE
        private void update_USBDevicesTable()
        {
            /*
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
            */

        }


        // BUTTON: EXPORT TO TEXT FILE
        private void exportToTextFile(object sender, EventArgs e)
        {
            exportToTextFile();
        }


        // EXPORT TO TEXT FILE
        private void exportToTextFile()
        {
            try
            {
                List<string> list = new List<string>();

                var usbDevices = GetUSBDevices();

                foreach (var usbDevice in usbDevices)
                {
                    string device = usbDevice.DeviceID.ToString() + ", " + usbDevice.PnpDeviceID.ToString() + ", " + usbDevice.Description.ToString();
                    list.Add(device);
                }

                String[] lines = list.ToArray();

                File.WriteAllLines("USBDevices.txt", lines);
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }
        }



        // ########################################################################################################




        // ######### USB ##########################################################################################        



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

        // USB HANDLING
        private void handleUSBDevice(string action)
        {   
            /*
            try
            {
                if (lvwDevices.SelectedItems.Count == 1)
                {
                    string deviceID = lvwDevices.SelectedItems[0].SubItems[1].Text;
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    if (getIs64BitOS())
                    {
                        startInfo.FileName = "pnputil.exe";                        
                    }
                    else
                    {
                        startInfo.FileName = "pnputil.exe";                        
                    }
                    startInfo.Arguments = action + " " + deviceID;
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
            */
        }

        // RESCAN
        private void findRemovedUSBDevices()
        {
            try
            {                                  
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
                startInfo.Arguments = "/rescan";
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();                 
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }

            updateUSBWithDelay();
        }

        // BUTTON: DISABLE USB
        private void btn_disableUSB(object sender, EventArgs e)
        {
            handleUSBDevice("/disable-device");
            
            updateUSBWithDelay();
        }

        // BUTTON: ENABLE USB
        private void btn_enableUSB(object sender, EventArgs e)
        {
            handleUSBDevice("/enable-device");

            updateUSBWithDelay();
        }
        
        // BUTTON: REMOVE USB
        private void btn_removeUSB(object sender, EventArgs e)
        {
            handleUSBDevice("/remove-device");

            updateUSBWithDelay();
        }

        // UPDATE USB DEVICES WITH DELAY AFTER ACTION
        private void updateUSBWithDelay()
        {
            System.Threading.Thread.Sleep(3000);
            try
            {
                update_USBDevicesTable();
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }
        }

        // BUTTON: SCAN USB
        private void btn_scan(object sender, EventArgs e)
        {
            findRemovedUSBDevices();

            
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
