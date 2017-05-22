using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.UI;

public class ProTipsDisplayer : MonoBehaviour
{

	public List<string> TipsKeys = new List<string>();
	private List<string> _randomizedTips = new List<string>();

	void Start()
	{
		RandomizeTips();
		DisplayNewTip();
	}

	public void DisplayNewTip()
	{
		GetComponentInChildren<Localizator.LocalizedText>().Fragment = _randomizedTips.ShiftRandomElement();
		if (_randomizedTips.Count == 0)
			RandomizeTips();
	}


	void RandomizeTips()
	{
		_randomizedTips = TipsKeys.ToList();
	}
}
