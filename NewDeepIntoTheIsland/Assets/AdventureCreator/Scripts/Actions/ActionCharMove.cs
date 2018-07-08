/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionCharMove.cs"
 * 
 *	This action moves characters by assinging them a Paths object.
 *	If a player is moved, the game will automatically pause.
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
	public class ActionCharMove : Action
	{

		public enum MovePathMethod { MoveOnNewPath, StopMoving, ResumeLastSetPath };
		public MovePathMethod movePathMethod = MovePathMethod.MoveOnNewPath;

		public int charToMoveParameterID = -1;
		public int movePathParameterID = -1;

		public int charToMoveID = 0;
		public int movePathID = 0;

		public bool stopInstantly;
		public Paths movePath;
		public bool isPlayer;
		public Char charToMove;
		public bool doTeleport;
		public bool doStop;
		public bool startRandom = false;

		private Char runtimeChar;

		
		public ActionCharMove ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Move along path";
			description = "Moves the Character along a pre-determined path. Will adhere to the speed setting selected in the relevant Paths object. Can also be used to stop a character from moving, or resume moving along a path if it was previously stopped.";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile <Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);
			movePath = AssignFile <Paths> (parameters, movePathParameterID, movePathID, movePath);

			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}
		}


		private void UpgradeSelf ()
		{
			if (!doStop)
			{
				return;
			}

			doStop = false;
			movePathMethod = MovePathMethod.StopMoving;
			
			if (Application.isPlaying)
			{
				ACDebug.Log ("'Character: Move along path' Action has been temporarily upgraded - - please view its Inspector when the game ends and save the scene.");
			}
			else
			{
				ACDebug.Log ("Upgraded 'Character: Move along path' Action, please save the scene.");
			}
		}


		override public float Run ()
		{
			UpgradeSelf ();

			if (movePath && movePath.GetComponent <Char>())
			{
				ACDebug.LogWarning ("Can't follow a Path attached to a Character!");
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				if (runtimeChar)
				{
					if (runtimeChar is NPC)
					{
						NPC npcToMove = (NPC) runtimeChar;
						npcToMove.StopFollowing ();
					}

					if (movePathMethod == MovePathMethod.StopMoving)
					{
						runtimeChar.EndPath ();
						if (runtimeChar.IsPlayer && KickStarter.playerInteraction.GetHotspotMovingTo () != null)
						{
							KickStarter.playerInteraction.StopMovingToHotspot ();
						}

						if (stopInstantly)
						{
							runtimeChar.Halt ();
						}
					}
					else if (movePathMethod == MovePathMethod.MoveOnNewPath)
					{
						if (movePath)
						{
							int randomIndex = -1;
							if (movePath.pathType == AC_PathType.IsRandom && startRandom)
							{
								if (movePath.nodes.Count > 1)
								{
									randomIndex = Random.Range (0, movePath.nodes.Count);
								}
							}

							PrepareCharacter (randomIndex);

							if (willWait && movePath.pathType != AC_PathType.ForwardOnly)
							{
								willWait = false;
								ACDebug.LogWarning ("Cannot pause while character moves along a non-forward only path, as this will create an indefinite cutscene.");
							}

							if (randomIndex >= 0)
							{
								runtimeChar.SetPath (movePath, randomIndex, 0);
							}
							else
							{
								runtimeChar.SetPath (movePath);
							}
						
							if (willWait)
							{
								return defaultPauseTime;
							}
						}
					}
					else if (movePathMethod == MovePathMethod.ResumeLastSetPath)
					{
						runtimeChar.ResumeLastPath ();
					}
				}

				return 0f;
			}
			else
			{
				if (runtimeChar.GetPath () != movePath)
				{
					isRunning = false;
					return 0f;
				}
				else
				{
					return (defaultPauseTime);
				}
			}
		}


		override public void Skip ()
		{
			if (runtimeChar)
			{
				runtimeChar.EndPath (movePath);

				if (runtimeChar is NPC)
				{
					NPC npcToMove = (NPC) runtimeChar;
					npcToMove.StopFollowing ();
				}
				
				if (doStop)
				{
					runtimeChar.EndPath ();
				}
				else if (movePath)
				{
					int randomIndex = -1;

					if (movePath.pathType == AC_PathType.ForwardOnly)
					{
						// Place at end
						int i = movePath.nodes.Count-1;
						runtimeChar.transform.position = movePath.nodes [i];
						if (i>0)
						{
							runtimeChar.SetLookDirection (movePath.nodes[i] - movePath.nodes[i-1], true);
						}
						return;
					}
					else if (movePath.pathType == AC_PathType.IsRandom && startRandom)
					{
						if (movePath.nodes.Count > 1)
						{
							randomIndex = Random.Range (0, movePath.nodes.Count);
						}
					}

					PrepareCharacter (randomIndex);

					if (!isPlayer)
					{
						if (randomIndex >= 0)
						{
							runtimeChar.SetPath (movePath, randomIndex, 0);
						}
						else
						{
							runtimeChar.SetPath (movePath);
						}
					}
				}
			}
		}


		private void PrepareCharacter (int randomIndex)
		{
			if (doTeleport)
			{
				if (randomIndex >= 0)
				{
					runtimeChar.transform.position = movePath.nodes[randomIndex];
				}
				else
				{
					runtimeChar.Teleport (movePath.transform.position);
					
					// Set rotation if there is more than one node
					if (movePath.nodes.Count > 1)
					{
						runtimeChar.SetLookDirection (movePath.nodes[1] - movePath.nodes[0], true);
					}
				}
			}
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			UpgradeSelf ();

			isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);

			if (!isPlayer)
			{
				charToMoveParameterID = Action.ChooseParameterGUI ("Character to move:", parameters, charToMoveParameterID, ParameterType.GameObject);
				if (charToMoveParameterID >= 0)
				{
					charToMoveID = 0;
					charToMove = null;
				}
				else
				{
					charToMove = (Char) EditorGUILayout.ObjectField ("Character to move:", charToMove, typeof (Char), true);
					
					charToMoveID = FieldToID <Char> (charToMove, charToMoveID);
					charToMove = IDToField <Char> (charToMove, charToMoveID, false);
				}
			}

			movePathMethod = (MovePathMethod) EditorGUILayout.EnumPopup ("Method:", movePathMethod);
			if (movePathMethod == MovePathMethod.MoveOnNewPath)
			{
				movePathParameterID = Action.ChooseParameterGUI ("Path to follow:", parameters, movePathParameterID, ParameterType.GameObject);
				if (movePathParameterID >= 0)
				{
					movePathID = 0;
					movePath = null;
				}
				else
				{
					movePath = (Paths) EditorGUILayout.ObjectField ("Path to follow:", movePath, typeof(Paths), true);
					
					movePathID = FieldToID <Paths> (movePath, movePathID);
					movePath = IDToField <Paths> (movePath, movePathID, false);
				}

				if (movePath != null && movePath.pathType == AC_PathType.IsRandom)
				{
					startRandom = EditorGUILayout.Toggle ("Start at random node?", startRandom);
				}

				doTeleport = EditorGUILayout.Toggle ("Teleport to start?", doTeleport);
				if (movePath != null && movePath.pathType != AC_PathType.ForwardOnly)
				{
					willWait = false;
				}
				else
				{
					willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
				}

				if (movePath != null && movePath.GetComponent <Char>())
				{
					EditorGUILayout.HelpBox ("Can't follow a Path attached to a Character!", MessageType.Warning);
				}
			}
			else if (movePathMethod == MovePathMethod.StopMoving)
			{
				stopInstantly = EditorGUILayout.Toggle ("Stop instantly?", stopInstantly);
			}
			else if (movePathMethod == MovePathMethod.ResumeLastSetPath)
			{
				//
			}
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <ConstantID> (movePath);
				if (!isPlayer && charToMove != null && charToMove.GetComponent <NPC>())
				{
					AddSaveScript <RememberNPC> (charToMove);
				}
			}

			if (!isPlayer)
			{
				AssignConstantID <Char> (charToMove, charToMoveID, charToMoveParameterID);
			}
			AssignConstantID <Paths> (movePath, movePathID, movePathParameterID);
		}

		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (charToMove && movePath)
			{
				labelAdd = " (" + charToMove.name + " to " + movePath.name + ")";
			}
			else if (isPlayer && movePath)
			{
				labelAdd = " (Player to " + movePath.name + ")";
			}
			
			return labelAdd;
		}

		#endif
		
	}

}