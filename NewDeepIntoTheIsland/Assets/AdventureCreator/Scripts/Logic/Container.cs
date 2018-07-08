/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"Container.cs"
 * 
 *	This script is used to store a set of
 *	Inventory items in the scene, to be
 *	either taken or added to by the player.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * This component that is used to store a local set of inventory items within a scene.
	 * The items stored here are separate to those held by the player, who can retrieve or place items in here for safe-keeping.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_container.html")]
	#endif
	public class Container : MonoBehaviour
	{

		/** The list of inventory items held by the Container */
		public List<ContainerItem> items = new List<ContainerItem>();
		/** If True, only inventory items (InvItem) with a specific category will be displayed */
		public bool limitToCategory;
		/** The category IDs to limit the display of inventory items by, if limitToCategory = True */
		public List<int> categoryIDs = new List<int>();


		private void Awake ()
		{
			RemoveWrongItems ();
		}


		private void RemoveWrongItems ()
		{
			if (limitToCategory && categoryIDs.Count > 0)
			{
				for (int i=0; i<items.Count; i++)
				{
					InvItem listedItem = KickStarter.inventoryManager.GetItem (items[i].linkedID);
					if (!categoryIDs.Contains (listedItem.binID))
					{
						items.RemoveAt (i);
						i--;
					}
				}
			}
		}


		/**
		 * Activates the Container.  If a Menu with an appearType = AppearType.OnContainer, it will be enabled and show the Container's contents.
		 */
		public void Interact ()
		{
			KickStarter.playerInput.activeContainer = this;
		}


		/**
		 * <summary>Adds an inventory item to the Container's contents.</summary>
		 * <param name = "_id">The ID number of the InvItem to add</param>
		 * <param name = "amount">How many instances of the inventory item to add</param>
		 * <returns>True if the addition was succesful</returns>
		 */
		public bool Add (int _id, int amount)
		{
			// Raise "count" by amount for appropriate ID
			foreach (ContainerItem containerItem in items)
			{
				if (containerItem.linkedID == _id)
				{
					if (KickStarter.inventoryManager.CanCarryMultiple (containerItem.linkedID))
					{
						containerItem.count += amount;
					}
					PlayerMenus.ResetInventoryBoxes ();
					return false;
				}
			}

			// Not already carrying the item
			InvItem itemToAdd = KickStarter.inventoryManager.GetItem (_id);
			if (itemToAdd != null)
			{
				if (limitToCategory && !categoryIDs.Contains (itemToAdd.binID))
				{
					return false;
				}

				if (!itemToAdd.canCarryMultiple)
				{
					amount = 1;
				}

				items.Add (new ContainerItem (_id, amount, GetIDArray ()));
				PlayerMenus.ResetInventoryBoxes ();
				return true;
			}

			PlayerMenus.ResetInventoryBoxes ();
			return true;
		}
		

		/**
		 * <summary>Removes an inventory item from the Container's contents.</summary>
		 * <param name = "_id">The ID number of the InvItem to remove</param>
		 * <param name = "amount">How many instances of the inventory item to remove</param>
		 */
		public void Remove (int _id, int amount)
		{
			// Reduce "count" by 1 for appropriate ID
			
			foreach (ContainerItem item in items)
			{
				if (item.linkedID == _id)
				{
					if (item.count > 0)
					{
						item.count -= amount;
					}
					if (item.count < 1)
					{
						items.Remove (item);
					}
					PlayerMenus.ResetInventoryBoxes ();
					return;
				}
			}
		}


		/**
		 * <summary>Removes all inventory items from the Container's contents.</summary>
		 */
		public void RemoveAll ()
		{
			items.Clear ();
			PlayerMenus.ResetInventoryBoxes ();
		}


		/**
		 * <summary>Gets the number of instances of a particular inventory item stored within the Container.</summary>
		 * <param name = "_id">The ID number of the InvItem to search for</param>
		 * <returns>The number of instances of the inventory item stored within the Container</returns>
		 */
		public int GetCount (int _id)
		{
			foreach (ContainerItem item in items)
			{
				if (item.linkedID == _id)
				{
					return (item.count);
				}
			}
			return 0;
		}


		/**
		 * <summary>Adds an inventory item to the Container's contents, at a particular index.</summary>
		 * <param name = "_item">The InvItem to place within the Container</param>
		 * <param name = "_index">The index number within the Container's current contents to insert the new item</param>
		 * <param name = "count">If >0, the quantity of the item to be added. Otherwise, the same quantity as _item will be added</param>
		 * <returns>The ContainerItem instance of the added item</returns>
		 */
		public ContainerItem InsertAt (InvItem _item, int _index, int count = 0)
		{
			if (limitToCategory && !categoryIDs.Contains (_item.binID))
			{
				return null;
			}

			ContainerItem newContainerItem = new ContainerItem (_item.id, GetIDArray ());

			if (count > 0)
			{
				newContainerItem.count = count;
			}
			else
			{
				newContainerItem.count = _item.count;
			}
			if (_index < items.Count)
			{
				if (items[_index] != null && items[_index].linkedID == _item.id)
				{
					// Same item in the slot, so just add instead
					newContainerItem.count += items[_index].count;
					items[_index] = newContainerItem;
				}
				else
				{
					items.Insert (_index, newContainerItem);
				}
			}
			else
			{
				items.Add (newContainerItem);
			}

			PlayerMenus.ResetInventoryBoxes ();
			return newContainerItem;
		}


		/**
		 * <summmary>Gets an array of ID numbers of existing ContainerItem classes, so that a unique number can be generated.</summary>
		 * <returns>Gets an array of ID numbers of existing ContainerItem classes</returns>
		 */
		public int[] GetIDArray ()
		{
			List<int> idArray = new List<int>();
			
			foreach (ContainerItem item in items)
			{
				idArray.Add (item.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}


		#if UNITY_EDITOR

		public int GetInventoryReferences (int invID)
		{
			int numFound = 0;
			if (items != null)
			{
				foreach (ContainerItem item in items)
				{
					if (item.linkedID == invID)
					{
						numFound ++;
					}
				}
			}
			return numFound;
		}

		#endif

	}

}