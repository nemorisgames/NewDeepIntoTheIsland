using UnityEngine;
using UnityEditor;

namespace AC
{

	[InitializeOnLoad]
	static class UpdateChecker
	{

		private const string versionLink = "https://adventurecreator.org/files/currentversion.txt";
		private const string dialogHeader = "AC update checker";
		private static WWW www;


		[MenuItem ("Adventure Creator/Check for updates", false, 21)]
		public static void CheckForUpdate ()
		{
			if (!IsChecking ())
			{
				www = new WWW (versionLink);
				EditorApplication.update += DoUpdateCheck;
			}
		}


		public static bool IsChecking ()
		{
			return (www != null);
		}


		private static void DoUpdateCheck ()
		{
			if (IsChecking ())
			{
				if (!www.isDone)
				{
					return;
				}

				try
				{
					if (string.IsNullOrEmpty (www.error) && !www.text.Contains ("404"))
					{
						string newVersion = www.text;
						bool isNewer = CompareVersion (newVersion, AdventureCreator.version);

						if (isNewer)
						{
							NeedUpdate (newVersion);
						}
						else
						{
							UpToDate ();
						}
					}
					else
					{
						OnFail ();
					}
				}
				catch (System.Exception e)
				{
					OnFail (e.ToString ());
				}
				
				www = null;
			}

			EditorApplication.update -= DoUpdateCheck;
		}


		private static bool CompareVersion (string newVersion, string oldVersion)
		{
			int[] newVersionArray = VersionToArray (newVersion);
			int[] oldVersionArray = VersionToArray (oldVersion);

			if (newVersionArray.Length >= 2 && oldVersionArray.Length >= 2)
			{
				if (newVersionArray[0] > oldVersionArray[0])
				{
					return true;
				}

				if (newVersionArray[1] > oldVersionArray[1])
				{
					return true;
				}

				if (newVersionArray.Length > 2)
				{
					if (oldVersionArray.Length <= 2)
					{
						return (newVersionArray[2] > 0);
					}

					if (newVersionArray[2] > oldVersionArray[2])
					{
						return true;
					}
				}
			}
			return false;
		}


		private static int[] VersionToArray (string version)
		{
			string[] stringArray = version.Split ("."[0]);
			int[] intArray = new int[stringArray.Length];

			for (int i=0; i<stringArray.Length; i++)
			{
				int a = -1;
				int.TryParse (stringArray[i], out a);
				intArray[i] = a;
			}

			return intArray;
		}


		private static void NeedUpdate (string version)
		{
			if (EditorUtility.DisplayDialog (dialogHeader, "Update available!  New version: " + version + ". Download?", "OK", "Cancel"))
			{
				Application.OpenURL (Resource.assetLink);
			}
		}


		private static void UpToDate ()
		{
			EditorUtility.DisplayDialog (dialogHeader, "You're up to date!", "OK");
		}


		private static void OnFail (string error = "")
		{
			string errorMessage = "Failed to connect to server.";
			if (!string.IsNullOrEmpty (error))
			{
				errorMessage += "\n" + error;
			}

			EditorUtility.DisplayDialog (dialogHeader, errorMessage, "OK");
		}

	}
}
