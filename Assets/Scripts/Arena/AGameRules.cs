using UnityEngine;
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
		// On init game rules common stuff
		StopAllCoroutines();
		CameraManager.IsManual = false;

		for (int i = 0; i < Player.PlayerList.Length; i++)
		{
			int playerNumber = Player.PlayerList[i].PlayerNumber;

			GUIManager.Instance.SetPlayerScoreActive(playerNumber, true);
			GUIManager.Instance.SetPlayerScoreIcon(playerNumber, Player.PlayerList[i].CharacterUsed._characterData.Portrait);
			//GUIManager.Instance.SetPlayerScore(playerNumber, 0);
		}

		StartCoroutine(Update_Implementation());

		if (ArenaAutoDestruction)
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
			CameraManager.Shake(ShakeStrength.Low, 0.3f);

			yield return new WaitWhile(() => MenuPauseManager.Instance.IsOpen);
			yield return new WaitForSeconds(0.5f);
			yield return new WaitWhile(() => MenuPauseManager.Instance.IsOpen);
			if (NetworkServer.active)
			{
				int[] tempTileArray = ArenaManager.Instance.GetOutsideTiles(_autoDestroyedTileIndex);

				_autoDestroyedTileIndex++;

				ArenaMasterManager.Instance.RpcRemoveTiles(tempTileArray);
			}

			yield return new WaitWhile(() => MenuPauseManager.Instance.IsOpen);
			yield return new WaitForSeconds(IntervalAutoDestruction - 0.5f);
		}
	}

	public virtual void OnPlayerDeath_Listener(Player player, Player killer)
	{
		CameraManager.Instance.RemoveTargetToTrack(player.Controller.transform);

		if (killer != null)
		{
			if (player != null)
				MessageManager.Log("Player " + killer.PlayerNumber+" killed => Player " + player.PlayerNumber);
		}
		else
			MessageManager.Log("Player " + player.PlayerNumber + " killed himself!");

		if (NetworkServer.active)
		{
			if (killer != null)
			{
				killer.Score += PointsGainPerKill;

				if (killer.Score >= ScoreToWin)
				{
					GameManager.Instance.OnRoundEndServer.Invoke(killer);
					return;
				}
			}
			else
			{
				if(player != null)
					player.Score += PointsLoosePerSuicide; // (added because IntRule is negative for display)
			}

		

			if (CanPlayerRespawn && player != null)
				StartCoroutine(RespawnPlayer_Retry(player, 2));
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

			player.Controller.RpcRespawn(tile.transform.position + Vector3.up * tile.transform.localScale.y * 0.5f + Vector3.up * player.Controller.GetComponentInChildren<CapsuleCollider>().height * 0.5f * player.transform.localScale.y);
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
		StopAllCoroutines();

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
		ServerManager.Instance.FreezeAllPlayers();

		for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
		{
			if (ServerManager.Instance.RegisteredPlayers[i].Score >= ScoreToWin)
			{
				Player.LocalPlayer.RpcOnPlayerWin(winner.gameObject);
				return;
			}
		}

		if (winner != null)
			Player.LocalPlayer.RpcOnRoundEnd(winner.gameObject);
		else
			Player.LocalPlayer.RpcOnRoundEnd(null);
	}

	public virtual void OnPlayerWin_Listener(Player winner)
	{
		StopAllCoroutines();
		EndGameManager.Instance.WinnerId = winner.PlayerNumber;
		
		ArenaManager.Instance.DisplayWinner(winner.gameObject);
	}

	public virtual void OnPlayerDisconnect(int playerNumber)
	{
		GameManager.Instance.OnLocalPlayerDeath.Invoke(null, null);
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
