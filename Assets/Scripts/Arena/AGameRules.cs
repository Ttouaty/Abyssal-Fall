using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

public struct ParsedGameRules
{
	public int NumberOfCharactersRequired;
	public int IsMatchRoundBased;
	public int CanPlayerRespawn;
	public int CanFalledTilesRespawn;
	public int NumberOfRounds;
	public int MatchDuration;
	public int TileRegerationTime;
	public int PointsGainPerKill;
	public int PointsLoosePerSuicide;
	public int TimeBeforeSuicide;
}

public abstract class AGameRules : MonoBehaviour 
{

	/*
	 * Il faut remplacer tout les ints & bool  par des IntRule & BoolRule
	 * & il faut faire suivre les custom editors
	 */
	public BoolRule IsMatchRoundBased;
	public IntRule NumberOfCharactersRequired;
	public IntRule ScoreToWin;
	public IntRule MatchDuration;

	public BoolRule CanFalledTilesRespawn;
	public IntRule TileRegerationTime;

	public IntRule PointsGainPerKill;
	public IntRule PointsLoosePerSuicide;
	public IntRule TimeBeforeSuicide;

	public BoolRule CanPlayerRespawn;
	public List<Tile> RespawnZones = new List<Tile>();

	public BehaviourConfiguration[] Behaviours;

	public List<PoolConfiguration> AdditionalPoolsToLoad = new List<PoolConfiguration>();

	public virtual void InitGameRules ()
	{
		// On initi game rules common stuff
		StartCoroutine(Update_Implementation());
	}

	public virtual IEnumerator Update_Implementation ()
	{
		yield return null;
	}

	public virtual void RespawnFallenTiles (Tile tile)
	{
		tile.PrepareRespawn();
		StartCoroutine(RespawnFallenTiles_Implementation(tile));
	}

	protected virtual IEnumerator RespawnFallenTiles_Implementation (Tile tile)
	{
		if (!CanFalledTilesRespawn)
			yield break;
		yield return new WaitForSeconds(TileRegerationTime);
		tile.ActivateRespawn();
	}

	public virtual void OnPlayerDeath_Listener (Player player, Player killer)
	{
		CameraManager.Instance.RemoveTargetToTrack(player.Controller.transform);

		if (NetworkServer.active)
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

			if(CanPlayerRespawn)
				RespawnPlayer(player);
		}
	}

	private void RespawnPlayer(Player player)
	{
		IEnumerable<Tile> tilesEnumerator = ArenaManager.Instance.Tiles.Where((Tile t) => t != null).Where((Tile t) => t.Obstacle == null && t.CanFall/* && !t.IsSpawn*/);
		List<Tile> tiles = new List<Tile>(tilesEnumerator);
		if (tiles.Count == 0)
		{
			// Cas où aucune tile n'est dispo pour faire spawn un player
			// Normalement ne devrait jamais passer ici, mais au cas où
			Debug.LogError("No respawn tile found !");
			StartCoroutine(RespawnPlayer_Retry(player, 1.0f));
		}
		else
		{
			Tile tile = tiles.RandomElement();
			tile.SetTimeLeft(tile.TimeLeftSave);

			player.Controller.RpcRespawn(tile.transform.position + Vector3.up * 2.25f);
		}
	}

	private IEnumerator RespawnPlayer_Retry(Player player, float delay)
	{
		yield return new WaitForSeconds(delay);
		RespawnPlayer(player);
	}

	public virtual void OnRoundEnd_Listener(Player winner)
	{
		ArenaManager.Instance.DisableBehaviours();
		CameraManager.Instance.ClearTrackedTargets();

		if (NetworkServer.active)
		{
			ServerManager.Instance.ResetAlivePlayers();

			if (winner != null)
			{
				if (winner.Score >= ScoreToWin)
				{
					Player.LocalPlayer.RpcOnPlayerWin(winner.gameObject);
				}
			}
		}

		/*
		Avoir une fonction server only & celle la
		*/

		if(winner.Score < ScoreToWin)
			EndStageManager.Instance.Open();
		if (IsMatchRoundBased)
		{
			++GameManager.Instance.CurrentStage;
			GUIManager.Instance.UpdateRoundCount(GameManager.Instance.CurrentStage);
		}
		else
		{
			GUIManager.Instance.StopTimer();
		}
	}

	public virtual void OnPlayerWin_Listener (Player winner)
	{
		
	}

	public virtual ParsedGameRules Serialize()
	{
		ParsedGameRules newParsedRules = new ParsedGameRules();
		newParsedRules.NumberOfCharactersRequired = NumberOfCharactersRequired._valueIndex;
		newParsedRules.IsMatchRoundBased = IsMatchRoundBased._valueIndex;
		newParsedRules.CanPlayerRespawn = CanPlayerRespawn._valueIndex;
		newParsedRules.CanFalledTilesRespawn = CanFalledTilesRespawn._valueIndex;
		newParsedRules.NumberOfRounds = ScoreToWin._valueIndex;
		newParsedRules.MatchDuration = MatchDuration._valueIndex;
		newParsedRules.TileRegerationTime = TileRegerationTime._valueIndex;
		newParsedRules.PointsGainPerKill = PointsGainPerKill._valueIndex;
		newParsedRules.PointsLoosePerSuicide = PointsLoosePerSuicide._valueIndex;
		newParsedRules.TimeBeforeSuicide = TimeBeforeSuicide._valueIndex;
		return newParsedRules;
	}

	public virtual void Parse(ParsedGameRules newRule)
	{
		NumberOfCharactersRequired._valueIndex = newRule.NumberOfCharactersRequired;
		IsMatchRoundBased._valueIndex = newRule.IsMatchRoundBased;
		CanPlayerRespawn._valueIndex = newRule.CanPlayerRespawn;
		CanFalledTilesRespawn._valueIndex = newRule.CanFalledTilesRespawn;
		ScoreToWin._valueIndex = newRule.NumberOfRounds;
		MatchDuration._valueIndex = newRule.MatchDuration;
		TileRegerationTime._valueIndex = newRule.TileRegerationTime;
		PointsGainPerKill._valueIndex = newRule.PointsGainPerKill;
		PointsLoosePerSuicide._valueIndex = newRule.PointsLoosePerSuicide;
		TimeBeforeSuicide._valueIndex = newRule.TimeBeforeSuicide;
	}
}
