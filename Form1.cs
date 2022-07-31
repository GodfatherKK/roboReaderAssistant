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
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading.Tasks;

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


        // INIT: ONLOAD HOOK
        private void initializeForm(object sender, EventArgs e)
        {
            if (checkIfConfigFilesExist() == 0)
            {
                readFileAndUpdatePath();

                comboBox1.SelectedIndex = 0;

                textBox1.Text = path_Instructions;

                textBox1.ReadOnly = true;

                comboBox2.SelectedIndex = 0;

                progressBar.Visible = true;
                progressLabel.Visible = true;

            }
        }


        // ########################################################################################################


        // MAIN ROUTINES


        private async void renameFoldersButton(object sender, EventArgs e)
        {
            var renameResult = await renameAllFoldersTask(this);
        }

        private async Task<int> renameAllFoldersTask(Form fm)
        {
            await Task.Run(() =>
            {
                // Set cursor as hourglass
                Cursor.Current = Cursors.WaitCursor;

                if (textBox1.Text != "undefined" && textBox1.Text.Length > 0)
                {

                    // GET ALL DIRECT SUBFOLDERS
                    var directories = Directory.GetDirectories(textBox1.Text);

                    List<string> folderNames = new List<string>();

                    // FILTER FOR IMP FOLDERS
                    foreach (var directory in directories)
                    {
                        string resultString = directory.ToString().Split(new string[] { textBox1.Text + "\\" }, StringSplitOptions.None)[1];

                        if (resultString.StartsWith("imp"))
                        {
                            folderNames.Add(directory.ToString());
                        }
                    }


                    // INIT PROGRESSBAR
                    int folderNamesCount = folderNames.Count;

                    initializeProgressbar(folderNamesCount);
      
                    // TRACK ALL RENAMED FOLDERS
                    List<string> renamedFolderNames = new List<string>();

                    // COLLECT ALL MESSAGES
                    List<string> messageList = new List<string>();

                    // COUNT ALL RENAMED FOLDERS
                    int renamedCount = 0;

                    // COUNT ALL NO DICOMDIR FILE FOLDERS
                    int noDicomdirFileCount = 0;

                    List<DicomEntry> myList = new List<DicomEntry>();

                    foreach (string folderName in folderNames)
                    {
                        myList.Clear();

                        string filePath = folderName + "\\DICOMDIR";

                        bool fileExists = File.Exists(filePath);
                        if (fileExists)
                        {
                            // <--CPU INTENSIVE-->
                            myList = readDicomFile(filePath);

                            bool entryFound = false;
                            DicomEntry foundEntry = new DicomEntry();

                            string tagText = "";
                            if (comboBox2.InvokeRequired)
                            {
                                comboBox2.Invoke(new MethodInvoker(delegate { tagText = comboBox2.Text; }));
                            }
                            else
                            {
                                tagText = comboBox2.Text;
                            }
                            

                            foreach (DicomEntry entry in myList)
                            {
                                if (entry.dicomKeyword == tagText)
                                {
                                    entryFound = true;
                                    foundEntry = entry;
                                }
                            }

                            if (entryFound)
                            {

                                string value = "";

                                for (int i = 0; i < foundEntry.dicomValueList.Count; i++)
                                {
                                    value = value + foundEntry.dicomValueList[i];

                                    if (i != foundEntry.dicomValueList.Count - 1)
                                    {
                                        value = value + "_";
                                    }
                                }


                                string[] destinationFolderSplit = folderName.Split(new string[] { "\\" }, StringSplitOptions.None);
                                string destinationFolderName = "";

                                for (int i = 0; i < destinationFolderSplit.Length - 1; i++)
                                {
                                    if (i == 0)
                                    {
                                        destinationFolderName = destinationFolderName + destinationFolderSplit[i];
                                    }
                                    else
                                    {
                                        destinationFolderName = destinationFolderName + "\\" + destinationFolderSplit[i];
                                    }

                                }
                                destinationFolderName = destinationFolderName + "\\" + value;

                                try
                                {
                                    if (!renamedFolderNames.Contains(value))
                                    {
                                        renamedFolderNames.Add(value);

                                        // RENAME
                                        Directory.Move(folderName, destinationFolderName);
                                        //messageList.Add(getFolderNameOfAbsolutePath(folderName) + " renamed to " + getFolderNameOfAbsolutePath(destinationFolderName)  + ".");
                                        renamedCount++;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < 1000; i++)
                                        {
                                            string countedValue = value + "_" + i;
                                            if (!renamedFolderNames.Contains(countedValue))
                                            {
                                                renamedFolderNames.Add(countedValue);

                                                // FALLBACK RENAME 
                                                Directory.Move(folderName, destinationFolderName + "_" + i);
                                                //messageList.Add(getFolderNameOfAbsolutePath(folderName) + "renamed to " + getFolderNameOfAbsolutePath(destinationFolderName) + ".");
                                                renamedCount++;

                                                break;
                                            }
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("An error has occured: " + ex.Message, "Error",
                                                                     MessageBoxButtons.OK,
                                                                     MessageBoxIcon.Error);
                                }

                            }
                            else
                            {
                                //var result = MessageBox.Show("Tag not found.", "Error",

                                messageList.Add("Tag not found in directory " + getFolderNameOfAbsolutePath(folderName) + ".");
                            }
                        }
                        else
                        {
                            //messageList.Add("Error: DICOMDIR file missing in directory " + getFolderNameOfAbsolutePath(folderName));
                            noDicomdirFileCount++;
                        }

                        // Perform the increment on the ProgressBar.
                        if (progressBar.InvokeRequired)
                        {
                            progressBar.Invoke(new MethodInvoker(delegate { progressBar.PerformStep(); }));
                        }
                        else
                        {
                            progressBar.PerformStep();
                        }
                        
                    }

                    // Set cursor as default arrow
                    Cursor.Current = Cursors.Default;

                    if (renamedCount == 0)
                    {
                        messageList.Add("No folders renamed.");
                    }
                    if (renamedCount == 1)
                    {
                        messageList.Add("Renamed " + renamedCount + " folder.");
                    }
                    if (renamedCount > 1)
                    {
                        messageList.Add("Renamed " + renamedCount + " folders.");
                    }

                    // NO DICOMDIR FILE CASE
                    if (noDicomdirFileCount == 1)
                    {
                        messageList.Add("Found " + noDicomdirFileCount + " folder without DICOMDIR file.");
                    }
                    if (noDicomdirFileCount > 1)
                    {
                        messageList.Add("Found " + noDicomdirFileCount + " folders without DICOMDIR file.");
                    }


                    if (messageList.Count > 0)
                    {
                        // SORT MESSAGE LIST
                        messageList.Sort();

                        // JOIN MESSAGE LIST STRINGS AND SHOW MESSAGEBOX
                        var message = string.Join(Environment.NewLine, messageList.ToArray());
                        DialogResult dialogResult = MessageBox.Show(message, "Action Log", MessageBoxButtons.OK,
                                     MessageBoxIcon.Information);
                        if (dialogResult == DialogResult.OK)
                        {

                            initializeProgressbar(folderNamesCount);
                        }
                    }


                }
            });





            return 0;
        }

        private void initializeProgressbar(int folderNamesCount)
        {
            // Initialize/Refresh the ProgressBar control.
            if (progressBar.InvokeRequired)
            {
                // Set Minimum to 1 to represent the first file being copied.
                progressBar.Invoke(new MethodInvoker(delegate { progressBar.Minimum = 1; }));

                // Set Maximum to the total number of files to copy.
                progressBar.Invoke(new MethodInvoker(delegate { progressBar.Maximum = folderNamesCount; }));

                // Set the initial value of the ProgressBar.
                progressBar.Invoke(new MethodInvoker(delegate { progressBar.Value = 1; }));

                // Set the Step property to a value of 1 to represent each file being copied.
                progressBar.Invoke(new MethodInvoker(delegate { progressBar.Step = 1; }));

            }
            else
            {
                // Set Minimum to 1 to represent the first file being copied.
                progressBar.Invoke(new MethodInvoker(delegate { progressBar.Minimum = 1; }));

                // Set Maximum to the total number of files to copy.
                progressBar.Invoke(new MethodInvoker(delegate { progressBar.Maximum = folderNamesCount; }));

                // Set the initial value of the ProgressBar.
                progressBar.Invoke(new MethodInvoker(delegate { progressBar.Value = 1; }));

                // Set the Step property to a value of 1 to represent each file being copied.
                progressBar.Invoke(new MethodInvoker(delegate { progressBar.Step = 1; }));


            }
        }


        private void resetFolderNamesButton(object sender, EventArgs e)
        {
            // Set cursor as hourglass
            Cursor.Current = Cursors.WaitCursor;

            DialogResult dialogResult = MessageBox.Show("This will reset all folder names in the selected path. Are you sure?", "Folder name reset confirmation", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                //do something

                // GET ALL DIRECT SUBFOLDERS
                var directories = Directory.GetDirectories(textBox1.Text);

                List<string> folderNames = new List<string>();

                int i = 0;

                int renamedFolderCount = 0;
                // GET ALL FOLDER NAMES
                foreach (var directory in directories)
                {
                    i++;

                    string resultString = directory.ToString().Split(new string[] { textBox1.Text + "\\" }, StringSplitOptions.None)[1];

                    folderNames.Add(resultString);

                    var sourcePath = textBox1.Text + "\\" + resultString;
                    while (Directory.Exists(textBox1.Text + "\\" + "imp_" + i))
                    {
                        i++;
                    }
                    var targetPath = textBox1.Text + "\\" + "imp_" + i;

                    if (sourcePath != targetPath)
                    {
                        Directory.Move(sourcePath, targetPath);
                        renamedFolderCount++;
                    }


                }

                System.Threading.Thread.Sleep(1000);
                var result = MessageBox.Show(renamedFolderCount + " folder names have been reset.", "Information",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Information);
            }
            else if (dialogResult == DialogResult.No)
            {
                //do something else
            }

            // Set cursor as default arrow
            Cursor.Current = Cursors.Default;


        }


        // ########################################################################################################



        // MISC

        public void changeSize(int width, int height)
        {
            this.Size = new Size(width, height);
        }




        public string concatenateAllSubNodeValues(XmlNode node)
        {
            string returnString = "";

            List<string> innerValues = new List<string>();

            if (node.ChildNodes.Count > 0)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.ChildNodes.Count > 0)
                    {
                        foreach (XmlNode childChild in child.ChildNodes)
                        {
                            if (childChild.InnerXml.Length > 0)
                            {
                                innerValues.Add(childChild.InnerText);
                            }
                        }
                    }
                }
            }

            for(int i = 0; i < innerValues.Count; i++)
            {
                    returnString = returnString + innerValues[i];
                    if (i != innerValues.Count - 1)
                    {
                        returnString = returnString + "_";
                    }

            }


            return returnString;
        }


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

                    }
                    if (subNode.LocalName != "Value" && subNode.LocalName != "Item")
                    {
                        string concatValue = concatenateAllSubNodeValues(subNode);
                        valueList.Add(concatValue);


                        var a = 1000;


                        var b = 2000;
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


        public string getFolderNameOfAbsolutePath(string absPath)
        {
            string[] returnString = absPath.Split(new string[] { "\\" }, StringSplitOptions.None);

            return returnString[returnString.Length - 1];
        }


      

        public List<DicomEntry> readDicomFile(string folderPath)
        {
            if (File.Exists(folderPath))
            {
                // OPEN DICOM FILE
                DicomFile file = DicomFile.Open(folderPath);

                // GET DATASET OF DICOM FILE
                DicomDataset dataset = file.Dataset;

                // CONVERT TO XML
                string xmlText = DicomXML.WriteToXml(dataset);

                // SANITIZE STRING                
                Regex estructureXml = new Regex(@"(?<initialTag>\<.+?\>)(?<content>.+?)(?<finalTag>\</.+?\>)", RegexOptions.Compiled);
                Regex filter = new Regex(@"[^</\w\s="">@.]", RegexOptions.Compiled);
                string xmlClean = estructureXml.Replace(xmlText, new MatchEvaluator(c =>
                {
                    return string.Format("{0}{1}{2}",
                        c.Groups["initialTag"].Value,
                        filter.Replace(c.Groups["content"].Value, string.Empty),
                        c.Groups["finalTag"].Value);
                }));


                // READ INTO XML OBJECT "DOC"
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlClean);

                // CREATE NODE LIST
                XmlNodeList nodeList;

                // DEFINE ROOT
                XmlNode root = doc.DocumentElement;

                // DEFINE PARENT NODE LIST
                nodeList = root.SelectNodes("/NativeDicomModel/DicomAttribute");

                List<DicomEntry> dicomEntryList = createDicomEntry(nodeList);

                return dicomEntryList;
            }
            else
            {
                return null;
            }

            

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
 

        // ########################################################################################################
        

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

                string secondPart = " " + "\"" + textBox1.Text.Replace('\\', '/') + "/" + "\"" ;

                string newString = firstPart + ":" + secondPart + ",";

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

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            changeSize(850, 420);
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
