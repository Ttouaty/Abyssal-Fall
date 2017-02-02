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
	public Text SpeedText;

	[HideInInspector]
	public bool Open = false;
	[HideInInspector]
	public bool Selected = false;
	[HideInInspector]
	public Player _playerRef;

	[Space]
	public UnityEvent OnSlotOpen;
	public UnityEvent OnSlotClose;

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
		if (!NetworkServer.active && NetworkClient.active)
			PressAContainer.SetActive(false);

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
		if (Selected)
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

		if (InputManager.GetButtonDown(InputEnum.X, _playerRef.JoystickNumber))
			_wheelRef.CmdChangeCharacterSkin((_wheelRef._selectedSkinIndex - 1).LoopAround(0, _availableCharacters[_wheelRef._selectedElementIndex]._characterData.NumberOfSkins -1));
		if (InputManager.GetButtonDown(InputEnum.Y, _playerRef.JoystickNumber))
			_wheelRef.CmdChangeCharacterSkin((_wheelRef._selectedSkinIndex + 1).LoopAround(0, _availableCharacters[_wheelRef._selectedElementIndex]._characterData.NumberOfSkins -1));

		if (InputManager.GetButtonDown(InputEnum.A, _playerRef.JoystickNumber))
			SelectCharacter();

	}

	public void SelectPedestal(bool ready)
	{
		TargetPedestal.SetSelect(ready);
		SetTextAlpha(Convert.ToInt32(!ready));
	}

	void SelectCharacter()
	{
		_playerRef.Ready(_wheelRef._selectedElementIndex, _wheelRef._selectedSkinIndex);

		Selected = true;
	}

	

	public void CancelCharacterSelection()
	{
		Selected = false;
		if(_playerRef != null)
			_playerRef.UnReady();
	}


	void AllowSwitchCharacter()
	{
		_canSwitchCharacter = true;
	}

	public void OpenSlot(CharacterSelectWheel newWheel)
	{
		_wheelRef = newWheel;

		if (!Open)
		{
			Open = true;
			OnSlotOpen.Invoke();
		}

		_playerRef = _wheelRef._playerRef.GetComponent<Player>();
		_wheelRef.transform.localRotation = Quaternion.identity;

		if (_playerRef.isLocalPlayer)
			SetTextAlpha(1);
	}

	public void CloseSlot()
	{
		OnSlotClose.Invoke();
		Open = false;
		frameDelay = 1;
		SetTextAlpha(0);
		Debug.Log("SLOT: " + name + " Closed");
	}

	public void ChangeCharacter(int direction)
	{
		_wheelRef.CmdChangeCharacterSkin(0);
		_wheelRef.CmdScrollToIndex((_wheelRef._selectedElementIndex + direction).LoopAround(0, _availableCharacters.Length - 1));
		_canSwitchCharacter = false;
		_switchCharacterCooldown.Add(_switchCharacterDelay);
	}

	public void SetCharacterInfoText(string special, string speed)
	{
		SpecialText.text = special;
		SpeedText.text = speed;
	}

	public void SetTextAlpha(float alpha)
	{
		if(_playerRef != null)
		{
			if(_playerRef.isLocalPlayer)
			{
				SpecialText.GetComponentInParent<CanvasGroup>().alpha = alpha;
				return;
			}
		}

		Debug.Log("Preventing alpha (player is not local player)");
		SpecialText.GetComponentInParent<CanvasGroup>().alpha = 0;
	}
}
