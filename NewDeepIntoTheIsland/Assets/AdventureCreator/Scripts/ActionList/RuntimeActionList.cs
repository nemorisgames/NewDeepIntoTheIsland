/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"RuntimeActionList.cs"
 * 
 *	This is a special derivative of ActionList.
 *	It is used to run ActionList assets, which are assets defined outside of the scene.
 *	This type of asset's actions are copied here and run locally.
 *	When a ActionList asset is copied is copied from a menu, the menu it is called from is recorded, so that the game returns
 *	to the appropriate state after running.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * An ActionList subclass used to run ActionListAssets, which exist in asset files outside of the scene.
	 * When an ActionListAsset is run, its Actions are copied to a new RuntimeActionList and run locally.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_runtime_action_list.html")]
	#endif
	public class RuntimeActionList : ActionList
	{

		/** The ActionListAsset that this ActionList's Actions are copied from */
		public ActionListAsset assetSource;


		/**
		 * <summary>Downloads and runs the settings and Actions stored within an ActionListAsset.</summary>
		 * <param name = "actionListAsset">The ActionListAsset to copy Actions from and run</param>
		 * <param name = "endConversation">If set, the supplied Conversation will be run when the AcionList ends</param>
		 * <param name = "i">The index number of the first Action to run</param>
		 * <param name = "doSkip">If True, then the Actions will be skipped, instead of run normally</param>
		 * <param name = "addToSkipQueue">If True, the ActionList will be skippable when the user presses 'EndCutscene'</param>
		 * <param name = "dontRun">If True, the Actions will not be run once transferred from the ActionListAsset</param>
		 */
		public void DownloadActions (ActionListAsset actionListAsset, Conversation endConversation, int i, bool doSkip, bool addToSkipQueue, bool dontRun = false)
		{
			//this.name = actionListAsset.name;
			assetSource = actionListAsset;
			useParameters = actionListAsset.useParameters;
			//parameters = actionListAsset.parameters;

			parameters = new List<ActionParameter>();
			if (useParameters && actionListAsset.parameters != null)
			{
				foreach (ActionParameter assetParameter in actionListAsset.parameters)
				{
					parameters.Add (new ActionParameter (assetParameter, true));
				}
			}

			unfreezePauseMenus = actionListAsset.unfreezePauseMenus;

			actionListType = actionListAsset.actionListType;
			if (actionListAsset.actionListType == ActionListType.PauseGameplay)
			{
				isSkippable = actionListAsset.isSkippable;
			}
			else
			{
				isSkippable = false;
			}

			conversation = endConversation;
			actions.Clear ();
			
			foreach (AC.Action action in actionListAsset.actions)
			{
				ActionEnd _lastResult = action.lastResult;
				
				actions.Add (action);
				
				if (doSkip && action != null)
				{
					actions[actions.Count-1].lastResult = _lastResult;
				}
			}

			if (!useParameters)
			{
				foreach (Action action in actions)
				{
					action.AssignValues (null);
				}
			}

			if (!dontRun)
			{
				if (doSkip)
				{
					Skip (i);
				}
				else
				{
					Interact (i, addToSkipQueue);
				}
			}
		}


		protected override void BeginActionList (int i, bool addToSkipQueue)
		{
			if (KickStarter.actionListManager)
			{
				KickStarter.actionListAssetManager.AddToList (this, assetSource, addToSkipQueue, i);
				ProcessAction (i);
			}
			else
			{
				ACDebug.LogWarning ("Cannot run " + this.name + " because no ActionListManager was found.");
			}
		}


		protected override void AddResumeToManager (int startIndex)
		{
			if (KickStarter.actionListAssetManager == null)
			{
				ACDebug.LogWarning ("Cannot run " + this.name + " because no ActionListAssetManager was found.");
				return;
			}
			KickStarter.actionListAssetManager.AddToList (this, assetSource, true, startIndex);
		}


		/**
		 * Stops the Actions from running and sets the gameState in StateHandler to the correct value.
		 */
		public override void Kill ()
		{
			StopCoroutine ("PauseUntilStart");
			StopCoroutine ("RunAction");
			StopCoroutine ("EndCutscene");
			
			KickStarter.actionListAssetManager.EndAssetList (this);
		}


		/**
		 * Destroys itself.
		 */
		public void DestroySelf ()
		{
			Destroy (this.gameObject);
		}


		protected new void ReturnLastResultToSource (ActionEnd _lastResult, int i)
		{
			assetSource.actions[i].lastResult = _lastResult;
		}


		protected override void FinishPause ()
		{
			KickStarter.actionListAssetManager.AssignResumeIndices (assetSource, resumeIndices.ToArray ());
			CheckEndCutscene ();
		}
	
	}

}
