using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using MessageBox = System.Windows.Forms.MessageBox;
using Dicom;
using Dicom.Media;
using Dicom.Serialization;
using System.Xml;

namespace roboReaderAssistant
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
            try
            {
                runBatFile("Roboend");
                System.Threading.Thread.Sleep(3000);


                updateJSONFile();
                System.Threading.Thread.Sleep(3000);

                runBatFile("Roborun");

                var result = MessageBox.Show("Changes have been applied.", "Success",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Information);
            }

            catch (Exception exp)
            {
                var result = MessageBox.Show("An error has occured: " + exp.Message, "Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
            }
         



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


        private string getValueFromTag(DicomTag tag, DicomDataset dataset)
        {            
            return dataset.GetSingleValue<string>(tag);
        }


        public static void DumpSingleDicomTag(string dicomFile, string tagNumber)
        {
            var dataset = DicomFile.Open(dicomFile).Dataset;
            var tag = Dicom.DicomTag.Parse(tagNumber);
            var result = dataset.GetString(tag);
            Console.WriteLine(result);
        }





        private void button4_Click(object sender, EventArgs e)
        {
            //ReadFile("C:\\0002.DCM");

            DicomFile file = DicomFile.Open(@"C:\dicomfileOrdner\DICOMDIR");

            DicomDataset dataset = file.Dataset;

            //dataset.WriteToXml();

            //file.Dataset.GetDicomItem<T>(DicomTag.PatientID);
            string xmlText = DicomXML.WriteToXml(dataset);

            //DumpSingleDicomTag(@"C:\dicomfileOrdner\DICOMDIR", "0010,0010");

            var a = 324;
            //Console.WriteLine(file.Dataset.Get<string>(DicomTag.PatientID));

            //dataset.AddOrUpdate(DicomTag.PatientName, "DOE^JOHN");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlText);
            //doc.Save("test.xml");

            var b = 200;

            Console.WriteLine("Patient Level Information:");
            //Console.WriteLine(getValueFromTag(DicomTag.PatientName, dataset));

            var c = 200;





        






            /*
            var dicomDirectory = DicomDirectory.Open("C:\\dicomfileOrdner\\DICOMDIR");
            foreach (var patientRecord in dicomDirectory.RootDirectoryRecordCollection)
            {
                var a = 100;

                foreach (var studyRecord in patientRecord.LowerLevelDirectoryRecordCollection)
                {
                    foreach (var seriesRecord in studyRecord.LowerLevelDirectoryRecordCollection)
                    {
                        foreach (var imageRecord in seriesRecord.LowerLevelDirectoryRecordCollection)
                        {
                            //this is the problematic line
                            //var dicomDataset = imageRecord.GetValue<DicomSequence>(DicomTag.IconImageSequence, 0).Items.First();
                            //more stuff
                        }
                    }
                }
            }
            */

            //var patientid = file.Dataset.GetString(DicomTag.PatientID);

            //var patientName = DicomTag.Parse("0010,0010");
            //string value = file.Dataset.GetString(patientName);



        }



        // READ DICOM FILE (not working)
        public void ReadFile(string filename)
        {
            using (FileStream fs = File.OpenRead(filename))
            {
                fs.Seek(128, SeekOrigin.Begin);
                if ((fs.ReadByte() != (byte)'D' ||
                     fs.ReadByte() != (byte)'I' ||
                     fs.ReadByte() != (byte)'C' ||
                     fs.ReadByte() != (byte)'M'))
                {
                    Console.WriteLine("Not a DCM");
                    return;
                }
                BinaryReader reader = new BinaryReader(fs);

                ushort g;
                ushort e;
                do
                {
                    g = reader.ReadUInt16();
                    e = reader.ReadUInt16();

                    string vr = new string(reader.ReadChars(2));
                    long length;
                    if (vr.Equals("AE") || vr.Equals("AS") || vr.Equals("AT")
                        || vr.Equals("CS") || vr.Equals("DA") || vr.Equals("DS")
                        || vr.Equals("DT") || vr.Equals("FL") || vr.Equals("FD")
                        || vr.Equals("IS") || vr.Equals("LO") || vr.Equals("PN")
                        || vr.Equals("SH") || vr.Equals("SL") || vr.Equals("SS")
                        || vr.Equals("ST") || vr.Equals("TM") || vr.Equals("UI")
                        || vr.Equals("UL") || vr.Equals("US"))
                        length = reader.ReadUInt16();
                    else
                    {
                        // Read the reserved byte
                        reader.ReadUInt16();
                        length = reader.ReadUInt32();
                    }

                    byte[] val = reader.ReadBytes((int)length);

                } while (g == 2);

                fs.Close();
            }

            return;
        }

        // ########################################################################################################




        // ######### EVENTS #######################################################################################

        // ONLOAD HOOK
        private void Form1_Load(object sender, EventArgs e)
        {
                if (checkIfConfigFilesExist() == 0)
                {
                    readFileAndUpdatePath();

                    comboBox1.SelectedIndex = 0;

                    textBox1.Text = path_Instructions;

                    textBox1.ReadOnly = true;
                 }                
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

        private void button3_Click(object sender, EventArgs e)
        {
            checkIfConfigFilesExist();
        }


        // CHECK IF ALL 4 CONFIG/BAT FILES EXIST
        private int checkIfConfigFilesExist()
        {
            string exeLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            // ROBORUN.BAT
            string roboRunFile = exeLocation + "\\Roborun.bat";            
            bool roboRun = File.Exists(roboRunFile);
            if (!roboRun)
            {
                var result = MessageBox.Show("An error has occured. Roborun.bat file missing in roboReaderAssistant.exe's directory (" + exeLocation + ").", "Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
                return 1;
            }

            // ROBOEND.BAT
            string roboEndFile = exeLocation + "\\Roboend.bat";
            bool roboEnd = File.Exists(roboEndFile);
            if (!roboEnd)
            {
                var result = MessageBox.Show("An error has occured. Roboend.bat file missing in roboReaderAssistant.exe's directory (" + exeLocation + ").", "Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
                return 2;
            }

            // CONFIG.CONF
            string configConfFile = exeLocation + "\\config.conf";
            bool configConf = File.Exists(configConfFile);
            if (!configConf)
            {
                var result = MessageBox.Show("An error has occured. config.conf file missing in roboReaderAssistant.exe's directory (" + exeLocation + ").", "Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
                return 3;
            }


            // SETTINGS.JSON
            string settingJSON = "C:\\Robo\\config\\setting.json";
            bool settingJSONBool = File.Exists(settingJSON);
            if (!settingJSONBool)
            {
                var result = MessageBox.Show("An error has occured. setting.json is missing in C:\\Robo\\config directory.", "Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
                return 4;
            }


            return 0;
        }


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

                string[] splitLine = lineToModify.Split(':');

                string firstPart = splitLine[0];

                string secondPart = " " + "\"" + textBox1.Text.Replace('\\', '/') + "\"";

                string newString = firstPart + ":" + secondPart;                

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
            // READ EACH LINE OF CONFIG.CONF FILE
            string[] config_lines = System.IO.File.ReadAllLines("config.conf");

            if (config_lines[0] == "[config-file]")
            {
                path_Instructions = config_lines[1].Split('=')[1];
                path_SolMeds = config_lines[2].Split('=')[1];
                path_PremMeds = config_lines[3].Split('=')[1];
                path_Diagnostic = config_lines[4].Split('=')[1];
            }

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
