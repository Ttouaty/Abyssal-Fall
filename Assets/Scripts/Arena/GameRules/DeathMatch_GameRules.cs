﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DeathMatch_GameRules : AGameRules
{
	public override void InitGameRules ()
	{
		base.InitGameRules();
		GUIManager.Instance.RunTimer(MatchDuration);
		GUIManager.Instance.Timer.OnCompleteCallback.AddListener(OnTimeOut);

		if(NetworkServer.active)
		{
			for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
			{
				int playerNumber = ServerManager.Instance.RegisteredPlayers[i].PlayerNumber;

				GUIManager.Instance.SetPlayerScoreActive(playerNumber, true);
				GUIManager.Instance.SetPlayerScoreIcon(playerNumber, ServerManager.Instance.RegisteredPlayers[i].CharacterUsed._characterData.Icon);
				GUIManager.Instance.SetPlayerScore(playerNumber, 0);
			}
		}
	}

	public override void OnPlayerDeath_Listener(Player player, Player killer)
	{
		base.OnPlayerDeath_Listener(player, killer);
		if (killer != null)
			GUIManager.Instance.SetPlayerScore(killer.PlayerNumber, killer.Score);
		if (player != null)
			GUIManager.Instance.SetPlayerScore(player.PlayerNumber, player.Score);
	}

	public override void OnPlayerDisconnect(int playerNumber)
	{
		GUIManager.Instance.SetPlayerScoreActive(playerNumber, false);
	}

	private void OnTimeOut()
	{
		if(NetworkServer.active)
		{
			int i;
			int winnerId = 0;
			Player winnerPlayer = ServerManager.Instance.RegisteredPlayers[winnerId];
			for (i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; ++i)
			{
				Player currentPlayer = ServerManager.Instance.RegisteredPlayers[i];
				if (currentPlayer != null)
				{
					currentPlayer.Controller.RpcFreeze();
					if (currentPlayer.Score > winnerPlayer.Score)
					{
						winnerId = i;
						winnerPlayer = currentPlayer;
					}
				}
			}

			Player.LocalPlayer.RpcOnPlayerWin(winnerPlayer.gameObject);
		}
	}

	public override void OnPlayerWin_Listener (Player winner)
	{
		base.OnPlayerWin_Listener(winner);

		// TODO : Gérer les égalités
	

		GUIManager.Instance.Timer.OnCompleteCallback.RemoveListener(OnTimeOut);
	}
}
