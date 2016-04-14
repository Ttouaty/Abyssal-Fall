using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ZoomEvent : UnityEvent { }
[System.Serializable]
public class PlayerEvent : UnityEvent<GameObject> { }

public class GameManager : MonoBehaviour
{
	public static GameManager instance;
	public static bool InProgress = false;

	private int _index;

	public ZoomEvent OnZoom;
	public PlayerEvent OnPlayerDeath;
	public PlayerEvent OnPlayerWin;

	public GameObject CountdownScreen;
	public GameObject EndStageScreen;
	public Arena Arena;
	public GameObject[] PlayersRefs;

	public int[] RegisteredPlayers = { 0, 0, 0, 0 };  
	public int[] PlayersScores = { 0, 0, 0, 0 };

	void Awake ()
	{
		instance = this;

		if(Arena == null)
		{
			Debug.LogError("Arena reference is missing");
			Debug.Break();
		}

		Application.LoadLevelAdditive("Menu");
	}

	public static void StartGame()
	{
		instance.Init();
		InProgress = true;
	}

	public static void ResetRegisteredPlayers()
	{
		for (int i = 0; i < 4; ++i) { instance.RegisteredPlayers[i] = 0; }
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
