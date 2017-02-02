using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PasteClipboard : MonoBehaviour
{
	public void Paste()
	{
		GetComponent<InputField>().text = GUIUtility.systemCopyBuffer;
	}
}
