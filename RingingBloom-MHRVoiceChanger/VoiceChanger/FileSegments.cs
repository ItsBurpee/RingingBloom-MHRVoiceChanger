using System.Collections.Generic;

namespace MHRVoiceChanger.VoiceChanger
{
    /* SUMMARY: FileSegments
    A class to handle the overview's file name
    It mainly consists of Dictionaries that are used to translate the voice file both ways
    */
    public class FileSegments
    {
        private Dictionary<string, string> voiceSegment = new Dictionary<string, string>()
        {
            {"Voice 1", "m_01"},
            {"Voice 2", "m_02"},
            {"Voice 3", "m_03"},
            {"Voice 4", "m_04"},
            {"Voice 5", "m_05"},
            {"Voice 6", "m_06"},
            {"Voice 7", "m_07"},
            {"Voice 8", "m_08"},
            {"Voice 9", "m_09"},
            {"Voice 10", "m_10"},
            {"Voice 11", "f_11"},
            {"Voice 12", "f_12"},
            {"Voice 13", "f_13"},
            {"Voice 14", "f_14"},
            {"Voice 15", "f_15"},
            {"Voice 16", "f_16"},
            {"Voice 17", "f_17"},
            {"Voice 18", "f_18"},
            {"Voice 19", "f_19"},
            {"Voice 20", "f_20"},
        };

        private Dictionary<string, string> voiceSegmentInverse = new Dictionary<string, string>()
        {
            {"m_01", "Voice 1"},
            {"m_02", "Voice 2"},
            {"m_03", "Voice 3"},
            {"m_04", "Voice 4"},
            {"m_05", "Voice 5"},
            {"m_06", "Voice 6"},
            {"m_07", "Voice 7"},
            {"m_08", "Voice 8"},
            {"m_09", "Voice 9"},
            {"m_10", "Voice 10"},
            {"f_11", "Voice 11"},
            {"f_12", "Voice 12"},
            {"f_13", "Voice 13"},
            {"f_14", "Voice 14"},
            {"f_15", "Voice 15"},
            {"f_16", "Voice 16"},
            {"f_17", "Voice 17"},
            {"f_18", "Voice 18"},
            {"f_19", "Voice 19"},
            {"f_20", "Voice 20"}
        };

        private Dictionary<string, string> langSegment = new Dictionary<string, string>()
        {
            {"English", "En"},
            {"Japanese", "Ja"},
            {"Monster Hunter Language", "Fc"}
        };

        private Dictionary<string, string> langSegmentInverse = new Dictionary<string, string>()
        {
            {"En", "English"},
            {"Ja", "Japanese"},
            {"Fc", "Monster Hunter Language"}
        };

        public string getVoiceSegment(string targetVoice)
        {
            return voiceSegment[targetVoice];
        }

        public string getVoiceSegmentInverse(string targetSegment)
        {
            return voiceSegmentInverse[targetSegment];
        }

        public string getLangSegment(string targetLang)
        {
            return langSegment[targetLang];
        }

        public string getLangSegmentInverse(string targetSegment)
        {
            return langSegmentInverse[targetSegment];
        }
    }
}
