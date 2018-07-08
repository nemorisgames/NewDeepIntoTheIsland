/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionAnim.cs"
 * 
 *	This action is used for standard animation playback for GameObjects.
 *	It is fairly simplistic, and not meant for characters.
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
	public class ActionAnim : Action
	{

		public int parameterID = -1;
		public int constantID = 0;

		// 3D variables
		
		public Animation _anim;
		public AnimationClip clip;
		public float fadeTime = 0f;
		
		// 2D variables
		
		public Transform _anim2D;
		public Animator animator;
		public string clip2D;
		public int clip2DParameterID = -1;
		public enum WrapMode2D { Once, Loop, PingPong };
		public WrapMode2D wrapMode2D;
		public int layerInt;

		// BlendShape variables

		public Shapeable shapeObject;
		public int shapeKey = 0;
		public float shapeValue = 0f;
		public bool isPlayer = false;

		// Mecanim variables

		public AnimMethodMecanim methodMecanim;
		public MecanimParameterType mecanimParameterType;
		public string parameterName;
		public int parameterNameID = -1;
		public float parameterValue;

		// Regular variables
		
		public AnimMethod method;
		
		public AnimationBlendMode blendMode = AnimationBlendMode.Blend;
		public AnimPlayMode playMode;
		
		public AnimationEngine animationEngine = AnimationEngine.Legacy;
		public string customClassName;
		public AnimEngine animEngine;

		
		public ActionAnim ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Animate";
			description = "Causes a GameObject to play or stop an animation, or modify a Blend Shape. The available options will differ depending on the chosen animation engine.";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (animEngine == null)
			{
				ResetAnimationEngine ();
			}
			
			if (animEngine != null)
			{
				animEngine.ActionAnimAssignValues (this, parameters);
			}

			parameterName = AssignString (parameters, parameterNameID, parameterName);
			clip2D = AssignString (parameters, clip2DParameterID, clip2D);

			if (method == AnimMethod.BlendShape && isPlayer)
			{
				if (KickStarter.player && KickStarter.player.GetComponent <Shapeable>())
				{
					shapeObject = KickStarter.player.GetComponent <Shapeable>();
				}
				else
				{
					shapeObject = null;
					ACDebug.LogWarning ("Cannot BlendShape Player since cannot find Shapeable script on Player.");
				}
			}
		}


		override public float Run ()
		{
			if (animEngine != null)
			{
				return animEngine.ActionAnimRun (this);
			}
			else
			{
				ACDebug.LogError ("Could not create animation engine!");
				return 0f;
			}
		}


		override public void Skip ()
		{
			if (animEngine != null)
			{
				animEngine.ActionAnimSkip (this);
			}
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			ResetAnimationEngine ();
			
			animationEngine = (AnimationEngine) EditorGUILayout.EnumPopup ("Animation engine:", animationEngine);
			if (animationEngine == AnimationEngine.Custom)
			{
				customClassName = EditorGUILayout.TextField ("Script name:", customClassName);
			}

			if (animEngine)
			{
				animEngine.ActionAnimGUI (this, parameters);
			}

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";

			if (animEngine)
			{
				labelAdd = " (" + animEngine.ActionAnimLabel (this) + ")";
			}

			return labelAdd;
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			if (!isPlayer && saveScriptsToo)
			{
				ResetAnimationEngine ();

				if (method == AnimMethod.PlayCustom)
				{
					if (animEngine != null && animator != null)
					{
						animEngine.AddSaveScript (this, animator.gameObject);
					}
				}
				else if (method == AnimMethod.BlendShape)
				{
					if (shapeObject != null)
					{
						AddSaveScript <RememberShapeable> (shapeObject);
					}
				}
			}
		}
		
		#endif


		private void ResetAnimationEngine ()
		{
			string className = "";
			if (animationEngine == AnimationEngine.Custom)
			{
				className = customClassName;
			}
			else
			{
				className = "AnimEngine_" + animationEngine.ToString ();
			}
				
			if (className != "" && (animEngine == null || animEngine.ToString () != className))
			{
				animEngine = (AnimEngine) ScriptableObject.CreateInstance (className);
			}
		}

	}

}