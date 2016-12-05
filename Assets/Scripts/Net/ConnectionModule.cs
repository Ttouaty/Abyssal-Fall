using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections;

public class ConnectionModule : MonoBehaviour
{
	public InputField TargetIPField;

	public UnityEventString OnFailedConnection;
	public UnityEvent OnSuccess;

	void Update()
	{
		if(NetworkClient.active)
		{
			Debug.Log("network Client is active !");
		}
	}

	public void Connect()
	{
		if(TargetIPField == null)
		{
			Debug.LogWarning("No TargetIPField found, Use Connect(string IPandPort); if you want to connect to a specified game.");
			OnFailedConnection.Invoke("No TargetIPField found, Use Connect(string IPandPort); if you want to connect to a specified game.");
			return;
		}

		Connect(TargetIPField.text);
	}

	public void Connect(string IPandPort)
	{
		string[] fragmentIp = IPandPort.Split(':');
		if(fragmentIp.Length < 2)
		{
			Debug.LogWarning("could not divide Ip and port for IP string: "+IPandPort);
			OnFailedConnection.Invoke("could not divide Ip and port for IP string: " + IPandPort);
		}

		Network.Connect(fragmentIp[0], Convert.ToInt32(fragmentIp[1]));
	}

	void OnConnectedToServer()
	{
		OnSuccess.Invoke();
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		OnFailedConnection.Invoke("Could not connect to server, Maybe the port 1358 was not open on the host router.");
	}
}

[Serializable]
public class UnityEventString: UnityEvent<string> { }