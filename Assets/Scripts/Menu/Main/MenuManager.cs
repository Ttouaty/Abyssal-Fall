using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class MenuManager : GenericSingleton<MenuManager>
{
	[HideInInspector]
	public Canvas _canvas;
    public GameObject Loading;
    public Image LoadingOut;

    [SerializeField]
	private GameObject _isartLogo;
    [SerializeField]
	private LoadingBar _loadBar;

	[SerializeField]
	private Button _StartButton;

	private MenuPanel _activeMenu;
	private MenuPanel[] _menuArray;

	private Transform _metaContainer;

	private Transform _leftMenuAnchor;
	private Transform _centerMenuAnchor;
	private Transform _rightMenuAnchor;

	private ReturnButton _returnGroupRef;
	private bool _isListeningForInput = false;
	private bool[] _controllerAlreadyInUse = new bool[12];
	private CharacterSlotsContainer _characterSlotsContainerRef;

	void Awake()
	{
		_canvas                         = GetComponentInChildren<Canvas>();
		_canvas.worldCamera             = Camera.main;
        _menuArray                      = GetComponentsInChildren<MenuPanel>();
        _loadBar                        = GetComponentInChildren<LoadingBar>();
        _characterSlotsContainerRef     = GetComponentInChildren<CharacterSlotsContainer>();
		_returnGroupRef					= GetComponentInChildren<ReturnButton>();
		_metaContainer					= _canvas.transform.FindChild("Meta");

		_leftMenuAnchor		= _metaContainer.Find("LeftMenuAnchor");
		_centerMenuAnchor	= _metaContainer.Find("CenterMenuAnchor");
		_rightMenuAnchor	= _metaContainer.Find("RightMenuAnchor");
	}

	void Start()
	{
        Loading.SetActive(false);

		_menuArray = GetComponentsInChildren<MenuPanel>();
		_loadBar = GetComponentInChildren<LoadingBar>();

		_activeMenu = _menuArray[0];

		StartCoroutine(FadeIsartLogo());

		for (int i = 0; i < _menuArray.Length; ++i)
		{
			if (i == 0)
				continue;

			_menuArray[i].transform.position = new Vector3(-Screen.width * 1.5f, _menuArray[i].transform.position.y, 0);
		}
	}

	private float timeCancelHeld = 0;
	private float timeCancelActivated = 0.5f;
	void Update()
	{
		//Only display return arrow if there is a menu to return to.
		_returnGroupRef.gameObject.SetActive(_activeMenu.ParentMenu != null);

		if (_isListeningForInput)
		{
			for (int i = -1; i < Input.GetJoystickNames().Length; i++)
			{
				if (i != -1)
                {
                    if (Input.GetJoystickNames()[i] == "")//ignores unplugged controllers but tests for keyboard
                        continue;
                }

                if (InputManager.GetButtonDown(InputEnum.A, i + 1) && !_controllerAlreadyInUse[i + 1]) //if new controller presses start
				{
                    Debug.Log("JOYSTICK NUMBER: " + (i + 1) + " PRESSED START");
					RegisterNewPlayer(i + 1);
				}
			}
		}
    }

    IEnumerator LoadPreview (EArenaConfiguration levelName)
    {
        Loading.SetActive(true);
        LoadingOut.GetComponent<Image>().fillAmount = 0;
        yield return StartCoroutine(LevelManager.Instance.LoadLevelPreview(levelName, (AsyncOperation async) =>
        {
            LoadingOut.GetComponent<Image>().fillAmount = async.progress;
        }));
        Loading.SetActive(false);
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
		GameManager.StartGame();
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

	public void MenuReturn()
	{
		if (_activeMenu.ParentMenu != null)
			MakeTransition(_activeMenu.ParentMenu, false);
	}

	public void MakeTransition(string newMenu)
	{
		MakeTransition(GetMenuPanel(newMenu), true);
	}

	public void MakeTransition(MenuPanel newMenu, bool forward = true)
	{
		timeCancelHeld = 0;
		_isListeningForInput = false;
		SetActiveButtons(_activeMenu, false);
		StopAllCoroutines();
		if (forward)
			StartCoroutine(SendOutLeft(_activeMenu));
		else
			StartCoroutine(SendOutRight(_activeMenu));

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

		if (newMenu == null)
		{
			_activeMenu = null;
			return;
		}

		SetActiveButtons(newMenu, true);
		if (forward)
			StartCoroutine(SendInRight(newMenu));
		else
			StartCoroutine(SendInLeft(newMenu));

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

	IEnumerator SendOutLeft(MenuPanel targetMenu)
	{
		yield return StartCoroutine(MovePanelOverTime(targetMenu, _centerMenuAnchor.position, _leftMenuAnchor.position));
	}

	IEnumerator SendOutRight(MenuPanel targetMenu)
	{
		yield return StartCoroutine(MovePanelOverTime(targetMenu, _centerMenuAnchor.position, _rightMenuAnchor.position));
	}

	IEnumerator SendInRight(MenuPanel targetMenu)
	{
		yield return StartCoroutine(MovePanelOverTime(targetMenu, _rightMenuAnchor.position, _centerMenuAnchor.position));
	}

	IEnumerator SendInLeft(MenuPanel targetMenu)
	{
		yield return StartCoroutine(MovePanelOverTime(targetMenu, _leftMenuAnchor.position, _centerMenuAnchor.position));
	}

	IEnumerator MovePanelOverTime(MenuPanel targetMenu, Vector3 start, Vector3 end)
	{
		float eT = 0;
		float timeTaken = 0.7f;

		targetMenu.transform.position = start;
		while (eT < timeTaken)
		{
			eT += Time.deltaTime;
			targetMenu.transform.position = Vector3.Lerp(targetMenu.transform.position, end, eT / timeTaken);
			yield return null;
		}
	}

	IEnumerator FadeIsartLogo()
	{
		SetActiveButtons(GetMenuPanel("Main"), false);

        _loadBar.SetPercent(0);
        yield return StartCoroutine(LevelManager.Instance.LoadLevelPreview(EArenaConfiguration.Aerial, (AsyncOperation async) =>
        {
            _loadBar.SetPercent(async.progress);
        }));

        OnLoadEnd(1.0f);

        _isartLogo.GetComponent<RawImage>().CrossFadeAlpha(0, 1, false);
		SetActiveButtons(GetMenuPanel("Main"), true);
		Destroy(_isartLogo, 1);
	}

	void OnLoadEnd(float progress)
	{
		_loadBar.SetPercent(progress);
		StartCoroutine(MoveObjectOverTime(_loadBar.gameObject, Vector3.down * Screen.height, 0.5f));
		Destroy(_loadBar.gameObject, 2);
	}

	void OnLoadProgress(float progress)
	{
		_loadBar.SetPercent(progress);
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
			Instance.StartCoroutine(Instance.SendOutLeft(Instance._activeMenu));
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
			StartCoroutine(SendInRight(GetMenuPanel(activeMenu)));
	}
}
