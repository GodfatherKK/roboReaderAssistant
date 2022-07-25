using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using MessageBox = System.Windows.Forms.MessageBox;
using Dicom;
using Dicom.Media;
using Dicom.Serialization;
using System.Xml;
using System.Linq;

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

        public List<DicomEntry> createDicomEntry(XmlNodeList nodeList)
        {
            List<DicomEntry> entries = new List<DicomEntry>();

            foreach (XmlNode node in nodeList)
            {
                
                List<string> valueList = new List<string>();

                foreach (XmlNode subNode in node.ChildNodes)
                {
                    if (subNode.LocalName == "Value")
                    {
                        valueList.Add(subNode.InnerXml);
                    }
                    if (subNode.LocalName == "Item")
                    {
                        

                        valueList.Add(subNode.InnerXml);                        

                        XmlNodeList itemNodeList = subNode.SelectNodes("DicomAttribute");

                        List<DicomEntry> dicomItemEntryList = createDicomEntry(itemNodeList);

                        foreach (DicomEntry diconItemEntry in dicomItemEntryList)
                        {
                            entries.Add(diconItemEntry);
                        }
                            

                        var test = 200;
                    }
                }

                DicomEntry entry = new DicomEntry();
                entry.dicomTagID = node.Attributes[0].Value;
                entry.dicomVR = node.Attributes[1].Value;
                entry.dicomKeyword = node.Attributes[2].Value;
                entry.dicomValueList = valueList;

                entries.Add(entry);
            }

            return entries;
        }

        private void renameAllFolders(object sender, EventArgs e)
        {
            if (textBox1.Text != "undefined" && textBox1.Text.Length > 0)
            {

                // GET ALL DIRECT SUBFOLDERS
                var directories = Directory.GetDirectories(textBox1.Text);

                List<string> folderNames = new List<string>();

                // FILTER FOR IMP FOLDERS
                foreach (var directory in directories)
                {
                    string resultString = directory.ToString().Split(new string[] { textBox1.Text + "\\"}, StringSplitOptions.None)[1];
                    
                    if (resultString.StartsWith("imp"))
                    {
                        folderNames.Add(directory.ToString());
                    }                    
                }


                foreach (string folderName in folderNames)
                {
                    string filePath = folderName + "\\DICOMDIR";

                    Console.WriteLine(filePath);
                    bool fileExists = File.Exists(filePath);
                    if (fileExists)
                    {
                        List<DicomEntry> myList = readDicomFile(filePath);

                        foreach (DicomEntry entry in myList)
                        {
                            if (entry.dicomKeyword == comboBox2.Text)
                            {
                                Console.WriteLine("Value found for: " + entry.dicomKeyword);
                                var result = MessageBox.Show(entry.dicomValueList[0], "Error",
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error);
                            }
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show("An error has occured. DICOMDIR file missing in directory (" + textBox1.Text + ").", "Error",
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error);
                    }
                }


                               
            }            
        }



        public List<DicomEntry> readDicomFile(string folderPath)
        {


            string currentFolderPath = folderPath;

            //currentFolderPath = "C:\\dicomfileOrdner\\DICOMDIR";

            // OPEN DICOM FILE
            DicomFile file = DicomFile.Open(currentFolderPath);

            // GET DATASET OF DICOM FILE
            DicomDataset dataset = file.Dataset;

            // CONVERT TO XML
            string xmlText = DicomXML.WriteToXml(dataset);

            // READ INTO XML OBJECT "DOC"
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlText);

            // CREATE NODE LIST
            XmlNodeList nodeList;

            // DEFINE ROOT
            XmlNode root = doc.DocumentElement;

            // DEFINE PARENT NODE LIST
            nodeList = root.SelectNodes("/NativeDicomModel/DicomAttribute");

            List<DicomEntry> dicomEntryList = createDicomEntry(nodeList);

            return dicomEntryList;

        }

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

        // ##############################################################
        // ##########################################

        public SortedDictionary<string, string> createSortedDictionary ()
        {

            SortedDictionary<string, string> sortedList = new SortedDictionary<string, string>();

            sortedList.Add("AccessionNumber", "00080050");
            sortedList.Add("BodyPartExamined", "00180015");
            sortedList.Add("Columns", "00280011");
            sortedList.Add("DirectoryRecordSequence", "00041220");
            sortedList.Add("DirectoryRecordType", "00041430");
            sortedList.Add("FileSetConsistencyFlag", "00041212");
            sortedList.Add("FileSetDescriptorFileID", "00041141");
            sortedList.Add("FileSetID", "00041130");
            sortedList.Add("ImageType", "00080008");
            sortedList.Add("InstanceNumber", "00200013");
            sortedList.Add("InstitutionAddress", "00080081");
            sortedList.Add("InstitutionName", "00080080");
            sortedList.Add("Modality", "00080060");
            sortedList.Add("OffsetOfReferencedLowerLevelDirectoryEntity", "00041420");
            sortedList.Add("OffsetOfTheFirstDirectoryRecordOfTheRootDirectoryEntity", "00041200");
            sortedList.Add("OffsetOfTheLastDirectoryRecordOfTheRootDirectoryEntity", "00041202");
            sortedList.Add("OffsetOfTheNextDirectoryRecord", "00041400");
            sortedList.Add("PatientBirthDate", "00100030");
            sortedList.Add("PatientID", "00100020");
            sortedList.Add("PatientName", "00100010");
            sortedList.Add("PatientPosition", "00185100");
            sortedList.Add("PatientSex", "00100040");
            sortedList.Add("RecordInUseFlag", "00041410");
            sortedList.Add("ReferencedFileID", "00041500");
            sortedList.Add("ReferencedImageSequence", "00081140");
            sortedList.Add("ReferencedSOPClassUID", "00081150");
            sortedList.Add("ReferencedSOPClassUIDInFile", "00041510");
            sortedList.Add("ReferencedSOPInstanceUID", "00081155");
            sortedList.Add("ReferencedSOPInstanceUIDInFile", "00041511");
            sortedList.Add("ReferencedTransferSyntaxUIDInFile", "00041512");
            sortedList.Add("Rows", "00280010");
            sortedList.Add("SeriesDate", "00080021");
            sortedList.Add("SeriesInstanceUID", "0020000E");
            sortedList.Add("SeriesNumber", "00200011");
            sortedList.Add("SeriesTime", "00080031");
            sortedList.Add("SpecificCharacterSet", "00080005");
            sortedList.Add("StudyDate", "00080020");
            sortedList.Add("StudyDescription", "00081030");
            sortedList.Add("StudyID", "00200010");
            sortedList.Add("StudyInstanceUID", "0020000D");
            sortedList.Add("StudyTime", "00080030");

            return sortedList;

        }


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

                    comboBox2.SelectedIndex = 0;

                    var awdawda = 2323;
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

        // TAG SELECTION CHANGED
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string value = "";

            string key = comboBox2.Text;

            SortedDictionary<string, string> dictionary = createSortedDictionary();

            if (dictionary.TryGetValue(key, out value))
            {
                label5.Text = value;
            }
        }


        // ######### HELPER FUNCTIONS #############################################################################


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


    public class DicomEntry
    {
        public string dicomTagID;
        public string dicomVR;
        public string dicomKeyword;
        public List<string> dicomValueList = new List<string>();
    };

}
