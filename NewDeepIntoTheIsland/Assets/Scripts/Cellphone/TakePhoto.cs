using UnityEngine;
using System.Collections;

public class TakePhoto : MonoBehaviour
{
	[Header("ForScreenShots")]
	public Camera cellphoneView;
	public RenderTexture screenTexture;
	public int nextPhotoNumber;
	public static string photoName = "ScreenShot_";
	bool save = false;
	int defaultCulling;
	// Use this for initialization
	void Start()
	{

	}
	void Awake()
	{
		nextPhotoNumber = PlayerPrefs.GetInt("PhotoNumber", 0);
	}

	// Update is called once per frame
	void Update()
	{

	}
	public void SaveCameraScreenShot()
	{
		defaultCulling = cellphoneView.cullingMask;
		cellphoneView.cullingMask = (defaultCulling | 1 << LayerMask.NameToLayer("OnlyCamera"));
		Debug.Log("Saving Screen Shot");
		save = true;
	}
	void OnPostRender()
	{
		if (save)
		{
			int width = cellphoneView.pixelWidth;
			int height = cellphoneView.pixelHeight;
			Debug.Log(width + " " + height);
			//RenderTexture.active = tempRT;
			Texture2D virtualPhoto = new Texture2D(width, height, TextureFormat.RGB24, false);
			// false, meaning no need for mipmaps
			virtualPhoto.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			virtualPhoto.Apply();

			// consider ... Destroy(tempRT);

			byte[] bytes;
			bytes = virtualPhoto.EncodeToPNG();

			System.IO.File.WriteAllBytes(ScreenShotLocation(), bytes);
			save = false;
			RenderTexture.active = screenTexture; //can help avoid errors 
			cellphoneView.targetTexture = screenTexture;
			cellphoneView.cullingMask = defaultCulling;
		}
	}
	private string ScreenShotLocation()
	{
		nextPhotoNumber = PlayerPrefs.GetInt("PhotoNumber", 0);
		string r = Application.persistentDataPath + "/" + photoName + nextPhotoNumber + ".png";
		nextPhotoNumber++;
		PlayerPrefs.SetInt("PhotoNumber", nextPhotoNumber);
		Debug.Log(r);
		return r;
	}
}
