using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextIP : MonoBehaviour {

	private string _gameId;

	public void ReGenerate()
	{
		_gameId = ServerManager.Instance.GameId.ToString();

		if (OptionPanel.OptionObj.ShowGameId == 1)
			GetComponent<InputField>().text = ServerManager.Instance.GameId.ToString();
		else
			GetComponent<InputField>().text = "Hidden";
	}

	public void SetText(string newText)
	{
		_gameId = ServerManager.Instance.GameId.ToString();

		if (OptionPanel.OptionObj.ShowGameId == 1)
			GetComponent<InputField>().text = newText;
		else
			GetComponent<InputField>().text = "Hidden";

	}

	public void CopyToClipBoard()
	{
		GUIUtility.systemCopyBuffer = _gameId;
	}
}
