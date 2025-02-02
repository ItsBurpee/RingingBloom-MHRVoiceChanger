﻿using MHRVoiceChanger.WWiseTypes.Common;
using System.Text;
using System.Xml;

namespace MHRVoiceChanger
{
    public class Options
    {
        public string defaultImport { get; set; }
        public string defaultExport { get; set; }
        public SupportedGames defaultGame { get; set; }

        public Options()
        {
            defaultImport = null;
            defaultExport = null;
            defaultGame = SupportedGames.MHRise;
        }

        public Options(string dImport, string dExport, SupportedGames dGame)
        {
            defaultImport = dImport;
            defaultExport = dExport;
            defaultGame = dGame;
        }

        public Options(XmlReader xml)
        {
            string content;
            while (xml.Read())
            {
                switch (xml.Name)
                {
                    case "DefaultImportPath":
                        content = xml.ReadElementContentAsString();
                        if ( content != "")
                        {
                            defaultImport = content;
                        }
                        break;
                    case "DefaultExportPath":
                        content = xml.ReadElementContentAsString();
                        if (content != "")
                        {
                            defaultExport = content;
                        }
                        break;
                    case "DefaultGame":
                        switch (xml.ReadElementContentAsString())
                        {
                            case "MHRise":
                                defaultGame = SupportedGames.MHRise;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            xml.Close();
        }

        public void WriteOptions()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = Encoding.UTF8;
            settings.OmitXmlDeclaration = true;
            XmlWriter xml = XmlWriter.Create("Options.xml",settings);//this will mean any time the function is invoked, the old is replaced
            xml.WriteStartElement("RingingBloomOptions");
            xml.WriteElementString("DefaultImportPath", defaultImport);
            xml.WriteElementString("DefaultExportPath", defaultExport);
            xml.WriteElementString("DefaultGame", defaultGame.ToString());
            xml.WriteEndElement();
            xml.Flush();
            xml.Close();

        }
    }
}
