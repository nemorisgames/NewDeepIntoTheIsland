using UnityEngine;
using System.Collections;

public class MenuButtons : MonoBehaviour
{
	public ScreenType type;
	public void OnClick()
	{
		ScreenManager.Instance.ShowScreen(type);
	}
	public void OnClickResume()
	{
		ScreenManager.Instance.CloseAllScreens();
	}
}
