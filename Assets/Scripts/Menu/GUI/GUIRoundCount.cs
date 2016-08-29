using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIRoundCount : MonoBehaviour 
{
	private Localizator.LocalizedText _RoundCountDisplay;
	
	void Awake()
	{
		_RoundCountDisplay = GetComponent<Localizator.LocalizedText>();
	}

	public void SetRound(int round)
	{
		_RoundCountDisplay.SetText(new KeyValuePair<string, string>("%NUMBER%", round.ToString()));
	}
}
