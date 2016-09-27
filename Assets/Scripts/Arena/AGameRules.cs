using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AGameRules : MonoBehaviour 
{

	/*
	 * Il faut remplacer tout les ints & bool  par des IntRule & BoolRule
	 * & il faut faire suivre les custom editors
	 */
	public bool IsMatchRoundBased = true;
	public int NumberOfRounds = 5;
	public int MatchDuration = 180;

	public bool CanFalledTilesRespawn = false;
	public int TileRegerationTime = 0;

	public int PointsGainPerKill = 1;
	public int PointsLoosePerSuicide = 0;
	public int TimeBeforeSuicide = 5;

	public bool CanPlayerRespawn = false;
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

	public virtual void OnPlayerWin_Listener ()
	{
		// On player win common stuff
		GameManager.Instance.ResetAlivePlayers();
		ArenaManager.Instance.DisableBehaviours();
		CameraManager.Instance.ClearTrackedTargets();
	}
}
