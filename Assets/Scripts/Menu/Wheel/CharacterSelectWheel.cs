using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterSelectWheel : NetworkBehaviour
{
	public static Dictionary<int, CharacterSelectWheel> WheelsRef = new Dictionary<int, CharacterSelectWheel>(4);

	public float _wheelRadius = 3;
	public float _rotateSpeed = 0.2f;

	protected float _alphaThresholdAngleMin = 10;
	protected float _alphaThresholdAngleMax = 90;

	protected GameObject[] _displayArray = new GameObject[0];
	protected PlayerController[] _returnArray = new PlayerController[0];

	[SyncVar(hook = "ScrollToIndex")]
	public int _selectedElementIndex = 0;
	[SyncVar(hook = "ChangeCharacterSkin")]
	[HideInInspector]
	public int _selectedSkinIndex = 0;
	protected float _rotationBetweenElements;

	protected float _tempElementAngle;

	[HideInInspector]
	[SyncVar]
	public GameObject _playerRef;
	private int playerNumber = 0;

	#region baseClass
	private void Internal_Generate(GameObject[] elementsToDisplay, PlayerController[] elementsToReturn)
	{
		transform.localScale = Vector3.one;

		for (int i = 0; i < _displayArray.Length; i++)
		{
			Destroy(_displayArray[i].gameObject);
		}

		_returnArray = elementsToReturn;
		_displayArray = elementsToDisplay;

		_rotationBetweenElements = 360 / elementsToDisplay.Length;

		if (_displayArray.Length != _returnArray.Length)
			Debug.LogError("Display and return arrays lengths do not match!\nThis may/will cause errors on selection.");

		for (int i = 0; i < elementsToDisplay.Length; i++)
		{
			elementsToDisplay[i].transform.SetParent(transform);
			elementsToDisplay[i].transform.localPosition = elementsToDisplay[i].transform.localPosition.ZeroY();


			ElementGenerate(elementsToDisplay[i], Quaternion.AngleAxis(-_rotationBetweenElements * i, Vector3.up) * Vector3.back * _wheelRadius);
			//elementsToDisplay[i].transform.RotateAround(transform.position, transform.up, -_rotationBetweenElements * i);
		}

		Update();
	}

	protected void ElementGenerate(GameObject element, Vector3 localPos)
	{
		element.transform.localRotation = Quaternion.identity;
		element.transform.localPosition = localPos;
	}

	public PlayerController GetSelectedElement()
	{
		return _returnArray[_selectedElementIndex];
	}

	protected void Update()
	{
		transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, _selectedElementIndex * _rotationBetweenElements, 0), _rotateSpeed);

		//######## apply alpha to Image elements #########
		RotateElementsFacingCam();
		ApplyAlpha();
	}

	protected void ApplyAlpha()
	{
		for (int i = 0; i < _displayArray.Length; i++)
		{
			_tempElementAngle = Vector3.Angle(-transform.parent.forward.normalized, (_displayArray[i].transform.position - transform.position).normalized);

			Debug.DrawLine(transform.position, _displayArray[i].transform.position, Color.green);

			if (_tempElementAngle < _alphaThresholdAngleMin)
				_displayArray[i].GetComponentInChildren<SetRenderQueue>().SetCutOff(1);
			else
				_displayArray[i].GetComponentInChildren<SetRenderQueue>().SetCutOff((_alphaThresholdAngleMax - _tempElementAngle) / _alphaThresholdAngleMax);
		}
	}

	protected void RotateElementsFacingCam()
	{
		for (int i = 0; i < _displayArray.Length; ++i)
		{
			_displayArray[i].transform.localRotation = Quaternion.Euler(0,180,0) * Quaternion.Inverse(transform.localRotation);
		}
	}

	public void ScrollToIndex(int newIndex)
	{
		_selectedElementIndex = newIndex;
		_selectedElementIndex = _selectedElementIndex.LoopAround(0, _returnArray.Length - 1);

		transform.GetComponentInParent<CharacterSlot>().SetCharacterInfoText(
			_returnArray[_selectedElementIndex]._characterData.SpecialInfoKey);
		//_displayArray[_selectedElementIndex].transform.SetAsLastSibling();
	}

	public void ScrollLeft()
	{
		ScrollToIndex(--_selectedElementIndex);
	}

	public void ScrollRight()
	{
		ScrollToIndex(++_selectedElementIndex);
	}

	public void Reset()
	{
		_selectedElementIndex = 0;
		transform.localRotation = Quaternion.identity;
		_selectedSkinIndex = 0;
	}
	#endregion

	public void Generate()
	{
		PlayerController[] AvailablePlayers = new PlayerController[0];
		DynamicConfig.Instance.GetConfigs(ref AvailablePlayers);
		GameObject[] tempGenerationSelectableCharacters = new GameObject[AvailablePlayers.Length];
		transform.localPosition = transform.localPosition.ZeroY();

		for (int i = 0; i < tempGenerationSelectableCharacters.Length; i++)
		{
			tempGenerationSelectableCharacters[i] = Instantiate(AvailablePlayers[i]._characterData.CharacterSelectModel.gameObject) as GameObject;
			tempGenerationSelectableCharacters[i].GetComponentInChildren<Animator>().SetTriggerAfterInit("Selection");
			//tempGenerationSelectableCharacters[i].AddComponent<NetworkIdentity>();
			//NetworkServer.Spawn(tempGenerationSelectableCharacters[i]);
		}

		_wheelRadius = Mathf.Abs(transform.parent.localPosition.z * (1 / transform.parent.localScale.z));
		Internal_Generate(tempGenerationSelectableCharacters, AvailablePlayers);

		ScrollToIndex(_playerRef.GetComponent<Player>().CharacterUsedIndex);
		ChangeCharacterSkin(_playerRef.GetComponent<Player>().SkinNumber);
		SetAnimBool("IsSelected", _playerRef.GetComponent<Player>().isReady);
		MenuManager.Instance._characterSlotsContainerRef.SlotsAvailable[playerNumber - 1].SelectPedestal(_playerRef.GetComponent<Player>().isReady);

		for (int i = 0; i < _displayArray.Length; i++)
		{
			_displayArray[i].GetComponentInChildren<CharacterModel>().SetOutlineColor(GameManager.Instance.PlayerColors[playerNumber - 1].SetAlpha(0), true);
			_displayArray[i].GetComponentInChildren<SetRenderQueue>().SetCutOff(0);
		}

		if (WheelsRef.ContainsKey(playerNumber))
			WheelsRef.Remove(playerNumber);

		WheelsRef.Add(playerNumber, this);
	}
	[ClientRpc]
	private void RpcChangeCharacterSkinPrecise(int skinIndex, int characterIndex)
	{
		if(characterIndex == -1)
			_displayArray[_selectedElementIndex].GetComponent<CharacterModel>().Reskin(skinIndex);
		else
			_displayArray[characterIndex].GetComponent<CharacterModel>().Reskin(skinIndex);
	}

	public void ChangeCharacterSkin(int skinIndex)
	{
		_displayArray[_selectedElementIndex].GetComponent<CharacterModel>().Reskin(skinIndex);
		_selectedSkinIndex = skinIndex;
	}

	[Command]
	public void CmdScrollToIndex(int newIndex)
	{
		_selectedElementIndex = newIndex;
		if(_playerRef != null)
			_playerRef.GetComponent<Player>().CmdSetPlayerCharacter(_selectedElementIndex, _selectedSkinIndex);
	}

	[Command]
	public void CmdChangeCharacterSkin(int newIndex)
	{
		_selectedSkinIndex = newIndex;
		if (_playerRef != null)
			_playerRef.GetComponent<Player>().CmdSetPlayerCharacter(_selectedElementIndex, _selectedSkinIndex);
	}

	[Command]
	public void CmdChangeCharacterSkinPrecise(int skinIndex, int characterIndex)
	{
		RpcChangeCharacterSkinPrecise(skinIndex, characterIndex);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		playerNumber = _playerRef.GetComponent<Player>().PlayerNumber;

		transform.SetParent(MenuManager.Instance._characterSlotsContainerRef.SlotsAvailable[playerNumber - 1].WheelSlot, false);

		MenuManager.Instance._characterSlotsContainerRef.SlotsAvailable[playerNumber - 1].OpenSlot(this);

		MenuManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine(WaitForActive()); // delayed to prevent setTrigger not firing
	}

	IEnumerator WaitForActive()
	{
		yield return new WaitUntil(() => gameObject.activeInHierarchy);
		Generate();
	}

	public void SetAnimTrigger(string triggerName)
	{
		_displayArray[_selectedElementIndex].GetComponentInChildren<Animator>().SetTriggerAfterInit(triggerName);
	}

	public void SetAnimBool(string targetName, bool active)
	{
		_displayArray[_selectedElementIndex].GetComponentInChildren<Animator>().SetBoolAfterInit(targetName, active);
	}

	public override void OnNetworkDestroy()
	{
		WheelsRef.Remove(playerNumber);
		base.OnNetworkDestroy();
	}

	void OnDestroy()
	{
		MenuManager.Instance._characterSlotsContainerRef.SlotsAvailable[playerNumber - 1].SelectPedestal(false);

		if (Player.LocalPlayer == null) // If client Disconnect
			WheelsRef.Clear();
	}

	void OnEnable()
	{
		if (_displayArray == null || _playerRef == null)
			return;
		for (int i = 0; i < _displayArray.Length; i++)
		{
			_displayArray[i].GetComponentInChildren<Animator>().SetTriggerAfterInit("Selection");
		}
		SetAnimBool("IsSelected", _playerRef.GetComponent<Player>().isReady);

	}
}
