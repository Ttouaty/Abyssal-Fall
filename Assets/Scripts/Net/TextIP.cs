using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextIP : MonoBehaviour {

	public void ReGenerate()
	{
		GetComponent<InputField>().text = ServerManager.Instance.GameId.ToString();
	}

	public void SetText(string newText)
	{
		GetComponent<InputField>().text = newText;
	}
}
