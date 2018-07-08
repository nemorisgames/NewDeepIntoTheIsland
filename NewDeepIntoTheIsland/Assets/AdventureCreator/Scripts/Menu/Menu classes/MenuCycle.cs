/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"MenuCycle.cs"
 * 
 *	This MenuElement is like a label, only its text cycles through an array when clicked on.
 * 
 */

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * A MenuElement that displays different text each time it is clicked on.
	 * The index number of the text array it displays can be linked to a Global Variable (GVar), a custom script, or the game's current language.
	 */
	public class MenuCycle : MenuElement
	{

		/** The Unity UI Button this is linked to (Unity UI Menus only) */
		public UnityEngine.UI.Button uiButton;

		#if UNITY_5_3_OR_NEWER
		/** The Unity UI Dropdown this is linked to (Unity UI Menus only) */
		public Dropdown uiDropdown;
		#endif

		/** The ActionListAsset to run when the element is clicked on */
		public ActionListAsset actionListOnClick = null;
		/** The text that's displayed on-screen, which prefixes the varying text */
		public string label = "Element";
		/** The special FX applied to the text (None, Outline, Shadow, OutlineAndShadow) */
		public TextEffects textEffects;
		/** The outline thickness, if textEffects != TextEffects.None */
		public float outlineSize = 2f;
		/** The text alignment */
		public TextAnchor anchor;
		/** The index number of the currently-shown text in optionsArray */
		public int selected;
		/** An array of texts that the element can show one at a time */
		public List<string> optionsArray = new List<string>();
		/** What the text links to (CustomScript, GlobalVariable, Language) */
		public AC_CycleType cycleType;
		/** The ID number of the linked GlobalVariable, if cycleType = AC_CycleType.GlobalVariable */
		public int varID;
		/** The method which this element are hidden from view when made invisible (DisableObject, DisableInteractability) */
		public UISelectableHideStyle uiSelectableHideStyle = UISelectableHideStyle.DisableObject;
		/** What kind of UI element the Cycle is linked to, if the Menu's Source is UnityUiPrefab or UnityUiInScene (Button, Dropdown) */
		public CycleUIBasis cycleUIBasis = CycleUIBasis.Button;

		private Text uiText;
		private string cycleText;
		private Menu parentMenu;


		/**
		 * Initialises the element when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiButton = null;
			uiText = null;
			label = "Cycle";
			selected = 0;
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			SetSize (new Vector2 (15f, 5f));
			anchor = TextAnchor.MiddleLeft;
			cycleType = AC_CycleType.CustomScript;
			varID = 0;
			optionsArray = new List<string>();
			cycleText = "";
			actionListOnClick = null;
			uiSelectableHideStyle = UISelectableHideStyle.DisableObject;
			cycleUIBasis = CycleUIBasis.Button;

			#if UNITY_5_3_OR_NEWER
			uiDropdown = null;
			#endif
			
			base.Declare ();
		}


		public override MenuElement DuplicateSelf (bool fromEditor, bool ignoreUnityUI)
		{
			MenuCycle newElement = CreateInstance <MenuCycle>();
			newElement.Declare ();
			newElement.CopyCycle (this, ignoreUnityUI);
			return newElement;
		}
		
		
		private void CopyCycle (MenuCycle _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiButton = null;
			}
			else
			{
				uiButton = _element.uiButton;
			}
			uiText = null;

			label = _element.label;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			anchor = _element.anchor;
			selected = _element.selected;
			optionsArray = _element.optionsArray;
			cycleType = _element.cycleType;
			varID = _element.varID;
			cycleText = "";
			actionListOnClick = _element.actionListOnClick;
			uiSelectableHideStyle = _element.uiSelectableHideStyle;
			cycleUIBasis = _element.cycleUIBasis;

			#if UNITY_5_3_OR_NEWER
			uiDropdown = _element.uiDropdown;
			#endif

			base.Copy (_element);
		}


		/**
		 * <summary>Initialises the linked Unity UI GameObject.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 */
		public override void LoadUnityUI (AC.Menu _menu, Canvas canvas)
		{
			if (_menu.menuSource != MenuSource.AdventureCreator)
			{
				if (cycleUIBasis == CycleUIBasis.Button)
				{
					uiButton = LinkUIElement <UnityEngine.UI.Button> (canvas);
					if (uiButton)
					{
						if (uiButton.GetComponentInChildren <Text>())
						{
							uiText = uiButton.GetComponentInChildren <Text>();
						}
						uiButton.onClick.AddListener (() => {
							ProcessClickUI (_menu, 0, KickStarter.playerInput.GetMouseState ());
						});
					}
				}
				else if (cycleUIBasis == CycleUIBasis.Dropdown)
				{
					#if UNITY_5_3_OR_NEWER
					if (uiDropdown != null)
					{
						parentMenu = _menu;
						uiDropdown = LinkUIElement <Dropdown> (canvas);
						uiDropdown.value = selected;
						uiDropdown.onValueChanged.AddListener (delegate {
	         				uiDropdownValueChangedHandler (uiDropdown);
	     				});
	     			}
     				#endif
				}
			}
		}


		#if UNITY_5_3_OR_NEWER
		private void uiDropdownValueChangedHandler (Dropdown _dropdown)
		{
			ProcessClickUI (parentMenu, 0, KickStarter.playerInput.GetMouseState ());
		}
		#endif


		/**
		 * <summary>Gets the boundary of the element</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <returns>The boundary Rect of the element</returns>
		 */
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiButton)
			{
				return uiButton.GetComponent <RectTransform>();
			}
			return null;
		}


		public override void SetUIInteractableState (bool state)
		{
			if (uiButton)
			{
				uiButton.interactable = state;
			}
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (Menu menu)
		{
			string apiPrefix = "(AC.PlayerMenus.GetElementWithName (\"" + menu.title + "\", \"" + title + "\") as AC.MenuCycle)";

			MenuSource source = menu.menuSource;
			EditorGUILayout.BeginVertical ("Button");

			if (source != AC.MenuSource.AdventureCreator)
			{
				cycleUIBasis = (CycleUIBasis) CustomGUILayout.EnumPopup ("UI basis:", cycleUIBasis, apiPrefix + ".cycleUIBasis");

				if (cycleUIBasis == CycleUIBasis.Button)
				{
					uiButton = LinkedUiGUI <UnityEngine.UI.Button> (uiButton, "Linked Button:", source);
				}
				else if (cycleUIBasis == CycleUIBasis.Dropdown)
				{
					#if UNITY_5_3_OR_NEWER
					uiDropdown = LinkedUiGUI <Dropdown> (uiDropdown, "Linked Dropdown:", source);
					#else
					EditorGUILayout.HelpBox ("This option is only available with Unity 5.4 or newer.", MessageType.Info);
					#endif
				}
				uiSelectableHideStyle = (UISelectableHideStyle) CustomGUILayout.EnumPopup ("When invisible:", uiSelectableHideStyle, apiPrefix + ".uiSelectableHideStyle");
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}

			cycleType = (AC_CycleType) CustomGUILayout.EnumPopup ("Cycle type:", cycleType, apiPrefix + ".cycleType");

			if (source == MenuSource.AdventureCreator || cycleUIBasis == CycleUIBasis.Button)
			{
				label = CustomGUILayout.TextField ("Label text:", label, apiPrefix + ".label");
			}

			if (cycleType == AC_CycleType.CustomScript || cycleType == AC_CycleType.Variable)
			{
				int numOptions = optionsArray.Count;
				numOptions = EditorGUILayout.IntField ("Number of choices:", optionsArray.Count);
				if (numOptions < 0)
				{
					numOptions = 0;
				}
				
				if (numOptions < optionsArray.Count)
				{
					optionsArray.RemoveRange (numOptions, optionsArray.Count - numOptions);
				}
				else if (numOptions > optionsArray.Count)
				{
					if(numOptions > optionsArray.Capacity)
					{
						optionsArray.Capacity = numOptions;
					}
					for (int i=optionsArray.Count; i<numOptions; i++)
					{
						optionsArray.Add ("");
					}
				}
				
				for (int i=0; i<optionsArray.Count; i++)
				{
					optionsArray [i] = CustomGUILayout.TextField ("Choice #" + i.ToString () + ":", optionsArray [i], apiPrefix + ".optionsArray[" + i.ToString () + "]");
				}
				
				if (cycleType == AC_CycleType.CustomScript)
				{
					if (optionsArray.Count > 0)
					{
						selected = CustomGUILayout.IntField ("Default option #:", selected, apiPrefix + ".selected");
					}
					ShowClipHelp ();
				}
				else if (cycleType == AC_CycleType.Variable)
				{
					varID = AdvGame.GlobalVariableGUI ("Global integer var:", varID, VariableType.Integer);
				}

				actionListOnClick = (ActionListAsset) CustomGUILayout.ObjectField <ActionListAsset> ("ActionList on click:", actionListOnClick, false, apiPrefix + ".actionListOnClick");
			}
			alternativeInputButton = CustomGUILayout.TextField ("Alternative input button:", alternativeInputButton, apiPrefix + ".alternativeInputButton");
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (menu);
		}


		protected override void ShowTextGUI (string apiPrefix)
		{
			anchor = (TextAnchor) CustomGUILayout.EnumPopup ("Text alignment:", anchor, apiPrefix + ".anchor");
			textEffects = (TextEffects) CustomGUILayout.EnumPopup ("Text effect:", textEffects, apiPrefix + ".textEffects");
			if (textEffects != TextEffects.None)
			{
				outlineSize = CustomGUILayout.Slider ("Effect size:", outlineSize, 1f, 5f, apiPrefix + ".outlineSize");
			}
		}


		public override bool CheckConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID)
		{
			if (cycleType == AC_CycleType.Variable && varID == oldGlobalID)
			{
				return true;
			}
			return false;
		}


		public override int GetVariableReferences (int _varID)
		{
			int numFound = 0;
			string tokenText = "[var:" + _varID.ToString () + "]";
			if (label.Contains (tokenText))
			{
				numFound ++;
			}

			if (cycleType == AC_CycleType.Variable && varID == _varID)
			{
				numFound ++;
			}

			if (cycleType == AC_CycleType.CustomScript || cycleType == AC_CycleType.Variable)
			{
				foreach (string optionLabel in optionsArray)
				{
					if (optionLabel.Contains (tokenText))
					{
						numFound ++;
					}
				}
			}

			return numFound + base.GetVariableReferences (_varID);
		}
		
		#endif
		

		/**
		 * <summary>Performs all calculations necessary to display the element.</summary>
		 * <param name = "_slot">The index number of the slot to display</param>
		 * <param name = "languageNumber">The index number of the language to display text in</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			CalculateValue ();

			cycleText = TranslateLabel (label, languageNumber) + " : ";

			#if UNITY_5_3_OR_NEWER
			if (Application.isPlaying && uiDropdown != null)
			{
				cycleText = "";
			}
			#endif
			
			if (Application.isPlaying)
			{
				if (optionsArray.Count > selected && selected > -1)
				{
					cycleText += optionsArray [selected];
				}
				else
				{
					ACDebug.Log ("Could not gather options for MenuCycle " + label);
					selected = 0;
				}
			}
			else if (optionsArray.Count > 0)
			{
				if (selected >= 0 && selected < optionsArray.Count)
				{
					cycleText += optionsArray [selected];
				}
				else
				{
					cycleText += optionsArray [0];
				}
			}
			else
			{
				cycleText += "Default option";	
			}

			if (uiButton)
			{
				if (uiText)
				{
					uiText.text = cycleText;
				}
				UpdateUISelectable (uiButton, uiSelectableHideStyle);
			}
			else
			{
				#if UNITY_5_3_OR_NEWER
				if (uiDropdown != null && Application.isPlaying)
				{
					uiDropdown.value = selected;
					UpdateUISelectable (uiDropdown, uiSelectableHideStyle);
				}
				#endif
			}
		}
		

		/**
		 * <summary>Draws the element using OnGUI.</summary>
		 * <param name = "_style">The GUIStyle to draw with</param>
		 * <param name = "_slot">The index number of the slot to display</param>
		 * <param name = "zoom">The zoom factor</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}

			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), cycleText, _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), cycleText, _style);
			}
		}
		

		/**
		 * <summary>Gets the display text of the element</summary>
		 * <param name = "slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language number to get the text in</param>
		 * <returns>The display text of the element's slot, or the whole element if it only has one slot</returns>
		 */
		public override string GetLabel (int slot, int languageNumber)
		{
			if (optionsArray.Count > selected && selected > -1)
			{
				return TranslateLabel (label, languageNumber) + " : " + optionsArray [selected];
			}
			
			return TranslateLabel (label, languageNumber);
		}


		public override bool IsSelectedByEventSystem (int slotIndex)
		{
			if (uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject (uiButton.gameObject);
			}
			return false;
		}
		

		/**
		 * <summary>Performs what should happen when the element is clicked on.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "_mouseState">The state of the mouse button</param>
		 */
		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (!_menu.IsClickable ())
			{
				return;
			}

			base.ProcessClick (_menu, _slot, _mouseState);

			#if UNITY_5_3_OR_NEWER
			if (uiDropdown != null)
			{
				selected = uiDropdown.value;
			}
			else
			{
				selected ++;
				if (selected > optionsArray.Count-1)
				{
					selected = 0;
				}
			}
			#else
			selected ++;
			if (selected > optionsArray.Count-1)
			{
				selected = 0;
			}
			#endif

			if (cycleType == AC_CycleType.Language)
			{
				if (selected == 0 && KickStarter.speechManager.ignoreOriginalText && KickStarter.runtimeLanguages.Languages.Count > 1)
				{
					// Ignore original text by skipping to first language
					selected = 1;
				}
				Options.SetLanguage (selected);
			}
			else if (cycleType == AC_CycleType.Variable)
			{
				if (varID >= 0)
				{
					GVar var = GlobalVariables.GetVariable (varID);
					if (var.type == VariableType.Integer)
					{
						var.val = selected;
						var.Upload ();
					}
				}
			}

			if (cycleType == AC_CycleType.CustomScript)
			{
				MenuSystem.OnElementClick (_menu, this, _slot, (int) _mouseState);
			}

			if (actionListOnClick)
			{
				AdvGame.RunActionListAsset (actionListOnClick);
			}
		}


		#if UNITY_5_3_OR_NEWER
		public override void RecalculateSize (MenuSource source)
		{
			if (Application.isPlaying && uiDropdown != null)
			{
				if (optionsArray.Count > selected && selected > -1 && uiDropdown.itemText != null)
				{
					uiDropdown.captionText.text = optionsArray[selected];
				}

				for (int i=0; i<optionsArray.Count; i++)
				{
					if (uiDropdown.options.Count > i && uiDropdown.options[i] != null)
					{
						uiDropdown.options[i].text = optionsArray[i];
					}
				}
			}
			base.RecalculateSize (source);
		}
		#endif


		private void CalculateValue ()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			if (cycleType == AC_CycleType.Language)
			{
				if (Application.isPlaying)
				{
					optionsArray = KickStarter.runtimeLanguages.Languages;
				}
				else
				{
					optionsArray = AdvGame.GetReferences ().speechManager.languages;
				}

				if (Options.optionsData != null)
				{
					selected = Options.optionsData.language;
				}
			}
			else if (cycleType == AC_CycleType.Variable)
			{
				if (varID >= 0)
				{
					if (GlobalVariables.GetVariable (varID) == null || GlobalVariables.GetVariable (varID).type != VariableType.Integer)
					{
						ACDebug.LogWarning ("Cannot link MenuToggle " + title + " to Variable " + varID + " as it is not an Integer.");
					}
					else if (optionsArray.Count > 0)
					{
						selected = Mathf.Clamp (GlobalVariables.GetIntegerValue (varID), 0, optionsArray.Count - 1);
					}
					else
					{
						selected = 0;
					}
				}
			}
		}


		protected override void AutoSize ()
		{
			AutoSize (new GUIContent (TranslateLabel (label, Options.GetLanguage ()) + " : Default option"));
		}
		
	}
	
}