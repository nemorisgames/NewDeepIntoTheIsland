using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Photo : MonoBehaviour
{
	public int pos = 0;
	//public Color higlighted;
	//public Color unHiglighted;
	public TweenColor highlight;
	public UITexture texture;
	//public GameObject selected;
	[SerializeField]
	float selectedPos = -32f;
	[SerializeField]
	float UnSelectedPos = -67f;
	public void OnClick()
	{
        print(pos);
		PhotoReview.Instance.ShowPhoto(pos);
	}
	public void Highlight()
	{
		transform.localPosition = new Vector3(transform.localPosition.x, selectedPos, 0);
        //selected.SetActive(true);
        //highlight.color = higlighted;
        highlight.PlayForward();
	}
	public void UnHighlight()
	{
		//selected.SetActive(false);
		transform.localPosition = new Vector3(transform.localPosition.x, UnSelectedPos, 0);
        //highlight.color = unHiglighted;
        highlight.PlayReverse();
    }
}
