using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AGameRules : MonoBehaviour 
{

	/*
	 * Il faut remplacer tout les ints & bool  par des IntRule & BoolRule
	 * & il faut faire suivre les custom editors
	 */
	public bool IsMatchRoundBased;
	public IntRule NumberOfRounds;
	public IntRule MatchDuration;

	public BoolRule CanFalledTilesRespawn;
	public IntRule TileRegerationTime;

	public IntRule PointsGainPerKill;
	public IntRule PointsLoosePerSuicide;
	public IntRule TimeBeforeSuicide;

	public bool CanPlayerRespawn;
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
		ServerManager.Instance.ResetAlivePlayers();
		ArenaManager.Instance.DisableBehaviours();
		CameraManager.Instance.ClearTrackedTargets();
	}
}
