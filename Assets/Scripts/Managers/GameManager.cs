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

    private int _currentStage = 0;
    public int CurrentStage { get { return _currentStage; } }

	public bool AreAllPlayerReady
	{
		get
		{
			if (nbPlayers == 0)
				return false;

			for (int i = 0; i < nbPlayers; ++i)
			{
				if (!RegisteredPlayers[i].isReady)
					return false;
			}
			return true;
		}
	}

    private List<Player> _alivePlayers;

    void Start ()
    {
        OnPlayerDeath.AddListener(OnPlayerDeath_Listener);
    }

    void OnPlayerDeath_Listener(Player player)
    {
        if(_alivePlayers.IndexOf(player) >= 0)
        {
            _alivePlayers.Remove(player);
            
            if(_alivePlayers.Count == 1)
            {
                // Win !!!
                ++_alivePlayers[0].Score;
                OnPlayerWin.Invoke(_alivePlayers[0]);

                if(_alivePlayers[0].Score == CurrentGameConfiguration.NumberOfStages)
                {
                    EndGameManager.Instance.WinnerId = _alivePlayers[0].PlayerNumber;
                    EndGameManager.Instance.Open();
                }
                else
                {
                    EndStageManager.Instance.Open();
                    ResetAlivePlayers();
                    ++_currentStage;
                }
            }
        }
    }

	public static void StartGame()
	{
        Instance.Init();
		InProgress = true;
	}

	public override void Init ()
    {
        ResetAlivePlayers();
        _currentStage = 1;
        LevelManager.Instance.StartLevel(CurrentGameConfiguration);
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