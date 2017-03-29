using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public struct FacialExpressionData
{
	public string name;
	public Material[] mat;
}

public class FacialExpresionModule : MonoBehaviour
{
	public FacialExpressionData[] ExpressionArray = new FacialExpressionData[0];
	public Renderer ExpressionTarget;
	public int TargetMaterialIndex = 0;
	public string DefaultExpression = "neutral";

	private Dictionary<string, Material[]> _expressionDictionnary = new Dictionary<string, Material[]>();
	private bool _generated = false;
	private string ActiveExpression;
	void Awake()
	{
		ActiveExpression = DefaultExpression;
		Generate();

		//SetExpression(DefaultExpression);
	}

	public void SetExpressionPrecise(string expressionName, int skinNumber)
	{
		if (!enabled || ExpressionTarget == null || ExpressionArray == null)
			return;
		if (!_generated)
		{
			Generate();
		}

		expressionName = expressionName.ToLower();

		if (!_expressionDictionnary.ContainsKey(expressionName))
		{
			Debug.LogWarning("Expression: \"" + expressionName + "\" not found on object " + gameObject.name);
			return;
		}
		else if(ExpressionTarget.materials.Length > TargetMaterialIndex)
		{
			if (skinNumber == -1)
			{
				for (int i = 0; i < _expressionDictionnary[ActiveExpression].Length; i++)
				{
					if(_expressionDictionnary[ActiveExpression][i].mainTexture == ExpressionTarget.materials[TargetMaterialIndex].mainTexture)
					{
						skinNumber = i;
						break;
					}
				}
			}

			Material[] tempMats = ExpressionTarget.materials;

			//Debug.Log("switching to expression => "+expressionName +" with skin number => "+skinNumber);
			if (tempMats[TargetMaterialIndex].mainTexture == _expressionDictionnary[expressionName][skinNumber].mainTexture)
				return;

			_expressionDictionnary[expressionName][skinNumber].SetTexture("_Ramp", GetComponentInParent<CharacterModel>().AmbientRampInUse);

			tempMats[TargetMaterialIndex] = _expressionDictionnary[expressionName][skinNumber];

			ExpressionTarget.materials = tempMats;
		}
		else
		{
			Debug.LogWarning("TargetMaterialIndex is out of bound (ExpressionTarget.materials.Length)");
			return;
		}
		ActiveExpression = expressionName;
	}

	public void SetExpression(string expressionName)
	{
		SetExpressionPrecise(expressionName, -1);
	}

	void Generate()
	{
		for (int i = 0; i < ExpressionArray.Length; i++)
		{
			if(ExpressionArray[i].name.Length != 0)
				_expressionDictionnary[ExpressionArray[i].name] = ExpressionArray[i].mat;
		}

		_generated = true;
	}
}
