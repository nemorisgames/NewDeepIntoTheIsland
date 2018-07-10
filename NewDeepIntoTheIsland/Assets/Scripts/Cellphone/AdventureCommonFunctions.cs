using UnityEngine;
using System.Collections;

public class AdventureCommonFunctions : MonoBehaviour {
    [SerializeField]
    GameObject objectFunctions;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    void Hide()
    {
        //Debug.Log(gameObject.name + " has been hidden");
        objectFunctions.SetActive(false);
    }
    void Show()
    {
        //Debug.Log(gameObject.name + " is now being shown");
        objectFunctions.SetActive(true);
    }
}
