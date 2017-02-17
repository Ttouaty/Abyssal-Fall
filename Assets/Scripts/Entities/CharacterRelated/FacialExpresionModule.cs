using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public struct FacialExpressionData
{
	public string name;
	public Material[] mat;
}

public class FacialExpresionModule : MonoBehaviour
{
	public FacialExpressionData[] ExpressionArray;
	public Renderer ExpressionTarget;
	public int TargetMaterialIndex = 0;
	public string DefaultExpression = "Neutral";

	private Dictionary<string, Material[]> _expressionDictionnary = new Dictionary<string, Material[]>();
	private bool _generated = false;

	void Awake()
	{
		Generate();

		//SetExpression(DefaultExpression);
	}

	public void SetExpression(string expressionName, int skinNumber)
	{
		if (!enabled || ExpressionTarget == null)
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
			ExpressionTarget.materials[TargetMaterialIndex] = _expressionDictionnary[expressionName][skinNumber];
		else
		{
			Debug.LogWarning("TargetMaterialIndex is out of bound (ExpressionTarget.materials.Length)");
			return;
		}
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
