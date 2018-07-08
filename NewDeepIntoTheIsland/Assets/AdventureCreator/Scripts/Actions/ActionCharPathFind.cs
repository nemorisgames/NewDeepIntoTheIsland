/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionCharPathFind.cs"
 * 
 *	This action moves characters by generating a path to a specified point.
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
	public class ActionCharPathFind : Action
	{

		public int charToMoveParameterID = -1;
		public int markerParameterID = -1;

		public int charToMoveID = 0;
		public int markerID = 0;
		
		public Marker marker;
		public bool isPlayer;
		public Char charToMove;
		public PathSpeed speed;
		public bool pathFind = true;
		public bool doFloat = false;

		public bool doTimeLimit;
		public int maxTimeParameterID = -1;
		public float maxTime = 10f;
		[SerializeField] private OnReachTimeLimit onReachTimeLimit = OnReachTimeLimit.TeleportToDestination;
		private enum OnReachTimeLimit { TeleportToDestination, StopMoving };
		private float currentTimer;
		private Char runtimeChar;

		
		public ActionCharPathFind ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Character;
			title = "Move to point";
			description = "Moves a character to a given Marker object. By default, the character will attempt to pathfind their way to the marker, but can optionally just move in a straight line.";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeChar = AssignFile <Char> (parameters, charToMoveParameterID, charToMoveID, charToMove);

			Hotspot markerHotspot = AssignFile <Hotspot> (parameters, markerParameterID, markerID, null);
			if (markerHotspot != null && markerHotspot.walkToMarker != null)
			{
				marker = markerHotspot.walkToMarker;
			}
			else
			{
				marker = AssignFile <Marker> (parameters, markerParameterID, markerID, marker);
			}

			if (isPlayer)
			{
				runtimeChar = KickStarter.player;
			}

			maxTime = AssignFloat (parameters, maxTimeParameterID, maxTime);
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;

				if (runtimeChar && marker)
				{
					Paths path = runtimeChar.GetComponent <Paths>();
					if (path == null)
					{
						ACDebug.LogWarning ("Cannot move a character with no Paths component");
					}
					else
					{
						if (runtimeChar is NPC)
						{
							NPC npcToMove = (NPC) runtimeChar;
							npcToMove.StopFollowing ();
						}

						path.pathType = AC_PathType.ForwardOnly;
						path.pathSpeed = speed;
						path.affectY = true;

						Vector3[] pointArray;
						Vector3 targetPosition = marker.transform.position;

						if (SceneSettings.ActInScreenSpace ())
						{
							targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
						}

						float distance = Vector3.Distance (targetPosition, runtimeChar.transform.position);
						if (distance <= KickStarter.settingsManager.GetDestinationThreshold ())
						{
							isRunning = false;
							return 0f;
						}

						if (pathFind && KickStarter.navigationManager)
						{
							pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (runtimeChar.transform.position, targetPosition, runtimeChar);
						}
						else
						{
							List<Vector3> pointList = new List<Vector3>();
							pointList.Add (targetPosition);
							pointArray = pointList.ToArray ();
						}

						if (speed == PathSpeed.Walk)
						{
							runtimeChar.MoveAlongPoints (pointArray, false, pathFind);
						}
						else
						{
							runtimeChar.MoveAlongPoints (pointArray, true, pathFind);
						}

						if (runtimeChar.GetPath ())
						{
							if (!pathFind && doFloat)
							{
								runtimeChar.GetPath ().affectY = true;
							}
							else
							{
								runtimeChar.GetPath ().affectY = false;
							}
						}

						if (willWait)
						{
							currentTimer = maxTime;
							return defaultPauseTime;
						}
					}
				}

				return 0f;
			}
			else
			{
				if (runtimeChar.GetPath () == null)
				{
					isRunning = false;
					return 0f;
				}
				else
				{
					if (doTimeLimit)
					{
						currentTimer -= Time.deltaTime;
						if (currentTimer <= 0)
						{
							switch (onReachTimeLimit)
							{
								case OnReachTimeLimit.StopMoving:
									runtimeChar.EndPath ();
									break;

								case OnReachTimeLimit.TeleportToDestination:
									Skip ();
									break;
							}

							isRunning = false;
							return 0f;
						}
					}

					return (defaultPauseTime);
				}
			}
		}


		override public void Skip ()
		{
			if (runtimeChar && marker)
			{
				runtimeChar.EndPath ();

				if (runtimeChar is NPC)
				{
					NPC npcToMove = (NPC) runtimeChar;
					npcToMove.StopFollowing ();
				}
				
				Vector3[] pointArray;
				Vector3 targetPosition = marker.transform.position;
				
				if (SceneSettings.ActInScreenSpace ())
				{
					targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
				}
				
				if (pathFind && KickStarter.navigationManager)
				{
					pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (runtimeChar.transform.position, targetPosition);
					KickStarter.navigationManager.navigationEngine.ResetHoles (KickStarter.sceneSettings.navMesh);
				}
				else
				{
					List<Vector3> pointList = new List<Vector3>();
					pointList.Add (targetPosition);
					pointArray = pointList.ToArray ();
				}
				
				int i = pointArray.Length-1;

				if (i>0)
				{
					runtimeChar.SetLookDirection (pointArray[i] - pointArray[i-1], true);
				}
				else
				{
					runtimeChar.SetLookDirection (pointArray[i] - runtimeChar.transform.position, true);
				}

				runtimeChar.Teleport (pointArray [i]);
			}
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
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

			markerParameterID = Action.ChooseParameterGUI ("Marker to reach:", parameters, markerParameterID, ParameterType.GameObject);
			if (markerParameterID >= 0)
			{
				markerID = 0;
				marker = null;

				EditorGUILayout.HelpBox ("If a Hotspot is passed to this parameter, that Hotspot's 'Walk-to Marker' will be referred to.", MessageType.Info);
			}
			else
			{
				marker = (Marker) EditorGUILayout.ObjectField ("Marker to reach:", marker, typeof (Marker), true);
				
				markerID = FieldToID <Marker> (marker, markerID);
				marker = IDToField <Marker> (marker, markerID, false);
			}

			speed = (PathSpeed) EditorGUILayout.EnumPopup ("Move speed:" , speed);
			pathFind = EditorGUILayout.Toggle ("Pathfind?", pathFind);
			if (!pathFind)
			{
				doFloat = EditorGUILayout.Toggle ("Ignore gravity?", doFloat);
			}
			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);

			if (willWait)
			{
				EditorGUILayout.Space ();
				doTimeLimit = EditorGUILayout.Toggle ("Enforce time limit?", doTimeLimit);
				if (doTimeLimit)
				{
					maxTimeParameterID = Action.ChooseParameterGUI ("Time limit (s):", parameters, maxTimeParameterID, ParameterType.Float);
					if (maxTimeParameterID < 0)
					{
						maxTime = EditorGUILayout.FloatField ("Time limit (s):", maxTime);
					}
					onReachTimeLimit = (OnReachTimeLimit) EditorGUILayout.EnumPopup ("On reach time limit:", onReachTimeLimit);
				}
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
			}

			if (!isPlayer)
			{
				AssignConstantID <Char> (charToMove, charToMoveID, charToMoveParameterID);
			}
			AssignConstantID <Marker> (marker, markerID, markerParameterID);
		}

		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (marker)
			{
				if (charToMove)
				{
					labelAdd = " (" + charToMove.name + " to " + marker.name + ")";
				}
				else if (isPlayer)
				{
					labelAdd = " (Player to " + marker.name + ")";
				}
			}
			
			return labelAdd;
		}

		#endif
		
	}

}