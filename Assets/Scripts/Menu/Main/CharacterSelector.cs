﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

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
	[HideInInspector]
	public PlayerController[] _availableCharacters;

	void Awake()
	{
		DynamicConfig.Instance.GetConfigs(ref _availableCharacters);
	}

	void Start()
	{
		SlotsAvailable = GetComponentsInChildren<CharacterSlot>();
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
