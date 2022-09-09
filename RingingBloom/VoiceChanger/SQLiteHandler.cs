using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Markup.Localizer;

namespace MHRVoiceChanger.VoiceChanger
{
    /* SUMMARY: SQLiteHandler
    A class to handle SQLite queries
    */
    public class SQLiteHandler
    {
        //Directory paths for "inputWems", "outputWems", and the MHR Voice Database SQlite file
        private string tempDirPathIn = Directory.GetCurrentDirectory() + "/inputWems/";
        private string tempDirPathOut = Directory.GetCurrentDirectory() + "/outputWems/";
        private string dbPath = @"URI=file:" + Directory.GetCurrentDirectory() + "/MHRVoiceDatabase.db";

        private SQLiteConnection conn = null;

        public SQLiteHandler()
        {
            // Create a connection with the MHR Voice Database and open it
            conn = new SQLiteConnection(dbPath);
            conn.Open();
        }

        /* SUMMARY: GetDBVersion
            Gets the database version of the MHR Voice Database

        RETURN:
            The database version # or -1 if an error occurs.
        */
        public double GetDBVersion()
        {
            // These 3 lines are for SQLite queries. It starts with the query string, the query as a SQL command, then finally a DbDataReader to get the row
            string query = "SELECT * FROM VERSION";

            var cmd = new SQLiteCommand(query, conn);
            SQLiteDataReader rdr = cmd.ExecuteReader();

            rdr.Read();
            return rdr.GetDouble(0);
        }


        /* SUMMARY: GetVoiceInfo
            Gets the voice information from its file name

        PARAMETERS:
            targetFileName - The name of the voice file

        RETURN:
            The row containing the voice information
        */
        private SQLiteDataReader GetVoiceInfo(string targetFileName)
        {
            string query = "SELECT * FROM VOICES WHERE File_Name = '" + targetFileName + "'";

            var cmd = new SQLiteCommand(query, conn);
            SQLiteDataReader rdr = cmd.ExecuteReader();

            rdr.Read();
            return rdr;
        }

        /* SUMMARY: GetWemEventInfo
        Gets the voice event information from its wem number

        PARAMETERS:
            targetVoice - The voice to check the information of
            targetWemNumber - The wem number to look for

        RETURN:
            An int array with the following info:
                ints[0] - Voice Event
                ints[1] - Wem Count
        */
        private int[] GetWemEventInfo(string targetVoice, string targetWemNumber)
        {
            string query = "SELECT Voice_Event, Wem_Count FROM " + targetVoice + " WHERE Wem_Number = " + targetWemNumber;

            var cmd = new SQLiteCommand(query, conn);
            SQLiteDataReader rdr = cmd.ExecuteReader();

            rdr.Read();
            int[] ints = new int[] { rdr.GetInt32(0), rdr.GetInt32(1) };
            return ints;
        }

        /* SUMMARY: GetWemNumber
        Gets the wem number from the voice event information

        PARAMETERS:
            targetVoice - The voice to get the wem number from
            targetVoiceEvent - The voice event assigned to that wem number
            targetVoiceEvent - The wem count assigned to that wem number

        RETURN:
            The wem number assigned to that info
        */
        private int GetWemNumber(string targetVoice, int targetVoiceEvent, int targetWemCount)
        {
            string query = "SELECT Wem_Number FROM " + targetVoice + " WHERE Voice_Event = " + targetVoiceEvent + " AND Wem_Count = " + targetWemCount;
            var cmd = new SQLiteCommand(query, conn);
            SQLiteDataReader rdr = cmd.ExecuteReader();

            rdr.Read();
            return rdr.GetInt32(0);
        }

