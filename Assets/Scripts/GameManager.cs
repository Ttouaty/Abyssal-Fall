using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	private Text _loadingType;
	private int _index;
	private Loadable[] _loadables;

	public GameObject LoadingScreen;
	public GameObject CountdownScreen;
	public Arena Arena;
	public GameObject[] PlayersRefs;

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

	// Use this for initialization
	void Start ()
	{
		OpenLoadingScreen();
		AddEvents();

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
			OnAllLoadablesLoaded();
		}
	}

	void OnAllLoadablesLoaded()
	{
		_loadingType.text = "Building Arena";
		Arena.StartGame();
		LoadingScreen.SetActive(false);
	}
}
