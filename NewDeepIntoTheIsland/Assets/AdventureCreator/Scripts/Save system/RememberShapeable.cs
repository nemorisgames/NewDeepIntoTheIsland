/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"RememberShapeable.cs"
 * 
 *	This script is attached to shapeable scripts in the scene
 *	with shapekey values we wish to save.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Attach this to Shapeable objects with shapekey values you wish to save.
	 */
	[AddComponentMenu("Adventure Creator/Save system/Remember Shapeable")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_shapeable.html")]
	#endif
	public class RememberShapeable : Remember
	{

		/**
		 * <summary>Serialises appropriate GameObject values into a string.</summary>
		 * <returns>The data, serialised as a string</returns>
		 */
		public override string SaveData ()
		{
			ShapeableData shapeableData = new ShapeableData();
			shapeableData.objectID = constantID;
			shapeableData.savePrevented = savePrevented;

			if (GetComponent <Shapeable>())
			{
				Shapeable shapeable = GetComponent <Shapeable>();
				List<int> activeKeyIDs = new List<int>();
				List<float> values = new List<float>();
				
				foreach (ShapeGroup shapeGroup in shapeable.shapeGroups)
				{
					activeKeyIDs.Add (shapeGroup.GetActiveKeyID ());
					values.Add (shapeGroup.GetActiveKeyValue ());
				}

				shapeableData._activeKeyIDs = ArrayToString <int> (activeKeyIDs.ToArray ());
				shapeableData._values = ArrayToString <float> (values.ToArray ());
			}

			return Serializer.SaveScriptData <ShapeableData> (shapeableData);
		}
		

		/**
		 * <summary>Deserialises a string of data, and restores the GameObject to its previous state.</summary>
		 * <param name = "stringData">The data, serialised as a string</param>
		 */
		public override void LoadData (string stringData)
		{
			ShapeableData data = Serializer.LoadScriptData <ShapeableData> (stringData);
			if (data == null) return;
			SavePrevented = data.savePrevented; if (savePrevented) return;

			if (GetComponent <Shapeable>())
			{
				Shapeable shapeable = GetComponent <Shapeable>();

				int[] activeKeyIDs = StringToIntArray (data._activeKeyIDs);
				float[] values = StringToFloatArray (data._values);

				for (int i=0; i<activeKeyIDs.Length; i++)
				{
					if (values.Length > i)
					{
						shapeable.shapeGroups[i].SetActive (activeKeyIDs[i], values[i], 0f, MoveMethod.Linear, null);
					}
				}
			}
		}
	
	}


	/**
	 * A data container used by the RememberShapeable script.
	 */
	[System.Serializable]
	public class ShapeableData : RememberData
	{

		/** The active ID number of each shape group */
		public string _activeKeyIDs;
		/** The value of each active shape key in each shape group */
		public string _values;

		/**
		 * The default Constructor.
		 */
		public ShapeableData () { }

	}

}
