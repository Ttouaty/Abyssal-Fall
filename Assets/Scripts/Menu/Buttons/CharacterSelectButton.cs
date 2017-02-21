using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelectButton : InputListener
{
	void Awake()
	{
		SetVisibility(false);
	}

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

	public override void LaunchCallback(int joy)
	{
		if (ServerManager.Instance.AreAllPlayerReady)
			base.LaunchCallback(joy);
	}

	public void SetVisibility(bool isVisible)
	{
		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(isVisible);
		}
		
	}
}
