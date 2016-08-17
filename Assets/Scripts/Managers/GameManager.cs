using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ZoomEvent : UnityEvent { }
[System.Serializable]
public class PlayerEvent : UnityEvent<GameObject> { }

public class GameManager : GenericSingleton<GameManager>
{
	public static bool InProgress = false;

    public ZoomEvent OnZoom;
	public PlayerEvent OnPlayerDeath;
	public PlayerEvent OnPlayerWin;

	public GameObject CountdownScreen;
	public GameObject EndStageScreen;
	public GameObject[] PlayersRefs;

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
	[SerializeField]
	private bool isInDebugMode = false;

	void Awake ()
	{
		if (isInDebugMode)
			return;
	}

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
		OnZoom.AddListener(Camera.main.GetComponent<CameraManager>().OnZoom);
		OnAllLoadablesLoaded();
	}

	public void Restart ()
	{
        OnAllLoadablesLoaded();
	}

	void OnAllLoadablesLoaded()
	{
		LevelManager.Instance.StartLevel(EArenaConfiguration.Aerial, EModeConfiguration.FreeForAll, EMapConfiguration.TestArena);
	}
}
