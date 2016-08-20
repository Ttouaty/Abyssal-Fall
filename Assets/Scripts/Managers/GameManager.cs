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
}

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

    [Space()]
    public GameConfiguration  CurrentGameConfiguration;

	public static void StartGame()
	{
		Instance.Init();
		InProgress = true;
	}

	public static void ResetRegisteredPlayers()
	{
		Instance.RegisteredPlayers = new Player[4];
		Instance.nbPlayers = 0;
	}
	// Use this for initialization
	private void Init ()
	{
		OnAllLoadablesLoaded();
	}

	public void Restart ()
	{
        OnAllLoadablesLoaded();
	}

	void OnAllLoadablesLoaded()
	{
		LevelManager.Instance.StartLevel(CurrentGameConfiguration);
	}
}