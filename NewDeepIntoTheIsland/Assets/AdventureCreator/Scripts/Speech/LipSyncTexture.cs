using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Animates a SkinnedMeshRenderer's textures based on lipsync animation
	 */
	[AddComponentMenu("Adventure Creator/Characters/Lipsync texture")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_lip_sync_texture.html")]
	#endif
	public class LipSyncTexture : MonoBehaviour
	{

		/** The SkinnedMeshRenderer to affect */
		public SkinnedMeshRenderer skinnedMeshRenderer;
		/** The index of the material to affect */
		public int materialIndex;
		/** The material's property name that will be replaced */
		public string propertyName = "_MainTex";
		/** A List of Texture2Ds that correspond to the phoneme defined in the Phonemes Editor */
		public List<Texture2D> textures = new List<Texture2D>();


		private void Awake ()
		{
			LimitTextureArray ();
		}


		/**
		 * Resizes the textures List to match the number of phonemes defined in the Phonemes Editor
		 */
		public void LimitTextureArray ()
		{
			if (AdvGame.GetReferences () == null || AdvGame.GetReferences ().speechManager == null)
			{
				return;
			}

			int arraySize = AdvGame.GetReferences ().speechManager.phonemes.Count;

			if (textures.Count != arraySize)
			{
				int numTextures = textures.Count;

				if (arraySize < numTextures)
				{
					textures.RemoveRange (arraySize, numTextures - arraySize);
				}
				else if (arraySize > numTextures)
				{
					for (int i=textures.Count; i<arraySize; i++)
					{
						textures.Add (null);
					}
				}
			}
		}


		/**
		 * <summary>Sets the material's texture based on the currently-active phoneme.</summary>
		 * <param name = "textureIndex">The index number of the phoneme</param>
		 */
		public void SetFrame (int textureIndex)
		{
			if (skinnedMeshRenderer)
			{
				if (materialIndex >= 0 && skinnedMeshRenderer.materials.Length > materialIndex)
				{
					skinnedMeshRenderer.materials [materialIndex].SetTexture (propertyName, textures [textureIndex]);
				}
				else
				{
					ACDebug.LogWarning ("Cannot find material index " + materialIndex + " on SkinnedMeshRenderer " + skinnedMeshRenderer.gameObject.name);
				}
			}
		}

	}

}