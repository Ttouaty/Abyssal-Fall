using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class GUIManager : GenericSingleton<GUIManager>
{
	public GUITimer			Timer;
	public GUIRoundCount	RoundCount;
	public Canvas			CanvasRef;

	[Space]
	public GameObject[] ScoreFields;

	public override void Init ()
	{
		base.Init();
		Timer.gameObject.SetActive(false);
		RoundCount.gameObject.SetActive(false);
		for (int i = 0; i < ScoreFields.Length; i++)
		{
			SetPlayerScoreActive(i + 1, false);
		}
	}

	void Update()
	{
		for (int i = 0; i < Player.PlayerList.Length; i++)
		{
			if (!ScoreFields[Player.PlayerList[i].PlayerNumber - 1].activeInHierarchy)
				continue;

			if (Player.PlayerList[i].Score != Mathf.Floor(Player.PlayerList[i].Score))
				ScoreFields[Player.PlayerList[i].PlayerNumber - 1].GetComponentInChildren<Text>().text =  Player.PlayerList[i].Score.ToString("0.0");
			else
				ScoreFields[Player.PlayerList[i].PlayerNumber - 1].GetComponentInChildren<Text>().text = Player.PlayerList[i].Score.ToString("0");

		}
	}

	public void RunTimer (float duration)
	{
		Timer.gameObject.SetActive(true);
		Timer.Run(duration);
	}

	public void StopTimer ()
	{
		Timer.gameObject.SetActive(false);
		Timer.Stop();
	}

	public void RunRoundCount ()
	{
		RoundCount.gameObject.SetActive(true);
		UpdateRoundCount(GameManager.Instance.CurrentStage);
	}

	public void UpdateRoundCount (int round)
	{
		RoundCount.SetRound(round);
	}

	public void SetActiveRoundCount (bool active)
	{
		RoundCount.gameObject.SetActive(active);
	}

	public void SetActiveAll(bool active)
	{
		CanvasRef.gameObject.SetActive(active);
	}

	public void SetPlayerScoreActive(int playerNumber, bool active)
	{
		ScoreFields[playerNumber - 1].SetActive(active);
	}

	public void SetPlayerScoreIcon(int playerNumber, Sprite newSprite)
	{
		ScoreFields[playerNumber - 1].transform.Find("BG portrait").GetChild(0).GetComponent<Image>().sprite = newSprite;
		ScoreFields[playerNumber - 1].transform.Find("BG portrait").GetComponent<Image>().color = GameManager.Instance.PlayerColors[playerNumber - 1];
	}

	//public void SetPlayerScore(int playerNumber, int score)
	//{
	//	ScoreFields[playerNumber - 1].GetComponentInChildren<Text>().text = score.ToString();
	//}
}
