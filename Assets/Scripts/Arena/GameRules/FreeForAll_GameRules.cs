using UnityEngine;
using UnityEngine.Networking;
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

		if(NetworkServer.active)
		{
			if (ServerManager.Instance.AlivePlayers.IndexOf(player) >= 0)
			{
				ServerManager.Instance.AlivePlayers.Remove(player);
				// Round won
				if (ServerManager.Instance.AlivePlayers.Count == 1)
				{
					// Increment score (pas en FFA)
					ServerManager.Instance.AlivePlayers[0].Score += PointsGainPerKill;
					// Invoke win event
					Player.LocalPlayer.RpcOnPlayerWin(ServerManager.Instance.AlivePlayers[0].gameObject);
				}
			}
			else
			{
				Debug.LogError("DRAW DETECTED !");
				Player.LocalPlayer.RpcOnPlayerWin(null);
			}
		}

	}

	protected override IEnumerator RespawnFalledTiles_Implementation (Tile tile)
	{
		//yield return new WaitForSeconds(5f);
		//GameObjectPool.AddObjectIntoPool(tile.gameObject);
		yield return null;
	}

	public override void OnPlayerWin_Listener (Player winner)
	{
		if (winner == null)
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

			base.OnPlayerWin_Listener(winner);

			return;
		}
		//Player winner = ServerManager.Instance.AlivePlayers[0];
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

		base.OnPlayerWin_Listener(winner);
	}
}
