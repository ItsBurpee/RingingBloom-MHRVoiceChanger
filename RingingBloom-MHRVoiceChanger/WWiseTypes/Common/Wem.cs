﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MHRVoiceChanger.WWiseTypes.Common
{
    public class Wem
    {
        public string name { get; set; }
        public uint id { get; set; }
        public uint length { get; set; }
        public uint languageEnum { get; set; }
        public byte[] file;
        public bool nameChanged = false;

        public Wem(string aName, string aId, BinaryReader aFile)
        {
            name = Path.GetFileName(aName);
            id = Convert.ToUInt32(aId);
            length = (uint)aFile.BaseStream.Length;
            file = aFile.ReadBytes((int)length);
            languageEnum = 0;
            aFile.Close();
        }

        public Wem(string aName, uint aID, byte[] aBinary)
        {
            name = aName;
            id = aID;
            length = (uint)aBinary.Length;
            file = aBinary;
            languageEnum = 0;
        }
        public Wem(string aName, uint aID, byte[] aBinary, uint lanEnum)
        {
            //the boolean only exists on pck, not bnk thus only needs to be generated by pck
            name = aName;
            id = aID;
            length = (uint)aBinary.Length;
            file = aBinary;
            languageEnum = lanEnum;
        }
    }
}
