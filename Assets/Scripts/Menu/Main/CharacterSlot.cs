using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
	private static SelectableCharacter[] _availableCharacters;
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

	private bool _canSwitchCharacter = true;

	private CharacterSelectWheel _wheelRef;
	private GameObject _selectedCharacterModel;
	private Vector3 _selectedModelTargetPosition;

	private Coroutine _activeCoroutineRef;

	public SelectableCharacter GetSelectedCharacter
	{
		get
		{
			return _availableCharacters[_selectedCharacterIndex];
		}
	}
	
	public Material GetSelectedSkin
	{
		get
		{
			return _availableCharacters[_selectedCharacterIndex].CharacterRef._characterData.CharacterMaterials[_selectedSkinIndex];
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
		_wheelRef.transform.position = transform.position + transform.forward * CharacterSelectWheel._wheelRadius;
		_wheelRef.gameObject.name = "characterWheel";

		_switchCharacterCooldown = new TimeCooldown(this);
		_switchCharacterCooldown.onFinish = AllowSwitchCharacter;

	}

	void Update()
	{
		if (!Open)
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
		if (_activeCoroutineRef != null)
		StopCoroutine(_activeCoroutineRef);
		GameManager.Instance.RegisteredPlayers[_playerIndex].Ready(_wheelRef.SelectedPlayerController);
		
		//PLACEMENT DES PARTICULES A L'ARRACHE

		Vector3 camDirection = (Camera.main.transform.position - transform.position).normalized;

		ParticleSystem spawnParticles = (ParticleSystem) Instantiate(OnCharacterSelectedParticles, transform.position + camDirection * 1.5f, Quaternion.identity);
		spawnParticles.transform.rotation = transform.rotation;
        spawnParticles.GetComponent<FlashAndRotate>()._rotationAxis = -transform.up;
        spawnParticles.transform.parent = MenuManager.Instance.transform;

        if (_selectedCharacterModel != null)
			Destroy(_selectedCharacterModel);

		GetSelectedCharacter.CharacterRef._characterData.CharacterModel.Reskin(GetSelectedSkin);
		_selectedCharacterModel = (GameObject)Instantiate(GetSelectedCharacter.CharacterRef._characterData.CharacterModel.gameObject, transform.position - transform.up * 30, transform.rotation * Quaternion.FromToRotation(Vector3.right, Vector3.left));
		_selectedCharacterModel.transform.parent = transform;
		_selectedModelTargetPosition = _selectedCharacterModel.transform.position;

		Selected = true;
		_activeCoroutineRef = StartCoroutine(SlideCharacterModelIn());
	}

	public void CancelCharacterSelection()
	{
		if (_activeCoroutineRef != null)
			StopCoroutine(_activeCoroutineRef);

		Selected = false;
		GameManager.Instance.RegisteredPlayers[_playerIndex].UnReady();
		Debug.Log("cancel selection");

		_activeCoroutineRef = StartCoroutine(SlideCharacterModelOut());

	}

	IEnumerator SlideCharacterModelIn()
	{
		float targetTime = Time.time + 1;
		Vector3 targetPosition = transform.position + (Camera.main.transform.position - transform.position).normalized;

		while (targetTime > Time.time)
		{
			_selectedCharacterModel.transform.position = Vector3.Lerp(_selectedCharacterModel.transform.position, targetPosition, 0.1f);
			yield return null;
		}
	}

	IEnumerator SlideCharacterModelOut()
	{
		float targetTime = Time.time + 1;
		Vector3 targetPosition = transform.position + transform.up * 10;

		while (targetTime > Time.time)
		{
			_selectedCharacterModel.transform.position = Vector3.Lerp(_selectedCharacterModel.transform.position, targetPosition, 0.1f);
			yield return null;
		}
	}

	void AllowSwitchCharacter()
	{
		_canSwitchCharacter = true;
	}

	public void OpenSlot(int playerNumber, int joyToListen)
	{
		Open = true;
		_joyToListen = joyToListen;
		_playerIndex = playerNumber;
		_wheelRef.gameObject.SetActive(true);
		_wheelRef.Generate(_availableCharacters);
		Debug.Log("SLOT: " + name + " Opened, Listening to gamePad n°: " + joyToListen);
	}

	public void CloseSlot()
	{
		_wheelRef.gameObject.SetActive(false);
		Open = false;
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
		_wheelRef.ChangeActiveCharacterSkin(_availableCharacters[_selectedCharacterIndex].ArtWorks[_selectedSkinIndex]);

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
