using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using System.Linq;

public class MenuManager : GenericSingleton<MenuManager>
{
	[HideInInspector]
	public Canvas _canvas;
	public GameObject MiniLoading;
	public Image LoadingIn;
	public Image LoadingOut;
	public GameObject SplashScreens;

	public FmodOneShotSound ReturnSound;
	[Space]
	public MenuPanelNew FirstMenuPanel;

	private bool[] _controllerAlreadyInUse = new bool[12];
	[HideInInspector]
	public CharacterSelector _characterSlotsContainerRef;
	private RawImage[] _splashscreens;
	private Coroutine _miniLoadingCoroutine;
	private bool _needFTUE = false;

	[HideInInspector]
	public List<int> LocalJoystickBuffer = new List<int>();

	public bool NeedFTUE { get { return _needFTUE; } }

	protected override void Awake()
	{

		_canvas = GetComponentInChildren<Canvas>();
		_splashscreens = SplashScreens.GetComponentsInChildren<RawImage>();
		_characterSlotsContainerRef = GetComponentInChildren<CharacterSelector>(true);
	
		_needFTUE = !PlayerPrefs.HasKey("FTUEDone");
	}

	void Start()
	{
		_canvas.worldCamera = Camera.main;
		MiniLoading.SetActive(false);

		if (_needFTUE)
			MenuPanelNew.PanelRefs["FTUE"].Open();

		CameraManager.OnCameraChange.AddListener(OnCameraChange);
	}

	public void OnCameraChange()
	{
		_canvas.worldCamera = Camera.main;
	}

	void Update()
	{
		//if (Input.GetKeyDown(KeyCode.Alpha1))
		//{
		//	LoadPreview(EArenaConfiguration.Aerial);
		//}
		//else if (Input.GetKeyDown(KeyCode.Alpha2))
		//{
		//	LoadPreview(EArenaConfiguration.Hell);
		//}
	}
	
	private void ResetPlayers()
	{
		//Trashy way but fuck it.
		_controllerAlreadyInUse = new bool[12];
		LocalJoystickBuffer.Clear();
		ServerManager.ResetRegisteredPlayers();
	}

	public void ResetCharacterSelector()
	{
		ResetPlayers();
		_characterSlotsContainerRef.CancelAllSelections(true);
	}

	public void RegisterNewPlayer(int joystickNumber)
	{
		if (_controllerAlreadyInUse[joystickNumber])
			return;

		LocalJoystickBuffer.Add(joystickNumber);
		_controllerAlreadyInUse[joystickNumber] = true;

		if (!NetworkServer.active && !NetworkClient.active)
		{
			Debug.Log("trying to start host");
			StartLocalHost(); //Server side or starting server
		}
		else if(NetworkServer.active)
			ServerManager.Instance.TryToAddPlayer();
	}

	public void CloseCharacterSlot(int slotNumber)
	{
		_characterSlotsContainerRef.CloseTargetSlot(slotNumber);
	}

	public void OpenSlotsForPreselectedPlayers()
	{
		if (!NetworkServer.active)
			return;

		for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
		{
			Debug.Log("Spawning wheel for player => " + ServerManager.Instance.RegisteredPlayers[i].name);
			ServerManager.Instance.SpawnCharacterWheel(ServerManager.Instance.RegisteredPlayers[i].gameObject);
			ServerManager.Instance.RegisteredPlayers[i].UnReady();

			if (ServerManager.Instance.RegisteredPlayers[i].isLocalPlayer)
				_controllerAlreadyInUse[ServerManager.Instance.RegisteredPlayers[i].JoystickNumber] = true;

			GetComponentInChildren<TextIP>().ReGenerate();
		}
		ServerManager.Instance.ForceUnready = false;
	}


	public void StartGame()
	{
		Debug.Log("game started");
		GameManager.Instance.StartGame();
		InputManager.SetInputLockTime(2);
		// SpawnFallingGround.instance.Init();
		//DeactivateMenu();
	}

