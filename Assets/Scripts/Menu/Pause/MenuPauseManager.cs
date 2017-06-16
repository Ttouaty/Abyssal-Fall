using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuPauseManager : GenericSingleton<MenuPauseManager>
{
	public bool IsOpen                  = false;
	public bool CanPause                = true;
	private PlayerScore[] ScoresFields;

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

		ScoresFields = GetComponentsInChildren<PlayerScore>(true);
		for (int i = 0; i < Player.PlayerList.Length; ++i)
		{
			if (Player.PlayerList[i] != null)
			{
				ScoresFields[i].CurrentPlayer = Player.PlayerList[i].GetComponent<Player>();
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

		SoundManager.Instance.GetInstance(LevelManager.Instance.CurrentArenaConfig.AmbianceSound).setPaused(IsOpen);
		//Stop Music Here
	}

	public void Open ()
	{
		if (IsOpen || !CanPause)
			return;


		for (int i = 0; i < ScoresFields.Length; ++i)
		{
			ScoresFields[i].DisplayScore();
		}

		transform.GetChild(0).gameObject.SetActive(true);
		InputManager.SetInputLockTime(0.7f);

		MenuPanelNew.ActiveMenupanel = null;
		MenuPanelNew.InputEnabled = true;
		MenuPanelNew.PanelRefs["Pause"].Open(false);

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

	public void BackToCharacterSelect()
	{
		EndGameManager.Instance.BackToCharacterSelect();
	}

	public void Close ()
	{
		if (!IsOpen)
			return;

		TimeManager.Resume();
		transform.GetChild(0).gameObject.SetActive(false);
		IsOpen = false;
		//MenuPanelNew.PanelRefs["Pause"].Close();

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
