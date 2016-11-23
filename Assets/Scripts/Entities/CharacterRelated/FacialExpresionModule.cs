using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public struct FacialExpressionData
{
	public string name;
	public Vector2 Offset;
}

public class FacialExpresionModule : MonoBehaviour
{
	public Material ExpressionMaterialModel;
	public Renderer ExpressionTarget;
	[Tooltip("Leave empty for default Starting expression")]
	public string StartingExpression = "Neutral";

	public FacialExpressionData[] AvailableExpressions;
	private Material _expressionMaterialInstance;
	private Dictionary<string, Vector2> _expressionDictionnary = new Dictionary<string, Vector2>();
	private bool _generated = false;

	void Awake()
	{
		if (ExpressionMaterialModel == null)
		{
			Debug.LogWarning("No ExpressionMaterialModel set for expression module in " + gameObject.name);
			enabled = false;
			return;
		}

		Generate();
		if(StartingExpression.Length != 0)
			SetExpression(StartingExpression);
	}

	public void SetExpression(string expressionName)
	{
		if (!enabled)
			return;
		if (!_generated)
		{
			Generate();
		}

		if (!_expressionDictionnary.ContainsKey(expressionName))
		{
			Debug.LogWarning("Expression: \"" + expressionName + "\" not found on object " + gameObject.name);
			return;
		}
		else
			ExpressionTarget.materials[1].mainTextureOffset = _expressionDictionnary[expressionName];
	}


	void Generate()
	{
		for (int i = 0; i < AvailableExpressions.Length; i++)
		{
			_expressionDictionnary[AvailableExpressions[i].name] = AvailableExpressions[i].Offset;
		}

		_expressionMaterialInstance = new Material(ExpressionMaterialModel);
		ExpressionTarget.materials[1] = _expressionMaterialInstance;

		_generated = true;
	}
}
