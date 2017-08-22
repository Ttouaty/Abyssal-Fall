using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GUIManager : GenericSingleton<GUIManager>
{
	public GUITimer Timer;
	public GUIRoundCount RoundCount;
	public Canvas CanvasRef;

	[Space]
	public ScoreField[] ScoreFields;

	public GameObject KillFeedContainer;
	public Transform KillPopup;
	public Transform SuicidePopup;

	private float _nextPopupY = 0;
	private float _targetContainerDisplacementY = 0;
	private float _popupDelayBeforeDestroy = 3;
	private float _popupSize = 25;

	public override void Init()
	{
		base.Init();
		Timer.gameObject.SetActive(false);
		RoundCount.gameObject.SetActive(false);
		for (int i = 0; i < ScoreFields.Length; i++)
		{
			SetPlayerScoreActive(i + 1, false);
		}

		StartCoroutine(ContainerFollowUp());
	}

	void Update()
	{
		for (int i = 0; i < Player.PlayerList.Length; i++)
		{
			if (!ScoreFields[Player.PlayerList[i].PlayerNumber - 1].gameObject.activeInHierarchy)
				continue;

			if (Player.PlayerList[i].Score != Mathf.Floor(Player.PlayerList[i].Score))
				ScoreFields[Player.PlayerList[i].PlayerNumber - 1].Score.text = Player.PlayerList[i].Score.ToString("0.00");
			else
				ScoreFields[Player.PlayerList[i].PlayerNumber - 1].Score.text = Player.PlayerList[i].Score.ToString("0");

			ScoreFields[Player.PlayerList[i].PlayerNumber - 1].PingDiv.SetActive(Player.PlayerList[i].Ping != 0);
			ScoreFields[Player.PlayerList[i].PlayerNumber - 1].HomeDiv.SetActive(Player.PlayerList[i].Ping == 0);
		}
	}

	public void RunTimer(float duration)
	{
		Timer.gameObject.SetActive(true);
		Timer.Run(duration);
	}

	public void StopTimer()
	{
		Timer.gameObject.SetActive(false);
		Timer.Stop();
	}

	public void RunRoundCount()
	{
		RoundCount.gameObject.SetActive(true);
		UpdateRoundCount(GameManager.Instance.CurrentStage);
	}

	public void UpdateRoundCount(int round)
	{
		RoundCount.SetRound(round);
	}

	public void SetActiveRoundCount(bool active)
	{
		RoundCount.gameObject.SetActive(active);
	}

	public void SetActiveAll(bool active)
	{
		CanvasRef.gameObject.SetActive(active);
	}

	public void SetPlayerScoreActive(int playerNumber, bool active)
	{
		ScoreFields[playerNumber - 1].gameObject.SetActive(active);
	}

	public void SetPlayerScoreIcon(int playerNumber, Sprite newSprite)
	{
		ScoreFields[playerNumber - 1].transform.Find("BG portrait").GetChild(0).GetComponent<Image>().sprite = newSprite;
		ScoreFields[playerNumber - 1].transform.Find("BG portrait").GetComponent<Image>().color = GameManager.Instance.PlayerColors[playerNumber - 1];
	}

	public void SetPlayerPingTo(int playerNumber)
	{
		ScoreFields[playerNumber - 1].PingDiv.GetComponentInChildren<PingDisplay>(true).Activate(Player.GetPlayerWithNumber(playerNumber));
	}

	public void DisplaySuicide(int playerNumber)
	{
		Transform newPopup = Instantiate(SuicidePopup, KillFeedContainer.transform, false) as Transform;

		newPopup.Find("Victim").GetComponent<Image>().color = GameManager.Instance.PlayerColors[playerNumber - 1];
		newPopup.Find("Victim").GetChild(0).GetComponent<Image>().sprite = Player.PlayerList[playerNumber - 1].CharacterUsed._characterData.Portrait;

		StartCoroutine(MovePopupInView(newPopup));
	}

	public void DisplayKill(int killerPlayerNumber, int victimPlayerNumber)
	{
		Transform newPopup = Instantiate(KillPopup, KillFeedContainer.transform, false) as Transform;

		newPopup.Find("Killer").GetComponent<Image>().color = GameManager.Instance.PlayerColors[killerPlayerNumber - 1];
		newPopup.Find("Killer").GetChild(0).GetComponent<Image>().sprite = Player.PlayerList[killerPlayerNumber - 1].CharacterUsed._characterData.Portrait;

		newPopup.Find("Method").GetChild(0).GetComponent<Image>().sprite = Player.PlayerList[killerPlayerNumber - 1].CharacterUsed._characterData.LightIcon;

		newPopup.Find("Victim").GetComponent<Image>().color = GameManager.Instance.PlayerColors[victimPlayerNumber - 1];
		newPopup.Find("Victim").GetChild(0).GetComponent<Image>().sprite = Player.PlayerList[victimPlayerNumber - 1].CharacterUsed._characterData.Portrait;

		StartCoroutine(MovePopupInView(newPopup));
	}

	IEnumerator MovePopupInView(Transform popupToMove)
	{
		float popupTime = 0.1f;
		popupToMove.localPosition = new Vector3(-50, -_nextPopupY, 0);

		_nextPopupY += _popupSize;

		CanvasGroup targetCanvasGroup = popupToMove.GetComponentInChildren<CanvasGroup>();
		Vector3 startPos = popupToMove.localPosition;


		for (float t = 0; t < popupTime; t += Time.deltaTime)
		{
			popupToMove.localPosition = Vector3.Lerp(startPos, new Vector3(0, popupToMove.localPosition.y, 0), t / popupTime);
			targetCanvasGroup.alpha = Mathf.Lerp(0, 1, t / popupTime);
			yield return null;
		}

		popupToMove.localPosition = popupToMove.localPosition.ZeroX();
		targetCanvasGroup.alpha = 1;

		yield return new WaitForSeconds(_popupDelayBeforeDestroy);

		startPos = popupToMove.localPosition;
		for (float t = 0; t < popupTime; t += Time.deltaTime)
		{
			popupToMove.localPosition = Vector3.Lerp(startPos, new Vector3(50, popupToMove.localPosition.y, 0), t / popupTime);
			targetCanvasGroup.alpha = Mathf.Lerp(1, 0, t / popupTime);
			yield return null;
		}
		targetCanvasGroup.alpha = 0;

		_targetContainerDisplacementY += _popupSize;

		Destroy(popupToMove.gameObject);
	}

	IEnumerator ContainerFollowUp()
	{
		while(true)
		{
			KillFeedContainer.transform.localPosition = Vector3.Lerp(KillFeedContainer.transform.localPosition, KillFeedContainer.transform.localPosition.SetAxis(Axis.y, _targetContainerDisplacementY * KillFeedContainer.transform.localScale.y) , 10 * Time.deltaTime);
			yield return null;
		}
	}

	//public void SetPlayerScore(int playerNumber, int score)
	//{
	//	ScoreFields[playerNumber - 1].GetComponentInChildren<Text>().text = score.ToString();
	//}
}
