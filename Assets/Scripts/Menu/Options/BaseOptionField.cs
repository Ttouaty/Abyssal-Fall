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

	public virtual void OnDecrease()
	{
		DecreaseButton.GetComponent<GrowAndFade>().Activate();
		CheckButtonsMatching();
	}
	public virtual void OnIncrease()
	{
		IncreaseButton.GetComponent<GrowAndFade>().Activate();
		CheckButtonsMatching();
	}

	public virtual void SetTargetRule(string newFieldName)
	{
		RuleFieldName = newFieldName;
		Description.text = GetTargetRule().Label;
		Description.GetComponent<Localizator.LocalizedText>().Fragment = GetTargetRule().Label;
		CheckButtonsMatching();
	}

	public virtual void ResetToDefault()
	{
		Description.text = GetTargetRule().Label;
		Description.GetComponent<Localizator.LocalizedText>().Fragment = GetTargetRule().Label;
		CheckButtonsMatching();
	}

	public void CheckButtonsMatching()
	{
		IncreaseButton.SetActive(GetTargetRule()._valueIndex != GetTargetRule().ValuesLength - 1);
		DecreaseButton.SetActive(GetTargetRule()._valueIndex != 0);
	}

	public BaseRule GetTargetRule()
	{
		return (BaseRule)TargetRuleSet.RuleObject.GetType().GetField(RuleFieldName).GetValue(TargetRuleSet.RuleObject);
	}
}
