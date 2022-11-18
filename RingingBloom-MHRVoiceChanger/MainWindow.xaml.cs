using MHRVoiceChanger;
using MHRVoiceChanger.VoiceChanger;
using MHRVoiceChanger.WWiseTypes;
using MHRVoiceChanger.WWiseTypes.Common;
using Microsoft.Win32;
using RingingBloom.Windows;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace RingingBloom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    /* EDITED for the MHRVoiceChanger
    General Changes:
        The original RingingBloom was a all-purpose tool for multiple games and run on different engines
        The MHRVoiceChanger repurposes this tool to focus on a singular game and component of RingingBloom which is the PCK Editor (NPCKEditor)
    */
    public partial class MainWindow : Window
    {
        SupportedGames mode = SupportedGames.MHRise;
        Options options;
        NPCKEditor npckEditor = null;


        FileSegments fileSegments = new FileSegments();
        SQLiteHandler voiceDatabase = new SQLiteHandler();
        string currentFilePath = null;
        string modBaseDirPath = Directory.GetCurrentDirectory() + "/Output Voice Mod/";
        int errorType = 0;

        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists("Options.xml"))
            {
                options = new Options(XmlReader.Create(new FileStream("Options.xml",FileMode.Open)));
            }
            else
            {
                options = new Options();
            }
            mode = options.defaultGame;
            
            // TRY: Getting the database version and displaying it
            try
            {
                dbVersion.Text = "DB Version: " + voiceDatabase.GetDBVersion();
            }
            // CATCH: Unable to read the database version, raise the corresponding error
            catch (System.Data.SQLite.SQLiteException)
            {
                dbVersion.Text = "DB Version: ERROR";
                dbVersion.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF0000");
                errorType = 3;
            }

            // Run the PCK Editor alongside the main window
            npckEditor = new NPCKEditor(mode, options);
            //npckEditor.Show();
        }

        private void SetOptions(object sender, RoutedEventArgs e)
        {
            OptionsWindow optionWindow = new OptionsWindow(options);
            optionWindow.ShowDialog();
            if(optionWindow.DialogResult == true)
            {
                ComboBoxItem DefaultGame = (ComboBoxItem)optionWindow.DefaultGame.SelectedItem;
                int defaultGame = Convert.ToInt32(DefaultGame.Tag);
                options = new Options(optionWindow.DefaultImport.Text, optionWindow.DefaultExport.Text, (SupportedGames)defaultGame);
                options.WriteOptions();
                mode = options.defaultGame;
            }
        }


        /* SUMMARY: ImportPCKFile 
        Imported and edited from NPCKEditor's ImportNPCK function
        The file name is used to determine information in the overview
        */
        private void ImportPCKFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog importFile = new OpenFileDialog();
            if (options.defaultImport != null)
            {
                importFile.InitialDirectory = options.defaultImport;
            }
            importFile.Multiselect = false;
            importFile.Filter = "WWise Package file (*.pck)|*.pck";
            switch (mode)
            {
                case SupportedGames.MHRise:
                    importFile.Filter += "|Monster Hunter Rise Switch WWise Package (*.x64)|*.pck.3.x64";
                    importFile.Filter += "|Monster Hunter Rise English WWise Package (*.En)|*.pck.3.x64.En";
                    importFile.Filter += "|Monster Hunter Rise Japanese WWise Package (*.Ja)|*.pck.3.x64.Ja";
                    importFile.Filter += "|Monster Hunter Rise Fictional WWise Package (*.Fc)|*.pck.3.x64.Fc";
                    importFile.Filter = "All supported files (*.pck,*.x64,*.En,*.Ja,*.Fc)|*.pck;*.pck.3.x64;*.pck.3.x64.En;*.pck.3.x64.Ja;*.pck.3.x64.Fc|" + importFile.Filter;
                    break;
                case SupportedGames.MHRiseSwitch:
                    importFile.Filter += "|Monster Hunter Rise Switch WWise Package (*.nsw)|*.pck.3.nsw";
                    importFile.Filter += "|Monster Hunter Rise English WWise Package (*.En)|*.pck.3.nsw.En";
                    importFile.Filter += "|Monster Hunter Rise Japanese WWise Package (*.Ja)|*.pck.3.nsw.Ja";
                    importFile.Filter += "|Monster Hunter Rise Fictional WWise Package (*.Fc)|*.pck.3.nsw.Fc";
                    importFile.Filter = "All supported files (*.pck,*.nsw,*.En,*.Ja,*.Fc)|*.pck;*.pck.3.nsw;*.pck.3.nsw.En;*.pck.3.nsw.Ja;*.pck.3.nsw.Fc|" + importFile.Filter;
                    break;
                default:
                    break;
            }
            if (importFile.ShowDialog() == true)
            {
                // TRY: Importing the file and updating the overview
                try
                {
                    //IF: The imported file's size is LESS than ~13 KB, confirm with the user if the file's the header file
                    if (new System.IO.FileInfo(importFile.FileName).Length < 13000)
                    {
                        MessageBoxResult sizeCheck = MessageBox.Show("The file imported might be the header file due to its small size. If it is, the conversion won't work.\n\nAre you sure you imported the 'streaming' PCK file?", "Warning: Incorrect File?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                        //IF: The user says "No" to the warning, cancel the import process
                        if (sizeCheck == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    // Get the file path and parse it for its name, voice and language
                    currentFilePath = importFile.FileName;
                    string currentFileName = currentFilePath.Split("\\").Last();
                    string currentFileVoice = currentFileName.Substring(9, 4);
                    string currentFileLang = currentFileName.Split(".").Last();

                    // Update the overview with the imported file
                    importedFilePath.Text = currentFilePath;
                    inputVoiceOV.Content = fileSegments.getVoiceSegmentInverse(currentFileVoice);
                    inputLangOV.Content = fileSegments.getLangSegmentInverse(currentFileLang);

                    errorType = 0;
                    InputFileUpdate();
                }
                // CATCH: The file failed to import, raise the corresponding error
                catch
                {
                    importedFilePath.Text = "ERROR";
                    statusLabel.Text = "ERROR";
                    errorType = 2;
                }
            }
        }


        /* SUMMARY: ConvertPCKFile
        Executes the conversion displayed in the overview
        Function for the "CONVERT" Button and the main function of the MHRVoiceChanger
        */
        private void ConvertPCKFile(object sender, RoutedEventArgs e)
        {
            // IF: If an error is present, explain the error and stop
            if (errorType != 0)
            {
                ErrorBoxHandler();
                return;
            }

            // IF: The PCK file isn't imported yet, state so and stop
            if (currentFilePath == null)
            {
                MessageBox.Show("No PCK file imported.", "Error: No PCK file imported", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            string convertOverview = "(" + inputVoiceOV.Content + ", " + inputLangOV.Content + ") -> (" + outputVoiceOV.Content + ", " + outputLangOV.Content + ")";
            MessageBoxResult confirmConvert = MessageBox.Show("Ready to convert?\n" + convertOverview, "Confirm Conversion", MessageBoxButton.OKCancel);
            // IF: The user agrees on the conversion, convert the voice file
            if (confirmConvert == MessageBoxResult.OK)
            {
                // IF: the file path is invalid, state so and stop 
                if (!File.Exists(currentFilePath))
                {
                    MessageBox.Show("Unable to read the imported file! It's file path may have changed.", "Error: Bad Import Path", MessageBoxButton.OK, MessageBoxImage.Error);
                    currentFilePath = null;
                    importedFilePath.Text = "ERROR";
                    CheckFileStatus();
                    return;
                }

                // Import the file into the PCK Editor
                BinaryReader readFile = HelperFunctions.OpenFile(currentFilePath);
                npckEditor.viewModel.SetNPCK(new NPCKHeader(readFile, mode, currentFilePath.Split("\\").Last().Split(".")[0]));
                readFile.Close();

                // Export the wems out of the PCK file
                npckEditor.Export_Wems();

                // Rename the wems to work with the output voice
                voiceDatabase.ConvertVoiceWEMs((string)inputFileOV.Content, (string)outputFileOV.Content);

                // Reimport the wems into the PCK Editor
                npckEditor.Reimport_Wems();

                // ID Replace all the files
                string convertedIDList = voiceDatabase.GetIDList((string)outputFileOV.Content);
                npckEditor.IDReplace(convertedIDList);

                // Export the PCK file and write the mod's information in a Fluffy ready folder
                npckEditor.ExportPCK((string)outputFileOV.Content);
                WriteModInfo(convertOverview);
                MessageBox.Show("Conversion Complete!\nLook for the 'Output Voice Mod' in this folder for your voice mod.\n\nIt's suggested to open it's 'modinfo.ini' file and renaming it to something more approriate.", "Conversion Complete");
            }
        }


        /* SUMMARY: WriteModInfo
        Writes the 'modinfo.ini' file for the converted voice mod
        
        PARAMETERS:
            convertOverview - The directory that is to be setup

        */
        private void WriteModInfo(string convertOverview)
        {
            // IF: The mod folder doesn't exist for some reason, state so 
            if (!Directory.Exists(modBaseDirPath))
            {
                MessageBox.Show("Unable to write to the Output Mod Folder! The mod is ready but will need a 'modinfo.ini' file.", "Error: Can't find Output Mod Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string[] lines =
            {
            "name=Converted Voice Mod", 
            "description=" + convertOverview,
            "author=Converted by the MHR Voice Changer",
            "NameAsBundle=Character Voice Mods"
            };

            File.WriteAllLinesAsync(modBaseDirPath + "modinfo.ini", lines);
        }


        /* SUMMARY: ManualModeToggle
        Toggles the "Manual Mode" section of the Input Voice
        */
        private void ManualModeToggle(object sender, RoutedEventArgs e)
        {
            bool manualModeEnabled = (bool)manualCheckBox.IsChecked;
            if (manualModeEnabled)
            {
                inputTypeLabel.Opacity = 1.0;
                inputVoiceLabel.Opacity = 1.0;
                inputLangLabel.Opacity = 1.0;

                inputTypePanel.Opacity = 1.0;
                inputVoiceComboBox.Opacity = 1.0;
                inputLangPanel.Opacity = 1.0;
            }
            else
            {
                inputTypeLabel.Opacity = 0.5;
                inputVoiceLabel.Opacity = 0.5;
                inputLangLabel.Opacity = 0.5;

                inputTypePanel.Opacity = 0.5;
                inputVoiceComboBox.Opacity = 0.5;
                inputLangPanel.Opacity = 0.5;
            }

            inputTypeSRadio.IsEnabled = manualModeEnabled;
            //inputTypeDRadio.IsEnabled = manualModeEnabled;

            inputVoiceComboBox.IsEnabled = manualModeEnabled;

            inputLangERadio.IsEnabled = manualModeEnabled;
            inputLangJRadio.IsEnabled = manualModeEnabled;
            inputLangFRadio.IsEnabled = manualModeEnabled;
        }


        /* SUMMARY: InputVoiceUpdate
        Updates the input voice in the overview
        */
        private void InputVoiceUpdate(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)inputVoiceComboBox.SelectedItem;
            inputVoiceOV.Content = selectedItem.Content;
            InputFileUpdate();
        }


        /* SUMMARY: InputLangUpdate
        Updates the input language in the overview
        */
        private void InputLangUpdate(object sender, RoutedEventArgs e)
        {
            foreach (RadioButton radioButton in inputLangPanel.Children)
            {
                if (radioButton.IsChecked == true)
                {
                    inputLangOV.Content = radioButton.Content;
                    InputFileUpdate();
                }
            }
        }


        /* SUMMARY: InputFileUpdate
        Updates the input file name in the overview
        */
        private void InputFileUpdate()
        {
            string selectedInputVoice = fileSegments.getVoiceSegment((string)inputVoiceOV.Content);
            string selectedInputLang = fileSegments.getLangSegment((string)inputLangOV.Content);
            inputFileOV.Content = "pl_voice_" + selectedInputVoice + "_media.pck.3.X64." + selectedInputLang;
            CheckFileStatus();
        }


        /* SUMMARY: OutputVoiceUpdate
        Updates the output voice in the overview
        */
        private void OutputVoiceUpdate(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)outputVoiceComboBox.SelectedItem;
            outputVoiceOV.Content = selectedItem.Content;
            OutputFileUpdate();
        }


        /* SUMMARY: OutputLangUpdate
        Updates the output language in the overview
        */
        private void OutputLangUpdate(object sender, RoutedEventArgs e)
        {
            foreach (RadioButton radioButton in outputLangPanel.Children)
            {
                if (radioButton.IsChecked == true)
                {
                    outputLangOV.Content = radioButton.Content;
                    OutputFileUpdate();
                }
            }
        }


        /* SUMMARY: OutputFileUpdate
        Updates the output file name in the overview
        */
        private void OutputFileUpdate()
        {
            string selectedOutputVoice = fileSegments.getVoiceSegment((string)outputVoiceOV.Content);
            string selectedOutputLang = fileSegments.getLangSegment((string)outputLangOV.Content);
            outputFileOV.Content = "pl_voice_" + selectedOutputVoice + "_media.pck.3.X64." + selectedOutputLang;
            CheckFileStatus();
        }


        /* SUMMARY: CheckFileStatus
        Checks if the given conversion is valid
        */
        private void CheckFileStatus()
        {

            // IF: There's no file imported, wait for the user to import one
            if (currentFilePath == null)
            {
                statusLabel.Text = "WAITING...";
                return;
            }

            bool errorPresent = false;

            // TRY: Checking the ready flag for the input file
            try
            {
                //IF: The voice file isn't ready, throw an exception
                if (!voiceDatabase.IsReady((string)inputFileOV.Content))
                {
                    throw new InvalidOperationException();
                }
                inputFileOV.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
            }
            // CATCH: The voice file isn't ready, raise the corresponding error
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.Data.SQLite.SQLiteException)
            {
                inputFileOV.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF0000");
                errorType = 1;
                errorPresent = true;
            }

            // TRY: Checking the ready flag for the output file
            try
            {
                //IF: The voice file isn't ready, throw an exception
                if (!voiceDatabase.IsReady((string)outputFileOV.Content))
                {
                    throw new InvalidOperationException();
                }
                outputFileOV.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");
            }
            // CATCH: The voice file isn't ready, raise the corresponding error
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.Data.SQLite.SQLiteException)
            {
                outputFileOV.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF0000");
                errorType = 1;
                errorPresent = true;
            }

            // IF: An error was present, state so
            if (errorPresent)
            {
                statusLabel.Text = "ERROR";
            }
            // ELSE: if not, the conversion is ready
            else
            {
                statusLabel.Text = "READY";
                errorType = 0;
            }

        }


        /* SUMMARY: CloseProgram
        Shutdowns the entire program
        Mainly to make sure that the PCK Editor isn't still running in the background!
        */
        private void CloseProgram(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }


        /* SUMMARY: ErrorBoxHandler
        Handles user error messages
        */
        private void ErrorBoxHandler()
        {
             // SWITCH: Check the current error type
            switch (errorType)
            {
                // CASE 1: One of the files are unsupported
                case 1:
                    MessageBox.Show("One of the files are unsupported.\nYou can check the database spreadsheet to see if those files are supported.", "Error: Unsupported File", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                // CASE 2: Bad import file path
                case 2:
                    MessageBox.Show("Unable to read the imported file.\nEither it's unsupported or the incorrect format.", "Error: Bad Import Path", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                // CASE 3: The DB failed to load
                case 3:
                    MessageBox.Show("Unable to load 'MHRVoiceDatabase.db'.\nMake sure it's updated and it's in the same folder as this program!", "Error: Can't Read Database", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

    }
    public class ModeSelect : DataTemplateSelector
    {
        public DataTemplate NoneType { get; set; }
        public DataTemplate REEngine { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                return REEngine;
            }
            else
            {
                return NoneType;
            }
        }
    }
}

