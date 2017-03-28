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
				_FEMref = GetComponentInChildren<FacialExpresionModule>();
				if(_FEMref == null) //double checking because fuck off
				{

					Debug.LogWarning("Adding default FEM for character model => "+name);
					_FEMref = gameObject.AddComponent<FacialExpresionModule>();
				}
			}
			return _FEMref;
		}
	}
	private FacialExpresionModule _FEMref;

	public List<Material> MaterialsInUse = new List<Material>();
	private Color OutlineColorInUse = Color.red;
	private int skinIndexInUse = 0;
	private Texture AmbientRampInUse;

	public void Reskin(int skinNumber)
	{
		if (_skinArray.Length <= skinNumber)
		{
			Debug.Log("No skinNumber found in CharacterModel => "+gameObject.name);
			return;
		}

		skinIndexInUse = skinNumber;

		MaterialsInUse.Clear();

		Debug.Log("reskin");
		for (int j = 0; j < _skinArray[skinNumber].TargetMaterials.Length; j++)
		{
			List<Material> materialArray = new List<Material>();
			materialArray.Add(_skinArray[skinNumber].TargetMaterials[j]);
			if (!MaterialsInUse.Contains(_skinArray[skinNumber].TargetMaterials[j]))
				MaterialsInUse.Add(_skinArray[skinNumber].TargetMaterials[j]);
			for (int i = j + 1; i < _skinArray[skinNumber].TargetMeshes.Length; i++)
			{
				if (_skinArray[skinNumber].TargetMeshes[i] == null)
				{
					if (_skinArray[skinNumber].TargetMaterials.Length > i)
					{
						materialArray.Add(_skinArray[skinNumber].TargetMaterials[i]);
						if (!MaterialsInUse.Contains(_skinArray[skinNumber].TargetMaterials[i]))
							MaterialsInUse.Add(_skinArray[skinNumber].TargetMaterials[i]);
					}
				}
				else
					break;
			}

			if(_skinArray[skinNumber].TargetMeshes[j] != null)
				_skinArray[skinNumber].TargetMeshes[j].materials = materialArray.ToArray();

			SetOutlineColor(OutlineColorInUse);
			SetAmbientRamp(AmbientRampInUse);
		}
	}

	public void SetOutlineColor(Color newColor)
	{
		SetOutlineColor(newColor, false);
	}

	public void SetOutlineColor(Color newColor, bool forceMaterialCheck)
	{
		OutlineColorInUse = newColor;
		if (forceMaterialCheck)
		{
			Reskin(skinIndexInUse);
			return; //will relaunch SetOutlineColor() at the end of reskin
		}

		for (int i = 0; i < MaterialsInUse.Count; i++)
		{
			MaterialsInUse[i].SetColor("_OutlineColor", newColor);
		}

	}

	public void SetAmbientRamp(Texture newTexture)
	{
		if (newTexture == null)
			return;
		AmbientRampInUse = newTexture;

		for (int i = 0; i < MaterialsInUse.Count; i++)
		{
			MaterialsInUse[i].SetTexture("_Ramp", newTexture);
		}
	}
}
