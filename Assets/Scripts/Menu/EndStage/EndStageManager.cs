﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class EndStageManager : GenericSingleton<EndStageManager>
{
	public bool IsOpen = false;
	private PlayerScore[] ScoresFields;

	public void ResetMap(bool checkIfOpen = true)
	{
		if (!checkIfOpen || IsOpen)
		{
			if(NetworkServer.active)
			{
				if(GameManager.Instance.GameRules.RandomArena)
					Player.LocalPlayer.RpcResetmap(false, UnityEngine.Random.Range(0, LevelManager.Instance.CurrentMapConfig.MapFiles.Length));
				else
					Player.LocalPlayer.RpcResetmap(false, GameManager.Instance.CurrentGameConfiguration.MapFileUsedIndex);
			}
		}
	}

	public override void Init()
	{
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

	public void Toggle()
	{
		if (IsOpen)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	public void Open()
	{
		MenuPauseManager.Instance.Close();
		MenuPauseManager.Instance.CanPause = false;
		InputManager.SetInputLockTime(0.7f);
		TimeManager.Pause();

		for (int i = 0; i < ScoresFields.Length; ++i)
		{
			ScoresFields[i].DisplayScore();
		}

		//StageText.GetComponent<Localizator.LocalizedText>().OnChangeLanguage();
		//StageText.text = StageText.text.Replace("%NUMBER%", GameManager.Instance.CurrentStage.ToString());
		transform.GetChild(0).gameObject.SetActive(true);
		IsOpen = true;
		if(GUIManager.Instance != null)
			GUIManager.Instance.SetActiveAll(false);
	}

	public void Close()
	{
		TimeManager.Resume();
		transform.GetChild(0).gameObject.SetActive(false);
		IsOpen = false;
		MenuPauseManager.Instance.CanPause = true;
		if(GUIManager.Instance != null)
			GUIManager.Instance.SetActiveAll(true);
	}
}
