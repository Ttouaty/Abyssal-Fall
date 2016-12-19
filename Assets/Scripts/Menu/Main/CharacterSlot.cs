using UnityEngine;
using UnityEngine.Networking;
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
	private static PlayerController[] _availableCharacters;
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
	public Player _playerRef;
	public CharacterSelectWheel _wheelRef;
	private bool _netSpawned = false;
	//private GameObject _selectedCharacterModel;

	//private Coroutine _activeCoroutineRef;

	public PlayerController GetSelectedCharacter
	{
		get
		{
			return GetComponentInChildren<CharacterSelectWheel>().GetSelectedElement();
		}
	}
	
	public Material GetSelectedSkin
	{
		get
		{
			return GetComponentInChildren<CharacterSelectWheel>().GetSelectedSkin();
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

		if (_playerRef == null)
			return;
		if (!_playerRef.isLocalPlayer)
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
		_playerRef.Ready(_selectedCharacterIndex, _selectedSkinIndex);
		
		Vector3 camDirection = (Camera.main.transform.position - transform.position).normalized;

		ParticleSystem spawnParticles = (ParticleSystem) Instantiate(OnCharacterSelectedParticles, transform.position + camDirection * 1.5f, Quaternion.identity);
		spawnParticles.transform.parent = MenuManager.Instance.transform;
		spawnParticles.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
		spawnParticles.GetComponent<FlashAndRotate>()._rotationAxis = transform.forward;

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
		if (ServerManager.Instance.RegisteredPlayers.Count == 0)
			return;

		if (ServerManager.Instance.RegisteredPlayers[_playerIndex] != null)
			ServerManager.Instance.RegisteredPlayers[_playerIndex].UnReady();

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

	
	public void OpenSlot(int playerNumber, Player player)
	{
		_wheelRef.gameObject.SetActive(true);
		if (!Open)
		{
			Open = true;
			OnSlotOpen.Invoke();
		}

		Debug.Log("Generate wheel "+name+" with netID => "+_wheelRef.GetComponent<NetworkIdentity>().netId);
		if (!_wheelRef.isGenerated)
		{
			PlayerController[] tempArray = new PlayerController[_availableCharacters.Length];

			for (int i = 0; i < _availableCharacters.Length; i++)
			{
				tempArray[i] = _availableCharacters[i];
			}

			_wheelRef.Generate(tempArray, null);
		}

		if (player != null)
		{
			_playerRef = player;
			_joyToListen = player.JoystickNumber;
			_playerIndex = playerNumber;
			_wheelRef.transform.localRotation = Quaternion.identity;
			_wheelRef.ParentPlayer = player;
		}
		else
			return;

		if (!NetworkServer.active || _netSpawned)
		{
			return;
		}

		//NetworkServer.SpawnWithClientAuthority(_wheelRef.gameObject, player.connectionToClient);
		_wheelRef.GetComponent<NetworkIdentity>().AssignClientAuthority(player.connectionToClient);
		_netSpawned = true;
		Debug.Log("SLOT: " + name + " Opened, Listening to gamePad n°: " + player.JoystickNumber);
	}

	public void CloseSlot()
	{
		_wheelRef.Reset();
		OnSlotClose.Invoke();
		_wheelRef.gameObject.SetActive(false);
		Open = false;
		_netSpawned = false;
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

		_selectedSkinIndex = _selectedSkinIndex.LoopAround(0, _availableCharacters[_selectedCharacterIndex]._characterData.CharacterMaterials.Length - 1);

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
