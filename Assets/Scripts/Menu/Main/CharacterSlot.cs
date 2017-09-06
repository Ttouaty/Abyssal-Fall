using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class CharacterSlot : MonoBehaviour
{
	private static PlayerController[] _availableCharacters;

	public CharacterSelectPedestal TargetPedestal;
	public Transform WheelSlot;
	public GameObject PressAContainer;
	public Text SpecialText;
	public Image GamePadController;
	public Image KeyboardController;
	public GameObject ArrowContainers;
	public GameObject PortraitBG;
	public GameObject PortraitImage;

	[HideInInspector]
	public bool Open = false;
	[HideInInspector]
	public bool Selected = false;
	[HideInInspector]
	public Player _playerRef;

	[Space]
	public UnityEvent OnSlotOpen;
	public UnityEvent OnSlotClose;
	public UnityEvent OnCharacterSelect;
	public UnityEvent OnCharacterChangeSkin;

	private bool _canSwitchCharacter = true;
	private float _switchCharacterDelay = 0.15f;
	private TimeCooldown _switchCharacterCooldown;
	
	private CharacterSelectWheel _wheelRef;
	private CharacterSelector _selectorRef;

	void Start()
	{
		_selectorRef = GetComponentInParent<CharacterSelector>();
		if (_availableCharacters == null) //setup static vars (messy)
		{
			_availableCharacters = _selectorRef._availableCharacters;
		}

		_switchCharacterCooldown = new TimeCooldown(this);
		_switchCharacterCooldown.onFinish = AllowSwitchCharacter;
		SetTextAlpha(0);
	}

	

	int frameDelay = 1; // security for opening a slot. (prevents openning && selecting character in 1 frame) dirty As Fuck
	void Update()
	{
		//if (!NetworkServer.active && NetworkClient.active) //Commented for multiple online clients
		//	PressAContainer.SetActive(false);

		if (!Open)
			return;
		if (frameDelay-- >= 0)
			return;

		if (_playerRef == null)
			return;

		if (!_playerRef.isLocalPlayer)
			return;

		
		if (InputManager.GetButtonDown(InputEnum.B, _playerRef.JoystickNumber) && Selected)
			CancelCharacterSelection();

		SetTextAlpha(Convert.ToInt32(InputManager.GetButtonHeld(InputEnum.X, _playerRef.JoystickNumber)));

		if (Selected)
			return;

		if (_wheelRef.IsGenerated)
			PortraitImage.GetComponent<Image>().sprite = _wheelRef.GetSelectedElement()._characterData.Portrait;

		KeyboardController.gameObject.SetActive(!_playerRef.IsUsingGamePad);
		GamePadController.gameObject.SetActive(_playerRef.IsUsingGamePad);


		if (InputManager.GetButtonDown(InputEnum.Y, _playerRef.JoystickNumber))
		{
			OnCharacterChangeSkin.Invoke();
			_wheelRef.CmdChangeCharacterSkin((_wheelRef._selectedSkinIndex + 1).LoopAround(0, _availableCharacters[_wheelRef._selectedElementIndex]._characterData.NumberOfSkins - 1));
		}

		if (InputManager.GetButtonDown(InputEnum.A, _playerRef.JoystickNumber))
			SelectCharacter();

		if (_availableCharacters.Length <= 1)
			return;

		if (Mathf.Abs(InputManager.GetAxis("x", _playerRef.JoystickNumber)) < 0.5f)
		{
			_switchCharacterCooldown.Set(0);
			_canSwitchCharacter = true;
		}

		if (_canSwitchCharacter && Mathf.Abs(InputManager.GetAxis("x", _playerRef.JoystickNumber)) > 0.7f)
		{
			ChangeCharacter((int)Mathf.Sign(InputManager.GetAxis("x", _playerRef.JoystickNumber)));
		}

		//if (InputManager.GetButtonDown(InputEnum.X, _playerRef.JoystickNumber))
		//	_wheelRef.CmdChangeCharacterSkin((_wheelRef._selectedSkinIndex - 1).LoopAround(0, _availableCharacters[_wheelRef._selectedElementIndex]._characterData.NumberOfSkins -1));
		

	}

	public void SelectPedestal(bool ready)
	{
		TargetPedestal.SetSelect(ready);
		if (ready)
			OnCharacterSelect.Invoke();
	}

	void SelectCharacter()
	{
		_playerRef.Ready(_wheelRef._selectedElementIndex, _wheelRef._selectedSkinIndex);
		ArrowContainers.SetActive(false);

		Selected = true;
	}

	

	public void CancelCharacterSelection()
	{
		Selected = false;
		if (_playerRef != null)
			_playerRef.UnReady();
		else
			return;
		ArrowContainers.SetActive(_playerRef.isLocalPlayer && _availableCharacters.Length > 1);
	}


	void AllowSwitchCharacter()
	{
		_canSwitchCharacter = true;
	}

	public void OpenSlot(CharacterSelectWheel newWheel)
	{
		_wheelRef = newWheel;

		if (_availableCharacters == null)
		{
			_selectorRef = MenuManager.Instance._characterSlotsContainerRef;
			_availableCharacters = _selectorRef._availableCharacters;
		}

		if (!Open)
		{
			Open = true;
			OnSlotOpen.Invoke();
		}

		PressAContainer.SetActive(false);

		_playerRef = _wheelRef._playerRef.GetComponent<Player>();
		_wheelRef.transform.localRotation = Quaternion.identity;

		KeyboardController.gameObject.SetActive(!_playerRef.IsUsingGamePad);
		GamePadController.gameObject.SetActive(_playerRef.IsUsingGamePad);

		TargetPedestal.transform.rotation = Camera.main.transform.rotation;

		ArrowContainers.SetActive(_playerRef.isLocalPlayer && _availableCharacters.Length > 1);
	}

	public void CloseSlot()
	{
		OnSlotClose.Invoke();
		Open = false;
		frameDelay = 1;
		SetTextAlpha(0);
		GamePadController.gameObject.SetActive(false);
		KeyboardController.gameObject.SetActive(false);
		ArrowContainers.SetActive(false);
		PressAContainer.SetActive(true);

		Debug.Log("SLOT: " + name + " Closed");
	}

	public void ChangeCharacter(int direction)
	{
		_wheelRef.CmdChangeCharacterSkinPrecise(0, _wheelRef._selectedElementIndex);
		_wheelRef.CmdScrollToIndex((_wheelRef._selectedElementIndex + direction).LoopAround(0, _availableCharacters.Length - 1));
		_wheelRef.CmdChangeCharacterSkin(0);

		_canSwitchCharacter = false;
		_switchCharacterCooldown.Add(_switchCharacterDelay);
	}

	public void SetCharacterInfoText(string special)
	{
		SpecialText.text = special;
	}

	public void SetTextAlpha(float alpha)
	{
		if (SpecialText.GetComponentInParent<CanvasGroup>() == null)
			return;

		if(_playerRef != null)
		{
			if(_playerRef.isLocalPlayer)
			{
				SpecialText.GetComponentInParent<CanvasGroup>().alpha = alpha;
				return;
			}
		}

		SpecialText.GetComponentInParent<CanvasGroup>().alpha = 0;
	}
}
