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
			ServerManager.Instance.OnGameEnd();
			ServerManager.Instance.ResetNetwork();
			MainManager.Instance.LEVEL_MANAGER.OpenMenu(false, "Main");
		}
	}

	public void BackToCharacterSelect()
	{
		Destroy(GameManager.Instance.GameRules.gameObject);
		MainManager.Instance.LEVEL_MANAGER.UnloadScene(LevelManager.Instance.CurrentArenaConfig.BackgroundLevel);

		CameraManager.Instance.Reset();
		ServerManager.Instance.OnGameEnd();
		if(NetworkServer.active)
		{
			for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
			{
				if (ServerManager.Instance.RegisteredPlayers[i].Controller != null)
				{
					Debug.Log("Destroying character " + ServerManager.Instance.RegisteredPlayers[i].Controller._characterData.IngameName);
					Destroy(ServerManager.Instance.RegisteredPlayers[i].Controller.gameObject);
				}
			}
			ServerManager.Instance.ForceUnready = true;
			Player.LocalPlayer.RpcOpenMenu(false, "CharacterSelect", true);
		}
		else
			Player.LocalPlayer.RpcOpenMenu(false, "CharacterSelect", false);

		MenuPanelNew.GlobalInputDelay = 4;
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
		MenuPauseManager.Instance.Close();
		if(GUIManager.Instance != null)
			GUIManager.Instance.SetActiveAll(false);
		InputManager.SetInputLockTime(1f);
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
