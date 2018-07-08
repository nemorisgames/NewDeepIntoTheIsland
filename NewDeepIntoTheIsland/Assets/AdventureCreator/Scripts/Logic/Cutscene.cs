/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"Cutscene.cs"
 * 
 *	This script acts just like an ActionList,
 *	only it is a subclass so that other base classes
 *	(such as Button, DialogOption) cannot be referenced 
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * An ActionList that can run when the scene begins, loads, or whenver it is called from another Action.
	 * A delay can be assigned to it, so that it won't run immediately when called.
	 */
	[System.Serializable]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_cutscene.html")]
	#endif
	public class Cutscene : ActionList
	{

		#if UNITY_EDITOR

		public void CopyFromAsset (ActionListAsset actionListAsset)
		{
			isSkippable = actionListAsset.isSkippable;
			actionListType = actionListAsset.actionListType;
			useParameters = actionListAsset.useParameters;

			// Copy parameters
			parameters = new List<ActionParameter>();
			parameters.Clear ();
			foreach (ActionParameter parameter in actionListAsset.parameters)
			{
				parameters.Add (new ActionParameter (parameter));
			}

			// Actions
			actions = new List<Action>();
			actions.Clear ();

			Vector2 firstPosition = new Vector2 (14f, 14f);
			foreach (Action originalAction in actionListAsset.actions)
			{
				if (originalAction == null)
				{
					continue;
				}

				AC.Action duplicatedAction = Object.Instantiate (originalAction) as AC.Action;
				
				if (actionListAsset.actions.IndexOf (originalAction) == 0)
				{
					duplicatedAction.nodeRect.x = firstPosition.x;
					duplicatedAction.nodeRect.y = firstPosition.y;
				}
				else
				{
					duplicatedAction.nodeRect.x = firstPosition.x + (originalAction.nodeRect.x - firstPosition.x);
					duplicatedAction.nodeRect.y = firstPosition.y + (originalAction.nodeRect.y - firstPosition.y);
				}

				duplicatedAction.ClearIDs ();
				duplicatedAction.isMarked = false;
				duplicatedAction.isAssetFile = false;
				actions.Add (duplicatedAction);
			}
		}

		#endif

	}

}