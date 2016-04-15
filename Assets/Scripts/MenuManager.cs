using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	[SerializeField]
	private GameObject _canvas;
	[SerializeField]
	private GameObject _isartLogo;
	[SerializeField]
	private GameObject _mainButtons;
	[SerializeField]
	private GameObject _lobbyButtons;
	[SerializeField]
	private GameObject _credits;
	[SerializeField]
	private Image _loadBarContainer;
	[SerializeField]
	private Image _loadBarProgress;

	[SerializeField]
	private Button _StartButton;


	private GameObject _activeMenu;

	private bool _isListeningForInput = false;

	void Start()
	{
		_activeMenu = _mainButtons;
		StartCoroutine(FadeIsartLogo());
		_lobbyButtons.transform.position = new Vector3(-Screen.width * 1.5f, _lobbyButtons.transform.position.y, 0);
		_credits.transform.position = new Vector3(-Screen.width * 1.5f, _credits.transform.position.y, 0);
	}

	void Update()
	{
		if (Input.GetButton("Cancel") && !GameManager.InProgress)
		{
			if (_activeMenu.GetInstanceID() != _mainButtons.GetInstanceID())
			{
				MakeTransition(_mainButtons);
			}
		}

		if (_isListeningForInput)
		{
			for (int i = 0; i < Input.GetJoystickNames().Length; i++)
			{
				if (i > 3)
					break;
				GameManager.instance.RegisteredPlayers[i] = Input.GetJoystickNames()[i] != null ? 1 : 0;
			}

			_StartButton.interactable = Input.GetJoystickNames().Length >= 2;
		}
	}

	public void StartGame()
	{
		Debug.Log("game started");
		GameManager.StartGame();
		// SpawnFallingGround.instance.Init();
		SetActiveButtons(_activeMenu, false);
		StartCoroutine(SendOut(_activeMenu));
	}

	public void LaunchLobby()
	{
		MakeTransition(_lobbyButtons);
		ListenForInput();
		_isListeningForInput = true;
	}

	public void ReturnToMainMenu()
	{
		MakeTransition(_mainButtons);
		GameManager.ResetRegisteredPlayers();
		_isListeningForInput = false;
	}


	private void MakeTransition(GameObject targetMenu)
	{
		SetActiveButtons(_activeMenu, false);
		StartCoroutine(SendOut(_activeMenu));

		SetActiveButtons(targetMenu, true);
		StartCoroutine(SendIn(targetMenu));

		_activeMenu = targetMenu;
	}

	private void SetActiveButtons(GameObject goTarget, bool active)
	{
		Button[] buttons = goTarget.GetComponentsInChildren<Button>();
		if (active)
			buttons[0].Select();
		for (int i = 0; i < buttons.Length; ++i)
		{
			buttons[i].interactable = active;
		}
	}

	public void LaunchCredits()
	{
		MakeTransition(_credits);
	}

	public void Exit()
	{
	}

	void ListenForInput()
	{

	}


	IEnumerator SendOut(GameObject targetMenu)
	{
		float eT = 0;
		float timeTaken = 0.7f;

		while (eT < timeTaken)
		{
			eT += Time.deltaTime;
			targetMenu.transform.position = Vector3.Lerp(targetMenu.transform.position, _canvas.transform.position + Vector3.left * Screen.width * 1.5f, eT / timeTaken);
			yield return null;
		}
	}

	IEnumerator SendIn(GameObject targetMenu)
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

	IEnumerator FadeIsartLogo()
	{
		SetActiveButtons(_mainButtons, false);
		GameObjectPool.instance.LoadStart.AddListener(OnLoadStart);
		GameObjectPool.instance.LoadEnd.AddListener(OnLoadEnd);
		GameObjectPool.instance.LoadProgress.AddListener(OnLoadProgress);
		yield return StartCoroutine(GameObjectPool.instance.Init());
		//yield return new WaitForSeconds(2);
		_isartLogo.GetComponent<RawImage>().CrossFadeAlpha(0, 1, false);
		SetActiveButtons(_mainButtons, true);
		Destroy(_isartLogo, 1);
	}


	private float originalSize;
	void OnLoadStart(float progress)
	{
		originalSize = _loadBarProgress.rectTransform.rect.width;
		_loadBarProgress.rectTransform.sizeDelta = new Vector2(0, _loadBarProgress.rectTransform.rect.height);
	}

	void OnLoadEnd(float progress)
	{

		StartCoroutine(MoveObjectOverTime(_loadBarProgress.gameObject, Vector3.down * Screen.height, 0.5f));
		StartCoroutine(MoveObjectOverTime(_loadBarContainer.gameObject, Vector3.down * Screen.height, 0.5f));
		_loadBarProgress.CrossFadeAlpha(0, 2, false);
		_loadBarContainer.CrossFadeAlpha(0, 2, false);
		Destroy(_loadBarProgress, 2);
		Destroy(_loadBarContainer, 2);
		//_isartLogo.GetComponent<RawImage>().CrossFadeAlpha(0, 1, false);
	}

	void OnLoadProgress(float progress)
	{
		_loadBarProgress.rectTransform.sizeDelta = new Vector2(progress * originalSize, _loadBarProgress.rectTransform.rect.height);
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
}
