using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageReview : MonoBehaviour {
    public static MessageReview instance = null;
    public GameObject noMessagesSign;
    public GameObject messagePrefab;
    public List<Message> messages;
    public Transform messagesGrid;
    int currentIndex = 0;
    [HideInInspector]
    public bool notReaded = false;
    public GameObject notReadedSign;

	// Use this for initialization
	void Awake () {
        instance = this;
        messages = new List<Message>();
        print("iniciado");
    }

    public void AddMessage(string text)
    {
        notReaded = true;
        notReadedSign.SetActive(notReaded);
        print("adding");
        GameObject m = (GameObject)Instantiate(messagePrefab);
        m.transform.parent = messagesGrid;
        m.transform.localRotation = Quaternion.identity;
        m.transform.localScale = Vector3.one;
        Message message = m.GetComponent<Message>();
        message.SetMessage(text);
        messages.Add(message);
        currentIndex = messages.Count - 1;
        OrderMessages();
    }

    void OrderMessages()
    {
        //elements order each other
        for (int i = 0; i < messages.Count; i++)
        {
            messages[i].transform.localPosition = new Vector3(-12f, (- i + currentIndex) * -85f, 0f);
            messages[i].SelectMessage(false);
        }
        messages[currentIndex].SelectMessage(true);
    }
    
    public void CheckMessages()
    {
        notReaded = false;
        notReadedSign.SetActive(notReaded);
        noMessagesSign.SetActive(messages.Count <= 0);
    }

    public void NextMessage()
    {
        currentIndex = Mathf.Clamp(currentIndex - 1, 0, messages.Count - 1);
        OrderMessages();
    }

    public void LastMessage()
    {
        currentIndex = Mathf.Clamp(currentIndex + 1, 0, messages.Count - 1);
        OrderMessages();
    }

    // Update is called once per frame
    void Update () {
        if (messages.Count <= 0) return;
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                NextMessage();
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                LastMessage();
            }
        }
    }
}
