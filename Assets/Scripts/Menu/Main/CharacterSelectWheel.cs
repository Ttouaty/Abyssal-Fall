using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct CharacterSelectionData
{
	public PlayerController Controller;
	public int SelectedSkinIndex;

	public CharacterSelectionData(PlayerController newController, int skinIndex)
	{
		Controller = newController;
		SelectedSkinIndex = skinIndex;
	}
}

public class CharacterSelectWheel : MonoBehaviour
{
	public static float _wheelRadius = 2;
	private static float _alphaThresholdAngleMin = 20;
	private static float _alphaThresholdAngleMax = 180;

	private SelectableCharacter[] _availableCharacters;
	private int _selectedCharacterIndex = 0;
	private int _selectedSkinIndex = 0;

	public List<SpriteRenderer> _characterArtworkList = new List<SpriteRenderer>();
	private float _rotationBetweenArtworks;
	private CharacterSelectionData _selectedCharacterData = new CharacterSelectionData();
	private bool _generated = false;

	public CharacterSelectionData SelectedPlayerController
	{
		get
		{
			return _selectedCharacterData;
		}
	}

	//public Transform ActiveCharacterArtwork
	//{
	//	get
	//	{
	//		return transform.GetChild(_selectedCharacterIndex).GetChild(0);
	//	}
	//}

	void Start()
	{
	}

	Color tempColor;
	float artworkAngle;

	void Update()
	{
		transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, _selectedCharacterIndex * _rotationBetweenArtworks, 0), 0.15f);
		
		for (int i = 0; i < _characterArtworkList.Count; ++i)
		{
			tempColor = _characterArtworkList[i].color;
			artworkAngle = Vector3.Angle(-Camera.main.transform.forward, (_characterArtworkList[i].transform.position - transform.position));

			Debug.DrawLine(transform.position, _characterArtworkList[i].transform.position, Color.green);

			if (artworkAngle < _alphaThresholdAngleMin)
				tempColor.a = 1;
			else
				tempColor.a = (_alphaThresholdAngleMax - artworkAngle) / _alphaThresholdAngleMax;

			_characterArtworkList[i].color = Color.Lerp(_characterArtworkList[i].color, tempColor, 0.1f);
		}
		RotateArtworksFacingCam();
	}


	public void Generate(SelectableCharacter[] availableCharacters)
	{
		_availableCharacters = availableCharacters;
		_rotationBetweenArtworks = 360 / _availableCharacters.Length;
		if (!_generated)
		{ 
			for (int i = 0; i < _availableCharacters.Length; i++)
			{
				GenerateCharacter(i);
			}
		}
		_generated = true;
		_selectedCharacterData.SelectedSkinIndex = _selectedSkinIndex;
		_selectedCharacterData.Controller = _availableCharacters[_selectedCharacterIndex].CharacterRef;
	}

	public void RotateArtworksFacingCam()
	{
		for (int i = 0; i < _characterArtworkList.Count; ++i)
		{
			_characterArtworkList[i].transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
		}
	}

	void GenerateCharacter(int charaIndex)
	{
		/*
		 Faire l'anim de change skin
		 
		 */
		GameObject artwork = new GameObject();
		artwork.name = "character_" + _availableCharacters[charaIndex].CharacterRef._characterData.IngameName;
		artwork.transform.parent = transform;
		artwork.transform.position = transform.position - transform.forward * _wheelRadius;
		artwork.transform.localRotation = Quaternion.identity;
		artwork.AddComponent<SpriteRenderer>().sprite = _availableCharacters[charaIndex].ArtWorks[0];

		tempColor = artwork.GetComponent<SpriteRenderer>().color;
		tempColor.a = 0;
		artwork.GetComponent<SpriteRenderer>().color = tempColor;

		artwork.transform.RotateAround(transform.position, transform.up, -_rotationBetweenArtworks * charaIndex);
		artwork.name += "_ " + charaIndex;

		_characterArtworkList.Add(artwork.GetComponent<SpriteRenderer>());

		//artworkParent.SetActive(true);
	}

	public void ScrollToIndex(int newIndex)
	{
		_selectedCharacterIndex = newIndex;
		_selectedCharacterData.Controller = _availableCharacters[_selectedCharacterIndex].CharacterRef;
	}

	public void ChangeActiveCharacterSkin(Sprite newSkin)
	{
		_characterArtworkList[_selectedCharacterIndex].sprite = newSkin;
		_selectedCharacterData.SelectedSkinIndex = _selectedSkinIndex;

	}
}
