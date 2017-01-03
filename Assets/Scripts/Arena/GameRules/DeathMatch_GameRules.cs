using UnityEngine;
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
	}

	public override void OnPlayerDeath_Listener (Player player, Player killer)
	{
		base.OnPlayerDeath_Listener(player, killer);

		if(NetworkServer.active)
		{
			if (killer != null)
			{
				killer.Score += PointsGainPerKill;
				--player.Score;
			}
			else
			{
				player.Score -= PointsLoosePerSuicide;
			}
			RespawnPlayer(player);
		}


		
	}

	private void RespawnPlayer (Player player)
	{
		IEnumerable<Tile> tilesEnumerator = ArenaManager.Instance.Tiles.Where((Tile t) => t.Obstacle == null && !t.IsFalling/* && !t.IsSpawn*/);
		List<Tile> tiles = new List<Tile>(tilesEnumerator);
		if(tiles.Count == 0)
		{
			// Cas où aucune tile n'est dispo pour faire spawn un player
			// Normalement ne devrait jamais passer ici, mais au cas où
			StartCoroutine(RespawnPlayer_Retry(player, 1.0f));
		}
		else
		{
			Tile tile = tiles.RandomElement();
			tile.SetTimeLeft(tile.TimeLeftSave);

			//Spawn spawn = tile.gameObject.AddComponent<Spawn>();
			//transform.position + Vector3.up
			//spawn.SpawnPlayer(player.Controller);
			
			player.Controller.RpcRespawn(tile.transform.position + Vector3.up);
		}
	}

	private IEnumerator RespawnPlayer_Retry (Player player, float delay)
	{
		yield return new WaitForSeconds(delay);
		RespawnPlayer(player);
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
					currentPlayer.Controller.Freeze();
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
		EndGameManager.Instance.WinnerId = winner.PlayerNumber;
		EndGameManager.Instance.Open();

		GUIManager.Instance.Timer.OnCompleteCallback.RemoveListener(OnTimeOut);
	}
}
