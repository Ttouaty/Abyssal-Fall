using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct PlayerField
{
	public Text PlayerRoot;
	public Text Score;

	public void Enable(bool value)
	{
		PlayerRoot.gameObject.SetActive(value);
	}
}

public class EndStageScreen : MonoBehaviour
{
	public Text PlayerXWins;
	public PlayerField[] Players = new PlayerField[4];
	public Text Countdown;

	public void Enable ()
	{
		gameObject.SetActive(true);
	}

	public void Disable()
	{
		gameObject.SetActive(false);
	}

	public void ShowPanel ()
	{
		Enable();
		for(var i = 0; i < GameManager.instance.RegisteredPlayers.Length; ++i)
		{
			SetFieldEnable(i, GameManager.instance.RegisteredPlayers[i] == 1);
			SetPlayerScore(i, GameManager.instance.PlayersScores[i]);
		}
	}

	public void SetPlayerScore (int id, int score)
	{
		Players[id].Score.text = score + " Rounds";
	}

	public void SetFieldEnable (int id, bool value)
	{
		Players[id].PlayerRoot.gameObject.SetActive(value);
	}

	public IEnumerator StartCountdown (int idWinner, int duration = 2)
	{
		PlayerXWins.text = "Player " + (idWinner + 1) + " wins";
		Countdown.text = "";
		yield return new WaitForSeconds(4);
		Countdown.text = "Next battle in \n";
		while (duration > 0)
		{
			Countdown.text += duration + "... ";
			--duration;
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(0.5f);
		Disable();
	}
}