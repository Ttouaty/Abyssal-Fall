using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DeathMatch_GameRules : AGameRules
{
	public override void InitGameRules ()
	{
		base.InitGameRules();
		GUIManager.Instance.RunTimer(MatchDuration);
		GUIManager.Instance.Timer.OnCompleteCallback.AddListener(OnPlayerWin_Listener);
	}

	public override void OnPlayerDeath_Listener (Player player, Player killer)
	{
		base.OnPlayerDeath_Listener(player, killer);

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
			tile.SetTimeLeft(tile.TimeLeft);

			Spawn spawn = tile.gameObject.AddComponent<Spawn>();

			spawn.SpawnPlayer(player.Controller);
			player.Controller._animator.SetTrigger("Reset");
			player.Controller._isDead = false;

			CameraManager.Instance.AddTargetToTrack(player.Controller.transform);
		}
	}

	private IEnumerator RespawnPlayer_Retry (Player player, float delay)
	{
		yield return new WaitForSeconds(delay);
		RespawnPlayer(player);
	}

	public override void OnPlayerWin_Listener ()
	{
		base.OnPlayerWin_Listener();

		// TODO : Gérer les égalités
		int i;
		int winnerId		= 0;
		Player winnerPlayer = GameManager.Instance.RegisteredPlayers[winnerId];
		for (i = 0; i < GameManager.Instance.RegisteredPlayers.Length; ++i)
		{
			Player currentPlayer = GameManager.Instance.RegisteredPlayers[i];
			if (currentPlayer != null)
			{
				currentPlayer.Controller.Freeze();
				if (currentPlayer.Score > winnerPlayer.Score)
				{
					winnerId		= i;
					winnerPlayer	= currentPlayer;
				}
			}
		}

		EndGameManager.Instance.WinnerId = winnerId;
		EndGameManager.Instance.Open();

		GUIManager.Instance.Timer.OnCompleteCallback.RemoveListener(OnPlayerWin_Listener);
	}
}
