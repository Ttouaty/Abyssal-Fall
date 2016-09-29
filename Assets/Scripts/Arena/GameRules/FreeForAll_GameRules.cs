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

		if (GameManager.Instance.AlivePlayers.IndexOf(player) >= 0)
		{
			GameManager.Instance.AlivePlayers.Remove(player);
			// Round won
			if (GameManager.Instance.AlivePlayers.Count == 1)
			{
				// Increment score
				GameManager.Instance.AlivePlayers[0].Score += PointsGainPerKill;
				// Invoke win event
				GameManager.Instance.OnPlayerWin.Invoke();
			}
		}
	}

	public override void RespawnFalledTiles (Tile tile)
	{
		base.RespawnFalledTiles(tile);
		GameObjectPool.AddObjectIntoPool(tile.gameObject);
	}

	public override void OnPlayerWin_Listener ()
	{

		Player winner = GameManager.Instance.AlivePlayers[0];
		winner.Controller.Freeze();

		if (winner.Score == GameManager.Instance.CurrentGameConfiguration.NumberOfStages)
		{
			EndGameManager.Instance.WinnerId = GameManager.Instance.AlivePlayers[0].PlayerNumber;
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
