using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Networking.Match;

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
	private CanvasGroup[] _splashscreens;
	private Coroutine _miniLoadingCoroutine;
	private bool _needFTUE = false;

	[HideInInspector]
	public List<int> LocalJoystickBuffer = new List<int>();

	public bool NeedFTUE { get { return _needFTUE; } }
	[HideInInspector]
	public bool GameIsPublic = false;	
	public void SetGameIsPublic(bool value) { GameIsPublic = value; }

	protected override void Awake()
	{

		_canvas = GetComponentInChildren<Canvas>();
		_splashscreens = SplashScreens.GetComponentsInChildren<CanvasGroup>();
		_characterSlotsContainerRef = GetComponentInChildren<CharacterSelector>(true);
	
		_needFTUE = !PlayerPrefs.HasKey("FTUEDone");
	}

	void Start()
	{
		_canvas.worldCamera = Camera.main;
		MiniLoading.SetActive(false);

		CameraManager.OnCameraChange.AddListener(OnCameraChange);
	}

	public void OnCameraChange()
	{
		_canvas.worldCamera = Camera.main;
	}

	void Update()
	{
		#if UNITY_EDITOR
		if (Input.GetKey(KeyCode.Keypad1) && Input.GetKey(KeyCode.Keypad2) && Input.GetKeyDown(KeyCode.Keypad3))
			ForceRegisterNewPlayer();
		#endif
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
		else 
			ServerManager.Instance.TryToAddPlayer();
	}

	public void ForceRegisterNewPlayer()
	{
		if (!NetworkServer.active && !NetworkClient.active)
		{
			Debug.Log("trying to start host");
			StartLocalHost(); //Server side or starting server
		}
		else
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
			ServerManager.Instance.RegisteredPlayers[i].UnReady();

			//if (ServerManager.Instance.RegisteredPlayers[i].isLocalPlayer)
			//	_controllerAlreadyInUse[ServerManager.Instance.RegisteredPlayers[i].JoystickNumber] = true;
		}
		StartCoroutine(SpawnWheelOvertime());

		GetComponentInChildren<TextIP>(true).ReGenerate();
		ServerManager.Instance.ForceUnready = false;
	}

	public void SetControllerInUse(int controllerIndex, bool isInUse)
	{
		_controllerAlreadyInUse[controllerIndex] = isInUse;
	}

	IEnumerator SpawnWheelOvertime()
	{
		for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
		{
			ServerManager.Instance.SpawnCharacterWheel(ServerManager.Instance.RegisteredPlayers[i].gameObject);
			yield return new WaitForSeconds(0.1f);
		}
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
			LevelManager.Instance.LoadSceneAlone(LevelManager.Instance.SceneTutorial, true),
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
		InputManager.SetInputLockTime(100000);
		int i = 0;
		float timePerScreenFadeIn = 0.3f;
		float timePerScreenWait = 0.9f;
		float timePerScreenFadeOut = 0.3f;
		float TimeBetweenScreens = 0.5f;

		for (i = 0; i < _splashscreens.Length; ++i)
		{
			_splashscreens[i].CrossFadeAlpha(0, 0);
		}

		if (shouldShowSplashscreens)
		{
			for (i = 0; i < _splashscreens.Length; ++i)
			{
				yield return new WaitForSeconds(TimeBetweenScreens);
				_splashscreens[i].CrossFadeAlpha(1, timePerScreenFadeIn);
				yield return new WaitForSeconds(timePerScreenFadeIn + timePerScreenWait);
				_splashscreens[i].CrossFadeAlpha(0, timePerScreenFadeOut);
				yield return new WaitForSeconds(timePerScreenFadeOut);
			}
		}
		yield return StartCoroutine(LoadPreview_Implementation(LevelManager.Instance.SceneMenuBg));
		SplashScreens.transform.Find("Background_black").GetComponent<Image>().CrossFadeAlpha(0, 1, false);
		Destroy(SplashScreens, 1);
		InputManager.SetInputLockTime(1);
	}

	IEnumerator LoadPreview_Implementation(SceneField levelName)
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
		if (GameIsPublic)
			ServerManager.Instance.StartHostAll(ServerManager.Instance.GameId + "-AbyssalFall-Public", 8, true); //Max players == 8 to try and fix (can't connect to game)
		else
			ServerManager.Instance.StartHostAll(ServerManager.Instance.GameId + "-AbyssalFall-Private", 8, true);

		//##### Security for multiple gameId ###
		//##### safety but slows down game creation so fuck it.
		//if (ServerManager.Instance.matchMaker == null) ServerManager.Instance.matchMaker = ServerManager.Instance.gameObject.AddComponent<NetworkMatch>();

		//MessageManager.Log("Checking if GameId is unique...", 2);

		//if (GameIsPublic)
		//	ServerManager.Instance.matchMaker.ListMatches(0, 1, "-AbyssalFall-Public", true, 0, 0, CheckIfGameIdIsAvailable);
		//else
		//	ServerManager.Instance.matchMaker.ListMatches(0, 1, "-AbyssalFall-Private", true, 0, 0, CheckIfGameIdIsAvailable);
	}

	//void CheckIfGameIdIsAvailable(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
	//{
	//	if (success)
	//	{
	//		if (matchList.Count != 0)
	//		{
	//			ServerManager.Instance.GenerateNewGameId();
	//			if (GameIsPublic)
	//				ServerManager.Instance.matchMaker.ListMatches(0, 1, ServerManager.Instance.GameId + "-AbyssalFall-Public", true, 0, 0, CheckIfGameIdIsAvailable);
	//			else
	//				ServerManager.Instance.matchMaker.ListMatches(0, 1, ServerManager.Instance.GameId + "-AbyssalFall-Private", true, 0, 0, CheckIfGameIdIsAvailable);
	//		}
	//		else
	//		{
	//			MessageManager.Log("GameId is ok", 2);

	//			if (GameIsPublic)
	//				ServerManager.Instance.StartHostAll(ServerManager.Instance.GameId + "-AbyssalFall-Public", 4, true);
	//			else
	//				ServerManager.Instance.StartHostAll(ServerManager.Instance.GameId + "-AbyssalFall-Private", 4, true);
	//		}
	//	}
	//	else
	//		MessageManager.Log("Error Creating game: could not generate unique GameId");
	//}

	public void DisconnectFromServer()
	{
		ServerManager.Instance.ResetNetwork();
	}

	public void LogMessage(string message)
	{
		MessageManager.Log(message);
	}

	public void Shake()
	{
		CameraManager.Shake(ShakeStrength.High, 0.3f);
	}

	public void CheckIfNeedFTUE()
	{
		if (_needFTUE)
			MenuPanelNew.PanelRefs["FTUE"].Open();
		//Debug.LogWarning("Tuto has been disabled");
	}

	public void SetMenuMusicProgression(float progression)
	{
		SoundManager.Instance.GetInstance("Menu Music").setParameterValue("Progression", progression);
	}
}
