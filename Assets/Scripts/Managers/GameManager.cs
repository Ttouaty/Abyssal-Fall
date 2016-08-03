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
	public static GameManager instance;
	public static bool InProgress = false;

	public ZoomEvent OnZoom;
	public PlayerEvent OnPlayerDeath;
	public PlayerEvent OnPlayerWin;

	public GameObject CountdownScreen;
	public GameObject EndStageScreen;
	public Arena Arena;
	public GameObject[] PlayersRefs;

	public AudioClip OnGroundDrop;
	public AudioClip OnObstacleDrop;
	public AudioClip OnThree;
	public AudioClip OnTWo;
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
		instance = this;

		if (isInDebugMode)
			return;

		AudioSource = GetComponent<AudioSource>();

		if (Arena == null)
		{
			Debug.LogError("Arena reference is missing");
			Debug.Break();
		}

		GameLoop.volume = 0.1f;

		Application.LoadLevelAdditive("Menu");
	}

	public static void StartGame()
	{
		instance.Init();
		InProgress = true;
	}

	public static void ResetRegisteredPlayers()
	{
		instance.RegisteredPlayers = new Player[4];
		instance.nbPlayers = 0;
	}
	// Use this for initialization
	private void Init ()
	{
		OnZoom.AddListener(Camera.main.GetComponent<CameraManager>().OnZoom);
		StartCoroutine(OnAllLoadablesLoaded());
	}

	public void Restart ()
	{
		StartCoroutine(OnAllLoadablesLoaded());
	}

	IEnumerator OnAllLoadablesLoaded()
	{
		yield return StartCoroutine(Arena.StartGame());
		yield return null;
	}
}
