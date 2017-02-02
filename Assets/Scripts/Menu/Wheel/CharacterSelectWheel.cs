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
	public bool isGenerated = false;

	[HideInInspector]
	[SyncVar]
	public GameObject _playerRef;

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
			ElementGenerate(elementsToDisplay[i], Quaternion.AngleAxis(-_rotationBetweenElements * i, Vector3.up) * Vector3.back * _wheelRadius);
			//elementsToDisplay[i].transform.RotateAround(transform.position, transform.up, -_rotationBetweenElements * i);
		}

		isGenerated = true;
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
		_selectedElementIndex = _selectedElementIndex.LoopAround(0, _displayArray.Length - 1);
		GetComponentInParent<CharacterSlot>().SetCharacterInfoText(_returnArray[_selectedElementIndex]._characterData.SpecialInfoKey, _returnArray[_selectedElementIndex]._characterData.SpeedInfoKey);
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

		for (int i = 0; i < tempGenerationSelectableCharacters.Length; i++)
		{
			tempGenerationSelectableCharacters[i] = Instantiate(AvailablePlayers[i]._characterData.CharacterSelectModel.gameObject) as GameObject;
			tempGenerationSelectableCharacters[i].transform.localScale = transform.parent.localScale * 1.8f;
			tempGenerationSelectableCharacters[i].GetComponentInChildren<Animator>().SetTrigger("Selection");
			//tempGenerationSelectableCharacters[i].AddComponent<NetworkIdentity>();
			//NetworkServer.Spawn(tempGenerationSelectableCharacters[i]);
		}

		_wheelRadius = Mathf.Abs(transform.parent.localPosition.z * 2.5f);
		Internal_Generate(tempGenerationSelectableCharacters, AvailablePlayers);
		_selectedElementIndex = _playerRef.GetComponent<Player>().CharacterUsedIndex;
		_selectedSkinIndex = _playerRef.GetComponent<Player>().SkinNumber;

		if (WheelsRef.ContainsKey(_playerRef.GetComponent<Player>().PlayerNumber))
			WheelsRef.Remove(_playerRef.GetComponent<Player>().PlayerNumber);

		WheelsRef.Add(_playerRef.GetComponent<Player>().PlayerNumber, this);
	}

	public void ChangeCharacterSkinPrecise(int skinIndex, int characterIndex)
	{
		if(characterIndex == -1)
			_displayArray[_selectedElementIndex].GetComponent<CharacterModel>().Reskin(skinIndex);
		else
			_displayArray[characterIndex].GetComponent<CharacterModel>().Reskin(skinIndex);

		_selectedSkinIndex = skinIndex;
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
		if(_playerRef != null)
			_playerRef.GetComponent<Player>().CmdSetPlayerCharacter(_selectedElementIndex, _selectedSkinIndex);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		transform.SetParent(MenuManager.Instance._characterSlotsContainerRef.SlotsAvailable[_playerRef.GetComponent<Player>().PlayerNumber - 1].WheelSlot, false);

		Generate();
		MenuManager.Instance._characterSlotsContainerRef.SlotsAvailable[_playerRef.GetComponent<Player>().PlayerNumber - 1].OpenSlot(this);
	}

	public void SetAnimTrigger(string triggerName)
	{
		_displayArray[_selectedElementIndex].GetComponentInChildren<Animator>().SetTrigger(triggerName);
	}

	public override void OnNetworkDestroy()
	{
		Destroy(gameObject);
		base.OnNetworkDestroy();
	}
}
