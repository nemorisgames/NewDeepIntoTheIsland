using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.Rendering;

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
		//defaultCulling = cellphoneView.cullingMask;
		//cellphoneView.cullingMask = (defaultCulling | 1 << LayerMask.NameToLayer("UI"));
		Debug.Log("Saving Screen Shot");
		save = true;
	}

    public void OnEnable()
    {
        // register the callback when enabling object
        RenderPipeline.beginCameraRendering += CameraFinishRender;
        //Camera.onPostRender += CameraFinishRender;
    }

    public void OnDisable()
    {
        // remove the callback when disabling object
        RenderPipeline.beginCameraRendering -= CameraFinishRender;
        //Camera.onPostRender -= CameraFinishRender;
    }

    void CameraFinishRender(Camera cam)
	{
		if (save)
		{

            Texture2D tex = new Texture2D(screenTexture.width, screenTexture.height);
            RenderTexture.active = screenTexture;
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            byte[] bytes;
            bytes = tex.EncodeToPNG();

            System.IO.File.WriteAllBytes(ScreenShotLocation(), bytes);
            save = false;
            RenderTexture.active = screenTexture;
            
            /*int width = Screen.width;
            int height = Screen.height;
			Debug.Log(width + " " + height);
			//RenderTexture.active = tempRT;
			Texture2D virtualPhoto = new Texture2D(width, height, TextureFormat.RGB24, false);
			// false, meaning no need for mipmaps
			virtualPhoto.ReadPixels(Camera.main.pixelRect, 0, 0);
			virtualPhoto.Apply();
			// consider ... Destroy(tempRT);

			byte[] bytes;
			bytes = virtualPhoto.EncodeToPNG();

			System.IO.File.WriteAllBytes(ScreenShotLocation(), bytes);
			save = false;
			//RenderTexture.active = screenTexture; //can help avoid errors 
			//cellphoneView.targetTexture = screenTexture;
			//cellphoneView.cullingMask = defaultCulling;
            */
            /*int width = Camera.main.pixelWidth;
            int height = Camera.main.pixelHeight;
            Texture2D snapShot = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture snapShotRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32); // We're gonna render the entire screen into this
            RenderTexture.active = snapShotRT;
            //Camera.main.Render();
            //snapShotRT = Camera.main.targetTexture;
            Rect lassoRectSS = new Rect(0, 0, width, height);
            snapShot.ReadPixels(lassoRectSS, 0, 0);
            snapShot.Apply();

            byte[] bytes;
            bytes = snapShot.EncodeToPNG();

            System.IO.File.WriteAllBytes(ScreenShotLocation(), bytes);
            save = false;
            RenderTexture.active = null;
            Camera.main.targetTexture = null;*/
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
