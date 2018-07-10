using UnityEngine;
using System.Collections.Generic;
public enum ScreenType
{
	PhotoView,
	BookView,
	ItemView,
	Options,
	Save,
	Load,
	Quit,
}
public class ScreenManager : MonoBehaviour
{
	public static ScreenManager Instance = null;
	[SerializeField]
	GameObject[] screens;
	[SerializeField]
	GameObject taskScreen;
	[SerializeField]
	bool debug = false;
	//[SerializeField]
	//AC.Cutscene pauseGame;
	//[SerializeField]
	//AC.Cutscene unPauseGame;
	public static bool paused = false;
	private Stack<ScreenType> showedScreens = new Stack<ScreenType>();
	void Start()
	{

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
	}
	public void ShowTaskScreen()
	{
		showedScreens.Clear();
		HideScreens();
		taskScreen.SetActive(true);
	}
	public void ShowScreen(ScreenType type)
	{
		if (screens[(int)type].activeSelf)
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
			case ScreenType.PhotoView:
				screens[(int)type].GetComponent<PhotoReview>().LoadPhotos();
				break;
			case ScreenType.BookView:
				screens[(int)type].GetComponent<ViewPages>().Load();
				break;
			default:
				break;
		}
		showedScreens.Push(type);
		PauseGame();
		screens[(int)type].SetActive(true);
	}
	public void CloseAllScreens()
	{
		HideScreens();
		showedScreens.Clear();
		ResumeGame();
	}
	public void CloseScreen()
	{
		HideScreens();
		if (showedScreens.Count == 1)
		{
			ResumeGame();
			showedScreens.Clear();
			return;
		}
		if (showedScreens.Count == 0)
		{
			//para cuando se vincule todo con escape
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
		paused = true;
		//pauseGame.Interact();
		if (CellPhone.Instance)
			CellPhone.Instance.CanUseScroller(false);
		vp_TimeUtility.Paused = (true);
		vp_Utility.LockCursor = false;
	}
	public void ResumeGame()
	{
		paused = false;
		//unPauseGame.Interact();
		vp_Utility.LockCursor = true;
		vp_TimeUtility.Paused = (false);
		if (CellPhone.Instance)
			CellPhone.Instance.CanUseScroller(true);
	}
}
