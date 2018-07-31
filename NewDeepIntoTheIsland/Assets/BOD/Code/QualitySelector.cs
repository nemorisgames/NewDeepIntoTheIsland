using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[ExecuteInEditMode]
public class QualitySelector : MonoBehaviour {
	public enum Platform { Desktop, PS4, PS4Pro, XB1, XB1X };

	[System.Serializable]
	public struct Quality {
		public Platform				platform;
		public RenderPipelineAsset	renderPipeline;
		public LayerCulling			layerCulling;
	}

	public Platform		overrideEditorPlatform	= Platform.Desktop;
	public Quality[]	qualitySettings			= new Quality[0];

	void OnValidate() {
		ApplyQuality();	
	}

#if UNITY_EDITOR
	void Awake() {
		// Reset override
		if(!Application.isPlaying && overrideEditorPlatform != Platform.Desktop)
			overrideEditorPlatform = Platform.Desktop;
	}
#endif

	void OnEnable() {
		ApplyQuality();
	}

	void ApplyQuality() {
		var platform = GetPlatform();
		foreach(var quality in qualitySettings) {
			if(quality.platform == platform) {
				ApplyQuality(quality);
				break;
			}
		}
	}

	void ApplyQuality(Quality quality) {
		if(Application.isPlaying)
			Debug.LogFormat("Applying quality: {0}", quality.platform);

		if(quality.renderPipeline && quality.renderPipeline != GraphicsSettings.renderPipelineAsset)
			GraphicsSettings.renderPipelineAsset = quality.renderPipeline;

		if(quality.layerCulling && quality.layerCulling != LayerCulling.Instance) {
			if(LayerCulling.Instance)
				LayerCulling.Instance.enabled = false;

			quality.layerCulling.enabled = true;

			if(!quality.layerCulling.isActiveAndEnabled)
				quality.layerCulling.gameObject.SetActive(true);
		}
	}

	Platform GetPlatform() {
#if UNITY_EDITOR
		return overrideEditorPlatform;
#elif UNITY_PS4
		return UnityEngine.PS4.Utility.neoMode ? Platform.PS4Pro : Platform.PS4;
#elif UNITY_XBOXONE
		var hwVer = UnityEngine.XboxOne.Hardware.version; 
		return hwVer == UnityEngine.XboxOne.HardwareVersion.XboxOne || hwVer == UnityEngine.XboxOne.HardwareVersion.XboxOneS ? Platform.XB1 : Platform.XB1X;
#else
		return Platform.Desktop;
#endif
	}
}
