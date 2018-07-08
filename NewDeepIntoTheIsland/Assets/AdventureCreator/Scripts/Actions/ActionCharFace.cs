/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionCharFace.cs"
 * 
 *	This action is used to make characters turn to face GameObjects.
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
	public class ActionCharFace : Action
	{

		public int charToMoveParameterID = -1;
		public int faceObjectParameterID = -1;

		public int charToMoveID = 0;
		public int faceObjectID = 0;

		public bool isInstant;
		public Char charToMove;
		public GameObject faceObject;
		public bool copyRotation;
		public bool facePlayer;
		
		public CharFaceType faceType = CharFaceType.Body;
		public bool isPlayer;
		public bool lookUpDown;
		public bool stopLooking = false;

		public bool lookAtHead = false;


		public ActionCharFace ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Face object";
			description = "Makes a Character turn, either instantly or over time. Can turn to face another object, or copy that object's facing direction.";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			charToMove = AssignFile <Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);
			faceObject = AssignFile (parameters, faceObjectParameterID, faceObjectID, faceObject);

			if (isPlayer)
			{
				charToMove = KickStarter.player;
			}
			else if (facePlayer && KickStarter.player)
			{
				faceObject = KickStarter.player.gameObject;
			}
		}

		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
			
				if (faceObject == null && (faceType == CharFaceType.Body || (faceType == CharFaceType.Head && !stopLooking)))
				{
					return 0f;
				}

				if (charToMove)
				{
					if (faceType == CharFaceType.Body)
					{
						if (!isInstant && charToMove.IsMovingAlongPath ())
						{
							charToMove.EndPath ();
						}

						if (lookUpDown && isPlayer && KickStarter.settingsManager.IsInFirstPerson ())
						{
							Player player = (Player) charToMove;
							player.SetTilt (faceObject.transform.position, isInstant);
						}

						charToMove.SetLookDirection (GetLookVector (KickStarter.settingsManager), isInstant);
					}
					else if (faceType == CharFaceType.Head)
					{
						if (stopLooking)
						{
							charToMove.ClearHeadTurnTarget (isInstant, HeadFacing.Manual);
						}
						else
						{
							Vector3 offset = Vector3.zero;

							Hotspot faceObjectHotspot = faceObject.GetComponent <Hotspot>();
							Char faceObjectChar = faceObject.GetComponent <Char>();

							if (faceObjectHotspot != null)
							{
								if (faceObjectHotspot.centrePoint != null)
								{
									faceObject = faceObjectHotspot.centrePoint.gameObject;
								}
								else
								{
									offset = faceObjectHotspot.GetIconPosition (true);
								}
							}
							else if (lookAtHead && faceObjectChar != null)
							{
								Transform neckBone = faceObjectChar.neckBone;
								if (neckBone != null)
								{
									faceObject = neckBone.gameObject;
								}
								else
								{
									ACDebug.Log ("Cannot look at " + faceObject.name + "'s head as their 'Neck bone' has not been defined.");
								}
							}

							charToMove.SetHeadTurnTarget (faceObject.transform, offset, isInstant);
						}
					}

					if (isInstant)
					{
						return 0f;
					}
					else
					{
						if (willWait)
						{
							return (defaultPauseTime);
						}
						else
						{
							return 0f;
						}
					}
				}

				return 0f;
			}
			else
			{
				if (faceType == CharFaceType.Head && charToMove.IsMovingHead ())
				{
					return defaultPauseTime;
				}
				else if (faceType == CharFaceType.Body && charToMove.IsTurning ())
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
					return 0f;
				}
			}
		}


		override public void Skip ()
		{
			if (faceObject == null && (faceType == CharFaceType.Body || (faceType == CharFaceType.Head && !stopLooking)))
			{
				return;
			}
			
			if (charToMove)
			{
				if (faceType == CharFaceType.Body)
				{
					charToMove.SetLookDirection (GetLookVector (KickStarter.settingsManager), true);
					
					if (lookUpDown && isPlayer && KickStarter.settingsManager.IsInFirstPerson ())
					{
						Player player = (Player) charToMove;
						player.SetTilt (faceObject.transform.position, true);
					}
				}

				else if (faceType == CharFaceType.Head)
				{
					if (stopLooking)
					{
						charToMove.ClearHeadTurnTarget (true, HeadFacing.Manual);
					}
					else
					{
						Vector3 offset = Vector3.zero;
						if (faceObject.GetComponent <Hotspot>())
						{
							offset = faceObject.GetComponent <Hotspot>().GetIconPosition (true);
						}
						else if (lookAtHead && faceObject.GetComponent <Char>())
						{
							Transform neckBone = faceObject.GetComponent <Char>().neckBone;
							if (neckBone != null)
							{
								faceObject = neckBone.gameObject;
							}
							else
							{
								ACDebug.Log ("Cannot look at " + faceObject.name + "'s head as their 'Neck bone' has not been defined.");
							}
						}

						charToMove.SetHeadTurnTarget (faceObject.transform, offset, true);
					}
				}
			}
		}

		
		private Vector3 GetLookVector (SettingsManager settingsManager)
		{
			Vector3 lookVector = faceObject.transform.position - charToMove.transform.position;
			if (copyRotation)
			{
				lookVector = faceObject.transform.forward;
			}
			else if (SceneSettings.ActInScreenSpace ())
			{
				lookVector = AdvGame.GetScreenDirection (charToMove.transform.position, faceObject.transform.position);
			}

			return lookVector;
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Affect Player?", isPlayer);
			if (!isPlayer)
			{
				charToMoveParameterID = Action.ChooseParameterGUI ("Character to turn:", parameters, charToMoveParameterID, ParameterType.GameObject);
				if (charToMoveParameterID >= 0)
				{
					charToMoveID = 0;
					charToMove = null;
				}
				else
				{
					charToMove = (Char) EditorGUILayout.ObjectField ("Character to turn:", charToMove, typeof(Char), true);
					
					charToMoveID = FieldToID <Char> (charToMove, charToMoveID);
					charToMove = IDToField <Char> (charToMove, charToMoveID, false);
				}

				facePlayer = EditorGUILayout.Toggle ("Face player?", facePlayer);
			}
			else
			{
				facePlayer = false;
				
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
				if (faceType == CharFaceType.Body && settingsManager && settingsManager.IsInFirstPerson ())
				{
					lookUpDown = EditorGUILayout.Toggle ("FPS head tilt?", lookUpDown);
				}
			}

			faceType = (CharFaceType) EditorGUILayout.EnumPopup ("Face with:", faceType);
			if (faceType == CharFaceType.Head)
			{
				stopLooking = EditorGUILayout.Toggle ("Stop looking?", stopLooking);
			}

			if (facePlayer || (faceType == CharFaceType.Head && stopLooking))
			{ }
			else
			{
				faceObjectParameterID = Action.ChooseParameterGUI ("Object to face:", parameters, faceObjectParameterID, ParameterType.GameObject);
				if (faceObjectParameterID >= 0)
				{
					faceObjectID = 0;
					faceObject = null;
				}
				else
				{
					faceObject = (GameObject) EditorGUILayout.ObjectField ("Object to face:", faceObject, typeof(GameObject), true);
					
					faceObjectID = FieldToID (faceObject, faceObjectID);
					faceObject = IDToField (faceObject, faceObjectID, false);
				}
			}

			if (faceType == CharFaceType.Body)
			{
				copyRotation = EditorGUILayout.Toggle ("Use object's rotation?", copyRotation);
			}
			else if (faceType == CharFaceType.Head && !stopLooking)
			{
				if (facePlayer || (faceObject != null && faceObject.GetComponent <Char>()))
				{
					lookAtHead = EditorGUILayout.Toggle ("Look at character's head?", lookAtHead);
				}
			}

			isInstant = EditorGUILayout.Toggle ("Is instant?", isInstant);
			if (!isInstant)
			{
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}

			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			if (saveScriptsToo)
			{
				if (!isPlayer && charToMove != null && charToMove.GetComponent <NPC>())
				{
					AddSaveScript <RememberNPC> (charToMove);
				}
				if (faceType == CharFaceType.Head && faceObject != null)
				{
					AddSaveScript <ConstantID> (faceObject);
				}
			}

			if (!isPlayer)
			{
				AssignConstantID <Char> (charToMove, charToMoveID, charToMoveParameterID);
			}
			AssignConstantID (faceObject, faceObjectID, faceObjectParameterID);
		}

		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (charToMove && faceObject)
			{
				labelAdd = " (" + charToMove.name + " to " + faceObject.name + ")";
			}
			else if (isPlayer && faceObject)
			{
				labelAdd = " (Player to " + faceObject.name + ")";
			}
			
			return labelAdd;
		}

		#endif
		
	}

}