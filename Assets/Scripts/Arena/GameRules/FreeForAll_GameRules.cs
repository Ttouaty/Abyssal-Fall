using UnityEngine;
using System.Collections;
using System;

public class FreeForAll_GameRules : AGameRules
{
	public override void InitGameRules ()
	{
		base.InitGameRules();
		GUIManager.Instance.RunRoundCount();
	}

	public override void OnPlayerDeath_Listener (Player player, Player killer)
	{
		base.OnPlayerDeath_Listener(player, killer);

		if (ServerManager.Instance.AlivePlayers.IndexOf(player) >= 0)
		{
			ServerManager.Instance.AlivePlayers.Remove(player);
			// Round won
			if (ServerManager.Instance.AlivePlayers.Count == 1)
			{
				// Increment score
				ServerManager.Instance.AlivePlayers[0].Score += PointsGainPerKill;
				// Invoke win event
				GameManager.Instance.OnPlayerWin.Invoke();
			}
		}
	}

	protected override IEnumerator RespawnFalledTiles_Implementation (Tile tile)
	{
		//yield return new WaitForSeconds(5f);
		//GameObjectPool.AddObjectIntoPool(tile.gameObject);
		yield return null;
	}

	public override void OnPlayerWin_Listener ()
	{
		if (ServerManager.Instance.AlivePlayers.Count == 0)
		{
			Debug.LogWarning("DRAW !!! Aucun joueur restant, est ce qu'une personne a déco ?");

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

			base.OnPlayerWin_Listener();

			return;
		}
		Player winner = ServerManager.Instance.AlivePlayers[0];
		winner.Controller.Freeze();
		InputManager.AddInputLockTime(1);

		if (winner.Score == GameManager.Instance.CurrentGameConfiguration.NumberOfStages)
		{
			EndGameManager.Instance.WinnerId = ServerManager.Instance.AlivePlayers[0].PlayerNumber;
			EndGameManager.Instance.Open();
			GUIManager.Instance.StopRoundCount();
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

		base.OnPlayerWin_Listener();
	}
}
