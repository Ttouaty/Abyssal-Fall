using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct GameConfiguration
{
	public EArenaConfiguration ArenaConfiguration;
	public EModeConfiguration ModeConfiguration;
	public EMapConfiguration MapConfiguration;
	public int NumberOfStages;
}

[System.Serializable]
public class GameEvent : UnityEvent<Player> { }

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
			OnPlayerDeath.RemoveListener(_gameRules.OnPlayerDeath_Listener);
			OnPlayerWin.RemoveListener(_gameRules.OnPlayerWin_Listener);

			_gameRules = value;

			OnPlayerDeath.AddListener(_gameRules.OnPlayerDeath_Listener);
			OnPlayerWin.AddListener(_gameRules.OnPlayerWin_Listener);
		}
	}

	public AudioClip OnGroundDrop;
	public AudioClip OnObstacleDrop;
	public AudioClip OnThree;
	public AudioClip OnTwo;
	public AudioClip OnOne;
	public AudioClip OnGo;
	public AudioSource AudioSource;
	public AudioSource GameLoop;

	[HideInInspector]
	public Player[] RegisteredPlayers = new Player[4];
	[HideInInspector]
	public int nbPlayers = 0;

	public GameEvent OnPlayerDeath;
	public GameEvent OnPlayerWin;

	[Space()]
	public GameConfiguration  CurrentGameConfiguration;

	public int CurrentStage = 0;
	private List<Player> _alivePlayers;
	public List<Player> AlivePlayers { get { return _alivePlayers; } }

	public void StartGame()
	{
		ResetAlivePlayers();
		// DEBUG en attendant que la sélection de la map soit dispo
		switch (_alivePlayers.Count)
		{
			case 2: CurrentGameConfiguration.MapConfiguration = EMapConfiguration.TestArena20x20; break;
			case 3: CurrentGameConfiguration.MapConfiguration = EMapConfiguration.TestArena26x26; break;
			case 4: CurrentGameConfiguration.MapConfiguration = EMapConfiguration.TestArena32x32; break;
		}

		CurrentStage = 1;
		LevelManager.Instance.StartLevel(CurrentGameConfiguration);

		InProgress = true;
	}

	public static void ResetRegisteredPlayers()
	{
		Instance.RegisteredPlayers = new Player[4];
		Instance.nbPlayers = 0;
	}

	public void ResetAlivePlayers ()
	{
		_alivePlayers = new List<Player>();
		for (int i = 0; i < RegisteredPlayers.Length; ++i)
		{
			if (RegisteredPlayers[i] != null)
			{
				_alivePlayers.Add(RegisteredPlayers[i]);
			}
		}
	}
}