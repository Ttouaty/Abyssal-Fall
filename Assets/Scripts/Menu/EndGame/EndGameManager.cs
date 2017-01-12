using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class EndGameManager : GenericSingleton<EndGameManager>
{
	public bool IsOpen = false;
	public int WinnerId = -1;
	
	public Text WinnerText;

	public void ResetGame(bool checkIfOpen = true)
	{
		if (!checkIfOpen || IsOpen)
		{
			Destroy(GameManager.Instance.GameRules.gameObject);
			MainManager.Instance.LEVEL_MANAGER.UnloadScene(LevelManager.Instance.CurrentArenaConfig.BackgroundLevel);
			MainManager.Instance.LEVEL_MANAGER.CurrentArenaConfig = null;
			CameraManager.Instance.Reset();
			MainManager.Instance.LEVEL_MANAGER.OpenMenu();
			ServerManager.Instance.OnGameEnd();
			ServerManager.Instance.ResetNetwork();
		}
	}

	public void BackToCharacterSelect()
	{
		Destroy(GameManager.Instance.GameRules.gameObject);
		MainManager.Instance.LEVEL_MANAGER.UnloadScene(LevelManager.Instance.CurrentArenaConfig.BackgroundLevel);
		CameraManager.Instance.Reset();
		ServerManager.Instance.OnGameEnd();
		if(NetworkServer.active)
			Player.LocalPlayer.CmdBroadCastOpenMenu(false, "Lobby");
		else
			Player.LocalPlayer.RpcOpenMenu(false, "Lobby");
	}

	public override void Init()
	{
		Close();
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
		MenuPauseManager.Instance.CanPause = false;
		if(GUIManager.Instance != null)
			GUIManager.Instance.SetActiveAll(false);
		InputManager.SetInputLockTime(0.5f);
		TimeManager.Pause();
		transform.GetChild(0).gameObject.SetActive(true);
		IsOpen = true;
		WinnerText.GetComponent<Localizator.LocalizedText>().OnChangeLanguage();
		WinnerText.text = WinnerText.text.Replace("%ID%", (WinnerId).ToString());
	}

	public void Close()
	{
		IsOpen = false;
		transform.GetChild(0).gameObject.SetActive(false);
		TimeManager.Resume();
		MenuPauseManager.Instance.CanPause = true;
		if(GUIManager.Instance != null)
			GUIManager.Instance.SetActiveAll(true);
	}
}
