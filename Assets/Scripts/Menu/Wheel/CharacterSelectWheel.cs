﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterSelectWheel : MenuWheel<PlayerController>
{
	public int _selectedSkinIndex = 0;

	public void Generate(PlayerController[] elementsToAdd)
	{
		GameObject[] tempGenerationSelectableCharacters = new GameObject[elementsToAdd.Length];

		for (int i = 0; i < tempGenerationSelectableCharacters.Length; i++)
		{
			tempGenerationSelectableCharacters[i] = Instantiate(elementsToAdd[i]._characterData.CharacterModel.gameObject) as GameObject;
			tempGenerationSelectableCharacters[i].transform.localScale = transform.parent.localScale * 1.8f;
		}

		base.Generate(tempGenerationSelectableCharacters, elementsToAdd);
	}

	public Material GetSelectedSkin()
	{
		return GetSelectedElement()._characterData.CharacterMaterials[_selectedSkinIndex];
	}

	public void ChangeCharacterSkin(int skinIndex, int characterIndex = -1)
	{
		if(characterIndex == -1)
			_displayArray[_selectedElementIndex].GetComponent<CharacterModel>().Reskin(_returnArray[_selectedElementIndex]._characterData.CharacterMaterials[skinIndex]);
		else
			_displayArray[characterIndex].GetComponent<CharacterModel>().Reskin(_returnArray[characterIndex]._characterData.CharacterMaterials[skinIndex]);
	}

	public override void Reset()
	{
		base.Reset();
		_selectedSkinIndex = 0;
	}
}
