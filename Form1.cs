using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;
using System.IO;
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




        // ######### BUTTONS ######################################################################################

        // APPLY SETTINGS
        private void applySettingsButton(object sender, EventArgs e)
        {

            runBatFile("Roboend");

            updateJSONFile();

            runBatFile("Roborun");
        }

        // SELECT FOLDER
        private void selectFolderButton(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {

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

        // COMBOBOX SELECTED INDEX CHANGED
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





        // ######### HELPER FUNCTIONS #############################################################################

        // GET LINE (STRING) OF SRC SETTING IN SETTING.JSON
        private string getLineOfSettingsJSONSource()
        {
            try
            {
                IEnumerable<String> aLines = File.ReadLines("C:\\Robo\\config\\setting.json");

                bool copy = false;

                int i = 0;
                foreach (string line in aLines)
                {
                    i++;

                    if (line.Contains("\"copy\""))
                    {
                        copy = true;

                    }

                    if (line.Contains("\"src\"") && copy)
                    {
                        return line;
                    }

                }

                return "99999";
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
                return "99999";
            }
        }

        // GET LINE NUMBER (INT) OF SRC SETTING IN SETTING.JSON
        private int getLineNumberOfSettingsJSONSource()
        {
            try
            {
                IEnumerable<String> aLines = File.ReadLines("C:\\Robo\\config\\setting.json");

                bool copy = false;

                int i = 0;
                foreach (string line in aLines)
                {
                    i++;

                    if (line.Contains("\"copy\""))
                    {
                        copy = true;

                    }

                    if (line.Contains("\"src\"") && copy)
                    {
                        return i;
                    }

                }

                return 99999;
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
                return 99999;
            }
        }

        // MODIFY SETTING.JSON
        private void updateJSONFile()
        {
            string lineToModify = getLineOfSettingsJSONSource();

            if (lineToModify != "99999")
            {
                Console.WriteLine(lineToModify);

                string[] splitLine = lineToModify.Split(':');

                string firstPart = splitLine[0];

                string secondPart = " " + "\"" + textBox1.Text.Replace('\\', '/') + "\"";

                string newString = firstPart + ":" + secondPart;

                Console.WriteLine(newString);

                int lineNumber = getLineNumberOfSettingsJSONSource();

                if (lineNumber != 99999)
                {
                    lineChanger(newString, "C:\\Robo\\config\\setting.json", lineNumber);
                }


                    
            }

            
        }

        // RUN BAT FILE (IN SAME DIRECTORY AS ROBOREADERASSISTANT EXE)
        private void runBatFile(string fileName)
        {
            try
            {
                string exeLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                System.Diagnostics.Process.Start(exeLocation + "\\" + fileName + ".bat");

            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception: " + exp.Message);
            }
            
        }

        // CHANGE LINE IN FILE (NEWTEXT, FILEPATH, LINENUMBER)
        static void lineChanger(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit - 1] = newText;
            File.WriteAllLines(fileName, arrLine);
        }

        // READ CONFIG.CONF (IN SAME DIRECTORY AS ROBOREADERASSISTANT EXE)
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

            /*
            Console.WriteLine("path_Instructions: " + path_Instructions);
            Console.WriteLine("path_SolMeds: " + path_SolMeds);
            Console.WriteLine("path_PremMeds: " + path_PremMeds);
            Console.WriteLine("path_Diagnostic: " + path_Diagnostic);
            */


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






        // ########################################################################################################

    }
}
