﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

public abstract class AGameRules : MonoBehaviour
{

	/*
	 * Il faut remplacer tout les ints & bool  par des IntRule & BoolRule
	 * & il faut faire suivre les custom editors
	 */
	public ModeConfiguration_SO RuleObject;

	public BoolRule IsMatchRoundBased			{ get { return RuleObject.IsMatchRoundBased; } }
	public IntRule NumberOfCharactersRequired	{ get { return RuleObject.NumberOfCharactersRequired; } }
	public IntRule ScoreToWin					{ get { return RuleObject.ScoreToWin; } }
	public IntRule MatchDuration				{ get { return RuleObject.MatchDuration; } }
	public IntRule TileRegerationTime			{ get { return RuleObject.TileRegerationTime; } }
	public IntRule PointsGainPerKill			{ get { return RuleObject.PointsPerKill; } }
	public IntRule PointsLoosePerSuicide		{ get { return RuleObject.PointsPerSuicide; } }
	public IntRule TimeBeforeSuicide			{ get { return RuleObject.TimeBeforeSuicide; } }
	public IntRule TimeBeforeAutoDestruction	{ get { return RuleObject.TimeBeforeAutoDestruction; } }
	public IntRule IntervalAutoDestruction		{ get { return RuleObject.IntervalAutoDestruction; } }

	public BoolRule CanPlayerRespawn			{ get { return RuleObject.CanPlayerRespawn; } }
	public BoolRule CanFallenTilesRespawn		{ get { return RuleObject.CanFallenTilesRespawn; } }
	public BoolRule ArenaAutoDestruction		{ get { return RuleObject.ArenaAutoDestruction; } }

	protected int _autoDestroyedTileIndex = 0;

	public List<Tile> RespawnZones = new List<Tile>();

	public BehaviourConfiguration[] Behaviours;

	public List<PoolConfiguration> AdditionalPoolsToLoad = new List<PoolConfiguration>();

	public virtual void InitGameRules()
	{
		// On initi game rules common stuff
		StartCoroutine(Update_Implementation());
		if(ArenaAutoDestruction)
			StartCoroutine(ArenaAutoDestruction_Implementation());

	}

	public virtual IEnumerator Update_Implementation()
	{
		yield return null;
	}

	public virtual void RespawnFallenTiles(Tile tile)
	{
		tile.PrepareRespawn();
		StartCoroutine(RespawnFallenTiles_Implementation(tile));
	}

	protected virtual IEnumerator RespawnFallenTiles_Implementation(Tile tile)
	{
		if (!CanFallenTilesRespawn)
			yield break;
		yield return new WaitForSeconds(TileRegerationTime);
		tile.ActivateRespawn();
	}

	protected virtual IEnumerator ArenaAutoDestruction_Implementation()
	{
		_autoDestroyedTileIndex = 0;
		yield return new WaitForSeconds(TimeBeforeAutoDestruction - 1);

		while (true)
		{
			CameraManager.Shake(ShakeStrength.Medium, 0.5f);
			yield return new WaitForSeconds(1);
			if (NetworkServer.active)
			{
				int[] tempTileArray = ArenaManager.Instance.GetOutsideTiles(_autoDestroyedTileIndex);

				_autoDestroyedTileIndex++;

				ArenaMasterManager.Instance.RpcRemoveTiles(tempTileArray);
			}
			yield return new WaitForSeconds(IntervalAutoDestruction - 1);
		}
	}

	public virtual void OnPlayerDeath_Listener(Player player, Player killer)
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

			if (CanPlayerRespawn)
				StartCoroutine(RespawnPlayer_Retry(player, 1));
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

			player.Controller.RpcRespawn(tile.transform.position + Vector3.up * 1.5f);
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

	public virtual void OnRoundEnd_Listener_Server(Player winner)
	{
		ServerManager.Instance.ResetAlivePlayers();

		if (winner != null)
		{
			if (winner.Score >= ScoreToWin)
			{
				Player.LocalPlayer.RpcOnPlayerWin(winner.gameObject);
				return;
			}

			Player.LocalPlayer.RpcOnRoundEnd(winner.gameObject);
		}

		Player.LocalPlayer.RpcOnRoundEnd(null);
	}

	public virtual void OnPlayerWin_Listener(Player winner)
	{
		EndGameManager.Instance.WinnerId = winner.PlayerNumber;
		EndGameManager.Instance.Open();
	}

	public virtual int[] Serialize()
	{
		List<int> tempList = new List<int>();

		foreach (var prop in RuleObject.GetType().GetFields())
		{
			if (prop.FieldType == typeof(BoolRule))
			{
				if (prop.GetValue(RuleObject) != null)
					tempList.Add(((BoolRule)prop.GetValue(RuleObject))._valueIndex);
				else
					tempList.Add(0);
			}
			else if (prop.FieldType == typeof(IntRule))
			{
				if (prop.GetValue(RuleObject) != null)
					tempList.Add(((IntRule)prop.GetValue(RuleObject))._valueIndex);
				else
					tempList.Add(0);
			}
			else if (prop.FieldType == typeof(EnumRule))
			{
				if (prop.GetValue(RuleObject) != null)
					tempList.Add(((EnumRule)prop.GetValue(RuleObject))._valueIndex);
				else
					tempList.Add(0);
			}
		}

		return tempList.ToArray();
	}

	public virtual void Parse(int[] newRules)
	{
		int i = 0;
		foreach (var prop in RuleObject.GetType().GetFields())
		{
			if (prop.FieldType == typeof(BoolRule))
				((BoolRule)prop.GetValue(RuleObject))._valueIndex = newRules[i];
			else if (prop.FieldType == typeof(IntRule))
				((IntRule)prop.GetValue(RuleObject))._valueIndex  = newRules[i];
			else if (prop.FieldType == typeof(EnumRule))
				((EnumRule)prop.GetValue(RuleObject))._valueIndex = newRules[i];

			i++;
		}
	}
}
