using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageManager : MonoBehaviour
{
	public static PageManager Instance = null;
	public GameObject pagePrefab;
	[SerializeField]
	public List<int> unlockedPages;
	[SerializeField]
	public GameObject[] allPages;
    [HideInInspector]
    public int totalPages = 4;
	private void Awake()
	{
		Instance = this;
		unlockedPages = new List<int>();
		Load();
	}
	void Load()
	{
		//test data
		//AddPage(0);
        for(int i = 1; i <= totalPages; i++)
        {
            if(PlayerPrefs.GetInt("page" + i, 0) == 1)
            {
                AddPage(i);
            }
        }
	}
	public void AddPage(int page)
	{
		if (!unlockedPages.Contains(page))
		{
			unlockedPages.Add(page);
            PlayerPrefs.SetInt("page" + page, 1);
        }
	}
}
