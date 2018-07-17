using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class CellPhone : MonoBehaviour
{
	public static CellPhone Instance = null;
	Transform target;
	GameObject cellphoneBody;
	public float speed = 1f;
	public float speedRotation = 1f;
	public Vector3 position;
	public Vector3 positionSelected;
	bool phoneUp = false;
	public Light light;
	public Light lightStandBy;
	bool bringingPhoneUp = false;
	public bool active = false;

	[Header("For Functions")]
	public TweenAlpha[] functions;
	//public GameObject[] selectors;
	//public Color phoneUpColor;
	//public Color unphoneUpColor;
	public enum CellphoneFunctions { Light, CameraPhoto, ReviewPhotos, Call};
	public CellphoneFunctions currentFunction = CellphoneFunctions.Light;
    
	int indiceActual = 0;

	[Header("ForScreenShots")]
	public TakePhoto photoFunctionality;
	private bool isSavingPhoto = false;
	private bool canUseMouseScroll = true;
	private bool isTakingPhoto = false;
	[Header("ForNotifications")]
	public GameObject notification;
	public AudioClip notificationSound;
	public AudioClip photoSound;
	public AudioSource source;
	private float timeOut = 0;
	private bool showingNotification = false;
	private int maxNotification = 5;
	private int currentNotifications = 0;
	float defaultSpotAngle = 0;
	float defaultIntensity = 0;
	bool defaultLightEnabled = false;

    public PostProcessVolume volume;
    DepthOfField depthOfField;
    MotionBlur motionBlur;
    MeshRenderer[] phoneRenderers;
    SkinnedMeshRenderer[] handRenderers;

    void Start()
	{
		notification.SetActive(false);
		cellphoneBody = transform.Find("Cuerpo").gameObject;
		cellphoneBody.SetActive(active);
        volume.profile.TryGetSettings(out depthOfField);
        volume.profile.TryGetSettings(out motionBlur);
        phoneRenderers = GetComponentsInChildren<MeshRenderer>();
        handRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        showHandAndPhone(false);
    }
	void Awake()
	{
		target = GameObject.FindWithTag("Player").transform.Find("FPSCamera");
		defaultIntensity = light.intensity;
		defaultSpotAngle = light.spotAngle;
		defaultLightEnabled = light.enabled;
		Instance = this;
	}

    void showHandAndPhone(bool show)
    {
        foreach(MeshRenderer m in phoneRenderers)
        {
            m.enabled = show;
        }
        foreach (SkinnedMeshRenderer m in handRenderers)
        {
            m.enabled = show;
        }
    }

	public void activate()
	{
		activate(true);
	}

	public void deactivate()
	{
		activate(false);
	}

	public void activate(bool active)
	{
		this.active = active;
		cellphoneBody.SetActive(active);
    }
	void LateUpdate()
	{

		if (!phoneUp)
		{
			if (target != null)
			{
				Vector3 tVec = new Vector3(target.position.x, target.position.y, target.position.z) + target.right * position.x + target.forward * position.z + target.up * position.y;
				transform.position = Vector3.Lerp(transform.position, tVec, Time.deltaTime * speed);
				transform.forward = Vector3.Lerp(transform.forward, target.forward, Time.deltaTime * speedRotation);
			}
		}
		else
		{
			if (transform.parent != target)
			{
				Vector3 tVec = new Vector3(target.position.x, target.position.y, target.position.z) + target.right * positionSelected.x + target.forward * positionSelected.z + target.up * positionSelected.y;
				transform.position = Vector3.Lerp(transform.position, tVec, Time.deltaTime * speed * 5f);
				transform.forward = Vector3.Lerp(transform.forward, target.forward, Time.deltaTime * speedRotation * 10f);
			}
		}

		//Checkout on cellphone
		if (Input.GetButtonDown("Fire2"))
		{
            if (ScreenManager.showingScreen)
            {
                ScreenManager.Instance.CloseScreen();
            }
            else
            {
                if (!bringingPhoneUp)
                {
                    print("showing screen " + ScreenManager.showingScreen);
                    phoneUp = !phoneUp;
                    if (phoneUp)
                    {
                        StartCoroutine(PhoneUpAnimatedTransition());
                        ShowNotificationIfActive();
                    }
                    else
                    {
                        ResetToDefaults();
                        transform.parent = null;
                    }
                    depthOfField.active = !phoneUp;
                    motionBlur.active = !phoneUp;
                    showHandAndPhone(phoneUp);
                    ScreenManager.Instance.CloseScreen();
                }
            }
        }

        if (!active || ScreenManager.paused)
            return;

        if (Input.GetAxis("Mouse ScrollWheel") != 0f && canUseMouseScroll && phoneUp)
		{
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				nextFunction(true);
			}
			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				nextFunction(false);
			}
		}

		//Cellphone function
		if (Input.GetButtonDown("Fire1") && phoneUp)
		{
			switch (currentFunction)
			{
				case CellphoneFunctions.Light:
					turnLight(!light.enabled);
					break;
				case CellphoneFunctions.CameraPhoto:
					prepareTakePhoto();
					break;
				case CellphoneFunctions.ReviewPhotos:
					ScreenManager.Instance.ShowScreen(ScreenType.PhotoView);
					canUseMouseScroll = false;
					break;
                case CellphoneFunctions.Call:
                    ScreenManager.Instance.ShowScreen(ScreenType.Call);
                    break;
            }
        }
		//SetFunctionColors((int)currentFunction);
	}

	void turnLight(bool lightOn)
	{
		light.enabled = lightOn;
		Light s = light.transform.Find("SupportLight").GetComponent<Light>();
		s.enabled = lightOn;
		//AC.GlobalVariables.SetBooleanValue(0, lightOn);
	}

	void prepareTakePhoto()
	{
		if (!isTakingPhoto)
			StartCoroutine(takePhoto());
	}

	void ResetToDefaults()
	{
		if (currentFunction != CellphoneFunctions.Light)
		{
			Debug.Log("Reseting defaults");
			light.spotAngle = defaultSpotAngle;
			light.intensity = defaultIntensity;
			//light.enabled = AC.GlobalVariables.GetBooleanValue(0, light.enabled);
			isSavingPhoto = false;
			isTakingPhoto = false;
		}
	}
	IEnumerator takePhoto()
	{
		isTakingPhoto = true;
		float intensityOriginal = light.intensity;
		float angleOriginal = light.spotAngle;
		bool lightEnabledOriginal = light.enabled;
		light.enabled = true;

		light.spotAngle = 140f;
		lightStandBy.enabled = true;
		while (light.intensity > intensityOriginal * 0.1f)
		{
			light.intensity -= intensityOriginal / 20f;
			yield return new WaitForSeconds(0.01f);
		}

		yield return new WaitForSeconds(0.2f);
		lightStandBy.enabled = false;
		lightStandBy.intensity = 1.3f;

		while (light.intensity < intensityOriginal * 4f)
		{
			light.intensity += intensityOriginal * 4f / 5f;
			yield return new WaitForSeconds(0.01f);
			if (!isSavingPhoto)
			{
				isSavingPhoto = true;
				source.clip = photoSound;
				source.Play();
				photoFunctionality.SaveCameraScreenShot();
			}
		}
		yield return new WaitForSeconds(0.2f);
		light.spotAngle = angleOriginal;
		light.intensity = intensityOriginal;
		light.enabled = lightEnabledOriginal;
		isSavingPhoto = false;
		isTakingPhoto = false;
	}
	/*void SetFunctionColors(int index)
	{
		for (int i = 0; i < functions.Length; i++)
		{
			if (i == index)
			{
				functions[i].GetComponent<Renderer>().material.color = phoneUpColor;
				selectors[i].SetActive(true);
				selectors[i].GetComponent<Renderer>().material.color = phoneUpColor;
			}
			else
			{
				functions[i].GetComponent<Renderer>().material.color = unphoneUpColor;
				selectors[i].SetActive(false);
				selectors[i].GetComponent<Renderer>().material.color = unphoneUpColor;
			}
		}
	}*/
	void nextFunction(bool next)
    {
        functions[indiceActual].PlayReverse();
        indiceActual = Mathf.Clamp(indiceActual + (next ? 1 : -1), 0, 3);
        functions[indiceActual].PlayForward();
        switch (indiceActual)
		{
			case 0: currentFunction = CellphoneFunctions.Light; break;
			case 1: currentFunction = CellphoneFunctions.CameraPhoto; break;
			case 2: currentFunction = CellphoneFunctions.ReviewPhotos; break;
			case 3: currentFunction = CellphoneFunctions.Call; break;
		}
	}
	IEnumerator PhoneUpAnimatedTransition()
	{
		bringingPhoneUp = true;
		yield return new WaitForSeconds(.3f);
		transform.parent = target;
		Vector3 tVec = new Vector3(target.position.x, target.position.y, target.position.z) + target.right * positionSelected.x + target.forward * positionSelected.z + target.up * positionSelected.y;
		transform.position = tVec;
		transform.forward = target.forward;
		transform.rotation = target.rotation;
		bringingPhoneUp = false;
	}
	void ShowNotificationIfActive()
	{
		if (showingNotification)
		{
			timeOut = Time.time;
			InvokeRepeating("NotificationDuration", 0f, 1f);
			showingNotification = false;
			CancelInvoke("NotificationSound");
		}
	}
	void ShowNotification()
	{
		if (!showingNotification)
		{
			currentNotifications = 0;
			showingNotification = true; //can be used to close notification when she shows the cell phone
			InvokeRepeating("NotificationSound", 0f, 1.5f);
		}
	}
	void NotificationSound()
	{
		//make sound
		currentNotifications++;
		source.clip = notificationSound;
		source.Play();
		if (currentNotifications >= maxNotification)
		{
			showingNotification = false;
			CancelInvoke("NotificationSound");
			return;
		}
	}
	void NotificationDuration()
	{
		notification.SetActive(true);
		//define max time
		if (Time.time - timeOut >= 5f)
		{
			timeOut = 0;
			CancelInvoke("NotificaionDuration");
			notification.SetActive(false);
		}
	}
	public void CanUseScroller(bool canUse)
	{
		canUseMouseScroll = canUse;
	}
}
