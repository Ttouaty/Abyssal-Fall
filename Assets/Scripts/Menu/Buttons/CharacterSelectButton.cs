using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelectButton : InputButton
{
	protected override void Start()
	{
 		base.Start();
		this.ListenToAllJoysticks = false;
	}
	protected override void Update()
	{
		// Fait a l'arrache mais c'est pas très grave :p
		for(int i = 0; i < GameManager.Instance.nbPlayers; ++i)
		{
			JoysticksToListen[i] = GameManager.Instance.RegisteredPlayers[i].JoystickNumber;
		}

		if (GetComponent<Button>() != null)
		{
			GetComponent<Button>().interactable = GameManager.Instance.AreAllPlayerReady;
		}

 		base.Update();

	}

	protected override void LaunchCallback()
	{
		if (GameManager.Instance.AreAllPlayerReady)
			base.LaunchCallback();
	}
}
