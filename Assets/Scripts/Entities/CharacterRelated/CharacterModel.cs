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
	private ReskinPair[] _SkinArray;

	private bool _hasFEM = false;
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
					_hasFEM = false;
					_FEMref = gameObject.AddComponent<FacialExpresionModule>();
				}
			}
			return _FEMref;
		}
	}
	private FacialExpresionModule _FEMref;

	public void Reskin(int skinNumber)
	{
		if(_SkinArray.Length <= skinNumber)
		{
			Debug.Log("No skinNumber found in CharacterModel => "+gameObject.name);
			return;
		}

		for (int j = 0; j < _SkinArray[skinNumber].TargetMeshes.Length; j++)
		{
			List<Material> materialArray = new List<Material>();
			materialArray.Add(_SkinArray[skinNumber].TargetMaterials[j]);
			for (int i = j + 1; i < _SkinArray[skinNumber].TargetMaterials.Length; i++)
			{
				if (_SkinArray[skinNumber].TargetMeshes.Length > i)
				{
					if (_SkinArray[skinNumber].TargetMeshes[i] == null)
						materialArray.Add(_SkinArray[skinNumber].TargetMaterials[i]);
				}
				else
					materialArray.Add(_SkinArray[skinNumber].TargetMaterials[i]);
			}

			if(_SkinArray[skinNumber].TargetMeshes[j] != null)
				_SkinArray[skinNumber].TargetMeshes[j].materials = materialArray.ToArray();
		}
	}
}
