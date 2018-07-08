using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace AC
{
	
	public class ActiveInputsWindow : EditorWindow
	{
		
		private SettingsManager settingsManager;
		private Vector2 scrollPos;

		
		[MenuItem ("Adventure Creator/Editors/Active Inputs Editor", false, 0)]
		public static void Init ()
		{
			ActiveInputsWindow window = GetWindowWithRect <ActiveInputsWindow> (new Rect (0, 0, 450, 460), true, "Active inputs", true);
			UnityVersionHandler.SetWindowTitle (window, "Active Inputs");
			window.position = new Rect (300, 200, 450, 460);
		}
		
		
		private void OnEnable ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
		}
		
		
		private void OnGUI ()
		{
			if (settingsManager == null)
			{
				EditorGUILayout.HelpBox ("A Settings Manager must be assigned before this window can display correctly.", MessageType.Warning);
				return;
			}

			ActiveInput.Upgrade ();
			settingsManager.activeInputs = ShowActiveInputsGUI (settingsManager.activeInputs);

			UnityVersionHandler.CustomSetDirty (settingsManager);
		}
		
		
		private List<ActiveInput> ShowActiveInputsGUI (List<ActiveInput> activeInputs)
		{
			EditorGUILayout.HelpBox ("Active Inputs are used to trigger ActionList assets when an input key is pressed under certain gameplay conditions.", MessageType.Info);

			scrollPos = EditorGUILayout.BeginScrollView (scrollPos);

			for (int i=0; i<activeInputs.Count; i++)
			{
				EditorGUILayout.BeginVertical (CustomStyles.thinBox);

				string defaultName = "ActiveInput_" + activeInputs[i].inputName;
				if (activeInputs[i].inputName == "") defaultName = "ActiveInput_" + i.ToString ();

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Input #" + activeInputs[i].ID, EditorStyles.boldLabel);
				if (GUILayout.Button ("-", GUILayout.Width (20f)))
				{
					activeInputs.RemoveAt (i);
					return activeInputs;
				}
				EditorGUILayout.EndHorizontal ();
				activeInputs[i].inputName = EditorGUILayout.TextField ("Input button:", activeInputs[i].inputName);
				activeInputs[i].inputType = (SimulateInputType) EditorGUILayout.EnumPopup ("Input type:", activeInputs[i].inputType);
				if (activeInputs[i].inputType == SimulateInputType.Axis)
				{
					activeInputs[i].axisThreshold = EditorGUILayout.Slider ("Axis threshold:", activeInputs[i].axisThreshold, -1f, 1f);
				}
				activeInputs[i].enabledOnStart = EditorGUILayout.Toggle ("Enabled by default?", activeInputs[i].enabledOnStart);
				activeInputs[i].gameState = (GameState) EditorGUILayout.EnumPopup ("Available when game is:", activeInputs[i].gameState);
				activeInputs[i].actionListAsset = ActionListAssetMenu.AssetGUI ("ActionList when triggered:", activeInputs[i].actionListAsset, "", defaultName);

				EditorGUILayout.EndVertical ();
			}

			if (activeInputs.Count > 0)
			{
				EditorGUILayout.Space ();
			}

			if (GUILayout.Button ("Create new Active Input"))
			{
				if (activeInputs.Count > 0)
				{
					List<int> idArray = new List<int>();
					foreach (ActiveInput activeInput in activeInputs)
					{
						idArray.Add (activeInput.ID);
					}
					idArray.Sort ();
					activeInputs.Add (new ActiveInput (idArray.ToArray ()));
				}
				else
				{
					activeInputs.Add (new ActiveInput (1));
				}
			}

			EditorGUILayout.Space ();

			EditorGUILayout.EndScrollView ();
			return activeInputs;
		}
		
		
	}
	
}
