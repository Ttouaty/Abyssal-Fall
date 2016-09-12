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

	public override void OnPlayerDeath_Listener (PlayerController player, PlayerController killer)
	{
		base.OnPlayerDeath_Listener(player, killer);

		if (killer != null)
		{
			killer._playerRef.Score += PointsGainPerKill;
			--player._playerRef.Score;
		}
		else
		{
			player._playerRef.Score -= PointsLoosePerSuicide;
		}

		RespawnPlayer(player);
	}

	private void RespawnPlayer (PlayerController player)
	{
		IEnumerable<Tile> tilesEnumerator = ArenaManager.Instance.Tiles.Where((Tile t) => t.Obstacle == null && !t.IsFalling/* && !t.IsSpawn*/);
		List<Tile> tiles = new List<Tile>(tilesEnumerator);
		if(tiles.Count == 0)
		{
			// Normalement ne devrait jamais passer ici, mais au cas où
			StartCoroutine(RespawnPlayer_Retry(player, 1.0f));
		}
		else
		{
			Tile tile = tiles.RandomElement();
			tile.SetTimeLeft(1.5f);	

			Spawn spawn = tile.gameObject.AddComponent<Spawn>();

			spawn.SpawnPlayer(player);
			player._animator.SetTrigger("Reset");
			player._isDead = false;
		}
	}

	private IEnumerator RespawnPlayer_Retry (PlayerController player, float delay)
	{
		yield return new WaitForSeconds(delay);
		RespawnPlayer(player);
	}

	public override void OnPlayerWin_Listener ()
	{
		base.OnPlayerWin_Listener();

		// TODO : Gérer les égalités
		int winnerId		= 0;
		Player winnerPlayer = GameManager.Instance.RegisteredPlayers[winnerId];
		for (int i = 0; i < GameManager.Instance.RegisteredPlayers.Length; ++i)
		{
			Player currentPlayer = GameManager.Instance.RegisteredPlayers[i];
			if (currentPlayer != null)
			{
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
