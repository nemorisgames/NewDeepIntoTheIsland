/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionDirector.cs"
 * 
 *	This action plays and stops controls Playable Directors
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_2017_1_OR_NEWER
using UnityEngine.Timeline;
using UnityEngine.Playables;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionTimeline : Action
	{

		public bool disableCamera;

		#if UNITY_2017_1_OR_NEWER
		public PlayableDirector director;
		public TimelineAsset newTimeline;
		public int directorConstantID = 0;
		public int directorParameterID = -1;

		public enum ActionDirectorMethod { Play, Stop };
		public ActionDirectorMethod method = ActionDirectorMethod.Play;
		public bool restart = true;
		public bool pause = false;
		public bool updateBindings = false;
		[SerializeField] private BindingData[] newBindings = new BindingData[0];
		#endif

		
		public ActionTimeline ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Control Timeline";
			description = "Controls a Timeline.  This is only compatible with Unity 2017 or newer.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			#if UNITY_2017_1_OR_NEWER
			director = AssignFile <PlayableDirector> (parameters, directorParameterID, directorConstantID, director);

			if (newBindings != null)
			{
				for (int i=0; i<newBindings.Length; i++)
				{
					newBindings[i].gameObject = AssignFile (parameters, newBindings[i].parameterID, newBindings[i].constantID, newBindings[i].gameObject);
				}
			}
			#endif
		}
		
		
		override public float Run ()
		{
			#if UNITY_2017_1_OR_NEWER
			if (!isRunning)
			{
				isRunning = true;

				if (director != null)
				{
					if (method == ActionDirectorMethod.Play)
					{
						isRunning = true;

						if (restart)
						{
							PrepareDirector ();

							director.time = 0f;
							director.Play ();
						}
						else
						{
							director.Resume ();
						}


						if (willWait)
						{
							if (disableCamera)
							{
								KickStarter.mainCamera.Disable ();
							}
							return ((float) director.duration - (float) director.time);
						}
					}
					else if (method == ActionDirectorMethod.Stop)
					{
						if (disableCamera)
						{
							KickStarter.mainCamera.Enable ();
						}

						if (pause)
						{
							director.Pause ();
						}
						else
						{
							director.time = director.duration;
							director.Stop ();
						}
					}
				}
			}
			else
			{
				if (method == ActionDirectorMethod.Play && disableCamera)
				{
					KickStarter.mainCamera.Enable ();
				}

				isRunning = false;
			}
			#endif
			
			return 0f;
		}


		override public void Skip ()
		{
			#if UNITY_2017_1_OR_NEWER
			if (director != null)
			{
				if (disableCamera)
				{
					KickStarter.mainCamera.Enable ();
				}

				if (method == ActionDirectorMethod.Play)
				{
					if (director.extrapolationMode == DirectorWrapMode.Loop)
					{
						PrepareDirector ();

						if (restart)
						{
							director.Play ();
						}
						else
						{
							director.Resume ();
						}
						return;
					}

					director.Stop ();
					director.time = director.duration;
				}
				else if (method == ActionDirectorMethod.Stop)
				{
					if (pause)
					{
						director.Pause ();
					}
					else
					{
						director.Stop ();
					}
				}
			}
			#endif
		}

		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			#if UNITY_2017_1_OR_NEWER
			method = (ActionDirectorMethod) EditorGUILayout.EnumPopup ("Method:", method);

			directorParameterID = Action.ChooseParameterGUI ("Director:", parameters, directorParameterID, ParameterType.GameObject);
			if (directorParameterID >= 0)
			{
				directorConstantID = 0;
				director = null;
			}
			else
			{
				director = (PlayableDirector) EditorGUILayout.ObjectField ("Director:", director, typeof (PlayableDirector), true);
				
				directorConstantID = FieldToID <PlayableDirector> (director, directorConstantID);
				director = IDToField <PlayableDirector> (director, directorConstantID, false);
			}

			if (method == ActionDirectorMethod.Play)
			{
				restart = EditorGUILayout.Toggle ("Play from beginning?", restart);
				if (restart)
				{
					newTimeline = (TimelineAsset) EditorGUILayout.ObjectField ("Timeline (optional):", newTimeline, typeof (TimelineAsset), false);
					EditorGUILayout.BeginVertical (CustomStyles.thinBox);
					updateBindings = EditorGUILayout.Toggle ("Remap bindings?", updateBindings);
					if (updateBindings)
					{
						if (newTimeline)
						{
							ShowBindingsUI (newTimeline, parameters);
						}
						else if (director != null && director.playableAsset != null)
						{
							ShowBindingsUI (director.playableAsset as TimelineAsset, parameters);
						}
						else
						{
							EditorGUILayout.HelpBox ("A Director or Timeline must be assigned in order to update bindings.", MessageType.Warning);
						}
					}
					else if (newTimeline != null)
					{
						EditorGUILayout.HelpBox ("The existing bindings will be transferred onto the new Timeline.", MessageType.Info);
					}
					EditorGUILayout.EndVertical ();
				}
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);

				if (willWait)
				{
					disableCamera = EditorGUILayout.Toggle ("Disable AC camera?", disableCamera);
				}
			}
			else if (method == ActionDirectorMethod.Stop)
			{
				pause = EditorGUILayout.Toggle ("Pause timeline?", pause);
				disableCamera = EditorGUILayout.Toggle ("Enable AC camera?", disableCamera);
			}

			#else
			EditorGUILayout.HelpBox ("This Action is only compatible with Unity 5.6 or newer.", MessageType.Info);
			#endif

			AfterRunningOption ();
		}


		#if UNITY_2017_1_OR_NEWER
		private void ShowBindingsUI (TimelineAsset timelineAsset, List<ActionParameter> parameters)
		{
			if (timelineAsset == null) return;

			if (newBindings == null || timelineAsset.outputTrackCount != newBindings.Length)
			{
				BindingData[] tempBindings = new BindingData[newBindings.Length];
				for (int i=0; i<newBindings.Length; i++)
				{
					tempBindings[i] = new BindingData (newBindings[i]);
				}

				newBindings = new BindingData[timelineAsset.outputTrackCount];
				for (int i=0; i<newBindings.Length; i++)
				{
					if (i < tempBindings.Length)
					{
						newBindings[i] = new BindingData (tempBindings[i]);
					}
					else
					{
						newBindings[i] = new BindingData ();
					}
				}
			}

			for (int i=0; i<newBindings.Length; i++)
			{
				newBindings[i].parameterID = Action.ChooseParameterGUI ("Track #" + i + ":", parameters, newBindings[i].parameterID, ParameterType.GameObject);
				if (newBindings[i].parameterID >= 0)
				{
					newBindings[i].constantID = 0;
					newBindings[i].gameObject = null;
				}
				else
				{
					newBindings[i].gameObject = (GameObject) EditorGUILayout.ObjectField ("Track #" + i + ":", newBindings[i].gameObject, typeof (GameObject), true);

					newBindings[i].constantID = FieldToID (newBindings[i].gameObject, newBindings[i].constantID);
					newBindings[i].gameObject = IDToField (newBindings[i].gameObject, newBindings[i].constantID, false);
				}
			}
		}
		#endif


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			#if UNITY_2017_1_OR_NEWER
			if (saveScriptsToo)
			{
				AddSaveScript <RememberTimeline> (director);
			}
			AssignConstantID <PlayableDirector> (director, directorConstantID, directorParameterID);

			if (updateBindings && newBindings != null && newBindings.Length > 0)
			{
				for (int i=0; i<newBindings.Length; i++)
				{
					if (newBindings[i].gameObject != null)
					{
						if (saveScriptsToo)
						{
							AddSaveScript <ConstantID> (newBindings[i].gameObject);
						}
						AssignConstantID (newBindings[i].gameObject, newBindings[i].constantID, newBindings[i].parameterID);
					}
				}
			}
			#endif
		}

		
		public override string SetLabel ()
		{
			#if UNITY_2017_1_OR_NEWER
			if (director != null)
			{
				return " (" + method.ToString () + " " + director.gameObject.name + ")";
			}
			#endif
			return "";
		}
		
		#endif


		#if UNITY_2017_1_OR_NEWER

		private void PrepareDirector ()
		{
			if (newTimeline != null)
			{
				if (director.playableAsset != null && director.playableAsset is TimelineAsset)
				{
					TimelineAsset oldTimeline = (TimelineAsset) director.playableAsset;
					GameObject[] transferBindings = new GameObject[oldTimeline.outputTrackCount];
					for (int i=0; i<transferBindings.Length; i++)
					{
						TrackAsset trackAsset = oldTimeline.GetOutputTrack (i);
						transferBindings[i] = director.GetGenericBinding (trackAsset) as GameObject;
					}

					director.playableAsset = newTimeline;

					for (int i=0; i<transferBindings.Length; i++)
					{
						if (transferBindings[i] != null)
						{
							var track = newTimeline.GetOutputTrack (i);
							if (track != null)
							{
								director.SetGenericBinding (track, transferBindings[i].gameObject);
							}
		                }
					}
				}
				else
				{
					director.playableAsset = newTimeline;
				}
			}

			if (updateBindings && newBindings != null)
			{
				TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
				if (timelineAsset != null)
				{
					for (var i=0; i<newBindings.Length; i++)
					{
						if (newBindings[i] != null)
						{
							var track = timelineAsset.GetOutputTrack (i);
							if (track != null)
							{
								director.SetGenericBinding (track, newBindings[i].gameObject);
							}
		                }
		            }
				}
			}
		}


		[System.Serializable]
		private class BindingData
		{

			public GameObject gameObject;
			public int constantID;
			public int parameterID = -1;


			public BindingData ()
			{
				gameObject = null;
				constantID = 0;
				parameterID = -1;
			}


			public BindingData (BindingData bindingData)
			{
				gameObject = bindingData.gameObject;
				constantID = bindingData.constantID;
				parameterID = bindingData.parameterID;
			}

		}

		#endif

	}
	
}