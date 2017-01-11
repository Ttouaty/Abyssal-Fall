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

	private int _joyToListen; 
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
			_wheelRef.CmdChangeCharacterSkin((_wheelRef._selectedSkinIndex - 1).LoopAround(0, _availableCharacters[_wheelRef._selectedElementIndex]._characterData.CharacterMaterials.Length - 1));
		if (InputManager.GetButtonDown(InputEnum.Y, _joyToListen))
			_wheelRef.CmdChangeCharacterSkin((_wheelRef._selectedSkinIndex + 1).LoopAround(0, _availableCharacters[_wheelRef._selectedElementIndex]._characterData.CharacterMaterials.Length - 1));

		if (InputManager.GetButtonDown(InputEnum.A, _joyToListen))
			SelectCharacter();

	}

	void SelectCharacter()
	{
		//if (_activeCoroutineRef != null)
		//	StopCoroutine(_activeCoroutineRef);
		_playerRef.Ready(_wheelRef._selectedElementIndex, _wheelRef._selectedSkinIndex);
		
		Vector3 camDirection = (Camera.main.transform.position - transform.position).normalized;

		ParticleSystem spawnParticles = (ParticleSystem) Instantiate(OnCharacterSelectedParticles, transform.position + camDirection * 1.5f, Quaternion.identity);
		spawnParticles.transform.parent = MenuManager.Instance.transform;
		spawnParticles.transform.rotation = Quaternion.LookRotation(transform.forward, transform.up);
		spawnParticles.GetComponent<FlashAndRotate>()._rotationAxis = transform.forward;

		Selected = true;
		//_activeCoroutineRef = StartCoroutine(SlideCharacterModelIn());
	}

	

	public void CancelCharacterSelection()
	{
		//if (_activeCoroutineRef != null)
		//	StopCoroutine(_activeCoroutineRef);

		Selected = false;
		if(_playerRef != null)
			_playerRef.UnReady();

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
		if (_wheelRef.GetComponent<NetworkIdentity>().clientAuthorityOwner != null)
			_wheelRef.GetComponent<NetworkIdentity>().RemoveClientAuthority(_wheelRef.GetComponent<NetworkIdentity>().clientAuthorityOwner);

		_wheelRef.GetComponent<NetworkIdentity>().AssignClientAuthority(player.connectionToClient);
		_netSpawned = true;
		Debug.Log("SLOT: " + name + " Opened, Listening to gamePad n°: " + player.JoystickNumber);
		ChangeCharacter(0);
	}

	public void CloseSlot()
	{
		_wheelRef.Reset();
		OnSlotClose.Invoke();
		_wheelRef.gameObject.SetActive(false);
		Open = false;
		_netSpawned = false;
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
