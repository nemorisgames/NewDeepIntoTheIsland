/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionSceneCheckAttribute.cs"
 * 
 *	This action checks to see if a scene attribute has been assigned a certain value,
 *	and performs something accordingly.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionSceneCheckAttribute : ActionCheck
	{

		public int attributeID;
		public int attributeNumber;

		public int intValue;
		public float floatValue;
		public IntCondition intCondition;
		public bool isAdditive = false;
		
		public BoolValue boolValue = BoolValue.True;
		public BoolCondition boolCondition;

		public string stringValue;
		public bool checkCase = true;

		private SceneSettings sceneSettings;

		
		public ActionSceneCheckAttribute ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Check attribute";
			description = "Queries the value of a scene attribute declared in the Scene Manager.";
		}


		override public void AssignParentList (ActionList actionList)
		{
			if (sceneSettings == null)
			{
				sceneSettings = KickStarter.sceneSettings;
			}
		}

		
		override public ActionEnd End (List<AC.Action> actions)
		{
			if (attributeID == -1)
			{
				return GenerateStopActionEnd ();
			}

			InvVar attribute = sceneSettings.GetAttribute (attributeID);
			if (attribute != null)
			{
				return ProcessResult (CheckCondition (attribute), actions);
			}

			ACDebug.LogWarning ("The 'Scene: Check attribute' Action halted the ActionList because it cannot find the scene attribute with an ID of " + attributeID);
			return GenerateStopActionEnd ();
		
		}
		
		
		private bool CheckCondition (InvVar attribute)
		{
			if (attribute == null)
			{
				ACDebug.LogWarning ("Cannot check state of attribute since it cannot be found!");
				return false;
			}

			if (attribute.type == VariableType.Boolean)
			{
				int fieldValue = attribute.val;
				int compareValue = (int) boolValue;

				if (boolCondition == BoolCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
			}

			else if (attribute.type == VariableType.Integer || attribute.type == VariableType.PopUp)
			{
				int fieldValue = attribute.val;
				int compareValue = intValue;

				if (intCondition == IntCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (fieldValue < compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan)
				{
					if (fieldValue > compareValue)
					{
						return true;
					}
				}
			}

			else if (attribute.type == VariableType.Float)
			{
				float fieldValue = attribute.floatVal;
				float compareValue = floatValue;

				if (intCondition == IntCondition.EqualTo)
				{
					if (Mathf.Approximately (fieldValue, compareValue))
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (!Mathf.Approximately (fieldValue, compareValue))
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (fieldValue < compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan)
				{
					if (fieldValue > compareValue)
					{
						return true;
					}
				}
			}

			else if (attribute.type == VariableType.String)
			{
				string fieldValue = attribute.textVal;
				string compareValue = AdvGame.ConvertTokens (stringValue);

				if (!checkCase)
				{
					fieldValue = fieldValue.ToLower ();
					compareValue = compareValue.ToLower ();
				}

				if (boolCondition == BoolCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
			}
			
			return false;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			if (AdvGame.GetReferences ().settingsManager)
			{
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

				attributeID = ShowVarGUI (settingsManager.sceneAttributes, attributeID, true);
			}
			else
			{
				EditorGUILayout.HelpBox ("A Settings Manager is required for this Action's GUI to display.", MessageType.Info);
			}
		}


		private int ShowAttributeSelectorGUI (List<InvVar> attributes, int ID)
		{
			attributeNumber = -1;
			
			List<string> labelList = new List<string>();
			foreach (GVar _var in attributes)
			{
				labelList.Add (_var.label);
			}
			
			attributeNumber = GetVarNumber (attributes, ID);
			
			if (attributeNumber == -1)
			{
				// Wasn't found (variable was deleted?), so revert to zero
				ACDebug.LogWarning ("Previously chosen attribute no longer exists!");
				attributeNumber = 0;
				ID = 0;
			}

			attributeNumber = EditorGUILayout.Popup ("Attribute:", attributeNumber, labelList.ToArray());
			ID = attributes[attributeNumber].id;

			return ID;
		}


		private int ShowVarGUI (List<InvVar> attributes, int ID, bool changeID)
		{
			if (attributes.Count > 0)
			{
				if (changeID)
				{
					ID = ShowAttributeSelectorGUI (attributes, ID);
				}

				attributeNumber = Mathf.Min (attributeNumber, attributes.Count-1);

				EditorGUILayout.BeginHorizontal ();

				if (attributes [attributeNumber].type == VariableType.Boolean)
				{
					boolCondition = (BoolCondition) EditorGUILayout.EnumPopup (boolCondition);
					EditorGUILayout.LabelField ("Boolean:", GUILayout.MaxWidth (60f));
					boolValue = (BoolValue) EditorGUILayout.EnumPopup (boolValue);
				}
				else if (attributes [attributeNumber].type == VariableType.Integer)
				{
					intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
					EditorGUILayout.LabelField ("Integer:", GUILayout.MaxWidth (60f));
					intValue = EditorGUILayout.IntField (intValue);
				}
				else if (attributes [attributeNumber].type == VariableType.PopUp)
				{
					intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
					EditorGUILayout.LabelField ("Value:", GUILayout.MaxWidth (60f));
					intValue = EditorGUILayout.Popup (intValue, attributes [attributeNumber].popUps);
				}
				else if (attributes [attributeNumber].type == VariableType.Float)
				{
					intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
					EditorGUILayout.LabelField ("Float:", GUILayout.MaxWidth (60f));
					floatValue = EditorGUILayout.FloatField (floatValue);
				}
				else if (attributes [attributeNumber].type == VariableType.String)
				{
					boolCondition = (BoolCondition) EditorGUILayout.EnumPopup (boolCondition);
					EditorGUILayout.LabelField ("String:", GUILayout.MaxWidth (60f));
					stringValue = EditorGUILayout.TextField (stringValue);
				}

				EditorGUILayout.EndHorizontal ();

				if (attributes [attributeNumber].type == VariableType.String)
				{
					checkCase = EditorGUILayout.Toggle ("Case-senstive?", checkCase);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				ID = -1;
				attributeNumber = -1;
			}

			return ID;
		}


		override public string SetLabel ()
		{
			if (sceneSettings)
			{
				return GetLabelString (sceneSettings.attributes);
			}

			return "";
		}


		private string GetLabelString (List<InvVar> attributes)
		{
			string labelAdd = "";

			if (attributes.Count > 0 && attributes.Count > attributeNumber && attributeNumber > -1)
			{
				labelAdd = " (" + attributes[attributeNumber].label;
				
				if (attributes [attributeNumber].type == VariableType.Boolean)
				{
					labelAdd += " " + boolCondition.ToString () + " " + boolValue.ToString ();
				}
				else if (attributes [attributeNumber].type == VariableType.Integer)
				{
					labelAdd += " " + intCondition.ToString () + " " + intValue.ToString ();
				}
				else if (attributes [attributeNumber].type == VariableType.Float)
				{
					labelAdd += " " + intCondition.ToString () + " " + floatValue.ToString ();
				}
				else if (attributes [attributeNumber].type == VariableType.String)
				{
					labelAdd += " " + boolCondition.ToString () + " " + stringValue;
				}
				else if (attributes [attributeNumber].type == VariableType.PopUp)
				{
					labelAdd += " " + intCondition.ToString () + " " + attributes[attributeNumber].popUps[intValue];
				}
				
				labelAdd += ")";
			}

			return labelAdd;
		}
		
		#endif


		private int GetVarNumber (List<InvVar> attributes, int ID)
		{
			int i = 0;
			foreach (InvVar attribute in attributes)
			{
				if (attribute.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}

	}

}