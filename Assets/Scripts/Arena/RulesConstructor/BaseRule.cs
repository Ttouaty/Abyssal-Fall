﻿using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct RuleValuePair
{
	public BoolRule Rule;
	public bool TargetValue;
}

public class BaseRule : ScriptableObject
{
	[SerializeField]
	protected string[] Values;
	[HideInInspector]
	public int _valueIndex = 0;
	[SerializeField]
	private int _defaultValue = 0;
	public int DefaultValue { get { return _defaultValue; } }
	[Space]
	public string Label; // will be used with key translation
	public bool UserCanModify = true; // is the rule displayed in optionSelector


	public RuleValuePair[] ParentRules;

	public bool CanBeDisplayed
	{
		get
		{
			for (int i = 0; i < ParentRules.Length; i++)
			{
				if (ParentRules[i].Rule != ParentRules[i].TargetValue || !ParentRules[i].Rule.CanBeDisplayed)
					return false; // If any ParentRule is not right return false;
			}

			return true;
		}
	}

	protected void OnEnable() { _valueIndex = _defaultValue; }
	public virtual string Value
	{
		get
		{
			if (Values.Length < _valueIndex)
			{
				Debug.Log("_valueIndex is oustide of Values.Length in => " + name);
				return "";
			}
			return Values[_valueIndex];
		}
	}

	public virtual int ValuesLength
	{
		get
		{
			return Values.Length;
		}
	}


	public static implicit operator bool(BaseRule self)
	{
		return Convert.ToBoolean(self.Value);
	}

	public void SetToNextValue()
	{
		_valueIndex = (++_valueIndex).LoopAround(0, Values.Length - 1);
	}

	public void SetToPreviousValue()
	{
		_valueIndex = (--_valueIndex).LoopAround(0, Values.Length - 1);
	}

	public void SetValueIndex(int newValue)
	{
		if (newValue.LoopAround(0, Values.Length - 1) != newValue)
		{
			Debug.LogWarning("ValueIndex set is outside of values array:\nindex: " + newValue + " / ValuesLength: " + Values.Length);
			return;
		}
		_valueIndex = newValue;
	}
}
