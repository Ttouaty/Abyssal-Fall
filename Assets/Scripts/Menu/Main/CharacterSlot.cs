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
	private static bool[,] _selectedCharacters = new bool[4,4];
	public static ParticleSystem OnCharacterSelectedParticles;
	

	private float _switchCharacterDelay = 0.15f;
	private TimeCooldown _switchCharacterCooldown;
	private int _selectedCharacterIndex = 0;
	private int _selectedSkinIndex = 0;

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

	void Start()
	{
		if (_availableCharacters == null) //setup static vars (messy)
		{
			OnCharacterSelectedParticles = GetComponentInParent<CharacterSlotsContainer>().OnCharacterSelectedParticles;
			_availableCharacters = GetComponentInParent<CharacterSlotsContainer>()._availableCharacters;
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
		GameManager.Instance.RegisteredPlayers[_playerIndex].Ready(_wheelRef.GetSelectedElement().Controller, _wheelRef._selectedSkinIndex);
		
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
		_selectedCharacters[_selectedCharacterIndex, _selectedSkinIndex] = true;
		Selected = true;
		_activeCoroutineRef = StartCoroutine(SlideCharacterModelIn());
	}

	bool CheckIfCharacterIsAvailable()
	{
		for (int i = 0; i < _selectedCharacters.Length; ++i)
		{
			if (_selectedCharacters[_selectedCharacterIndex,_selectedSkinIndex])
				return false;
		}
		return true;
	}

	public void CancelCharacterSelection()
	{
		if (_activeCoroutineRef != null)
			StopCoroutine(_activeCoroutineRef);

		_selectedCharacters[_selectedCharacterIndex, _selectedSkinIndex] = false;
		Selected = false;
		if (GameManager.Instance.RegisteredPlayers[_playerIndex] != null)
			GameManager.Instance.RegisteredPlayers[_playerIndex].UnReady();

		_activeCoroutineRef = StartCoroutine(SlideCharacterModelOut());

	}

	IEnumerator SlideCharacterModelIn()
	{
		float targetTime = Time.time + 1;
		Vector3 targetPosition = transform.position + (Camera.main.transform.position - transform.position).normalized;

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
		Vector3 targetPosition = transform.position + transform.up * 10;

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
		//transform.GetChild(_selectedCharacterIndex).GetChild(_selectedSkinIndex).gameObject.SetActive(false);
		if(direction == 0)
			_selectedSkinIndex = 0;
		else
			_selectedSkinIndex += direction;

		if (_selectedSkinIndex >= _availableCharacters[_selectedCharacterIndex].CharacterRef._characterData.CharacterMaterials.Length)
			_selectedSkinIndex = 0;
		else if (_selectedSkinIndex < 0)
			_selectedSkinIndex = _availableCharacters[_selectedCharacterIndex].CharacterRef._characterData.CharacterMaterials.Length - 1;

		//transform.GetChild(_selectedCharacterIndex).GetChild(_selectedSkinIndex).gameObject.SetActive(true);
		_wheelRef.ChangeActiveCharacterSkin(_availableCharacters[_selectedCharacterIndex].ArtWorks[_selectedSkinIndex], _selectedSkinIndex);

	}

	public void ChangeCharacter(int direction)
	{
		//transform.GetChild(_selectedCharacterIndex).gameObject.SetActive(false);

		_selectedCharacterIndex += direction;

		if (_selectedCharacterIndex >= _availableCharacters.Length)
			_selectedCharacterIndex = 0;
		else if (_selectedCharacterIndex < 0)
			_selectedCharacterIndex = _availableCharacters.Length -1;

		//transform.GetChild(_selectedCharacterIndex).gameObject.SetActive(true);

		_wheelRef.ScrollToIndex(_selectedCharacterIndex);
		_canSwitchCharacter = false;
		_switchCharacterCooldown.Add(_switchCharacterDelay);
	}
}
