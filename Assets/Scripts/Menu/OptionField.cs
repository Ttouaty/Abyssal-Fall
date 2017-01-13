using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OptionField<T> : MonoBehaviour
{
	public BaseRule<T> TargetRule;
	private Text _displayedValue;
	
	void Start()
	{
		_displayedValue = GetComponentInChildren<Text>();

	}

	void Update()
	{
		_displayedValue.text = TargetRule.ToString();
	}
}
