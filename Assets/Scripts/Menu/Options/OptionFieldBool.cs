using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class OptionFieldBool : BaseOptionField
{
	[Space]
	public Image TrueImage;
	public Image FalseImage;

	protected void Start()
	{
		ToggleValue();
	}

	public override void ResetToDefault()
	{
		ToggleValue();
	}

	public override void OnDecrease()
	{
		base.OnDecrease();
		ToggleValue();
	}

	public override void OnIncrease()
	{
		base.OnIncrease();
		ToggleValue();
	}

	protected void ToggleValue()
	{
		if(GetTargetRule() == null)
		{
			TrueImage.gameObject.SetActive(false);
			FalseImage.gameObject.SetActive(true);
			return;
		}

		TrueImage.gameObject.SetActive(GetTargetRule());
		FalseImage.gameObject.SetActive(!GetTargetRule());

		if(GetTargetRule())
			TrueImage.GetComponent<GrowAndFade>().Activate();
		else
			FalseImage.GetComponent<GrowAndFade>().Activate();
	}
}