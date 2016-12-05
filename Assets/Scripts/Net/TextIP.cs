using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextIP : MonoBehaviour {

	void Start ()
	{
		GetComponent<InputField>().text = "Could not connect to the internet";
		ServerManager.Instance.OnExternalIpRetrieved.AddListener(OnGetIP);

	}

	void OnGetIP(string ip)
	{
		GetComponent<InputField>().text = ServerManager.Instance.ExternalIp/* + ":" + ServerManager.singleton.networkPort*/;
	}
}
