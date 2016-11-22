﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.Networking;

[Serializable]
public enum OpenCharacterSlot
{
	None, First, Second, Third, Fourth
}

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
	private CharacterSelector _characterSlotsContainerRef;
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
		ServerManager.ResetRegisteredPlayers();
	}

	public void RegisterNewPlayer(JoystickNumber joystickNumber)
	{
		if (!ServerManager.Instance.IsOnline)
		{
			if (_controllerAlreadyInUse[joystickNumber] || ServerManager.Instance.RegisteredPlayers.Count >= 4)
				return;
			ServerManager.singleton.StartClient();

			LocalJoystickBuffer.Add(joystickNumber);
			_controllerAlreadyInUse[joystickNumber] = true;
			_characterSlotsContainerRef.OpenNextSlot(joystickNumber);
		}
		else
			Debug.Log("need to start client with correct IP and stuff");
	}

	void CmdRegisterNewPlayer(JoystickNumber joystickNumber)
	{

		Debug.Log("CMD recieved, need to open new characterslot");

		//
		//_controllerAlreadyInUse[joystickNumber] = true; // J'aurai pu utiliser un enum, mais je me rappellais plus comment faire dans le metro lAUl.

		//Player newPlayer = new Player();
		//ServerManager.Instance.RegisteredPlayers[newPlayer.PlayerNumber] = newPlayer;
		//newPlayer.JoystickNumber = joystickNumber;
		//_characterSlotsContainerRef.OpenNextSlot(joystickNumber);
	}
	//[ClientRpc]
	//void RpcOpenCharacterSlots(OpenCharacterSlot slotsOpen)
	//{
	//	if (slotsOpen == OpenCharacterSlot.None)
	//		return;


	//	Debug.Log(slotsOpen.ToString());

	//	if ((slotsOpen & OpenCharacterSlot.First) != 0)
	//		Debug.Log("1 is open");
	//	if ((slotsOpen & OpenCharacterSlot.Second) != 0)
	//		Debug.Log("2 is open");
	//	if ((slotsOpen & OpenCharacterSlot.Third) != 0)
	//		Debug.Log("3 is open");
	//	if ((slotsOpen & OpenCharacterSlot.Fourth) != 0)
	//		Debug.Log("4 is open");

	//}

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

	private void SetActiveButtons(MenuPanel target, bool active)
	{
		Button[] buttons = target.GetComponentsInChildren<Button>();
		for (int i = 0; i < buttons.Length; ++i)
		{
			buttons[i].interactable = active;
		}
	}

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

		if (_activeMenu != null)
		{
			SetActiveButtons(_activeMenu, false);
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

		SetActiveButtons(newMenu, true); // Attention peux poser des problemes si on veux avoir des boutons verrouillés


		_activeMenu = newMenu;
		InputManager.SetInputLockTime(InputDelayBetweenTransition);

		yield return new WaitForSeconds(InputDelayBetweenTransition);

		if (_activeMenu.LastButtonSelected == null)
		{
			if (_activeMenu.PreSelectedButton != null)
				_activeMenu.PreSelectedButton.Select();
		}
		else
		{
			_activeMenu.LastButtonSelected.Select();
		}
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
		float timeTaken = 0.3f;

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
		SetActiveButtons(_activeMenu, false);
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

		SetActiveButtons(_activeMenu, true);
		if (_activeMenu.PreSelectedButton != null)
			_activeMenu.PreSelectedButton.Select();
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
		if (instant)
		{
			Instance.StartCoroutine(Instance.SendOutLeft(Instance._activeMenu));
			Instance.gameObject.SetActive(false);
		}
		else
			Instance.StartCoroutine(Instance.DeactivateMenuCoroutine());
		Instance.SetActiveButtons(Instance._activeMenu, false);
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

		Debug.Log("asshole");
		ServerManager.singleton.OnStartHost();
	}

	public void SetGameIsOnline(bool online)
	{
		ServerManager.Instance.IsOnline = online;
	}
}
