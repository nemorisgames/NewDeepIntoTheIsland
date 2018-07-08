
/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"InvVar.cs"
 * 
 *	This script is a data class for inventory properties.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * A data class for inventory properties.
	 * Properties are created in the Inventory Manager asset file, and stored in each instance of InvItem class.
	 * Inventory items held by the player during gameplay are stored in the localItems List within RuntimeInventory.
	 */
	[System.Serializable]
	public class InvVar : GVar
	{

		/** If True, then the property will be limited to inventory items within certain categories */
		public bool limitToCategories = false;
		/** A List of category IDs that the property belongs to, if limitToCategories = True.  Categories are stored within InventoryManager's bins variable */
		public List<int> categoryIDs = new List<int>();


		/**
		 * The main Constructor.
		 * An array of ID numbers is required, to ensure its own ID is unique.
		 */
		public InvVar (int[] idArray)
		{
			val = 0;
			floatVal = 0f;
			textVal = "";
			type = VariableType.Boolean;
			id = 0;
			popUps = null;
			textValLineID = -1;
			popUpsLineID = -1;
			vector3Val = Vector3.zero;

			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
				{
					id ++;
				}
			}
			
			label = "Property " + (id + 1).ToString ();
		}


		/**
		 * A blank Constructor.
		 */
		public InvVar ()
		{
			val = 0;
			floatVal = 0f;
			textVal = "";
			type = VariableType.Boolean;
			id = 0;
			popUps = null;
			textValLineID = -1;
			popUpsLineID = -1;
			label = "";
			vector3Val = Vector3.zero;
		}


		/**
		 * A Constructor that copies all values from another variable.
		 * This way ensures that no connection remains to the asset file.
		 */
		public InvVar (InvVar assetVar)
		{
			val = assetVar.val;
			floatVal = assetVar.floatVal;
			textVal = assetVar.textVal;
			type = assetVar.type;
			id = assetVar.id;
			label = assetVar.label;
			link = assetVar.link;
			pmVar = assetVar.pmVar;
			popUps = assetVar.popUps;
			updateLinkOnStart = assetVar.updateLinkOnStart;
			categoryIDs = assetVar.categoryIDs;
			limitToCategories = assetVar.limitToCategories;
			textValLineID = assetVar.textValLineID;
			popUpsLineID = assetVar.popUpsLineID;
			vector3Val = assetVar.vector3Val;
		}


		/**
		 * <summary>Updates the 'value' variables specific to an inventory item based on another InvVar instance.</summary>
		 * <param name = "invVar">The other InvVar to copy 'value' variables from</param>
		 */
		public void TransferValues (InvVar invVar)
		{
			val = invVar.val;
			floatVal = invVar.floatVal;
			textVal = invVar.textVal;
			textValLineID = invVar.textValLineID;
			vector3Val = invVar.vector3Val;
		}


		/**
		 * <summary>Gets the property's value as a string.</summary>
		 * <param name = "languageNumber">The index number of the game's current language (0 = original)</param>
		 * <returns>The property's value as a string</returns>
		 */
		public string GetDisplayValue (int languageNumber = 0)
		{
			if (type == VariableType.Integer)
			{
				return val.ToString ();
			}
			else if (type == VariableType.Float)
			{
				return floatVal.ToString ();
			}
			else if (type == VariableType.Boolean)
			{
				if (val == 1)
				{
					return "True";
				}
				return "False";
			}
			else if (type == VariableType.PopUp)
			{
				if (val >= 0 && popUps.Length > val)
				{
					if (languageNumber > 0)
					{
						string popUpsString = KickStarter.runtimeLanguages.GetTranslation (GetPopUpsString (), popUpsLineID, languageNumber);
						string[] popUpsNew = popUpsString.Split ("]"[0]);
						if (popUpsNew.Length < val)
						{
							return popUpsNew[val];
						}
					}
					return popUps[val];
				}
			}
			else if (type == VariableType.String)
			{
				if (languageNumber > 0)
				{
					return KickStarter.runtimeLanguages.GetTranslation (textVal, textValLineID, languageNumber);
				}
				return textVal;
			}
			else if (type == VariableType.Vector3)
			{
				return "(" + vector3Val.x.ToString () + ", " + vector3Val.y.ToString () + ", " + vector3Val.z.ToString () + ")";
			}
			return "";
		}

	}

}