/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionTintMap.cs"
 * 
 *	This action changes which TintMap a FollowTintMap uses, and the intensity of the effect
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionTintMap : Action
	{

		public bool isPlayer;
		
		public FollowTintMap followTintMap;
		public int followTintMapConstantID = 0;
		public int followTintMapParameterID = -1;

		public TintMapMethod tintMapMethod = TintMapMethod.ChangeTintMap;

		public float newIntensity = 1f;
		public bool isInstant = true;
		public float timeToChange = 0f;

		public bool followDefault = false;
		public TintMap newTintMap;
		public int newTintMapConstantID = 0;
		public int newTintMapParameterID = -1;
		
		
		public ActionTintMap ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Change Tint map";
			description = "Changes which Tint map a Follow Tint Map component uses, and the intensity of the effect.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				if (KickStarter.player && KickStarter.player.spriteChild != null && KickStarter.player.spriteChild.GetComponent <FollowTintMap>())
				{
					followTintMap = KickStarter.player.spriteChild.GetComponent <FollowTintMap>();
				}
				else
				{
					ACDebug.LogWarning ("Could not find a FollowTintMap component on the Player - be sure to place one on the sprite child.");
				}
			}
			else
			{
				followTintMap = AssignFile <FollowTintMap> (parameters, followTintMapParameterID, followTintMapConstantID, followTintMap);
			}

			if (tintMapMethod == TintMapMethod.ChangeTintMap && !followDefault)
			{
				newTintMap = AssignFile <TintMap> (parameters, newTintMapParameterID, newTintMapConstantID, newTintMap);
			}

			if (timeToChange < 0f)
			{
				timeToChange = 0f;
			}
		}


		override public float Run ()
		{
			if (followTintMap == null)
			{
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				if (tintMapMethod == TintMapMethod.ChangeIntensity)
				{
					if (isInstant || timeToChange <= 0f)
					{
						followTintMap.SetIntensity (newIntensity);
					}
					else
					{
						followTintMap.SetIntensity (newIntensity, timeToChange);

						if (willWait)
						{
							return timeToChange;
						}
					}
				}
				else if (tintMapMethod == TintMapMethod.ChangeTintMap)
				{
					if (followDefault)
					{
						followTintMap.useDefaultTintMap = true;
						followTintMap.ResetTintMap ();
					}
					else
					{
						if (newTintMap)
						{
							followTintMap.useDefaultTintMap = false;
							followTintMap.tintMap = newTintMap;
							followTintMap.ResetTintMap ();
						}
						else
						{
							ACDebug.LogWarning ("Could not change " + followTintMap.gameObject.name + " - no alternative provided!");
						}
					}
				}
			}
			else
			{
				isRunning = false;
			}
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			tintMapMethod = (TintMapMethod) EditorGUILayout.EnumPopup ("Change to make:", tintMapMethod);

			isPlayer = EditorGUILayout.Toggle ("Affect Player?", isPlayer);
			if (!isPlayer)
			{
				followTintMapParameterID = Action.ChooseParameterGUI ("FollowTintMap:", parameters, followTintMapParameterID, ParameterType.GameObject);
				if (followTintMapParameterID >= 0)
				{
					followTintMapConstantID = 0;
					followTintMap = null;
				}
				else
				{
					followTintMap = (FollowTintMap) EditorGUILayout.ObjectField ("FollowTintMap:", followTintMap, typeof (FollowTintMap), true);
					
					followTintMapConstantID = FieldToID <FollowTintMap> (followTintMap, followTintMapConstantID);
					followTintMap = IDToField <FollowTintMap> (followTintMap, followTintMapConstantID, false);
				}
			}

			if (tintMapMethod == TintMapMethod.ChangeTintMap)
			{
				followDefault = EditorGUILayout.Toggle ("Use scene's default TintMap?", followDefault);
				if (!followDefault)
				{
					newTintMapParameterID = Action.ChooseParameterGUI ("New TintMap:", parameters, newTintMapParameterID, ParameterType.GameObject);
					if (newTintMapParameterID >= 0)
					{
						newTintMapConstantID = 0;
						followTintMap = null;
					}
					else
					{
						newTintMap = (TintMap) EditorGUILayout.ObjectField ("New TintMap:", newTintMap, typeof (TintMap), true);
						
						newTintMapConstantID = FieldToID <TintMap> (newTintMap, newTintMapConstantID);
						newTintMap = IDToField <TintMap> (newTintMap, newTintMapConstantID, false);
					}
				}
			}
			else if (tintMapMethod == TintMapMethod.ChangeIntensity)
			{
				newIntensity = EditorGUILayout.Slider ("New intensity:", newIntensity, 0f, 1f);
				isInstant = EditorGUILayout.Toggle ("Change instantly?", isInstant);
				if (!isInstant)
				{
					timeToChange = EditorGUILayout.FloatField ("Time to change (s):", timeToChange);
					if (timeToChange > 0f)
					{
						willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
					}
				}
			}
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			AssignConstantID <TintMap> (newTintMap, newTintMapConstantID, newTintMapParameterID);
		}


		override public string SetLabel ()
		{
			string labelAdd = " (" + tintMapMethod.ToString ();

			if (isPlayer)
			{
				labelAdd += " - Player";
			}
			else if (followTintMap != null && followTintMap.gameObject)
			{
				labelAdd += " - " + followTintMap.gameObject.name;
			}

			labelAdd += ")";
			return labelAdd;
		}
		
		#endif
		
	}
	
}