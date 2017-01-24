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
	private float _switchCharacterDelay = 0.15f;
	private TimeCooldown _switchCharacterCooldown;

	[HideInInspector]
	public bool Open = false;
	[HideInInspector]
	public bool Selected = false;

	[Space]
	public UnityEvent OnSlotOpen;
	public UnityEvent OnSlotClose;

	private bool _canSwitchCharacter = true;
	[HideInInspector]
	public Player _playerRef;
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

	}

	

	int frameDelay = 1; // security for opening a slot. (prevents openning && selecting character in 1 frame)
	void Update()
	{
		if(!NetworkServer.active && NetworkClient.active)
			transform.FindChild("BG Container").gameObject.SetActive(false);

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
	}

	void SelectCharacter()
	{
		//if (_activeCoroutineRef != null)
		//	StopCoroutine(_activeCoroutineRef);
		_playerRef.Ready(_wheelRef._selectedElementIndex, _wheelRef._selectedSkinIndex);
		
		//Vector3 camDirection = (Camera.main.transform.position - transform.position).normalized;
		//SelectCharacter();
		//ParticleSystem spawnParticles = (ParticleSystem) Instantiate(OnCharacterSelectedParticles, transform.position + camDirection * 1.5f, Quaternion.identity);
		//spawnParticles.transform.parent = MenuManager.Instance.transform;
		//spawnParticles.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
		//spawnParticles.GetComponent<FlashAndRotate>()._rotationAxis = transform.forward;

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

		//Debug.Log("Generate wheel "+name+" with netID => "+_wheelRef.GetComponent<NetworkIdentity>().netId);
		_playerRef = _wheelRef._playerRef.GetComponent<Player>();
		_wheelRef.transform.localRotation = Quaternion.identity;

		//if (!NetworkServer.active || _netSpawned)
		//{
		//	return;
		//}

		//NetworkServer.SpawnWithClientAuthority(_wheelRef.gameObject, player.connectionToClient);
		//if (_wheelRef.GetComponent<NetworkIdentity>().clientAuthorityOwner != null)
		//	_wheelRef.GetComponent<NetworkIdentity>().RemoveClientAuthority(_wheelRef.GetComponent<NetworkIdentity>().clientAuthorityOwner);
		
		//if (_wheelRef.netId.Value == 0)
		//	NetworkServer.SpawnWithClientAuthority(_wheelRef.gameObject, player.connectionToClient);
		//else
		//	_wheelRef.GetComponent<NetworkIdentity>().AssignClientAuthority(player.connectionToClient);
		//_netSpawned = true;
		//Debug.Log("SLOT: " + name + " Opened, Listening to gamePad n°: " + player.JoystickNumber);
		//ChangeCharacter(characterNumber);
		//_wheelRef.CmdChangeCharacterSkin(skinNumber);
	}

	public void CloseSlot()
	{
		_wheelRef.Reset();
		OnSlotClose.Invoke();
		_wheelRef.gameObject.SetActive(false);
		Open = false;
		frameDelay = 1;
		_wheelRef.CmdChangeCharacterSkin(0);
		_wheelRef.CmdScrollToIndex(0);
		Debug.Log("SLOT: " + name + " Closed");
	}

	public void ChangeCharacter(int direction)
	{
		_wheelRef.CmdChangeCharacterSkin(0);
		_wheelRef.CmdScrollToIndex((_wheelRef._selectedElementIndex + direction).LoopAround(0, _availableCharacters.Length - 1));
		_canSwitchCharacter = false;
		_switchCharacterCooldown.Add(_switchCharacterDelay);
	}
}
