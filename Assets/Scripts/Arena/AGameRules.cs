using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using System;
using FMOD.Studio;

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
	public BoolRule AreTilesFrozen				{ get { return RuleObject.AreTilesFrozen; } }
	public BoolRule ArenaAutoDestruction		{ get { return RuleObject.ArenaAutoDestruction; } }
	public BoolRule RandomArena					{ get { return RuleObject.RandomArena; } }
	public BoolRule CanSuddenDeath				{ get { return RuleObject.CanSuddenDeath; } }

	protected int _autoDestroyedTileIndex = 0;

	public BehaviourConfiguration[] Behaviours;

	public List<PoolConfiguration> AdditionalPoolsToLoad = new List<PoolConfiguration>();

	protected bool _isInSuddenDeath = false;
	[HideInInspector]
	public EventInstance ActiveMusic;

	public virtual void InitMusic() { }

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

		if (ArenaAutoDestruction && !AreTilesFrozen)
			StartCoroutine(ArenaAutoDestruction_Implementation());
	}

	public virtual IEnumerator Update_Implementation()
	{
		yield return null;
	}

	public virtual void RespawnFallenTiles(Tile tile)
	{
		if (!CanFallenTilesRespawn)
			return;
		tile.PrepareRespawn();
		StartCoroutine(RespawnFallenTiles_Implementation(tile));
	}

	protected virtual IEnumerator RespawnFallenTiles_Implementation(Tile tile)
	{
		if (!CanFallenTilesRespawn)
			yield break;
		yield return new WaitForSeconds(TileRegerationTime);
		//if(Array.IndexOf(ArenaManager.Instance.Tiles, tile) != -1)	
		tile.ActivateRespawn();
	}

	protected virtual IEnumerator ArenaAutoDestruction_Implementation()
	{
		_autoDestroyedTileIndex = 0;
		yield return new WaitForSeconds(TimeBeforeAutoDestruction - 1);

		while (true)
		{
			CameraManager.Shake(ShakeStrength.Low, 0.2f);

			yield return new WaitWhile(() => MenuPauseManager.Instance.IsOpen);
			yield return new WaitForSeconds(0.5f);
			yield return new WaitWhile(() => MenuPauseManager.Instance.IsOpen);
			if (NetworkServer.active)
			{
				int[] tempTileArray = ArenaManager.Instance.GetOutsideTiles(_autoDestroyedTileIndex);

				_autoDestroyedTileIndex++;

				ArenaMasterManager.Instance.RpcRemoveTiles(tempTileArray);
			}

			if (_autoDestroyedTileIndex > ArenaManager.Instance.CurrentMapConfig.MapSize.x * 0.5f)
			{

				Debug.Log("_autoDestroyedTileIndex is > ArenaManager.Instance.CurrentMapConfig.MapSize.x *0.5f => "+ (ArenaManager.Instance.CurrentMapConfig.MapSize.x * 0.5f)+" reseting to 0");
				_autoDestroyedTileIndex = 0;
			}
			yield return new WaitWhile(() => MenuPauseManager.Instance.IsOpen);
			yield return new WaitForSeconds(IntervalAutoDestruction - 0.5f);
			yield return new WaitWhile(() => MenuPauseManager.Instance.IsOpen);
		}
	}

	public virtual void OnPlayerDeath_Listener(Player player, Player killer)
	{
		CameraManager.Instance.RemoveTargetToTrack(player.Controller.transform);

		if(_isInSuddenDeath)
		{
			if (NetworkServer.active)
			{
				if (ServerManager.Instance.AlivePlayers.IndexOf(player) >= 0)
				{
					ServerManager.Instance.AlivePlayers.Remove(player);
					// Round won
					if (ServerManager.Instance.AlivePlayers.Count == 1)
					{
						Player.LocalPlayer.RpcOnPlayerWin(ServerManager.Instance.AlivePlayers[0].gameObject);
					}
				}
			}
		}
		else
		{
			if (killer != null)
			{
				if (player != null)
				{
					GUIManager.Instance.DisplayKill(killer.PlayerNumber, player.PlayerNumber);
					//MessageManager.Log("Player " + killer.PlayerNumber+" killed => Player " + player.PlayerNumber);
				}
			}
			else
				GUIManager.Instance.DisplaySuicide(player.PlayerNumber);

			//MessageManager.Log("Player " + player.PlayerNumber + " killed himself!");

			if (NetworkServer.active)
			{
				if (killer != null)
				{
					killer.Score += PointsGainPerKill;

					if (killer.Score >= ScoreToWin && ScoreToWin != -1)
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

	}

	private void RespawnPlayer(Player player)
	{
		IEnumerable<Tile> tilesEnumerator = ArenaManager.Instance.Tiles.Where((Tile t) => t != null).Where((Tile t) => (t.Obstacle == null && t.CanFall && !t.IsTouched));
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
			tile.SetTimeLeft(tile.TimeLeftSave * 2);

			if(player.Controller != null)
				player.Controller.RpcRespawn(
				tile.transform.position + Vector3.up * tile.transform.localScale.y * 0.5f + Vector3.up * player.Controller.GetComponentInChildren<CapsuleCollider>().height * 0.5f * player.transform.localScale.y,
				tile.TileIndex);
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

		List<GameObject> HightestScorePlayers = new List<GameObject>();

		if(IsMatchRoundBased)
		{
			for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
			{
				if (ServerManager.Instance.RegisteredPlayers[i].Score >= ScoreToWin)
				{
					HightestScorePlayers.Add(ServerManager.Instance.RegisteredPlayers[i].gameObject);
				}
			}
		}
		else
		{
			HightestScorePlayers.Add(ServerManager.Instance.RegisteredPlayers[0].gameObject);

			for (int i = 1; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
			{
				if(HightestScorePlayers[0].GetComponent<Player>().Score < ServerManager.Instance.RegisteredPlayers[i].Score)
				{
					HightestScorePlayers.Clear();
					HightestScorePlayers.Add(ServerManager.Instance.RegisteredPlayers[i].gameObject);
				}
				else if(HightestScorePlayers[0].GetComponent<Player>().Score == ServerManager.Instance.RegisteredPlayers[i].Score)
				{
					HightestScorePlayers.Add(ServerManager.Instance.RegisteredPlayers[i].gameObject);
				}
			}
		}

		if(HightestScorePlayers.Count > 1)
		{
			if(CanSuddenDeath)
			{
				GameConfiguration newConfig = GameManager.Instance.CurrentGameConfiguration;

				switch (HightestScorePlayers.Count)
				{
					case 1: newConfig.MapConfiguration = EMapConfiguration.TestArena_2; break;
					case 2: newConfig.MapConfiguration = EMapConfiguration.TestArena_2; break;
					case 3: newConfig.MapConfiguration = EMapConfiguration.TestArena_3; break;
					case 4: newConfig.MapConfiguration = EMapConfiguration.TestArena_4; break;
				}

				newConfig.ModeConfiguration = EModeConfiguration.FreeForAll;
				MainManager.Instance.DYNAMIC_CONFIG.GetConfig(newConfig.MapConfiguration, out LevelManager.Instance.CurrentMapConfig);
				newConfig.MapFileUsedIndex = UnityEngine.Random.Range(0, LevelManager.Instance.CurrentMapConfig.MapFiles.Length);


				Player.LocalPlayer.RpcSuddenDeath(HightestScorePlayers.ToArray(), newConfig);
				return;
			}
		}
		else if(HightestScorePlayers.Count == 1)
		{
			Player.LocalPlayer.RpcOnPlayerWin(HightestScorePlayers[0]);
			return;
		}

		if (winner != null)
			Player.LocalPlayer.RpcOnRoundEnd(winner.gameObject);
		else
			Player.LocalPlayer.RpcOnRoundEnd(null);
	}

	public IEnumerator SuddenDeath(params GameObject[] sdPlayers)
	{
		_isInSuddenDeath = true;
		MenuPauseManager.Instance.CanPause = false;
		MenuPauseManager.Instance.Close();
		InputManager.SetInputLockTime(2);

		AutoFade.StartFade(0.5f, 0.5f, 0.5f, Color.white);
		yield return new WaitForSeconds(0.5f);

		SoundManager.Instance.DestroyInstance(ActiveMusic, STOP_MODE.IMMEDIATE);
		ActiveMusic = SoundManager.Instance.CreateInstance(LevelManager.Instance.CurrentArenaConfig.ClassicMusicKey);
		ActiveMusic.start();
		ActiveMusic.setParameterValue("END", 1);

		ArenaManager.Instance._currentArenaConfig = MainManager.Instance.LEVEL_MANAGER.CurrentArenaConfig;
		ArenaManager.Instance._currentModeConfig = MainManager.Instance.LEVEL_MANAGER.CurrentModeConfig;
		ArenaManager.Instance._currentMapConfig = MainManager.Instance.LEVEL_MANAGER.CurrentMapConfig;
		yield return ArenaManager.Instance.ResetMapCo(false, sdPlayers);

		GameObject tempPopup = Instantiate(GameManager.Instance.Popups["SuddenDeath"], Camera.main.transform, false) as GameObject;
		tempPopup.transform.localPosition = new Vector3(0,-2,10);
		tempPopup.transform.localRotation = Quaternion.identity;
	}


	public virtual void OnPlayerWin_Listener(Player winner)
	{
		StopAllCoroutines();
		EndGameManager.Instance.WinnerId = winner.PlayerNumber;
		SoundManager.Instance.DestroyInstance(ActiveMusic, STOP_MODE.ALLOWFADEOUT);
		SoundManager.Instance.PlayOS("Victory Music");
		ArenaManager.Instance.DisplayWinner(winner.gameObject);
	}

	public virtual void OnPlayerDisconnect(int playerNumber)
	{
		GUIManager.Instance.SetPlayerScoreActive(playerNumber, false);
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