        /* SUMMARY: ExceptionCaseHandler
        Handles voice exception cases if they occur
        Current Exceptions Covered:
            Voice 19

        PARAMETERS:
            targetInputVoice - The voice imported by the user
            targetOutputVoice - The voice requested by the user
            targetExceptionCase - the current exception case

        */
        private void ExceptionCaseHandler(string targetInputVoice, string targetOutputVoice, int targetExceptionCase)
        {
            // SWITCH: Check the current exception case
            switch (targetExceptionCase)
            {
                // CASE 1: The input voice is Voice 19 only
                // Remove the extra voice lines before converting
                case 1:
                    // FOR: Voice Event 19, Wem Counts 4 & 5, delete those files
                    for (int x = 4; x < 6; x++)
                    {
                        int wemNumber = GetWemNumber(targetInputVoice, 19, x);
                        string wemPath = tempDirPathIn + wemNumber + ".wem";
                        File.Delete(wemPath);
                    }

                    break;
                // CASE -1: The output voice is Voice 19 only
                // copy the extra voice lines before converting
                case -1:
                    // FOR: Voice Event 19, Wem Counts 4 & 5, create a copy for those files from Wem Counts 0 & 1 (x-4)
                    for (int x = 4; x < 6; x++)
                    {
                        string inputWemNumber = GetWemNumber(targetInputVoice, 19, x-4).ToString();

                        // WHILE: the the wem DOESN'T have 3 digits, add leading 0's until it does
                        while (inputWemNumber.Length < 3)
                        {
                            inputWemNumber = 0 + inputWemNumber;
                        }

                        int outputWemNumber = GetWemNumber(targetOutputVoice, 19, x);
                        string inputWemPath = tempDirPathIn + inputWemNumber + ".wem";
                        string outputWemPath = tempDirPathOut + outputWemNumber + ".wem";
                        File.Copy(inputWemPath, outputWemPath);
                    }
                    break;
            }
        }


        /* SUMMARY: ConvertVoiceWEMs
        Renames the wems from the input voice to work with the output voice
        This is the main function of this class

        PARAMETERS:
            targetInputVoice - The voice imported by the user
            targetOutputVoice - The voice requested by the user

        */
        public void ConvertVoiceWEMs(string targetInputVoice, string targetOutputVoice)
        {
            // Get the voice information on each voice
            SQLiteDataReader inputVoiceInfo = GetVoiceInfo(targetInputVoice);
            SQLiteDataReader outputVoiceInfo = GetVoiceInfo(targetOutputVoice);
            // Get their table names
            string inputVoice = inputVoiceInfo.GetString(0);
            string outputVoice = outputVoiceInfo.GetString(0);

            // Check for voice exception cases
            int inputVoiceCase = inputVoiceInfo.GetInt32(2);
            int outputVoiceCase = outputVoiceInfo.GetInt32(2);

            int exceptionCase = inputVoiceCase - outputVoiceCase;
            // IF: This conversion is a voice exception case, handle it
            if (exceptionCase != 0)
            {
                ExceptionCaseHandler(inputVoice, outputVoice, exceptionCase);
            }


            // IF: Both the "inputWems" AND "outputWems" directories EXISTS, continue
            if (Directory.Exists(tempDirPathIn) && Directory.Exists(tempDirPathOut))
            {
                // Get the each of the files in "inputWems"
                DirectoryInfo tempDI = new DirectoryInfo(tempDirPathIn);
                FileInfo[] fileInfos = tempDI.GetFiles();
                // FOREACH: File in "inputWems", rename them accordingly
                foreach (FileInfo file in fileInfos)
                {
                    // Remove the file extension to isolate the wem number
                    string wemNumber = file.Name.Split(".")[0];
                    // Get the wem number's information
                    int[] wemInfo = GetWemEventInfo(inputVoice, wemNumber);
                    // Use that information to get the corresponding wem number from the output voice
                    int outputNumber = GetWemNumber(outputVoice, wemInfo[0], wemInfo[1]);
                    string outputWem = outputNumber + ".wem";

                    // WHILE: the file DOESN'T have this format: ###.wem, add leading 0's until it does
                    // ###.wem has a total of 7 characters
                    while (outputWem.Length < 7)
                    {
                        outputWem = 0 + outputWem;
                    }

                    //Move the file to "outputWems" and rename it
                    File.Move(file.FullName, tempDirPathOut + outputWem);
                }
            }
        }
    }
}
