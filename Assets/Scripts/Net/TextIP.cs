using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextIP : MonoBehaviour {

	void Awake ()
	{
		GetComponent<InputField>().text = ServerManager.Instance.ExternalIp + ":" + ServerManager.singleton.networkPort;

		if(ServerManager.Instance.ExternalIp.Length == 0)
		{
			GetComponent<InputField>().text = "Could not connect to the internet";
		}
	}
}
