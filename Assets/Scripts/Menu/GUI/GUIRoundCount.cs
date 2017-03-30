using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIRoundCount : MonoBehaviour 
{
	private Localizator.LocalizedText _RoundCountDisplay;
	
	void Awake()
	{
		_RoundCountDisplay = GetComponentInChildren<Localizator.LocalizedText>();
	}

	public void SetRound(int round)
	{
		GetComponent<CanvasGroup>().alpha = 1;
		_RoundCountDisplay.SetText(new KeyValuePair<string, string>("%NUMBER%", round.ToString()));
		Fade();
	}

	public void Fade()
	{
		StartCoroutine(FadeCo());
	}

	IEnumerator FadeCo()
	{
		yield return new WaitUntil(() => gameObject.activeInHierarchy);
		GetComponent<CanvasGroup>().CrossFadeAlpha(0, 3);

	}
}
