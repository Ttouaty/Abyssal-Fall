using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.Networking;

public class OptionSelector : MonoBehaviour
{
	public Sprite NormalSprite;
	public Sprite HighlightSprite;
	[Space]
	public OptionFieldInt	IntOptionPrefab;
	public OptionFieldBool	BoolOptionPrefab;
	public OptionFieldEnum	EnumOptionPrefab;

	[Space]
	public Transform ButtonContainer;
	public int ButtonScrollMargin = 3;

	private Vector3			_nextButtonPosition = Vector3.zero;
	private AGameRules		_targetRuleSet;

	private int _selectedFieldIndex;
	private BaseOptionField _selectedField;
	private List<BaseOptionField> _allFields = new List<BaseOptionField>();

	void Update()
	{

		int Yoffset = Mathf.Clamp(_selectedFieldIndex, ButtonScrollMargin, ((ButtonScrollMargin * 2) > _allFields.Count ? ButtonScrollMargin : (_allFields.Count - ButtonScrollMargin))) - ButtonScrollMargin;

		ButtonContainer.transform.localPosition = Vector3.Lerp(ButtonContainer.transform.localPosition, 
																new Vector3(0, - Yoffset * BoolOptionPrefab.GetComponent<RectTransform>().sizeDelta.y), 
																Time.deltaTime * 10);
	}

	public void Generate()
	{
		DynamicConfig.Instance.GetConfig(GameManager.Instance.CurrentGameConfiguration.ModeConfiguration, out _targetRuleSet);
		BaseOptionField.TargetRuleSet = _targetRuleSet;
		GenerateOptionFields();
	}

	void GenerateOptionFields()
	{
		ButtonContainer.DestroyAllChildren();

		foreach (var prop in _targetRuleSet.RuleObject.GetType().GetFields())
		{
			if (prop.FieldType == typeof(BoolRule))
			{
				if(prop.GetValue(_targetRuleSet.RuleObject) != null)
					if(((BoolRule)prop.GetValue(_targetRuleSet.RuleObject)).UserCanModify)
						CreateOptionBoolField(prop.Name);
			}
			else if (prop.FieldType == typeof(IntRule))
			{
				if(prop.GetValue(_targetRuleSet.RuleObject) != null)
					if(((IntRule)prop.GetValue(_targetRuleSet.RuleObject)).UserCanModify)
						CreateOptionIntField(prop.Name);
			}
			else if(prop.FieldType == typeof(EnumRule))
			{
				if(prop.GetValue(_targetRuleSet.RuleObject) != null)
					if(((EnumRule)prop.GetValue(_targetRuleSet.RuleObject)).UserCanModify)
						CreateOptionEnumField(prop.Name);
			}
		}

		SelectField(_allFields[0]);
	}

	public void CreateOptionIntField(string ruleName)
	{
		GameObject newField = CreateOptionField(IntOptionPrefab.gameObject);
		newField.GetComponent<OptionFieldInt>().SetTargetRule(ruleName);
	}

	public void CreateOptionBoolField(string ruleName)
	{
		GameObject newField = CreateOptionField(BoolOptionPrefab.gameObject);
		newField.GetComponent<OptionFieldBool>().SetTargetRule(ruleName);
	}

	public void CreateOptionEnumField(string ruleName)
	{
		GameObject newField = CreateOptionField(EnumOptionPrefab.gameObject);
		newField.GetComponent<OptionFieldEnum>().SetTargetRule(ruleName);
	}

	private GameObject CreateOptionField(GameObject newField)
	{
		GameObject newFieldGO = (GameObject)Instantiate(newField, ButtonContainer, false);
		_allFields.Add(newFieldGO.GetComponent<BaseOptionField>());
		newFieldGO.transform.localPosition = _nextButtonPosition;

		_nextButtonPosition.y -= newFieldGO.GetComponent<RectTransform>().sizeDelta.y;

		return newFieldGO;
	}

	public void SelectNext() { SelectField(_allFields[(_allFields.IndexOf(_selectedField) + 1).LoopAround(0, _allFields.Count - 1)]); }
	public void SelectPrevious() { SelectField(_allFields[(_allFields.IndexOf(_selectedField) - 1).LoopAround(0, _allFields.Count - 1)]); }

	public void IncreaseRule()
	{
		_selectedField.GetTargetRule().SetToNextValue();
		_selectedField.OnIncrease();
	}

	public void DecreaseRule()
	{
		_selectedField.GetTargetRule().SetToPreviousValue();
		_selectedField.OnDecrease();
	}

	private void SelectField(BaseOptionField targetField)
	{
		if (_selectedField != null)
			_selectedField.gameObject.GetComponent<Image>().sprite = NormalSprite;
		targetField.gameObject.GetComponent<Image>().sprite = HighlightSprite;
		_selectedField = targetField;
		_selectedFieldIndex = _allFields.IndexOf(_selectedField);
	}
}
