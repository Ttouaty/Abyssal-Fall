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

	public void OpenNextSlot(int JoyToListen)
	{
		for (int i = 0; i < SlotsAvailable.Length; ++i)
		{
			if (!SlotsAvailable[i].Open)
			{
				//NetworkServer.Spawn(SlotsAvailable[i].gameObject);
				//SlotsAvailable[i].OpenSlot(i, JoyToListen);
				Debug.Log("open new slot");
				return;
			}
		}

		Debug.LogWarning("no more slots to open !");
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
