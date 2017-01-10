using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterSelectWheel : NetworkBehaviour
{
	public float _wheelRadius = 3;
	public float _rotateSpeed = 0.2f;

	protected float _alphaThresholdAngleMin = 20;
	protected float _alphaThresholdAngleMax = 150;


	protected GameObject[] _displayArray = new GameObject[0];
	protected PlayerController[] _returnArray = new PlayerController[0];

	[SyncVar(hook = "ScrollToIndex")]
	public int _selectedElementIndex = 0;
	[SyncVar(hook = "ChangeCharacterSkin")]
	[HideInInspector]
	public int _selectedSkinIndex = 0;
	protected float _rotationBetweenElements;

	protected Color _tempColor;
	protected float _tempElementAngle;
	public Player ParentPlayer;

	[HideInInspector]
	public bool isGenerated = false;

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

		_selectedElementIndex = 0;
		_rotationBetweenElements = 360 / elementsToDisplay.Length;

		if (_displayArray.Length != _returnArray.Length)
			Debug.LogError("Display and return arrays lengths do not match!\nThis may/will cause crashes on selection.");

		for (int i = 0; i < elementsToDisplay.Length; i++)
		{
			elementsToDisplay[i].transform.SetParent(transform);
			ElementGenerate(elementsToDisplay[i], Quaternion.AngleAxis(-_rotationBetweenElements * i, Vector3.up) * Vector3.back * _wheelRadius);
			//elementsToDisplay[i].transform.RotateAround(transform.position, transform.up, -_rotationBetweenElements * i);
		}

		ScrollToIndex(_selectedElementIndex);
		isGenerated = true;
		Update();
	}

	protected void ElementGenerate(GameObject element, Vector3 localPos)
	{
		//element.transform.localScale = Vector3.one;
		element.transform.localRotation = Quaternion.identity;
		element.transform.localPosition = localPos;
	}

	public PlayerController GetSelectedElement()
	{
		return _returnArray[_selectedElementIndex];
	}

	protected void Update()
	{
		//if (ParentPlayer != null)
		//{
		//	if (ParentPlayer.isLocalPlayer)
		//	{
		transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, _selectedElementIndex * _rotationBetweenElements, 0), _rotateSpeed);
		//}
		//}


		//######## apply alpha to Image elements #########
		RotateElementsFacingCam();
		ApplyAlpha();
	}

	protected void ApplyAlpha()
	{
		Image tempImageRef;
		for (int i = 0; i < _displayArray.Length; ++i)
		{
			tempImageRef = _displayArray[i].GetComponentInChildren<Image>();
			if (tempImageRef == null)
				return;

			_tempColor = tempImageRef.color;
			_tempElementAngle = Vector3.Angle(-Camera.main.transform.forward, (_displayArray[i].transform.position - transform.position));

			Debug.DrawLine(transform.position, _displayArray[i].transform.position, Color.green);

			if (_tempElementAngle < _alphaThresholdAngleMin)
				_tempColor.a = 1;
			else
				_tempColor.a = (_alphaThresholdAngleMax - _tempElementAngle) / _alphaThresholdAngleMax;

			tempImageRef.color = Color.Lerp(tempImageRef.color, _tempColor, _rotateSpeed);
		}
	}

	protected void RotateElementsFacingCam()
	{
		for (int i = 0; i < _displayArray.Length; ++i)
		{
			_displayArray[i].transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
		}
	}

	public void ScrollToIndex(int newIndex)
	{
		_selectedElementIndex = newIndex;
		_selectedElementIndex = _selectedElementIndex.LoopAround(0, _displayArray.Length - 1);
		_displayArray[_selectedElementIndex].transform.SetAsLastSibling();
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

	public void Generate(PlayerController[] elementsToAdd, Player parentPlayer)
	{
		ParentPlayer = parentPlayer;
		GameObject[] tempGenerationSelectableCharacters = new GameObject[elementsToAdd.Length];

		for (int i = 0; i < tempGenerationSelectableCharacters.Length; i++)
		{
			tempGenerationSelectableCharacters[i] = Instantiate(elementsToAdd[i]._characterData.CharacterModel.gameObject) as GameObject;
			tempGenerationSelectableCharacters[i].transform.localScale = transform.parent.localScale * 1.8f;
			//tempGenerationSelectableCharacters[i].AddComponent<NetworkIdentity>();
			//NetworkServer.Spawn(tempGenerationSelectableCharacters[i]);
		}

		_wheelRadius = Mathf.Abs(transform.localPosition.z);

		Internal_Generate(tempGenerationSelectableCharacters, elementsToAdd);
	}

	public Material GetSelectedSkin()
	{
		return GetSelectedElement()._characterData.CharacterMaterials[_selectedSkinIndex];
	}

	public void ChangeCharacterSkinPrecise(int skinIndex, int characterIndex)
	{
		if(characterIndex == -1)
			_displayArray[_selectedElementIndex].GetComponent<CharacterModel>().Reskin(_returnArray[_selectedElementIndex]._characterData.CharacterMaterials[skinIndex]);
		else
			_displayArray[characterIndex].GetComponent<CharacterModel>().Reskin(_returnArray[characterIndex]._characterData.CharacterMaterials[skinIndex]);

		_selectedSkinIndex = skinIndex;
	}

	public void ChangeCharacterSkin(int skinIndex)
	{
		_displayArray[_selectedElementIndex].GetComponent<CharacterModel>().Reskin(_returnArray[_selectedElementIndex]._characterData.CharacterMaterials[skinIndex]);
		_selectedSkinIndex = skinIndex;
	}

	[Command]
	public void CmdScrollToIndex(int newIndex)
	{
		_selectedElementIndex = newIndex;
	}

	[Command]
	public void CmdChangeCharacterSkin(int newIndex)
	{
		_selectedSkinIndex = newIndex;
	}
}
