using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message : MonoBehaviour {
    public UILabel text;
    public GameObject highlight;
    bool selected = false;
	// Use this for initialization
	void Start () {
		
	}

    public void SelectMessage(bool selected)
    {
        this.selected = selected;
        highlight.SetActive(selected);
    }

    public bool IsSelected()
    {
        return selected;
    }

    public void SetMessage(string t)
    {
        text.text = t;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
