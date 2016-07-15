using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

[Serializable]
public struct SelectableCharacter
{
	public PlayerController CharacterRef;
	public Sprite[] ArtWorks;
	public Image InfoImage;//Unused for now
}

public class CharacterSlotsContainer : MonoBehaviour 
{
	private CharacterSlot[] _slotsAvailable;
	public SelectableCharacter[] _availableCharacters;

	void Start()
	{
		_slotsAvailable = GetComponentsInChildren<CharacterSlot>();
	}

	public void OpenNextSlot(int JoyToListen)
	{
		for (int i = 0; i < _slotsAvailable.Length; ++i)
		{
			if (!_slotsAvailable[i].Open)
			{
				_slotsAvailable[i].OpenSlot(i, JoyToListen);
				return;
			}
		}

		Debug.LogWarning("no more slots to open !");
	}
}
