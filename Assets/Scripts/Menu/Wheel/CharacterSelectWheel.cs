using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterSelectWheel : MenuWheel<WheelSelectableCharacter>
{
	public int _selectedSkinIndex = 0;

	public void Generate(SelectableCharacter[] elementsToAdd)
	{
		WheelSelectableCharacter[] tempGenerationSelectableCharacters = new WheelSelectableCharacter[elementsToAdd.Length];

		GameObject tempGO;
		for (int i = 0; i < tempGenerationSelectableCharacters.Length; i++)
		{
			tempGO = new GameObject();
			tempGO.AddComponent<RectTransform>();
			tempGenerationSelectableCharacters[i] = tempGO.AddComponent<WheelSelectableCharacter>();
			tempGenerationSelectableCharacters[i].sprite = elementsToAdd[i].ArtWorks[0];
			tempGenerationSelectableCharacters[i].Controller = elementsToAdd[i].CharacterRef;
		}

		base.Generate(tempGenerationSelectableCharacters);

		for (int i = 0; i < _elementList.Count; i++)
		{
			_elementList[i].gameObject.name = "character_" + elementsToAdd[i].CharacterRef._characterData.IngameName + "_" + i;
		}
	}

	public Material GetSelectedSkin()
	{
		return GetSelectedElement().Controller._characterData.CharacterMaterials[_selectedSkinIndex];
	}

	public void ChangeActiveCharacterSkin(Sprite newSkin, int index)
	{
		_elementList[_selectedElementIndex].sprite = newSkin;
		_selectedSkinIndex = index;
	}

	public override void Reset()
	{
		base.Reset();
		_selectedSkinIndex = 0;
	}
}
