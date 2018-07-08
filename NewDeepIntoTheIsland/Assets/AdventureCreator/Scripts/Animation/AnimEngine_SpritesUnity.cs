/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"AnimEngine_SpritesUnity.cs"
 * 
 *	This script uses Unity's built-in 2D
 *	sprite engine for animation.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	public class AnimEngine_SpritesUnity : AnimEngine
	{

		#if UNITY_EDITOR
		private bool listExpectedAnimations = false;
		#endif

		private string hideHeadClip = "HideHead";
		private string headDirection;


		public override void Declare (AC.Char _character)
		{
			character = _character;
			turningStyle = TurningStyle.Linear;
			isSpriteBased = true;
			updateHeadAlways = true;
		}
		
		
		private string ShowExpected (AC.Char character, string animName, string result, int layerIndex)
		{
			if (character == null || animName == "")
			{
				return result;
			}

			string indexString = "   (" + layerIndex + ")";

			if (character.doDirections)
			{
				result += "\n- " + animName + "_U" + indexString;
				result += "\n- " + animName + "_D" + indexString;
				
				if (character.frameFlipping == AC_2DFrameFlipping.LeftMirrorsRight)
				{
					result += "\n- " + animName + "_R" + indexString;
				}
				else if (character.frameFlipping == AC_2DFrameFlipping.RightMirrorsLeft)
				{
					result += "\n- " + animName + "_L" + indexString;
				}
				else
				{
					result += "\n- " + animName + "_L" + indexString;
					result += "\n- " + animName + "_R" + indexString;
				}
				
				if (character.doDiagonals)
				{
					if (character.frameFlipping == AC_2DFrameFlipping.LeftMirrorsRight)
					{
						result += "\n- " + animName + "_UR" + indexString;
						result += "\n- " + animName + "_DR" + indexString;
					}
					else if (character.frameFlipping == AC_2DFrameFlipping.RightMirrorsLeft)
					{
						result += "\n- " + animName + "_UL" + indexString;
						result += "\n- " + animName + "_DL" + indexString;
					}
					else
					{
						result += "\n- " + animName + "_UL" + indexString;
						result += "\n- " + animName + "_DL" + indexString;
						result += "\n- " + animName + "_DR" + indexString;
						result += "\n- " + animName + "_UR" + indexString;
					}
				}
				
				result += "\n";
			}
			else
			{
				result += "\n- " + animName + indexString;
			}

			return result;
		}
		
		
		public override void CharSettingsGUI ()
		{
			#if UNITY_EDITOR
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Standard 2D animations:", EditorStyles.boldLabel);
			
			character.talkingAnimation = (TalkingAnimation) EditorGUILayout.EnumPopup ("Talk animation style:", character.talkingAnimation);
			character.spriteChild = (Transform) EditorGUILayout.ObjectField ("Sprite child:", character.spriteChild, typeof (Transform), true);

			if (character.spriteChild != null && character.spriteChild.GetComponent <Animator>() == null)
			{
				character.customAnimator = (Animator) EditorGUILayout.ObjectField ("Animator (if not on s.c.):", character.customAnimator, typeof (Animator), true);
			}

			character.idleAnimSprite = EditorGUILayout.TextField ("Idle name:", character.idleAnimSprite);
			character.walkAnimSprite = EditorGUILayout.TextField ("Walk name:", character.walkAnimSprite);
			character.runAnimSprite = EditorGUILayout.TextField ("Run name:", character.runAnimSprite);
			if (character.talkingAnimation == TalkingAnimation.Standard)
			{
				character.talkAnimSprite = EditorGUILayout.TextField ("Talk name:", character.talkAnimSprite);
				character.separateTalkingLayer = EditorGUILayout.Toggle ("Head on separate layer?", character.separateTalkingLayer);
				if (character.separateTalkingLayer)
				{
					character.headLayer = EditorGUILayout.IntField ("Head layer:", character.headLayer);
					if (character.headLayer < 1)
					{
						EditorGUILayout.HelpBox ("The head layer index must be 1 or greater.", MessageType.Warning);
					}
				}
			}
			character.doDirections = EditorGUILayout.Toggle ("Multiple directions?", character.doDirections);
			if (character.doDirections)
			{
				character.doDiagonals = EditorGUILayout.Toggle ("Diagonal sprites?", character.doDiagonals);
				character.frameFlipping = (AC_2DFrameFlipping) EditorGUILayout.EnumPopup ("Frame flipping:", character.frameFlipping);
				if (character.frameFlipping != AC_2DFrameFlipping.None)
				{
					character.flipCustomAnims = EditorGUILayout.Toggle ("Flip custom animations?", character.flipCustomAnims);
				}
			}
			
			character.crossfadeAnims = EditorGUILayout.Toggle ("Crossfade animation?", character.crossfadeAnims);
			
			Animator charAnimator = character.GetAnimator ();
			if (charAnimator == null || !charAnimator.applyRootMotion)
			{
				character.antiGlideMode = EditorGUILayout.ToggleLeft ("Only move when sprite changes?", character.antiGlideMode);

				if (character.antiGlideMode)
				{
					if (character.GetComponent <Rigidbody2D>())
					{
						EditorGUILayout.HelpBox ("This feature will disable use of the Rigidbody2D component.", MessageType.Warning);
					}
					if (character is Player && AdvGame.GetReferences () != null && AdvGame.GetReferences ().settingsManager != null)
					{
						if (AdvGame.GetReferences ().settingsManager.movementMethod != MovementMethod.PointAndClick && AdvGame.GetReferences ().settingsManager.movementMethod != MovementMethod.None)
						{
							EditorGUILayout.HelpBox ("This feature will not work with collision - it is not recommended for " + AdvGame.GetReferences ().settingsManager.movementMethod.ToString () + " movement.", MessageType.Warning);
						}
					}
				}
			}
			
			if (SceneSettings.CameraPerspective != CameraPerspective.TwoD)
			{
				character.rotateSprite3D = (RotateSprite3D) EditorGUILayout.EnumPopup ("Rotate sprite to:", character.rotateSprite3D);
			}

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (" ", GUILayout.Width (9));
			listExpectedAnimations = EditorGUILayout.Foldout (listExpectedAnimations, "List expected animations?");
			EditorGUILayout.EndHorizontal ();
			if (listExpectedAnimations)
			{
				string result = "\n";
				result = ShowExpected (character, character.idleAnimSprite, result, 0);
				result = ShowExpected (character, character.walkAnimSprite, result, 0);
				result = ShowExpected (character, character.runAnimSprite, result, 0);
				if (character.talkingAnimation == TalkingAnimation.Standard)
				{
					if (character.separateTalkingLayer)
					{
						result = ShowExpected (character, character.idleAnimSprite, result, character.headLayer);
						result = ShowExpected (character, character.talkAnimSprite, result, character.headLayer);
						result += "\n- " + hideHeadClip + "  (" + character.headLayer + ")";
					}
					else
					{
						result = ShowExpected (character, character.talkAnimSprite, result, 0);
					}
				}
				
				EditorGUILayout.HelpBox ("The following animations are required, based on the settings above (numbers are the Animator layer indices):" + result, MessageType.Info);
			}

			EditorGUILayout.EndVertical ();
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (character);
			}
			
			#endif
		}
		
		
		public override void ActionCharAnimGUI (ActionCharAnim action, List<ActionParameter> parameters = null)
		{
			#if UNITY_EDITOR
			
			action.method = (ActionCharAnim.AnimMethodChar) EditorGUILayout.EnumPopup ("Method:", action.method);
			
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
			{
				action.clip2DParameterID = Action.ChooseParameterGUI ("Clip:", parameters, action.clip2DParameterID, ParameterType.String);
				if (action.clip2DParameterID < 0)
				{
					action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
				}

				action.includeDirection = EditorGUILayout.Toggle ("Add directional suffix?", action.includeDirection);

				if (action.animChar != null && action.animChar.talkingAnimation == TalkingAnimation.Standard && action.animChar.separateTalkingLayer)
				{
					action.hideHead = EditorGUILayout.Toggle ("Hide head?", action.hideHead);
					if (action.hideHead)
					{
						EditorGUILayout.HelpBox ("The head layer will play '" + hideHeadClip + "' for the duration.", MessageType.Info);
					}
				}
								
				action.layerInt = EditorGUILayout.IntField ("Mecanim layer:", action.layerInt);
				action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 1f);
				action.willWait = EditorGUILayout.Toggle ("Wait until finish?", action.willWait);
				if (action.willWait)
				{
					action.idleAfter = EditorGUILayout.Toggle ("Return to idle after?", action.idleAfter);
				}
			}
			else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom)
			{
				EditorGUILayout.HelpBox ("This Action does not work for Sprite-based characters.", MessageType.Info);
			}
			else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
			{
				action.standard = (AnimStandard) EditorGUILayout.EnumPopup ("Change:", action.standard);

				action.clip2DParameterID = Action.ChooseParameterGUI ("Clip:", parameters, action.clip2DParameterID, ParameterType.String);
				if (action.clip2DParameterID < 0)
				{
					action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
				}

				if (action.standard == AnimStandard.Walk || action.standard == AnimStandard.Run)
				{
					action.changeSound = EditorGUILayout.Toggle ("Change sound?", action.changeSound);
					if (action.changeSound)
					{
						action.newSoundParameterID = Action.ChooseParameterGUI ("New sound:", parameters, action.newSoundParameterID, ParameterType.UnityObject);
						if (action.newSoundParameterID < 0)
						{
							action.newSound = (AudioClip) EditorGUILayout.ObjectField ("New sound:", action.newSound, typeof (AudioClip), false);
						}
					}
					action.changeSpeed = EditorGUILayout.Toggle ("Change speed?", action.changeSpeed);
					if (action.changeSpeed)
					{
						action.newSpeed = EditorGUILayout.FloatField ("New speed:", action.newSpeed);
					}
				}
			}
			else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				action.idleAfterCustom = EditorGUILayout.Toggle ("Wait for animation to finish?", action.idleAfterCustom);
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (action);
			}
			
			#endif
		}
		
		
		public override float ActionCharAnimRun (ActionCharAnim action)
		{
			string clip2DNew = action.clip2D;
			if (action.includeDirection)
			{
				clip2DNew += action.animChar.GetSpriteDirection ();
			}
			
			if (!action.isRunning)
			{
				action.isRunning = true;
				
				if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip2D != "")
				{
					if (action.animChar.GetAnimator ())
					{
						#if UNITY_EDITOR && (UNITY_5 || UNITY_2017_1_OR_NEWER)
						int hash = Animator.StringToHash (clip2DNew);
						if (!character.GetAnimator ().HasState (action.layerInt, hash))
						{
							ACDebug.LogError ("Cannot play clip " + clip2DNew + " on " + character.name);
						}
						#endif

						action.animChar.charState = CharState.Custom;
						action.animChar.GetAnimator ().CrossFade (clip2DNew, action.fadeTime, action.layerInt);

						if (action.hideHead && action.animChar.talkingAnimation == TalkingAnimation.Standard && action.animChar.separateTalkingLayer)
						{
							PlayHeadAnim (hideHeadClip, false);
						}
					}
				}
				
				else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
				{
					if (action.idleAfterCustom)
					{
						action.layerInt = 0;
						return (action.defaultPauseTime);
					}
					else
					{
						action.animChar.ResetBaseClips ();
						action.animChar.charState = CharState.Idle;
					}
				}
				
				else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
				{
					if (action.clip2D != "")
					{
						if (action.standard == AnimStandard.Idle)
						{
							action.animChar.idleAnimSprite = action.clip2D;
						}
						else if (action.standard == AnimStandard.Walk)
						{
							action.animChar.walkAnimSprite = action.clip2D;
						}
						else if (action.standard == AnimStandard.Talk)
						{
							action.animChar.talkAnimSprite = action.clip2D;
						}
						else if (action.standard == AnimStandard.Run)
						{
							action.animChar.runAnimSprite = action.clip2D;
						}
					}
					
					if (action.changeSpeed)
					{
						if (action.standard == AnimStandard.Walk)
						{
							action.animChar.walkSpeedScale = action.newSpeed;
						}
						else if (action.standard == AnimStandard.Run)
						{
							action.animChar.runSpeedScale = action.newSpeed;
						}
					}
					
					if (action.changeSound)
					{
						if (action.standard == AnimStandard.Walk)
						{
							if (action.newSound != null)
							{
								action.animChar.walkSound = action.newSound;
							}
							else
							{
								action.animChar.walkSound = null;
							}
						}
						else if (action.standard == AnimStandard.Run)
						{
							if (action.newSound != null)
							{
								action.animChar.runSound = action.newSound;
							}
							else
							{
								action.animChar.runSound = null;
							}
						}
					}
				}
				
				if (action.willWait && action.clip2D != "")
				{
					if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
					{
						return (action.defaultPauseTime);
					}
				}
			}	
			
			else
			{
				if (action.animChar.GetAnimator ())
				{
					// Calc how much longer left to wait
					float totalLength = action.animChar.GetAnimator ().GetCurrentAnimatorStateInfo (action.layerInt).length;
					float timeLeft = (1f - action.animChar.GetAnimator ().GetCurrentAnimatorStateInfo (action.layerInt).normalizedTime) * totalLength;
					
					// Subtract a small amount of time to prevent overshooting
					timeLeft -= 0.1f;
					
					if (timeLeft > 0f)
					{
						return (timeLeft);
					}
					else
					{
						if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
						{
							action.animChar.ResetBaseClips ();
							action.animChar.charState = CharState.Idle;
						}
						else if (action.idleAfter)
						{
							action.animChar.charState = CharState.Idle;
						}
						
						action.isRunning = false;
						return 0f;
					}
				}
				else
				{
					action.isRunning = false;
					action.animChar.charState = CharState.Idle;
					return 0f;
				}
			}
			
			return 0f;
		}
		
		
		public override void ActionCharAnimSkip (ActionCharAnim action)
		{
			if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
			{
				ActionCharAnimRun (action);
				return;
			}
			else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				action.animChar.ResetBaseClips ();
				action.animChar.charState = CharState.Idle;
				return;
			}
			
			string clip2DNew = action.clip2D;
			if (action.includeDirection)
			{
				clip2DNew += action.animChar.GetSpriteDirection ();
			}
			
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
			{
				if (action.willWait && action.idleAfter)
				{
					action.animChar.charState = CharState.Idle;
				}
				else if (action.animChar.GetAnimator ())
				{
					action.animChar.charState = CharState.Custom;
					action.animChar.GetAnimator ().Play (clip2DNew, action.layerInt, 0.8f);
				}
			}
		}
		
		
		public override void ActionSpeechGUI (ActionSpeech action, Char speaker)
		{
			#if UNITY_EDITOR
			
			if (speaker != null && speaker.talkingAnimation == TalkingAnimation.CustomFace)
			{
				action.play2DHeadAnim = EditorGUILayout.BeginToggleGroup ("Custom head animation?", action.play2DHeadAnim);
				action.headClip2D = EditorGUILayout.TextField ("Head animation:", action.headClip2D);
				action.headLayer = EditorGUILayout.IntField ("Mecanim layer:", action.headLayer);
				EditorGUILayout.EndToggleGroup ();
				
				action.play2DMouthAnim = EditorGUILayout.BeginToggleGroup ("Custom mouth animation?", action.play2DMouthAnim);
				action.mouthClip2D = EditorGUILayout.TextField ("Mouth animation:", action.mouthClip2D);
				action.mouthLayer = EditorGUILayout.IntField ("Mecanim layer:", action.mouthLayer);
				EditorGUILayout.EndToggleGroup ();
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (action);
			}
			
			#endif
		}
		
		
		public override void ActionSpeechRun (ActionSpeech action)
		{
			if (action.Speaker.talkingAnimation == TalkingAnimation.CustomFace && action.Speaker.GetAnimator ())
			{
				if (action.play2DHeadAnim && action.headClip2D != "")
				{
					try
					{
						action.Speaker.GetAnimator ().Play (action.headClip2D, action.headLayer);
					}
					catch {}
				}
				
				if (action.play2DMouthAnim && action.mouthClip2D != "")
				{
					try
					{
						action.Speaker.GetAnimator ().Play (action.mouthClip2D, action.mouthLayer);
					}
					catch {}
				}
			}
		}
		
		
		public override void ActionAnimGUI (ActionAnim action, List<ActionParameter> parameters)
		{
			#if UNITY_EDITOR
			
			action.method = (AnimMethod) EditorGUILayout.EnumPopup ("Method:", action.method);
			
			if (action.method == AnimMethod.PlayCustom)
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

				action.clip2DParameterID = Action.ChooseParameterGUI ("Clip:", parameters, action.clip2DParameterID, ParameterType.String);
				if (action.clip2DParameterID < 0)
				{
					action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
				}

				action.layerInt = EditorGUILayout.IntField ("Mecanim layer:", action.layerInt);
				action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 2f);
				action.willWait = EditorGUILayout.Toggle ("Wait until finish?", action.willWait);
			}
			else if (action.method == AnimMethod.StopCustom)
			{
				EditorGUILayout.HelpBox ("'Stop Custom' is not available for Unity-based 2D animation.", MessageType.Info);
			}
			else if (action.method == AnimMethod.BlendShape)
			{
				EditorGUILayout.HelpBox ("BlendShapes are not available in 2D animation.", MessageType.Info);
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
				
				if (action.method == AnimMethod.PlayCustom && action.clip2D != "")
				{
					label += " - " + action.clip2D;
				}
			}
			
			return label;
		}
		
		
		public override void ActionAnimAssignValues (ActionAnim action, List<ActionParameter> parameters)
		{
			action.animator = action.AssignFile <Animator> (parameters, action.parameterID, action.constantID, action.animator);
		}
		
		
		public override float ActionAnimRun (ActionAnim action)
		{
			if (!action.isRunning)
			{
				action.isRunning = true;
				
				if (action.animator && action.clip2D != "")
				{
					if (action.method == AnimMethod.PlayCustom)
					{
						action.animator.CrossFade (action.clip2D, action.fadeTime, action.layerInt);
						
						if (action.willWait)
						{
							return (action.defaultPauseTime);
						}
					}
					else if (action.method == AnimMethod.BlendShape)
					{
						ACDebug.LogWarning ("BlendShapes not available for 2D animation.");
						return 0f;
					}
				}
			}
			else
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
			
			return 0f;
		}
		
		
		public override void ActionAnimSkip (ActionAnim action)
		{
			if (action.animator && action.clip2D != "")
			{
				if (action.method == AnimMethod.PlayCustom)
				{
					action.animator.Play (action.clip2D, action.layerInt, 0.8f);
				}
			}
		}
		
		
		public override void ActionCharRenderGUI (ActionCharRender action)
		{
			#if UNITY_EDITOR
			
			EditorGUILayout.Space ();
			action.renderLock_scale = (RenderLock) EditorGUILayout.EnumPopup ("Sprite scale:", action.renderLock_scale);
			if (action.renderLock_scale == RenderLock.Set)
			{
				action.scale = EditorGUILayout.IntField ("New scale (%):", action.scale);
			}
			
			EditorGUILayout.Space ();
			action.renderLock_direction = (RenderLock) EditorGUILayout.EnumPopup ("Sprite direction:", action.renderLock_direction);
			if (action.renderLock_direction == RenderLock.Set)
			{
				action.direction = (CharDirection) EditorGUILayout.EnumPopup ("New direction:", action.direction);
			}
			
			EditorGUILayout.Space ();
			action.renderLock_sortingMap = (RenderLock) EditorGUILayout.EnumPopup ("Sorting Map:", action.renderLock_sortingMap);
			if (action.renderLock_sortingMap == RenderLock.Set)
			{
				action.sortingMap = (SortingMap) EditorGUILayout.ObjectField ("New Sorting Map:", action.sortingMap, typeof (SortingMap), true);
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
				action._char.spriteScale = (float) action.scale / 100f;
			}
			else if (action.renderLock_scale == RenderLock.Release)
			{
				action._char.lockScale = false;
			}
			
			if (action.renderLock_direction == RenderLock.Set)
			{
				action._char.SetSpriteDirection (action.direction);
			}
			else if (action.renderLock_direction == RenderLock.Release)
			{
				action._char.lockDirection = false;
			}
			
			if (action.renderLock_sortingMap != RenderLock.NoChange && action._char.GetComponentInChildren <FollowSortingMap>())
			{
				FollowSortingMap[] followSortingMaps = action._char.GetComponentsInChildren <FollowSortingMap>();
				SortingMap sortingMap = (action.renderLock_sortingMap == RenderLock.Set) ? action.sortingMap : KickStarter.sceneSettings.sortingMap;
				
				foreach (FollowSortingMap followSortingMap in followSortingMaps)
				{
					followSortingMap.SetSortingMap (sortingMap);
				}
			}
			
			return 0f;
		}
		
		
		public override void PlayIdle ()
		{
			PlayStandardAnim (character.idleAnimSprite, character.doDirections);
			PlaySeparateHead ();
		}
		
		
		public override void PlayWalk ()
		{
			PlayStandardAnim (character.walkAnimSprite, character.doDirections);
			PlaySeparateHead ();
		}
		
		
		public override void PlayRun ()
		{
			if (character.runAnimSprite != "")
			{
				PlayStandardAnim (character.runAnimSprite, character.doDirections);
			}
			else
			{
				PlayWalk ();
			}
			PlaySeparateHead ();
		}
		
		
		public override void PlayTalk ()
		{
			if (string.IsNullOrEmpty (character.talkAnimSprite))
			{
				PlayIdle ();
			}
			else if (character.talkingAnimation == TalkingAnimation.Standard && character.separateTalkingLayer)
			{
				PlayIdle ();
			}
			else if (character.LipSyncGameObject () && character.GetAnimator ())
			{
				PlayLipSync (false);
			}
			else
			{
				PlayStandardAnim (character.talkAnimSprite, character.doDirections);
			}
		}


		private void PlaySeparateHead ()
		{
			if (character.talkingAnimation == TalkingAnimation.Standard && character.separateTalkingLayer)
			{
				if (character.isTalking)
				{
					if (character.LipSyncGameObject () && character.GetAnimator ())
					{
						PlayLipSync (true);
					}
					else
					{
						PlayHeadAnim (character.talkAnimSprite, character.doDirections);
					}
				}
				else
				{
					PlayHeadAnim (character.idleAnimSprite, character.doDirections);
				}
			}
		}


		private void PlayLipSync (bool onlyHead)
		{
			string clip = character.talkAnimSprite;
			int layer = (onlyHead) ? character.headLayer : 0;
			if (character.doDirections)
			{
				if (layer > 0)
				{
					clip += headDirection;
				}
				else
				{
					clip += character.GetSpriteDirection ();
				}
			}
			character.GetAnimator ().speed = 0f;
			
			#if UNITY_EDITOR && (UNITY_5 || UNITY_2017_1_OR_NEWER)
			
			int hash = Animator.StringToHash (clip);
			if (character.GetAnimator ().HasState (layer, hash))
			{
				character.GetAnimator ().Play (hash, layer, character.GetLipSyncNormalised ());
			}
			else
			{
				ACDebug.LogError ("Cannot play clip " + clip + " (layer " + layer + ") on " + character.name);
			}
			
			#else
			
			try
			{
				character.GetAnimator ().Play (clip, layer, character.GetLipSyncNormalised ());
			}
			catch
			{}
			
			#endif
			
			character.GetAnimator ().speed = 1f;
		}


		public override void TurnHead (Vector2 angles)
		{
			if (character.lockDirection)
			{
				headDirection = character.GetSpriteDirection ();
			}
			else if (character.talkingAnimation == TalkingAnimation.Standard && character.separateTalkingLayer)
			{
				float spinAngleOffset = angles.x * Mathf.Rad2Deg;
				float headAngle = character.GetSpriteAngle () + spinAngleOffset;

				headDirection = "_" + CalcHeadDirection (headAngle);
			}
		}


		private string CalcHeadDirection (float headAngle)
		{
			if (character.doDiagonals)
			{
				if (headAngle > 22.5f && headAngle < 67.5f)
				{
					return "DL";
				}
				else if (headAngle > 112.5f && headAngle < 157.5f)
				{
					return "UL";
				}
				else if (headAngle > 202.5f && headAngle < 247.5f)
				{
					return "UR";
				}
				else if (headAngle > 292.5f && headAngle < 337.5f)
				{
					return "DR";
				}
			}

			if (headAngle > 135f && headAngle < 225f)
			{
				return "U";
			}
			else if (headAngle < 45f || headAngle > 315f)
			{
				return "D";
			}
			else if (headAngle > 180f)
			{
				return "R";
			}
			
			return "L";
		}
		
		
		private void PlayStandardAnim (string clip, bool includeDirection)
		{
			if (character && character.GetAnimator () && clip != "")
			{
				if (includeDirection)
				{
					clip += character.GetSpriteDirection ();
				}
				
				PlayCharAnim (clip, 0);
			}
		}


		private void PlayHeadAnim (string clip, bool includeDirection)
		{
			if (character && character.GetAnimator () && clip != "")
			{
				if (includeDirection)
				{
					clip += headDirection;
				}

				PlayCharAnim (clip, character.headLayer);
			}
		}


		private void PlayCharAnim (string clip, int layer)
		{
			#if UNITY_EDITOR && (UNITY_5 || UNITY_2017_1_OR_NEWER)

			int hash = Animator.StringToHash (clip);
			if (character.GetAnimator ().HasState (layer, hash))
			{
				if (character.crossfadeAnims)
				{
					character.GetAnimator ().CrossFade (hash, character.animCrossfadeSpeed, layer);
				}
				else
				{
					character.GetAnimator ().Play (hash, layer);
				}
			}
			else
			{
				ACDebug.LogWarning ("Cannot play animation clip " + clip + " (layer " + layer + ") on " + character.name);
			}
			
			#else
			
			if (character.crossfadeAnims)
			{
				try
				{
					character.GetAnimator ().CrossFade (clip, character.animCrossfadeSpeed, layer);
				}
				catch
				{}
			}
			else
			{
				try
				{
					character.GetAnimator ().Play (clip, layer);
				}
				catch
				{}
			}
			
			#endif
		}
		
	}
	
}