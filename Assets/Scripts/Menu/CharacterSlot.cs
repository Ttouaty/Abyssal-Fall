using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterSlot : MonoBehaviour
{
	private static SelectableCharacter[] _availableCharacters;
	
	private float _switchCharacterDelay = 0.2f;
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
		if (_availableCharacters == null)
			_availableCharacters = GetComponentInParent<CharacterSlotsContainer>()._availableCharacters;


		_wheelRef = new GameObject().AddComponent<CharacterSelectWheel>();
		_wheelRef.transform.SetParent(transform);
		_wheelRef.transform.localPosition = transform.forward * 60;
		_wheelRef.gameObject.name = "characterWheel";

		_switchCharacterCooldown = new TimeCooldown(this);
		_switchCharacterCooldown.onFinish = AllowSwitchCharacter;

	}

	void Update()
	{
		if (!Open)
			return;

		if (InputManager.GetButtonDown(1, _joyToListen, true))
		{
			Debug.Log("cancel selection");
			GameManager.instance.RegisteredPlayers[_playerIndex].UnReady();
			Selected = false;
		}

		if (Selected)
			return;

		//Cheesing
		if (_canSwitchCharacter)
		{
			if (Input.GetKeyDown(KeyCode.RightArrow))
				ChangeCharacter(1);
			if (Input.GetKeyDown(KeyCode.LeftArrow))
				ChangeCharacter(-1);
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
			ChangeSkin(1);
		if (Input.GetKeyDown(KeyCode.DownArrow))
			ChangeSkin(-1);


		//end Cheesing

		/*
		 rendre fonctionnel le character select
		 tester une partie !		 
		 */

		if (_canSwitchCharacter)
		{
			if (Mathf.Abs(InputManager.GetAxis("x", _joyToListen)) > 0.9f)
				ChangeCharacter((int) Mathf.Sign(InputManager.GetAxis("x", _joyToListen)));
		}

		if (InputManager.GetButtonDown(3, _joyToListen))
			ChangeSkin(1);

		if (InputManager.GetButtonDown(0, _joyToListen))
		{
			GameObject characterModel = Instantiate(_availableCharacters[_selectedCharacterIndex].CharacterRef._characterData.CharacterModel.gameObject, Camera.main.ScreenToWorldPoint(transform.position), Quaternion.identity) as GameObject;
			GameManager.instance.RegisteredPlayers[_playerIndex].Ready(_availableCharacters[_selectedCharacterIndex].CharacterRef);
			Selected = true;
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
