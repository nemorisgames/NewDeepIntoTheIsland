using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ViewPages : MonoBehaviour
{
	public RawImage image;
	public GameObject noPages;
	private int currentPage = 0;
	private int pagesCount = 1;
	private List<Texture2D> pages;
    PageManager p;

    void OnEnable()
	{
		LoadPages ();
	}
	void LoadPages()
	{
		//load pages from here and add to grid
		pages.Clear();

		p = transform.parent.GetComponent<PageManager> ();
		pagesCount = p.unlockedPages.Count;
        int contador = 0;
        for (int i = 1; i <= p.totalPages; i++){
            if (PlayerPrefs.GetInt("page" + i, 0) == 1)
            {
                pages.Add((Texture2D)p.allPages[i - 1].GetComponent<RawImage>().texture);
                contador++;
                print("adding " + i + " " + (Texture2D)p.allPages[contador].GetComponent<RawImage>().texture);
            }
		}
        ShowPage();

    }
	void NextPage()
	{
		currentPage++;
		if (currentPage >= pagesCount)
		{
			currentPage = pagesCount - 1;
		}
		ShowPage();
	}
	void LastPage()
	{
		currentPage--;
		if (currentPage < 0)
			currentPage = 0;
		ShowPage();
	}
	public void Load()
	{
		/*if (pages != null)
		{
			for (int i = 0; i < pages.Count; i++)
			{
				Destroy(pages[i]);
			}
		}*/
		pages = new List<Texture2D>();
		//aqui cargar paginas
		for (int i = 0; i < pagesCount; i++)
		{
			pages.Add(new Texture2D(500, 500));
		}
		currentPage = 0;
		noPages.SetActive(false);
        print("load pages");
	}
	void ShowPage(int pos = -1)
	{
		if (pos != -1)
		{
			currentPage = pos;
		}
        if (pages != null && pages.Count > 0)
            image.texture = pages[currentPage];
        else
            noPages.SetActive(true);
        //image.color = pages[currentPage].;
        //show currentPage		
        print("show pages");
    }
	void Update()
	{
		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				NextPage();
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				LastPage();
			}
		}
	}
}
