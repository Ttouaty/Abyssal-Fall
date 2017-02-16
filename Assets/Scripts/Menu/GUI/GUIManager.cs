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
		ScoreFields[playerNumber - 1].GetComponentInChildren<Image>().sprite = newSprite;
	}

	public void SetPlayerScore(int playerNumber, int score)
	{
		ScoreFields[playerNumber - 1].GetComponentInChildren<Text>().text = score.ToString();
	}
}
