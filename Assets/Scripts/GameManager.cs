using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class ZoomEvent : UnityEvent { }

public class GameManager : MonoBehaviour
{
	public static GameManager instance;


	private Text _loadingType;
	private int _index;
	private Loadable[] _loadables;

	public ZoomEvent OnZoom;
	public GameObject LoadingScreen;
	public GameObject CountdownScreen;
	public Arena Arena;
	public GameObject[] PlayersRefs;

	public int[] RegisteredPlayers = { 0, 0, 0, 0 };  

	void Awake ()
	{
		instance = this;

		if(Arena == null)
		{
			Debug.LogError("Arena reference is missing");
			Debug.Break();
		}

		_loadables = GameObject.FindObjectsOfType<Loadable>();
	}


	public static void StartGame()
	{
		instance.Init();
	}

	public static void ResetRegisteredPlayers()
	{
		for (int i = 0; i < 4; ++i) { instance.RegisteredPlayers[i] = 0; }
	}
	// Use this for initialization
	private void Init ()
	{
		OpenLoadingScreen();
		AddEvents();

		OnZoom.AddListener(Camera.main.GetComponent<CameraManager>().OnZoom);

		_index = 0;
		_loadables[_index].Init();
	}

	void OpenLoadingScreen ()
	{
		_loadingType = LoadingScreen.transform.FindChild("LoadingType").GetComponent<Text>();
		_loadingType.text = "Init core";
		LoadingScreen.SetActive(true);
	}

	void AddEvents ()
	{
		for (var i = 0; i < _loadables.Length; ++i)
		{
			Loadable loadable = _loadables[i];
			loadable.OnMessage.AddListener(OnChangeLoadingMessage);
			loadable.OnLoadComplete.AddListener(OnLoadableComplete);
		}
	}

	void OnChangeLoadingMessage(string msg)
	{
		_loadingType.text = msg;
	}

	void OnLoadableComplete()
	{
		++_index;
		if(_index < _loadables.Length)
		{
			_loadables[_index].Init();
		}
		else
		{
			StartCoroutine(OnAllLoadablesLoaded());
		}
	}

	IEnumerator OnAllLoadablesLoaded()
	{
		LoadingScreen.SetActive(false);
		yield return StartCoroutine(Arena.StartGame());
		yield return null;
	}
}
