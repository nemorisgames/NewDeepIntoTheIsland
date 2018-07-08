/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"RememberAnimator.cs"
 * 
 *	This script is attached to Animator components in the scene we wish to save the state of. (Unity 5-only)
 * 
 */

using UnityEngine;
using System.Text;

namespace AC
{

	#if UNITY_5 || UNITY_2017_1_OR_NEWER
	
	/**
	 * This script is attached to Animator components in the scene we wish to save the state of. (Unity 5-only)
	 */
	[RequireComponent (typeof (Animator))]
	[AddComponentMenu("Adventure Creator/Save system/Remember Animator")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_remember_animator.html")]
	#endif
	public class RememberAnimator : Remember
	{
		
		private Animator _animator;
		
		
		private void Awake ()
		{
			_animator = GetComponent <Animator>();
		}
		
		
		public override string SaveData ()
		{
			AnimatorData animatorData = new AnimatorData ();
			animatorData.objectID = constantID;
			animatorData.savePrevented = savePrevented;
			
			animatorData.parameterData = ParameterValuesToString (_animator.parameters);
			animatorData.layerWeightData = LayerWeightsToString ();
			animatorData.stateData = StatesToString ();

			return Serializer.SaveScriptData <AnimatorData> (animatorData);
		}
		
		
		public override void LoadData (string stringData)
		{
			AnimatorData data = Serializer.LoadScriptData <AnimatorData> (stringData);
			if (data == null) return;
			SavePrevented = data.savePrevented; if (savePrevented) return;
			
			StringToParameterValues (_animator.parameters, data.parameterData);
			StringToLayerWeights (data.layerWeightData);
			StringToStates (data.stateData);
		}
		
		
		private string ParameterValuesToString (AnimatorControllerParameter[] parameters)
		{
			StringBuilder stateString = new StringBuilder ();
			
			foreach (AnimatorControllerParameter parameter in parameters)
			{
				if (parameter.type == AnimatorControllerParameterType.Bool)
				{
					string value = (_animator.GetBool (parameter.name)) ? "1" : "0";
					stateString.Append (value);
				}
				else if (parameter.type == AnimatorControllerParameterType.Float)
				{
					stateString.Append (_animator.GetFloat (parameter.name).ToString ());
				}
				else if (parameter.type == AnimatorControllerParameterType.Int)
				{
					stateString.Append (_animator.GetInteger (parameter.name).ToString ());
				}
				
				stateString.Append (SaveSystem.pipe);
			}
			
			return stateString.ToString ();
		}


		private string LayerWeightsToString ()
		{
			StringBuilder stateString = new StringBuilder ();

			if (_animator.layerCount > 1)
			{
				for (int i=1; i<_animator.layerCount; i++)
				{
					float weight = _animator.GetLayerWeight (i);
					stateString.Append (weight.ToString ());
					stateString.Append (SaveSystem.pipe);
				}
			}

			return stateString.ToString ();
		}


		private string StatesToString ()
		{
			StringBuilder stateString = new StringBuilder ();

			for (int i=0; i<_animator.layerCount; i++)
			{
				if (_animator.IsInTransition (i))
				{
					stateString = ProcessState (stateString, _animator.GetNextAnimatorStateInfo (i));
				}
				else
				{
					stateString = ProcessState (stateString, _animator.GetCurrentAnimatorStateInfo (i));
				}

				stateString.Append (SaveSystem.pipe);
			}

			return stateString.ToString ();
		}


		private StringBuilder ProcessState (StringBuilder stateString, AnimatorStateInfo stateInfo)
		{
			int nameHash = stateInfo.shortNameHash;
			float timeAlong = stateInfo.normalizedTime;

			if (timeAlong > 1f)
			{
				if (stateInfo.loop)
				{
					while (timeAlong > 1f)
					{
						timeAlong -= 1f;
					}
				}
				else
				{
					timeAlong = 1f;
				}
			}

			stateString.Append (nameHash + "," + timeAlong);
			return stateString;
		}
		
		
		private void StringToParameterValues (AnimatorControllerParameter[] parameters, string valuesString)
		{
			if (string.IsNullOrEmpty (valuesString))
			{
				return;
			}
			
			string[] valuesArray = valuesString.Split (SaveSystem.pipe[0]);
			
			for (int i=0; i<parameters.Length; i++)
			{
				if (i < valuesArray.Length && valuesArray[i].Length > 0)
				{
					string parameterName = parameters[i].name;
					
					if (parameters[i].type == AnimatorControllerParameterType.Bool)
					{
						_animator.SetBool (parameterName, (valuesArray[i] == "1") ? true : false);
					}
					else if (parameters[i].type == AnimatorControllerParameterType.Float)
					{
						float value = 0f;
						if (float.TryParse (valuesArray[i], out value))
						{
							_animator.SetFloat (parameterName, value);
						}
					}
					else if (parameters[i].type == AnimatorControllerParameterType.Int)
					{
						int value = 0;
						if (int.TryParse (valuesArray[i], out value))
						{
							_animator.SetInteger (parameterName, value);
						}
					}
				}
			}
		}


		private void StringToLayerWeights (string valuesString)
		{
			if (string.IsNullOrEmpty (valuesString) || _animator.layerCount <= 1)
			{
				return;
			}

			string[] valuesArray = valuesString.Split (SaveSystem.pipe[0]);

			for (int i=1; i<_animator.layerCount; i++)
			{
				if (i < (valuesArray.Length+1) && valuesArray[i-1].Length > 0)
				{
					float weight = 1f;
					if (float.TryParse (valuesArray[i-1], out weight))
					{
						_animator.SetLayerWeight (i, weight);
					}
				}
			}
		}


		private void StringToStates (string valuesString)
		{
			if (string.IsNullOrEmpty (valuesString))
			{
				return;
			}
			
			string[] valuesArray = valuesString.Split (SaveSystem.pipe[0]);
			
			for (int i=0; i<_animator.layerCount; i++)
			{
				if (i < (valuesArray.Length) && valuesArray[i].Length > 0)
				{
					string[] stateInfoArray = valuesArray[i].Split (","[0]);

					if (stateInfoArray.Length >= 2)
					{
						int nameHash = 0;
						float timeAlong = 0f;

						if (int.TryParse (stateInfoArray[0], out nameHash))
						{
							if (float.TryParse (stateInfoArray[1], out timeAlong))
							{
								_animator.Play (nameHash, i, timeAlong);
							}
						}
					}
				}
			}
		}
		
	}
	

	/**
	 * A data container used by the RememberAnimator script.
	 */
	[System.Serializable]
	public class AnimatorData : RememberData
	{

		/** The values of the parameters, separated by a pipe (|) character. */
		public string parameterData;
		/** The weights of each layer, separated by a pipe (|) character. */
		public string layerWeightData;
		/** Data for each layer's animation state. */
		public string stateData;

		/**
		 * The default Constructor.
		 */
		public AnimatorData () { }

	}

	#else

	public class RememberAnimator : MonoBehaviour
	{ }

	#endif

}
	