using UnityEngine;
using System.Collections.Generic;
public enum ScreenType
{
    Menu,
    Camera,
	PhotoView,
	BookView,
	ItemView,
	Options,
    Message,
	Save,
	Load,
	Quit,
}
public class ScreenManager : MonoBehaviour
{
	public static ScreenManager Instance = null;
    [SerializeField]
    GameObject menuScreen;
    [SerializeField]
	GameObject[] screens;
	[SerializeField]
	GameObject taskScreen;
    [SerializeField]
    TweenAlpha iconsScreen;
    [SerializeField]
	bool debug = false;
    public static bool showingScreen = false;
	private Stack<ScreenType> showedScreens = new Stack<ScreenType>();
    vp_FPInput inputUFPS;

	void Start()
	{
        inputUFPS = GameManager.instance.playerInput;
    }
	void Awake()
	{
		Instance = this;
		showedScreens = new Stack<ScreenType>();
		HideScreens();
		if (debug)
		{
			ShowScreen(ScreenType.PhotoView);
		}
	}
	void HideScreens()
	{
		for (int i = 0; i < screens.Length; i++)
		{
			screens[i].SetActive(false);
		}
		taskScreen.SetActive(false);
        iconsScreen.PlayForward();

    }
	public void ShowTaskScreen()
	{
		showedScreens.Clear();
		HideScreens();
        taskScreen.SetActive(true);
        showingScreen = true;
    }

    public ScreenType GetCurrentType()
    {
        if (showedScreens == null || showedScreens.Count == 0) return ScreenType.Menu;
        return showedScreens.Peek();
    }

	public void ShowScreen(ScreenType type)
	{
        int index = 0;
        for(int i = 0; i < screens.Length; i++)
        {
            if (type == ScreenType.Message && screens[i].GetComponent<MessageReview>() != null)
            {
                index = i;
                break;
            }
            if (type == ScreenType.PhotoView && screens[i].GetComponent<PhotoReview>() != null)
            {
                index = i;
                break;
            }
        }

        if (screens[index].activeSelf)
		{
			if (!showedScreens.Contains(type))
			{
				HideScreens();
				screens[(int)type].SetActive(true);
				showedScreens.Push(type);
				return;
			}
		}
		HideScreens();
		if (showedScreens.Contains(type))
		{
			while (showedScreens.Pop() != type)
			{

			}
		}
		//any aditional setups
		switch (type)
		{
            case ScreenType.Camera:
                menuScreen.SetActive(false);
                break;
			case ScreenType.PhotoView:
				screens[index].GetComponent<PhotoReview>().LoadPhotos();
                menuScreen.SetActive(false);
				break;
			case ScreenType.BookView:
				screens[index].GetComponent<ViewPages>().Load();
				break;
            case ScreenType.Message:
                screens[index].GetComponent<MessageReview>().CheckMessages();
                menuScreen.SetActive(false);
                break;
            default:
				break;
		}
		showedScreens.Push(type);

        if (type == ScreenType.Camera)
        {
            menuScreen.SetActive(false);
        }
        else
        {
            PauseGame();
            screens[index].SetActive(true);
        }

        showingScreen = true;
    }
	public void CloseAllScreens()
	{
		HideScreens();
		showedScreens.Clear();
		ResumeGame();
        showingScreen = false;
    }
	public void CloseScreen()
	{
		HideScreens();
		if (showedScreens.Count == 1)
		{
			ResumeGame();
			showedScreens.Clear();
            showingScreen = false;
            if (!menuScreen.activeSelf)
                menuScreen.SetActive(true);
            iconsScreen.PlayReverse();
            return;
		}
		if (showedScreens.Count == 0)
		{
            //para cuando se vincule todo con escape
            iconsScreen.PlayReverse();
            return;
		}
		ScreenType type = showedScreens.Pop();
		screens[(int)type].SetActive(true);
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetButtonUp("Book"))
		{
			if (!screens[(int)ScreenType.BookView].activeSelf)
				ShowScreen(ScreenType.BookView);
		}
	}
	public void PauseGame()
	{
        GameManager.instance.gameStatus = GameManager.GameStatus.Paused;
		//paused = true;
		//pauseGame.Interact();
		if (CellPhone.Instance)
			CellPhone.Instance.CanUseScroller(false);
		vp_TimeUtility.Paused = (true);
        //vp_Utility.LockCursor = false;
        inputUFPS.MouseCursorForced = true;


    }
	public void ResumeGame()
    {
        GameManager.instance.gameStatus = GameManager.GameStatus.Normal;
        //paused = false;
		//unPauseGame.Interact();
		//vp_Utility.LockCursor = true;
        inputUFPS.MouseCursorForced = false;
        vp_TimeUtility.Paused = (false);
		if (CellPhone.Instance)
			CellPhone.Instance.CanUseScroller(true);
	}
}
