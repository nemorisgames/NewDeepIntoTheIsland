using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AC
{

	public class ActionListEditorWindow : EditorWindow
	{

		public ActionListEditorWindowData windowData = new ActionListEditorWindowData ();

		private bool isMarquee = false;
		private Rect marqueeRect = new Rect (0f, 0f, 0f, 0f);
		private bool canMarquee = true;
		private bool marqueeShift = false;
		private bool isAutoArranging = false;
		private bool showProperties = false;
		private int focusActionIndex;

		private float zoom = 1f;
		private float zoomMin = 0.2f;
		private float zoomMax = 1f;
		
		private Action actionChanging = null;
		private bool resultType;
		private int multipleResultType;
		private int offsetChanging = 0;
		private int numActions = 0;

		private Vector2 scrollPosition = Vector2.zero;
		private Vector2 maxScroll;
		private Vector2 menuPosition;
		
		private ActionsManager actionsManager;

		
		[MenuItem ("Adventure Creator/Editors/ActionList Editor", false, 1)]
		private static void Init ()
		{
			ActionListEditorWindow window = CreateWindow ();
			window.Repaint ();
			window.Show ();
			UnityVersionHandler.SetWindowTitle (window, "ActionList Editor");
			window.windowData = new ActionListEditorWindowData ();
		}


		private static ActionListEditorWindow CreateWindow ()
		{
			if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().actionsManager != null && AdvGame.GetReferences ().actionsManager.allowMultipleActionListWindows == false)
			{
				return (ActionListEditorWindow) EditorWindow.GetWindow (typeof (ActionListEditorWindow));
			}
			else
			{
				return CreateInstance <ActionListEditorWindow>();
			}
		}


		public static void Init (ActionList actionList)
		{
			if (actionList.source == ActionListSource.AssetFile)
			{
				if (actionList.assetFile != null)
				{
					ActionListEditorWindow.Init (actionList.assetFile);
				}
				else
				{
					ACDebug.Log ("Cannot open ActionList Editor window, as no ActionList asset file has been assigned to " + actionList.gameObject.name + ".");
				}
			}
			else
			{
				ActionListEditorWindow window = CreateWindow ();
				window.AssignNewSource (new ActionListEditorWindowData (actionList));
			}
		}


		public static void Init (ActionListAsset actionListAsset)
		{
			ActionListEditorWindow window = CreateWindow ();
			ActionListEditorWindowData windowData = new ActionListEditorWindowData (actionListAsset);
			window.AssignNewSource (windowData);
		}


		public void AssignNewSource (ActionListEditorWindowData _data)
		{
			scrollPosition = Vector2.zero;
			zoom = 1f;
			showProperties = false;
			UnityVersionHandler.SetWindowTitle (this, "ActionList Editor");
			windowData = _data;
			Repaint ();
			Show ();
		}


		private void OnEnable ()
		{
			if (AdvGame.GetReferences ())
			{
				if (AdvGame.GetReferences ().actionsManager)
				{
					actionsManager = AdvGame.GetReferences ().actionsManager;
					AdventureCreator.RefreshActions ();
				}
				else
				{
					ACDebug.LogError ("An Actions Manager is required - please use the Game Editor window to create one.");
				}
			}
			else
			{
				ACDebug.LogError ("A References file is required - please use the Game Editor window to create one.");
			}
			
			UnmarkAll (windowData.targetAsset != null);
		}

		
		private void PanAndZoomWindow ()
		{
			if (actionChanging)
			{
				return;
			}

			ActionListEditorScrollWheel scrollWheel = ActionListEditorScrollWheel.PansWindow;
			bool invertPanning = false;
			float speedFactor = 1f;
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().actionsManager)
			{
				scrollWheel = AdvGame.GetReferences ().actionsManager.actionListEditorScrollWheel;
				invertPanning = AdvGame.GetReferences ().actionsManager.invertPanning;
				speedFactor = AdvGame.GetReferences ().actionsManager.panSpeed;
			}
			invertPanning = !invertPanning;

			if (scrollWheel == ActionListEditorScrollWheel.ZoomsWindow && Event.current.type == EventType.ScrollWheel)
			{
				Vector2 screenCoordsMousePos = Event.current.mousePosition;
				Vector2 delta = Event.current.delta * speedFactor;
				float zoomDelta = -delta.y / 80.0f;
				float oldZoom = zoom;
				zoom += zoomDelta;
				zoom = Mathf.Clamp (zoom, zoomMin, zoomMax);
				scrollPosition += (screenCoordsMousePos - scrollPosition) - (oldZoom / zoom) * (screenCoordsMousePos - scrollPosition);

				Event.current.Use();
			}

			if (scrollWheel == ActionListEditorScrollWheel.PansWindow && Event.current.type == EventType.ScrollWheel)
			{
				Vector2 delta = Event.current.delta * speedFactor * 8f;
				if (invertPanning)
				{
					scrollPosition += delta;
				}
				else
				{
					scrollPosition -= delta;
				}
				Event.current.Use ();
			}
			else if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
			{
				Vector2 delta = Event.current.delta * speedFactor * 1.25f;
				if (invertPanning)
				{
					scrollPosition += delta;
				}
				else
				{
					scrollPosition -= delta;
				}
				
				Event.current.Use ();
			}
		}
		
		
		private void DrawMarquee (bool isAsset)
		{
			if (actionChanging)
			{
				return;
			}
			
			if (!canMarquee)
			{
				isMarquee = false;
				return;
			}

			Event e = Event.current;
			
			if (e.type == EventType.MouseDown && e.button == 0 && !isMarquee)
			{
				if (e.mousePosition.y > 24)
				{
					isMarquee = true;
					marqueeShift = false;
					marqueeRect = new Rect (e.mousePosition.x, e.mousePosition.y, 0f, 0f);
				}
			}
			else if (e.rawType == EventType.MouseUp)
			{
				if (isMarquee)
				{
					MarqueeSelect (isAsset, marqueeShift);
				}
				isMarquee = false;
			}
			if (isMarquee && e.shift)
			{
				marqueeShift = true;
			}

			if (isMarquee)
			{
				marqueeRect.width = e.mousePosition.x - marqueeRect.x;
				marqueeRect.height = e.mousePosition.y - marqueeRect.y;
				GUI.Label (marqueeRect, "", Resource.NodeSkin.customStyles[9]);
			}
		}
		
		
		private void MarqueeSelect (bool isAsset, bool isCumulative)
		{
			List<Action> actionList = new List<Action>();
			actionList = (isAsset) ? windowData.targetAsset.actions : windowData.target.actions;
			
			if (marqueeRect.width < 0f)
			{
				marqueeRect.x += marqueeRect.width;
				marqueeRect.width *= -1f;
			}
			if (marqueeRect.height < 0f)
			{
				marqueeRect.y += marqueeRect.height;
				marqueeRect.height *= -1f;
			}
			
			// Correct for zooming
			marqueeRect.x /= zoom;
			marqueeRect.y /= zoom;
			marqueeRect.width /= zoom;
			marqueeRect.height /= zoom;
			
			// Correct for panning
			marqueeRect.x += scrollPosition.x;
			marqueeRect.y += scrollPosition.y;

			marqueeRect.y -= 18f;

			if (!isCumulative)
			{
				UnmarkAll (isAsset);
			}

			foreach (Action action in actionList)
			{
				if (IsRectInRect (action.nodeRect, marqueeRect) || IsRectInRect (marqueeRect, action.nodeRect))
				{
					action.isMarked = true;
				}
			}
		}


		private bool IsRectInRect (Rect rect1, Rect rect2)
		{
			if (rect1.Contains (rect2.BottomRight ()) || rect1.Contains (rect2.BottomLeft ()) || rect1.Contains (rect2.TopLeft ()) || rect1.Contains (rect2.TopRight ()))
			{
				return true;
			}
			return false;
		}

		
		private void OnGUI ()
		{
			if (isAutoArranging)
			{
				return;
			}

			if (!windowData.isLocked)
			{
				if (Selection.activeObject && Selection.activeObject is ActionListAsset)
				{
					windowData.targetAsset = (ActionListAsset) Selection.activeObject;
					windowData.target = null;
				}
				else if (Selection.activeGameObject && Selection.activeGameObject.GetComponent <ActionList>())
				{
					windowData.targetAsset = null;
					windowData.target = Selection.activeGameObject.GetComponent<ActionList>();
				}
			}

			if (windowData.targetAsset != null)
			{
				ActionListAssetEditor.ResetList (windowData.targetAsset);
				
				if (showProperties)
				{
					PropertiesGUI (true);
				}
				else
				{
					PanAndZoomWindow ();
					NodesGUI (true);
					DrawMarquee (true);
				}
				TopToolbarGUI (true);
				
				if (GUI.changed)
				{
					EditorUtility.SetDirty (windowData.targetAsset);
				}
			}
			else if (windowData.target != null)
			{
				ActionListEditor.ResetList (windowData.target);

				if (showProperties)
				{
					PropertiesGUI (false);
				}
				else if (windowData.target.source != ActionListSource.AssetFile)
				{
					PanAndZoomWindow ();
					NodesGUI (false);
					DrawMarquee (false);
				}
				TopToolbarGUI (false);

				if (GUI.changed)
				{
					UnityVersionHandler.CustomSetDirty (windowData.target);
				}
			}
			else
			{
				TopToolbarGUI (false);
			}
		}


		private void TopToolbarGUI (bool isAsset)
		{
			bool noList = false;
			bool showLabel = false;
			float buttonWidth = 20f;
			if (position.width > 480)
			{
				buttonWidth = 60f;
				showLabel = true;
			}

			if ((isAsset && windowData.targetAsset == null) || (!isAsset && windowData.target == null) || (!isAsset && !windowData.target.gameObject.activeInHierarchy))
			{
				noList = true;
			}

			GUILayout.BeginArea (new Rect (0, position.height - 20, position.width, 20), Resource.NodeSkin.box);
			string labelText;
			if (noList)
			{
				labelText = "No ActionList selected";
			}
			else if (isAsset)
			{
				labelText = "Editing ActionList asset: " + windowData.targetAsset.name;
			}
			else
			{
				labelText = "Editing " + windowData.target.GetType ().ToString ().Replace ("AC.", "") + ": " + windowData.target.gameObject.name;
			}

			int iconNumber = 11;
			if (!windowData.isLocked)
			{
				iconNumber = 12;
			}
			if (GUI.Button (new Rect (10, 0, 18, 18), "", Resource.NodeSkin.customStyles [iconNumber]))
			{
				windowData.isLocked = !windowData.isLocked;
			}

			GUI.Label (new Rect (30,2,50,20), labelText, Resource.NodeSkin.customStyles[8]);
			if ((isAsset && windowData.targetAsset != null) || (!isAsset && windowData.target != null))
			{
				if (GUI.Button (new Rect (position.width - 240, 0, 120, 20), "Ping object", EditorStyles.miniButtonLeft))
				{
					if (windowData.targetAsset != null)
					{
						EditorGUIUtility.PingObject (windowData.targetAsset);
					}
					else if (windowData.target != null)
					{
						EditorGUIUtility.PingObject (windowData.target.gameObject);
					}
				}

				string propertiesLabel = "Show ";
				if (showProperties) propertiesLabel += "Actions"; else propertiesLabel += "properties";
				if (GUI.Button (new Rect (position.width - 120, 0, 120, 20), propertiesLabel, EditorStyles.miniButtonRight))
				{
					showProperties = !showProperties;
				}
			}

			GUILayout.EndArea ();

			GUILayout.BeginArea (new Rect (0,0,position.width,24), Resource.NodeSkin.box);

			float midX = position.width * 0.4f;

			if (noList)
			{
				GUI.enabled = false;
			}

			if (ToolbarButton (10f, buttonWidth, showLabel, "Insert", 7))
			{
				menuPosition = new Vector2 (70f, 30f) + scrollPosition;
				PerformEmptyCallBack ("Add new Action");
			}

			if (!noList && NumActionsMarked (isAsset) > 0)
			{
				GUI.enabled = true;
			}
			else
			{
				GUI.enabled = false;
			}
			
			if (ToolbarButton (buttonWidth+10f, buttonWidth, showLabel, "Delete", 5))
			{
				PerformEmptyCallBack ("Delete selected");
			}

			if (!noList)
			{
				GUI.enabled = true;
			}

			if (ToolbarButton (position.width-(buttonWidth*3f), buttonWidth*1.5f, showLabel, "Auto-arrange", 6))
			{
				//AutoArrange (isAsset);
				PerformEmptyCallBack ("Auto-arrange");
			}

			GUI.enabled = (noList) ? false : Application.isPlaying;

			if (ToolbarButton (position.width-buttonWidth, buttonWidth, showLabel, "Run", 4))
			{
				if (isAsset)
				{
					AdvGame.RunActionListAsset (windowData.targetAsset);
				}
				else
				{
					windowData.target.Interact ();
				}
			}

			if (!noList && NumActionsMarked (isAsset) > 0 && !Application.isPlaying)
			{
				GUI.enabled = true;
			}
			else
			{
				GUI.enabled = false;
			}

			if (ToolbarButton (midX - buttonWidth + 10f, buttonWidth, showLabel, "Cut", 3))
			{
				PerformEmptyCallBack ("Cut selected");
			}

			if (ToolbarButton (midX, buttonWidth, showLabel, "Copy", 1))
			{
				PerformEmptyCallBack ("Copy selected");
			}

			if (!noList && AdvGame.copiedActions != null && AdvGame.copiedActions.Count > 0)
			{
				GUI.enabled = true;
			}
			else
			{
				GUI.enabled = false;
			}

			if (ToolbarButton (midX + buttonWidth, buttonWidth, showLabel, "Paste", 2))
			{
				menuPosition = new Vector2 (70f, 30f) + scrollPosition;
				EmptyCallback ("Paste copied Action(s)");
			}

			GUI.enabled = true;

			GUILayout.EndArea ();
		}


		private void PropertiesGUI (bool isAsset)
		{
			EditorZoomArea.Begin (1, new Rect (0, 0, position.width, position.height - 24));
			GUILayout.BeginArea (new Rect (0, 24, 350, position.height - 48));
			if (isAsset && windowData.targetAsset != null)
			{
				ActionListAssetEditor.ShowPropertiesGUI (windowData.targetAsset);
			}
			else if (!isAsset && windowData.target != null)
			{
				if (windowData.target is Cutscene)
				{
					Cutscene cutscene = (Cutscene) windowData.target;
					CutsceneEditor.PropertiesGUI (cutscene);
				}
				else if (windowData.target is AC_Trigger)
				{
					AC_Trigger cutscene = (AC_Trigger) windowData.target;
					AC_TriggerEditor.PropertiesGUI (cutscene);
				}
				else if (windowData.target is DialogueOption)
				{
					DialogueOption cutscene = (DialogueOption) windowData.target;
					DialogueOptionEditor.PropertiesGUI (cutscene);
				}
				else if (windowData.target is Interaction)
				{
					Interaction cutscene = (Interaction) windowData.target;
					InteractionEditor.PropertiesGUI (cutscene);
				}
			}
			GUILayout.EndArea ();
			EditorZoomArea.End ();
		}


		private bool ToolbarButton (float startX, float width, bool showLabel, string label, int styleIndex)
		{
			if (showLabel)
			{
				return GUI.Button (new Rect (startX,2,width,20), label, Resource.NodeSkin.customStyles[styleIndex]);
			}
			return GUI.Button (new Rect (startX,2,20,20), "", Resource.NodeSkin.customStyles[styleIndex]);
		}
		
		
		private void OnInspectorUpdate ()
		{
			Repaint ();
		}


		private void NodeWindow (int i)
		{
			if (actionsManager == null)
			{
				OnEnable ();
			}
			if (actionsManager == null)
			{
				return;
			}
			
			bool isAsset;
			Action _action;
			List<ActionParameter> parameters = null;
			
			if (windowData.targetAsset != null)
			{
				_action = windowData.targetAsset.actions[i];
				isAsset = _action.isAssetFile = true;
				if (windowData.targetAsset.useParameters)
				{
					parameters = windowData.targetAsset.parameters;
				}
			}
			else
			{
				_action = windowData.target.actions[i];
				isAsset = _action.isAssetFile = false;
				if (windowData.target.useParameters)
				{
					parameters = windowData.target.parameters;
				}
			}

			if (_action.showComment)
			{
				Color _color = GUI.color;
				GUI.color = new Color (1f, 1f, 0.5f, 1f);
				EditorStyles.textField.wordWrap = true;
				_action.comment = EditorGUILayout.TextArea (_action.comment, GUILayout.MaxWidth (280f));
				GUI.color = _color;
				EditorGUILayout.Space ();
			}

			if (!actionsManager.DoesActionExist (_action.GetType ().ToString ()))
			{
				EditorGUILayout.HelpBox ("This Action type has been disabled in the Actions Manager", MessageType.Warning);
			}
			else
			{
				int typeIndex = KickStarter.actionsManager.GetActionTypeIndex (_action);
				int newTypeIndex = ActionListEditor.ShowTypePopup (_action, typeIndex);

				if (newTypeIndex >= 0)
				{
					// Rebuild constructor if Subclass and type string do not match
					Vector2 currentPosition = new Vector2 (_action.nodeRect.x, _action.nodeRect.y);
					
					// Store "After running data" to transfer over
					ActionEnd _end = new ActionEnd ();
					_end.resultAction = _action.endAction;
					_end.skipAction = _action.skipAction;
					_end.linkedAsset = _action.linkedAsset;
					_end.linkedCutscene = _action.linkedCutscene;
					
					if (isAsset)
					{
						Undo.RecordObject (windowData.targetAsset, "Change Action type");
						
						Action newAction = ActionListAssetEditor.RebuildAction (_action, newTypeIndex, windowData.targetAsset, _end.resultAction, _end.skipAction, _end.linkedAsset, _end.linkedCutscene);
						newAction.nodeRect.x = currentPosition.x;
						newAction.nodeRect.y = currentPosition.y;
						
						windowData.targetAsset.actions.Remove (_action);
						windowData.targetAsset.actions.Insert (i, newAction);
					}
					else
					{
						Undo.RecordObject (windowData.target, "Change Action type");
						
						Action newAction = ActionListEditor.RebuildAction (_action, newTypeIndex, _end.resultAction, _end.skipAction, _end.linkedAsset, _end.linkedCutscene);
						newAction.nodeRect.x = currentPosition.x;
						newAction.nodeRect.y = currentPosition.y;
						
						windowData.target.actions.Remove (_action);
						windowData.target.actions.Insert (i, newAction);
					}
				}

				_action.ShowGUI (parameters);
			}
			
			if (_action.endAction == ResultAction.Skip || _action.numSockets == 2 || _action is ActionCheckMultiple || _action is ActionParallel)
			{
				if (isAsset)
				{
					_action.SkipActionGUI (windowData.targetAsset.actions, true);
				}
				else
				{
					_action.SkipActionGUI (windowData.target.actions, true);
				}
			}

			_action.isDisplayed = EditorGUI.Foldout (new Rect (10,1,20,16), _action.isDisplayed, "");
			
			if (GUI.Button (new Rect(273,3,16,16), " ", Resource.NodeSkin.customStyles[0]))
			{
				CreateNodeMenu (isAsset, i, _action);
			}
			
			if (i == 0)
			{
				_action.nodeRect.x = 14;
				_action.nodeRect.y = 14;
			}
			else
			{
				if (Event.current.button == 0)
				{
					GUI.DragWindow ();
				}
			}
		}
		
		
		private void EmptyNodeWindow (int i)
		{
			Action _action;
			bool isAsset = false;
			
			if (windowData.targetAsset != null)
			{
				_action = windowData.targetAsset.actions[i];
				isAsset = true;
			}
			else
			{
				_action = windowData.target.actions[i];
			}

			if (_action.endAction == ResultAction.Skip || _action.numSockets == 2 || _action is ActionCheckMultiple || _action is ActionParallel)
			{
				if (isAsset)
				{
					_action.SkipActionGUI (windowData.targetAsset.actions, false);
				}
				else
				{
					_action.SkipActionGUI (windowData.target.actions, false);
				}
			}
			
			_action.isDisplayed = EditorGUI.Foldout (new Rect (10,1,20,16), _action.isDisplayed, "");

			if (_action.showComment)
			{
				Color _color = GUI.color;
				GUI.color = new Color (1f, 1f, 0.5f, 1f);
				EditorStyles.textField.wordWrap = true;
				_action.comment = EditorGUILayout.TextArea (_action.comment, GUILayout.MaxWidth (280f));
				GUI.color = _color;
			}

			if (GUI.Button (new Rect(273,3,16,16), " ", Resource.NodeSkin.customStyles[0]))
			{
				CreateNodeMenu (isAsset, i, _action);
			}

			if (i == 0)
			{
				_action.nodeRect.x = 14;
				_action.nodeRect.y = 14;
			}
			else
			{
				if (Event.current.button == 0)
				{
					GUI.DragWindow ();
				}
			}
		}


		private bool IsActionInView (Action action)
		{
			float height = action.nodeRect.height;

			if (isAutoArranging || action.isMarked)
			{
				return true;
			}
			if (action.nodeRect.y > scrollPosition.y + position.height / zoom)
			{
				return false;
			}
			if (action.nodeRect.y + height < scrollPosition.y)
			{
				return false;
			}
			if (action.nodeRect.x > scrollPosition.x + position.width / zoom)
			{
				return false;
			}
			if (action.nodeRect.x + action.nodeRect.width < scrollPosition.x)
			{
				return false;
			}
			return true;
		}
		
		
		private void LimitWindow (Action action)
		{
			if (action.nodeRect.x < 1)
			{
				action.nodeRect.x = 1;
			}
			
			if (action.nodeRect.y < 14)
			{
				action.nodeRect.y = 14;
			}
		}
		
		
		private void NodesGUI (bool isAsset)
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().actionsManager)
			{
				actionsManager = AdvGame.GetReferences ().actionsManager;
			}
			if (actionsManager == null)
			{
				GUILayout.Space (30f);
				EditorGUILayout.HelpBox ("An Actions Manager asset file must be assigned in the Game Editor Window", MessageType.Warning);
				OnEnable ();
				return;
			}
			if (!isAsset && PrefabUtility.GetPrefabType (windowData.target) == PrefabType.Prefab)
			{
				GUILayout.Space (30f);
				EditorGUILayout.HelpBox ("Scene-based Actions can not live in prefabs - use ActionList assets instead.", MessageType.Info);
				return;
			}
			if (!isAsset && windowData.target != null)
			{
				if (windowData.target.source == ActionListSource.AssetFile)
				{
					GUILayout.Space (30f);
					EditorGUILayout.HelpBox ("Cannot view Actions, since this object references an Asset file.", MessageType.Info);
					return;
				}
			}

			bool loseConnection = false;
			Event e = Event.current;

			if (e.isMouse && actionChanging != null)
			{
				if (e.type == EventType.MouseUp)
				{
					loseConnection = true;
				}
				else if (e.mousePosition.x < 0f || e.mousePosition.x > position.width || e.mousePosition.y < 0f || e.mousePosition.y > position.height)
				{
					loseConnection = true;
					actionChanging = null;
				}
			}
			
			if (isAsset)
			{
				numActions = windowData.targetAsset.actions.Count;
				if (numActions < 1)
				{
					numActions = 1;
					AC.Action newAction = ActionList.GetDefaultAction ();
					newAction.hideFlags = HideFlags.HideInHierarchy;
					windowData.targetAsset.actions.Add (newAction);
					AssetDatabase.AddObjectToAsset (newAction, windowData.targetAsset);
					AssetDatabase.SaveAssets ();
				}
				numActions = windowData.targetAsset.actions.Count;
			}
			else
			{
				numActions = windowData.target.actions.Count;
				if (numActions < 1)
				{
					numActions = 1;
					AC.Action newAction = ActionList.GetDefaultAction ();
					windowData.target.actions.Add (newAction);
				}
				numActions = windowData.target.actions.Count;
			}

			EditorZoomArea.Begin (zoom, new Rect (0, 0, position.width / zoom, position.height / zoom - 24));
			scrollPosition = GUI.BeginScrollView (new Rect (0, 24, position.width / zoom, position.height / zoom - 48), scrollPosition, new Rect (0, 0, maxScroll.x, maxScroll.y), false, false);
			
			BeginWindows ();
			
			canMarquee = true;
			Vector2 newMaxScroll = Vector2.zero;
			for (int i=0; i<numActions; i++)
			{
				FixConnections (i, isAsset);
				
				Action _action;
				if (isAsset)
				{
					_action = windowData.targetAsset.actions[i];
				}
				else
				{
					_action = windowData.target.actions[i];
				}

				if (_action == null) continue;

				if (i == 0)
				{
					GUI.Label (new Rect (16, -2, 100, 20), "START", Resource.NodeSkin.label);

					if (Mathf.Approximately (_action.nodeRect.x, 50) && Mathf.Approximately (_action.nodeRect.y, 50))
					{
						// Upgrade
						_action.nodeRect.x = _action.nodeRect.y = 14;
						MarkAll (isAsset);
						PerformEmptyCallBack ("Expand selected");
						UnmarkAll (isAsset);
					}
				}

				Vector2 originalPosition = new Vector2 (_action.nodeRect.x, _action.nodeRect.y);

				if (IsActionInView (_action))
				{
					bool isProSkin = EditorGUIUtility.isProSkin;
					GUIStyle nodeStyle = (isProSkin) ? Resource.NodeSkin.GetStyle ("Window") : Resource.NodeSkin.customStyles [19];

					Color originalColor = GUI.color;
					GUI.color = actionsManager.EnabledActions[actionsManager.GetActionTypeIndex (_action)].color;

					if (_action.isRunning && Application.isPlaying)
					{
						// Running
						nodeStyle = (isProSkin) ? Resource.NodeSkin.customStyles [16] : Resource.NodeSkin.customStyles [21];
					}
					else if (_action.isMarked ||
							(actionChanging != null && _action.nodeRect.Contains (e.mousePosition)))
					{
						// Selected
						nodeStyle = (isProSkin) ? Resource.NodeSkin.customStyles [15] : Resource.NodeSkin.customStyles [20];
					}
					else if (_action.isBreakPoint)
					{
						//  Breakpoint
						nodeStyle = (isProSkin) ? Resource.NodeSkin.customStyles [17] : Resource.NodeSkin.customStyles [22];
					}
					else if (!_action.isEnabled)
					{
						// Disabled
						nodeStyle = (isProSkin) ? Resource.NodeSkin.customStyles [18] : Resource.NodeSkin.customStyles [23];
					}

					_action.AssignParentList (windowData.target);

					if (_action.nodeRect.width == 0)
					{
						_action.nodeRect.width = 300;
					}

					string label = i + ": " + actionsManager.EnabledActions[actionsManager.GetActionTypeIndex (_action)].GetFullTitle ();
					if (!_action.isDisplayed)
					{
						_action.nodeRect.height = 21f;

						if (_action.showComment)
						{
							GUIContent content = new GUIContent (_action.comment);
							float commentHeight = nodeStyle.CalcHeight (content, _action.nodeRect.width);
							_action.nodeRect.height += commentHeight + 5;
						}

						string extraLabel = _action.SetLabel ();
						if (_action is ActionComment)
						{
							if (extraLabel.Length > 40)
							{
								extraLabel = extraLabel.Substring (0, 40) + "..)";
							}
							label = extraLabel;
						}
						else
						{
							if (extraLabel.Length > 15)
							{
								extraLabel = extraLabel.Substring (0, 15) + "..)";
							}
							label += extraLabel;
						}

						_action.nodeRect = GUI.Window (i, _action.nodeRect, EmptyNodeWindow, label, nodeStyle);
					}
					else
					{
						_action.nodeRect = GUILayout.Window (i, _action.nodeRect, NodeWindow, label, nodeStyle);
					}

					GUI.color = originalColor;
				}

				Vector2 finalPosition = new Vector2 (_action.nodeRect.x, _action.nodeRect.y);
				if (finalPosition != originalPosition)
				{
					if (isAsset)
					{
						DragNodes (_action, windowData.targetAsset.actions, finalPosition - originalPosition);
					}
					else
					{
						DragNodes (_action, windowData.target.actions, finalPosition - originalPosition);
					}
				}	
					
				if (_action.nodeRect.x + _action.nodeRect.width + 20 > newMaxScroll.x)
				{
					newMaxScroll.x = _action.nodeRect.x + _action.nodeRect.width + 20;
				}
				
				if (!Mathf.Approximately (_action.nodeRect.height, 10))
				{
					if (_action.nodeRect.y + _action.nodeRect.height + 100 > newMaxScroll.y)
					{
						newMaxScroll.y = _action.nodeRect.y + _action.nodeRect.height + 100;
					}
				}

				LimitWindow (_action);
				DrawSockets (_action, isAsset);
				
				if (isAsset)
				{
					windowData.targetAsset.actions = ActionListAssetEditor.ResizeList (windowData.targetAsset, numActions);
				}
				else
				{
					windowData.target.actions = ActionListEditor.ResizeList (windowData.target.actions, numActions);
				}
				
				if (actionChanging != null && loseConnection && _action.nodeRect.Contains (e.mousePosition))
				{
					Reconnect (actionChanging, _action, isAsset);
				}
				
				if (!isMarquee && _action.nodeRect.Contains (e.mousePosition))
				{
					canMarquee = false;
				}
			}
			
			if (loseConnection && actionChanging != null)
			{
				EndConnect (actionChanging, e.mousePosition, isAsset);
			}
			
			if (actionChanging != null)
			{
				bool onSide = false;
				if (actionChanging is ActionCheck || actionChanging is ActionCheckMultiple || actionChanging is ActionParallel)
				{
					onSide = true;
				}
				AdvGame.DrawNodeCurve (actionChanging.nodeRect, e.mousePosition, Color.black, offsetChanging, onSide, false, actionChanging.isDisplayed);
			}
			
			if (e.type == EventType.ContextClick && actionChanging == null && !isMarquee)
			{
				menuPosition = e.mousePosition;
				CreateEmptyMenu (isAsset);
			}
			
			EndWindows ();
			GUI.EndScrollView ();
			EditorZoomArea.End ();
			
			if (!Mathf.Approximately (newMaxScroll.y, 0f))
			{
				maxScroll = newMaxScroll;
			}
		}


		private void DragNodes (Action dragAction, List<Action> actionList, Vector2 offset)
		{
			foreach (Action _action in actionList)
			{
				if (dragAction != _action && _action.isMarked)
				{
					_action.nodeRect.x += offset.x;
					_action.nodeRect.y += offset.y;
				}
			}
		}


		private void SetMarked (bool isAsset, bool state)
		{
			if (isAsset)
			{
				if (windowData.targetAsset && windowData.targetAsset.actions.Count > 0)
				{
					foreach (Action action in windowData.targetAsset.actions)
					{
						if (action)
						{
							action.isMarked = state;
						}
					}
				}
			}
			else
			{
				if (windowData.target && windowData.target.actions.Count > 0)
				{
					foreach (Action action in windowData.target.actions)
					{
						if (action)
						{
							action.isMarked = state;
						}
					}
				}
			}
		}
		
		
		private void UnmarkAll (bool isAsset)
		{
			SetMarked (isAsset, false);
		}


		private void MarkAll (bool isAsset)
		{
			SetMarked (isAsset, true);
		}
			
		
		private Action InsertAction (int i, Vector2 _position, bool isAsset)
		{
			List<Action> actionList = new List<Action>();
			if (isAsset)
			{
				actionList = windowData.targetAsset.actions;
				Undo.RecordObject (windowData.targetAsset, "Create action");
				ActionListAssetEditor.AddAction (actionsManager.GetDefaultAction (), i+1, windowData.targetAsset);
			}
			else
			{
				actionList = windowData.target.actions;
				ActionListEditor.ModifyAction (windowData.target, windowData.target.actions[i], "Insert after");
			}
			
			numActions ++;
			UnmarkAll (isAsset);
			
			actionList [i+1].nodeRect.x = _position.x - 150;
			actionList [i+1].nodeRect.y = _position.y;
			actionList [i+1].endAction = ResultAction.Stop;
			actionList [i+1].isDisplayed = true;
			
			return actionList [i+1];
		}
		
		
		private void FixConnections (int i, bool isAsset)
		{
			List<Action> actionList = new List<Action>();
			if (isAsset)
			{
				actionList = windowData.targetAsset.actions;
			}
			else
			{
				actionList = windowData.target.actions;
			}

			if (actionList[i] == null) return;
			
			if (actionList[i].numSockets == 0)
			{
				actionList[i].endAction = ResultAction.Stop;
			}
			
			else if (actionList[i] is ActionCheck)
			{
				ActionCheck tempAction = (ActionCheck) actionList[i];
				if (tempAction.resultActionTrue == ResultAction.Skip && !actionList.Contains (tempAction.skipActionTrueActual))
				{
					if (tempAction.skipActionTrue >= actionList.Count)
					{
						tempAction.resultActionTrue = ResultAction.Stop;
					}
				}
				if (tempAction.resultActionFail == ResultAction.Skip && !actionList.Contains (tempAction.skipActionFailActual))
				{
					if (tempAction.skipActionFail >= actionList.Count)
					{
						tempAction.resultActionFail = ResultAction.Stop;
					}
				}
			}
			else if (actionList[i] is ActionCheckMultiple)
			{
				ActionCheckMultiple tempAction = (ActionCheckMultiple) actionList[i];
				foreach (ActionEnd ending in tempAction.endings)
				{
					if (ending.resultAction == ResultAction.Skip && !actionList.Contains (ending.skipActionActual))
					{
						if (ending.skipAction >= actionList.Count)
						{
							ending.resultAction = ResultAction.Stop;
						}
					}
				}
			}
			else if (actionList[i] is ActionParallel)
			{
				ActionParallel tempAction = (ActionParallel) actionList[i];
				foreach (ActionEnd ending in tempAction.endings)
				{
					if (ending.resultAction == ResultAction.Skip && !actionList.Contains (ending.skipActionActual))
					{
						if (ending.skipAction >= actionList.Count)
						{
							ending.resultAction = ResultAction.Stop;
						}
					}
				}
			}
			else
			{
				if (actionList[i].endAction == ResultAction.Skip && !actionList.Contains (actionList[i].skipActionActual))
				{
					if (actionList[i].skipAction >= actionList.Count)
					{
						actionList[i].endAction = ResultAction.Stop;
					}
				}
			}
		}
		
		
		private void EndConnect (Action action1, Vector2 mousePosition, bool isAsset)
		{
			List<Action> actionList = (isAsset) ? windowData.targetAsset.actions : windowData.target.actions;
			
			isMarquee = false;
			
			if (action1 is ActionCheck)
			{
				ActionCheck tempAction = (ActionCheck) action1;
				if (resultType)
				{
					if (actionList.IndexOf (action1) == actionList.Count - 1 && tempAction.resultActionTrue != ResultAction.Skip)
					{
						InsertAction (actionList.IndexOf (action1), mousePosition, isAsset);
						tempAction.resultActionTrue = ResultAction.Continue;

						if (tempAction.resultActionFail == ResultAction.Continue)
						{
							tempAction.resultActionFail = ResultAction.Stop;
						}
					}
					else if (tempAction.resultActionTrue == ResultAction.Stop)
					{
						tempAction.resultActionTrue = ResultAction.Skip;
						tempAction.skipActionTrueActual = InsertAction (actionList.Count-1, mousePosition, isAsset);
					}
					else
					{
						tempAction.resultActionTrue = ResultAction.Stop;
					}
				}
				else
				{
					if (actionList.IndexOf (action1) == actionList.Count - 1 && tempAction.resultActionFail != ResultAction.Skip)
					{
						InsertAction (actionList.IndexOf (action1), mousePosition, isAsset);
						tempAction.resultActionFail = ResultAction.Continue;

						if (tempAction.resultActionTrue == ResultAction.Continue)
						{
							tempAction.resultActionTrue = ResultAction.Stop;
						}
					}
					else if (tempAction.resultActionFail == ResultAction.Stop)
					{
						tempAction.resultActionFail = ResultAction.Skip;
						tempAction.skipActionFailActual = InsertAction (actionList.Count-1, mousePosition, isAsset);
					}
					else
					{
						tempAction.resultActionFail = ResultAction.Stop;
					}
				}
			}
			else if (action1 is ActionCheckMultiple)
			{
				ActionCheckMultiple tempAction = (ActionCheckMultiple) action1;
				ActionEnd ending = tempAction.endings [multipleResultType];
				
				if (actionList.IndexOf (action1) == actionList.Count - 1 && ending.resultAction != ResultAction.Skip)
				{
					InsertAction (actionList.IndexOf (action1), mousePosition, isAsset);
					ending.resultAction = ResultAction.Continue;

					foreach (ActionEnd otherEnding in tempAction.endings)
					{
						if (otherEnding != ending && otherEnding.resultAction == ResultAction.Continue)
						{
							otherEnding.resultAction = ResultAction.Stop;
						}
					}
				}
				else if (ending.resultAction == ResultAction.Stop)
				{
					ending.resultAction = ResultAction.Skip;
					ending.skipActionActual = InsertAction (actionList.Count-1, mousePosition, isAsset);
				}
				else
				{
					ending.resultAction = ResultAction.Stop;
				}
			}
			else if (action1 is ActionParallel)
			{
				ActionParallel tempAction = (ActionParallel) action1;
				ActionEnd ending = tempAction.endings [multipleResultType];
				
				if (actionList.IndexOf (action1) == actionList.Count - 1 && ending.resultAction != ResultAction.Skip)
				{
					InsertAction (actionList.IndexOf (action1), mousePosition, isAsset);
					ending.resultAction = ResultAction.Continue;

					foreach (ActionEnd otherEnding in tempAction.endings)
					{
						if (otherEnding != ending && otherEnding.resultAction == ResultAction.Continue)
						{
							otherEnding.resultAction = ResultAction.Stop;
						}
					}
				}
				else if (ending.resultAction == ResultAction.Stop)
				{
					ending.resultAction = ResultAction.Skip;
					ending.skipActionActual = InsertAction (actionList.Count-1, mousePosition, isAsset);
				}
				else
				{
					ending.resultAction = ResultAction.Stop;
				}
			}
			else
			{
				if (actionList.IndexOf (action1) == actionList.Count - 1 && action1.endAction != ResultAction.Skip)
				{
					InsertAction (actionList.IndexOf (action1), mousePosition, isAsset);
					action1.endAction = ResultAction.Continue;
				}
				else if (action1.endAction == ResultAction.Stop)
				{
					// Remove bad "end" connection
					float x = mousePosition.x;
					foreach (AC.Action action in actionList)
					{
						if (action.nodeRect.x > x && !(action is ActionCheck) && !(action is ActionCheckMultiple || action is ActionParallel) && action.endAction == ResultAction.Continue)
						{
							// Is this the "last" one?
							int i = actionList.IndexOf (action);
							if (actionList.Count == (i+1))
							{
								action.endAction = ResultAction.Stop;
							}
						}
					}
					
					action1.endAction = ResultAction.Skip;
					action1.skipActionActual = InsertAction (actionList.Count-1, mousePosition, isAsset);
				}
				else
				{
					action1.endAction = ResultAction.Stop;
				}
			}
			
			actionChanging = null;
			offsetChanging = 0;
			
			if (isAsset)
			{
				EditorUtility.SetDirty (windowData.targetAsset);
			}
			else
			{
				UnityVersionHandler.CustomSetDirty (windowData.target, true);
			}
		}
		
		
		private void Reconnect (Action action1, Action action2, bool isAsset)
		{
			isMarquee = false;
			
			if (action1 is ActionCheck)
			{
				ActionCheck actionCheck = (ActionCheck) action1;
				
				if (resultType)
				{
					actionCheck.resultActionTrue = ResultAction.Skip;
					if (action2 != null)
					{
						actionCheck.skipActionTrueActual = action2;
					}
				}
				else
				{
					actionCheck.resultActionFail = ResultAction.Skip;
					if (action2 != null)
					{
						actionCheck.skipActionFailActual = action2;
					}
				}
			}
			else if (action1 is ActionCheckMultiple)
			{
				ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action1;
				
				ActionEnd ending = actionCheckMultiple.endings [multipleResultType];
				
				ending.resultAction = ResultAction.Skip;
				if (action2 != null)
				{
					ending.skipActionActual = action2;
				}
			}
			else if (action1 is ActionParallel)
			{
				ActionParallel actionParallel = (ActionParallel) action1;
				ActionEnd ending = actionParallel.endings [multipleResultType];
				
				ending.resultAction = ResultAction.Skip;
				if (action2 != null)
				{
					ending.skipActionActual = action2;
				}
			}
			else
			{
				action1.endAction = ResultAction.Skip;
				action1.skipActionActual = action2;
			}
			
			actionChanging = null;
			offsetChanging = 0;
			
			if (isAsset)
			{
				EditorUtility.SetDirty (windowData.targetAsset);
			}
			else
			{
				UnityVersionHandler.CustomSetDirty (windowData.target, true);
			}
		}
		
		
		private void DrawSockets (Action action, bool isAsset)
		{
			List<Action> actionList = new List<Action>();
			if (isAsset)
			{
				actionList = windowData.targetAsset.actions;
			}
			else
			{
				actionList = windowData.target.actions;
			}
			
			int i = actionList.IndexOf (action);
			Event e = Event.current;
			
			if (action.numSockets == 0)
			{
				return;
			}
			
			if (!action.isDisplayed && (action is ActionCheck || action is ActionCheckMultiple || action is ActionParallel))
			{
				action.DrawOutWires (actionList, i, 0);
				return;
			}
			
			int offset = 0;
			
			if (action is ActionCheck)
			{
				ActionCheck actionCheck = (ActionCheck) action;
				if (actionCheck.resultActionFail != ResultAction.RunCutscene)
				{
					if (actionCheck.resultActionFail != ResultAction.Skip || action.showOutputSockets)
					{
						Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width - 2, action.nodeRect.y - 22 + action.nodeRect.height, 16, 16);
						
						if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
						{
							if (e.button == 0)
							{
								offsetChanging = 10;
								resultType = false;
								actionChanging = action;
							}
							else if (e.button == 1)
							{
								if (actionCheck.resultActionFail == ResultAction.Continue && (actionList.IndexOf (action) < actionList.Count - 1))
								{
									CreateSocketMenu (actionList.IndexOf (action) + 1);
								}
								else if (actionCheck.resultActionFail == ResultAction.Skip)
								{
									CreateSocketMenu (actionCheck.skipActionFail);
								}
							}
						}

						
						GUI.Button (buttonRect, "", Resource.NodeSkin.customStyles[10]);
					}

					if (actionCheck.resultActionFail == ResultAction.Skip)
					{
						offset = 17;
					}
				}
				if (actionCheck.resultActionTrue != ResultAction.RunCutscene)
				{
					if (actionCheck.resultActionTrue != ResultAction.Skip || action.showOutputSockets)
					{
						Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width - 2, action.nodeRect.y - 40 - offset + action.nodeRect.height, 16, 16);
						
						if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
						{
							if (e.button == 0)
							{
								offsetChanging = 30 + offset;
								resultType = true;
								actionChanging = action;
							}
							else if (e.button == 1)
							{
								if (actionCheck.resultActionTrue == ResultAction.Continue && (actionList.IndexOf (action) < actionList.Count - 1))
								{
									CreateSocketMenu (actionList.IndexOf (action) + 1);
								}
								else if (actionCheck.resultActionTrue == ResultAction.Skip)
								{
									CreateSocketMenu (actionCheck.skipActionTrue);
								}
							}
						}
						
						GUI.Button (buttonRect, "", Resource.NodeSkin.customStyles[10]);
					}
				}
			}
			else if (action is ActionCheckMultiple)
			{
				ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;

				int totalHeight = 20;
				for (int j = actionCheckMultiple.endings.Count-1; j>=0; j--)
				{
					ActionEnd ending = actionCheckMultiple.endings [j];

					if (ending.resultAction != ResultAction.RunCutscene)
					{
						if (ending.resultAction != ResultAction.Skip || action.showOutputSockets)
						{
							Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width - 2,
							                            action.nodeRect.y + action.nodeRect.height
							                            - totalHeight,
							                            16, 16);

							if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
							{
								if (e.button == 0)
								{
									offsetChanging = totalHeight - 10;
									multipleResultType = actionCheckMultiple.endings.IndexOf (ending);
									actionChanging = action;
								}
								else if (e.button == 1)
								{
									if (ending.resultAction == ResultAction.Continue && (actionList.IndexOf (action) < actionList.Count - 1))
									{
										CreateSocketMenu (actionList.IndexOf (action) + 1);
									}
									else if (ending.resultAction == ResultAction.Skip)
									{
										CreateSocketMenu (ending.skipAction);
									}
								}
							}
							
							GUI.Button (buttonRect, "", Resource.NodeSkin.customStyles[10]);
						}
					}

					if (ending.resultAction == ResultAction.Skip)
					{
						totalHeight += 44;
					}
					else
					{
						totalHeight += 26;
					}
				}
			}
			else if (action is ActionParallel)
			{
				ActionParallel actionParallel = (ActionParallel) action;

				int totalHeight = 20;
				for (int j = actionParallel.endings.Count-1; j>=0; j--)
				{
					ActionEnd ending = actionParallel.endings [j];

					if (ending.resultAction != ResultAction.RunCutscene)
					{
						if (ending.resultAction != ResultAction.Skip || action.showOutputSockets)
						{
							Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width - 2,
							                            action.nodeRect.y + action.nodeRect.height
							                            - totalHeight,
							                            16, 16);

							if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
							{
								if (e.button == 0)
								{
									offsetChanging = totalHeight - 10;
									multipleResultType = actionParallel.endings.IndexOf (ending);
									actionChanging = action;
								}
								else if (e.button == 1)
								{
									if (ending.resultAction == ResultAction.Continue && (actionList.IndexOf (action) < actionList.Count - 1))
									{
										CreateSocketMenu (actionList.IndexOf (action) + 1);
									}
									else if (ending.resultAction == ResultAction.Skip)
									{
										CreateSocketMenu (ending.skipAction);
									}
								}
							}
							
							GUI.Button (buttonRect, "", Resource.NodeSkin.customStyles[10]);
						}
					}

					if (ending.resultAction == ResultAction.Skip)
					{
						totalHeight += 44;
					}
					else
					{
						totalHeight += 26;
					}
				}
			}
			else
			{
				if (action.endAction != ResultAction.RunCutscene)
				{
					if (action.endAction != ResultAction.Skip || action.showOutputSockets)
					{
						Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width / 2f - 8, action.nodeRect.y + action.nodeRect.height, 16, 16);
						
						if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
						{
							if (e.button == 0)
							{
								offsetChanging = 10;
								actionChanging = action;
							}
							else if (e.button == 1)
							{
								if (action.endAction == ResultAction.Continue && (actionList.IndexOf (action) < actionList.Count - 1))
								{
									CreateSocketMenu (actionList.IndexOf (action) + 1);
								}
								else if (action.endAction == ResultAction.Skip)
								{
									CreateSocketMenu (action.skipAction);
								}
							}
						}
						
						GUI.Button (buttonRect, "", Resource.NodeSkin.customStyles[10]);
					}
				}
			}
			
			action.DrawOutWires (actionList, i, offset);
		}


		private void FocusOnAction (int i, bool isAsset)
		{
			UnmarkAll (isAsset);
			List<Action> actionList = new List<Action>();
			if (isAsset)
			{
				actionList = windowData.targetAsset.actions;
			}
			else
			{
				actionList = windowData.target.actions;
			}

			if (actionList.Count > i && i >= 0 && actionList[i] != null)
			{
				actionList[i].isMarked = true;
				Vector2 topLeft = new Vector2 (actionList[i].nodeRect.x, actionList[i].nodeRect.y);
				Vector2 offset = new Vector2 (-50f, -50f);

				scrollPosition = topLeft + offset;
			}
		}
		
		
		private int NumActionsMarked (bool isAsset)
		{
			List<Action> actionList = new List<Action>();
			if (isAsset)
			{
				actionList = windowData.targetAsset.actions;
			}
			else
			{
				actionList = windowData.target.actions;
			}
			
			int i=0;
			foreach (Action action in actionList)
			{
				if (action != null && action.isMarked)
				{
					i++;
				}
			}
			
			return i;
		}
		
		
		private void CreateEmptyMenu (bool isAsset)
		{
			EditorGUIUtility.editingTextField = false;
			GenericMenu menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Add new Action"), false, EmptyCallback, "Add new Action");
			if (AdvGame.copiedActions != null && AdvGame.copiedActions.Count > 0)
			{
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Paste copied Action(s)"), false, EmptyCallback, "Paste copied Action(s)");
			}
			
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Select all"), false, EmptyCallback, "Select all");
			
			if (NumActionsMarked (isAsset) > 0)
			{
				menu.AddItem (new GUIContent ("Deselect all"), false, EmptyCallback, "Deselect all");
				menu.AddSeparator ("");
				if (!Application.isPlaying)
				{
					menu.AddItem (new GUIContent ("Cut selected"), false, EmptyCallback, "Cut selected");
					menu.AddItem (new GUIContent ("Copy selected"), false, EmptyCallback, "Copy selected");
				}
				menu.AddItem (new GUIContent ("Delete selected"), false, EmptyCallback, "Delete selected");
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Collapse selected"), false, EmptyCallback, "Collapse selected");
				menu.AddItem (new GUIContent ("Expand selected"), false, EmptyCallback, "Expand selected");
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Comment selected"), false, EmptyCallback, "Comment selected");
				menu.AddItem (new GUIContent ("Uncomment selected"), false, EmptyCallback, "Uncomment selected");
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Show output socket(s)"), false, EmptyCallback, "Show output socket(s)");
				menu.AddItem (new GUIContent ("Hide output socket(s)"), false, EmptyCallback, "Hide output socket(s)");

				if (NumActionsMarked (isAsset) == 1)
				{
					menu.AddSeparator ("");
					menu.AddItem (new GUIContent ("Move to front"), false, EmptyCallback, "Move to front");
				}
			}
			
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Auto-arrange"), false, EmptyCallback, "Auto-arrange");
			
			menu.ShowAsContext ();
		}
		
		
		private void CreateNodeMenu (bool isAsset, int i, Action _action)
		{
			EditorGUIUtility.editingTextField = false;
			UnmarkAll (isAsset);
			_action.isMarked = true;
			
			GenericMenu menu = new GenericMenu ();

			if (!Application.isPlaying)
			{
				menu.AddItem (new GUIContent ("Cut"), false, EmptyCallback, "Cut selected");
				menu.AddItem (new GUIContent ("Copy"), false, EmptyCallback, "Copy selected");
				if (AdvGame.copiedActions.Count > 0)
				{
					menu.AddItem (new GUIContent ("Paste after"), false, EmptyCallback, "Paste after");
				}
				menu.AddSeparator ("");
			}
			menu.AddItem (new GUIContent ("Insert after"), false, EmptyCallback, "Insert after");
			menu.AddItem (new GUIContent ("Delete"), false, EmptyCallback, "Delete selected");
			
			if (i>0)
			{
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Move to front"), false, EmptyCallback, "Move to front");
			}

			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Toggle breakpoint"), false, EmptyCallback, "Toggle breakpoint");
			menu.AddItem (new GUIContent ("Toggle comment"), false, EmptyCallback, "Toggle comment");
			menu.AddItem (new GUIContent ("Toggle output socket(s)"), false, EmptyCallback, "Toggle output socket(s)");
			
			menu.ShowAsContext ();
		}


		private void CreateSocketMenu (int i)
		{
			focusActionIndex = i;
			EditorGUIUtility.editingTextField = false;

			GenericMenu menu = new GenericMenu ();

			menu.AddItem (new GUIContent ("Show linked Action"), false, SocketCallBack, "Focus on linked Action");
			menu.ShowAsContext ();
		}
		

		private void EmptyCallback (object obj)
		{
			PerformEmptyCallBack (obj.ToString ());
		}


		private void SocketCallBack (object obj)
		{
			PerformSocketCallBack (obj.ToString ());
		}


		private void PerformEmptyCallBack (string objString)
		{
			bool isAsset = false;
			bool doUndo = (objString != "Copy selected");
			List<Action> actionList = new List<Action>();
			if (windowData.targetAsset != null)
			{
				isAsset = true;
				actionList = windowData.targetAsset.actions;
			}
			else
			{
				actionList = windowData.target.actions;
			}

			if (doUndo)
			{
				if (isAsset)
				{
					Undo.SetCurrentGroupName (objString);
					Undo.RecordObjects (new UnityEngine.Object [] {  windowData.targetAsset }, objString);
					Undo.RecordObjects (windowData.targetAsset.actions.ToArray (), objString);
				}
				else
				{
					Undo.SetCurrentGroupName (objString);
					Undo.RecordObjects (new UnityEngine.Object [] {  windowData.target }, objString);
					Undo.RecordObjects (windowData.target.actions.ToArray (), objString);
				}
			}

			foreach (Action action in actionList)
			{
				action.SkipActionGUI (actionList, false);
			}
			
			if (objString == "Add new Action")
			{
				Action currentAction = actionList [actionList.Count-1];
				if (currentAction.endAction == ResultAction.Continue)
				{
					currentAction.endAction = ResultAction.Stop;
				}
				
				if (isAsset)
				{
					ActionListAssetEditor.ModifyAction (windowData.targetAsset, currentAction, "Insert after");
				}
				else
				{
					ActionListEditor.ModifyAction (windowData.target, null, "Insert end");
				}
				
				actionList[actionList.Count-1].nodeRect.x = menuPosition.x;
				actionList[actionList.Count-1].nodeRect.y = menuPosition.y;
				actionList[actionList.Count-1].isDisplayed = true;
			}
			else if (objString == "Paste copied Action(s)")
			{
				if (AdvGame.copiedActions.Count == 0)
				{
					return;
				}

				int offset = actionList.Count;
				UnmarkAll (isAsset);

				Action currentLastAction = actionList [actionList.Count-1];
				if (currentLastAction.endAction == ResultAction.Continue)
				{
					currentLastAction.endAction = ResultAction.Stop;
				}
				
				Vector2 firstPosition = new Vector2 (AdvGame.copiedActions[0].nodeRect.x, AdvGame.copiedActions[0].nodeRect.y);
				foreach (Action actionToCopy in AdvGame.copiedActions)
				{
					if (actionToCopy == null)
					{
						ACDebug.LogWarning ("Error when pasting Action - cannot find original. Did you change scene before pasting? If you need to transfer Actions between scenes, copy them to an ActionList asset first.");
						continue;
					}

					AC.Action duplicatedAction = Object.Instantiate (actionToCopy) as AC.Action;
					duplicatedAction.PrepareToPaste (offset);

					if (AdvGame.copiedActions.IndexOf (actionToCopy) == 0)
					{
						duplicatedAction.nodeRect.x = menuPosition.x;
						duplicatedAction.nodeRect.y = menuPosition.y;
					}
					else
					{
						duplicatedAction.nodeRect.x = menuPosition.x + (actionToCopy.nodeRect.x - firstPosition.x);
						duplicatedAction.nodeRect.y = menuPosition.y + (actionToCopy.nodeRect.y - firstPosition.y);
					}
					if (isAsset)
					{
						duplicatedAction.hideFlags = HideFlags.HideInHierarchy;
						AssetDatabase.AddObjectToAsset (duplicatedAction, windowData.targetAsset);
					}

					duplicatedAction.isMarked = true;
					actionList.Add (duplicatedAction);
				}
				if (isAsset)
				{
					AssetDatabase.SaveAssets ();
				}
				AdvGame.DuplicateActionsBuffer ();
			}
			else if (objString == "Select all")
			{
				foreach (Action action in actionList)
				{
					action.isMarked = true;
				}
			}
			else if (objString == "Deselect all")
			{
				foreach (Action action in actionList)
				{
					action.isMarked = false;
				}
			}
			else if (objString == "Expand selected")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.isDisplayed = true;
					}
				}
			}
			else if (objString == "Collapse selected")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.isDisplayed = false;
					}
				}
			}
			else if (objString == "Comment selected")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.showComment = true;
					}
				}
			}
			else if (objString == "Uncomment selected")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.showComment = false;
					}
				}
			}
			else if (objString == "Show output socket(s)")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.showOutputSockets = true;
					}
				}
			}
			else if (objString == "Hide output socket(s)")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.showOutputSockets = false;
					}
				}
			}
			else if (objString == "Cut selected")
			{
				List<Action> copyList = new List<Action>();
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						Action copyAction = Object.Instantiate (action) as Action;
						copyAction.name = copyAction.name.Replace ("(Clone)", "");
						copyAction.PrepareToCopy (actionList.IndexOf (action), actionList);
						copyAction.ClearIDs ();
						copyAction.isMarked = false;
						copyList.Add (copyAction);
					}
				}

				AdvGame.copiedActions = copyList;
				PerformEmptyCallBack ("Delete selected");
			}
			else if (objString == "Copy selected")
			{
				List<Action> copyList = new List<Action>();
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						Action copyAction = Object.Instantiate (action) as Action;
						copyAction.name = copyAction.name.Replace ("(Clone)", "");
						copyAction.PrepareToCopy (actionList.IndexOf (action), actionList);
						copyAction.ClearIDs ();
						copyAction.isMarked = false;
						copyList.Add (copyAction);
					}
				}

				AdvGame.copiedActions = copyList;
				UnmarkAll (isAsset);
			}
			else if (objString == "Delete selected")
			{
				while (NumActionsMarked (isAsset) > 0)
				{
					foreach (Action action in actionList)
					{
						if (action.isMarked)
						{
							// Work out what has to be re-connected to what after deletion
							Action targetAction = null;
							if (action is ActionCheck || action is ActionCheckMultiple || action is ActionParallel) {}
							else
							{
								if (action.endAction == ResultAction.Skip && action.skipActionActual)
								{
									targetAction = action.skipActionActual;
								}
								else if (action.endAction == ResultAction.Continue && actionList.IndexOf (action) < (actionList.Count - 1))
								{
									targetAction = actionList [actionList.IndexOf (action)+1];
								}

								foreach (Action _action in actionList)
								{
									if (action != _action)
									{
										_action.FixLinkAfterDeleting (action, targetAction, actionList);
									}
								}

								if (targetAction != null && actionList.IndexOf (action) == 0)
								{
									// Deleting first, so find new first
									actionList.Remove (targetAction);
									actionList.Insert (0, targetAction);
								}
							}

							
							if (isAsset)
							{
								ActionListAssetEditor.DeleteAction (action, windowData.targetAsset);
							}
							else
							{
								actionList.Remove (action);
							}
							
							numActions --;
							if (action != null)
							{
								Undo.DestroyObjectImmediate (action);
							}
							break;
						}
					}
				}
				if (actionList.Count == 0)
				{
					if (isAsset)
					{
						actionList.Add (ActionList.GetDefaultAction ());
					}
				}
			}
			else if (objString == "Move to front")
			{
				for (int i=0; i<actionList.Count; i++)
				{
					Action action = actionList[i];
					if (action.isMarked)
					{
						action.isMarked = false;

						if (i > 0)
						{
							if (action is ActionParallel)
							{
								ActionParallel actionParallel = (ActionParallel) action;

								foreach (ActionEnd ending in actionParallel.endings)
								{
									if (ending.resultAction == ResultAction.Continue)
									{
										if (i == actionList.Count - 1)
										{
											ending.resultAction = ResultAction.Stop;
										}
										else
										{
											ending.resultAction = ResultAction.Skip;
											ending.skipActionActual = actionList[i+1];
										}
									}
								}
							}
							else if (action is ActionCheck)
							{
								ActionCheck actionCheck = (ActionCheck) action;

								if (actionCheck.resultActionTrue == ResultAction.Continue)
								{
									if (i == actionList.Count - 1)
									{
										actionCheck.resultActionTrue = ResultAction.Stop;
									}
									else
									{
										actionCheck.resultActionTrue = ResultAction.Skip;
										actionCheck.skipActionTrueActual = actionList[i+1];
									}
								}

								if (actionCheck.resultActionFail == ResultAction.Continue)
								{
									if (i == actionList.Count - 1)
									{
										actionCheck.resultActionFail = ResultAction.Stop;
									}
									else
									{
										actionCheck.resultActionFail = ResultAction.Skip;
										actionCheck.skipActionFailActual = actionList[i+1];
									}
								}
							}
							else if (action is ActionCheckMultiple)
							{
								ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;

								foreach (ActionEnd ending in actionCheckMultiple.endings)
								{
									if (ending.resultAction == ResultAction.Continue)
									{
										if (i == actionList.Count - 1)
										{
											ending.resultAction = ResultAction.Stop;
										}
										else
										{
											ending.resultAction = ResultAction.Skip;
											ending.skipActionActual = actionList[i+1];
										}
									}
								}
							}
							else
							{
								if (action.endAction == ResultAction.Continue)
								{
									if (i == actionList.Count - 1)
									{
										action.endAction = ResultAction.Stop;
									}
									else
									{
										action.endAction = ResultAction.Skip;
										action.skipActionActual = actionList[i+1];
									}
								}
							}
							
							actionList[0].nodeRect.x += 30f;
							actionList[0].nodeRect.y += 30f;
							actionList.Remove (action);
							actionList.Insert (0, action);
						}
					}
				}
			}
			else if (objString == "Auto-arrange")
			{
				AutoArrange (isAsset);
			}
			else if (objString == "Toggle breakpoint")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.isBreakPoint = !action.isBreakPoint;
						action.isMarked = false;
					}
				}
			}
			else if (objString == "Toggle comment")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.showComment = !action.showComment;
						action.isMarked = false;
					}
				}
			}
			else if (objString == "Toggle output socket(s)")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.showOutputSockets = !action.showOutputSockets;
						action.isMarked = false;
					}
				}
			}
			else if (objString == "Insert after")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.isMarked = false;

						int newIndex = actionList.IndexOf (action) + 1;

						if (isAsset)
						{
							ActionListAssetEditor.ModifyAction (windowData.targetAsset, action, "Insert after");
						}
						else
						{
							ActionListEditor.ModifyAction (windowData.target, action, "Insert after");
						}

						actionList[newIndex].nodeRect.x = action.nodeRect.x + 50;
						actionList[newIndex].nodeRect.y = action.nodeRect.y + 100;
						actionList[newIndex].isDisplayed = true;

						if (action is ActionParallel)
						{
							ActionParallel actionParallel = (ActionParallel) action;
							if (actionParallel.endings != null && actionParallel.endings.Count > 0)
							{
								actionParallel.endings[0].resultAction = ResultAction.Continue;
							}
						}
						else if (action is ActionCheck)
						{
							ActionCheck actionCheck = (ActionCheck) action;
							actionCheck.resultActionTrue = ResultAction.Continue;
						}
						else if (action is ActionCheckMultiple)
						{
							ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;
							if (actionCheckMultiple.endings != null && actionCheckMultiple.endings.Count > 0)
							{
								actionCheckMultiple.endings[0].resultAction = ResultAction.Continue;
							}
						}
						else
						{
							action.endAction = ResultAction.Continue;
						}

						if (isAsset)
						{
							AssetDatabase.SaveAssets ();
						}

						break;
					}
				}
			}
			else if (objString == "Paste after")
			{
				foreach (Action action in actionList)
				{
					if (action.isMarked)
					{
						action.isMarked = false;

						int offset = actionList.IndexOf (action) + 1;
						Vector2 initialPosition = new Vector2 (action.nodeRect.x + 50, action.nodeRect.y + 100);

						//
						Vector2 firstPosition = new Vector2 (AdvGame.copiedActions[0].nodeRect.x, AdvGame.copiedActions[0].nodeRect.y);
						foreach (Action actionToCopy in AdvGame.copiedActions)
						{
							if (actionToCopy == null)
							{
								ACDebug.LogWarning ("Error when pasting Action - cannot find original. Did you change scene before pasting? If you need to transfer Actions between scenes, copy them to an ActionList asset first.");
								continue;
							}

							int ownIndex = AdvGame.copiedActions.IndexOf (actionToCopy);

							AC.Action duplicatedAction = Object.Instantiate (actionToCopy) as AC.Action;
							duplicatedAction.PrepareToPaste (offset);

							if (ownIndex == 0)
							{
								duplicatedAction.nodeRect.x = initialPosition.x;
								duplicatedAction.nodeRect.y = initialPosition.y;
							}
							else
							{
								duplicatedAction.nodeRect.x = initialPosition.x + (actionToCopy.nodeRect.x - firstPosition.x);
								duplicatedAction.nodeRect.y = initialPosition.y + (actionToCopy.nodeRect.y - firstPosition.y);
							}
							if (isAsset)
							{
								duplicatedAction.hideFlags = HideFlags.HideInHierarchy;
								AssetDatabase.AddObjectToAsset (duplicatedAction, windowData.targetAsset);
							}

							duplicatedAction.isMarked = true;
							actionList.Insert (offset + ownIndex, duplicatedAction);
						}

						if (action is ActionParallel)
						{
							ActionParallel actionParallel = (ActionParallel) action;
							if (actionParallel.endings != null && actionParallel.endings.Count > 0)
							{
								actionParallel.endings[0].resultAction = ResultAction.Continue;
							}
						}
						else if (action is ActionCheck)
						{
							ActionCheck actionCheck = (ActionCheck) action;
							actionCheck.resultActionTrue = ResultAction.Continue;
						}
						else if (action is ActionCheckMultiple)
						{
							ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;
							if (actionCheckMultiple.endings != null && actionCheckMultiple.endings.Count > 0)
							{
								actionCheckMultiple.endings[0].resultAction = ResultAction.Continue;
							}
						}
						else
						{
							action.endAction = ResultAction.Continue;
						}

						if (isAsset)
						{
							AssetDatabase.SaveAssets ();
						}

						/*
						int newIndex = actionList.IndexOf (action) + 1;

						if (isAsset)
						{
							ActionListAssetEditor.ModifyAction (windowData.targetAsset, action, "Paste after");
						}
						else
						{
							ActionListEditor.ModifyAction (windowData.target, action, "Paste after");
						}
						
						//actionList[newIndex].nodeRect.x = action.nodeRect.x + 50;
						//actionList[newIndex].nodeRect.y = action.nodeRect.y + 100;
						actionList[newIndex].isDisplayed = true;

						if (action is ActionParallel)
						{
							ActionParallel actionParallel = (ActionParallel) action;
							if (actionParallel.endings != null && actionParallel.endings.Count > 0)
							{
								actionParallel.endings[0].resultAction = ResultAction.Continue;
							}
						}
						else if (action is ActionCheck)
						{
							ActionCheck actionCheck = (ActionCheck) action;
							actionCheck.resultActionTrue = ResultAction.Continue;
						}
						else if (action is ActionCheckMultiple)
						{
							ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;
							if (actionCheckMultiple.endings != null && actionCheckMultiple.endings.Count > 0)
							{
								actionCheckMultiple.endings[0].resultAction = ResultAction.Continue;
							}
						}
						else
						{
							action.endAction = ResultAction.Continue;
						}

						//*/

						AdvGame.DuplicateActionsBuffer ();
						break;
					}
				}
			}

			foreach (Action action in actionList)
			{
				action.SkipActionGUI (actionList, false);
			}

			if (doUndo)
			{
				if (isAsset)
				{
					Undo.RecordObjects (new UnityEngine.Object [] { windowData.targetAsset }, objString);
					Undo.RecordObjects (windowData.targetAsset.actions.ToArray (), objString);
				}
				else
				{
					Undo.RecordObjects (new UnityEngine.Object [] { windowData.target }, objString);
					Undo.RecordObjects (windowData.target.actions.ToArray (), objString);
				}
				Undo.CollapseUndoOperations (Undo.GetCurrentGroup ());
			}

			if (isAsset)
			{
				EditorUtility.SetDirty (windowData.targetAsset);
			}
			else
			{
				EditorUtility.SetDirty (windowData.target);
			}
		}


		private void PerformSocketCallBack (string objString)
		{
			bool isAsset = false;
			if (windowData.targetAsset != null)
			{
				isAsset = true;
			}

			if (objString == "Focus on linked Action")
			{
				FocusOnAction (focusActionIndex, isAsset);
			}
		}
		
		
		private void AutoArrange (bool isAsset)
		{
			isAutoArranging = true;

			List<Action> actionList = (isAsset) ? windowData.targetAsset.actions : windowData.target.actions;
			foreach (Action action in actionList)
			{
				// Fix reconnection error from non-displayed Actions
				if (action.endAction == ResultAction.Skip || action.numSockets == 2 || action is ActionCheckMultiple || action is ActionParallel)
				{
					if (isAsset)
					{
						action.SkipActionGUI (actionList, false);
					}
					else
					{
						action.SkipActionGUI (actionList, false);
					}
				}

				action.isMarked = true;
				if (actionList.IndexOf (action) != 0)
				{
					action.nodeRect.x = action.nodeRect.y = -10;
				}
			}
			
			DisplayActionsInEditor _display = DisplayActionsInEditor.ArrangedVertically;
			if (AdvGame.GetReferences ().actionsManager && AdvGame.GetReferences ().actionsManager.displayActionsInEditor == DisplayActionsInEditor.ArrangedHorizontally)
			{
				_display = DisplayActionsInEditor.ArrangedHorizontally;
			}

			ArrangeFromIndex (actionList, 0, 0, 14, _display);

			int i=1;
			float maxValue = 0f;
			foreach (Action _action in actionList)
			{
				if (_display == DisplayActionsInEditor.ArrangedVertically)
				{
					maxValue = Mathf.Max (maxValue, _action.nodeRect.y + _action.nodeRect.height);
				}
				else
				{
					maxValue = Mathf.Max (maxValue, _action.nodeRect.x);
				}
			}

			foreach (Action _action in actionList)
			{
				if (_action.isMarked)
				{
					// Wasn't arranged
					if (_display == DisplayActionsInEditor.ArrangedVertically)
					{
						_action.nodeRect.x = 14;
						_action.nodeRect.y = maxValue + 14*i;
						ArrangeFromIndex (actionList, actionList.IndexOf (_action), 0, 14, _display);
					}
					else
					{
						_action.nodeRect.x = maxValue + 350*i;
						_action.nodeRect.y = 14;
						ArrangeFromIndex (actionList, actionList.IndexOf (_action), 0, 14, _display);
					}
					_action.isMarked = false;
					i++;
				}
			}

			isAutoArranging = false;
		}
		
		
		private void ArrangeFromIndex (List<Action> actionList, int i, int depth, float minValue, DisplayActionsInEditor _display)
		{
			while (i > -1 && actionList.Count > i)
			{
				Action _action = actionList[i];
				
				if (i > 0 && _action.isMarked)
				{
					if (_display == DisplayActionsInEditor.ArrangedVertically)
					{
						_action.nodeRect.x = 14 + (350 * depth);

						// Find top-most Y position
						float yPos = minValue;
						bool doAgain = true;
						
						while (doAgain)
						{
							int numChanged = 0;
							foreach (Action otherAction in actionList)
							{
								if (otherAction != _action && Mathf.Approximately (otherAction.nodeRect.x, _action.nodeRect.x) && otherAction.nodeRect.y >= yPos)
								{
									yPos = otherAction.nodeRect.y + otherAction.nodeRect.height + 30f;
									numChanged ++;
								}
							}
							
							if (numChanged == 0)
							{
								doAgain = false;
							}
						}
						_action.nodeRect.y = yPos;
					}
					else
					{
						_action.nodeRect.y = 14 + (260 * depth);

						// Find left-most X position
						float xPos = minValue + 350;
						bool doAgain = true;
						
						while (doAgain)
						{
							int numChanged = 0;
							foreach (Action otherAction in actionList)
							{
								if (otherAction != _action && Mathf.Approximately (otherAction.nodeRect.x, xPos) && Mathf.Approximately (otherAction.nodeRect.y, _action.nodeRect.y))
								{
									xPos += 350;
									numChanged ++;
								}
							}
							
							if (numChanged == 0)
							{
								doAgain = false;
							}
						}
						_action.nodeRect.x = xPos;
					}
				}
				
				if (_action.isMarked == false)
				{
					return;
				}
				
				_action.isMarked = false;

				float newMinValue = 0f;
				if (_display == DisplayActionsInEditor.ArrangedVertically)
				{
					newMinValue = _action.nodeRect.y + _action.nodeRect.height + 30f;
				}
				else
				{
					newMinValue = _action.nodeRect.x;
				}
				
				if (_action is ActionCheckMultiple)
				{
					ActionCheckMultiple _actionCheckMultiple = (ActionCheckMultiple) _action;
					
					for (int j=_actionCheckMultiple.endings.Count-1; j>=0; j--)
					{
						ActionEnd ending = _actionCheckMultiple.endings [j];
						if (j >= 0)
						{
							if (ending.resultAction == ResultAction.Skip)
							{
								int newDepth = depth;
								for (int k = 0; k<j; k++)
								{
									ActionEnd prevEnding = _actionCheckMultiple.endings [k];
									if (prevEnding.resultAction == ResultAction.Continue || 
									    (prevEnding.resultAction == ResultAction.Skip && prevEnding.skipAction != i))
									{
										newDepth ++;
									}
								}

								ArrangeFromIndex (actionList, actionList.IndexOf (ending.skipActionActual), newDepth, newMinValue, _display);
							}
							else if (ending.resultAction == ResultAction.Continue)
							{
								ArrangeFromIndex (actionList, i+1, depth+j, newMinValue, _display);
							}
						}
					}
				}
				if (_action is ActionParallel)
				{
					ActionParallel _ActionParallel = (ActionParallel) _action;
					
					for (int j=_ActionParallel.endings.Count-1; j>=0; j--)
					{
						ActionEnd ending = _ActionParallel.endings [j];
						if (j >= 0) // Want this to run for all, now
						{
							if (ending.resultAction == ResultAction.Skip)
							{
								ArrangeFromIndex (actionList, actionList.IndexOf (ending.skipActionActual), depth+j, newMinValue, _display);
							}
							else if (ending.resultAction == ResultAction.Continue)
							{
								ArrangeFromIndex (actionList, i+1, depth+j, newMinValue, _display);
							}
						}
						else
						{
							if (ending.resultAction == ResultAction.Skip)
							{
								i = actionList.IndexOf (ending.skipActionActual);
							}
							else if (ending.resultAction == ResultAction.Continue)
							{
								i++;
							}
							else
							{
								i = -1;
							}
						}
					}
				}
				else if (_action is ActionCheck)
				{
					ActionCheck _actionCheck = (ActionCheck) _action;

					if (_actionCheck.resultActionFail == ResultAction.Stop || _actionCheck.resultActionFail == ResultAction.RunCutscene)
					{
						if (_actionCheck.resultActionTrue == ResultAction.Skip)
						{
							i = actionList.IndexOf (_actionCheck.skipActionTrueActual);
						}
						else if (_actionCheck.resultActionTrue == ResultAction.Continue)
						{
							i++;
						}
						else
						{
							i = -1;
						}
					}
					else
					{
						if (_actionCheck.resultActionTrue == ResultAction.Skip)
						{
							ArrangeFromIndex (actionList, actionList.IndexOf (_actionCheck.skipActionTrueActual), depth+1, newMinValue, _display);
						}
						else if (_actionCheck.resultActionTrue == ResultAction.Continue)
						{
							ArrangeFromIndex (actionList, i+1, depth+1, newMinValue, _display);
						}
						
						if (_actionCheck.resultActionFail == ResultAction.Skip)
						{
							i = actionList.IndexOf (_actionCheck.skipActionFailActual);
						}
						else if (_actionCheck.resultActionFail == ResultAction.Continue)
						{
							i++;
						}
						else
						{
							i = -1;
						}
					}
				}
				else
				{
					if (_action.endAction == ResultAction.Skip)
					{
						i = actionList.IndexOf (_action.skipActionActual);
					}
					else if (_action.endAction == ResultAction.Continue)
					{
						i++;
					}
					else
					{
						i = -1;
					}
				}
			}
		}

	}

}