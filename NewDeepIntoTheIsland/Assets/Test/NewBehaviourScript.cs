using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {
    public vp_FPInput input;
	// Use this for initialization
	void Start () {
		
	}

    public void click()
    {
        print("click");
        //input.MouseCursorForced = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            print("fdafdsa");
            input.MouseCursorForced = true;
        }
        input.MouseCursorForced = true;
    }
}
