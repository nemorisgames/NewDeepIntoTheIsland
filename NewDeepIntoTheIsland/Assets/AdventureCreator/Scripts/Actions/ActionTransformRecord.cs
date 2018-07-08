/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionTransformRecord.cs"
 * 
 *	This action records an object's position, rotation, or scale - and stores it in a Vector3 variable.
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
	public class ActionTransformRecord : Action
	{

		public GameObject obToRead;
		public int obToReadParameterID = -1;
		public int obToReadConstantID = 0;

		public TransformRecordType transformRecordType = TransformRecordType.Position;
		public enum TransformRecordType { Position, Rotation, Scale };
		public VariableLocation transformLocation;

		public VariableLocation variableLocation;
		public int variableID;


		public ActionTransformRecord ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Record transform";
			description = "Records the transform values of a GameObject.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			obToRead = AssignFile (parameters, obToReadParameterID, obToReadConstantID, obToRead);
		}


		override public float Run ()	
		{
			if (obToRead != null)
			{
				GVar variable = null;
				if (variableLocation == VariableLocation.Global)
				{
					variable = GlobalVariables.GetVariable (variableID);
				}
				else if (variableLocation == VariableLocation.Local && !isAssetFile)
				{
					variable = LocalVariables.GetVariable (variableID);
				}

				if (variable != null)
				{
					switch (transformRecordType)
					{
						case TransformRecordType.Position:
						if (transformLocation == VariableLocation.Global)
						{
							variable.SetVector3Value (obToRead.transform.position);
						}
						else if (transformLocation == VariableLocation.Local)
						{
							variable.SetVector3Value (obToRead.transform.localPosition);
						}
						break;

						case TransformRecordType.Rotation:
						if (transformLocation == VariableLocation.Global)
						{
							variable.SetVector3Value (obToRead.transform.eulerAngles);
						}
						else if (transformLocation == VariableLocation.Local)
						{
							variable.SetVector3Value (obToRead.transform.localEulerAngles);
						}
						break;

						case TransformRecordType.Scale:
						if (transformLocation == VariableLocation.Global)
						{
							variable.SetVector3Value (obToRead.transform.lossyScale);
						}
						else if (transformLocation == VariableLocation.Local)
						{
							variable.SetVector3Value (obToRead.transform.localScale);
						}
						break;
					}
				}
			}

			return 0f;
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			obToReadParameterID = Action.ChooseParameterGUI ("Object to record:", parameters, obToReadParameterID, ParameterType.GameObject);
			if (obToReadParameterID >= 0)
			{
				obToReadConstantID = 0;
				obToRead = null;
			}
			else
			{
				obToRead = (GameObject) EditorGUILayout.ObjectField ("Object to record:", obToRead, typeof (GameObject), true);

				obToReadConstantID = FieldToID (obToRead, obToReadConstantID);
				obToRead = IDToField (obToRead, obToReadConstantID, false);
			}

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Record:", GUILayout.MaxWidth (100f));
			transformLocation = (VariableLocation) EditorGUILayout.EnumPopup (transformLocation);
			transformRecordType = (TransformRecordType) EditorGUILayout.EnumPopup (transformRecordType);
			EditorGUILayout.EndHorizontal ();

			if (isAssetFile)
			{
				variableLocation = VariableLocation.Global;
			}
			else
			{
				variableLocation = (VariableLocation) EditorGUILayout.EnumPopup ("Variable location:", variableLocation);
			}

			if (variableLocation == VariableLocation.Global)
			{
				variableID = AdvGame.GlobalVariableGUI ("Record to variable:", variableID, VariableType.Vector3);
			}
			else if (variableLocation == VariableLocation.Local)
			{
				variableID = AdvGame.LocalVariableGUI ("Record to variable:", variableID, VariableType.Vector3);
			}

			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo = false)
		{
			AssignConstantID (obToRead, obToReadConstantID, obToReadParameterID);
		}


		override public string SetLabel ()
		{
			string labelAdd = "";
			if (obToRead)
			{
				labelAdd = " (" + obToRead.name + " " + transformRecordType.ToString () + ")";
			}
			
			return labelAdd;
		}

		#endif

	}

}
