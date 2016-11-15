using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

struct SelectedCharacters
{
	public int characterIndex;
	public int skinIndex;
}

public class CharacterSlot : MonoBehaviour
{
	private static SelectableCharacter[] _availableCharacters;
	public static ParticleSystem OnCharacterSelectedParticles;
	

	private float _switchCharacterDelay = 0.15f;
	private TimeCooldown _switchCharacterCooldown;
	[HideInInspector]
	public int _selectedCharacterIndex = 0;
	[HideInInspector]
	public int _selectedSkinIndex = 0;

	private int _joyToListen; 
	private int _playerIndex;
	[HideInInspector]
	public bool Open = false;
	[HideInInspector]
	public bool Selected = false;

	public UnityEvent OnSlotOpen;
	public UnityEvent OnSlotClose;

	private bool _canSwitchCharacter = true;

	private CharacterSelectWheel _wheelRef;
	//private GameObject _selectedCharacterModel;

	//private Coroutine _activeCoroutineRef;

	public PlayerController GetSelectedCharacter
	{
		get
		{
			return _wheelRef.GetSelectedElement();
		}
	}
	
	public Material GetSelectedSkin
	{
		get
		{
			return _wheelRef.GetSelectedSkin();
		}
	}

	private CharacterSelector _selectorRef;

	void Start()
	{
		_selectorRef = GetComponentInParent<CharacterSelector>();
		if (_availableCharacters == null) //setup static vars (messy)
		{
			OnCharacterSelectedParticles = _selectorRef.OnCharacterSelectedParticles;
			_availableCharacters = _selectorRef._availableCharacters;
		}


		_wheelRef = new GameObject().AddComponent<CharacterSelectWheel>();
		_wheelRef.transform.SetParent(transform);
		_wheelRef.transform.localRotation = Quaternion.identity;
		_wheelRef.transform.position = transform.position;
		_wheelRef.gameObject.name = "characterWheel";

		_switchCharacterCooldown = new TimeCooldown(this);
		_switchCharacterCooldown.onFinish = AllowSwitchCharacter;

	}

	int frameDelay = 1; // security for opening a slot. (prevents openning && selecting character in 1 frame)
	void Update()
	{
		if (!Open)
			return;
		if (frameDelay-- >= 0)
			return;
		if (InputManager.GetButtonDown(InputEnum.B, _joyToListen) && Selected)
			CancelCharacterSelection();

		if (Selected)
			return;


		if (Mathf.Abs(InputManager.GetAxis("x", _joyToListen)) < 0.5f)
		{
			_switchCharacterCooldown.Set(0);
			_canSwitchCharacter = true;
		}

		if (_canSwitchCharacter && Mathf.Abs(InputManager.GetAxis("x", _joyToListen)) > 0.7f)
		{
			ChangeCharacter((int)Mathf.Sign(InputManager.GetAxis("x", _joyToListen)));
		}

		if (InputManager.GetButtonDown(InputEnum.X, _joyToListen))
			ChangeSkin(-1);
		if (InputManager.GetButtonDown(InputEnum.Y, _joyToListen))
			ChangeSkin(1);

		if (InputManager.GetButtonDown(InputEnum.A, _joyToListen))
			SelectCharacter();

	}

	void SelectCharacter()
	{
		if (!CheckIfCharacterIsAvailable())
		{
			Debug.Log("Ce personnage et ce skin sont déja selectionné.");
			return;
		}
		//if (_activeCoroutineRef != null)
		//	StopCoroutine(_activeCoroutineRef);
		GameManager.Instance.RegisteredPlayers[_playerIndex].Ready(_wheelRef.GetSelectedElement(), _selectedSkinIndex);
		
		//PLACEMENT DES PARTICULES A L'ARRACHE
		Vector3 camDirection = (Camera.main.transform.position - transform.position).normalized;

		ParticleSystem spawnParticles = (ParticleSystem) Instantiate(OnCharacterSelectedParticles, transform.position + camDirection * 1.5f, Quaternion.identity);
		spawnParticles.transform.parent = MenuManager.Instance.transform;
		spawnParticles.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
		spawnParticles.GetComponent<FlashAndRotate>()._rotationAxis = transform.forward;

		//if (_selectedCharacterModel != null)
		//	Destroy(_selectedCharacterModel);

		//_wheelRef.GetSelectedElement()._characterData.CharacterModel.Reskin(GetSelectedSkin);
		//_selectedCharacterModel = (GameObject)Instantiate(_wheelRef.GetSelectedElement()._characterData.CharacterModel.gameObject, transform.position, transform.rotation * Quaternion.FromToRotation(Vector3.right, Vector3.left));
		//_selectedCharacterModel.transform.localScale = transform.localScale * 1.1f;
		//_selectedCharacterModel.transform.parent = transform;
		//_selectedCharacterModel.transform.localPosition = Vector3.zero;

		Selected = true;
		//_activeCoroutineRef = StartCoroutine(SlideCharacterModelIn());
	}

	bool CheckIfCharacterIsAvailable()
	{
		for (int i = 0; i < _selectorRef.SlotsAvailable.Length; i++)
		{
			if (_selectorRef.SlotsAvailable[i]._playerIndex != _playerIndex && _selectorRef.SlotsAvailable[i].Open)
			{
				if (_selectorRef.SlotsAvailable[i]._selectedCharacterIndex == _selectedCharacterIndex &&
					_selectorRef.SlotsAvailable[i]._selectedSkinIndex == _selectedSkinIndex)
					return false;
			}
		}

		return true;
	}

