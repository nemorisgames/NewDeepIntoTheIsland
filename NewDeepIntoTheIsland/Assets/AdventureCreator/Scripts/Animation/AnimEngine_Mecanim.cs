/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"AnimEngine_Mecanim.cs"
 * 
 *	This script uses the Mecanim
 *	system for 3D animation.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	public class AnimEngine_Mecanim : AnimEngine
	{

		public override void Declare (AC.Char _character)
		{
			character = _character;
			turningStyle = TurningStyle.RootMotion;
		}


		public override void CharSettingsGUI ()
		{
			#if UNITY_EDITOR
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Mecanim parameters:", EditorStyles.boldLabel);

			character.moveSpeedParameter = EditorGUILayout.TextField ("Move speed float:", character.moveSpeedParameter);
			character.turnParameter = EditorGUILayout.TextField ("Turn float:", character.turnParameter);
			character.talkParameter = EditorGUILayout.TextField ("Talk bool:", character.talkParameter);

			if (AdvGame.GetReferences () && AdvGame.GetReferences ().speechManager &&
			    AdvGame.GetReferences ().speechManager.lipSyncMode != LipSyncMode.Off && AdvGame.GetReferences ().speechManager.lipSyncMode != LipSyncMode.FaceFX)
			{
				if (AdvGame.GetReferences ().speechManager.lipSyncOutput == LipSyncOutput.PortraitAndGameObject)
				{
					character.phonemeParameter = EditorGUILayout.TextField ("Phoneme integer:", character.phonemeParameter);
					if (character.GetShapeable ())
					{
						character.lipSyncGroupID = ActionBlendShape.ShapeableGroupGUI ("Phoneme shape group:", character.GetShapeable ().shapeGroups, character.lipSyncGroupID);
					}
				}
				else if (AdvGame.GetReferences ().speechManager.lipSyncOutput == LipSyncOutput.GameObjectTexture)
				{
					if (character.GetComponent <LipSyncTexture>() == null)
					{
						EditorGUILayout.HelpBox ("Attach a LipSyncTexture script to allow texture lip-syncing.", MessageType.Info);
					}
				}
			}

			if (!character.ikHeadTurning)
			{
				character.headYawParameter = EditorGUILayout.TextField ("Head yaw float:", character.headYawParameter);
				character.headPitchParameter = EditorGUILayout.TextField ("Head pitch float:", character.headPitchParameter);
			}

			character.verticalMovementParameter = EditorGUILayout.TextField ("Vertical movement float:", character.verticalMovementParameter);
			character.isGroundedParameter = EditorGUILayout.TextField ("'Is grounded' bool:", character.isGroundedParameter);
			if (character is Player)
			{
				Player player = (Player) character;
				player.jumpParameter = EditorGUILayout.TextField ("Jump bool:", player.jumpParameter);
			}
			character.talkingAnimation = TalkingAnimation.Standard;

			if (character.useExpressions)
			{
				character.expressionParameter = EditorGUILayout.TextField ("Expression ID integer:", character.expressionParameter);
			}

			EditorGUILayout.EndVertical ();
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Mecanim settings:", EditorStyles.boldLabel);

			if (SceneSettings.IsTopDown ())
			{
				character.spriteChild = (Transform) EditorGUILayout.ObjectField ("Animator child:", character.spriteChild, typeof (Transform), true);
			}
			else
			{
				character.spriteChild = null;
				character.customAnimator = (Animator) EditorGUILayout.ObjectField ("Animator (optional):", character.customAnimator, typeof (Animator), true);
			}

			character.headLayer = EditorGUILayout.IntField ("Head layer #:", character.headLayer);
			character.mouthLayer = EditorGUILayout.IntField ("Mouth layer #:", character.mouthLayer);

			character.ikHeadTurning = EditorGUILayout.Toggle ("IK head-turning?", character.ikHeadTurning);
			if (character.ikHeadTurning)
			{
				#if UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE
				EditorGUILayout.HelpBox ("'IK Pass' must be enabled for this character's Base layer.", MessageType.Info);
				#else
				EditorGUILayout.HelpBox ("This features is only available with Unity 5 or Unity Pro.", MessageType.Info);
				#endif
			}

			if (!Application.isPlaying)
			{
				character.ResetAnimator ();
			}
			Animator charAnimator = character.GetAnimator ();
			if (charAnimator != null && charAnimator.applyRootMotion)
			{
				character.rootTurningFactor = EditorGUILayout.Slider ("Root Motion turning:", character.rootTurningFactor, 0f, 1f);
			}
			character.doWallReduction = EditorGUILayout.BeginToggleGroup ("Slow movement near wall colliders?", character.doWallReduction);
			character.wallLayer = EditorGUILayout.TextField ("Wall collider layer:", character.wallLayer);
			character.wallDistance = EditorGUILayout.Slider ("Collider distance:", character.wallDistance, 0f, 2f);
			character.wallReductionOnlyParameter = EditorGUILayout.Toggle ("Only affects Mecanim parameter?", character.wallReductionOnlyParameter);
			EditorGUILayout.EndToggleGroup ();

			EditorGUILayout.EndVertical ();
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Bone transforms:", EditorStyles.boldLabel);

			character.neckBone = (Transform) EditorGUILayout.ObjectField ("Neck bone:", character.neckBone, typeof (Transform), true);
			character.leftHandBone = (Transform) EditorGUILayout.ObjectField ("Left hand:", character.leftHandBone, typeof (Transform), true);
			character.rightHandBone = (Transform) EditorGUILayout.ObjectField ("Right hand:", character.rightHandBone, typeof (Transform), true);
			EditorGUILayout.EndVertical ();

			if (GUI.changed)
			{
				EditorUtility.SetDirty (character);
			}

			#endif
		}


		public override void CharExpressionsGUI ()
		{
			#if UNITY_EDITOR
			if (character.useExpressions)
			{
				character.mapExpressionsToShapeable = EditorGUILayout.Toggle ("Map to Shapeable?", character.mapExpressionsToShapeable);
				if (character.mapExpressionsToShapeable)
				{
					if (character.GetShapeable ())
					{
						character.expressionGroupID = ActionBlendShape.ShapeableGroupGUI ("Expression shape group:", character.GetShapeable ().shapeGroups, character.expressionGroupID);
						EditorGUILayout.HelpBox ("The names of the expressions below must match the shape key labels.", MessageType.Info);
					}
					else
					{
						EditorGUILayout.HelpBox ("A Shapeable component must be present on the model's Skinned Mesh Renderer.", MessageType.Warning);
					}
				}
			}
			#endif
		}


		public override void ActionSpeechGUI (ActionSpeech action, Char speaker)
		{
			#if UNITY_EDITOR
			
			action.headClip2D = EditorGUILayout.TextField ("Head animation:", action.headClip2D);
			action.mouthClip2D = EditorGUILayout.TextField ("Mouth animation:", action.mouthClip2D);

			if (GUI.changed)
			{
				try
				{
					EditorUtility.SetDirty (action);
				} catch {}
			}
			
			#endif
		}


		public override void ActionSpeechRun (ActionSpeech action)
		{
			if (action.headClip2D != "" || action.mouthClip2D != "")
			{
				if (character.GetAnimator () == null)
				{
					return;
				}

				if (action.headClip2D != "")
				{
					character.GetAnimator ().CrossFade (action.headClip2D, 0.1f, character.headLayer);
				}
				if (action.mouthClip2D != "")
				{
					character.GetAnimator ().CrossFade (action.mouthClip2D, 0.1f, character.mouthLayer);
				}
			}
		}


		public override void ActionSpeechSkip (ActionSpeech action)
		{}


		public override void ActionCharAnimGUI (ActionCharAnim action, List<ActionParameter> parameters = null)
		{
			#if UNITY_EDITOR

			action.methodMecanim = (AnimMethodCharMecanim) EditorGUILayout.EnumPopup ("Method:", action.methodMecanim);
			
			if (action.methodMecanim == AnimMethodCharMecanim.ChangeParameterValue)
			{
				action.parameterNameID = Action.ChooseParameterGUI ("Parameter to affect:", parameters, action.parameterNameID, ParameterType.String);
				if (action.parameterNameID < 0)
				{
					action.parameterName = EditorGUILayout.TextField ("Parameter to affect:", action.parameterName);
				}

				action.mecanimParameterType = (MecanimParameterType) EditorGUILayout.EnumPopup ("Parameter type:", action.mecanimParameterType);
				if (action.mecanimParameterType == MecanimParameterType.Bool)
				{
					bool value = (action.parameterValue <= 0f) ? false : true;
					value = EditorGUILayout.Toggle ("Set as value:", value);
					action.parameterValue = (value) ? 1f : 0f;
				}
				else if (action.mecanimParameterType == MecanimParameterType.Int)
				{
					int value = (int) action.parameterValue;
					value = EditorGUILayout.IntField ("Set as value:", value);
					action.parameterValue = (float) value;
				}
				else if (action.mecanimParameterType == MecanimParameterType.Float)
				{
					action.parameterValue = EditorGUILayout.FloatField ("Set as value:", action.parameterValue);
				}
				else if (action.mecanimParameterType == MecanimParameterType.Trigger)
				{
					bool value = (action.parameterValue <= 0f) ? false : true;
					value = EditorGUILayout.Toggle ("Ignore when skipping?", value);
					action.parameterValue = (value) ? 1f : 0f;
				}
			}

			else if (action.methodMecanim == AnimMethodCharMecanim.SetStandard)
			{
				action.mecanimCharParameter = (MecanimCharParameter) EditorGUILayout.EnumPopup ("Parameter to change:", action.mecanimCharParameter);
				action.parameterName = EditorGUILayout.TextField ("New parameter name:", action.parameterName);

				if (action.mecanimCharParameter == MecanimCharParameter.MoveSpeedFloat)
				{
				    action.changeSpeed = EditorGUILayout.Toggle ("Change speed scale?", action.changeSpeed);
				    if (action.changeSpeed)
				    {
						action.newSpeed = EditorGUILayout.FloatField ("Walk speed scale:", action.newSpeed);
						action.parameterValue = EditorGUILayout.FloatField ("Run speed scale:", action.parameterValue);
					}

					action.changeSound = EditorGUILayout.Toggle ("Change sound?", action.changeSound);
					if (action.changeSound)
					{
						action.standard = (AnimStandard) EditorGUILayout.EnumPopup ("Change:", action.standard);
						if (action.standard == AnimStandard.Walk || action.standard == AnimStandard.Run)
						{
							action.newSound = (AudioClip) EditorGUILayout.ObjectField ("New sound:", action.newSound, typeof (AudioClip), false);
						}
						else
						{
							EditorGUILayout.HelpBox ("Only Walk and Run have a standard sounds.", MessageType.Info);
						}
					}
				}
			}

			else if (action.methodMecanim == AnimMethodCharMecanim.PlayCustom)
			{
				action.clip2DParameterID = Action.ChooseParameterGUI ("Clip name:", parameters, action.clip2DParameterID, ParameterType.String);
				if (action.clip2DParameterID < 0)
				{
					action.clip2D = EditorGUILayout.TextField ("Clip name:", action.clip2D);
				}
				action.includeDirection = EditorGUILayout.Toggle ("Add directional suffix?", action.includeDirection);
				
				action.layerInt = EditorGUILayout.IntField ("Mecanim layer:", action.layerInt);
				action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 1f);
				action.willWait = EditorGUILayout.Toggle ("Wait until finish?", action.willWait);
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty (action);
			}

			#endif
		}
		
		
		public override float ActionCharAnimRun (ActionCharAnim action)
		{
			return ActionCharAnimProcess (action, false);
		}


		public override void ActionCharAnimSkip (ActionCharAnim action)
		{
			ActionCharAnimProcess (action, true);
		}


		private float ActionCharAnimProcess (ActionCharAnim action, bool isSkipping)
		{
			if (action.methodMecanim == AnimMethodCharMecanim.SetStandard)
			{
				if (action.mecanimCharParameter == MecanimCharParameter.MoveSpeedFloat)
				{
					if (action.parameterName != "")
					{
						action.animChar.moveSpeedParameter = action.parameterName;
					}

					if (action.changeSpeed)
					{
						character.walkSpeedScale = action.newSpeed;
						character.runSpeedScale = action.parameterValue;
					}

					if (action.changeSound)
					{
						if (action.standard == AnimStandard.Walk)
						{
							action.animChar.walkSound = action.newSound;
						}
						else if (action.standard == AnimStandard.Run)
						{
							action.animChar.runSound = action.newSound;
						}
					}
				}
				else if (action.mecanimCharParameter == MecanimCharParameter.TalkBool)
				{
					action.animChar.talkParameter = action.parameterName;
				}
				else if (action.mecanimCharParameter == MecanimCharParameter.TurnFloat)
				{
					action.animChar.turnParameter = action.parameterName;
				}
				
				return 0f;
			}

			if (character.GetAnimator () == null)
			{
				return 0f;
			}
			
			if (!action.isRunning)
			{
				action.isRunning = true;
				if (action.methodMecanim == AnimMethodCharMecanim.ChangeParameterValue)
				{
					if (action.parameterName != "")
					{
						if (action.mecanimParameterType == MecanimParameterType.Float)
						{
							character.GetAnimator ().SetFloat (action.parameterName, action.parameterValue);
						}
						else if (action.mecanimParameterType == MecanimParameterType.Int)
						{
							character.GetAnimator ().SetInteger (action.parameterName, (int) action.parameterValue);
						}
						else if (action.mecanimParameterType == MecanimParameterType.Bool)
						{
							bool paramValue = (action.parameterValue > 0f) ? true : false;
							character.GetAnimator ().SetBool (action.parameterName, paramValue);
						}
						else if (action.mecanimParameterType == MecanimParameterType.Trigger)
						{
							if (!isSkipping || action.parameterValue < 1f)
							{
								character.GetAnimator ().SetTrigger (action.parameterName);
							}
						}
					}
				}
				else if (action.methodMecanim == AnimMethodCharMecanim.PlayCustom)
				{
					if (action.clip2D != "")
					{
						string clip2DNew = action.clip2D;
						if (action.includeDirection)
						{
							clip2DNew += action.animChar.GetSpriteDirection ();
						}
						character.GetAnimator ().CrossFade (clip2DNew, action.fadeTime, action.layerInt);
						
						if (action.willWait)
						{
							return (action.defaultPauseTime);
						}
					}
				}
			}
			else
			{
				if (action.methodMecanim == AnimMethodCharMecanim.PlayCustom)
				{
					if (action.clip2D != "")
					{
						if (character.GetAnimator ().GetCurrentAnimatorStateInfo (action.layerInt).normalizedTime < 0.98f)
						{
							return (action.defaultPauseTime / 6f);
						}
						else
						{
							action.isRunning = false;
							return 0f;
						}
					}
				}
			}
			
			return 0f;
		}


		public override bool ActionCharHoldPossible ()
		{
			return true;
		}


		public override void ActionAnimGUI (ActionAnim action, List<ActionParameter> parameters)
		{
			#if UNITY_EDITOR

			action.methodMecanim = (AnimMethodMecanim) EditorGUILayout.EnumPopup ("Method:", action.methodMecanim);

			if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue || action.methodMecanim == AnimMethodMecanim.PlayCustom)
			{
				action.parameterID = AC.Action.ChooseParameterGUI ("Animator:", parameters, action.parameterID, ParameterType.GameObject);
				if (action.parameterID >= 0)
				{
					action.constantID = 0;
					action.animator = null;
				}
				else
				{
					action.animator = (Animator) EditorGUILayout.ObjectField ("Animator:", action.animator, typeof (Animator), true);
					
					action.constantID = action.FieldToID <Animator> (action.animator, action.constantID);
					action.animator = action.IDToField <Animator> (action.animator, action.constantID, false);
				}
			}

			if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue)
			{
				action.parameterNameID = Action.ChooseParameterGUI ("Parameter to affect:", parameters, action.parameterNameID, ParameterType.String);
				if (action.parameterNameID < 0)
				{
					action.parameterName = EditorGUILayout.TextField ("Parameter to affect:", action.parameterName);
				}

				action.mecanimParameterType = (MecanimParameterType) EditorGUILayout.EnumPopup ("Parameter type:", action.mecanimParameterType);
				if (action.mecanimParameterType == MecanimParameterType.Bool)
				{
					bool value = (action.parameterValue <= 0f) ? false : true;
					value = EditorGUILayout.Toggle ("Set as value:", value);
					action.parameterValue = (value) ? 1f : 0f;
				}
				else if (action.mecanimParameterType == MecanimParameterType.Int)
				{
					int value = (int) action.parameterValue;
					value = EditorGUILayout.IntField ("Set as value:", value);
					action.parameterValue = (float) value;
				}
				else if (action.mecanimParameterType == MecanimParameterType.Float)
				{
					action.parameterValue = EditorGUILayout.FloatField ("Set as value:", action.parameterValue);
				}
				else if (action.mecanimParameterType == MecanimParameterType.Trigger)
				{
					bool value = (action.parameterValue <= 0f) ? false : true;
					value = EditorGUILayout.Toggle ("Ignore when skipping?", value);
					action.parameterValue = (value) ? 1f : 0f;
				}
			}
			else if (action.methodMecanim == AnimMethodMecanim.PlayCustom)
			{
				action.clip2DParameterID = Action.ChooseParameterGUI ("Clip name:", parameters, action.clip2DParameterID, ParameterType.String);
				if (action.clip2DParameterID < 0)
				{
					action.clip2D = EditorGUILayout.TextField ("Clip name:", action.clip2D);
				}
				action.layerInt = EditorGUILayout.IntField ("Mecanim layer:", action.layerInt);
				action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 2f);
				action.willWait = EditorGUILayout.Toggle ("Wait until finish?", action.willWait);
			}
			else if (action.methodMecanim == AnimMethodMecanim.BlendShape)
			{
				action.isPlayer = EditorGUILayout.Toggle ("Is player?", action.isPlayer);
				if (!action.isPlayer)
				{
					action.parameterID = AC.Action.ChooseParameterGUI ("Object:", parameters, action.parameterID, ParameterType.GameObject);
					if (action.parameterID >= 0)
					{
						action.constantID = 0;
						action.shapeObject = null;
					}
					else
					{
						action.shapeObject = (Shapeable) EditorGUILayout.ObjectField ("Object:", action.shapeObject, typeof (Shapeable), true);
						
						action.constantID = action.FieldToID <Shapeable> (action.shapeObject, action.constantID);
						action.shapeObject = action.IDToField <Shapeable> (action.shapeObject, action.constantID, false);
					}
				}

				action.shapeKey = EditorGUILayout.IntField ("Shape key:", action.shapeKey);
				action.shapeValue = EditorGUILayout.Slider ("Shape value:", action.shapeValue, 0f, 100f);
				action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 2f);
				action.willWait = EditorGUILayout.Toggle ("Wait until finish?", action.willWait);
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty (action);
			}
			
			#endif
		}


		public override string ActionAnimLabel (ActionAnim action)
		{
			string label = "";
			
			if (action.animator)
			{
				label = action.animator.name;
				
				if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue && action.parameterName != "")
				{
					label += " - " + action.parameterName;
				}
				else if (action.methodMecanim == AnimMethodMecanim.BlendShape)
				{
					label += " - Shapekey";
				}
			}
			
			return label;
		}


		public override void ActionAnimAssignValues (ActionAnim action, List<ActionParameter> parameters)
		{
			action.animator = action.AssignFile <Animator> (parameters, action.parameterID, action.constantID, action.animator);
			action.shapeObject = action.AssignFile <Shapeable> (parameters, action.parameterID, action.constantID, action.shapeObject);
		}


		public override float ActionAnimRun (ActionAnim action)
		{
			return ActionAnimProcess (action, false);
		}

		
		public override void ActionAnimSkip (ActionAnim action)
		{
			if (action.methodMecanim == AnimMethodMecanim.BlendShape)
			{
				if (action.shapeObject)
				{
					action.shapeObject.Change (action.shapeKey, action.shapeValue, action.fadeTime);
				}
			}
			else
			{
				ActionAnimProcess (action, true);
			}
		}


		private float ActionAnimProcess (ActionAnim action, bool isSkipping)
		{
			if (!action.isRunning)
			{
				action.isRunning = true;

				if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue && action.animator && action.parameterName != "")
				{
					if (action.mecanimParameterType == MecanimParameterType.Float)
					{
						action.animator.SetFloat (action.parameterName, action.parameterValue);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Int)
					{
						action.animator.SetInteger (action.parameterName, (int) action.parameterValue);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Bool)
					{
						bool paramValue = (action.parameterValue > 0f) ? true : false;
						action.animator.SetBool (action.parameterName, paramValue);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Trigger)
					{
						if (!isSkipping || action.parameterValue < 1f)
						{
							action.animator.SetTrigger (action.parameterName);
						}
					}
					
					return 0f;
				}

				else if (action.methodMecanim == AnimMethodMecanim.PlayCustom && action.animator)
				{
					if (action.clip2D != "")
					{
						#if UNITY_EDITOR && (UNITY_5 || UNITY_2017_1_OR_NEWER)

						int hash = Animator.StringToHash (action.clip2D);
						if (action.animator.HasState (0, hash))
						{
							action.animator.CrossFade (hash, action.fadeTime, action.layerInt);
						}
						else
						{
							ACDebug.LogError ("Cannot play clip " + action.clip2D + " on " + action.animator.name, action.animator);
						}
						
						#else
						
						try
						{
							action.animator.CrossFade (action.clip2D, action.fadeTime, action.layerInt);
						}
						catch
						{}
						
						#endif

						if (action.willWait)
						{
							return (action.defaultPauseTime);
						}
					}
				}
				
				else if (action.methodMecanim == AnimMethodMecanim.BlendShape && action.shapeKey > -1)
				{
					if (action.shapeObject)
					{
						action.shapeObject.Change (action.shapeKey, action.shapeValue, action.fadeTime);
						
						if (action.willWait)
						{
							return (action.fadeTime);
						}
					}
				}
			}
			else
			{
				if (action.methodMecanim == AnimMethodMecanim.BlendShape && action.shapeObject)
				{
					action.isRunning = false;
					return 0f;
				}
				else if (action.methodMecanim == AnimMethodMecanim.PlayCustom)
				{
					if (action.animator && action.clip2D != "")
					{
						if (action.animator.GetCurrentAnimatorStateInfo (action.layerInt).normalizedTime < 1f)
						{
							return (action.defaultPauseTime / 6f);
						}
						else
						{
							action.isRunning = false;
							return 0f;
						}
					}
				}
			}
			
			return 0f;
		}


		public override void ActionCharRenderGUI (ActionCharRender action)
		{
			#if UNITY_EDITOR
			
			EditorGUILayout.Space ();
			action.renderLock_scale = (RenderLock) EditorGUILayout.EnumPopup ("Character scale:", action.renderLock_scale);
			if (action.renderLock_scale == RenderLock.Set)
			{
				action.scale = EditorGUILayout.IntField ("New scale (%):", action.scale);
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty (action);
			}
			
			#endif
		}
		
		
		public override float ActionCharRenderRun (ActionCharRender action)
		{
			if (action.renderLock_scale == RenderLock.Set)
			{
				action._char.lockScale = true;
				float _scale = (float) action.scale / 100f;
				
				if (action._char.spriteChild != null)
				{
					action._char.spriteScale = _scale;
				}
				else
				{
					action._char.transform.localScale = new Vector3 (_scale, _scale, _scale);
				}
			}
			else if (action.renderLock_scale == RenderLock.Release)
			{
				action._char.lockScale = false;
			}
			
			return 0f;
		}


		public override void PlayIdle ()
		{
			if (character.GetAnimator () == null)
			{
				return;
			}

			MoveCharacter ();
			AnimTalk (character.GetAnimator ());

			if (character.turnParameter != "")
			{
				character.GetAnimator ().SetFloat (character.turnParameter, character.GetTurnFloat ());
			}

			if (character.IsPlayer)
			{
				Player player = (Player) character;
				
				if (player.jumpParameter != "")
				{
					character.GetAnimator ().SetBool (player.jumpParameter, player.isJumping);
				}
			}
		}


		public override void PlayWalk ()
		{
			if (character.GetAnimator () == null)
			{
				return;
			}

			MoveCharacter ();
			AnimTalk (character.GetAnimator ());

			if (character.turnParameter != "")
			{
				character.GetAnimator ().SetFloat (character.turnParameter, character.GetTurnFloat ());
			}

			if (character is Player)
			{
				Player player = (Player) character;
				
				if (player.jumpParameter != "")
				{
					character.GetAnimator ().SetBool (player.jumpParameter, player.isJumping);
				}
			}
		}


		private void MoveCharacter ()
		{
			if (character.moveSpeedParameter != "")
			{
				character.GetAnimator ().SetFloat (character.moveSpeedParameter, character.GetMoveSpeed (true));
			}
		}


		public override void PlayRun ()
		{
			if (character.GetAnimator () == null)
			{
				return;
			}

			MoveCharacter ();
			AnimTalk (character.GetAnimator ());

			if (character.turnParameter != "")
			{
				character.GetAnimator ().SetFloat (character.turnParameter, character.GetTurnFloat ());
			}

			if (character.IsPlayer)
			{
				Player player = (Player) character;
				
				if (player.jumpParameter != "")
				{
					character.GetAnimator ().SetBool (player.jumpParameter, player.isJumping);
				}
			}
		}


		public override void PlayTalk ()
		{
			PlayIdle ();
		}


		private void AnimTalk (Animator animator)
		{
			if (!string.IsNullOrEmpty (character.talkParameter))
			{
				animator.SetBool (character.talkParameter, character.isTalking);
			}

			if (!string.IsNullOrEmpty (character.phonemeParameter) && character.LipSyncGameObject ())
			{
				animator.SetInteger (character.phonemeParameter, character.GetLipSyncFrame ());
			}

			if (!string.IsNullOrEmpty (character.expressionParameter) && character.useExpressions)
			{
				animator.SetInteger (character.expressionParameter, character.GetExpressionID ());
			}
		}


		public override void PlayVertical ()
		{
			if (character.GetAnimator () == null)
			{
				return;
			}
			
			if (!string.IsNullOrEmpty (character.verticalMovementParameter))
			{
				character.GetAnimator ().SetFloat (character.verticalMovementParameter, character.GetHeightChange ());
			}

			if (!string.IsNullOrEmpty (character.isGroundedParameter))
			{
				character.GetAnimator ().SetBool (character.isGroundedParameter, character.IsGrounded ());
			}
		}


		public override void PlayJump ()
		{
			if (character.GetAnimator () == null)
			{
				return;
			}

			if (character.IsPlayer)
			{
				Player player = (Player) character;
				
				if (player.jumpParameter != "")
				{
					character.GetAnimator ().SetBool (player.jumpParameter, true);
				}

				AnimTalk (character.GetAnimator ());
			}
		}


		public override void TurnHead (Vector2 angles)
		{
			if (character.GetAnimator () == null)
			{
				return;
			}

			if (character.headYawParameter != "")
			{
				character.GetAnimator ().SetFloat (character.headYawParameter, angles.x);
			}

			if (character.headPitchParameter != "")
			{
				character.GetAnimator ().SetFloat (character.headPitchParameter, angles.y);
			}
		}


		public override void OnSetExpression ()
		{
			if (character.mapExpressionsToShapeable && character.GetShapeable () != null)
			{
				if (character.CurrentExpression != null)
				{
					character.GetShapeable ().SetActiveKey (character.expressionGroupID, character.CurrentExpression.label, 100f, 0.2f, MoveMethod.Smooth, null);
				}
				else
				{
					character.GetShapeable ().DisableAllKeys (character.expressionGroupID, 0.2f, MoveMethod.Smooth, null);
				}
			}
		}


		#if UNITY_EDITOR && (UNITY_5 || UNITY_2017_1_OR_NEWER)

		public override void AddSaveScript (Action _action, GameObject _gameObject)
		{
			if (_gameObject != null && _gameObject.GetComponentInChildren <Animator>())
			{
				_action.AddSaveScript <RememberAnimator> (_gameObject.GetComponentInChildren <Animator>());
			}
		}

		#endif
		
	}

}