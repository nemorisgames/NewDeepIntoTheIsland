/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionCharHold.cs"
 * 
 *	This action parents a GameObject to a character's hand.
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
	public class ActionCharHold : Action
	{

		public int objectToHoldParameterID = -1;

		public int _charID = 0;
		public int objectToHoldID = 0;

		public GameObject objectToHold;
		public bool isPlayer;
		public Char _char;
		public bool rotate90;
		private GameObject loadedObject = null;
		
		public Hand hand;
		
		
		public ActionCharHold ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Hold object";
			description = "Parents a GameObject to a Character's hand transform, as chosen in the Character's inspector. The local transforms of the GameObject will be cleared. Note that this action only works with 3D characters.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			_char = AssignFile <Char> (_charID, _char);
			objectToHold = AssignFile (parameters, objectToHoldParameterID, objectToHoldID, objectToHold);

			if (objectToHold != null && !objectToHold.activeInHierarchy)
			{
				loadedObject = (GameObject) Instantiate (objectToHold);
			}

			if (isPlayer)
			{
				_char = KickStarter.player;
			}
		}


		private GameObject GetObjectToHold ()
		{
			if (loadedObject)
			{
				return loadedObject;
			}
			return objectToHold;
		}

		
		override public float Run ()
		{
			if (_char)
			{
				if (_char.GetAnimEngine () != null && _char.GetAnimEngine ().ActionCharHoldPossible ())
				{
					if (_char.HoldObject (GetObjectToHold (), hand))
					{
						if (rotate90)
						{
							GetObjectToHold ().transform.localEulerAngles = new Vector3 (0f, 0f, 90f);
						}
						else
						{
							GetObjectToHold ().transform.localEulerAngles = Vector3.zero;
						}
					}
				}
			}
			else
			{
				ACDebug.LogWarning ("Could not create animation engine!");
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
			if (isPlayer)
			{
				if (Application.isPlaying)
				{
					_char = KickStarter.player;
				}
				else if (AdvGame.GetReferences ().settingsManager)
				{
					_char = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
				}
				else
				{
					EditorGUILayout.HelpBox ("A Settings Manager and player must be defined", MessageType.Warning);
				}
			}
			else
			{
				_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
					
				_charID = FieldToID <Char> (_char, _charID);
				_char = IDToField <Char> (_char, _charID, true);
			}
			
			if (_char)
			{
				if (_char.GetAnimEngine () && _char.GetAnimEngine ().ActionCharHoldPossible ())
				{
					objectToHoldParameterID = Action.ChooseParameterGUI ("Object to hold:", parameters, objectToHoldParameterID, ParameterType.GameObject);
					if (objectToHoldParameterID >= 0)
					{
						objectToHoldID = 0;
						objectToHold = null;
					}
					else
					{
						objectToHold = (GameObject) EditorGUILayout.ObjectField ("Object to hold:", objectToHold, typeof (GameObject), true);
						
						objectToHoldID = FieldToID (objectToHold, objectToHoldID);
						objectToHold = IDToField (objectToHold, objectToHoldID, false);
					}
					
					hand = (Hand) EditorGUILayout.EnumPopup ("Hand:", hand);
					rotate90 = EditorGUILayout.Toggle ("Rotate 90 degrees?", rotate90);
				}
				else
				{
					EditorGUILayout.HelpBox ("This Action is not compatible with this Character's Animation Engine.", MessageType.Info);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
			}
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			if (saveScriptsToo)
			{
				if (!isPlayer && _char != null && _char.GetComponent <NPC>())
				{
					AddSaveScript <RememberNPC> (_char);
				}

				AddSaveScript <RememberTransform> (objectToHold);
				if (objectToHold != null && objectToHold.GetComponent <RememberTransform>())
				{
					objectToHold.GetComponent <RememberTransform>().saveParent = true;
					if (objectToHold.transform.parent)
					{
						AddSaveScript <ConstantID> (objectToHold.transform.parent.gameObject);
					}
				}
			}

			if (!isPlayer)
			{
				AssignConstantID <Char> (_char, _charID, 0);
			}
			AssignConstantID (objectToHold, objectToHoldID, objectToHoldParameterID);
		}

		
		public override string SetLabel ()
		{
			string labelAdd = "";
			
			if (_char && objectToHold)
			{
				labelAdd = "(" + _char.name + " hold " + objectToHold.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
		
	}

}