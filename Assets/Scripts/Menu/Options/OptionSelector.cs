using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

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

	//private Vector3			_nextButtonPosition = Vector3.zero;
	private AGameRules		_targetRuleSet;

	//private int _selectedFieldIndex;
	private int _selectedChildIndex;
	private BaseOptionField _selectedField;
	private List<BaseOptionField> _allFields = new List<BaseOptionField>();
	void Update()
	{
		int Yoffset = Mathf.Clamp(_selectedChildIndex, ButtonScrollMargin, ((ButtonScrollMargin * 2) > _allFields.Count ? ButtonScrollMargin : (_allFields.Count - ButtonScrollMargin))) - ButtonScrollMargin;

		ButtonContainer.transform.localPosition = Vector3.Lerp(ButtonContainer.transform.localPosition, 
																new Vector3(0, Yoffset * BoolOptionPrefab.GetComponent<RectTransform>().sizeDelta.y), 
																Time.deltaTime * 10);
	}

	public void Generate()
	{
		DynamicConfig.Instance.GetConfig(GameManager.Instance.CurrentGameConfiguration.ModeConfiguration, out _targetRuleSet);
		BaseOptionField.TargetRuleSet = _targetRuleSet;
		_selectedChildIndex = 0;
		GenerateOptionFields();
	}

	void GenerateOptionFields()
	{
		ButtonContainer.DestroyAllChildren();
		_allFields.Clear();
		//_nextButtonPosition.y = 0;

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

		SortFields();
		UpdateRuleCanBeDisplayed();
		SelectFirst();
	}

	public void SortFields()
	{
		List<BaseOptionField> children = new List<BaseOptionField>();
		foreach (Transform child in ButtonContainer) { children.Add(child.GetComponentInChildren<BaseOptionField>(true)); }

		children.Sort((BaseOptionField a, BaseOptionField b) => {
			if(a.GetTargetRule().ParentRules.Length - b.GetTargetRule().ParentRules.Length == 0)
				return a.name.CompareTo(b.name);

			return a.GetTargetRule().ParentRules.Length - b.GetTargetRule().ParentRules.Length;
		});

		for (int i = 0; i < children.Count; i++)
		{
			children[i].transform.SetSiblingIndex(i);
		}
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

		return newFieldGO;
	}

	public void SelectFirst()
	{
		_selectedChildIndex = -1;
		SelectField(GetActiveChild(1));
	}
	public void SelectNext() { SelectField(GetActiveChild(1)); }
	public void SelectPrevious() { SelectField(GetActiveChild(-1)); }

	private BaseOptionField GetActiveChild(int direction = 1)
	{
		for (int i = 0; i < ButtonContainer.childCount; i++)
		{
			_selectedChildIndex = (_selectedChildIndex + direction).LoopAround(0, ButtonContainer.childCount - 1);
			if (ButtonContainer.GetChild(_selectedChildIndex).gameObject.activeInHierarchy)
			{
				return ButtonContainer.GetChild(_selectedChildIndex).GetComponentInChildren<BaseOptionField>();
			}
		}


		Debug.LogError("No active Option field found to select");
		return ButtonContainer.GetChild(0).GetComponentInChildren<BaseOptionField>(true);
	}

	public void IncreaseRule()
	{
		if(_selectedField.GetTargetRule()._valueIndex != _selectedField.GetTargetRule().ValuesLength - 1)
		{
			_selectedField.GetTargetRule().SetToNextValue();
			_selectedField.OnIncrease();
			SoundManager.Instance.PlayOS("UI button Select 2");
			UpdateRuleCanBeDisplayed();
		}
	}

	public void DecreaseRule()
	{
		if (_selectedField.GetTargetRule()._valueIndex != 0)
		{
			_selectedField.GetTargetRule().SetToPreviousValue();
			_selectedField.OnDecrease();
			SoundManager.Instance.PlayOS("UI button Select 2");
			UpdateRuleCanBeDisplayed();
		}
	}

	private void UpdateRuleCanBeDisplayed()
	{
		for (int i = 0; i < _allFields.Count; i++)
		{
			_allFields[i].gameObject.SetActive(_allFields[i].GetTargetRule().CanBeDisplayed);
		}
	}

	private void SelectField(BaseOptionField targetField)
	{
		if(targetField.GetComponentInChildren<Selectable>(true) != null)
		{
			if(_selectedField != null)
				_selectedField.GetComponent<Animator>().SetTrigger("Normal");
			targetField.GetComponent<Animator>().SetTrigger("Highlighted");
			SoundManager.Instance.PlayOS("UI button Change 1");
		}
		_selectedField = targetField;
		//_selectedFieldIndex = _allFields.IndexOf(_selectedField);
	}
}
