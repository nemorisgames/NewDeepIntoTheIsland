#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace AC
{
	
	/**
	 * Provides an EditorWindow to manage the export of variables
	 */
	public class VarExportWizardWindow : EditorWindow
	{

		private VariablesManager variablesManager;
		private List<ExportColumn> exportColumns = new List<ExportColumn>();
		private int sideMenuIndex = -1;

		private Vector2 scroll;


		public void _Init (VariablesManager _variablesManager)
		{
			variablesManager = _variablesManager;

			exportColumns.Clear ();
			exportColumns.Add (new ExportColumn (ExportColumn.ColumnType.Location));
			exportColumns.Add (new ExportColumn (ExportColumn.ColumnType.SceneName));
			exportColumns.Add (new ExportColumn (ExportColumn.ColumnType.Label));
			exportColumns.Add (new ExportColumn (ExportColumn.ColumnType.Type));
			exportColumns.Add (new ExportColumn (ExportColumn.ColumnType.InitialValue));
		}


		/**
		 * <summary>Initialises the window.</summary>
		 */
		public static void Init (VariablesManager _variablesManager)
		{
			if (_variablesManager == null) return;

			VarExportWizardWindow window = EditorWindow.GetWindowWithRect <VarExportWizardWindow> (new Rect (0, 0, 350, 500), true, "Variables exporter", true);
			UnityVersionHandler.SetWindowTitle (window, "Variables exporter");
			window.position = new Rect (300, 200, 350, 500);
			window._Init (_variablesManager);
		}
		
		
		private void OnGUI ()
		{
			if (variablesManager == null)
			{
				EditorGUILayout.HelpBox ("A Variables Manager must be assigned before this window can display correctly.", MessageType.Warning);
				return;
			}

			if (exportColumns == null)
			{
				exportColumns = new List<ExportColumn>();
				exportColumns.Add (new ExportColumn ());
			}

			EditorGUILayout.LabelField ("Variable export wizard", CustomStyles.managerHeader);
			scroll = GUILayout.BeginScrollView (scroll);

			EditorGUILayout.HelpBox ("Choose the fields to export as columns below, then click 'Export CSV'.", MessageType.Info);
			EditorGUILayout.Space ();

			ShowColumnsGUI ();

			EditorGUILayout.Space ();
			if (exportColumns.Count == 0)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button ("Export CSV"))
			{
				Export ();
			}
			GUI.enabled = true;

			EditorGUILayout.Space ();
			GUILayout.EndScrollView ();
		}


		private void ShowColumnsGUI ()
		{
			EditorGUILayout.LabelField ("Define columns",  CustomStyles.subHeader);
			EditorGUILayout.Space ();
			for (int i=0; i<exportColumns.Count; i++)
			{
				EditorGUILayout.BeginVertical ("Button");

				EditorGUILayout.BeginHorizontal ();
				exportColumns[i].ShowFieldSelector (i);
				if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
				{
					SideMenu (i);
				}
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.EndVertical ();
			}

			EditorGUILayout.Space ();
			if (GUILayout.Button ("Add new column"))
			{
				exportColumns.Add (new ExportColumn ());
			}

			EditorGUILayout.Space ();
		}


		private void SideMenu (int i)
		{
			GenericMenu menu = new GenericMenu ();

			sideMenuIndex = i;

			if (exportColumns.Count > 1)
			{
				if (i > 0)
				{
					menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, MenuCallback, "Move to top");
					menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, MenuCallback, "Move up");
				}
				if (i < (exportColumns.Count - 1))
				{
					menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, MenuCallback, "Move down");
					menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, MenuCallback, "Move to bottom");
				}
				menu.AddSeparator ("");
			}
			menu.AddItem (new GUIContent ("Delete"), false, MenuCallback, "Delete");
			menu.ShowAsContext ();
		}


		private void MenuCallback (object obj)
		{
			if (sideMenuIndex >= 0)
			{
				int i = sideMenuIndex;
				ExportColumn _column = exportColumns[i];

				switch (obj.ToString ())
				{
				case "Move to top":
					exportColumns.Remove (_column);
					exportColumns.Insert (0, _column);
					break;
					
				case "Move up":
					exportColumns.Remove (_column);
					exportColumns.Insert (i-1, _column);
					break;
					
				case "Move to bottom":
					exportColumns.Remove (_column);
					exportColumns.Insert (exportColumns.Count, _column);
					break;
					
				case "Move down":
					exportColumns.Remove (_column);
					exportColumns.Insert (i+1, _column);
					break;

				case "Delete":
					exportColumns.Remove (_column);
					break;
				}
			}
			
			sideMenuIndex = -1;
		}

		
		private void Export ()
		{
			#if UNITY_WEBPLAYER
			ACDebug.LogWarning ("Game text cannot be exported in WebPlayer mode - please switch platform and try again.");
			#else
			
			if (variablesManager == null || exportColumns == null || exportColumns.Count == 0) return;

			bool canProceed = EditorUtility.DisplayDialog ("Export variables", "AC will now go through your game, and collect all variables to be exported.\n\nIt is recommended to back up your project beforehand.", "OK", "Cancel");
			if (!canProceed) return;

			if (!UnityVersionHandler.SaveSceneIfUserWants ())
			{
				return;
			}

			string suggestedFilename = "";
			if (AdvGame.GetReferences ().settingsManager)
			{
				suggestedFilename = AdvGame.GetReferences ().settingsManager.saveFileName + " - ";
			}
			suggestedFilename += "Variables.csv";
			
			string fileName = EditorUtility.SaveFilePanel ("Export variables", "Assets", suggestedFilename, "csv");
			if (fileName.Length == 0)
			{
				return;
			}

			List<GVar> exportVars = new List<GVar>();
			foreach (GVar globalVariable in variablesManager.vars)
			{
				exportVars.Add (new GVar (globalVariable));
			}

			bool fail = false;
			List<string[]> output = new List<string[]>();

			List<string> headerList = new List<string>();
			headerList.Add ("ID");
			foreach (ExportColumn exportColumn in exportColumns)
			{
				headerList.Add (exportColumn.GetHeader ());
			}
			output.Add (headerList.ToArray ());
			
			foreach (GVar exportVar in exportVars)
			{
				List<string> rowList = new List<string>();
				rowList.Add (exportVar.id.ToString ());
				foreach (ExportColumn exportColumn in exportColumns)
				{
					string cellText = exportColumn.GetCellText (exportVar, VariableLocation.Global);
					rowList.Add (cellText);

					if (cellText.Contains (CSVReader.csvDelimiter))
					{
						fail = true;
						ACDebug.LogError ("Cannot export variables since global variable " + exportVar.id.ToString () + " (" + exportVar.label + ") contains the character '" + CSVReader.csvDelimiter + "'.");
					}
				}
				output.Add (rowList.ToArray ());
			}

			// Local
			int numLocalVars = 0;
			string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
			string[] sceneFiles = AdvGame.GetSceneFiles ();
			foreach (string sceneFile in sceneFiles)
			{
				UnityVersionHandler.OpenScene (sceneFile);

				List<GVar> localExportVars = new List<GVar>();
				if (FindObjectOfType <LocalVariables>())
				{
					LocalVariables localVariables = FindObjectOfType <LocalVariables>();
					string sceneName = UnityVersionHandler.GetCurrentSceneName ();

					foreach (GVar localVariable in localVariables.localVars)
					{
						localExportVars.Add (new GVar (localVariable));
					}

					foreach (GVar localExportVar in localExportVars)
					{
						numLocalVars ++;

						List<string> rowList = new List<string>();
						rowList.Add (localExportVar.id.ToString ());
						foreach (ExportColumn exportColumn in exportColumns)
						{
							string cellText = exportColumn.GetCellText (localExportVar, VariableLocation.Local, sceneName);
							rowList.Add (cellText);

							if (cellText.Contains (CSVReader.csvDelimiter))
							{
								fail = true;
								ACDebug.LogError ("Cannot export variables since local variable " + localExportVar.id.ToString () + " (" + localExportVar.label + ") contains the character '" + CSVReader.csvDelimiter + "'.");
							}
						}
						output.Add (rowList.ToArray ());
					}
				}
			}

			if (originalScene == "")
			{
				UnityVersionHandler.NewScene ();
			}
			else
			{
				UnityVersionHandler.OpenScene (originalScene);
			}

			if (!fail)
			{
				int length = output.Count;
				
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				for (int j=0; j<length; j++)
				{
					sb.AppendLine (string.Join (CSVReader.csvDelimiter, output[j]));
				}
				
				if (Serializer.SaveFile (fileName, sb.ToString ()))
				{
					ACDebug.Log ((exportVars.Count-1).ToString () + " global variables, " + numLocalVars.ToString () + " local variables exported.");
				}
			}

			//this.Close ();
			#endif
		}


		private class ExportColumn
		{

			public enum ColumnType { Location, SceneName, Label, Type, Description, InitialValue };
			private ColumnType columnType;


			public ExportColumn ()
			{
				columnType = ColumnType.Label;
			}


			public ExportColumn (ColumnType _columnType)
			{
				columnType = _columnType;
			}


			public void ShowFieldSelector (int i)
			{
				columnType = (ColumnType) EditorGUILayout.EnumPopup ("Column #" + (i+1).ToString (), columnType);
			}


			public string GetHeader ()
			{
				return columnType.ToString ();
			}


			public string GetCellText (GVar variable, VariableLocation location, string sceneName = "")
			{
				string cellText = " ";

				switch (columnType)
				{
					case ColumnType.Location:
						cellText = location.ToString ();
						break;

					case ColumnType.SceneName:
						if (location == VariableLocation.Local)
						{
							cellText = sceneName;
						}
						break;

					case ColumnType.Label:
						cellText = variable.label;
						break;

					case ColumnType.Type:
						cellText = variable.type.ToString ();
						break;

					case ColumnType.Description:
						cellText = variable.description;
						break;

					case ColumnType.InitialValue:
						cellText = variable.GetValue ();
						break;
				}

				if (cellText == "") cellText = " ";
				return RemoveLineBreaks (cellText);
			}


			private string RemoveLineBreaks (string text)
			{
				if (text.Length == 0) return " ";
	            //text = text.Replace("\r\n", "[break]").Replace("\n", "[break]");
				text = text.Replace("\r\n", "[break]");
				text = text.Replace("\n", "[break]");
				text = text.Replace("\r", "[break]");
	            return text;
	        }

		}

	}
	
}

#endif