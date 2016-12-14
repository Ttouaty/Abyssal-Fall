using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

[Serializable]
public struct SelectableCharacter
{
	public PlayerController CharacterRef;
	public Sprite[] ArtWorks;
	public Image InfoImage;//Unused for now
}

[Flags]
public enum OpenSlots
{
	None = 0,
	One = 1,
	Two = 2,
	Three = 4,
	Four = 8
};

public class CharacterSelector : MonoBehaviour
{
	public ParticleSystem OnCharacterSelectedParticles;
	[Space]
	[HideInInspector]
	public CharacterSlot[] SlotsAvailable;
	public SelectableCharacter[] _availableCharacters;

	void Start()
	{
		SlotsAvailable = GetComponentsInChildren<CharacterSlot>();
	}

	public void OpenTargetSlot(int slotNumber, Player player)
	{
		SlotsAvailable[slotNumber].OpenSlot(slotNumber, player);
	}


	public void CloseTargetSlot(int slotNumber)
	{
		if (SlotsAvailable[slotNumber].Selected)
			SlotsAvailable[slotNumber].CancelCharacterSelection();
		if (SlotsAvailable[slotNumber].Open)
			SlotsAvailable[slotNumber].CloseSlot();
	}

	public void CancelAllSelections(bool needClose = true)
	{
		for (int i = 0; i < SlotsAvailable.Length; ++i)
		{
			if (SlotsAvailable[i].Selected)
				SlotsAvailable[i].CancelCharacterSelection();
			if (SlotsAvailable[i].Open && needClose)
				SlotsAvailable[i].CloseSlot();

		}
	}
}
