using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GlobalOptionButton : Selectable
{
	public string TargetOptionName;
	[Space]
	public int[] Values;
	public string[] DisplayedStrings;
	public bool Looping = false;

	public Text DisplayedText;
	public GameObject LeftArrow;
	public GameObject RightArrow;


	public UnityEventInt OnValueChange;

	private int activeValueIndex = 0;

	public void Init(int optionIndex)
	{
		activeValueIndex = optionIndex;

		UpdateText();
	}

	public void ChangeValue(int valueToAdd)
	{
		if (Looping)
			activeValueIndex = (activeValueIndex + valueToAdd).LoopAround(0, Values.Length - 1);
		else
			activeValueIndex = Mathf.Clamp(activeValueIndex + valueToAdd, 0, Values.Length - 1);

		UpdateText();
		OnValueChange.Invoke(activeValueIndex);
	}

	private void UpdateText()
	{
		if (DisplayedStrings.Length > activeValueIndex)
			DisplayedText.text = DisplayedStrings[activeValueIndex];
		else
			Debug.LogError("optionIndex out of range in GlobalOptionButton => " + name);

		LeftArrow.SetActive(activeValueIndex != 0 || Looping);
		RightArrow.SetActive(activeValueIndex != Values.Length - 1 || Looping);
	}
}
