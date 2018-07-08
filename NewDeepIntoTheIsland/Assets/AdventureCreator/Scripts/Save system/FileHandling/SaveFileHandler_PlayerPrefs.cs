using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	public class SaveFileHandler_PlayerPrefs : iSaveFileHandler
	{

		public string GetDefaultSaveLabel (int saveID)
		{
			string label = "Save " + saveID.ToString ();
			if (saveID == 0)
			{
				label = "Autosave";
			}
			return label;
		}


		public void DeleteAll (int profileID)
		{
			List<SaveFile> allSaveFiles = GatherSaveFiles (profileID);
			foreach (SaveFile saveFile in allSaveFiles)
			{
				Delete (saveFile);
			}
		}


		public bool Delete (SaveFile saveFile)
		{
			string filename = saveFile.fileName;

			if (PlayerPrefs.HasKey (filename))
			{
				PlayerPrefs.DeleteKey (filename);
				ACDebug.Log ("PlayerPrefs key deleted: " + filename);
				return true;
			}
			return false;
		}


		public void Save (SaveFile saveFile, string dataToSave)
		{
			string fullFilename = GetSaveFilename (saveFile.saveID, saveFile.profileID);
			bool isSuccessful = false;

			try
			{
				PlayerPrefs.SetString (fullFilename, dataToSave);
				#if UNITY_PS4
				PlayerPrefs.Save ();
				#endif
				ACDebug.Log ("PlayerPrefs key written: " + fullFilename);
				isSuccessful = true;
			}
			catch (System.Exception e)
 			{
				ACDebug.LogWarning ("Could not save PlayerPrefs data under key " + fullFilename + ". Exception: " + e);
 			}

			KickStarter.saveSystem.OnFinishSaveRequest (saveFile, isSuccessful);
		}


		public void Load (SaveFile saveFile, bool doLog)
		{
			string filename = saveFile.fileName;
			string _data = PlayerPrefs.GetString (filename, "");
			
			if (doLog && _data != "")
			{
				ACDebug.Log ("PlayerPrefs key read: " + filename);
			}

			KickStarter.saveSystem.ReceiveDataToLoad (saveFile, _data);
		}


		public void Import (SaveFile saveFile, bool doLog)
		{
			string filename = saveFile.fileName;
			string _data = PlayerPrefs.GetString (filename, "");
			
			if (doLog && _data != "")
			{
				ACDebug.Log ("PlayerPrefs key read: " + filename);
			}

			KickStarter.saveSystem.ReceiveDataToImport (saveFile, _data);
		}


		public List<SaveFile> GatherSaveFiles (int profileID)
		{
			return GatherSaveFiles (profileID, false, -1, "");
		}


		public List<SaveFile> GatherImportFiles (int profileID, int boolID, string separateProductName, string separateFilePrefix)
		{
			if (!string.IsNullOrEmpty (separateProductName) && !string.IsNullOrEmpty (separateFilePrefix))
			{
				return GatherSaveFiles (profileID, true, boolID, separateFilePrefix);
			}
			return null;
		}


		private List<SaveFile> GatherSaveFiles (int profileID, bool isImport, int boolID, string separateFilePrefix)
		{
			List<SaveFile> gatheredSaveFiles = new List<SaveFile>();

			for (int i=0; i<50; i++)
			{
				bool isAutoSave = false;
				string filename = (isImport) ? GetImportFilename (i, separateFilePrefix, profileID) : GetSaveFilename (i, profileID);

				if (PlayerPrefs.HasKey (filename))
				{
					string label = "Save " + i.ToString ();
					if (i == 0)
					{
						label = "Autosave";
						isAutoSave = true;
					}
					gatheredSaveFiles.Add (new SaveFile (i, profileID, label, filename, isAutoSave, null, "", 0));
				}
			}

			return gatheredSaveFiles;
		}


		private string GetSaveFilename (int saveID, int profileID = -1)
		{
			if (profileID == -1)
			{
				profileID = Options.GetActiveProfileID ();
			}

			return KickStarter.settingsManager.SavePrefix + KickStarter.saveSystem.GenerateSaveSuffix (saveID, profileID);
		}


		public void SaveScreenshot (SaveFile saveFile)
		{}


		private string GetImportFilename (int saveID, string filePrefix, int profileID = -1)
		{
			if (profileID == -1)
			{
				profileID = Options.GetActiveProfileID ();
			}

			return filePrefix + KickStarter.saveSystem.GenerateSaveSuffix (saveID, profileID);
		}

	}

}