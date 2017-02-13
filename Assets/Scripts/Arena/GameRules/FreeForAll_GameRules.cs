using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class FreeForAll_GameRules : AGameRules
{
	private Coroutine roundEndCoroutine;

	public override void InitGameRules ()
	{
		base.InitGameRules();
		GUIManager.Instance.RunRoundCount();
	}

	public override void OnPlayerDeath_Listener (Player player, Player killer)
	{
		base.OnPlayerDeath_Listener(player, killer);

		// + 1 Point to last alive
		if(NetworkServer.active)
		{
			if (ServerManager.Instance.AlivePlayers.IndexOf(player) >= 0)
			{
				ServerManager.Instance.AlivePlayers.Remove(player);
				// Round won
				if (ServerManager.Instance.AlivePlayers.Count == 1)
				{
					// Invoke win event after 1 sec
					roundEndCoroutine = StartCoroutine(DelayRoundEnd());
				}
			}
			else
			{
				Debug.LogError("DRAW DETECTED !");
				StopCoroutine(roundEndCoroutine);
				Player.LocalPlayer.RpcOnRoundEnd(null);
			}
		}
	}

	private IEnumerator DelayRoundEnd()
	{
		yield return new WaitForSeconds(1);
		if(ServerManager.Instance.AlivePlayers.Count == 0)
			Player.LocalPlayer.RpcOnRoundEnd(null);
		else
			Player.LocalPlayer.RpcOnRoundEnd(ServerManager.Instance.AlivePlayers[0].gameObject);
	}

	public override void OnPlayerWin_Listener (Player winner)
	{
		if (winner == null)
		{
			Debug.LogWarning("DRAW !!!");
			MessageManager.Log("Draw! No Point awarded !");
			

			base.OnPlayerWin_Listener(winner);

			return;
		}
		//Player winner = ServerManager.Instance.AlivePlayers[0];
		winner.Controller.Freeze();
		winner.Score++;
		InputManager.AddInputLockTime(1);

		if (winner.Score >= GameManager.Instance.CurrentGameConfiguration.NumberOfStages)
		{
			EndGameManager.Instance.WinnerId = winner.PlayerNumber;
			EndGameManager.Instance.Open();
			GUIManager.Instance.SetActiveRoundCount(false);
		}
		else
		{
			EndStageManager.Instance.Open();
			if (MainManager.Instance.LEVEL_MANAGER.CurrentModeConfig.IsMatchRoundBased)
			{
				++GameManager.Instance.CurrentStage;
				GUIManager.Instance.UpdateRoundCount(GameManager.Instance.CurrentStage);
			}
			else
			{
				GUIManager.Instance.StopTimer();
			}
		}

		base.OnPlayerWin_Listener(winner);
	}
}
