using MHRVoiceChanger.WWiseTypes.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace MHRVoiceChanger.WWiseTypes.ViewModels
{
    /* EDITED for the MHRVoiceChanger
    General Changes:
         Removed "MassReplace" and edited the "Export" functions
    */
    public class NPCKViewModel  : BaseViewModel
    {
        public NPCKHeader npck = null;//shouldn't have to bind directly to this

        public ObservableCollection<Wem> wems 
        { 
            get {
                if(npck == null)
                {
                    return new ObservableCollection<Wem>();
                }
                else
                {
                    return new ObservableCollection<Wem>(npck.WemList);
                }
                
            } 
            set {
                npck.WemList = new List<Wem>(value);
                OnPropertyChanged("wems"); 
            }
        }
        public ObservableCollection<string> languages
        { 
            get 
            { 
                if(npck == null)
                {
                    return new ObservableCollection<string>();
                }
                else
                {
                    return new ObservableCollection<string>(npck.GetLanguages());
                }
            }
            set => throw new NotImplementedException();
        }

        public NPCKViewModel()
        {
            npck = null;
        }

        public void SetNPCK(NPCKHeader file)
        {
            npck = file;
            OnPropertyChanged("wems");
            OnPropertyChanged("languages");
        }

        public void AddWems(string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                Wem newWem = HelperFunctions.MakeWems(fileName, HelperFunctions.OpenFile(fileName));
                npck.WemList.Add(newWem);
            }
            OnPropertyChanged("wems");
        }

        public void ReplaceWem(Wem newWem,int index)
        {
            newWem.id = npck.WemList[index].id;
            newWem.languageEnum = npck.WemList[index].languageEnum;
            npck.WemList[index] = newWem;
            OnPropertyChanged("wems");
        }
        public void DeleteWem(int index)
        {
            npck.WemList.RemoveAt(index);
            OnPropertyChanged("wems");
        }

        /* EDITED for the MHRVoiceChanger
            Removed the prompt to save with names
            It must save with names to work with the Voice Changer components
        */
        public void ExportWems(string savePath)
        {
            foreach (Wem newWem in npck.WemList)
            {
                string name = savePath + "\\" + newWem.name;
                BinaryWriter bw = new BinaryWriter(new FileStream(name, FileMode.OpenOrCreate));
                bw.Write(newWem.file);
                bw.Close();
            }
        }

        /* EDITED for the MHRVoiceChanger
            Rather than putting the header file alongside the main file, it places it in the correct mod directory for a Fluffy mod
        */
        public void ExportNPCK(string[] filePaths)
        {
            npck.ExportFile(filePaths[0]);
            npck.ExportHeader(filePaths[1]);
        }

        public void IDReplace(string[] id2)
        {
            for (int i = 0; i < id2.Length; i++)
            {
                try
                {
                    npck.WemList[i].id = Convert.ToUInt32(id2[i]);
                }
                catch (ArgumentOutOfRangeException)
                {
                    break;
                }
            }
            OnPropertyChanged("wems");
        }

        public void ExportLabels(SupportedGames mode, string currentFileName, List<uint> changedIds)
        {
            npck.labels.Export(Directory.GetCurrentDirectory() + "/" + mode.ToString() + "/PCK/" + currentFileName + ".lbl", npck.WemList, changedIds);
        }

        public List<uint> GetWemIds()
        {
            List<uint> wemIds = new List<uint>();
            for (int i = 0; i < npck.WemList.Count; i++)
            {
                wemIds.Add(npck.WemList[i].id);
            }
            return wemIds;
        }

    }
}
