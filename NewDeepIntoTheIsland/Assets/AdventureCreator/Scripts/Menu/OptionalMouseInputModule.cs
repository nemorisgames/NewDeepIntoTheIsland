/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"OptionalMouseInputModule.cs"
 * 
 *	This script is an alternative to the Standalone Input Module that makes mouse input optional.
 *  Code adapted from Vodolazz: http://answers.unity3d.com/questions/1197380/make-standalone-input-module-ignore-mouse-input.html
 */

using UnityEngine.EventSystems;

namespace AC
{

	/**
	 * <summary>This script is an alternative to the Standalone Input Module that makes mouse input optional.
 	 * Code adapted from Vodolazz: http://answers.unity3d.com/questions/1197380/make-standalone-input-module-ignore-mouse-input.html</summary>
	 */
	public class OptionalMouseInputModule : StandaloneInputModule
	{

		private bool allowMouseInput = true;


		public bool AllowMouseInput
		{
			get
			{
				return allowMouseInput;
			}
			set
			{
				allowMouseInput = value;
			}
		}


		private void Update ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.inputMethod == InputMethod.KeyboardOrController)
			{
				if (KickStarter.menuManager.disableMouseIfKeyboardControlling)
				{
					if (KickStarter.playerMenus.IsEventSystemSelectingObject ())
					{
						allowMouseInput = false;
					}
					else
					{
						allowMouseInput = true;
					}
				}
			}
		}


		#if !UNITY_5_0

		public override void Process ()
		{
			bool usedEvent = SendUpdateEventToSelectedObject ();
	 
			if (eventSystem.sendNavigationEvents)
			{
				if (!usedEvent)
				{
					usedEvent |= SendMoveEventToSelectedObject ();
				}
	 
				if (!usedEvent)
				{
					SendSubmitEventToSelectedObject ();
				}
			}

			if (allowMouseInput)
			{
				ProcessMouseEvent ();
			}
		}

		#endif

	}

}