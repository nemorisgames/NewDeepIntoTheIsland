/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"NPC.cs"
 * 
 *	This is attached to all non-Player characters.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Attaching this to a GameObject will make it an NPC, or Non-Player Character.
	 */
	[AddComponentMenu("Adventure Creator/Characters/NPC")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_n_p_c.html")]
	#endif
	public class NPC : Char
	{

		/** If True, the NPC will attempt to keep out of the Player's way */
		public bool moveOutOfPlayersWay = false;
		/** The minimum distance to keep from the Player, if moveOutOfPlayersWay = True */
		public float minPlayerDistance = 1f;
		
		private Char followTarget = null;
		private bool followTargetIsPlayer = false;
		private float followFrequency = 0f;
		private float followDistance = 0f;
		private float followDistanceMax = 0f;
		private bool followFaceWhenIdle = false;
		private bool followRandomDirection = false;

		private LayerMask LayerOn;
		private LayerMask LayerOff;
		
		
		private void Awake ()
		{
			if (KickStarter.settingsManager != null)
			{
				LayerOn = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
				LayerOff = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
			}

			_Awake ();
		}


		/**
		 * The NPC's "Update" function, called by StateHandler.
		 */
		public override void _Update ()
		{
			if (moveOutOfPlayersWay && charState == CharState.Idle)
			{
				if (activePath && !pausePath)
				{
					// Don't evade player if waiting between path nodes
				}
				else
				{
					StayAwayFromPlayer ();
				}
			}

			if (activePath && followTarget)
			{
				FollowCheckDistance ();
				FollowCheckDistanceMax ();
			}

			if (activePath && !pausePath)
			{
				if (IsTurningBeforeWalking ())
				{
					if (charState == CharState.Move)
					{
						charState = CharState.Decelerate;
					}
					else if (charState == CharState.Custom)
					{
						charState = CharState.Idle;
					}
				}
				else 
				{
					charState = CharState.Move;
				}
			}

			base._Update ();
		}


		private void StayAwayFromPlayer ()
		{
			if (followTarget == null && KickStarter.player != null && Vector3.Distance (transform.position, KickStarter.player.transform.position) < minPlayerDistance)
			{
				// Move out the way
				Vector3[] pointArray = TryNavPoint (transform.position - KickStarter.player.transform.position);
				int i=0;

				if (pointArray == null)
				{
					// Right
					pointArray = TryNavPoint (Vector3.Cross (transform.up, transform.position - KickStarter.player.transform.position).normalized);
					i++;
				}

				if (pointArray == null)
				{
					// Left
					pointArray = TryNavPoint (Vector3.Cross (-transform.up, transform.position - KickStarter.player.transform.position).normalized);
					i++;
				}

				if (pointArray == null)
				{
					// Towards
					pointArray = TryNavPoint (KickStarter.player.transform.position - transform.position);
					i++;
				}

				if (pointArray != null)
				{
					if (i == 0)
					{
						MoveAlongPoints (pointArray, false);
					}
					else
					{
						MoveToPoint (pointArray [pointArray.Length - 1], false);
					}
				}
			}
		}


		private Vector3[] TryNavPoint (Vector3 _direction)
		{
			Vector3 _targetPosition = transform.position + _direction.normalized * minPlayerDistance * 1.2f;

			if (SceneSettings.ActInScreenSpace ())
			{
				_targetPosition = AdvGame.GetScreenNavMesh (_targetPosition);
			}
			else if (SceneSettings.CameraPerspective == CameraPerspective.ThreeD)
			{
				_targetPosition.y = transform.position.y;
			}
			
			Vector3[] pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (transform.position, _targetPosition, this);

			if (pointArray.Length == 0 || Vector3.Distance (pointArray [pointArray.Length-1], transform.position) < minPlayerDistance * 0.6f)
			{
				// Not far away enough
				return null;
			}
			return pointArray;
		}


		/**
		 * Stops the NPC from following the Player or another NPC.
		 */
		public void StopFollowing ()
		{
			FollowStop ();

			followTarget = null;
			followTargetIsPlayer = false;
			followFrequency = 0f;
			followDistance = 0f;
		}


		private void FollowUpdate ()
		{
			if (followTarget)
			{
				FollowMove ();
				Invoke ("FollowUpdate", followFrequency);
			}
		}


		private void FollowMove ()
		{
			float dist = FollowCheckDistance ();
			if (dist > followDistance)
			{
				Paths path = GetComponent <Paths>();
				if (path == null)
				{
					ACDebug.LogWarning ("Cannot move a character with no Paths component");
				}
				else
				{
					path.pathType = AC_PathType.ForwardOnly;
					path.affectY = true;
					
					Vector3[] pointArray;
					Vector3 targetPosition = followTarget.transform.position;
					
					if (SceneSettings.ActInScreenSpace ())
					{
						targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
					}
					
					if (KickStarter.navigationManager)
					{
						if (followRandomDirection)
						{
							targetPosition = KickStarter.navigationManager.navigationEngine.GetPointNear (targetPosition, followDistance, followDistanceMax);
						}
						pointArray = KickStarter.navigationManager.navigationEngine.GetPointsArray (transform.position, targetPosition, this);
					}
					else
					{
						List<Vector3> pointList = new List<Vector3>();
						pointList.Add (targetPosition);
						pointArray = pointList.ToArray ();
					}

					if (dist > followDistanceMax)
					{
						MoveAlongPoints (pointArray, true);
					}
					else
					{
						MoveAlongPoints (pointArray, false);
					}
				}
			}
		}


		private float FollowCheckDistance ()
		{
			float dist = Vector3.Distance (followTarget.transform.position, transform.position);

			if (dist < followDistance)
			{
				if (!followRandomDirection)
				{
					EndPath ();
				}

				if (activePath == null && followFaceWhenIdle)
				{
					Vector3 _lookDirection = followTarget.transform.position - transform.position;
					SetLookDirection (_lookDirection, false);
				}
			}

			return (dist);
		}


		private void FollowCheckDistanceMax ()
		{
			if (followTarget)
			{
				if (FollowCheckDistance () > followDistanceMax)
				{
					if (!isRunning)
					{
						FollowMove ();
					}
				}
				else if (isRunning)
				{
					FollowMove ();
				}
			}
		}


		private void FollowStop ()
		{
			StopCoroutine ("FollowUpdate");

			if (followTarget != null)
			{
				EndPath ();
			}
		}


		/**
		 * <summary>Assigns a new target (NPC or Player) to start following.</summary>
		 * <param name = "_followTarget">The target to follow</param>
		 * <param name = "_followTargetIsPlayer">If True, the NPC will follow the current Player, and _followTarget will be ignored</param>
		 * <param name = "_followFrequency">The frequency with which to follow the target</param>
		 * <param name = "_followDistance">The minimum distance to keep from the target</param>
		 * <param name = "_followDistanceMax">The maximum distance to keep from the target</param>
		 * <param name = "_faceWhenIdle">If True, the NPC will face the target when idle</param>
		 */
		public void FollowAssign (Char _followTarget, bool _followTargetIsPlayer, float _followFrequency, float _followDistance, float _followDistanceMax, bool _faceWhenIdle, bool _followRandomDirection)
		{
			if (_followTargetIsPlayer)
			{
				_followTarget = KickStarter.player;
			}

			if (_followTarget == null || _followFrequency <= 0f || _followFrequency < 0f || _followDistance <= 0f || _followDistanceMax <= 0f)
			{
				StopFollowing ();
				return;
			}

			followTarget = _followTarget;
			followTargetIsPlayer = _followTargetIsPlayer;
			followFrequency = _followFrequency;
			followDistance = _followDistance;
			followDistanceMax = _followDistanceMax;
			followFaceWhenIdle = _faceWhenIdle;
			followRandomDirection = _followRandomDirection;

			FollowUpdate ();
		}
		
		
		private void TurnOn ()
		{
			gameObject.layer = LayerOn;
		}
		

		private void TurnOff ()
		{
			gameObject.layer = LayerOff;
		}


		/**
		 * <summary>Updates a NPCData class with its own variables that need saving.</summary>
		 * <param name = "npcData">The original NPCData class</param>
		 * <returns>The updated NPCData class</returns>
		 */
		public NPCData SaveData (NPCData npcData)
		{
			npcData.RotX = TransformRotation.eulerAngles.x;
			npcData.RotY = TransformRotation.eulerAngles.y;
			npcData.RotZ = TransformRotation.eulerAngles.z;

			if (animationEngine == AnimationEngine.Sprites2DToolkit || animationEngine == AnimationEngine.SpritesUnity)
			{
				npcData.idleAnim = idleAnimSprite;
				npcData.walkAnim = walkAnimSprite;
				npcData.talkAnim = talkAnimSprite;
				npcData.runAnim = runAnimSprite;
			}
			else if (animationEngine == AnimationEngine.Legacy)
			{
				npcData.idleAnim = AssetLoader.GetAssetInstanceID (idleAnim);
				npcData.walkAnim = AssetLoader.GetAssetInstanceID (walkAnim);
				npcData.runAnim = AssetLoader.GetAssetInstanceID (runAnim);
				npcData.talkAnim = AssetLoader.GetAssetInstanceID (talkAnim);
			}
			else if (animationEngine == AnimationEngine.Mecanim)
			{
				npcData.walkAnim = moveSpeedParameter;
				npcData.talkAnim = talkParameter;
				npcData.runAnim = turnParameter;
			}
			
			npcData.walkSound = AssetLoader.GetAssetInstanceID (walkSound);
			npcData.runSound = AssetLoader.GetAssetInstanceID (runSound);
			
			npcData.speechLabel = GetName ();
			npcData.displayLineID = displayLineID;
			npcData.portraitGraphic = AssetLoader.GetAssetInstanceID (portraitIcon.texture);
			
			npcData.walkSpeed = walkSpeedScale;
			npcData.runSpeed = runSpeedScale;
			
			// Rendering
			npcData.lockDirection = lockDirection;
			npcData.lockScale = lockScale;
			if (spriteChild && spriteChild.GetComponent <FollowSortingMap>())
			{
				npcData.lockSorting = spriteChild.GetComponent <FollowSortingMap>().lockSorting;
			}
			else if (GetComponent <FollowSortingMap>())
			{
				npcData.lockSorting = GetComponent <FollowSortingMap>().lockSorting;
			}
			else
			{
				npcData.lockSorting = false;
			}
			npcData.spriteDirection = spriteDirection;
			npcData.spriteScale = spriteScale;
			if (spriteChild && spriteChild.GetComponent <Renderer>())
			{
				npcData.sortingOrder = spriteChild.GetComponent <Renderer>().sortingOrder;
				npcData.sortingLayer = spriteChild.GetComponent <Renderer>().sortingLayerName;
			}
			else if (GetComponent <Renderer>())
			{
				npcData.sortingOrder = GetComponent <Renderer>().sortingOrder;
				npcData.sortingLayer = GetComponent <Renderer>().sortingLayerName;
			}
			
			npcData.pathID = 0;
			npcData.lastPathID = 0;
			if (GetPath ())
			{
				npcData.targetNode = GetTargetNode ();
				npcData.prevNode = GetPrevNode ();
				npcData.isRunning = isRunning;
				npcData.pathAffectY = GetPath ().affectY;
				
				if (GetPath () == GetComponent <Paths>())
				{
					npcData.pathData = Serializer.CreatePathData (GetComponent <Paths>());
				}
				else
				{
					if (GetPath ().GetComponent <ConstantID>())
					{
						npcData.pathID = GetPath ().GetComponent <ConstantID>().constantID;
					}
					else
					{
						ACDebug.LogWarning ("Want to save path data for " + name + " but path has no ID!");
					}
				}
			}
			
			if (GetLastPath ())
			{
				npcData.lastTargetNode = GetLastTargetNode ();
				npcData.lastPrevNode = GetLastPrevNode ();
				
				if (GetLastPath ().GetComponent <ConstantID>())
				{
					npcData.lastPathID = GetLastPath ().GetComponent <ConstantID>().constantID;
				}
				else
				{
					ACDebug.LogWarning ("Want to save previous path data for " + name + " but path has no ID!");
				}
			}
			
			if (followTarget)
			{
				if (!followTargetIsPlayer)
				{
					if (followTarget.GetComponent <ConstantID>())
					{
						npcData.followTargetID = followTarget.GetComponent <ConstantID>().constantID;
						npcData.followTargetIsPlayer = followTargetIsPlayer;
						npcData.followFrequency = followFrequency;
						npcData.followDistance = followDistance;
						npcData.followDistanceMax = followDistanceMax;
						npcData.followFaceWhenIdle = followFaceWhenIdle;
						npcData.followRandomDirection = followRandomDirection;
					}
					else
					{
						ACDebug.LogWarning ("Want to save follow data for " + name + " but " + followTarget.name + " has no ID!");
					}
				}
				else
				{
					npcData.followTargetID = 0;
					npcData.followTargetIsPlayer = followTargetIsPlayer;
					npcData.followFrequency = followFrequency;
					npcData.followDistance = followDistance;
					npcData.followDistanceMax = followDistanceMax;
					//followFaceWhenIdle = false;
					npcData.followFaceWhenIdle = followFaceWhenIdle;
					npcData.followRandomDirection = followRandomDirection;
				}
			}
			else
			{
				npcData.followTargetID = 0;
				npcData.followTargetIsPlayer = false;
				npcData.followFrequency = 0f;
				npcData.followDistance = 0f;
				npcData.followDistanceMax = 0f;
				npcData.followFaceWhenIdle = false;
				npcData.followRandomDirection = false;
			}
			
			if (headFacing == HeadFacing.Manual && headTurnTarget != null)
			{
				npcData.isHeadTurning = true;
				npcData.headTargetID = Serializer.GetConstantID (headTurnTarget);
				if (npcData.headTargetID == 0)
				{
					ACDebug.LogWarning ("The NPC " + gameObject.name + "'s head-turning target Transform, " + headTurnTarget + ", was not saved because it has no Constant ID");
				}
				npcData.headTargetX = headTurnTargetOffset.x;
				npcData.headTargetY = headTurnTargetOffset.y;
				npcData.headTargetZ = headTurnTargetOffset.z;
			}
			else
			{
				npcData.isHeadTurning = false;
				npcData.headTargetID = 0;
				npcData.headTargetX = 0f;
				npcData.headTargetY = 0f;
				npcData.headTargetZ = 0f;
			}

			if (GetComponentInChildren <FollowSortingMap>() != null)
			{
				FollowSortingMap followSortingMap = GetComponentInChildren <FollowSortingMap>();
				npcData.followSortingMap = followSortingMap.followSortingMap;
				if (!npcData.followSortingMap && followSortingMap.GetSortingMap () != null)
				{
					if (followSortingMap.GetSortingMap ().GetComponent <ConstantID>() != null)
					{
						npcData.customSortingMapID = followSortingMap.GetSortingMap ().GetComponent <ConstantID>().constantID;
					}
					else
					{
						ACDebug.LogWarning ("The NPC " + gameObject.name + "'s SortingMap, " + followSortingMap.GetSortingMap ().name + ", was not saved because it has no Constant ID");
						npcData.customSortingMapID = 0;
					}
					npcData.customSortingMapID = followSortingMap.GetSortingMap ().GetComponent <ConstantID>().constantID;
				}
				else
				{
					npcData.customSortingMapID = 0;
				}
			}
			else
			{
				npcData.followSortingMap = false;
				npcData.customSortingMapID = 0;
			}

			return npcData;
		}


		/**
		 * <summary>Updates its own variables from a NPCData class.</summary>
		 * <param name = "data">The NPCData class to load from</param>
		 */
		public void LoadData (NPCData data)
		{
			EndPath ();
			
			if (animationEngine == AnimationEngine.Sprites2DToolkit || animationEngine == AnimationEngine.SpritesUnity)
			{
				idleAnimSprite = data.idleAnim;
				walkAnimSprite = data.walkAnim;
				talkAnimSprite = data.talkAnim;
				runAnimSprite = data.runAnim;
			}
			else if (animationEngine == AnimationEngine.Legacy)
			{
				idleAnim = AssetLoader.RetrieveAsset (idleAnim, data.idleAnim);
				walkAnim = AssetLoader.RetrieveAsset (walkAnim, data.walkAnim);
				runAnim = AssetLoader.RetrieveAsset (runAnim, data.talkAnim);
				talkAnim = AssetLoader.RetrieveAsset (talkAnim, data.runAnim);
			}
			else if (animationEngine == AnimationEngine.Mecanim)
			{
				moveSpeedParameter = data.walkAnim;
				talkParameter = data.talkAnim;
				turnParameter = data.runAnim;;
			}
			
			walkSound = AssetLoader.RetrieveAsset (walkSound, data.walkSound);
			runSound = AssetLoader.RetrieveAsset (runSound, data.runSound);

			if (data.speechLabel != "")
			{
				SetName (data.speechLabel, data.displayLineID);
			}

			portraitIcon.texture = AssetLoader.RetrieveAsset (portraitIcon.texture, data.portraitGraphic);
			
			walkSpeedScale = data.walkSpeed;
			runSpeedScale = data.runSpeed;
			
			// Rendering
			lockDirection = data.lockDirection;
			lockScale = data.lockScale;
			if (spriteChild && spriteChild.GetComponent <FollowSortingMap>())
			{
				spriteChild.GetComponent <FollowSortingMap>().lockSorting = data.lockSorting;
			}
			else if (GetComponent <FollowSortingMap>())
			{
				GetComponent <FollowSortingMap>().lockSorting = data.lockSorting;
			}
			else
			{
				ReleaseSorting ();
			}
			
			if (data.lockDirection)
			{
				spriteDirection = data.spriteDirection;
			}
			if (data.lockScale)
			{
				spriteScale = data.spriteScale;
			}
			if (data.lockSorting)
			{
				if (spriteChild && spriteChild.GetComponent <Renderer>())
				{
					spriteChild.GetComponent <Renderer>().sortingOrder = data.sortingOrder;
					spriteChild.GetComponent <Renderer>().sortingLayerName = data.sortingLayer;
				}
				else if (GetComponent <Renderer>())
				{
					GetComponent <Renderer>().sortingOrder = data.sortingOrder;
					GetComponent <Renderer>().sortingLayerName = data.sortingLayer;
				}
			}
			
			AC.Char charToFollow = null;
			if (data.followTargetID != 0)
			{
				RememberNPC followNPC = Serializer.returnComponent <RememberNPC> (data.followTargetID);
				if (followNPC.GetComponent <AC.Char>())
				{
					charToFollow = followNPC.GetComponent <AC.Char>();
				}
			}
			
			FollowAssign (charToFollow, data.followTargetIsPlayer, data.followFrequency, data.followDistance, data.followDistanceMax, data.followFaceWhenIdle, data.followRandomDirection);
			Halt ();
			
			if (data.pathData != null && data.pathData != "" && GetComponent <Paths>())
			{
				Paths savedPath = GetComponent <Paths>();
				savedPath = Serializer.RestorePathData (savedPath, data.pathData);
				SetPath (savedPath, data.targetNode, data.prevNode, data.pathAffectY);
				isRunning = data.isRunning;
			}
			else if (data.pathID != 0)
			{
				Paths pathObject = Serializer.returnComponent <Paths> (data.pathID);
				
				if (pathObject != null)
				{
					SetPath (pathObject, data.targetNode, data.prevNode);
				}
				else
				{
					ACDebug.LogWarning ("Trying to assign a path for NPC " + this.name + ", but the path was not found - was it deleted?");
				}
			}
			
			if (data.lastPathID != 0)
			{
				Paths pathObject = Serializer.returnComponent <Paths> (data.lastPathID);
				
				if (pathObject != null)
				{
					SetLastPath (pathObject, data.lastTargetNode, data.lastPrevNode);
				}
				else
				{
					ACDebug.LogWarning ("Trying to assign the previous path for NPC " + this.name + ", but the path was not found - was it deleted?");
				}
			}
			
			// Head target
			if (data.isHeadTurning)
			{
				ConstantID _headTargetID = Serializer.returnComponent <ConstantID> (data.headTargetID);
				if (_headTargetID != null)
				{
					SetHeadTurnTarget (_headTargetID.transform, new Vector3 (data.headTargetX, data.headTargetY, data.headTargetZ), true);
				}
				else
				{
					ClearHeadTurnTarget (true);
				}
			}
			else
			{
				ClearHeadTurnTarget (true);
			}

			if (GetComponentsInChildren <FollowSortingMap>() != null)
			{
				FollowSortingMap[] followSortingMaps = GetComponentsInChildren <FollowSortingMap>();
				SortingMap customSortingMap = Serializer.returnComponent <SortingMap> (data.customSortingMapID);

				foreach (FollowSortingMap followSortingMap in followSortingMaps)
				{
					followSortingMap.followSortingMap = data.followSortingMap;
					if (!data.followSortingMap)
					{
						followSortingMap.SetSortingMap (customSortingMap);
					}
				}
			}
		}


		/**
		 * <summary>Gets the character that the NPC is currently following, if any.</summary>
		 * <returns>The character that the NPC is currently following, if any.</returns>
		 */
		public AC.Char GetFollowTarget ()
		{
			return followTarget;
		}
		
	}

}