	public void CancelCharacterSelection()
	{
		//if (_activeCoroutineRef != null)
		//	StopCoroutine(_activeCoroutineRef);

		Selected = false;
		if (GameManager.Instance.RegisteredPlayers[_playerIndex] != null)
			GameManager.Instance.RegisteredPlayers[_playerIndex].UnReady();

		//_activeCoroutineRef = StartCoroutine(SlideCharacterModelOut());

	}

	//IEnumerator SlideCharacterModelIn()
	//{
	//	float timeTaken = 0.3f;
	//	float eT = 0;
	//	Vector3 targetPosition = (Camera.main.transform.position - transform.position).normalized;
	//	Vector3 startPos;
	//	if (_playerIndex < 2)
	//		startPos = Vector3.up * MenuManager.Instance._canvas.pixelRect.height * (1 / transform.localScale.y) * 0.5f;
	//	else
	//		startPos = - Vector3.up * MenuManager.Instance._canvas.pixelRect.height * (1 / transform.localScale.y) * 0.5f;

	//	_selectedCharacterModel.transform.localPosition = startPos;
	//	while (eT < timeTaken)
	//	{
	//		_selectedCharacterModel.transform.localPosition = Vector3.Lerp(startPos, targetPosition, Curves.EaseInOutCurve.Evaluate(eT / timeTaken));
	//		eT += Time.deltaTime;
	//		yield return null;
	//	}
	//	_selectedCharacterModel.transform.localPosition = targetPosition;
	//}

	//IEnumerator SlideCharacterModelOut()
	//{
	//	float timeTaken = 0.3f;
	//	float eT = 0;
	//	Vector3 startPos = _selectedCharacterModel.transform.localPosition;
	//	Vector3 targetPosition;
	//	if (_playerIndex < 2)
	//		targetPosition = Vector3.up * MenuManager.Instance._canvas.pixelRect.height * (1 / transform.localScale.y) * 0.5f;
	//	else
	//		targetPosition = - Vector3.up * MenuManager.Instance._canvas.pixelRect.height * (1 / transform.localScale.y) * 0.5f;

	//	while (eT < timeTaken)
	//	{
	//		_selectedCharacterModel.transform.localPosition = Vector3.Lerp(startPos, targetPosition, Curves.EaseInOutCurve.Evaluate(eT / timeTaken));
	//		eT += Time.deltaTime;
	//		yield return null;
	//	}

	//	_selectedCharacterModel.transform.localPosition = targetPosition;
	//}

	void AllowSwitchCharacter()
	{
		_canSwitchCharacter = true;
	}

	public void OpenSlot(int playerNumber, int joyToListen)
	{
		Open = true;
		OnSlotOpen.Invoke();
		_joyToListen = joyToListen;
		_playerIndex = playerNumber;
		_wheelRef.gameObject.SetActive(true);
		_wheelRef.transform.SetParent(transform);
		_wheelRef.transform.localRotation = Quaternion.identity;
		if(_wheelRef.transform.localPosition.z < 1)
			_wheelRef.transform.position = transform.position + transform.forward.normalized * (_wheelRef._wheelRadius - 1);
		PlayerController[] tempArray = new PlayerController[_availableCharacters.Length];

		for (int i = 0; i < _availableCharacters.Length; i++)
		{
			tempArray[i] = _availableCharacters[i].CharacterRef;
		}
		_wheelRef.Generate(tempArray);
		ChangeSkin(0);
		Debug.Log("SLOT: " + name + " Opened, Listening to gamePad n°: " + joyToListen);
	}

	public void CloseSlot()
	{
		_wheelRef.Reset();
		OnSlotClose.Invoke();
		_wheelRef.gameObject.SetActive(false);
		Open = false;
		frameDelay = 1;
		_selectedCharacterIndex = 0;
		_selectedSkinIndex = 0;
		Debug.Log("SLOT: " + name + " Closed");
	}


	public void ChangeSkin(int direction)
	{
		TrySwitchSkin(direction);

		_wheelRef.ChangeCharacterSkin(_selectedSkinIndex);
	}

	private int tempSkinSwitchAttempts = 0; // used to prevent infinite loop. (security if the character has less skins than players)
	private void TrySwitchSkin(int direction)
	{
		tempSkinSwitchAttempts++;
		if (direction == 0)
			_selectedSkinIndex = 0;
		else
			_selectedSkinIndex += direction;

		_selectedSkinIndex = _selectedSkinIndex.LoopAround(0, _availableCharacters[_selectedCharacterIndex].CharacterRef._characterData.CharacterMaterials.Length - 1);

		direction = direction == 0 ? 1 : direction;
		if (!CheckIfCharacterIsAvailable() && tempSkinSwitchAttempts < 5)
			TrySwitchSkin(direction);
		else
		{
			if(tempSkinSwitchAttempts == 5)
				Debug.LogWarning("Too many tries in switching skin, canceling");
			tempSkinSwitchAttempts = 0;
		}
	}

	public void ChangeCharacter(int direction)
	{
		_selectedCharacterIndex += direction;

		_selectedCharacterIndex = _selectedCharacterIndex.LoopAround(0, _availableCharacters.Length -1);

		_wheelRef.ScrollToIndex(_selectedCharacterIndex);
		_canSwitchCharacter = false;
		_switchCharacterCooldown.Add(_switchCharacterDelay);
		ChangeSkin(0);


	}
}
