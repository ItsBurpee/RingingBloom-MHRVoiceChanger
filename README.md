# MHRVoiceChanger
Allow the reassigning of voices to other voices. This means that any voice or voice mod is compatible with (almost) any other voice!

This is a modified version of Silvris's RingingBloom tool that's written in C#
It mainly focuses on using it's PCK Editor

You can download the latest version in the Releases: https://github.com/ItsBurpee/RingingBloom-MHRVoiceChanger/releases

## How to Use:
1. Load the .pck file with the "Import File" button (It should auto-detect its information in the "Overview")
	- IMPORTANT: Make sure you get the "streaming" file! There's another file with the same name in a different directory.
  		- That file can be found with this directory: .../natives/STM/streaming/Sound/Wwise/
2. Select the voice to convert to with the "Output Voice" options
3. Click the "CONVERT" button to convert

- If the conversion is successful, your new voice mod should be in a folder named "Output Voice Mod"
	- This folder is ready for Fluffy Manager but it's recommended to rename the "modinfo.ini" file according to what you converted
- If the conversion failed, check the error message that comes up

- An in-depth description about what this program does can be found under **"Conversion Explained"**

## Errors:
Red text indicates issues or unsupported voices outside of the Imported File Path

- DB Version: ERROR
	- The program can't read the "version" table from the database file: MHRCharacterVoices.db
	- Make sure the MHRCharacterVoices.db is in the same folder as the .exe
- Overview File
	- The selected voice doesn't have information on it
	- I do apologize if you're on one of those voices. I'll get to it at some point
	- You can check if your voice is supported in the spreadsheet under **"Other Notes"**
- Overview Imported File
	- The file described by the import path couldn't be found
	- This is for the case where the file's location changes before the "CONVERT" button was pressed
  
## Other Notes:
- This program opens a console alongside itself
	- This is used to track errors or progress
	- If your conversion fails, look for errors in the console
- There is a "Manual Mode" option in the "Input Voice" section
	- There shoudn't be a case where you have to turn this on
		- DO NOT use this mode to bypass any of the errors described above
- MHRCharacterVoices.db
	- A database that holds information about character voices
	- A link to the spreadsheet it's based on is here: https://docs.google.com/spreadsheets/d/1r2K97igdGBfjWyFoHlzHB75ECZ6SqfS209V8WWGxy18/
    
## Conversion Explained:
When this program opens, RingingBloom's PCK editor runs in the background. When you close this program, the PCK editor will do the same

Upon pressing the "CONVERT" button, the program will do the following: (Sections in bracket is the RingingBloom equivalent)
1. Read the file provided in the "Imported File" section and give it to the PCK editor (RingingBloom: Import)
2. Create an "inputWems" folder and export the wems into it (RingingBloom: Export Wems, Export with names)
3. Create an "outputWems" folder and use the SQLite database to rename and move each wem to that folder
	- There can be voice exception cases during this step:
		- Voice 19 is only on 1 of the target files
			- This file has 2 more voice files than the standard file
			- On input, it will delete the 2 extra files
			- On output, it will copy the first 2 files assigned to that category
4. Reimport the wems into the PCK editor and reassign their IDs (RingingBloom: New -> ID Replace)
	- At this point, the "inputWems" and "outputWems" folders are deleted
5. Export the PCK file into a Fluffy ready folder (RingingBloom: Export)
	- The file is exported to this path: ./Output Voice Mod/natives/STM/streaming/Sound/Wwise/
	- The header file is exported to this path: ./Output Voice Mod/natives/STM/Sound/Wwise/
6. Create a "modinfo.ini" file in the Fluffy ready folder with some basic information
  
### Thanks To/Dependencies:
- Silvris and everyone else who contributed to the RingingBloom tool
	- The link to that tool's GitHub is here: https://github.com/Silvris/RingingBloom
- D. Richard Hipp for SQLite3

### To-do:
- Cover more voices
	- This would also progress the program's version
- More features
	- Give the user some options to play around with
- Optimization
	- The program's size and runtime could be improved?

