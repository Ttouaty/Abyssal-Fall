﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelectButton : InputListener
{
	protected override void Start()
	{
 		base.Start();
		this.ListenToAllJoysticks = false;
	}

	protected override void Update()
	{
		for(int i = 0; i < GameManager.Instance.nbPlayers; ++i)
		{
			JoysticksToListen[i] = GameManager.Instance.RegisteredPlayers[i].JoystickNumber;
		}

		SetVisibility(GameManager.Instance.AreAllPlayerReady && GameManager.Instance.nbPlayers >= 2);
		if (GameManager.Instance.AreAllPlayerReady && GameManager.Instance.nbPlayers >= 2) //doublon mais fuk :p
 			base.Update();

	}

	protected override void LaunchCallback(int joy)
	{
		if (GameManager.Instance.AreAllPlayerReady)
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
