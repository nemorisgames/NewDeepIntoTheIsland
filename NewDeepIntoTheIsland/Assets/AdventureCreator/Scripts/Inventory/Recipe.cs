/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"Recipe.cs"
 * 
 *	This script is a container class for recipes.
 * 
 */

using System.Collections.Generic;

namespace AC
{

	/**
	 * A data container for a recipe.
	 * A recipe requires multiple inventory items, optionally arranged in a specific pattern, and replaces them with a new inventory item.
	 */
	[System.Serializable]
	public class Recipe
	{

		/** The recipe's editor name */
		public string label;
		/** A unique identifier */
		public int id;
		/** If True, the recipe is being edited in InventoryManager */
		public bool isEditing = false;
		/** If True, then the output ingredient will appear automatically when the correct ingredients are used. If False, then the player will have to run the "Inventory: Crafting" Action as an additional step. */
		public bool autoCreate = true;
		/** The required ingredients */
		public List<Ingredient> ingredients = new List<Ingredient>();
		/** The ID number of the associated inventory item (InvItem) created when the recipe is complete */
		public int resultID;
		/** If True, then the ingredients must be placed in specific slots within a MenuCrafting element */
		public bool useSpecificSlots;
		/** What happens when the recipe is created (JustMoveToInventory, SelectItem, RunActionList) */
		public OnCreateRecipe onCreateRecipe = OnCreateRecipe.JustMoveToInventory;
		/** The ActionListAsset to run, if onCreateRecipe = OnCreateRecipe.RunActionList */
		public ActionListAsset invActionList;
		/** The ActionListAsset to run when the recipe is created */
		public ActionListAsset actionListOnCreate;


		/**
		 * <summary>The default Constructor.</summary>
		 * <param name = "idArray">An array of already-used ID numbers, so that a unique one can be generated</param>
		 */
		public Recipe (int[] idArray)
		{
			isEditing = false;
			ingredients = new List<Ingredient>();
			resultID = 0;
			useSpecificSlots = false;
			invActionList = null;
			autoCreate = true;
			onCreateRecipe = OnCreateRecipe.JustMoveToInventory;
			actionListOnCreate = null;

			// Update id based on array
			foreach (int _id in idArray)
			{
				if (id == _id)
					id ++;
			}
			
			label = "Recipe " + (id + 1).ToString ();
		}

	}


	/**
	 * A data container for an ingredient within a Recipe.
	 * If multiple instances of the associated inventory item can be carried, then the amount needed of that item can also be set.
	 */
	[System.Serializable]
	public class Ingredient
	{

		/** The ID number of the associated inventory item (InvItem) */
		public int itemID;
		/** The amount needed, if the InvItem's canCarryMultiple = True */
		public int amount;
		/** The recipe's required slot, if the Recipe's useSpecificSlots = True */
		public int slotNumber;


		/**
		 * The default Constructor.
		 */
		public Ingredient ()
		{
			itemID = 0;
			amount = 1;
			slotNumber = 1;
		}

	}

}