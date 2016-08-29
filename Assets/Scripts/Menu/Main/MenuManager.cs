using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class MenuManager : GenericSingleton<MenuManager>
{
	[HideInInspector]
	public Canvas _canvas;
	public GameObject MiniLoading;
	public Image LoadingIn;
	public Image LoadingOut;
	public GameObject SplashScreens;

	[SerializeField]
	private GameObject _isartLogo;
	[SerializeField]
	private LoadingBar _loadBar;

	[SerializeField]
	private Button _StartButton;

	private MenuPanel _activeMenu;

	//public string[] debugNames;
	private MenuPanel[] _menuArray;

	private bool _isListeningForInput                               = false;
	private bool[] _controllerAlreadyInUse                          = new bool[12];
	private float timeCancelActivated                               = 0.5f;
	private float timeCancelHeld                                    = 0;
	private CharacterSlotsContainer _characterSlotsContainerRef;
	private RawImage[] _splashscreens;
	private Coroutine _miniLoadingCoroutine;

	void Awake()
	{
		_canvas                         = GetComponentInChildren<Canvas>();
		_canvas.worldCamera             = Camera.main;
		_menuArray                      = GetComponentsInChildren<MenuPanel>();
		_loadBar                        = GetComponentInChildren<LoadingBar>();
		_characterSlotsContainerRef     = GetComponentInChildren<CharacterSlotsContainer>();
		_splashscreens                  = SplashScreens.GetComponentsInChildren<RawImage>();
	}

	void Start()
	{
		MiniLoading.SetActive(false);
		_activeMenu = _menuArray[0];

		for (int i = 0; i < _menuArray.Length; ++i)
		{
			if (i == 0)
				continue;

			_menuArray[i].transform.position = new Vector3(-Screen.width * 1.5f, _menuArray[i].transform.position.y, 0);
		}
	}

	void Update()
	{
		timeCancelHeld = Mathf.Clamp(timeCancelHeld + (Input.GetButton("Cancel") ? Time.deltaTime : -Time.deltaTime), 0, timeCancelActivated);
		if (timeCancelHeld == timeCancelActivated && !GameManager.InProgress)
		{
			timeCancelHeld = 0;
			if (_activeMenu.ParentMenu != null)
			{
				MakeTransition(_activeMenu.ParentMenu);
				if (_activeMenu.MenuName == "Main")
					_activeMenu.ParentMenu = null;
			}
		}
		if (_isListeningForInput)
		{
			for (int i = -1; i < Input.GetJoystickNames().Length; i++)
			{
				if (i != -1)
				{
					if (Input.GetJoystickNames()[i] == "")//ignores unplugged controllers but tests for keyboard
						continue;
				}

				if (InputManager.GetButtonDown("Start", i + 1) && !_controllerAlreadyInUse[i + 1]) //if new controller presses start
				{
					Debug.Log("JOYSTICK NUMBER: " + (i + 1) + " PRESSED START");
					RegisterNewPlayer(i + 1);
				}
			}
			_StartButton.interactable = GameManager.Instance.nbPlayers >= 2 && !GameManager.InProgress && AllPlayersReady();
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			LoadPreview(EArenaConfiguration.Aerial);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			LoadPreview(EArenaConfiguration.Hell);
		}
	}

	bool AllPlayersReady()
	{
		for (int i = 0; i < GameManager.Instance.RegisteredPlayers.Length; ++i)
		{
			if (GameManager.Instance.RegisteredPlayers[i] != null)
			{
				if (!GameManager.Instance.RegisteredPlayers[i].isReady)
					return false;
			}
		}
		return true;
	}

	private void ResetPlayers()
	{
		//Trashy way but fuck it.
		_controllerAlreadyInUse = new bool[12];
		GameManager.ResetRegisteredPlayers();
	}

	private void RegisterNewPlayer(int joystickNumber)
	{
		_controllerAlreadyInUse[joystickNumber] = true; // J'aurai pu utiliser un enum, mais je me rappellais plus comment faire dans le metro lAUl.

		Player newPlayer = new Player();
		GameManager.Instance.RegisteredPlayers[newPlayer.PlayerNumber] = newPlayer;
		newPlayer.JoystickNumber = joystickNumber;
		_characterSlotsContainerRef.OpenNextSlot(joystickNumber);
	}

	public void StartGame()
	{
		Debug.Log("game started");
		GameManager.Instance.StartGame();
		// SpawnFallingGround.instance.Init();
		DeactivateMenu();
	}

	private MenuPanel GetMenuPanel(string panelName)
	{
		for (int i = 0; i < _menuArray.Length; ++i)
		{
			if (panelName == _menuArray[i].MenuName)
				return _menuArray[i];
		}

		Debug.Log("no panel found for name: " + panelName);
		return null;
	}

	public void MakeTransition(string newMenu)
	{
		MakeTransition(GetMenuPanel(newMenu));
	}

	public void MakeTransition(MenuPanel newMenu)
	{
		timeCancelHeld = 0;
		_isListeningForInput = false;
		SetActiveButtons(_activeMenu, false);
		StopAllCoroutines();
		StartCoroutine(SendOut(_activeMenu));

		if (newMenu == null)
		{
			_activeMenu = null;
			return;
		}

		if (newMenu.MenuName == "Lobby")
		{
			_isListeningForInput = true;
		}
		else
		{
			ResetPlayers();
			_characterSlotsContainerRef.CancelAllSelections(true);
		}

		newMenu.ParentMenu = _activeMenu;
		SetActiveButtons(newMenu, true);
		StartCoroutine(SendIn(newMenu));
		newMenu.PreSelectedButton.Select();
		_activeMenu = newMenu;
	}

	private void SetActiveButtons(MenuPanel target, bool active)
	{
		Button[] buttons = target.GetComponentsInChildren<Button>();
		//if (active)
		//	buttons[0].Select();
		for (int i = 0; i < buttons.Length; ++i)
		{
			buttons[i].interactable = active;
		}
	}

	public void Exit()
	{
		Application.Quit();
	}

	IEnumerator SendOut(MenuPanel targetMenu)
	{
		float eT = 0;
		float timeTaken = 0.7f;

		while (eT < timeTaken)
		{
			eT += Time.deltaTime;
			targetMenu.transform.position = Vector3.Lerp(targetMenu.transform.position, _canvas.transform.position - _canvas.transform.right * _canvas.pixelRect.width / _canvas.referencePixelsPerUnit * 5, eT / timeTaken);
			yield return null;
		}
	}

	IEnumerator SendIn(MenuPanel targetMenu)
	{
		float eT = 0;
		float timeTaken = 0.7f;

		while (eT < timeTaken)
		{
			eT += Time.deltaTime;
			targetMenu.transform.position = Vector3.Lerp(targetMenu.transform.position, _canvas.transform.position, eT / timeTaken);
			yield return null;
		}
	}

	public void FadeSplashscreens (bool shouldShowSplashscreens)
	{
		StartCoroutine(FadeSplashscreens_Implementation(shouldShowSplashscreens));
	}

	IEnumerator FadeSplashscreens_Implementation(bool shouldShowSplashscreens)
	{
		SetActiveButtons(GetMenuPanel("Main"), false);
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
				yield return new WaitForSeconds(2);
				_splashscreens[i].CrossFadeAlpha(0, 2, false);
				yield return new WaitForSeconds(2);
			}
		}
		yield return StartCoroutine(LoadPreview_Implementation(EArenaConfiguration.Aerial));
		Destroy(SplashScreens, 0);
		SetActiveButtons(GetMenuPanel("Main"), true);
	}

	public void LoadPreview(EArenaConfiguration levelName)
	{
		if(_miniLoadingCoroutine != null)
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

		Color color             = Color.white;
		color.a                 = 1.0f;
		loadingOutImage.color   = color;
		loadingInImage.color    = color;

		loadingOutImage.fillAmount = 0.0f;
		yield return StartCoroutine(LevelManager.Instance.LoadLevelPreview(levelName, (AsyncOperation async) =>
		{
			loadingOutImage.fillAmount = async.progress;
		}));
		loadingOutImage.fillAmount = 1.0f;

		float timer = 1.0f;
		while(timer > 0)
		{
			timer                  -= Time.deltaTime;
			color.a                 = Mathf.Lerp(1.0f, 0.0f, 1.0f - timer);
			loadingOutImage.color   = color;
			loadingInImage.color    = color;
			yield return null;
		}

		MiniLoading.SetActive(false);

		color.a                 = 1.0f;
		loadingOutImage.color   = color;
		loadingInImage.color    = color;
	}

	IEnumerator MoveObjectOverTime(GameObject go, Vector3 offset, float time)
	{
		float eT = 0;
		Vector3 endPos = go.transform.position + offset;
		while (eT < time)
		{
			eT += Time.deltaTime;
			go.transform.position = Vector3.Lerp(go.transform.position, endPos, eT);
			yield return null;
		}
	}

	public static void DeactivateMenu(bool instant = false)
	{
		if (instant)
		{
			Instance.StartCoroutine(Instance.SendOut(Instance._activeMenu));
			Instance.gameObject.SetActive(false);
		}
		else
			Instance.StartCoroutine(Instance.DeactivateMenuCoroutine());
		
	}

	private IEnumerator DeactivateMenuCoroutine()
	{
		MakeTransition((MenuPanel) null);
		yield return new WaitForSeconds(1);
		gameObject.SetActive(false);
	}

	public static void ActivateMenu(bool instant = false, string activeMenu = "Main")
	{
		Instance.ReactivateMenu(instant, activeMenu);
	}

	private void ReactivateMenu(bool instant = false, string activeMenu = "Main")
	{
		gameObject.SetActive(true);
		if (instant)
		{
			MakeTransition(GetMenuPanel(activeMenu));
			_activeMenu.transform.position = _canvas.transform.position;
		}
		else
			StartCoroutine(SendIn(GetMenuPanel(activeMenu)));
	}
}
