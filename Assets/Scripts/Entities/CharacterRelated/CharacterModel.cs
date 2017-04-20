using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class GoArray
{
	[SerializeField]
	private GameObject[] array;

	public int Length
	{
		get { return array.Length; }
	}

	public GameObject this[int i]
	{
		get { return array[i]; }
		set { array[i] = value; }
	}
}

[Serializable]
public struct ReskinPair
{
	public Renderer[] TargetMeshes;
	public Material[] TargetMaterials;
}

public class CharacterModel : MonoBehaviour
{
	[SerializeField]
	private ReskinPair[] _skinArray;
	public ReskinPair[] SkinArray
	{
		get
		{
			return _skinArray;
		}
	}

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

	[HideInInspector]
	public List<Material> MaterialsInUse = new List<Material>();
	private Color OutlineColorInUse = Color.red;
	private float OutlineWidthInUse = 0.002f;
	private int skinIndexInUse = 0;
	[HideInInspector]
	public Texture AmbientRampInUse;
	[Space]
	public GoArray[] SkinEffectObjects;

	void Start()
	{
		for (int i = 0; i < SkinEffectObjects.Length; i++)
		{
			for (int j = 0; j < SkinEffectObjects[i].Length; j++)
			{
				SkinEffectObjects[i][j].SetActive(i == skinIndexInUse);
			}
		}
	}

	public void Reskin(int skinNumber)
	{
		if (_skinArray.Length <= skinNumber)
		{
			Debug.Log("No skinNumber found in CharacterModel => "+gameObject.name);
			return;
		}



		if (SkinEffectObjects.Length > skinNumber)
		{
			for (int i = 0; i < SkinEffectObjects[skinIndexInUse].Length; i++)
			{
				SkinEffectObjects[skinIndexInUse][i].SetActive(false);
			}

			for (int j = 0; j < SkinEffectObjects[skinNumber].Length; j++)
			{
				SkinEffectObjects[skinNumber][j].SetActive(true);
			}

			GetComponentInChildren<AnimationToolkit>().InitParticles();
		}

		skinIndexInUse = skinNumber;

		MaterialsInUse.Clear();

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
		SetOutlineColor(newColor, false, OutlineWidthInUse);
	}

	public void SetOutlineColor(Color newColor, bool forceMaterialCheck, float outlineWidth = 0.004f)
	{
		OutlineColorInUse = newColor;
		OutlineWidthInUse = outlineWidth;
		if (forceMaterialCheck)
		{
			Reskin(skinIndexInUse);
			return; //will relaunch SetOutlineColor() at the end of reskin
		}

		for (int i = 0; i < MaterialsInUse.Count; i++)
		{
			MaterialsInUse[i].SetColor("_OutlineColor", newColor);
			MaterialsInUse[i].SetFloat("_Outline", outlineWidth);
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

	public void SetAmbientRampForced(Texture newTexture)
	{
		AmbientRampInUse = newTexture;

		for (int i = 0; i < MaterialsInUse.Count; i++)
		{
			MaterialsInUse[i].SetTexture("_Ramp", newTexture);
		}
	}
}
