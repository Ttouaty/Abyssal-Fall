using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndStageManager : GenericSingleton<EndStageManager>
{
	public bool IsOpen = false;
	public PlayerScore[] ScoresFields = new PlayerScore[4];
	public Text StageText;

	public void ResetMap(bool checkIfOpen = true)
	{
		if (!checkIfOpen || IsOpen)
		{
			Player.LocalPlayer.RpcResetmap(false);
		}
	}

	public override void Init()
	{
		Close();

		for (int i = 0; i < Player.LocalPlayer.PlayerList.Length; ++i)
		{
			if (Player.LocalPlayer.PlayerList[i] != null)
			{
				ScoresFields[i].CurrentPlayer = Player.LocalPlayer.PlayerList[i].GetComponent<Player>();
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

		StageText.GetComponent<Localizator.LocalizedText>().OnChangeLanguage();
		StageText.text = StageText.text.Replace("%NUMBER%", GameManager.Instance.CurrentStage.ToString());
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
