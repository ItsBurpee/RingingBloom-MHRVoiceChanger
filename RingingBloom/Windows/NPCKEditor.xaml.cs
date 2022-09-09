using MHRVoiceChanger;
using MHRVoiceChanger.WWiseTypes;
using MHRVoiceChanger.WWiseTypes.Common;
using MHRVoiceChanger.WWiseTypes.ViewModels;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace RingingBloom.Windows
{
    /// <summary>
    /// Interaction logic for NPCKEditor.xaml
    /// </summary>

    /* EDITED for the MHRVoiceChanger
    A lot of the file has been overhaul to work with the Voice Changer changes
    At this point, the window assigned to this class is more of a PCK viewer instead of an editor
    */
    public partial class NPCKEditor : Window
    {
        public SupportedGames mode = SupportedGames.MHRise;
        public NPCKViewModel viewModel { get; set; }
        private string ImportPath = null;
        private string ExportPath = null;

        //Directory paths for "inputWems", "outputWems", and the Fluffy template's main folder and header folder
        private string tempDirPathIn = Directory.GetCurrentDirectory() + "/inputWems/";
        private string tempDirPath2 = Directory.GetCurrentDirectory() + "/outputWems/";
        private string modDirPath = Directory.GetCurrentDirectory() + "/Output Voice Mod/natives/STM/streaming/Sound/Wwise/";
        private string modNonStreamDirPath = Directory.GetCurrentDirectory() + "/Output Voice Mod/natives/STM/Sound/Wwise/";

        public NPCKEditor(SupportedGames Mode,Options options)
        {
            InitializeComponent();
            mode = Mode;
            viewModel = new NPCKViewModel();
            WemView.DataContext = viewModel;
            if(options.defaultImport != null)
            {
                ImportPath = options.defaultImport;
            }
            if (options.defaultExport != null)
            {
                ExportPath = options.defaultExport;
            }
        }

        private void Import_Wems(object sender, RoutedEventArgs e)
        {
            if (viewModel.npck == null)
            {
                MessageBox.Show("NPCK not loaded.");
                return;
            }
            OpenFileDialog openFile = new OpenFileDialog();
            if(ImportPath != null)
            {
                openFile.InitialDirectory = ImportPath;
            }
            openFile.Multiselect = true;
            openFile.Filter = "WWise Wem files (*.wem)|*.wem";
            if (openFile.ShowDialog() == true)
            {
                viewModel.AddWems(openFile.FileNames);
            }


        }

        /* EDITED for the MHRVoiceChanger
            The destination for the wems to be exported is predetermined: "inputWems" & "outputWems"
        */
        public void Export_Wems()
        {
            if (viewModel.npck == null)
            {
                MessageBox.Show("NPCK not loaded.");
                return;
            }
            // Setup the directories for "inputWems" and "outputWems"
            SetupDirectory(tempDirPathIn);
            SetupDirectory(tempDirPath2);
            viewModel.ExportWems(tempDirPathIn);
        }

        /* SUMMARY: Reimport_Wems
            Reimports the wems from "outputWems" back into the PCK Editor
        */
        public void Reimport_Wems()
        {
            viewModel.SetNPCK(new NPCKHeader(mode));

            string[] reimportedWems = new string[Directory.GetFiles(tempDirPath2).Length];
            int arrayIndex = 0;
            // FOREACH: File in "outputWems", add them to an array for reimporting
            foreach (string filePath in Directory.GetFiles(tempDirPath2))
            {
                reimportedWems[arrayIndex] = filePath;
                arrayIndex++;
            }

            viewModel.AddWems(reimportedWems);

            DismantleDirectory(tempDirPathIn);
            DismantleDirectory(tempDirPath2);
        }

        /* SUMMARY: SetupDirectory
            Setups a directory
            That is, empty the directory for usage if it exist or create it if it doesn't

        PARAMETERS:
            targetDirectory - The directory that is to be setup

        */
        private void SetupDirectory(string targetDirectory)
        {
            // IF: the directory EXISTS, clear its contents
            if (Directory.Exists(targetDirectory))
            {
                DirectoryInfo tempDI = new DirectoryInfo(targetDirectory);
                foreach (FileInfo file in tempDI.EnumerateFiles())
                {
                    file.Delete();
                }
            }
            // ELSE: the directory DOESN'T EXIST, create it
            else
            {
                Directory.CreateDirectory(targetDirectory);
            }
        }

        /* SUMMARY: DismantleDirectory
        clears then deletes a directory

        PARAMETERS:
            targetDirectory - The directory that is to be dismantled

        */
        private void DismantleDirectory(string targetDirectory)
        {
            // IF: the directory EXISTS, clear its contents
            if (Directory.Exists(targetDirectory))
            {
                DirectoryInfo tempDI = new DirectoryInfo(targetDirectory);
                foreach (FileInfo file in tempDI.EnumerateFiles())
                {
                    file.Delete();
                }
                Directory.Delete(targetDirectory, false);
            }
        }

        /* EDITED for the MHRVoiceChanger
            The destination for the PCK file to be exported is predetermined: A Fluffy ready mod folder template
        */
        public void ExportPCK(string targetFileName)
        {
            SetupDirectory(modDirPath);
            SetupDirectory(modNonStreamDirPath);
            string[] targetFilePaths = new string[] { modDirPath + targetFileName, modNonStreamDirPath + targetFileName };
            viewModel.ExportNPCK(targetFilePaths);

        }

        /* EDITED for the MHRVoiceChanger
            The IDList string is predetermined by the file given
        */
        public void IDReplace(string targetIDList)
        {
            string[] id2 = targetIDList.Split(',');
            viewModel.IDReplace(id2);
        }

    }
}
