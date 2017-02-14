using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class BaseOptionField : MonoBehaviour
{
	public static AGameRules TargetRuleSet;

	//[HideInInspector]
	//private BaseRule<object> TargetRule;
	[HideInInspector]
	public string RuleFieldName;
	public Text Description;
	public Text DisplayedValue;
	public GameObject IncreaseButton;
	public GameObject DecreaseButton;

	public virtual void OnDecrease() { DecreaseButton.GetComponent<GrowAndFade>().Activate(); }
	public virtual void OnIncrease() { IncreaseButton.GetComponent<GrowAndFade>().Activate(); }

	public virtual void SetTargetRule(string newFieldName)
	{
		RuleFieldName = newFieldName; 
		Description.text = GetTargetRule().Label;
	}

	public BaseRule GetTargetRule()
	{
		return (BaseRule) TargetRuleSet.RuleObject.GetType().GetField(RuleFieldName).GetValue(TargetRuleSet.RuleObject);
	}
}
