using UnityEngine;
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

	protected override void LaunchCallback()
	{
		if (GameManager.Instance.AreAllPlayerReady)
			base.LaunchCallback();
	}

	public void SetVisibility(bool isVisible)
	{
		GetComponent<Image>().enabled = isVisible;
		
		transform.GetChild(0).gameObject.SetActive(isVisible);
		
	}
}