	public void StartFtue()
	{
		MainManager.Instance.StartCoroutine(StartTutorial());
	}

	private IEnumerator StartTutorial()
	{
		yield return StartCoroutine(AutoFade.StartFade(1,
			LevelManager.Instance.LoadSceneAlone(LevelManager.Instance.SceneTutorial),
			AutoFade.EndFade(0.5f, 1, Color.white),
			Color.white));
	}

	public void Exit()
	{
		Application.Quit();
	}

	public void FadeSplashscreens(bool shouldShowSplashscreens)
	{
		StartCoroutine(FadeSplashscreens_Implementation(shouldShowSplashscreens));
	}

	IEnumerator FadeSplashscreens_Implementation(bool shouldShowSplashscreens)
	{
		//SetActiveButtons(_activeMenu, false);
		Color color;
		int i = 0;

		for (i = 0; i < _splashscreens.Length; ++i)
		{
			color = _splashscreens[i].color;
			color.a = 0;
			_splashscreens[i].color = color;
		}

		if (shouldShowSplashscreens)
		{
			for (i = 0; i < _splashscreens.Length; ++i)
			{
				color = _splashscreens[i].color;
				color.a = 1;
				_splashscreens[i].color = color;
				yield return new WaitForSeconds(0.5f);
				_splashscreens[i].CrossFadeAlpha(0, 0.5f, false);
				yield return new WaitForSeconds(0.5f);
			}
		}

		yield return StartCoroutine(LoadPreview_Implementation(EArenaConfiguration.Aerial));
		SplashScreens.transform.Find("Background_black").GetComponent<Image>().CrossFadeAlpha(0, 1, false);
		Destroy(SplashScreens, 1);
	}

	public void LoadPreview(EArenaConfiguration levelName)
	{
		if (_miniLoadingCoroutine != null)
		{
			StopCoroutine(_miniLoadingCoroutine);
		}
		_miniLoadingCoroutine = StartCoroutine(LoadPreview_Implementation(levelName));
	}

	IEnumerator LoadPreview_Implementation(EArenaConfiguration levelName)
	{
		MiniLoading.SetActive(true);
		Image loadingInImage = LoadingIn.GetComponent<Image>();
		Image loadingOutImage = LoadingOut.GetComponent<Image>();

		Color color = Color.white;
		color.a = 1.0f;
		loadingOutImage.color = color;
		loadingInImage.color = color;

		loadingOutImage.fillAmount = 0.0f;
		yield return StartCoroutine(LevelManager.Instance.LoadLevelPreview(levelName, (AsyncOperation async) =>
		{
			loadingOutImage.fillAmount = async.progress;
		}));
		loadingOutImage.fillAmount = 1.0f;

		float timer = 1.0f;
		while (timer > 0)
		{
			timer -= Time.deltaTime;
			color.a = Mathf.Lerp(1.0f, 0.0f, 1.0f - timer);
			loadingOutImage.color = color;
			loadingInImage.color = color;
			yield return null;
		}

		MiniLoading.SetActive(false);

		color.a = 1.0f;
		loadingOutImage.color = color;
		loadingInImage.color = color;
	}

	public void SetMode(string modeName)
	{
		GameManager.Instance.CurrentGameConfiguration.ModeConfiguration = (EModeConfiguration)Enum.Parse(typeof(EModeConfiguration), modeName);
	}

	public void SkipFTUE()
	{
		PlayerPrefs.SetInt("FTUEDone", 1);
		PlayerPrefs.Save();
	}

	public void StartLocalHost()
	{
		ServerManager.Instance.StartHostAll("AbyssalFall-"+ServerManager.Instance.GameId, 4, true);
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		Debug.Log("Could not connect to server: " + error);
	}

	public void DisconnectFromServer()
	{
		ServerManager.Instance.ResetNetwork();
	}

	public void LogMessage(string message)
	{
		MessageManager.Log(message);
	}
}
