using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

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
	public IntRule NumberOfRounds;
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

	public virtual void RespawnFalledTiles (Tile tile)
	{
		tile.PrepareRespawn();
		StartCoroutine(RespawnFalledTiles_Implementation(tile));
	}

	protected virtual IEnumerator RespawnFalledTiles_Implementation (Tile tile)
	{
		yield return new WaitForSeconds(TileRegerationTime);
		tile.ActivateRespawn();
	}

	public virtual void OnPlayerDeath_Listener (Player player, Player killer)
	{
		// On player death common stuff
		CameraManager.Instance.RemoveTargetToTrack(player.Controller.transform);
	}

	public virtual void OnPlayerWin_Listener (Player winner)
	{
		// On player win common stuff
		if (NetworkServer.active)
		{
			ServerManager.Instance.ResetAlivePlayers();
		}
		ArenaManager.Instance.DisableBehaviours();
		CameraManager.Instance.ClearTrackedTargets();
	}

	public virtual ParsedGameRules Serialize()
	{
		ParsedGameRules newParsedRules = new ParsedGameRules();
		newParsedRules.NumberOfCharactersRequired = NumberOfCharactersRequired._valueIndex;
		newParsedRules.IsMatchRoundBased = IsMatchRoundBased._valueIndex;
		newParsedRules.CanPlayerRespawn = CanPlayerRespawn._valueIndex;
		newParsedRules.CanFalledTilesRespawn = CanFalledTilesRespawn._valueIndex;
		newParsedRules.NumberOfRounds = NumberOfRounds._valueIndex;
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
		NumberOfRounds._valueIndex = newRule.NumberOfRounds;
		MatchDuration._valueIndex = newRule.MatchDuration;
		TileRegerationTime._valueIndex = newRule.TileRegerationTime;
		PointsGainPerKill._valueIndex = newRule.PointsGainPerKill;
		PointsLoosePerSuicide._valueIndex = newRule.PointsLoosePerSuicide;
		TimeBeforeSuicide._valueIndex = newRule.TimeBeforeSuicide;
	}
}
