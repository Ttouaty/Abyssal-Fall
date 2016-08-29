using UnityEngine;
using System.Collections;
using System;

public class FreeForAll_GameRules : AGameRules
{
	public override void InitGameRules ()
	{
		base.InitGameRules();
	}

	public override void OnPlayerDeath_Listener (Player player)
	{
		if (GameManager.Instance.AlivePlayers.IndexOf(player) >= 0)
		{
			GameManager.Instance.AlivePlayers.Remove(player);

			// Round won
			if (GameManager.Instance.AlivePlayers.Count == 1)
			{
				// Increment score
				GameManager.Instance.AlivePlayers[0].Score += PointsPerKill;
				// Invoke win event
				GameManager.Instance.OnPlayerWin.Invoke(GameManager.Instance.AlivePlayers[0]);
			}
		}
	}

	public override void OnPlayerWin_Listener (Player player)
	{
		if (player.Score == GameManager.Instance.CurrentGameConfiguration.NumberOfStages)
		{
			EndGameManager.Instance.WinnerId = GameManager.Instance.AlivePlayers[0].PlayerNumber;
			EndGameManager.Instance.Open();
		}
		else
		{
			EndStageManager.Instance.Open();
			if (MainManager.Instance.LEVEL_MANAGER.CurrentModeConfig.IsMatchRoundBased)
			{
				++MainManager.Instance.GAME_MANAGER.CurrentStage;
			}
			else
			{
				GUIManager.Instance.StopTimer();
			}
		}

		ArenaManager.Instance.DisableBehaviours();
		MainManager.Instance.GAME_MANAGER.ResetAlivePlayers();
	}
}
