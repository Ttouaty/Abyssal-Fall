using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
struct ReskinPair
{
	public Renderer[] TargetMeshes;
	public Material[] TargetMaterials;
}

public class CharacterModel : MonoBehaviour
{
	[SerializeField]
	private ReskinPair[] _skinArray;

	[HideInInspector]
	public FacialExpresionModule FEMref
	{
		get
		{
			if (_FEMref == null)
			{
				_FEMref = GetComponent<FacialExpresionModule>();
				if(_FEMref == null) //double checking because fuck off
				{
					_FEMref = gameObject.AddComponent<FacialExpresionModule>();
				}
			}
			return _FEMref;
		}
	}
	private FacialExpresionModule _FEMref;

	public void Reskin(int skinNumber)
	{
		if(_skinArray.Length <= skinNumber)
		{
			Debug.Log("No skinNumber found in CharacterModel => "+gameObject.name);
			return;
		}

		for (int j = 0; j < _skinArray[skinNumber].TargetMaterials.Length; j++)
		{
			List<Material> materialArray = new List<Material>();
			materialArray.Add(_skinArray[skinNumber].TargetMaterials[j]);
			for (int i = j + 1; i < _skinArray[skinNumber].TargetMeshes.Length; i++)
			{
				if (_skinArray[skinNumber].TargetMeshes[i] == null)
				{
					if (_skinArray[skinNumber].TargetMaterials.Length > i)
						materialArray.Add(_skinArray[skinNumber].TargetMaterials[i]);
				}
				else
					break;
			}

			if(_skinArray[skinNumber].TargetMeshes[j] != null)
				_skinArray[skinNumber].TargetMeshes[j].materials = materialArray.ToArray();
		}
	}
}
