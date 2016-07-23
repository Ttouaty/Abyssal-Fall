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

	public int GetSelectedSkin
	{
		get
		{
			return _selectedSkinIndex;
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

		if (InputManager.GetButtonDown(1, _joyToListen, true))
			CancelCharacterSelection();

		if (Selected)
			return;

		if (_canSwitchCharacter)
		{
			if (_joyToListen == 0)
			{ 
				if (Mathf.Abs(InputManager.GetAxis("x", _joyToListen)) > 0.5f)
					ChangeCharacter((int)Mathf.Sign(InputManager.GetAxis("x", _joyToListen)));
			}
			else
			{
				if (Mathf.Abs(InputManager.GetAxis("x", _joyToListen)) > 0.9f)
					ChangeCharacter((int) Mathf.Sign(InputManager.GetAxis("x", _joyToListen)));
			}
		}

		if (InputManager.GetButtonDown(3, _joyToListen))
			ChangeSkin(1);

		if (InputManager.GetButtonDown(0, _joyToListen))
			SelectCharacter();

	}

	void SelectCharacter()
	{
		if (_activeCoroutineRef != null)
		StopCoroutine(_activeCoroutineRef);

		GameManager.instance.RegisteredPlayers[_playerIndex].Ready(_wheelRef.SelectedPlayerController);
		//PLACEMENT DES PARTICULES A L'ARRACHE

		// changer les particules
		/*
		 * faire 2 systems de particules
		 * 1 flash 
		 * et une explosion de particules en dessous.
		 */
		Instantiate(OnCharacterSelectedParticles, transform.position - transform.up * 5 + (Camera.main.transform.position - transform.position).normalized * 1.5f, transform.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.up));
	
		if (_selectedCharacterModel != null)
			Destroy(_selectedCharacterModel);

		_selectedCharacterModel = (GameObject)Instantiate(GetSelectedCharacter.CharacterRef._characterData.CharacterModel.gameObject, transform.position - transform.up * 30, transform.rotation * Quaternion.FromToRotation(Vector3.right, Vector3.left));
		_selectedModelTargetPosition = _selectedCharacterModel.transform.position;

		Selected = true;
		_activeCoroutineRef = StartCoroutine(SlideCharacterModelIn());
	}

	void CancelCharacterSelection()
	{
		if (_activeCoroutineRef != null)
			StopCoroutine(_activeCoroutineRef);

		Selected = false;
		GameManager.instance.RegisteredPlayers[_playerIndex].UnReady();
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
		_wheelRef.Generate(_availableCharacters);
		Debug.Log("SLOT: " + name + ", Listening to gamePad n°: " + joyToListen);
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
