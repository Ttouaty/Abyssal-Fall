using UnityEngine;
using System.Collections;

public class OptionFieldEnum : BaseOptionField
{
	void Update()
	{
		if (GetTargetRule() != null)
		{
			//Debug.Log("Option with label => " + TargetRule.Label + " has value => " + TargetRule.Value);
			DisplayedValue.text = GetTargetRule().Value.ToString();
		}
	}
}
