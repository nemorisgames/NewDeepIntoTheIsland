/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionParameter.cs"
 * 
 *	This defines a parameter that can be used by ActionLists
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * A data container for an ActionList parameter. A parameter can change the value of an Action's public variables dynamically during gameplay, allowing the same Action to be repurposed for different tasks.
	 */
	[System.Serializable]
	public class ActionParameter
	{

		/** The display name in the Editor */
		public string label = "";
		/** A unique identifier */
		public int ID = 0;
		/** The type of variable it overrides (GameObject, InventoryItem, GlobalVariable, LocalVariable, String, Float, Integer, Boolean, Vector3) */
		public ParameterType parameterType = ParameterType.GameObject;
		/** The new value or ID number, if parameterType = ParameterType.Integer / Boolean / LocalVariable / GlobalVariable / InventoryItem.  If parameterType = ParameterType.GameObject, it is the ConstantID number of the GameObject if it is not currently accessible */
		public int intValue = -1;
		/** The new value, if parameterType = ParameterType.Float */
		public float floatValue = 0f;
		/** The new value, if parameterType = ParameterType.String */
		public string stringValue = "";
		/** The new value, if parameterType = ParameterType.GameObject */
		public GameObject gameObject;
		/** The new value, if parameterType = ParameterType.UnityObject */
		public Object objectValue;
		/** The new value, if parameterType = ParameterType.Vector3 */
		public Vector3 vector3Value;


		/**
		 * <summary>A Constructor that generates a unique ID number.</summary>
		 * <param name = "idArray">An array of previously-used ID numbers, to ensure its own ID is unique.</param>
		 */
		public ActionParameter (int[] idArray)
		{
			label = "";
			ID = 0;
			intValue = -1;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
			objectValue = null;
			parameterType = ParameterType.GameObject;
			vector3Value = Vector3.zero;
			
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (ID == _id)
					ID ++;
			}
			
			label = "Parameter " + (ID + 1).ToString ();
		}


		/**
		 * <summary>A Constructor that sets the ID number explicitly.</summary>
		 * <param name = "id">The unique identifier to assign</param>
		 */
		public ActionParameter (int id)
		{
			label = "";
			ID = id;
			intValue = -1;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
			objectValue = null;
			parameterType = ParameterType.GameObject;
			vector3Value = Vector3.zero;
			
			label = "Parameter " + (ID + 1).ToString ();
		}


		/**
		 * <summary>A Constructor that duplicates another ActionParameter.</summary>
		 */
		public ActionParameter (ActionParameter _actionParameter, bool alsoCopyValues = false)
		{
			label = _actionParameter.label;
			ID = _actionParameter.ID;
			parameterType = _actionParameter.parameterType;

			if (alsoCopyValues)
			{
				intValue = _actionParameter.intValue;
				floatValue = _actionParameter.floatValue;
				stringValue = _actionParameter.stringValue;
				gameObject = _actionParameter.gameObject;
				objectValue = _actionParameter.objectValue;
				vector3Value = _actionParameter.vector3Value;
			}
			else
			{
				intValue = -1;
				floatValue = 0f;
				stringValue = "";
				gameObject = null;
				objectValue = null;
				vector3Value = Vector3.zero;
			}
		}


		/**
		 * <summary>Copies the "value" variables from another ActionParameter, without changing the type, ID, or label.</summary>
		 * <parameter name = "otherParameter">The ActionParameter to copy from</param>
		 */
		public void CopyValues (ActionParameter otherParameter)
		{
			intValue = otherParameter.intValue;
			floatValue = otherParameter.floatValue;
			stringValue = otherParameter.stringValue;
			gameObject = otherParameter.gameObject;
			objectValue = otherParameter.objectValue;
			vector3Value = otherParameter.vector3Value;
		}


		/**
		 * Resets the value that the parameter assigns.
		 */
		public void Reset ()
		{
			intValue = -1;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
		}


		/**
		 * <summary>Checks if the parameter's value is an integer. This is the case if parameterType = ParameterType.GameObject, GlobalVariable, Integer, InventoryItem or LocalVariable.</summary>
		 * <returns>True if the parameter's value is an integer.</returns>
		 */
		public bool IsIntegerBased ()
		{
			if (parameterType == ParameterType.GameObject ||
			    parameterType == ParameterType.GlobalVariable ||
			    parameterType == ParameterType.Integer ||
				parameterType == ParameterType.Boolean ||
			    parameterType == ParameterType.InventoryItem ||
			    parameterType == ParameterType.LocalVariable)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Sets the intValue that the parameter assigns</summary>
		 * <param name = "_value">The new value or ID number, if parameterType = ParameterType.Integer / Boolean / LocalVariable / GlobalVariable / InventoryItem.  If parameterType = ParameterType.GameObject, it is the ConstantID number of the GameObject if it is not currently accessible</param>
		 */
		public void SetValue (int _value)
		{
			intValue = _value;
			floatValue = 0f;
			stringValue = "";
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
		}


		/**
		 * <summary>Sets the floatValue that the parameter assigns</summary>
		 * <param name = "_value">The new value, if parameterType = ParameterType.Float</param>
		 */
		public void SetValue (float _value)
		{
			floatValue = _value;
			stringValue = "";
			intValue = -1;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
		}


		/**
		 * <summary>Sets the stringValue that the parameter assigns</summary>
		 * <param name = "_value">The new value, if parameterType = ParameterType.String</param>
		 */
		public void SetValue (string _value)
		{
			stringValue = AdvGame.ConvertTokens (_value);
			floatValue = 0f;
			intValue = -1;
			gameObject = null;
			objectValue = null;
			vector3Value = Vector3.zero;
		}


		/**
		 * <summary>Sets the vector3Value that the parameter assigns</summary>
		 * <param name = "_value">The new value, if parameterType = ParameterType.Vector3</param>
		 */
		public void SetValue (Vector3 _value)
		{
			stringValue = "";
			floatValue = 0f;
			intValue = -1;
			gameObject = null;
			objectValue = null;
			vector3Value = _value;
		}


		/**
		 * <summary>Sets the gameObject that the parameter assigns</summary>
		 * <param name = "_object">The new GameObject, if parameterType = ParameterType.GameObject</param>
		 */
		public void SetValue (GameObject _object)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = "";
			intValue = -1;
			objectValue = null;
			vector3Value = Vector3.zero;
		}


		/**
		 * <summary>Sets the objectValue that the parameter assigns</summary>
		 * <param name = "_object">The new Unity Object, if parameterType = ParameterType.UnityObject</param>
		 */
		public void SetValue (Object _object)
		{
			gameObject = null;
			floatValue = 0f;
			stringValue = "";
			intValue = -1;
			objectValue = _object;
			vector3Value = Vector3.zero;
		}


		/**
		 * <summary>Sets the gameObject that the parameter assigns</summary>
		 * <param name = "_object">The new GameObject, if parameterType = ParameterType.GameObject</param>
		 * <param name = "_value">The GameObject's ConstantID number, which is used to find the GameObject if it is not always in the same scene as the ActionParameter class</param>
		 */
		public void SetValue (GameObject _object, int _value)
		{
			gameObject = _object;
			floatValue = 0f;
			stringValue = "";
			intValue = _value;
			objectValue = null;
			vector3Value = Vector3.zero;
		}


		/**
		 * <summary>Generates a label that represents the name of the parameter's value, if the parameterType = ParameterType.GameObject<summary>
		 * <returns>A label that represents the name of the parameter's value, if the parameterType = ParameterType.GameObject<summary>
		 */
		public string GetLabel ()
		{
			if (parameterType == ParameterType.GameObject)
			{
				if (gameObject != null)
				{
					Hotspot _hotspot = gameObject.GetComponent <Hotspot>();
					if (_hotspot) return _hotspot.GetName (Options.GetLanguage ());

					Char _char = gameObject.GetComponent <Char>();
					if (_char) return _char.GetName (Options.GetLanguage ());

					return gameObject.name;
				}
				return "";
			}
			return GetSaveData ();
		}


		/**
		 * <summary>Generates a string that represents the parameter's saveable data</summar>
		 * <returns>The data string</returns>
		 */
		public string GetSaveData ()
		{
			if (parameterType == ParameterType.Float)
			{
				return floatValue.ToString ();
			}
			else if (parameterType == ParameterType.String)
			{
				return AdvGame.PrepareStringForSaving (stringValue);
			}
			else if (parameterType == ParameterType.GameObject)
			{
				if (gameObject != null)
				{
					if (gameObject.GetComponent <ConstantID>())
					{
						return gameObject.GetComponent <ConstantID>().constantID.ToString ();
					}
					ACDebug.LogWarning ("Could not save parameter data for '" + gameObject.name + "' as it has no Constant ID number.", gameObject);
				}
			}
			else if (parameterType == ParameterType.UnityObject)
			{
				if (objectValue != null)
				{
					return objectValue.name;
				}
			}
			else if (parameterType == ParameterType.Vector3)
			{
				string vector3Val = vector3Value.x.ToString () + "," + vector3Value.y.ToString () + "," + vector3Value.z.ToString ();
				vector3Val = AdvGame.PrepareStringForSaving (vector3Val);
				return vector3Val;
			}
			else
			{
				return intValue.ToString ();
			}
			return "";
		}


		/**
		 * <summary>Restores data from a data string</summary>
		 * <param name="dataString">The data</param>
		 */
		public void LoadData (string dataString)
		{
			if (parameterType == ParameterType.Float)
			{
				floatValue = 0f;
				float.TryParse (dataString, out floatValue);
			}
			else if (parameterType == ParameterType.String)
			{
				stringValue = AdvGame.PrepareStringForLoading (dataString);
			}
			else if (parameterType == ParameterType.GameObject)
			{
				gameObject = null;
				int constantID = 0;
				if (int.TryParse (dataString, out constantID))
				{
					ConstantID _constantID = Serializer.returnComponent <ConstantID> (constantID);
					if (_constantID != null)
					{
						gameObject = _constantID.gameObject;
					}
				}
			}
			else if (parameterType == ParameterType.UnityObject)
			{
				if (dataString == "")
				{
					objectValue = null;
				}
				else
				{
					Object[] objects = (Object[]) Resources.LoadAll ("");
					foreach (Object _object in objects)
					{
						if (_object.name == dataString)
						{
							objectValue = _object;
							return;
						}
					}
				}
			}
			else if (parameterType == ParameterType.Vector3)
			{
				if (!string.IsNullOrEmpty (dataString))
				{
					dataString = AdvGame.PrepareStringForLoading (dataString);

					Vector3 _value = Vector3.zero;
					string[] valuesArray = dataString.Split (","[0]);
					if (valuesArray != null && valuesArray.Length == 3)
					{
						float xValue = 0f;
						float.TryParse (valuesArray[0], out xValue);

						float yValue = 0f;
						float.TryParse (valuesArray[1], out yValue);

						float zValue = 0f;
						float.TryParse (valuesArray[2], out zValue);

						_value = new Vector3 (xValue, yValue, zValue);
					}

					vector3Value = _value;
				}
			}
			else
			{
				intValue = 0;
				int.TryParse (dataString, out intValue);
			}
		}

	}

}