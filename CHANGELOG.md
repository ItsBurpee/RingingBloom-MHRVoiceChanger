# Version 0.92
MainWindow.xaml.cs
- File status now checks ready flag rather than ID list

MainWindow.xaml
- Enabled the "Monster Hunter Language" option

NPCKEditor.xaml.cs
- Added an error message in case the user imports the header file
- Removed: Import_Wems()
	- Unused function

SQLiteHandler.cs
- Added ready flag getter
- Added ID list getter

Options
- Added a simple import path option

IDLists.cs
- Removed: ID lists are now handled by the DB file

MHRVoiceDatabase.db
- VOICES table
	- Added Ready_Flag
- ID_LISTS tables
	- Added Table: The replacement for "IDLists.cs"
