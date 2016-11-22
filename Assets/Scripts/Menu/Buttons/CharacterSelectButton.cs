using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelectButton : InputListener
{
	protected override void Start()
	{
 		base.Start();
	}

	protected override void Update()
	{
		SetVisibility(ServerManager.Instance.AreAllPlayerReady);
		if (ServerManager.Instance.AreAllPlayerReady) //doublon mais fuk :p
 			base.Update();
	}

	protected override void LaunchCallback(int joy)
	{
		if (ServerManager.Instance.AreAllPlayerReady)
			base.LaunchCallback(joy);
	}

	Color tempColor;
	public void SetVisibility(bool isVisible)
	{
		tempColor = GetComponent<Image>().color;
		tempColor.a = isVisible ? 1 : 0.3f;
		GetComponent<Image>().color = tempColor;

		transform.GetChild(0).gameObject.SetActive(isVisible);
		
	}
}
