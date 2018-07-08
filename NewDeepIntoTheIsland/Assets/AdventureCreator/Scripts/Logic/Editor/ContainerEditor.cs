using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AC
{

	[CustomEditor(typeof(Container))]
	public class ContainerEditor : Editor
	{

		private Container _target;
		private int itemNumber;
		private int sideItem;
		private InventoryManager inventoryManager;


		public void OnEnable()
		{
			_target = (Container) target;

			if (AdvGame.GetReferences () && AdvGame.GetReferences ().inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}
		}
		
		
		public override void OnInspectorGUI ()
		{
			if (_target == null || inventoryManager == null)
			{
				OnEnable ();
				return;
			}

			ShowCategoriesUI (_target);
			EditorGUILayout.Space ();

			EditorGUILayout.LabelField ("Stored Inventory items", EditorStyles.boldLabel);
			if (_target.items.Count > 0)
			{
				EditorGUILayout.BeginVertical ("Button");
				for (int i=0; i<_target.items.Count; i++)
				{
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Item name:", GUILayout.Width (80f));
					if (inventoryManager.CanCarryMultiple (_target.items[i].linkedID))
					{
						EditorGUILayout.LabelField (inventoryManager.GetLabel (_target.items[i].linkedID), EditorStyles.boldLabel, GUILayout.Width (135f));
						EditorGUILayout.LabelField ("Count:", GUILayout.Width (50f));
						_target.items[i].count = EditorGUILayout.IntField (_target.items[i].count, GUILayout.Width (44f));
					}
					else
					{
						EditorGUILayout.LabelField (inventoryManager.GetLabel (_target.items[i].linkedID), EditorStyles.boldLabel);
						_target.items[i].count = 1;
					}

					if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
					{
						SideMenu (_target.items[i]);
					}

					EditorGUILayout.EndHorizontal ();

					if (_target.limitToCategory && _target.categoryIDs != null && _target.categoryIDs.Count > 0)
					{
						InvItem listedItem = inventoryManager.GetItem (_target.items[i].linkedID);
						if (listedItem != null && !_target.categoryIDs.Contains (listedItem.binID))
					 	{
							EditorGUILayout.HelpBox ("This item is not in the categories checked above and will not be displayed.", MessageType.Warning);
						}
					}

					GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
				}
				EditorGUILayout.EndVertical ();
			}
			else
			{
				EditorGUILayout.HelpBox ("This Container has no items", MessageType.Info);
			}

			EditorGUILayout.Space ();

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Item to create:", GUILayout.MaxWidth (130f));
			itemNumber = EditorGUILayout.Popup (itemNumber, CreateItemList ());
			if (GUILayout.Button ("Add new item"))
			{
				ContainerItem newItem = new ContainerItem (CreateItemID (itemNumber), _target.GetIDArray ());
				_target.items.Add (newItem);
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space ();

			UnityVersionHandler.CustomSetDirty (_target);
		}


		private void ShowCategoriesUI (Container _target)
		{
			EditorGUILayout.BeginVertical ("Button");
			_target.limitToCategory = EditorGUILayout.Toggle ("Limit by category?", _target.limitToCategory);
			if (_target.limitToCategory)
			{
				List<InvBin> bins = AdvGame.GetReferences ().inventoryManager.bins;

				if (bins == null || bins.Count == 0)
				{
					_target.categoryIDs.Clear ();
					EditorGUILayout.HelpBox ("No categories defined!", MessageType.Warning);
				}
				else
				{
					for (int i=0; i<bins.Count; i++)
					{
						bool include = (_target.categoryIDs.Contains (bins[i].id)) ? true : false;
						include = EditorGUILayout.ToggleLeft (" " + i.ToString () + ": " + bins[i].label, include);

						if (include)
						{
							if (!_target.categoryIDs.Contains (bins[i].id))
							{
								_target.categoryIDs.Add (bins[i].id);
							}
						}
						else
						{
							if (_target.categoryIDs.Contains (bins[i].id))
							{
								_target.categoryIDs.Remove (bins[i].id);
							}
						}
					}

					if (_target.categoryIDs.Count == 0)
					{
						EditorGUILayout.HelpBox ("At least one category must be checked for this to take effect.", MessageType.Info);
					}
				}
			}
			EditorGUILayout.EndVertical ();
		}


		private void SideMenu (ContainerItem item)
		{
			GenericMenu menu = new GenericMenu ();
			sideItem = _target.items.IndexOf (item);
			
			if (_target.items.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
			if (sideItem > 0 || sideItem < _target.items.Count-1)
			{
				menu.AddSeparator ("");
			}
			if (sideItem > 0)
			{
				menu.AddItem (new GUIContent ("Move up"), false, Callback, "Move up");
			}
			if (sideItem < _target.items.Count-1)
			{
				menu.AddItem (new GUIContent ("Move down"), false, Callback, "Move down");
			}
			
			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			if (sideItem >= 0)
			{
				ContainerItem tempItem = _target.items[sideItem];
				
				switch (obj.ToString ())
				{
				case "Delete":
					Undo.RecordObject (_target, "Delete item");
					_target.items.RemoveAt (sideItem);
					break;
					
				case "Move up":
					Undo.RecordObject (this, "Move item up");
					_target.items.RemoveAt (sideItem);
					_target.items.Insert (sideItem-1, tempItem);
					break;
					
				case "Move down":
					Undo.RecordObject (this, "Move item down");
					_target.items.RemoveAt (sideItem);
					_target.items.Insert (sideItem+1, tempItem);
					break;
				}
			}
			
			sideItem = -1;
		}
		
		
		private string[] CreateItemList ()
		{
			List<string> itemList = new List<string>();
			
			foreach (InvItem item in inventoryManager.items)
			{
				itemList.Add (item.label);
			}

			return itemList.ToArray ();
		}


		private int CreateItemID (int i)
		{
			return (inventoryManager.items[i].id);
		}

	}

}