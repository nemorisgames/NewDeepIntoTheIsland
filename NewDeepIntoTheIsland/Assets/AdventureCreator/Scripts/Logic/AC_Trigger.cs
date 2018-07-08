/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"AC_Trigger.cs"
 * 
 *	This ActionList runs when the Player enters it.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * An ActionList that is run when the Player, or another object, comes into contact with it.
	 */
	[System.Serializable]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_a_c___trigger.html")]
	#endif
	public class AC_Trigger : ActionList
	{

		/** What the Trigger will react to (Player, SetObject, AnyObject, AnyObjectWithComponent) */
		public TriggerDetects detects = TriggerDetects.Player;
		/** The GameObject that the Trigger reacts to, if detects = TriggerDetects.SetObject */
		public GameObject obToDetect;
		/** The component that must be attached to an object for the Trigger to react to, if detects = TriggerDetects.AnyObjectWithComponent */
		public string detectComponent = "";

		/** What kind of contact the Trigger reacts to (0 = "On enter", 1 = "Continuous", 2 = "On exit") */
		public int triggerType;
		/** If True, then a Gizmo will be drawn in the Scene window at the Trigger's position */
		public bool showInEditor = false;
		/** If True, and the Player sets off the Trigger while walking towards a Hotspot Interaction, then the Player will stop, and the Interaction will be cancelled */
		public bool cancelInteractions = false;
		/** The state of the game under which the trigger reactrs (OnlyDuringGameplay, OnlyDuringCutscenes, DuringCutscenesAndGameplay) */
		public TriggerReacts triggerReacts = TriggerReacts.OnlyDuringGameplay;
		

		private void Interact (GameObject collisionOb)
		{
			if (cancelInteractions)
			{
				KickStarter.playerInteraction.StopMovingToHotspot ();
			}
			
			if (actionListType == ActionListType.PauseGameplay)
			{
				KickStarter.playerInteraction.DeselectHotspot (false);
			}
			
			// Set correct parameter
			if (useParameters && parameters != null && parameters.Count >= 1)
			{
				if (parameters[0].parameterType == ParameterType.GameObject)
				{
					parameters[0].gameObject = collisionOb;
				}
				else
				{
					ACDebug.Log ("Cannot set the value of parameter 0 ('" + parameters[0].label + "') as it is not of the type 'Game Object'.");
				}
			}
			
			Interact ();
		}
		
		
		private void OnTriggerEnter (Collider other)
		{
			if (triggerType == 0 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerEnter2D (Collider2D other)
		{
			if (triggerType == 0 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerStay (Collider other)
		{
			if (triggerType == 1 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerStay2D (Collider2D other)
		{
			if (triggerType == 1 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerExit (Collider other)
		{
			if (triggerType == 2 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}
		
		
		private void OnTriggerExit2D (Collider2D other)
		{
			if (triggerType == 2 && IsObjectCorrect (other.gameObject))
			{
				Interact (other.gameObject);
			}
		}


		/**
		 * <summary>Checks if the Trigger is enabled.</summary>
		 * <returns>True if the Trigger is enabled.</summary>
		 */
		public bool IsOn ()
		{
			if (GetComponent <Collider>())
			{
				return GetComponent <Collider>().enabled;
			}
			else if (GetComponent <Collider2D>())
			{
				return GetComponent <Collider2D>().enabled;
			}
			return false;
		}
		

		/**
		 * <summary>Enables the Trigger.</summary>
		 */
		public void TurnOn ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = true;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = true;
			}
			else
			{
				ACDebug.LogWarning ("Cannot turn " + this.name + " on because it has no Collider component.");
			}
		}
		

		/**
		 * <summary>Disables the Trigger.</summary>
		 */
		public void TurnOff ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = false;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = false;
			}
			else
			{
				ACDebug.LogWarning ("Cannot turn " + this.name + " off because it has no Collider component.");
			}
		}
		
		
		private bool IsObjectCorrect (GameObject obToCheck)
		{
			if (KickStarter.stateHandler == null || KickStarter.stateHandler.gameState == GameState.Paused || obToCheck == null)
			{
				return false;
			}

			if (triggerReacts == TriggerReacts.OnlyDuringGameplay && KickStarter.stateHandler.gameState != GameState.Normal)
			{
				return false;
			}
			else if (triggerReacts == TriggerReacts.OnlyDuringCutscenes && KickStarter.stateHandler.gameState == GameState.Normal)
			{
				return false;
			}

			if (KickStarter.stateHandler != null && KickStarter.stateHandler.AreTriggersDisabled ())
			{
				return false;
			}

			if (detects == TriggerDetects.Player)
			{
				if (obToCheck.CompareTag (Tags.player))
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.SetObject)
			{
				if (obToDetect != null && obToCheck == obToDetect)
				{
					return true;
				}
			}
			else if (detects == TriggerDetects.AnyObjectWithComponent)
			{
				if (!string.IsNullOrEmpty (detectComponent))
				{
					string[] allComponents = detectComponent.Split (";"[0]);
					foreach (string component in allComponents)
					{
						if (!string.IsNullOrEmpty (component) && obToCheck.GetComponent (component))
						{
							return true;
						}
					}
				}
			}
			else if (detects == TriggerDetects.AnyObjectWithTag)
			{
				if (!string.IsNullOrEmpty (detectComponent))
				{
					string[] allComponents = detectComponent.Split (";"[0]);
					foreach (string component in allComponents)
					{
						if (!string.IsNullOrEmpty (component) && obToCheck.tag == component)
						{
							return true;
						}
					}
				}
			}
			else if (detects == TriggerDetects.AnyObject)
			{
				return true;
			}
			
			return false;
		}


		#if UNITY_EDITOR
		
		private void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		private void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}
		
		
		private void DrawGizmos ()
		{
			Color gizmoColor = new Color (1f, 0.3f, 0f, 0.8f);

			if (GetComponent <PolygonCollider2D>())
			{
				AdvGame.DrawPolygonCollider (transform, GetComponent <PolygonCollider2D>(), gizmoColor);
			}
			else if (GetComponent <MeshCollider>())
			{
				AdvGame.DrawMeshCollider (transform, GetComponent <MeshCollider>().sharedMesh, gizmoColor);
			}
			else if (GetComponent <SphereCollider>())
			{
				AdvGame.DrawSphereCollider (transform, GetComponent <SphereCollider>(), gizmoColor);
			}
			else if (GetComponent <BoxCollider2D>() != null || GetComponent <BoxCollider>() != null)
			{
				AdvGame.DrawCubeCollider (transform, gizmoColor);
			}
		}

		#endif

	}
	
}
