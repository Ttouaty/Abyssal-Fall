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
	[Space]
	public float InputDelayBetweenTransition = 0.5f;
	public FmodOneShotSound ReturnSound;

	private MenuPanel _activeMenu;
	private MenuPanel[] _menuArray;

	private Transform _metaContainer;
	private Transform _leftMenuAnchor;
	private Transform _centerMenuAnchor;
	private Transform _rightMenuAnchor;

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
		_menuArray = GetComponentsInChildren<MenuPanel>();
		_splashscreens = SplashScreens.GetComponentsInChildren<RawImage>();
		_characterSlotsContainerRef = GetComponentInChildren<CharacterSelector>();
		_metaContainer = _canvas.transform.FindChild("Meta");

		_needFTUE = !PlayerPrefs.HasKey("FTUEDone");

		_leftMenuAnchor = _metaContainer.Find("LeftMenuAnchor");
		_centerMenuAnchor = _metaContainer.Find("CenterMenuAnchor");
		_rightMenuAnchor = _metaContainer.Find("RightMenuAnchor");
	}

	void Start()
	{
		_canvas.worldCamera = Camera.main;
		MiniLoading.SetActive(false);
		ServerManager.ResetRegisteredPlayers();
		_menuArray = GetComponentsInChildren<MenuPanel>();

		_activeMenu = _menuArray[0];
		_activeMenu.PreSelectedButton.Select();

		for (int i = 0; i < _menuArray.Length; ++i)
		{
			if (i == 0)
			{
				_menuArray[i].transform.position = _centerMenuAnchor.position;
				_menuArray[i].gameObject.SetActive(true);
				continue;
			}

			_menuArray[i].transform.position = _leftMenuAnchor.position;
			_menuArray[i].gameObject.SetActive(false);
		}

		if (_needFTUE)
		{
			_menuArray[0].transform.position = _leftMenuAnchor.position;
			_menuArray[0].gameObject.SetActive(false);
			GetMenuPanel("FTUE").transform.position = _centerMenuAnchor.position;
			GetMenuPanel("FTUE").gameObject.SetActive(true);
			_activeMenu = GetMenuPanel("FTUE");
		}

		CameraManager.OnCameraChange.AddListener(OnCameraChange);
	}

	public void OnCameraChange()
	{
		_canvas.worldCamera = Camera.main;
	}

	void Update()
	{

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			LoadPreview(EArenaConfiguration.Aerial);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			LoadPreview(EArenaConfiguration.Hell);
		}
	}
	
	private void ResetPlayers()
	{
		//Trashy way but fuck it.
		_controllerAlreadyInUse = new bool[12];
		LocalJoystickBuffer.Clear();
		ServerManager.ResetRegisteredPlayers();
	}

	public void RegisterNewPlayer(JoystickNumber joystickNumber)
	{
		if (_controllerAlreadyInUse[joystickNumber] || LocalJoystickBuffer.Count >= 4)
		{
			return;
		}

		LocalJoystickBuffer.Add(joystickNumber);
		_controllerAlreadyInUse[joystickNumber] = true;

		if (!NetworkServer.active && !NetworkClient.active)
		{
			Debug.Log("trying to start host");
			StartLocalHost(); //Server side or starting server
		}
		else if(Player.LocalPlayer == null)
		{
			ServerManager.Instance.TryToAddPlayer();
		}
	}

	public void OpenCharacterSlot(OpenSlots slotNumbers, Player player)
	{
		int i = -1;
		foreach (OpenSlots slot in Enum.GetValues(typeof(OpenSlots)))
		{
			if ((slotNumbers & slot) != 0 && i != -1)
			{
				_characterSlotsContainerRef.OpenTargetSlot(i, player);
			}
			i++;
		}
	}

	

	public void CloseCharacterSlot(int slotNumber)
	{
		_characterSlotsContainerRef.CloseTargetSlot(slotNumber);
	}

	public void StartGame()
	{
		Debug.Log("game started");
		GameManager.Instance.StartGame();
		// SpawnFallingGround.instance.Init();
		//DeactivateMenu();
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

	public void MenuReturn()
	{
		if (_activeMenu.ParentMenu != null)
		{
			ReturnSound.Play();
			MakeTransition(_activeMenu.ParentMenu, false);
		}
	}

	public void MakeTransition(string newMenu)
	{
		MakeTransition(GetMenuPanel(newMenu), true);
	}

	public void MakeTransition(MenuPanel newMenu, bool forward = true)
	{
		StartCoroutine(Transition(newMenu, forward));
	}

	//private void SetActiveButtons(MenuPanel target, bool active)
	//{
	//	Button[] buttons = target.GetComponentsInChildren<Button>();
	//	for (int i = 0; i < buttons.Length; ++i)
	//	{
	//		buttons[i].interactable = active;
	//	}
	//}

	public void StartFtue()
	{
		DeactivateMenu();
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

	IEnumerator Transition(MenuPanel newMenu, bool forward = true)
	{
		//yield return null; // delay by one frame to avoid input error.
		ServerManager.Instance.IsInLobby = false;

		if (_activeMenu != null)
		{
			//SetActiveButtons(_activeMenu, false);
			//StopAllCoroutines();
			if (forward)
			{
				StartCoroutine(SendOutLeft(_activeMenu));
				StartCoroutine(SendInRight(newMenu));
			}
			else
			{
				StartCoroutine(SendOutRight(_activeMenu));
				StartCoroutine(SendInLeft(newMenu));
			}
		}

		if (newMenu == null)
		{
			StartCoroutine(SendOutLeft(_activeMenu));
			_activeMenu = null;
			yield break;
		}

		if (newMenu.MenuName == "GameConfig")
			MessageManager.Log("! WIP ! ya rien qui marche ici, laisse tomber.");
		else if (newMenu.MenuName == "Main")
		{
			ResetPlayers();
			_characterSlotsContainerRef.CancelAllSelections(true);
		}
		else if(newMenu.MenuName == "Lobby")
		{
			ServerManager.Instance.IsInLobby = true;
			//if(!NetworkClient.active)
			//{
			//	RegisterNewPlayer(new JoystickNumber(InputManager.AnyButtonDown(true) == -1 ? 0 : InputManager.AnyButtonDown(true)));
			//}
		}

		//SetActiveButtons(newMenu, true); 


		_activeMenu = newMenu;

		if (_activeMenu.LastElementSelected == null)
		{
			if (_activeMenu.PreSelectedButton != null)
			{
				_activeMenu.PreSelectedButton.Select();
				_activeMenu.LastElementSelected = _activeMenu.PreSelectedButton.GetComponent<LastSelectedComponent>();
			}
		}
		else
		{
			_activeMenu.SelectLastButton();
		}

		InputManager.SetInputLockTime(InputDelayBetweenTransition);

		yield return new WaitForSeconds(InputDelayBetweenTransition);
	}

	IEnumerator SendOutLeft(MenuPanel targetMenu)
	{
		yield return StartCoroutine(MovePanelOverTime(targetMenu, _centerMenuAnchor.localPosition, _leftMenuAnchor.localPosition));
		targetMenu.gameObject.SetActive(false);
	}

	IEnumerator SendOutRight(MenuPanel targetMenu)
	{
		yield return StartCoroutine(MovePanelOverTime(targetMenu, _centerMenuAnchor.localPosition, _rightMenuAnchor.localPosition));
		targetMenu.gameObject.SetActive(false);
	}

	IEnumerator SendInRight(MenuPanel targetMenu)
	{
		if (targetMenu != null)
		{
			targetMenu.gameObject.SetActive(true);
			yield return StartCoroutine(MovePanelOverTime(targetMenu, _rightMenuAnchor.localPosition, _centerMenuAnchor.localPosition));
		}
	}

	IEnumerator SendInLeft(MenuPanel targetMenu)
	{
		if (targetMenu != null)
		{
			targetMenu.gameObject.SetActive(true);
			yield return StartCoroutine(MovePanelOverTime(targetMenu, _leftMenuAnchor.localPosition, _centerMenuAnchor.localPosition));
		}
	}

	IEnumerator MovePanelOverTime(MenuPanel targetMenu, Vector3 start, Vector3 end)
	{
		float eT = 0;
		float timeTaken = 0.2f;

		targetMenu.transform.localPosition = start;
		while (eT < timeTaken)
		{
			eT += Time.deltaTime;
			targetMenu.transform.localPosition = Vector3.Lerp(targetMenu.transform.localPosition, end, eT / timeTaken);
			yield return null;
		}
		targetMenu.transform.localPosition = end;
	}

	public void FadeSplashscreens(bool shouldShowSplashscreens)
	{
		StartCoroutine(FadeSplashscreens_Implementation(shouldShowSplashscreens));
	}

	IEnumerator FadeSplashscreens_Implementation(bool shouldShowSplashscreens)
	{
		//SetActiveButtons(_activeMenu, false);
		_activeMenu.gameObject.SetActive(false);
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
				yield return new WaitForSeconds(1);
				_splashscreens[i].CrossFadeAlpha(0, 1, false);
				yield return new WaitForSeconds(1);
			}
		}

		yield return StartCoroutine(LoadPreview_Implementation(EArenaConfiguration.Aerial));
		_activeMenu.gameObject.SetActive(true);
		SplashScreens.transform.Find("Background_black").GetComponent<Image>().CrossFadeAlpha(0, 1, false);
		Destroy(SplashScreens, 1);

		yield return StartCoroutine(WaitForFixedCamera());

		//SetActiveButtons(_activeMenu, true);
		if (_activeMenu.PreSelectedButton != null)
		{
			_activeMenu.LastElementSelected = _activeMenu.PreSelectedButton.GetComponent<LastSelectedComponent>();
			_activeMenu.PreSelectedButton.Select();
		}
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

	public static void DeactivateMenu(bool instant = false)
	{
		//Instance.SetActiveButtons(Instance._activeMenu, false);
		if (instant)
		{
			Instance.StartCoroutine(Instance.SendOutLeft(Instance._activeMenu));
			Instance.gameObject.SetActive(false);
		}
		else
			Instance.StartCoroutine(Instance.DeactivateMenuCoroutine());
	}

	private IEnumerator DeactivateMenuCoroutine()
	{
		MakeTransition((MenuPanel)null);
		yield return new WaitForSeconds(1);
		gameObject.SetActive(false);
	}

	private IEnumerator WaitForFixedCamera()
	{
		while (CameraManager.Instance.IsMoving)
		{
			yield return null;
		}
	}

	public void SetMode(string modeName)
	{
		GameManager.Instance.CurrentGameConfiguration.ModeConfiguration = (EModeConfiguration)Enum.Parse(typeof(EModeConfiguration), modeName);
	}

	public void SkipFTUE()
	{
		PlayerPrefs.SetInt("FTUEDone", 1);
		PlayerPrefs.Save();
		MakeTransition("Main");
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
		MasterServer.UnregisterHost();
		ServerManager.singleton.StopHost();
		ServerManager.singleton.StopClient();
	}

	public void LogMessage(string message)
	{
		MessageManager.Log(message);
	}
}
