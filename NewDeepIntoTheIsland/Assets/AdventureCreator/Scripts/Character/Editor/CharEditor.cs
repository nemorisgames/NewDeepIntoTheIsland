using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace AC
{

	[CustomEditor (typeof (AC.Char))]
	public class CharEditor : Editor
	{

		private Expression expressionToAffect;


		public override void OnInspectorGUI ()
		{
			EditorGUILayout.HelpBox ("This component should not be used directly - use Player or NPC instead.", MessageType.Warning);
		}


		protected void SharedGUIOne (AC.Char _target)
		{
			_target.GetAnimEngine ();

			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Animation settings:", EditorStyles.boldLabel);
				_target.animationEngine = (AnimationEngine) EditorGUILayout.EnumPopup ("Animation engine:", _target.animationEngine);
				if (_target.animationEngine == AnimationEngine.Custom)
				{
					_target.customAnimationClass = EditorGUILayout.TextField ("Script name:", _target.customAnimationClass);
				}
				_target.motionControl = (MotionControl) EditorGUILayout.EnumPopup ("Motion control:", _target.motionControl);
			EditorGUILayout.EndVertical ();

			_target.GetAnimEngine ().CharSettingsGUI ();

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Movement settings:", EditorStyles.boldLabel);

			if (_target.GetMotionControl () == MotionControl.Automatic)
			{
	 			_target.walkSpeedScale = EditorGUILayout.FloatField ("Walk speed scale:", _target.walkSpeedScale);
				_target.runSpeedScale = EditorGUILayout.FloatField ("Run speed scale:", _target.runSpeedScale);
				_target.acceleration = EditorGUILayout.FloatField ("Acceleration:", _target.acceleration);
				_target.deceleration = EditorGUILayout.FloatField ("Deceleration:", _target.deceleration);
				_target.runDistanceThreshold = EditorGUILayout.FloatField ("Minimum run distance:", _target.runDistanceThreshold);
			}
			if (_target.GetMotionControl () != MotionControl.Manual)
			{
				_target.turnSpeed = EditorGUILayout.FloatField ("Turn speed:", _target.turnSpeed);

				if (_target.GetAnimEngine ().isSpriteBased)
				{
					_target.turn2DCharactersIn3DSpace = EditorGUILayout.Toggle ("Turn root object in 3D space?", _target.turn2DCharactersIn3DSpace);
				}
			}
			_target.turnBeforeWalking = EditorGUILayout.Toggle ("Turn before walking?", _target.turnBeforeWalking);
			_target.retroPathfinding = EditorGUILayout.Toggle ("Retro-style movement?", _target.retroPathfinding);

			_target.headTurnSpeed = EditorGUILayout.Slider ("Head turn speed:", _target.headTurnSpeed, 0.1f, 20f);
			if (_target is Player && AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.PlayerCanReverse ())
			{
				_target.reverseSpeedFactor = EditorGUILayout.Slider ("Reverse speed factor:", _target.reverseSpeedFactor, 0f, 1f);
			}

			EditorGUILayout.EndVertical ();
		}


		protected void SharedGUITwo (AC.Char _target)
		{
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Physics settings:", EditorStyles.boldLabel);
			_target.ignoreGravity = EditorGUILayout.Toggle ("Ignore gravity?", _target.ignoreGravity);
			if (_target.GetComponent <Rigidbody>() != null || _target.GetComponent <Rigidbody2D>() != null)
			{
				_target.freezeRigidbodyWhenIdle = EditorGUILayout.Toggle ("Freeze Rigidbody when Idle?", _target.freezeRigidbodyWhenIdle);
				if (_target.GetComponent <Rigidbody>() != null)
				{
					_target.useRigidbodyForMovement = EditorGUILayout.Toggle ("Move with Rigidbody?", _target.useRigidbodyForMovement);

					if (_target.useRigidbodyForMovement)
					{
						if (_target.GetAnimator () != null && _target.GetAnimator ().applyRootMotion)
						{
							EditorGUILayout.HelpBox ("Rigidbody movement will be disabled as 'Root motion' is enabled in the Animator.", MessageType.Info);
						}
						else if (_target.GetComponent <Rigidbody>().interpolation == RigidbodyInterpolation.None)
						{
							EditorGUILayout.HelpBox ("For smooth movement, the Rigidbody's 'Interpolation' should be set to either 'Interpolate' or 'Extrapolate'.", MessageType.Warning);
						}
					}
				}
				else if (_target.GetComponent <Rigidbody2D>() != null)
				{
					_target.useRigidbody2DForMovement = EditorGUILayout.Toggle ("Move with Rigidbody 2D?", _target.useRigidbody2DForMovement);

					if (_target.useRigidbody2DForMovement)
					{
						if (_target.GetAnimator () != null && _target.GetAnimator ().applyRootMotion)
						{
							EditorGUILayout.HelpBox ("Rigidbody movement will be disabled as 'Root motion' is enabled in the Animator.", MessageType.Info);
						}
						else if (_target.GetComponent <Rigidbody2D>().interpolation == RigidbodyInterpolation2D.None)
						{
							EditorGUILayout.HelpBox ("For smooth movement, the Rigidbody's 'Interpolation' should be set to either 'Interpolate' or 'Extrapolate'.", MessageType.Warning);
						}

						if (SceneSettings.CameraPerspective != CameraPerspective.TwoD)
						{
							EditorGUILayout.HelpBox ("Rigidbody2D-based motion only allows for X and Y movement, not Z, which may not be appropriate for 3D.", MessageType.Warning);
						}

						#if (UNITY_5_6_OR_NEWER || UNITY_2017_1_OR_NEWER)
						if (_target.GetAnimEngine ().isSpriteBased && _target.turn2DCharactersIn3DSpace)
						{
							EditorGUILayout.HelpBox ("For best results, 'Turn root object in 3D space?' above should be disabled.", MessageType.Warning);
						}
						#endif
					}
				}
			}

			if (_target.GetComponent <Collider>() != null && _target.GetComponent <CharacterController>() == null)
			{
				_target.groundCheckLayerMask = LayerMaskField ("Ground-check layer(s):", _target.groundCheckLayerMask);
			}
			EditorGUILayout.EndVertical ();
			
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Audio clips:", EditorStyles.boldLabel);
		
			_target.walkSound = (AudioClip) EditorGUILayout.ObjectField ("Walk sound:", _target.walkSound, typeof (AudioClip), false);
			_target.runSound = (AudioClip) EditorGUILayout.ObjectField ("Run sound:", _target.runSound, typeof (AudioClip), false);
			if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().speechManager != null && AdvGame.GetReferences ().speechManager.scrollSubtitles)
			{
				_target.textScrollClip = (AudioClip) EditorGUILayout.ObjectField ("Text scroll override:", _target.textScrollClip, typeof (AudioClip), false);
			}
			_target.soundChild = (Sound) EditorGUILayout.ObjectField ("SFX Sound child:", _target.soundChild, typeof (Sound), true);
			_target.speechAudioSource = (AudioSource) EditorGUILayout.ObjectField ("Speech AudioSource:", _target.speechAudioSource, typeof (AudioSource), true);
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Dialogue settings:", EditorStyles.boldLabel);

			_target.speechColor = EditorGUILayout.ColorField ("Speech text colour:", _target.speechColor);
			_target.speechLabel = EditorGUILayout.TextField ("Speaker label:", _target.speechLabel);
			_target.speechMenuPlacement = (Transform) EditorGUILayout.ObjectField ("Speech menu placement child:", _target.speechMenuPlacement, typeof (Transform), true);
			if (_target.useExpressions)
			{
				EditorGUILayout.LabelField ("Default portrait graphic:");
			}
			else
			{
				EditorGUILayout.LabelField ("Portrait graphic:");
			}
			_target.portraitIcon.ShowGUI (false);

			_target.useExpressions = EditorGUILayout.Toggle ("Use expressions?", _target.useExpressions);
			if (_target.useExpressions)
			{
				_target.GetAnimEngine ().CharExpressionsGUI ();

				EditorGUILayout.Space ();
				EditorGUILayout.BeginVertical ("Button");
				for (int i=0; i<_target.expressions.Count; i++)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Expression #" + _target.expressions[i].ID.ToString (), EditorStyles.boldLabel);
					if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
					{
						ExpressionSideMenu (_target, i);
					}
					EditorGUILayout.EndHorizontal ();
					_target.expressions[i].ShowGUI ();
				}

				if (GUILayout.Button ("Add new expression"))
				{
					_target.expressions.Add (new Expression (GetExpressionIDArray (_target.expressions)));
				}
				EditorGUILayout.EndVertical ();
			}

			EditorGUILayout.EndVertical ();
		}


		private int[] GetExpressionIDArray (List<Expression> expressions)
		{
			List<int> idArray = new List<int>();
			if (expressions != null)
			{
				foreach (Expression expression in expressions)
				{
					idArray.Add (expression.ID);
				}
			}
			idArray.Sort ();
			return idArray.ToArray ();
		}


		private void ExpressionSideMenu (AC.Char _target, int i)
		{
			expressionToAffect = _target.expressions[i];
			GenericMenu menu = new GenericMenu ();
			
			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");

			if (i > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, Callback, "Move up");
			}
			if (i < _target.expressions.Count-1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, Callback, "Move down");
			}
			
			menu.ShowAsContext ();
		}


		private void Callback (object obj)
		{
			AC.Char t = (AC.Char) target;
			ModifyAction (t, expressionToAffect, obj.ToString ());
			EditorUtility.SetDirty (t);
		}
		
		
		private void ModifyAction (AC.Char _target, Expression _expression, string callback)
		{
			int i = -1;
			if (_expression != null && _target.expressions.IndexOf (_expression) > -1)
			{
				i = _target.expressions.IndexOf (_expression);
			}
			
			switch (callback)
			{
			case "Insert after":
				Undo.RecordObject (_target, "Create expression");
				_target.expressions.Insert (i+1, new Expression (GetExpressionIDArray (_target.expressions)));
				break;
				
			case "Delete":
				Undo.RecordObject (_target, "Delete expression");
				_target.expressions.Remove (_expression);
				break;
				
			case "Move up":
				Undo.RecordObject (_target, "Move expression up");
				_target.expressions.Remove (_expression);
				_target.expressions.Insert (i-1, _expression);
				break;
				
			case "Move down":
				Undo.RecordObject (_target, "Move expression down");
				_target.expressions.Remove (_expression);
				_target.expressions.Insert (i+1, _expression);
				break;
			}
		}


 		private List<int> layerNumbers = new List<int>();
		private LayerMask LayerMaskField (string label, LayerMask layerMask)
		{
			var layers = InternalEditorUtility.layers;

			layerNumbers.Clear ();

			for (int i = 0; i < layers.Length; i++)
			layerNumbers.Add(LayerMask.NameToLayer(layers[i]));

			int maskWithoutEmpty = 0;
			for (int i = 0; i < layerNumbers.Count; i++)
			{
				if (((1 << layerNumbers[i]) & layerMask.value) > 0)
				{
					maskWithoutEmpty |= (1 << i);
				}
			}

			maskWithoutEmpty = UnityEditor.EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

			int mask = 0;
			for (int i = 0; i < layerNumbers.Count; i++)
			{
				if ((maskWithoutEmpty & (1 << i)) != 0)
				{
					mask |= (1 << layerNumbers[i]);
				}
			}
			layerMask.value = mask;

			return layerMask;
		}

	}

}