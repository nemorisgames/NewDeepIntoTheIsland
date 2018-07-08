/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionInventoryCrafting.cs"
 * 
 *	This action is used to perform crafting-related tasks.
 * 
 */

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionInventoryCrafting : Action
	{

		public enum ActionCraftingMethod { ClearRecipe, CreateRecipe };
		public ActionCraftingMethod craftingMethod;


		public ActionInventoryCrafting ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Crafting";
			description = "Either clears the current arrangement of crafting ingredients, or evaluates them to create an appropriate result (if this is not done automatically by the recipe itself).";
		}

		
		override public float Run ()
		{
			if (craftingMethod == ActionCraftingMethod.ClearRecipe)
			{
				KickStarter.runtimeInventory.RemoveRecipes ();
			}
			else if (craftingMethod == ActionCraftingMethod.CreateRecipe)
			{
				PlayerMenus.CreateRecipe ();
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			craftingMethod = (ActionCraftingMethod) EditorGUILayout.EnumPopup ("Method:", craftingMethod);

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			if (craftingMethod == ActionCraftingMethod.CreateRecipe)
			{
				return (" (Create recipe)");
			}
			else if (craftingMethod == ActionCraftingMethod.ClearRecipe)
			{
				return (" (Clear recipe)");
			}
			return "";
		}
		
		#endif
		
	}

}