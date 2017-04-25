using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct GameConfiguration
{
	public EArenaConfiguration ArenaConfiguration;
	public EModeConfiguration ModeConfiguration;
	public EMapConfiguration MapConfiguration;
	public int MapFileUsedIndex;
}

[System.Serializable]
public class GameEventDeath : UnityEvent<Player, Player> { }
[System.Serializable]
public class GameEventWin : UnityEvent<Player> { }

public class GameManager : GenericSingleton<GameManager>
{
	public static bool InProgress = false;

	private AGameRules _gameRules;
	public AGameRules GameRules
	{
		get
		{
			return _gameRules;
		}
		set
		{
			if (_gameRules != null)
			{
				OnLocalPlayerDeath.RemoveListener(_gameRules.OnPlayerDeath_Listener);
				OnPlayerWin.RemoveListener(_gameRules.OnPlayerWin_Listener);
				OnRoundEndServer.RemoveListener(_gameRules.OnRoundEnd_Listener_Server);
				OnRoundEnd.RemoveListener(_gameRules.OnRoundEnd_Listener);
			}

			_gameRules = value;

			OnLocalPlayerDeath.AddListener(_gameRules.OnPlayerDeath_Listener);
			OnPlayerWin.AddListener(_gameRules.OnPlayerWin_Listener);
			OnRoundEnd.AddListener(_gameRules.OnRoundEnd_Listener);
			OnRoundEndServer.AddListener(_gameRules.OnRoundEnd_Listener_Server);
		}
	}

	public AudioClip OnGroundDrop; //TODO Switch to FmodOneShotSound
	public AudioClip OnObstacleDrop;
	public AudioClip OnThree;
	public AudioClip OnTwo;
	public AudioClip OnOne;
	public AudioClip OnGo;
	public AudioSource AudioSource;
	public AudioSource GameLoop;

	[Space]
	public Color[] PlayerColors;
	public GameObject[] PlayerNumberImages;
	[Space]

	//[HideInInspector]
	//public Player[] RegisteredPlayers = new Player[4];
	//// [HideInInspector]
	//public int nbPlayers = -1;

	public GameEventDeath OnLocalPlayerDeath;
	public GameEventWin OnRoundEndServer;
	public GameEventWin OnRoundEnd;
	public GameEventWin OnPlayerWin;

	[Space()]
	public GameConfiguration CurrentGameConfiguration;

	public int CurrentStage = 0;

	public void StartGame()
	{
		ServerManager.Instance.ResetAlivePlayers();
		// DEBUG en attendant que la sélection de la map soit dispo
		

		if (NetworkServer.active)
		{
			switch (ServerManager.Instance.AlivePlayers.Count)
			{
				case 1: CurrentGameConfiguration.MapConfiguration = EMapConfiguration.TestArena_2; break;
				case 2: CurrentGameConfiguration.MapConfiguration = EMapConfiguration.TestArena_2; break;
				case 3: CurrentGameConfiguration.MapConfiguration = EMapConfiguration.TestArena_3; break;
				case 4: CurrentGameConfiguration.MapConfiguration = EMapConfiguration.TestArena_4; break;
			}

			MenuManager.Instance.GetComponentInChildren<MapSelectWheel>(true).SendSelectionToGameManager();
			AGameRules tempRules;
			MainManager.Instance.DYNAMIC_CONFIG.GetConfig(CurrentGameConfiguration.ModeConfiguration, out tempRules);

			for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
			{
				ServerManager.Instance.RegisteredPlayers[i].Score = 0;
			}

			MainManager.Instance.DYNAMIC_CONFIG.GetConfig(CurrentGameConfiguration.MapConfiguration, out LevelManager.Instance.CurrentMapConfig);

			CurrentGameConfiguration.MapFileUsedIndex = UnityEngine.Random.Range(0, LevelManager.Instance.CurrentMapConfig.MapFiles.Length);

			Player.LocalPlayer.RpcStartGame(CurrentGameConfiguration, tempRules.Serialize());
		}
		else
			Debug.LogWarning("StartGame(); called from client! This should not be allowed! Aborting");
	}

	public void RpcChangeCurrentMapFileUsedIndex(int newIndex)
	{
		CurrentGameConfiguration.MapFileUsedIndex = newIndex;
	}


	public void StartGameWithConfig(GameConfiguration newConfig, int[] customConfig)
	{
		CurrentGameConfiguration = newConfig;
		CurrentStage = 1;
		StartCoroutine(StartLevelCoroutine(customConfig));
	}

	private IEnumerator StartLevelCoroutine(int[] customConfig)
	{
		yield return StartCoroutine(AutoFade.StartFade(1, LevelManager.Instance.StartLevel(CurrentGameConfiguration, customConfig), AutoFade.EndFade(0.5f, 1, Color.black), Color.black));
		InProgress = true;
	}
}