using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Task : MonoBehaviour
{
	[SerializeField]
	Image selector;
	[SerializeField]
	Text text;
	public void ShowSelector(bool a)
	{
		selector.gameObject.SetActive(a);
	}
	public void SetText(string t)
	{
		text.text = t;
	}
}
