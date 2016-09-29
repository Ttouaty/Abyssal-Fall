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
	private GameObject _selectedCharacterModel;

	private Coroutine _activeCoroutineRef;

	public WheelSelectableCharacter GetSelectedCharacter
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
		if (_activeCoroutineRef != null)
			StopCoroutine(_activeCoroutineRef);
		GameManager.Instance.RegisteredPlayers[_playerIndex].Ready(_wheelRef.GetSelectedElement().Controller, _selectedSkinIndex);
		
		//PLACEMENT DES PARTICULES A L'ARRACHE
		Vector3 camDirection = (Camera.main.transform.position - transform.position).normalized;

		ParticleSystem spawnParticles = (ParticleSystem) Instantiate(OnCharacterSelectedParticles, transform.position + camDirection * 1.5f, Quaternion.identity);
		spawnParticles.transform.parent = MenuManager.Instance.transform;
		spawnParticles.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
		spawnParticles.GetComponent<FlashAndRotate>()._rotationAxis = transform.forward;

		if (_selectedCharacterModel != null)
			Destroy(_selectedCharacterModel);

		_wheelRef.GetSelectedElement().Controller._characterData.CharacterModel.Reskin(GetSelectedSkin);
		_selectedCharacterModel = (GameObject)Instantiate(_wheelRef.GetSelectedElement().Controller._characterData.CharacterModel.gameObject, transform.position - transform.up * 30, transform.rotation * Quaternion.FromToRotation(Vector3.right, Vector3.left));
		_selectedCharacterModel.transform.parent = transform;

		Selected = true;
		_activeCoroutineRef = StartCoroutine(SlideCharacterModelIn());
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
		if (_activeCoroutineRef != null)
			StopCoroutine(_activeCoroutineRef);

		Selected = false;
		if (GameManager.Instance.RegisteredPlayers[_playerIndex] != null)
			GameManager.Instance.RegisteredPlayers[_playerIndex].UnReady();

		_activeCoroutineRef = StartCoroutine(SlideCharacterModelOut());

	}

	IEnumerator SlideCharacterModelIn()
	{
		float targetTime = Time.time + 1;
		Vector3 targetPosition = transform.position + (Camera.main.transform.position - transform.position).normalized;

		if (_playerIndex < 2)
			_selectedCharacterModel.transform.position = transform.position + transform.up * 10;
		else
			_selectedCharacterModel.transform.position = transform.position - transform.up * 10;

		while (targetTime > Time.time)
		{
			_selectedCharacterModel.transform.position = Vector3.Lerp(_selectedCharacterModel.transform.position, targetPosition, 0.15f);
			yield return null;
		}
		_selectedCharacterModel.transform.position = targetPosition;
	}

	IEnumerator SlideCharacterModelOut()
	{
		float targetTime = Time.time + 1;
		Vector3 targetPosition;
		if (_playerIndex < 2)
			targetPosition = transform.position + transform.up * 10;
		else
			targetPosition = transform.position - transform.up * 10;

		while (targetTime > Time.time)
		{
			_selectedCharacterModel.transform.position = Vector3.Lerp(_selectedCharacterModel.transform.position, targetPosition, 0.15f);
			yield return null;
		}

		_selectedCharacterModel.transform.position = targetPosition;
	}

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
		_wheelRef.transform.position = transform.position;
		_wheelRef.Generate(_availableCharacters);
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
		int oldIndex = _selectedSkinIndex;

		TrySwitchSkin(direction);

		_wheelRef.ChangeActiveCharacterSkin(_availableCharacters[_selectedCharacterIndex].ArtWorks[_selectedSkinIndex], _selectedSkinIndex);
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
