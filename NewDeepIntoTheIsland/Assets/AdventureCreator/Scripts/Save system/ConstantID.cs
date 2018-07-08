/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ConstantID.cs"
 * 
 *	This script is used by the Serialization classes to store a permanent ID
 *	of the gameObject (like InstanceID, only retained after reloading the project).
 *	To save a reference to an arbitrary object in a scene, this script must be attached to it.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * This script is used by the Serialization classes to store a permanent ID
 	 * of the gameObject (like InstanceID, only retained after reloading the project).
 	 * To save a reference to an arbitrary object in a scene, this script must be attached to it.
	*/
	[ExecuteInEditMode]
	[AddComponentMenu("Adventure Creator/Save system/Constant ID")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_constant_i_d.html")]
	#endif
	public class ConstantID : MonoBehaviour
	{

		/** The recorded Constant ID number */
		public int constantID;
		/** If True, prefabs will share the same Constant ID as their scene-based counterparts */ 
		public bool retainInPrefab = false;
		/** Is the Constant ID set automatically or manually? */
		public AutoManual autoManual = AutoManual.Automatic;

		#if UNITY_EDITOR
		private bool isNewInstance = true;
		#endif


		/**
		 * <summary>Serialises appropriate GameObject values into a string.  Overriden by subclasses.</summary>
		 * <returns>The data, serialised as a string (empty in the base class)</returns>
		 */
		public virtual string SaveData ()
		{
			return "";
		}
		

		/**
		 * <summary>Deserialises a string of data, and restores the GameObject to its previous state.  Overridden by subclasses.</summary>
		 * <param name = "stringData">The data, serialised as a string</param>
		 */
		public virtual void LoadData (string stringData)
		{}


		/**
		 * <summary>A base function, overridden by subclasses when loading data.</summary>
		 * <param name = "stringData">A string of serialised data</param>
		 * <param name = "restoringSaveFile">True if the game is currently restoring a save game file, as opposed to transitioning between scenes</param>
		 */
		public virtual void LoadData (string stringData, bool restoringSaveFile)
		{
			LoadData (stringData);
		}

			
		protected bool GameIsPlaying ()
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return false;
			}
			#endif
			return true;
		}


		#if UNITY_EDITOR

		/**
		 * <summary>Sets a new Constant ID number.</summary>
		 * <param name = "forcePrefab">If True, sets "retainInPrefab" to True. Otherwise, it will be determined by whether or not the component is part of an asset file.</param>
		 */
		public int AssignInitialValue (bool forcePrefab = false)
		{
			if (forcePrefab)
			{
				retainInPrefab = true;
				SetNewID_Prefab ();
			}
			else if (UnityVersionHandler.ShouldAssignPrefabConstantID (gameObject))
			{
				retainInPrefab = true;
				SetNewID_Prefab ();
			}
			else
			{
				retainInPrefab = false;
				SetNewID ();
			}
			return constantID;
		}

		
		protected void Update ()
		{
			if (gameObject.activeInHierarchy && !Application.isPlaying)
			{
				if (constantID == 0)
				{
					SetNewID ();
				}
				
				if (isNewInstance)
				{
					isNewInstance = false;
					CheckForDuplicateIDs ();
				}
			}
		}


		/**
		 * Sets a new Constant ID number for a prefab.
		 */
		public void SetNewID_Prefab ()
		{
			SetNewID ();
			isNewInstance = false;
		}
		

		private void SetNewID ()
		{
			// Share ID if another ID script already exists on object
			ConstantID[] idScripts = GetComponents <ConstantID>();
			foreach (ConstantID idScript in idScripts)
			{
				if (idScript != this && idScript.constantID != 0)
				{
					constantID = idScript.constantID;
					UnityVersionHandler.CustomSetDirty (this, true);
					return;
				}
			}

			constantID = GetInstanceID ();
			if (constantID < 0)
			{
				constantID *= -1;
			}

			UnityVersionHandler.CustomSetDirty (this, true);
			ACDebug.Log ("Set new ID for " + this.name + ": " + constantID, gameObject);
		}
		
		
		private void CheckForDuplicateIDs ()
		{
			ConstantID[] idScripts = FindObjectsOfType (typeof (ConstantID)) as ConstantID[];
			
			foreach (ConstantID idScript in idScripts)
			{
				if (idScript.constantID == constantID && idScript.gameObject != this.gameObject && constantID != 0)
				{
					ACDebug.Log ("Duplicate ID found: " + idScript.gameObject.name + " and " + this.name + " : " + constantID, gameObject);
					SetNewID ();
					break;
				}
			}
		}
		
		#endif


		protected bool[] StringToBoolArray (string _string)
		{
			if (_string == null || _string == "" || _string.Length == 0)
			{
				return null;
			}
			
			string[] boolArray = _string.Split (SaveSystem.pipe[0]);
			List<bool> boolList = new List<bool>();
			
			foreach (string chunk in boolArray)
			{
				if (chunk == "False")
				{
					boolList.Add (false);
				}
				else
				{
					boolList.Add (true);
				}
			}
			
			return boolList.ToArray ();
		}


		protected int[] StringToIntArray (string _string)
		{
			if (_string == null || _string == "" || _string.Length == 0)
			{
				return null;
			}
			
			string[] intArray = _string.Split (SaveSystem.pipe[0]);
			List<int> intList = new List<int>();
			
			foreach (string chunk in intArray)
			{
				intList.Add (int.Parse (chunk));
			}
			
			return intList.ToArray ();
		}


		protected float[] StringToFloatArray (string _string)
		{
			if (_string == null || _string == "" || _string.Length == 0)
			{
				return null;
			}
			
			string[] floatArray = _string.Split (SaveSystem.pipe[0]);
			List<float> floatList = new List<float>();
			
			foreach (string chunk in floatArray)
			{
				floatList.Add (float.Parse (chunk));
			}
			
			return floatList.ToArray ();
		}


		protected string[] StringToStringArray (string _string)
		{
			if (_string == null || _string == "" || _string.Length == 0)
			{
				return null;
			}
			
			string[] stringArray = _string.Split (SaveSystem.pipe[0]);
			for (int i=0; i<stringArray.Length; i++)
			{
				stringArray[i] = AdvGame.PrepareStringForLoading (stringArray[i]);
			}

			return stringArray;
		}
		
		
		protected string ArrayToString <T> (T[] _list)
		{
			System.Text.StringBuilder _string = new System.Text.StringBuilder ();
			
			foreach (T state in _list)
			{
				string stateString = AdvGame.PrepareStringForSaving (state.ToString ());
				_string.Append (stateString + SaveSystem.pipe);
			}
			if (_string.Length > 0)
			{
				_string.Remove (_string.Length-1, 1);
			}
			return _string.ToString ();
		}

	}


	/**
	 * A subclass of ConstantID, that is used to distinguish further subclasses from ConstantID components.
	 */
	[System.Serializable]
	public class Remember : ConstantID
	{

		protected bool savePrevented = false;

		/** If True, saving is prevented */
		public bool SavePrevented
		{
			get
			{
				return savePrevented;
			}
			set
			{
				savePrevented = value;
			}
		}

	}
	

	/**
	 * The base class of saved data.  Each Remember subclass uses its own RememberData subclass to store its data.
	 */
	[System.Serializable]
	public class RememberData
	{

		/** The ConstantID number of the object being saved */
		public int objectID;
		/** If True, saving is prevented */
		public bool savePrevented;

		/**
		 * The base constructor.
		 */
		public RememberData () { }
	}
	
}