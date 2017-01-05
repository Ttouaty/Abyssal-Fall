using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuPauseManager : GenericSingleton<MenuPauseManager>
{
	public bool IsOpen                  = false;
	public PlayerScore[] ScoresFields   = new PlayerScore[4];
	public bool CanPause                = true;

	void Update ()
	{
		//for(int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; ++i)
		//{
		//	Player player = ServerManager.Instance.RegisteredPlayers[i];
		//	if(player != null)
		//	{
		//		if(InputManager.GetButtonDown("Start", player.JoystickNumber) && CanPause)
		//		{
		//			Toggle();
		//		}
		//	}
		//}
	}

	public override void Init ()
	{
		IsOpen = true;
		Close();

		for (int i = 0; i < Player.LocalPlayer.PlayerList.Length; ++i)
		{
			if(Player.LocalPlayer.PlayerList[i] != null)
			{
				ScoresFields[i].CurrentPlayer = Player.LocalPlayer.PlayerList[i].GetComponent<Player>();
			}
			ScoresFields[i].Init();
		}
	}

	public void Toggle ()
	{
		if(IsOpen)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	public void Open ()
	{
		if (IsOpen)
			return;

		InputManager.SetInputLockTime(0.3f);

		for (int i = 0; i < ScoresFields.Length; ++i)
		{
			ScoresFields[i].DisplayScore();
		}
		transform.GetChild(0).gameObject.SetActive(true);

		if (ServerManager.Instance.ExternalPlayerNumber == 0 && Player.LocalPlayer.isServer)
		{
			TimeManager.Pause();
			List<Player> alives = ServerManager.Instance.AlivePlayers;
			if (alives != null)
			{
				for (int i = 0; i < alives.Count; ++i)
				{
					if (alives[i].Controller != null)
					{
						alives[i].Controller.Freeze();
					}
				}
			}
		}

		IsOpen = true;
	}

	public void BackToMainMenu()
	{
		EndGameManager.Instance.ResetGame(false);
	}

	public void Close ()
	{
		if (!IsOpen)
			return;

		TimeManager.Resume();
		transform.GetChild(0).gameObject.SetActive(false);
		IsOpen = false;

		List<Player> alives = ServerManager.Instance.AlivePlayers;
		if(alives != null)
		{
			for (int i = 0; i < alives.Count; ++i)
			{
				if(alives[i].Controller != null)
				{
					alives[i].Controller.UnFreeze();
				}
			}
		}
	}
}
