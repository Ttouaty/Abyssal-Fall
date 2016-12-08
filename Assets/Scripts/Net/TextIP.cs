using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextIP : MonoBehaviour {

	void Start ()
	{
		GetComponent<InputField>().text = ServerManager.Instance.GameId.ToString()/* + ":" + ServerManager.singleton.networkPort*/;
	}
}
